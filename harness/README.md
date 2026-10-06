# Agentic Feature Harness for Claude Code

Idea → spec → design → technical plan → numbered features → autonomous
implement/QA/PR loop.
Architecture per Anthropic's harness-engineering posts: separate generator
and evaluator agents, file-based handoffs, fresh context per feature.

See [`ARCHITECTURE.md`](ARCHITECTURE.md) for the diagrams: the three phases
(first build, autonomous loop, iteration), the command reference, and the
single-writer ownership map.

## Setup

1. Drop `.claude/`, `harness/`, and an empty `features/` into your repo.
2. Requirements: `claude` CLI (logged in on your Max plan), `gh` (authed),
   `jq`. For UI projects, add the Playwright MCP so the evaluator can click
   through the running app: `claude mcp add playwright -- npx @playwright/mcp@latest`
3. **Permissions (required for headless runs):** copy
   `harness/settings.example.json` to `.claude/settings.json` and adjust the
   bash allowlist to your stack. In `-p` mode nobody can answer permission
   prompts — without the allowlist, the generator stalls on its first build
   or test command. Keep `git push`/`gh pr merge` denied: pushing and merging
   are the DRIVER's job, not the agents'. Notes: `cat` is deliberately not
   allowed (it defeats the `.env` read deny), and `curl` is allowed only
   pinned to localhost so the evaluator can hit API endpoints. For fully
   autonomous overnight runs, the more robust option is running the whole
   loop in a Docker sandbox with `--dangerously-skip-permissions` — never use
   that flag outside a sandbox.
4. **Evaluator boundary (mechanical):** `harness/eval-settings.json` ships
   with the harness and is passed automatically on every `/evaluate` call.
   It restricts the evaluator's writes to `features/` and denies git
   add/commit/checkout — the evaluate-don't-fix rule is enforced by
   permissions, not just by prompt. run.sh refuses to start without it. The
   driver also deletes any verdict file before each evaluation, so a stale
   or generator-written verdict can never be read as the evaluator's.
   It also wires the **evidence gate** (`harness/hooks/`):
   `track-evidence.sh` logs every execution event in the eval session
   (Playwright calls, test/app runs, curl probes, screenshot reads) and
   `verify-gate.sh` denies writing the verdict file until enough exist —
   "verification means execution" is a hook, not just a prompt. The driver
   resets the log (`harness/.evidence-reads`) before each evaluation.
5. **.gitignore** these driver-local paths:
   ```
   harness/logs/
   harness/STOP
   harness/STEER.md*
   harness/.evidence-reads
   features/*.verdict.json
   features/*.blocked
   ```
6. **Autonomous mode never touches main.** It builds on an integration branch
   (`harness/integration`, override via `INTEGRATION_BRANCH`): feature branches
   are cut from it, per-feature PRs target it, and the driver squash-merges
   each PR only after ALL its CI checks complete green (fail-closed: pending,
   unregistered, or red checks refuse the merge and stop the run — see
   `CI_REGISTER_TIMEOUT_SECS`). After the run you test the integration branch,
   promote it to main as ONE reviewed PR, then delete the branch so the next
   run recreates it fresh from main. The non-LLM gates are per-feature CI plus
   your end-of-run testing — no branch protection required, which keeps
   autonomous mode viable on GitHub Free private repos. (Branch protection on
   main is still worth enabling if your plan allows it, especially with
   teammates.) Make sure your CI workflow triggers on `pull_request` to the
   integration branch, or every merge will fail closed waiting for checks.

## Run controls

- **Live output + transcripts:** agents run with `--output-format stream-json`,
  so progress streams to the console as one line per event (tool calls, text,
  errors — permission denials show up as `xx` lines within seconds). The
  rendered view is saved to `harness/logs/<run-timestamp>/NNN-rR-{gen,eval}.log`
  and the raw event stream to the `.jsonl` next to it. Evaluator tuning =
  reading these and patching `harness/evaluator-system.md` where its
  judgment diverged from yours. Add `harness/logs/` to `.gitignore`.
  Agent output flows through a foreground pipe (`claude | tee jsonl | jq | tee
  log`), so there is no background tailer to orphan or wedge — when the agent
  exits, the render ends with it.
