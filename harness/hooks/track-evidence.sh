#!/usr/bin/env bash
# PostToolUse hook (evaluator only, wired in eval-settings.json): append a line
# to harness/.evidence-reads for every tool call that counts as EXECUTION
# evidence — browser interaction, test/app runs, API probes, screenshot reads.
# verify-gate.sh requires these lines to exist before the verdict may be
# written. The driver deletes the log before each evaluation session.
root="${CLAUDE_PROJECT_DIR:-.}"
log="$root/harness/.evidence-reads"
payload=$(cat)
tool=$(jq -r '.tool_name // empty' <<<"$payload")
case "$tool" in
  mcp__playwright__*)
    echo "browser $tool" >> "$log"
    ;;
  Bash)
    cmd=$(jq -r '.tool_input.command // empty' <<<"$payload")
    if grep -qE '(dotnet (test|run)|npm (run|test)|pnpm (run|test)|npx playwright|npx vite|curl |init\.sh)' <<<"$cmd"; then
      printf 'exec %.300s\n' "$cmd" >> "$log"
    fi
    ;;
  Read)
    fp=$(jq -r '.tool_input.file_path // empty' <<<"$payload")
    if [[ "$fp" =~ \.(png|jpe?g)$ ]]; then
      echo "screenshot $fp" >> "$log"
    fi
    ;;
esac
exit 0
