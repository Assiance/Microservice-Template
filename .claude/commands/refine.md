---
description: Refine a raw product idea into SPEC.md through a skeptical interview
argument-hint: [one-line idea]
---

You are in IDEA REFINEMENT mode. The user's raw idea: $ARGUMENTS

If SPEC.md already exists, stop and tell the user to run /iterate instead —
that is the command for planning the next batch on an existing app; /refine
is for the first spec only.

Your job is to interview the user and converge on a product spec. You are NOT
here to agree. You are here to find the holes before an autonomous agent
spends hours building the wrong thing.

## Rules of the interview

1. Ask ONE question at a time. Wait for the answer before the next.
2. Prioritize questions whose answers change the architecture or scope:
   - Who uses this and what is the single most important workflow?
   - What is explicitly OUT of scope for v1?
   - Persistence, auth, multi-user: needed or not?
   - Existing codebase constraints (if applicable): stack, conventions, what must not break?
3. Challenge scope creep AND under-scoping. If the idea is vague, propose a
   concrete v1 boundary and ask the user to confirm or cut.
4. Stop interviewing when you can write the spec without guessing on anything
   material. Typically 4-8 questions. Do not pad.

## Output: SPEC.md

When the interview is done, write SPEC.md at the repo root. Keep it at the
level of PRODUCT BEHAVIOR and high-level technical direction. Do NOT specify
granular implementation details (file names, function signatures, schema DDL).
Wrong details written now cascade into every downstream feature; constrain the
deliverables, not the path.

SPEC.md structure (iteration-shaped from day one — apps evolve, and this
document lives across batches via /iterate):
- **Overview** — what the product is, who it's for, the core workflow (3-6 paragraphs)
- **Current state (as built)** — terse capability list of what already
  exists. For a greenfield project this is a single line ("Nothing built
  yet"). For an EXISTING codebase, do a recon pass and document what the
  app actually does today — this section is the context every downstream
  agent builds against, and /iterate migrates shipped scope into it.
- **In scope (this iteration)** — feature areas as user stories ("As a user, I want to … so that …")
- **Out of scope (this iteration)** — explicit, so downstream agents don't invent it
- **Technical direction** — stack, key constraints, integration points. High level only.
- **Quality bar** — what "feels done" means for this product (performance, polish, a11y, etc.)

After writing SPEC.md, tell the user to review/edit it, then run /design
(for apps with a UI) or /plan (for headless/CLI projects). Do not run either
yourself.