- **Per-agent timeout (wall-clock):** a background watchdog force-kills the
  agent tree if a single invocation runs past `AGENT_TIMEOUT_SECS` (default
  3600), turning a wedged CLI/API call into a bounded `rc=124` the retry loop
  re-runs. It uses `taskkill /F /T` rather than GNU `timeout`, because on Windows
  MSYS hands the native `claude.exe` a signal it ignores — `timeout` would fire
  but the process would keep running (this is what let a stuck agent run 13h).
  Pair the cap with `GEN_MAX_TURNS`/`EVAL_MAX_TURNS` for the turn-count guard.
- **Stopping a stuck run:** `./harness/kill.sh` force-kills the active run and
  everything it spawned (agent + dev servers), frees port 5173, and clears the
  lock — it finds the tree via the Windows PID run.sh records in `harness/.lock`,
  so you don't hunt PIDs. `./harness/kill.sh --stop` is the graceful variant
  (writes `harness/STOP`; the agent halts on its next tool call and the driver
  exits cleanly). Ctrl+C in the run.sh terminal also works — the driver's exit
  trap runs the same `taskkill` teardown.
- **One run at a time:** the driver takes `harness/.lock` at startup and
  refuses to start while another run.sh is alive (stale locks from dead PIDs are
  reclaimed automatically); it releases the lock and force-kills any in-flight
  agent tree on exit.
- **Stop (immediate):** `touch harness/STOP` — the kill-switch hook
  (`.claude/settings.json` → `harness/hooks/kill-switch.sh`) denies every
  tool call the moment the file exists, so an in-flight agent session halts
  NOW instead of burning turns; the driver then exits between rounds as
  before. Remove the file and re-run to resume; an interrupted feature
  resumes on its existing branch. (The hook fires in interactive sessions
  too — if tools mysteriously fail, check for a leftover STOP file.)
- **Steering (mid-run, no restart):** write `harness/STEER.md` while a run
  is live. The steer hook delivers its content to whichever agent is
  currently running on its next tool call, then renames the file to
  `.delivered` so it fires exactly once per note.
- **Driver tripwires (non-LLM gates):** after every generator round the
  driver mechanically fails the round — writing a synthetic fail verdict the
  next generator round must fix — if (a) the generator set `status: passed`
  or `done` (those are evaluator/driver-owned; a self-certified `done` would
  merge unevaluated code via the resume path), (b) the tree has uncommitted
  changes (generator died mid-work or skipped its commit), or (c) the branch
  diff DELETES test files (the cheapest way to a green suite is weakening
  it). The test check is a crude path match; a rare false positive costs one
  round, a false negative costs you main. On resume, a committed
  `status: done` is only trusted if the flip commit is the driver's own
  (subject-line check); otherwise status resets to `implemented` and the
  slice goes back through evaluation instead of skipping to merge.
- **Transient failures:** each agent invocation retries up to `CLAUDE_RETRIES`
  (default 3) with backoff on nonzero exit, then the feature is marked
  `escalated` instead of killing the whole overnight run mid-loop.
- **Resume vs restart:** re-running resumes an in-flight feature on its
  existing `feat/NNN` branch — committed work and a pending fail-verdict are
  preserved (the verdict drives a retry round). A feature that passed but
  never merged (push failure, red CI, auto-merge refusal, timeout) resumes
  directly at the push/PR/merge stage without re-running any agents. A
  feature still parked at a human gate re-checks its marker on resume — you
  must actually delete `features/NNN.blocked` to proceed. `--restart`
  force-resets the branch from main instead; the driver refuses to reset
  under an OPEN PR (close or merge it first), and refuses to touch an
  `escalated` feature at all without `--restart` or manual fixing.
- **Human provisioning gates:** when a slice needs manual Render/Supabase/Vercel
  work (apply a migration, set a secret, configure a project), the generator
  parks it at `status: awaiting_human`, writes the checklist to the feature
  file and to `features/NNN.blocked`, and the driver stops. Do the steps,
  delete the marker, re-run `run.sh` to resume into evaluation. By default the
  script **exits cleanly** at a gate (robust across multi-day waits — state
  lives in files/git, nothing has to stay running). Pass `--wait` to instead
  block-and-poll through a short gate in one continuous run. `NOTIFY_CMD` fires
  at the gate either way.
- **Notifications:** set `NOTIFY_CMD` to any command taking a message arg
  (ntfy, Slack webhook script). Fires on PR-ready (gated mode), human gates,
  escalation, and completion. Example: `NOTIFY_CMD="ntfy publish my-topic" ./harness/run.sh …`

## Golden template + monorepo

