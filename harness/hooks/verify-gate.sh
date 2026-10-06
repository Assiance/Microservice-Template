#!/usr/bin/env bash
# PreToolUse hook (evaluator only, wired in eval-settings.json) with two jobs:
#
# 1) EVIDENCE GATE (deny): writing features/*.verdict.json before this session
#    has produced execution evidence (recorded by track-evidence.sh) is denied.
#    Makes "verification means EXECUTION" mechanical instead of prompted: a
#    verdict with zero observed runs is exactly the lazy-evaluator failure mode
#    the harness exists to prevent. Tripwire, not proof of thoroughness.
#
# 2) FEATURES ALLOW: Write/Edit under features/ is explicitly allowed HERE
#    because settings path rules can't be trusted for this on Windows — the
#    agent addresses the same file as features/x, C:/repo/features/x, or
#    C:\repo\features\x, and no single Write(...) glob matches all three
#    (feature 002's evaluator burned its whole turn budget re-trying a denied
#    verdict write). The hook normalizes the path and decides, so the verdict
#    and the feature file's status frontmatter are always writable.
#
# Anything outside features/ emits nothing, falling through to the normal
# permission flow (auto-deny in headless mode) — the evaluate-don't-fix
# boundary is unchanged.
root="${CLAUDE_PROJECT_DIR:-.}"
payload=$(cat)
tool=$(jq -r '.tool_name // empty' <<<"$payload")
[[ "$tool" == "Write" || "$tool" == "Edit" ]] || exit 0
fp=$(jq -r '.tool_input.file_path // empty' <<<"$payload")
[[ -n "$fp" ]] || exit 0

fpn="${fp//\\//}"                     # backslashes -> forward slashes
[[ "$fpn" == *..* ]] && exit 0        # no traversal — normal permission flow
rootn="${root//\\//}"

shopt -s nocasematch                  # Windows drive letters vary in case
if [[ "$fpn" == features/* || "$fpn" == "$rootn/features/"* ]]; then
  :
elif [[ "$rootn" == "." && "$fpn" == */features/* ]]; then
  :                                   # no project dir known — best effort
else
  shopt -u nocasematch
  exit 0
fi
shopt -u nocasematch

if [[ "$fpn" == *.verdict.json ]]; then
  log="$root/harness/.evidence-reads"
  count=$(grep -c . "$log" 2>/dev/null); count=${count:-0}
  execish=$(grep -cE '^(browser|exec)' "$log" 2>/dev/null); execish=${execish:-0}
  if (( count < 3 || execish < 1 )); then
    echo "DENIED: you are trying to write a verdict with only $count evidence events ($execish execution events) recorded this session. Verification means execution: run the full test suite, stand the app up (./init.sh), and exercise the acceptance criteria against the RUNNING application (curl / Playwright) before writing any verdict. If a probe is genuinely impossible, do everything that IS possible first and record the limitation in the verdict summary." >&2
    exit 2
  fi
fi

jq -n '{hookSpecificOutput:{hookEventName:"PreToolUse",permissionDecision:"allow",permissionDecisionReason:"features/ is the evaluator-writable surface (verdict JSON + status frontmatter)"}}'
exit 0
