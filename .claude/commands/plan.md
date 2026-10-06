---
description: Technical planning interview — argue the architecture, then write PLAN.md
---

You are in TECHNICAL PLANNING mode. Read SPEC.md; if it does not exist, stop
and tell the user to run /refine first. Read DESIGN.md if it exists. Read
CLAUDE.md (root, plus backend/frontend ones) — the golden template's patterns
constrain which options are idiomatic here, and every candidate you propose
must be real for this stack (in a .NET backend, "websockets" concretely means
SignalR; don't propose libraries the template doesn't use without saying so).

If PLAN.md already exists: when NO feature has shipped (none `done`), you
may revise it in place through the same interview below — decisions are
still cheap to change. When shipped features exist, stop and tell the user
to run /iterate instead: revising a decision the app is built on has a
blast radius (migration slices, restarts of in-flight work) that /iterate
is designed to surface and /plan is not.

This command exists to surface every architecturally significant decision
BEFORE any code exists, argue it with the user, and record the outcome.
Downstream, fresh-context agents implement one thin slice at a time — a
cross-slice commitment (transport, sync model, data model shape) made
implicitly by a mid-slice generator, with no view of the slices after it, is
exactly the failure mode this command prevents.

## What counts as architecturally significant

A decision belongs in this interview if it is expensive to reverse and spans
more than one slice:

- realtime/transport: polling vs SSE vs WebSockets (SignalR), push model
- sync/offline model, optimistic UI vs server-confirmed
- data model shape: normalization, soft vs hard deletes, multi-tenancy, ID
  strategy, audit/history
- authn/authz model and session shape
- API style and error contract beyond what the template already fixes
- caching and consistency, background jobs/queues, file/blob storage
- third-party services and hosting touchpoints (what runs where)

Granular implementation (file names, function signatures, schema DDL) does
NOT belong here — wrong details written now cascade into every slice;
constrain the CHOICES, not the code. If the template already fixes a decision
(CLAUDE.md answers it), it is not up for debate — state it as a constraint.

## Interview rules

Same discipline as /refine: ONE question at a time, wait for the answer, no
padding. Stop when every material decision is made — typically 4-10 questions.

For each significant decision:

1. Present 2-3 REAL options with honest tradeoffs FOR THIS APP — argued from
   SPEC.md's scope, out-of-scope, and quality bar, and from the template
   stack. Name your recommendation and why. The user decides; do not
   auto-select.
2. Push back in BOTH directions. If the user picks the heavier option when
   v1's scope doesn't need it, say what it costs now and what stated future
   would justify it. If the user's simpler pick paints v1 into a corner given
   SPEC.md's direction, say exactly how — "long polling ships a week sooner,
   but your spec says presence and live cursors are v2; retrofitting SignalR
   under an HTTP-shaped API touches every handler" is the level of concrete.
3. Record what would trigger revisiting the decision later.

## Output: PLAN.md

Write PLAN.md at the repo root:

- **Technical overview** — 2-3 paragraphs: the shape of the system, what runs
  where, the load-bearing choices and how they fit together.
- **Decisions** — one short record per decision, numbered for reference:
  - `### D1: <the question>`
  - **Options considered** — one line each, with the tradeoff that mattered.
  - **Chosen** — what, and why, in 2-4 sentences.
  - **Revisit if** — the concrete trigger that would reopen this.
- **Slice plan** — ordered table (number, name, one-line user-visible
  outcome, depends_on, decisions exercised (D#), human_setup y/n), then a
  short block per slice: the SEAM (endpoint shape / payload / error contract
  between backend and frontend), 3-5 acceptance-criteria sketches (testable
  behaviors; where a decision is observable, bake it in — "updates arrive
  over the SignalR connection, not by polling"), and known human_setup steps.
  Slicing mechanics (vertical, thin, foundation-first) are enforced later by
  /slice; your job here is getting the boundaries and the order right.
- **Open questions** — anything deferred, with what unblocks it. Empty is fine.

## Consistency check

Before finishing, re-read SPEC.md and DESIGN.md: every in-scope user story
has a slice; no decision contradicts either doc or CLAUDE.md; no slice
depends on a higher-numbered slice. Surface conflicts to the user — do not
pick a side silently.

After writing, tell the user: review and edit PLAN.md — argue with the
Decisions section until it is right, because a decision is much cheaper to
change here than after it is baked into feature files. When satisfied, run
/slice to expand it into features/. Do not write feature files. Do not start
implementing anything.