The intended setup is a backend+frontend monorepo, itself a GitHub **template
repo** so one command gives a clean repo with everything inherited:

```bash
gh repo create my-app --template Assiance/base-app --private --clone && cd my-app
```

This copies a single fresh commit of the default branch — `.claude/`, `harness/`,
`features/`, vendored skills, and (once distilled) the CLAUDE.md conventions +
init.sh all come along. No history, no other branches; the `harness/integration`
branch is recreated fresh from main on the first autonomous run. Per machine,
confirm `gh`/`jq` and the Playwright MCP are present — those aren't copied.

```
my-app/
├── CLAUDE.md            # thin: monorepo layout, cross-cutting/contract rules, pointers
├── backend/
│   └── CLAUDE.md        # backend patterns (from /conventions backend)
├── frontend/
│   └── CLAUDE.md        # frontend patterns (from /conventions frontend)
├── .claude/
│   ├── settings.json    # ONE merged allowlist (dotnet + npm/vite/playwright)
│   ├── commands/        # the slash commands
│   └── skills/          # design skills (root-level in a monorepo)
├── harness/             # run.sh, evaluator-system.md, settings.example.json
└── features/
```

Setup, once in the template:

1. Bring your microservice-template content in under `backend/`, scaffold
   `frontend/`, and commit `.claude/`, `harness/`, empty `features/`.
2. Copy `harness/settings.example.json` → `.claude/settings.json`; the example
   is already merged for both stacks — trim what you don't use.
3. Distill conventions per stack:
   `/conventions` (writes the thin root — and authors `init.sh`, the ONE
   committed way to boot both stacks + smoke check; every agent session
   starts the app via `./init.sh`, never ad-hoc commands), then
   `/conventions backend` and `/conventions frontend`. Commit the three
   CLAUDE.md files and init.sh.
4. CLAUDE.md is auto-loaded hierarchically: an agent editing `backend/…` gets
   root + backend conventions; a frontend agent gets root + frontend. Free and
   deterministic — no re-reading the template each run.
5. Re-run a stack's `/conventions` only when that stack's patterns change.

## Recommended skills (optional, per-template)

Install skills into `.claude/skills/` (committed). In a monorepo that's the
repo root, so they're shared across stacks — backend slices simply won't
trigger frontend skills. Every installed skill costs tokens in every session,
so install only what earns it. Read any skill's SKILL.md before installing;
only the vercel-labs and anthropics entries are from official sources.

| Skill | Source | Stage | Why |
|---|---|---|---|
| frontend-design | anthropics/skills | /design | Intentional design choices over generic AI aesthetics; near-zero footprint |
| web-design-guidelines | vercel-labs/agent-skills | /evaluate | Audits UI against 100+ a11y/UX rules, file:line findings |
| react-best-practices | vercel-labs/agent-skills | /implement + /evaluate | ~70 React/Next perf rules with good/bad examples (React frontends only) |
| ui-ux-pro-max | nextlevelbuilder (community) | /design | Design-system generator: concrete candidate palettes/styles/pairings to react to during the design interview |
| greploop | greptileai/skills | PR stage | Iterates PR against Greptile review until clean (requires paid Greptile) |

Design skills are design-time only: once DESIGN.md exists it is the sole
authority, and /implement and /evaluate follow the doc, not skill databases.
Audit skills at /evaluate (web-design-guidelines, react-best-practices) are
subordinate to the repo docs: evaluate.md now states that DESIGN.md/CLAUDE.md
win on conflict, and skill findings count only where the docs are silent
(a11y, perf, framework correctness). Two rulebooks with no precedence rule
is a coin-flip the evaluator resolves silently — so the precedence is
written down. Community skills (ui-ux-pro-max) run with bash access: read
the SKILL.md before install, pin the version, diff on update. Do not install
find-skills (or any skill-installing skill) in repos the autonomous loop
runs in.

### Installed in this repo (pinned)

Four skills are vendored under `.claude/skills/` (committed, so they travel to
template children and are available to headless `-p` runs). They're pinned to a
recorded commit — not auto-updating — with provenance and the update workflow in
[`.claude/skills/SOURCES.md`](../.claude/skills/SOURCES.md).

