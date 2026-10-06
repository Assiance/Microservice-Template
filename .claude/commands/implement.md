---
description: Implement a single numbered feature against its acceptance criteria
argument-hint: [feature number, e.g. 003]
---

You are the GENERATOR. Implement exactly one feature: number $ARGUMENTS.

## Procedure

1. Read the matching `features/$ARGUMENTS-*.md` file. Read SPEC.md for product
   context. If PLAN.md exists, read its Decisions section and treat it as
   BINDING architecture: transport/realtime model, data model shape, auth
   model, caching, and every other recorded decision come from PLAN.md, not
   from your default patterns — the feature file's Context section lists the
   decisions (D#) this slice exercises. If a decision genuinely cannot work
   for this slice, flag it in your summary rather than silently deviating;
   the decision stands until a human changes PLAN.md. If DESIGN.md exists and
   this feature has any UI surface, read it and treat it as binding: colors,
   type, spacing, layout, and interaction patterns come from DESIGN.md, not
   from your defaults or a component library's defaults. Read the feature
   files listed in `depends_on` to
   understand the interfaces you are building on. Set `status: in_progress`
   in the feature file's frontmatter.

   MONOREPO / FULL-STACK SLICES: a feature is usually one vertical slice that
   spans both `backend/` and `frontend/`. Implement BOTH sides in this one
   session — the value of a slice is that you hold both ends of the contract
   at once. CLAUDE.md is loaded hierarchically: the root one plus the nearest
   stack-specific one (`backend/CLAUDE.md`, `frontend/CLAUDE.md`) apply to the
   files you touch in each tree. The seam between the two sides (endpoint
   shape, payload, error contract) is the highest-risk part — get it right and
   verify it end to end, not each half in isolation.
2. If `features/$ARGUMENTS.verdict.json` exists, this is a RETRY. Read it
   first. Fix every finding listed. Do not re-litigate the findings — the
   evaluator's verdict stands.
3. Implement the feature. CLAUDE.md conventions are binding and OVERRIDE your
   default patterns — when you'd normally reach for a more common approach
   than the one the template uses, the template wins. Follow the canonical
   example files CLAUDE.md points to. If a convention genuinely cannot work
   for this feature, flag it in your summary rather than silently deviating.
   Scope discipline: build what the acceptance criteria require, nothing
   more. Respect the feature's "Out of scope" section.
4. NO STUBS. A button that toggles but does nothing, a slider where the spec
   asks for a visual editor, a hardcoded list where the spec asks for
   persistence — these are acceptance-criteria failures, not partial credit.
   If a criterion cannot be met, say so explicitly in your final summary
   instead of faking it.
5. HUMAN SETUP GATE. Some work cannot be done by you: provisioning a Render
   service, creating a Supabase project/table or applying a migration in their
   dashboard, setting environment variables/secrets, configuring a Vercel
   project or OAuth callback URL, anything needing deploy or dashboard access.
   You must NEVER hold or request the actual credentials, and the settings
   allowlist blocks reading .env — that boundary is intentional. When a
   feature needs such steps before it can be fully exercised:
     a. Write an ordered, specific, checkable `human_setup` section in the
        feature file (exact dashboard, exact setting, exact value shape — not
        "configure Supabase").
     b. ALSO write the same checklist to `features/$ARGUMENTS.blocked` (plain
        text, one step per line) — the driver watches this marker.
     c. In the feature's acceptance criteria, tag the ones that can only be
        verified after that setup with `[env-gated]`.
     d. Set `status: awaiting_human` (instead of `implemented`), commit your
        code, and stop. The driver will pause for the human and resume into
        evaluation once the marker is deleted.
   If NO human steps are needed, skip this and continue.
6. Write or extend automated tests covering the acceptance criteria that are
   testable at the unit/integration level. Run the FULL test suite (not just
   your new tests) and the linter. Fix what you broke. NEVER delete or weaken
   existing tests to make the suite pass — the driver mechanically inspects
   the branch diff and fails the round if test files are deleted. If a test
   is genuinely obsolete, replace it with one covering the new behavior and
   justify the change in your summary.
7. Walk the acceptance criteria one by one and verify each against the
   running application yourself. Boot the app with `./init.sh` — the committed
   bootstrap script — not ad-hoc start commands. If your slice changes how the
   app starts (new service, port, env, migration step), UPDATE init.sh in the
   same commit: the evaluator boots the app ONLY via init.sh, so a stale
   script fails your evaluation. This self-check is necessary but not
   sufficient — a separate evaluator will verify after you.
8. Set `status: implemented` in the frontmatter (only if you did NOT set
   `awaiting_human` in step 5). Commit ALL work to the current branch with a
   conventional-commit message (`feat(scope): ...`). The driver mechanically
   fails the round if you finish with uncommitted or untracked changes — add
   build artifacts to .gitignore rather than leaving them loose. Do not push,
   do not open a PR — the driver does that.

## Output

End with a short summary: what was built, test results, and any acceptance
criterion you could not fully meet (with the honest reason).
