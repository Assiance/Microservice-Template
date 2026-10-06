#!/usr/bin/env bash
# Agentic feature-loop driver.
#
# Usage:
#   ./harness/run.sh --features-until 4                   # complete features 001-004
#   ./harness/run.sh --features 2,5-7                     # specific features/ranges
#   ./harness/run.sh --features-until 4 --mode autonomous # build on harness/integration:
#                                                         # per-feature PRs merge there on
#                                                         # pass+CI-green; you test the branch
#                                                         # and promote it to main afterwards
#   ./harness/run.sh --features-until 4 --mode gated      # PRs to main, wait for your merge
#   ./harness/run.sh --features-until 4 --wait            # block-and-poll at human gates
#   ./harness/run.sh --features 3 --restart               # nuke and rebuild an escalated/
#                                                         # in-flight feature's branch
#
# State model:
#   - Feature status lives in features/*.md frontmatter and is COMMITTED. The
#     driver commits the terminal 'done' flip on the feature branch BEFORE
#     pushing, so the merged base branch (main in gated mode, the integration
#     branch in autonomous mode) is the source of truth for what's complete.
#     (Previously 'passed'/'done' flips were uncommitted working-tree edits;
#     that broke the post-merge checkout of main and made back-to-back
#     features impossible.)
#   - Verdicts (features/NNN.verdict.json) and gate markers (features/NNN.blocked)
#     are driver-local scratch. Gitignore them (see README).
#   - Re-running is safe: done features skip; a feature with an existing
#     feat/NNN branch RESUMES on that branch (no reset) unless --restart.

set -euo pipefail
export MSYS_NO_PATHCONV=1 MSYS2_ARG_CONV_EXCL='*'


# ---------- config (override via env) ----------
GEN_MODEL="${GEN_MODEL:-claude-sonnet-5}"  # generator: Sonnet 5 — draws the separate Sonnet weekly pool.
                                           # Pinned to the full ID, not the 'sonnet' alias, which resolves
                                           # to the previous-gen Sonnet 4.6 (that's the 'sonnet-4-6' the logs showed).
EVAL_MODEL="${EVAL_MODEL:-claude-opus-5}"  # evaluator: Opus 5 — the current top-tier Opus; judgment is where
                                           # it earns its cost. Pinned to the full ID because the 'opus' alias
                                           # still resolves to the previous-gen Opus 4.6, not 5.
MAX_ROUNDS="${MAX_ROUNDS:-3}"           # implement->evaluate rounds per feature before escalating
GEN_MAX_TURNS="${GEN_MAX_TURNS:-150}"   # runaway-session guards
EVAL_MAX_TURNS="${EVAL_MAX_TURNS:-120}" # evaluate.md's mandate is heavy; 80 truncated evals on fat slices
CLAUDE_RETRIES="${CLAUDE_RETRIES:-3}"   # retries per agent invocation on nonzero exit (transient API/CLI errors)
BASE_BRANCH="${BASE_BRANCH:-main}"
INTEGRATION_BRANCH="${INTEGRATION_BRANCH:-harness/integration}"  # autonomous runs build/merge here, never on main
POLL_SECS="${POLL_SECS:-60}"
MERGE_TIMEOUT_SECS="${MERGE_TIMEOUT_SECS:-0}"  # 0 = wait forever. Autonomous merges additionally fail-fast on red CI.
AGENT_TIMEOUT_SECS="${AGENT_TIMEOUT_SECS:-3600}"  # hard wall-clock cap per agent invocation; a wedged CLI/API call becomes rc=124 and the retry loop re-runs it
CI_REGISTER_TIMEOUT_SECS="${CI_REGISTER_TIMEOUT_SECS:-300}"  # max wait for checks to APPEAR on a PR before failing closed
EVAL_SYSTEM="harness/evaluator-system.md"
EVAL_SETTINGS="harness/eval-settings.json"     # mechanical evaluator boundary — required
MCP_CONFIG="harness/mcp.headless.json"         # ONLY the MCP servers agents actually use (playwright).
                                               # Passed with --strict-mcp-config so a headless `claude -p`
                                               # run ignores every other configured server — notably the
                                               # interactively-authed claude.ai connectors (Gmail/Drive/
                                               # Calendar), whose OAuth handshake can't complete non-
                                               # interactively and wedges the agent at startup (no init
                                               # event, no output) until the wall-clock watchdog fires.
LOG_DIR="harness/logs/$(date +%Y%m%d-%H%M%S)"  # per-run transcripts; read these to tune the evaluator
NOTIFY_CMD="${NOTIFY_CMD:-}"
STOP_FILE="harness/STOP"

MODE="gated"
UNTIL=""
FEATURES=""
WAIT_AT_GATES=false
RESTART=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --features-until) UNTIL="$2"; shift 2 ;;
    --features) FEATURES="$2"; shift 2 ;;
    --mode)     MODE="$2"; shift 2 ;;
    --wait)     WAIT_AT_GATES=true; shift ;;
    --restart)  RESTART=true; shift ;;
    *) echo "Unknown arg: $1" >&2; exit 1 ;;
  esac
done
[[ "$MODE" == "gated" || "$MODE" == "autonomous" ]] || { echo "--mode must be gated|autonomous" >&2; exit 1; }