| Skill | Path | Stage | What you get |
|---|---|---|---|
| frontend-design | `.claude/skills/frontend-design` | /design | Design-lead system prompt pushing distinctive, non-templated aesthetic choices. Near-zero footprint. |
| react-best-practices | `.claude/skills/react-best-practices` | /implement + /evaluate | 70 React/Next perf rules across 8 categories (waterfalls, bundle, re-render…), each with good/bad examples. Matches this repo's React 19 frontend. |
| web-design-guidelines | `.claude/skills/web-design-guidelines` | /evaluate | Audits UI against 100+ a11y/UX rules, `file:line` findings. Thin wrapper that fetches the *latest* rules from raw GitHub at runtime, so its guidance self-updates. |
| ui-ux-pro-max | `.claude/skills/ui-ux-pro-max` | /design | Searchable design-system generator (styles, palettes, font pairings, UX rules) via a Python tool. **Community skill, runs Python w/ bash access — requires Python 3.x; diff on every update.** |

Pinning means updates are a deliberate pull, never a silent mid-run change:
re-clone the source at latest, copy the folder over the old one, then
`git diff .claude/skills/<name>` before committing. See SOURCES.md for the
pinned SHAs and an upstream-HEAD check one-liner.

Precedence still holds for all four: DESIGN.md/CLAUDE.md win on conflict; skill
findings count only where the repo docs are silent (a11y, perf, framework
correctness).

## Workflow

```bash
# New project from the template repo:
#   gh repo create my-app --template Assiance/base-app --private --clone && cd my-app
# Conventions are already inherited; if building the template itself, distill once:
claude
> /conventions            # thin root CLAUDE.md
> /conventions backend     # backend/CLAUDE.md
> /conventions frontend    # frontend/CLAUDE.md

> /refine a kanban board with offline sync
# …answer the interview, review/edit SPEC.md…
> /design
# …design interview, review/edit DESIGN.md… (skip for headless/CLI projects)
> /plan
# …TECHNICAL interview: argue transport/data model/auth candidates, then
#   review/edit PLAN.md — the Decisions section is the thing to get right;
#   it is binding on the generator and enforced by the evaluator…
> /slice
# …mechanical expansion of PLAN.md into features/NNN-*.md; skim the coverage
#   table + seam criteria and any human_setup steps…
exit

./harness/run.sh --features-until 4 --mode gated     # PRs to main, you merge each one
./harness/run.sh --features 5,7-9 --mode autonomous  # builds on harness/integration:
                                                     # merges each feature there on
                                                     # evaluator pass + CI green
./harness/run.sh --features-until 4 --wait           # block through human gates in one run

# After an autonomous run: test the integration branch, then promote it.
git checkout harness/integration   # run the app, click through the features
gh pr create --base main --head harness/integration --title "Promote features 001-004"
# review + squash-merge the promotion PR, then delete the branch so the next
# run recreates it fresh from main (avoids squash-merge divergence):
git checkout main && git pull
git branch -D harness/integration && git push origin --delete harness/integration
```

Re-running is safe: `done` features are skipped and a feature parked at
`awaiting_human` resumes into evaluation — so `--features-until 8` after
`--features-until 4` builds 005-008, and re-running after a provisioning gate
continues that slice.

## Iterating on an existing app

Apps evolve; the docs are living contracts, not a v1 snapshot. When you want
the next batch of features:

- **`/iterate <goal>`** — the brownfield entry point. It triages every
  feature (done / in-flight / parked / escalated / not started), interviews
  you about the new goals AND any pressure on existing PLAN.md decisions
  (challenging migrate-vs-conform-vs-scope before accepting a revision),
  then presents the blast radius per feature state: done features get
  migration slices, not-started files get regenerated, in-flight features
  are explicitly restarted (`--restart`, work discarded) or deferred — never
  silently built against changed docs. On approval it updates SPEC.md
  (shipped scope folds into "Current state") and PLAN.md (revised decisions
  get superseded notes + successor records), then hands you to /design
  (extend mode — only when the batch introduces UI surfaces DESIGN.md is
  silent on, or a design revision was approved) and /slice. Requires a
  quiesced state: no active run, and in autonomous mode promote (or delete)
  `harness/integration` first so statuses read true.
- **`/compact`** — run when SPEC.md/PLAN.md grow fat with shipped history.
  Collapses only what has a mechanical doneness signal (done slices'
  detail blocks, superseded D-records, fully-shipped iteration scope) and
  never touches the live contract or `features/`; it leaves the edit
  uncommitted and you review the git diff. Git history is the archive.
