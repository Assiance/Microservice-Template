---
description: Compact SPEC.md and PLAN.md — shrink the record, never the contract
---

You are in COMPACTION mode. Your job is to make SPEC.md and PLAN.md short
again after iterations have grown them, WITHOUT losing any information a
fresh-context agent still needs. These are binding contract documents that
every generator and evaluator session obeys — a careless cut here silently
changes what gets built. Be conservative: when in doubt, keep it.

## The three rules

1. **Only compact what has a mechanical doneness signal.** Something is
   compactable history ONLY if you can point at the signal that says it is
   finished:
   - A PLAN.md slice-table row / per-slice detail block whose feature file
     has `status: done` (check `features/NNN-*.md` frontmatter yourself —
     do not trust the table).
   - A D-record explicitly marked superseded.
   - "This iteration" scope in SPEC.md whose implementing features are ALL
     done.
   - Prose that verbatim restates what a terser line in the same document
     already says.
   No signal = not history = stays verbatim.
2. **Never touch the live surface.** Off limits entirely: active
   (non-superseded) D-records, the "Current state (as built)" capability
   list (you may ADD shipped scope to it — see below — never remove from
   it), unfinished iteration scope, Open questions, and everything under
   `features/` — this command does not edit feature files, ever. DESIGN.md
   and CLAUDE.md are also out of scope unless the user explicitly asks.
3. **The git diff is the safety net.** Leave your edits uncommitted. Finish
   by summarizing exactly what you removed or condensed (with before/after
   line counts per file) and tell the user to review `git diff` and commit.
   Git history is the archive — nothing is ever truly lost — but the human
   approves every compaction.

## What compaction looks like

- **PLAN.md, shipped slices**: collapse each done slice's detail block
  (seam, criteria sketches, human_setup) — the feature file and git history
  carry all of it. Keep one line per shipped slice in the table (number,
  name, "shipped") so numbering context survives.
- **PLAN.md, superseded decisions**: collapse the full record to one line —
  `D4: SQS — superseded by D9 (see git history)`. The successor record
  stays full.
- **SPEC.md, shipped scope**: fold "This iteration" items whose features
  are all done into "Current state (as built)" as terse capability lines
  (one line per capability, not the original user-story prose), then
  remove them from the iteration section. Unfinished items stay where they
  are, untouched.
- **Both docs**: delete duplicated or superseded prose only when the
  surviving text says the same thing — tightening wording is fine,
  changing meaning is not.

## Safety check (mandatory, before finishing)

Grep every NON-done feature file (`pending`, `in_progress`, `implemented`,
`failed`, `awaiting_human`, `escalated`) for references to PLAN.md decision
numbers (D#) and SPEC.md scope items. Anything still referenced by
unfinished work must survive compaction in full — if your edit condensed a
D-record that an unfinished feature references, restore it. List the
references you checked in your summary.

Do not commit. Do not touch feature files. Do not start implementing
anything.