# Autonomous runs never touch main: features are cut from and merged into the
# integration branch, which the human tests and promotes to PROMOTE_BASE as one
# reviewed PR after the run. Gated mode targets BASE_BRANCH (main) directly —
# the human reviews each feature PR instead.
PROMOTE_BASE="$BASE_BRANCH"
if [[ "$MODE" == "autonomous" ]]; then
  BASE_BRANCH="$INTEGRATION_BRANCH"
fi

# ---------- preflight ----------
for bin in claude gh jq git awk; do
  command -v "$bin" >/dev/null || { echo "Missing required tool: $bin" >&2; exit 1; }
done
[[ -f "$EVAL_SETTINGS" ]] || {
  echo "Missing $EVAL_SETTINGS — refusing to run the evaluator with the generator's permissions." >&2
  echo "That file is the mechanical enforcement of the generator/evaluator separation." >&2
  exit 1
}
[[ -f "$EVAL_SYSTEM" ]] || { echo "Missing $EVAL_SYSTEM" >&2; exit 1; }
[[ -f "$MCP_CONFIG" ]] || {
  echo "Missing $MCP_CONFIG — refusing to run agents without a pinned MCP config." >&2
  echo "Without --strict-mcp-config, interactively-authed connectors (claude.ai) can hang a headless -p run at startup." >&2
  exit 1
}

# ---------- helpers ----------
feature_file() { ls features/"$(printf '%03d' "$1")"-*.md 2>/dev/null | head -n1; }
fm() { # fm <file> <key> -> frontmatter value
  awk -v k="$2" 'f&&$1==k":"{sub(/^[^:]*: */,"");print;exit} /^---$/{f=!f}' "$1"
}
status_of() { fm "$(feature_file "$1")" status; }
deps_of() { fm "$(feature_file "$1")" depends_on | tr -d '[]' | tr ',' ' '; }

