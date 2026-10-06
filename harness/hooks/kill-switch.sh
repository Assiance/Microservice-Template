#!/usr/bin/env bash
# PreToolUse hook (all agents, all tools): while harness/STOP exists, deny every
# tool call so an in-flight agent session halts NOW instead of burning turns
# until the driver's next between-rounds check. Exit 2 = block, stderr goes to
# the agent. The driver's own STOP handling still exits the loop gracefully.
root="${CLAUDE_PROJECT_DIR:-.}"
if [[ -f "$root/harness/STOP" ]]; then
  echo "harness/STOP is present: the operator has halted this run. Stop working immediately and end your session with a one-line note of where you left off. Do not attempt further tool calls." >&2
  exit 2
fi
exit 0
