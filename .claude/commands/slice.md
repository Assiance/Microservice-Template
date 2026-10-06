---
description: Mechanically expand the approved PLAN.md into numbered feature files
---

You are in SLICING mode. Read PLAN.md; if it does not exist, stop and tell
the user to run /plan first. Read SPEC.md, DESIGN.md (if present), and
CLAUDE.md too.

PLAN.md is the APPROVED plan: the technical decisions are made and the slice
boundaries are set. Your job is a faithful expansion of its slice plan into
`features/NNN-*.md` — not a re-plan. If you find a genuine problem while
expanding (a missing dependency, a slice whose criteria can't be made
testable, a contradiction between PLAN.md and SPEC/DESIGN), STOP and surface
it to the user so they can fix PLAN.md — do not silently restructure.

## Existing feature files (incremental runs)

If features/ already has files: only ADD slices that are in PLAN.md's table
but have no feature file yet, numbered after the existing ones. Never
renumber existing features, and never rewrite a feature whose status is
anything other than `pending` — the harness owns those. If PLAN.md changed a
slice that is already built or in flight, surface it; that's a human
decision, not yours.

## Decomposition rules

1. One file per slice in PLAN.md's slice table, 1:1, same order:
   `features/001-<slug>.md`, `002-<slug>.md`, … Numbering is build order.
   Lower numbers must not depend on higher numbers.
2. Features are VERTICAL SLICES, not stack layers. A slice spans both
   `backend/` and `frontend/` for one user-visible capability. Do NOT split a
   capability into a backend feature and a frontend feature — that
   reintroduces the integration gap the harness exists to catch. `depends_on`
   runs between SLICES, never between halves of one slice. If PLAN.md's
   boundaries violate this, stop and say so.
3. Slice THIN. One generator session must be able to build both sides and
   test them. If a PLAN.md slice is clearly too fat for one session, stop and
   propose the split to the user rather than inventing it.
4. Foundation first (scaffold, data layer, auth), then capability slices,
   then polish/integration — PLAN.md's ordering should already reflect this;
   flag it if not. The scaffold slice must produce (or extend) a working
   `./init.sh` at the repo root and include an acceptance criterion asserting
   it (e.g. "./init.sh starts backend and frontend and its smoke check
   passes").
5. DECISIONS BECOME CRITERIA. Where a PLAN.md decision is observable in a
   slice's behavior, bake it into an acceptance criterion so the evaluator
   enforces it: "board updates arrive over the SignalR connection (D2), not
   by polling" — not just "board updates appear". Reference the decision
   number. This is how the plan stays binding after fresh-context agents take
   over.
6. MODIFICATION SLICES. On an existing app (iterations), a slice often
   CHANGES current behavior — including migration slices from a revised
   PLAN.md decision — rather than adding something new. Every such slice
   must carry at least one PRESERVATION criterion naming the existing
   behavior that must not change ("existing untagged search returns the
   same results as before", "messages in flight during the queue cutover
   are not lost"). The evaluator executes preservation criteria like any
   other — without them it has no reason to catch the regression.
7. HOSTING TOUCHPOINTS. Carry each slice's human_setup notes from PLAN.md
   into the feature file's `human_setup` section. If provisioning must exist
   BEFORE code can be developed (not just before it's tested), it should be
   its own earlier slice with a single `[env-gated]` criterion — if PLAN.md
   missed this, surface it.
8. Every feature file uses this exact format:

```markdown
---
status: pending
depends_on: []        # list of feature numbers, e.g. [1, 3]
---

# Feature NNN: <name>

## Goal
<2-4 sentences: what exists when this is done and why it matters to the user>

## Context
<SPEC.md sections, PLAN.md decisions (D#) this slice exercises, prior-slice
interfaces to build against, backend/ and frontend/ modules this slice
touches>

## Acceptance criteria
<numbered list. Each criterion is a TESTABLE BEHAVIOR, verifiable by executing
the app — not by reading code. "User can drag a clip and it persists after
reload" — not "implement drag-and-drop". Expand PLAN.md's criteria sketches
to full coverage (5-15 per slice). INCLUDE THE SEAM from PLAN.md as a
criterion: e.g. "submitting the login form posts to /auth/login and a
rejected credential renders the error inline" — not separate untethered
backend and frontend criteria. Where a PLAN.md decision is observable, assert
it (rule 5). Tag any criterion only verifiable in a deployed/provisioned
environment with [env-gated]. These are the contract: the evaluator fails the
slice if any non-deferred criterion fails.>

## human_setup
<ordered, specific manual steps a human must do in Render/Supabase/Vercel
before this slice can be fully tested — exact dashboard, exact setting, exact
value shape. Empty if none. Seeded from PLAN.md; the generator refines this
as it discovers steps.>

## Out of scope
<what this slice deliberately does NOT do, so the generator doesn't gold-plate>
```

9. CONSISTENCY PASS (mandatory, after writing all feature files):
   a. Plan fidelity: every row in PLAN.md's slice table has exactly one
      feature file and vice versa; depends_on matches the table; every
      decision (D#) referenced in a criterion exists in PLAN.md.
   b. Coverage: walk every "In scope" user story in SPEC.md and name the
      slice that implements it. Any story with no home is a gap — surface it.
      Any slice implementing nothing in SPEC.md is scope invention.
   c. Dependencies: no slice depends on a higher-numbered slice; every
      depends_on reference exists.
   d. Doc agreement: no slice contradicts SPEC.md, DESIGN.md, or PLAN.md.
      Surface contradictions instead of picking a side.
   Print the coverage table (user story → slice number) as proof.

10. After the consistency pass, print a one-line-per-slice summary table
   (number, name, depends_on, decisions exercised, whether it has
   human_setup) and tell the user to review, then run the harness:
   `./harness/run.sh --features-until N --mode gated|autonomous`. Mention
   that slices with human_setup will pause for them (clean-exit by default,
   or block with `--wait`).

## Authority handoff

From this point the feature files are the live record — the harness writes
status into their frontmatter and the evaluator judges against their
criteria. PLAN.md is not updated mid-run and may go stale on details; that is
expected. To extend or change the plan later, edit PLAN.md deliberately and
re-run /slice (incremental rules above) — never edit PLAN.md as a side
effect of implementation.

Do not start implementing anything.