selected_features() {
  local all sel=()
  all=$(ls features/[0-9][0-9][0-9]-*.md 2>/dev/null \
        | sed -E 's|features/([0-9]{3}).*|\1|' | sort -n) || true
  for n in $all; do
    local num=$((10#$n))
    if [[ -n "$UNTIL" ]]; then
      (( num <= UNTIL )) && sel+=("$num")
    elif [[ -n "$FEATURES" ]]; then
      for part in ${FEATURES//,/ }; do
        if [[ "$part" == *-* ]]; then
          (( num >= ${part%-*} && num <= ${part#*-} )) && sel+=("$num")
        else
          (( num == part )) && sel+=("$num")
        fi
      done
    else
      sel+=("$num")
    fi
  done
  # ${arr[@]+...} guards empty-array expansion under set -u (bash 3.2 / macOS)
  printf '%s\n' ${sel[@]+"${sel[@]}"}
}

deps_done() {
  for d in $(deps_of "$1"); do
    [[ "$(status_of "$d")" == "done" ]] || { echo "$d"; return 1; }
  done
  return 0
}

set_status() { # set_status <num> <status> — portable (no GNU-only sed -i)
  local f; f=$(feature_file "$1")
  awk -v s="$2" '!done && /^status: /{print "status: " s; done=1; next} {print}' "$f" > "$f.tmp" \
    && mv "$f.tmp" "$f"
}

branch_exists() { git show-ref --verify --quiet "refs/heads/$1"; }
pr_state() { gh pr view "$1" --json state -q .state 2>/dev/null || echo "NONE"; }

notify() { # best-effort, never fails the run
  echo "  [notify] $1"
  [[ -n "$NOTIFY_CMD" ]] && $NOTIFY_CMD "$1" 2>/dev/null || true
}

# Porcelain minus the driver-local scratch files (belt-and-suspenders in case
# they aren't gitignored yet).
tree_dirt() {
  git status --porcelain -- . ':(exclude)features/*.verdict.json' ':(exclude)features/*.blocked' 2>/dev/null
}

require_clean_tree() {
  if [[ -n "$(tree_dirt)" ]]; then
    echo "Working tree is dirty — refusing to start a feature on top of leftover state." >&2
    echo "Inspect with 'git status'; commit, stash, or clean, then re-run." >&2
    exit 1
  fi
}

# ---------- live agent output ----------
# claude -p buffers everything and prints only the final result, so a bare
# `| tee` shows NOTHING for the entire 15-60 min session — a healthy run is
# indistinguishable from a hang. Instead agents run with stream-json output:
# raw events land in <log>.jsonl (the full transcript) while this jq program
# renders one line per event to the console and <log>. Non-JSON lines
# (claude's own stderr) pass through prefixed '!!'.
STREAM_FMT='
def ts: now | localtime | strftime("%H:%M:%S");
def clip(n): tostring | gsub("\\s+"; " ") | .[0:n];
. as $line | (try fromjson catch null) as $e |
if $e == null then "!! \($line)"
elif $e.type == "system" and $e.subtype == "init" then
  "[\(ts)] session start: model=\($e.model // "?")"
elif $e.type == "assistant" then
  ($e.message.content // [])[]
  | if .type == "tool_use" then "[\(ts)] -> \(.name) \(.input | clip(160))"
    elif .type == "text" and (.text | length) > 0 then "[\(ts)]    \(.text | clip(300))"
    else empty end
elif $e.type == "user" then
  ($e.message.content | if type == "array" then .[] else empty end)
  | if .type == "tool_result" and .is_error == true then "[\(ts)] xx \(.content | clip(200))"
    else empty end
elif $e.type == "result" then
  "[\(ts)] == \($e.subtype // "result"): turns=\($e.num_turns // "?") duration=\((($e.duration_ms // 0) / 1000) | floor)s cost=$\($e.total_cost_usd // 0)"
else empty end'

# ---------- reliable agent kill (Windows) ----------
# `kill`, Ctrl+C, and GNU `timeout` all send POSIX signals that a NATIVE
# claude.exe (not an MSYS program) ignores — which is why a wedged agent could
# run for hours past its cap, and why Ctrl+C left orphaned dev servers behind.
# taskkill /F /T kills the agent AND its whole native child tree (dev servers,
# node, browsers) by Windows PID. We scope the search to OUR process group so we
# never touch your other claude sessions (desktop app, VS Code, other
# terminals) — those live in different groups. Non-Windows falls back to a POSIX
# process-group kill.
MAIN_PID=$$
MAIN_PGID=$(ps -W 2>/dev/null | awk -v p=$$ '$1==p{print $3; exit}'); MAIN_PGID=${MAIN_PGID:-$$}
winpid_of() { ps -W 2>/dev/null | awk -v p="$1" '$1==p{print $4; exit}'; }
# Match OUR agent by process GROUP *or* direct parentage. The scoping to
# MAIN_PGID exists so we never touch your other claude sessions (desktop, VS
# Code, other terminals) — but a NATIVE claude.exe launched from the pipeline
# gets its OWN process group (PGID == its own PID, not MAIN_PGID), so the group
# match alone missed the very agent we spawned and the watchdog killed nothing.
# The agent is still a direct child of this script, so also match PPID==MAIN_PID
# (and taskkill /T reaps its whole subtree: dev servers, node, browsers).
hard_kill_agents() {
  command -v taskkill >/dev/null 2>&1 || { kill -TERM "-$MAIN_PGID" 2>/dev/null || true; return; }
  local pid w
  for pid in $(ps -W 2>/dev/null | awk -v g="$MAIN_PGID" -v pp="$MAIN_PID" '($3==g || $2==pp) && /claude/{print $1}'); do
    w=$(winpid_of "$pid"); [[ -n "$w" ]] && taskkill /F /T /PID "$w" >/dev/null 2>&1 || true
  done
}

# ---------- reap orphaned dev servers ----------
# init.sh boots the API on 5000 and Vite on 5173. When an agent tree is
# force-killed (watchdog, Ctrl+C, or a killed round), those servers ORPHAN
# (reparented away from the agent) and keep holding their ports AND an open
# handle on the build output (.dll/.exe on Windows) — so the NEXT round's
# `dotnet build` fails with a file lock. The agent then correctly tries to
# taskkill the stale PID, but a headless `-p` run can't approve a non-allowlisted
# Bash command, so it stalls and ends the round with an uncommitted tree ->
# tripwire fail -> escalate. We reap the port holders OURSELVES at every boundary
# so the agent never has to. hard_kill_agents is claude-scoped and misses these
# (dev servers aren't claude and, once orphaned, aren't in our process group).
DEV_PORTS="${DEV_PORTS:-5000 5173}"
reap_dev_servers() {
  local port pid
  for port in $DEV_PORTS; do
    if command -v taskkill >/dev/null 2>&1; then
      # Windows netstat -ano: cols = Proto LocalAddr ForeignAddr State PID.
      # Match LISTENING rows whose local address ends in :PORT (handles IPv4 and
      # [::1]:PORT), then taskkill /T the owning winpid and its child tree.
      for pid in $(netstat -ano 2>/dev/null \
            | awk -v pat=":$port\$" '$4=="LISTENING" && $2 ~ pat {print $5}' | sort -u); do
        [[ "$pid" =~ ^[0-9]+$ && "$pid" != 0 ]] && taskkill /F /T /PID "$pid" >/dev/null 2>&1 || true
      done
    else
      # POSIX fallback.
      for pid in $(lsof -ti tcp:"$port" -sTCP:LISTEN 2>/dev/null); do
        kill -9 "$pid" 2>/dev/null || true
      done
    fi
  done
}

# run_claude <logfile> <label> -- <claude args...>
# Live output via a FOREGROUND pipe: claude's stream-json events flow through
# `tee <jsonl>` (the full raw transcript) into the jq formatter and out to the
# console + <log>. There is deliberately NO background tail/wait on the render —
# when claude exits, EOF cascades and every pipe stage ends on its own, so the
# renderer can never orphan or hang the driver. (An earlier `tail -f --pid`
# design wedged here: on Git Bash tail never detected the native claude.exe
# exiting, so `wait` blocked forever and even a *successful* run hung.) A
# background watchdog enforces the AGENT_TIMEOUT_SECS wall-clock cap by
# taskkill'ing the agent tree — GNU `timeout` can't, because MSYS hands the
# native claude.exe a signal it ignores (that gap let a wedged agent run 13h).
# Retries transient failures with backoff; a retry re-runs the whole slash
# command, safe because /implement and /evaluate read on-disk state first.
run_claude() {
  local log="$1" label="$2"; shift 2
  [[ "${1:-}" == "--" ]] && shift
  local jsonl="${log%.log}.jsonl" done="${log%.log}.done"
  local attempt rc wpid
  for attempt in $(seq 1 "$CLAUDE_RETRIES"); do
    : > "$jsonl"; : > "$log"; rm -f "$done" "$done.timedout"
    # Wall-clock watchdog: force-kill the agent tree if it overruns the cap.
    ( waited=0
      while (( waited < AGENT_TIMEOUT_SECS )); do
        sleep 15; waited=$(( waited + 15 ))
        [[ -f "$done" ]] && exit 0
      done
      [[ -f "$done" ]] && exit 0
      echo "  -- $label exceeded ${AGENT_TIMEOUT_SECS}s wall-clock — force-killing agent tree" >&2
      : > "$done.timedout"
      hard_kill_agents
    ) &
    wpid=$!
    set +o pipefail; set +e
    # stdin from /dev/null, NOT the terminal. claude.exe is a NATIVE Windows
    # binary; when run.sh is launched from an interactive Git Bash / VS Code
    # terminal, the agent inherits an MSYS pseudo-terminal (pty) on stdin. A
    # native app probing/reading that pty for console mode BLOCKS at startup —
    # the session wedges before it emits its first `init` event (0-byte log,
    # near-zero CPU, no child processes), and the only rescue is the wall-clock
    # watchdog an hour later. In -p print mode the prompt comes from the arg, so
    # stdin is unused anyway; detaching it is what non-tty piped runs already do.
    claude "$@" --output-format stream-json --verbose </dev/null 2>&1 \
      | tee "$jsonl" \
      | jq -Rr --unbuffered "$STREAM_FMT" 2>/dev/null \
      | tee -a "$log"
    rc=${PIPESTATUS[0]}
    set -e; set -o pipefail
    touch "$done"; kill "$wpid" 2>/dev/null || true; wait "$wpid" 2>/dev/null || true
    [[ -f "$done.timedout" ]] && { rc=124; rm -f "$done.timedout"; }
    (( rc == 0 )) && return 0
    if (( rc == 124 )); then
      echo "  -- $label TIMED OUT after ${AGENT_TIMEOUT_SECS}s (attempt $attempt/$CLAUDE_RETRIES) — see $jsonl" >&2
    else
      echo "  -- $label exited rc=$rc (attempt $attempt/$CLAUDE_RETRIES) — see $jsonl" >&2
    fi
    (( attempt < CLAUDE_RETRIES )) && sleep $(( attempt * 30 ))
  done
  return 1
}

# ---------- lifecycle: single-instance lock ----------
# One run at a time: overlapping runs share the working tree, feature branch,
# dev-server port, and Playwright profile, and corrupt each other. On any exit
# (Ctrl+C included) we hard-kill the in-flight agent tree — a bare SIGINT reaches
# the bash pipeline but not always the native claude.exe or the dev servers it
# spawned — then release the lock.
LOCK_DIR="harness/.lock"
LOCK_HELD=false

write_lock() { echo $$ > "$LOCK_DIR/pid"; winpid_of $$ > "$LOCK_DIR/winpid" 2>/dev/null || true; LOCK_HELD=true; }
acquire_lock() {
  if mkdir "$LOCK_DIR" 2>/dev/null; then write_lock; return 0; fi
  local other; other=$(cat "$LOCK_DIR/pid" 2>/dev/null || true)
  if [[ -n "$other" ]] && kill -0 "$other" 2>/dev/null; then
    echo "Another run.sh (PID $other) is already running — refusing to start a second instance." >&2
    echo "Stop it with ./harness/kill.sh, or if that's wrong delete $LOCK_DIR, then re-run." >&2
    exit 1
  fi
  echo "  -- stale lock (PID ${other:-unknown} not running) — reclaiming"
  rm -rf "$LOCK_DIR"; mkdir "$LOCK_DIR"; write_lock
}

cleanup() {
  local rc=$?
  hard_kill_agents            # no-op on a clean finish; nukes a live agent on Ctrl+C/error
  reap_dev_servers            # don't leave a dev server holding 5000/5173 for the next run
  if $LOCK_HELD; then rm -rf "$LOCK_DIR"; fi
  exit "$rc"
}
trap cleanup EXIT
trap 'exit 130' INT TERM

escalate() { # escalate <num> <pad> <why>  — commits the flip so the branch carries it
  set_status "$1" "escalated"
  git add "$(feature_file "$1")" 2>/dev/null || true
  git commit -m "chore(harness): feature $2 escalated" >/dev/null 2>&1 || true
  notify "Harness: feature $2 ESCALATED — $3"
  echo "== Feature $2: ESCALATED — $3" >&2
  exit 1
}

synthetic_fail() { # synthetic_fail <pad> <description> — driver-authored fail verdict
  jq -n --arg f "$1" --arg d "$2" \
    '{feature:$f, verdict:"fail", criteria:[], deferred:[],
      findings:[{severity:"blocker", criterion:0, description:$d}],
      summary:("Mechanical driver fail: " + $d)}' \
    > "features/$1.verdict.json"
}

# Non-LLM tripwires after each generator round. Returns 1 (and writes a
# synthetic fail verdict for the next generator round) on trip.
post_generator_checks() { # <num> <pad>
  local num="$1" pad="$2"

  # 0) Status ownership. 'passed' is the evaluator's to set and 'done' is the
  #    driver's — a generator that flips either gets mechanically failed,
  #    because the resume path trusts a committed 'done' as "passed evaluation"
  #    and would push/PR/merge the branch without ever running the evaluator.
  local st_now; st_now=$(status_of "$num")
  if [[ "$st_now" == "done" || "$st_now" == "passed" ]]; then
    echo "  -- TRIPWIRE: generator set status '$st_now' (evaluator/driver-owned value)." >&2
    synthetic_fail "$pad" "The generator set status: $st_now, which only the evaluator (passed/failed) or driver (done) may set. Set status back to implemented (or awaiting_human) and let evaluation decide. Do not self-certify completion."
    set_status "$num" "failed"
    return 1
  fi

  # 1) Generator must end committed. Uncommitted state means it died mid-work
  #    or skipped its commit step; evaluating it would be evaluating mystery state.
  local dirt; dirt=$(tree_dirt)
  if [[ -n "$dirt" ]]; then
    echo "  -- TRIPWIRE: generator left uncommitted changes:" >&2
    echo "$dirt" | sed 's/^/       /' >&2
    synthetic_fail "$pad" "Generator ended the round with uncommitted changes (listed in driver log). Finish the work, run the suite, and commit everything; add build artifacts to .gitignore rather than leaving them untracked."
    set_status "$num" "failed"
    return 1
  fi

  # 2) Deleting tests is the cheapest way to a green suite. Crude pattern match
  #    on deleted paths; a rare false positive costs one round, a false negative
  #    costs you main. Justified renames/moves should be re-added in the same
  #    diff and won't show as pure deletions.
  local deleted
  deleted=$(git diff --name-only --diff-filter=D "$BASE_BRANCH"...HEAD 2>/dev/null \
            | grep -Ei '((^|/)(__tests__|tests?|spec)/)|\.(test|spec)\.' || true)
  if [[ -n "$deleted" ]]; then
    echo "  -- TRIPWIRE: test files deleted in branch diff:" >&2
    echo "$deleted" | sed 's/^/       /' >&2
    synthetic_fail "$pad" "This branch's diff DELETES test files: $(echo "$deleted" | tr '\n' ' '). Restore them. Weakening or removing existing tests to make the suite pass is an automatic round failure. If a test is genuinely obsolete, replace it and justify the change in your summary."
    set_status "$num" "failed"
    return 1
  fi
  return 0
}

wait_for_merge() { # wait_for_merge <branch>
  local branch="$1" waited=0 state failed
  echo "  -- waiting on PR for $branch (mode=$MODE, timeout=${MERGE_TIMEOUT_SECS}s, 0=forever)…"
  while true; do
    state=$(pr_state "$branch")
    case "$state" in
      MERGED) return 0 ;;
      CLOSED) echo "  -- PR closed without merge — stopping." >&2; return 1 ;;
    esac
    if [[ "$MODE" == "autonomous" ]]; then
      # Red CI means auto-merge will never fire; hanging forever at 3am helps no one.
      # (gh pr checks output is tab-separated: name, status, elapsed, url)
      failed=$(gh pr checks "$branch" 2>/dev/null | awk -F'\t' 'tolower($2)=="fail"{print $1}' || true)
      if [[ -n "$failed" ]]; then
        echo "  -- CI checks FAILED on $branch: $(echo "$failed" | tr '\n' ' ')" >&2
        echo "  -- auto-merge will never fire; PR left open for you." >&2
        return 1
      fi
    fi
    if (( MERGE_TIMEOUT_SECS > 0 && waited >= MERGE_TIMEOUT_SECS )); then
      echo "  -- timed out after ${MERGE_TIMEOUT_SECS}s waiting for merge of $branch." >&2
      return 1
    fi
    sleep "$POLL_SECS"; waited=$(( waited + POLL_SECS ))
  done
}

# Autonomous merge gate (no branch protection required): wait for the PR's CI
# checks to ALL complete green, then squash-merge directly. Fail closed on
# every ambiguous state:
#   - pending or not-yet-registered checks mean KEEP WAITING (a just-pushed PR
#     reports zero checks for a while — merging then would be merging ungated);
#   - zero checks after CI_REGISTER_TIMEOUT_SECS means CI never triggered for
#     this base branch (ci.yml trigger misconfig) — refuse, don't merge;
#   - any failed check, or an unknown/terminal-weird state that never resolves
#     within the timeouts — refuse, PR left open for the human.
merge_when_green() { # <branch>
  local branch="$1" waited=0 checks total failed not_green
  echo "  -- waiting for CI on $branch (register timeout ${CI_REGISTER_TIMEOUT_SECS}s, merge timeout ${MERGE_TIMEOUT_SECS}s, 0=forever)…"
  while true; do
    # gh pr checks output is tab-separated: name, state, elapsed, url.
    # It exits nonzero while checks are pending/failed — capture regardless.
    checks=$(gh pr checks "$branch" 2>/dev/null || true)
    total=$(printf '%s' "$checks" | grep -c . || true)
    failed=$(printf '%s\n' "$checks" | awk -F'\t' 'tolower($2)=="fail"{print $1}')
    not_green=$(printf '%s\n' "$checks" | awk -F'\t' '$1!="" && tolower($2)!="pass" && tolower($2)!="skipping"{print $1}')
    if [[ -n "$failed" ]]; then
      echo "  -- CI checks FAILED on $branch: $(echo "$failed" | tr '\n' ' ')" >&2
      return 1
    fi
    if (( total > 0 )) && [[ -z "$not_green" ]]; then
      echo "  -- all $total CI checks green — squash-merging $branch into $BASE_BRANCH"
      gh pr merge "$branch" --squash && return 0
      echo "  -- gh pr merge failed on $branch" >&2
      return 1
    fi
    if (( total == 0 && waited >= CI_REGISTER_TIMEOUT_SECS )); then
      echo "  -- no CI checks registered on $branch after ${CI_REGISTER_TIMEOUT_SECS}s — refusing to merge ungated." >&2
      echo "  -- does .github/workflows/ci.yml trigger on pull_request to $BASE_BRANCH?" >&2
      return 1
    fi
    if (( MERGE_TIMEOUT_SECS > 0 && waited >= MERGE_TIMEOUT_SECS )); then
      echo "  -- timed out after ${MERGE_TIMEOUT_SECS}s waiting for CI on $branch." >&2
      return 1
    fi
    sleep "$POLL_SECS"; waited=$(( waited + POLL_SECS ))
  done
}

# Human provisioning gate. The MARKER is the gate: status stays awaiting_human
# until the evaluator overwrites it, so a deleted marker means the human did
# the steps and we may proceed to evaluation.
human_gate() { # human_gate <num> <pad>
  local num="$1" pad="$2"
  local marker="features/$pad.blocked"
  [[ "$(status_of "$num")" == "awaiting_human" ]] || return 0
  [[ -f "$marker" ]] || return 0   # marker cleared -> gate open

  echo "  -- feature $pad needs manual setup before it can be evaluated:"
  [[ -f "$marker" ]] && sed 's/^/       /' "$marker"
  notify "Harness: feature $pad needs manual setup (Render/Supabase/Vercel). See $marker, then delete it to resume."

  if ! $WAIT_AT_GATES; then
    echo "  -- exiting cleanly at human gate. Do the steps, delete $marker, then re-run run.sh to resume."
    return 1
  fi

  echo "  -- [--wait] blocking until you delete $marker …"
  while [[ -f "$marker" ]]; do
    [[ -f "$STOP_FILE" ]] && { echo "  -- STOP file appeared; halting."; return 1; }
    sleep "$POLL_SECS"
  done
  echo "  -- marker cleared; resuming to evaluation."
  return 0
}

# Put the base branch in place and up to date. Gated mode: best-effort checkout
# + pull of main. Autonomous mode: BASE_BRANCH is the integration branch —
# create it from PROMOTE_BASE if missing, otherwise sync it (ff preferred, real
# merge tolerated, conflict = refuse: that usually means a squash-promotion
# happened and the branch should be deleted so the driver can recreate it).
prepare_base_branch() {
  git fetch origin >/dev/null 2>&1 || true
  if [[ "$MODE" != "autonomous" ]]; then
    git checkout "$BASE_BRANCH" >/dev/null 2>&1 && git pull --ff-only >/dev/null 2>&1 || true
    return 0
  fi
  git checkout "$PROMOTE_BASE" >/dev/null 2>&1 && git pull --ff-only >/dev/null 2>&1 || true
  if branch_exists "$BASE_BRANCH" || git show-ref --verify --quiet "refs/remotes/origin/$BASE_BRANCH"; then
    git checkout "$BASE_BRANCH" >/dev/null 2>&1 \
      || git checkout -b "$BASE_BRANCH" "origin/$BASE_BRANCH" >/dev/null 2>&1
    git pull --ff-only >/dev/null 2>&1 || true   # tolerate no upstream yet
    # Bring in whatever landed on PROMOTE_BASE since (hotfixes, promoted work).
    if ! git merge --ff-only "$PROMOTE_BASE" >/dev/null 2>&1; then
      if ! git merge --no-edit "$PROMOTE_BASE" >/dev/null 2>&1; then
        git merge --abort >/dev/null 2>&1 || true
        echo "Cannot sync $BASE_BRANCH with $PROMOTE_BASE (merge conflict)." >&2
        echo "If you already promoted this branch to $PROMOTE_BASE, delete it and re-run —" >&2
        echo "the driver recreates it fresh from $PROMOTE_BASE:" >&2
        echo "  git branch -D '$BASE_BRANCH' && git push origin --delete '$BASE_BRANCH'" >&2
        exit 1
      fi
      git push origin "$BASE_BRANCH" >/dev/null 2>&1 || true
    fi
  else
    echo "== Creating integration branch $BASE_BRANCH from $PROMOTE_BASE"
    git checkout -b "$BASE_BRANCH" "$PROMOTE_BASE"
    git push -u origin "$BASE_BRANCH" >/dev/null 2>&1 \
      || echo "  -- could not push $BASE_BRANCH to origin (no remote or no auth?) — continuing locally" >&2
  fi
}

# ---------- main loop ----------
# Statuses are read from the checked-out tree, so normalize the vantage point:
# start from an up-to-date base branch when possible. Best-effort — resume
# paths re-read from the feature branch after checking it out, and the blocked
# marker (on disk, gitignored) survives branch switches.
acquire_lock
reap_dev_servers          # clear any dev servers orphaned by a prior crashed/killed run
prepare_base_branch

SELECTED=$(selected_features)
if [[ -z "$SELECTED" ]]; then
  echo "No matching feature files (check features/ exists and your --features-until/--features selection). Run /plan then /slice first." >&2
  exit 1
fi

mkdir -p "$LOG_DIR"
echo "Transcripts: $LOG_DIR"

for num in $SELECTED; do
  if [[ -f "$STOP_FILE" ]]; then
    echo "STOP file found ($STOP_FILE) — halting gracefully. Remove it and re-run to continue."
    exit 0
  fi

  pad=$(printf '%03d' "$num")
  file=$(feature_file "$num")
  st=$(status_of "$num")
  branch="feat/$pad"

  # A blocked marker on disk means the feature is parked at a human gate even
  # if the frontmatter we can currently see (e.g. main's copy) says otherwise.
  [[ -f "features/$pad.blocked" && "$st" != "awaiting_human" ]] && st="awaiting_human"

  if [[ "$st" == "done" ]]; then
    echo "== Feature $pad: already done, skipping"; continue
  fi
  if ! missing=$(deps_done "$num"); then
    echo "== Feature $pad: dependency $missing not done — stopping (re-run after it completes)." >&2
    exit 1
  fi

  resuming_at_gate=false
  fresh=false
  skip_rounds=false
  passed=false

  if $RESTART; then
    fresh=true
  elif [[ "$st" == "escalated" ]]; then
    echo "== Feature $pad: escalated on a previous run." >&2
    echo "   Inspect features/$pad.verdict.json and branch $branch; fix and merge by hand," >&2
    echo "   or re-run with --restart to reset the branch and rebuild from $BASE_BRANCH." >&2
    exit 1
  elif [[ "$st" == "awaiting_human" ]]; then
    if ! branch_exists "$branch"; then
      echo "== Feature $pad: state says awaiting_human but branch $branch does not exist — stale marker or deleted branch." >&2
      echo "   If you want a fresh build of this feature: delete features/$pad.blocked and re-run." >&2
      exit 1
    fi
    echo "== Feature $pad: resuming from human gate"
    git checkout "$branch"
    if ! human_gate "$num" "$pad"; then
      exit 0   # marker still present (or STOP during --wait) — do the steps, delete it, re-run
    fi
    resuming_at_gate=true
  elif branch_exists "$branch"; then
    echo "== Feature $pad: in-flight branch $branch exists (status: $st) — resuming on it."
    echo "   (Committed work is preserved; a pending verdict drives a retry round. Use --restart to reset instead.)"
    git checkout "$branch"
    st=$(status_of "$num")   # re-read from the branch's copy
    if [[ "$st" == "escalated" ]]; then
      echo "== Feature $pad: branch is escalated — see above guidance or use --restart." >&2
      exit 1
    fi
    if [[ "$st" == "awaiting_human" ]]; then
      # The branch says awaiting_human but the marker is gone (a present
      # marker routes through the awaiting_human path above via the loop-top
      # override). Gate cleared: resume into evaluation, don't re-generate.
      echo "== Feature $pad: human gate cleared — resuming into evaluation."
      resuming_at_gate=true
    fi
    if [[ "$st" == "done" ]]; then
      # Passed and marked done on the branch, but the merge never landed
      # (push/PR failure, autonomous refusal, red CI, timeout). 'done' is only
      # trustworthy if the DRIVER committed the flip after a pass verdict — an
      # agent-authored 'done' skipping straight to merge would put unevaluated
      # code on main. Verify authorship via the flip commit's subject line.
      flip_subject=$(git log -1 --format=%s -- "$(feature_file "$num")")
      if [[ "$flip_subject" == "chore(harness): feature $pad passed evaluation"* ]]; then
        echo "== Feature $pad: previously passed (done committed on branch) but never merged — resuming at push/PR/merge."
        echo "   (To rebuild instead: close any open PR, then re-run with --restart.)"
        skip_rounds=true
        passed=true
      else
        echo "== Feature $pad: status is 'done' on the branch, but the last commit touching the feature file" >&2
        echo "   is not the driver's pass flip (found: '${flip_subject:-none}'). Refusing to trust it." >&2
        echo "   Resetting status to implemented and re-running evaluation."
        set_status "$num" "implemented"
        git add "$(feature_file "$num")"
        git commit -m "chore(harness): feature $pad — revert non-driver 'done' flip" >/dev/null
        resuming_at_gate=true   # code exists; skip the generator, go straight to evaluation
      fi
    fi
  else
    fresh=true
  fi

  if $fresh; then
    if [[ "$(pr_state "$branch")" == "OPEN" ]]; then
      echo "== Feature $pad: an OPEN PR already exists for $branch." >&2
      echo "   Merge or close it first — rebuilding underneath an open PR force-pushes over reviewed work." >&2
      exit 1
    fi
    echo "== Feature $pad: starting fresh ($file)"
    git checkout "$BASE_BRANCH" >/dev/null 2>&1 && git pull --ff-only >/dev/null 2>&1 || true
    require_clean_tree
    git checkout -B "$branch" "$BASE_BRANCH"
    rm -f "features/$pad.verdict.json" "features/$pad.blocked"
  fi

  if ! $skip_rounds; then
  for round in $(seq 1 "$MAX_ROUNDS"); do
    if [[ -f "$STOP_FILE" ]]; then
      echo "STOP file found — halting between rounds. Feature $pad resumes on re-run."
      exit 0
    fi

    # Free 5000/5173 before the generator builds: the prior round's agent may
    # have left a server holding the port and locking the build output.
    reap_dev_servers

    if ! $resuming_at_gate; then
      echo "  -- round $round/$MAX_ROUNDS: generator ($GEN_MODEL) [log: $LOG_DIR/$pad-r$round-gen.log]"
      run_claude "$LOG_DIR/$pad-r$round-gen.log" "generator" -- \
        -p "/implement $pad" \
        --model "$GEN_MODEL" \
        --permission-mode acceptEdits \
        --mcp-config "$MCP_CONFIG" --strict-mcp-config \
        --max-turns "$GEN_MAX_TURNS" \
        || escalate "$num" "$pad" "generator failed $CLAUDE_RETRIES consecutive attempts — see $LOG_DIR/$pad-r$round-gen.log"

      # Generator may have parked the feature for manual provisioning.
      if ! human_gate "$num" "$pad"; then
        exit 0   # clean-exit at gate (or STOP); re-run to resume
      fi

      # Non-LLM tripwires: uncommitted state, deleted tests.
      if ! post_generator_checks "$num" "$pad"; then
        echo "  -- tripwire verdict written — next round's generator must fix it"
        continue
      fi
    fi
    resuming_at_gate=false   # only skips the generator for the first post-resume round

    # A stale verdict — or one the GENERATOR wrote into features/ — must never
    # be readable as the evaluator's. Delete unconditionally before every eval;
    # a missing verdict after the eval already maps to "fail" below. The
    # evidence log resets with it so the verify-gate hook counts only THIS
    # evaluation session's execution evidence.
    rm -f "features/$pad.verdict.json" "harness/.evidence-reads"

    # Free 5000/5173 before the evaluator boots the app to exercise it — the
    # generator may have left its self-check server running.
    reap_dev_servers

    echo "  -- round $round/$MAX_ROUNDS: evaluator ($EVAL_MODEL) [log: $LOG_DIR/$pad-r$round-eval.log]"
    run_claude "$LOG_DIR/$pad-r$round-eval.log" "evaluator" -- \
      -p "/evaluate $pad" \
      --model "$EVAL_MODEL" \
      --settings "$EVAL_SETTINGS" \
      --mcp-config "$MCP_CONFIG" --strict-mcp-config \
      --max-turns "$EVAL_MAX_TURNS" \
      --append-system-prompt "$(cat "$EVAL_SYSTEM")" \
      || escalate "$num" "$pad" "evaluator failed $CLAUDE_RETRIES consecutive attempts — see $LOG_DIR/$pad-r$round-eval.log"

    verdict=$(jq -r .verdict "features/$pad.verdict.json" 2>/dev/null || echo "missing")
    if [[ "$verdict" == "pass" ]]; then
      passed=true; break
    fi
    echo "  -- verdict: $verdict — feeding findings back to generator"
  done
  fi  # skip_rounds

  if ! $passed; then
    escalate "$num" "$pad" "failed after $MAX_ROUNDS rounds. Verdict in features/$pad.verdict.json"
  fi

  # Commit the terminal status ON THE BRANCH before pushing: merged main
  # becomes the durable record of completion, the post-merge checkout/pull is
  # clean, and back-to-back features work.
  set_status "$num" "done"
  git add "$(feature_file "$num")"
  git commit -m "chore(harness): feature $pad passed evaluation — mark done" \
    || echo "  -- status flip produced no change (already committed?) — continuing"

  git push -u origin "$branch"

  if [[ "$(pr_state "$branch")" == "OPEN" ]]; then
    echo "  -- reusing existing open PR for $branch"
  else
    gh pr create --base "$BASE_BRANCH" --head "$branch" --fill \
      --body-file "features/$pad.verdict.json" 2>/dev/null \
      || gh pr create --base "$BASE_BRANCH" --head "$branch" --fill
  fi

  if [[ "$MODE" == "autonomous" ]]; then
    # Autonomous merges land on the integration branch ($BASE_BRANCH), never on
    # $PROMOTE_BASE. The non-LLM gates: CI green on every feature PR (checked
    # here, fail-closed), plus the human testing the integration branch before
    # promoting it to $PROMOTE_BASE as one reviewed PR. No branch protection
    # needed — main is never the target of an autonomous merge.
    if ! merge_when_green "$branch"; then
      notify "Harness: feature $pad PR open but CI not green/registered — refusing to merge into $BASE_BRANCH."
      git checkout "$BASE_BRANCH" >/dev/null 2>&1 || true
      exit 1
    fi
  else
    notify "Harness: feature $pad passed evaluation — PR ready for your review (feat/$pad)"
  fi

  if ! wait_for_merge "$branch"; then
    notify "Harness: PR for feature $pad did not merge (closed / red CI / timeout) — needs you."
    git checkout "$BASE_BRANCH" >/dev/null 2>&1 || true
    exit 1
  fi

  git checkout "$BASE_BRANCH" && git pull --ff-only
  git branch -D "$branch" >/dev/null 2>&1 || true
  rm -f "features/$pad.verdict.json" "features/$pad.blocked"
  echo "== Feature $pad: done"
done

notify "Harness: all selected features complete."
echo "All selected features complete. Transcripts in $LOG_DIR"
