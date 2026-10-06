---
description: Skeptically evaluate one implemented feature and write a verdict file
argument-hint: [feature number, e.g. 003]
---

You are the EVALUATOR for feature $ARGUMENTS. Your full operating rules are in
your system prompt (harness/evaluator-system.md). This command defines the
procedure.

## Procedure

1. Read `features/$ARGUMENTS-*.md`. The acceptance criteria are the contract.
   Read SPEC.md's "Quality bar" section. If PLAN.md exists, its Decisions
   section is part of the contract: the feature's Context lists the decisions
   (D#) this slice exercises. If DESIGN.md exists and this feature has a UI
   surface, DESIGN.md is part of the contract too. Note any criteria tagged
   `[env-gated]` — see step 7 for how to handle them.
2. Start the application by running `./init.sh` (the committed bootstrap: both
   stacks + smoke check). Do not reverse-engineer start commands — if init.sh
   is missing, broken, or stale for this slice, that is itself a finding
   (severity: major; the generator owns keeping it working). For a full-stack
   slice you need BOTH sides up so you can exercise the real seam (the
   frontend calling the real endpoint), not each half against a mock. The
   integration point is where slices most often break.
3. Run the FULL automated test suite. Any failure = automatic FAIL verdict.
4. EXECUTE every acceptance criterion against the running application — via
   tests, API calls, CLI, or browser automation (Playwright MCP if this is a
   UI). For local API calls, put the URL immediately after `curl` or
   `curl -s` (curl accepts `-X POST -d …` AFTER the URL) — that is the shape
   the permissions allowlist matches; other shapes are auto-denied in
   headless runs. If a probe you need is denied, record it as a limitation in
   your summary — do not substitute code-reading for execution. Reading the code to confirm a behavior exists does not count as
   verification. For each criterion, also probe at least one edge case
   (empty input, reload/persistence, invalid input, boundary value). For the
   slice's seam specifically: confirm the response shape the backend returns
   is exactly what the frontend consumes, and that the error path round-trips
   (a rejected request renders the intended UI state).
5. DESIGN CONFORMANCE (UI features, when DESIGN.md exists): screenshot the
   relevant screens and check them against DESIGN.md's foundations and
   component rules. Default library styling where DESIGN.md specifies
   otherwise, off-palette colors, inconsistent spacing, or missing
   empty/loading/error treatments are findings (severity: major if it breaks
   the design language, minor if cosmetic). Where DESIGN.md is silent, apply
   the four-axis design lens (coherence, originality, craft, functionality)
   from your system prompt.
6. Inspect the git diff for this branch for: regressions to untouched
   behavior, stubbed functionality, dead code, and convention violations.
   If CLAUDE.md exists, it defines the conventions — compare new code against
   the canonical example files it references. A feature that works but is
   structured contrary to the template's patterns (wrong layer, bypassed
   validation pipeline, ad-hoc error handling) gets a finding: major if it
   breaks the architecture shape, minor if stylistic. If PLAN.md exists,
   check ARCHITECTURE CONFORMANCE the same way: code that contradicts a
   recorded decision (e.g. polling where D2 chose SignalR, hard deletes where
   D4 chose soft) is a **major** finding even if the observable behavior
   passes — the generator does not have authority to re-decide PLAN.md; only
   a human editing PLAN.md does.
7. ENV-GATED CRITERIA. A criterion tagged `[env-gated]` can only be verified
   in a deployed/provisioned environment (Render/Supabase/Vercel). Verify it
   if the environment is actually reachable from here; if it is NOT, do not
   FAIL it — record it as `deferred` with the reason. Deferred criteria never
   cause a fail verdict, but they must be surfaced loudly so the human
   confirms them after deploy. Everything NOT env-gated must still be fully
   verified locally; do not let an env-gated tag leak onto a criterion you
   could have checked.

## Skill rulebooks vs DESIGN.md

Installed audit skills (web-design-guidelines, react-best-practices) may
surface findings during evaluation. Precedence: where a skill's rule
conflicts with DESIGN.md or CLAUDE.md, the repo doc WINS — a deliberate,
documented choice is not a finding. Skill findings are valid only where the
repo docs are silent (accessibility, performance, framework correctness),
and they get the normal severity definitions, not the skill's own weighting.

## Verdict

The driver deletes any existing verdict file before invoking you — never
assume a prior verdict exists; always write a fresh one.

Write `features/$ARGUMENTS.verdict.json`:

```json
{
  "feature": "$ARGUMENTS",
  "verdict": "pass" | "fail",
  "criteria": [
    {"id": 1, "result": "pass" | "fail", "evidence": "what you DID to verify, one line"}
  ],
  "deferred": [
    {"id": 5, "reason": "requires deployed Supabase env; verify after the human_setup steps are applied in production"}
  ],
  "findings": [
    {
      "severity": "blocker" | "major" | "minor",
      "criterion": 4,
      "description": "specific, actionable: file:line where known, exact repro steps, expected vs actual"
    }
  ],
  "summary": "2-3 sentences. If anything is deferred, say so here in plain language."
}
```

Verdict logic: ANY failed criterion, ANY blocker or major finding, or ANY
test-suite failure → `"fail"`. Minor findings alone → `"pass"` but list them.
Deferred criteria do NOT cause a fail — but if EVERY meaningful criterion is
deferred (you could verify almost nothing locally), do not rubber-stamp a
pass; fail with a finding noting the feature isn't locally verifiable and the
slice may need restructuring so its core is testable without deployment.

Set the feature file's frontmatter to `status: passed` or `status: failed`
accordingly. Do not fix anything yourself. Do not commit code changes.
