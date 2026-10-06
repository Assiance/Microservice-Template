#!/usr/bin/env bash
# Force-stop a stuck harness run and everything it spawned.
#
#   ./harness/kill.sh          # kill the active run.sh tree + free port 5173
#   ./harness/kill.sh --stop   # graceful: ask the in-flight AGENT to halt (via
#                              # harness/STOP), leaving run.sh to exit cleanly
#
# Why this exists: on Windows, Ctrl+C / `kill` reach the bash process but not
# reliably the native claude.exe or the dev servers it launched. taskkill /T
# kills the whole process tree. run.sh records its own Windows PID in the lock,
# so this finds the tree without you hunting PIDs.
set -uo pipefail
export MSYS_NO_PATHCONV=1
cd "$(dirname "$0")/.." || exit 1

# --- graceful mode: let the kill-switch hook stop the agent mid-work ----------
if [[ "${1:-}" == "--stop" ]]; then
  touch harness/STOP
  echo "Wrote harness/STOP — the in-flight agent halts on its next tool call and"
  echo "run.sh exits between rounds. Delete it before re-running:  rm harness/STOP"
  exit 0
fi

winpid_of() { ps -W 2>/dev/null | awk -v p="$1" '$1==p{print $4; exit}'; }
killwin()   { taskkill /F /T /PID "$1" >/dev/null 2>&1 && echo "  killed tree of winpid $1"; }
killed=0

# 1) Preferred: the Windows PID run.sh recorded in its lock.
if [[ -f harness/.lock/winpid ]]; then
  w=$(cat harness/.lock/winpid 2>/dev/null || true)
  [[ -n "${w:-}" ]] && killwin "$w" && killed=1
elif [[ -f harness/.lock/pid ]]; then
  w=$(winpid_of "$(cat harness/.lock/pid 2>/dev/null)")
  [[ -n "${w:-}" ]] && killwin "$w" && killed=1
fi

# 2) ALWAYS sweep the harness CLI agent trees too — taskkill /T on the run.sh
#    tree has been observed to miss the agent (MSYS parentage isn't always
#    visible to Windows), leaving the evaluator running after a "successful"
#    kill. This matches only the `claude` CLI (.local/bin/claude), never the
#    desktop app or VS Code extension (…\claude.exe), so your other sessions
#    are untouched.
for pid in $(ps -W 2>/dev/null | awk '/\.local\/bin\/claude$/{print $1}'); do
  w=$(winpid_of "$pid"); [[ -n "$w" ]] && killwin "$w" && killed=1
done

# 3) Free the Vite dev-server port — double-forked servers can outlive the tree.
for p in $(netstat -ano 2>/dev/null | grep -E ':5173\b.*LISTENING' | awk '{print $NF}' | sort -u); do
  taskkill /F /PID "$p" >/dev/null 2>&1 && echo "  freed port 5173 (pid $p)"
done

# 4) Release the lock so the next run can start.
rm -rf harness/.lock 2>/dev/null && echo "  removed harness/.lock"

if (( killed )); then echo "Done. Re-run with ./harness/run.sh ..."; else echo "No active harness run found (nothing to kill)."; fi
