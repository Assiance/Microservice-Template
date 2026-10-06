#!/usr/bin/env bash
# PostToolUse hook (all agents, all tools): one-shot operator steering without a
# restart. Write harness/STEER.md while a run is live; the next tool call
# delivers its content to whichever agent is currently running, then renames the
# file to .delivered so it fires exactly once per note.
root="${CLAUDE_PROJECT_DIR:-.}"
note="$root/harness/STEER.md"
[[ -f "$note" ]] || exit 0
content=$(cat "$note")
mv "$note" "$note.delivered"
jq -n --arg c "$content" \
  '{hookSpecificOutput:{hookEventName:"PostToolUse",
    additionalContext:("Operator steering note (one-time, from harness/STEER.md — follow it for the rest of this session): " + $c)}}'
exit 0