- Modification slices (changing existing behavior, including migrations)
  carry PRESERVATION criteria — see /slice rule 6 — so the evaluator
  executes "what must not change" alongside "what's new".

## Knobs (env vars)

| Var | Default | Notes |
|---|---|---|
| `GEN_MODEL` | `sonnet` | Generator. Sonnet draws the separate Sonnet weekly pool on Max — set `opus` for gnarly features. |
| `EVAL_MODEL` | `opus` | Evaluator judgment is where Opus earns its cost. |
| `MAX_ROUNDS` | `3` | Implement→evaluate rounds before the script stops and escalates to you. |
| `GEN_MAX_TURNS` / `EVAL_MAX_TURNS` | `150` / `120` | Runaway-session guards. Eval raised from 80: evaluate.md's mandate (both stacks up, full suite, per-criterion edge probes via Playwright, design screenshots, diff review) truncated on fat slices. Watch transcripts for evals that ran out of turns. |
| `CLAUDE_RETRIES` | `3` | Retries per agent invocation on nonzero exit before escalating the feature. Caveat: a max-turns kill may also exit nonzero, and a retry re-runs the full session — set to `1` if you'd rather escalate immediately than risk 3x a long session. |
| `AGENT_TIMEOUT_SECS` | `3600` | Hard wall-clock cap per agent invocation. On timeout the agent is killed (rc=124) and retried like any other failure. |
| `MERGE_TIMEOUT_SECS` | `0` | Max wait for a PR's CI + merge (0 = forever). Autonomous mode additionally fail-fasts on red CI regardless of this. |
| `BASE_BRANCH` | `main` | Gated-mode target, and the branch the integration branch is created from / promoted to. |
| `INTEGRATION_BRANCH` | `harness/integration` | Autonomous-mode build/merge target. Created from `BASE_BRANCH` if missing; synced from it at run start (conflict = refuse with delete-and-recreate guidance). |
| `CI_REGISTER_TIMEOUT_SECS` | `300` | Autonomous merges wait for checks to APPEAR on a PR; if none register in this window (usually a ci.yml trigger misconfig), the merge fails closed instead of landing ungated. |

## Feature lifecycle

`pending → in_progress → implemented → passed/failed → done`,
with two extra states:
- `awaiting_human` — generator parked the slice for manual provisioning;
  driver paused. Clear `features/NNN.blocked` and re-run to resume.
- `escalated` — failed `MAX_ROUNDS` evaluations (or an agent invocation
  failed repeatedly); verdict in `features/NNN.verdict.json` explains why.
  The driver won't touch an escalated feature again until you either fix and
  merge by hand or pass `--restart`.

The `done` flip is committed by the driver ON the feature branch before
pushing, so it lands in main with the merge — merged history is the durable
record of completion, and re-runs (or a fresh clone) skip correctly. (The
prior design left `passed`/`done` as uncommitted working-tree edits, which
blocked the post-merge checkout of main and broke back-to-back features.)

Acceptance criteria tagged `[env-gated]` that can't be checked locally are
recorded as **deferred** in the verdict (not failed) and surfaced in the PR
for you to confirm after deploy. A slice still passes on its locally
verifiable criteria — unless *everything* is deferred, which fails with a note
that the slice needs restructuring to be testable.

## Tuning the evaluator (this is the real work)

`harness/evaluator-system.md` is the quality dial. When a bad feature passes:
find the evaluator transcript, identify where its judgment diverged from
yours, and add that specific failure pattern to the "Forbidden moves" or
"Calibration" sections. Prefer WORKED EXAMPLES over new rules where you can —
the "Worked examples" section exists to be grown from real transcript
excerpts (finding → severity → why), which calibrate judgment better than
abstract rules do. Watch your criteria language too: grading phrasing steers
output character (Anthropic found "museum quality" wording caused visual
convergence), so describe the contract, not an ideal. Expect several rounds
of this. Periodically try *removing* rules too — every harness component
encodes an assumption about what the model can't do, and those go stale as
models improve; after each model upgrade, disable one component at a time
(evaluator rules, thin-slice sizing, MAX_ROUNDS) and measure.

## Max 5x discipline

- Run feature batches (`--features-until N`), not whole apps, and keep the Sonnet
  generator default — it's effectively a second budget pool.
- The retry cap is your burn protection; don't raise MAX_ROUNDS past 3.
- If Claude Code offers to continue on API usage credits mid-run, that's
  billed at API rates on top of your plan. Decline unless you mean it.
