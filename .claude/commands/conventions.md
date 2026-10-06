---
description: Distill the golden-template codebase into CLAUDE.md so every agent inherits its patterns
argument-hint: [optional path to scope to, e.g. backend or frontend]
---

You are in CONVENTIONS mode. This repo was seeded from (or should follow) a
golden template whose patterns are the standard. Your job is to distill those
patterns into CLAUDE.md ONCE per stack, so every downstream agent — generator
and evaluator, fresh context each time — inherits them automatically without
re-reading the template.

## Scope (monorepo-aware)

Claude Code loads CLAUDE.md hierarchically: the root file PLUS the nearest one
to the files being edited. Use that:

- If $ARGUMENTS names a subtree (e.g. `backend` or `frontend`), study ONLY
  that subtree and write `$ARGUMENTS/CLAUDE.md` with that stack's patterns.
  Run this command once per stack.
- If $ARGUMENTS is empty, decide from the repo shape: a monorepo with
  `backend/` and `frontend/` should get a THIN root CLAUDE.md (monorepo layout,
  where each stack lives, cross-cutting rules like API-contract ownership and
  shared types, and pointers to the nested files) — then tell the user to run
  `/conventions backend` and `/conventions frontend` for the stack details. A
  single-stack repo gets one root CLAUDE.md as before.

Do not duplicate stack-specific rules into the root file; the root is for what
spans stacks only. Keep each file focused on its own scope.

## What to extract (per scoped stack)

Read the codebase the way a senior engineer onboards: structure, a
representative vertical slice, tests, configuration. Extract the DECISIONS,
not a file inventory:

- **Architecture shape** — layering/slicing, project boundaries, where new
  feature code goes (with a concrete example path).
- **Patterns in force** — DI/registration style, request/response or mediator
  patterns, validation, error handling and result types, mapping, async.
- **Data access** — ORM patterns, migration workflow, transaction boundaries.
  (frontend equivalent: state management, data fetching/caching patterns.)
- **API conventions** — routing, versioning, status codes, response envelope,
  auth wiring. (frontend: routing, component structure, styling system.)
- **Testing conventions** — framework, structure, naming, unit vs integration.
- **Naming and style** — anything the linter doesn't already enforce.

## Output: CLAUDE.md

Write/update the scoped CLAUDE.md (root or `backend/`/`frontend/`). Rules:

1. Every convention gets a one-line rule plus a pointer to a canonical example
   file ("follow backend/src/Features/Orders/CreateOrder.cs"). Agents follow
   examples far more reliably than descriptions.
2. State explicitly: **template conventions override model defaults.** When the
   model "knows" a more common pattern than the one used here, the template
   wins. Deviations require flagging, not silent judgment calls.
3. Include a short DON'T list for deliberate omissions ("no AutoMapper —
   manual mapping per the example"; "no repository abstraction over EF").
4. Keep each file under ~150 lines. CLAUDE.md loads into EVERY relevant agent
   invocation; every line costs tokens forever. Distill, don't dump.

## init.sh (root scope only)

When running at root scope, also write (or update) `init.sh` at the repo root:
the ONE committed way to stand the app up. It must, idempotently: kill any
stale dev servers it previously started, start the backend and the frontend
dev servers in the background, wait for both health endpoints to respond, run
a basic smoke check (one API round-trip; one page load), and print the URLs.
Every generator and evaluator session boots the app by running `./init.sh` —
never by rediscovering start commands — so the script failing loudly and
early is a feature. If a stack does not exist yet (e.g. frontend not
scaffolded), have the script say so and boot what does exist; the scaffold
slice extends it later. Reference `./init.sh` from the root CLAUDE.md.

## Consistency note

If SPEC.md exists and its Technical direction conflicts with the template's
stack or patterns, surface the conflict — do not pick a side silently.

After writing, tell the user what to review and what to run next (the other
stack's /conventions if applicable, then /refine, or /plan if SPEC.md exists).
