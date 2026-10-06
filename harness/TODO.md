# Harness backlog

Snapshot: 2026-08-04. Known defects and unbuilt work, from a full review of
`harness/` (run.sh, hooks, settings) and the `.claude/` command pipeline.
The "Recently shipped" section records what landed in the same pass, so the
open items below read against a known baseline.

---

## Recently shipped

### Pipeline restructure
- [x] **`/plan` split into two commands** — `/plan` is now a technical
      architecture interview (2-3 real options per decision, argued from
      SPEC scope + template stack, pushback in both directions), producing
      ADR-style D-records in PLAN.md. `/slice` mechanically expands the
      approved plan into `features/NNN-*.md`.
- [x] **Decisions become binding** — `/implement` treats PLAN.md decisions as
      binding architecture; `/evaluate` checks architecture conformance in the
      diff (contradicting a D-record = **major** finding = automatic fail).
- [x] **Decisions become criteria** — `/slice` bakes observable decisions into
      acceptance criteria with D# references, so the plan stays enforced after
      fresh-context agents take over.

### Iteration support (existing apps)
- [x] **`/iterate`** — brownfield entry point: mechanical triage (done /
      in-flight / parked / escalated / not started), interview (new goals +
      decision pressure with migrate-vs-conform-vs-scope premise challenge +
      design surface check), blast radius per feature state, doc updates,
      hand-off to `/design`+`/slice`+`run.sh --restart`.
- [x] **`/compact`** — shrinks SPEC/PLAN history using mechanical doneness
      signals only; never touches the live contract or `features/`; leaves the
      edit uncommitted for git-diff review; mandatory safety check greps
      unfinished features for D# references.
- [x] **`/design` extend mode** — with an existing DESIGN.md, foundations are
      settled constraints; interviews only about new surfaces; changing a
      foundation routes through `/iterate` (blast radius: restyle migration
      slices).
- [x] **`/refine` iteration-shaped SPEC** — "Current state (as built)" +
      "In scope (this iteration)"; stops and points at `/iterate` if SPEC.md
      exists; documents an existing codebase on first run.
- [x] **`/plan` defers to `/iterate`** once any feature has shipped.
- [x] **Preservation criteria** — `/slice` rule 6: modification and migration
      slices must name the existing behavior that must not change.

### Docs
- [x] **`harness/ARCHITECTURE.md`** — three-phase diagrams (first build,
      autonomous loop, iteration), command reference, generator-vs-evaluator
      table, single-writer ownership map, cross-cutting controls.
- [x] **README** — workflow section updated for `/plan`+`/slice`, new
      "Iterating on an existing app" section, ARCHITECTURE link.

---

## Open — review findings

### High
- [ ] **Evaluator guardrails are read from the branch being evaluated.** The
      generator can edit `eval-settings.json`, `evaluator-system.md`, the
      hooks, or `ci.yml` and commit them on its branch; nothing watches for it,
      and autonomous mode merges without human review.
      *Fix: tripwire in `post_generator_checks` failing the round if the branch
      diff touches `harness/`, `.claude/`, or `.github/`.*
- [ ] **Committed settings drift.** `.claude/settings.json` has accumulated
      one-off approvals that break stated invariants: `Bash(xargs cat)`
      (defeats the `.env` read-deny), `Bash(gh auth *)` (allows `gh auth
      token` → token in logs), machine-specific `C:/Users/…` paths.
      `.claude/settings.local.json` is committed and allows `head`/`tail`/
      `grep` (three more `.env` bypasses).
      *Fix: restore settings.json from settings.example.json, move personal
      approvals to an untracked settings.local.json, gitignore it.*
- [ ] **`merge_when_green` check-registration race.** Only refuses at *zero*
      checks — a fast first workflow can green-light a merge before a second
      registers. *Fix: require named checks, or two consecutive all-green polls.*
- [ ] **Unhandled `gh pr create` failure → infinite hang.** `wait_for_merge`
      polls state `NONE` forever with default `MERGE_TIMEOUT_SECS=0`.
- [ ] **Acceptance criteria are writable by both agents.** `verify-gate.sh`
      allows *any* Write/Edit under `features/` — only the prompt stops the
      evaluator from softening the contract it is grading against. The
      generator runs with `--permission-mode acceptEdits` and can edit them
      too. Same class as the guardrail item above, different target, and the
      `post_generator_checks` test-deletion tripwire doesn't cover it.
      *Fix: fail the round if `git diff BASE...HEAD -- features/NNN-*.md`
      changes anything beyond the `status:` line.*

### Medium
- [ ] **Non-Windows fallback TERMs the driver's own process group** on *every*
      exit (`cleanup()` → `hard_kill_agents` → `kill -TERM "-$MAIN_PGID"`).
      The harness is effectively Windows/MSYS-only until this is a scoped
      child-kill.
- [ ] **STOP ignored while waiting on CI/merge** — neither `wait_for_merge` nor
      `merge_when_green` checks the STOP file, so a run parked on a PR can only
      be hard-killed.
- [ ] **Evaluator retries reuse prior evidence** — the evidence log resets
      before `run_claude`, but retries happen inside it, so attempt 2's gate can
      be satisfied by attempt 1's crashed session.
- [ ] **`fm()`/`set_status` aren't frontmatter-scoped** — the awk toggles on
      every `---`, so a markdown horizontal rule in the body re-opens
      "frontmatter".
- [ ] **curl localhost pin bypassable** via URL userinfo
      (`curl http://localhost:5173@evil.com/` prefix-matches the allowlist).
- [ ] **`kill.sh` sweep kills all other claude CLI sessions**, contradicting
      run.sh's careful PGID scoping.
- [ ] **The verdict never reaches the PR body.** `run.sh:745-747` passes
      `--fill` *and* `--body-file features/NNN.verdict.json`; `--fill` derives
      title and body from commits, so it either conflicts with or overwrites
      `--body-file`, and `2>/dev/null` plus the `--fill`-only fallback hides
      which. This silently breaks the documented promise that deferred
      `[env-gated]` criteria are "surfaced in the PR for you to confirm after
      deploy." *Fix: `--title` + `--body-file`, and render the verdict as
      markdown rather than dumping raw JSON.*
- [ ] **Playwright MCP is unpinned.** `mcp.headless.json:6` uses
      `@playwright/mcp@latest` — every run resolves whatever is newest. Skills
      are pinned by SHA with a documented diff-on-update workflow (SOURCES.md);
      the one dependency that can break an unattended 3am run floats.
- [ ] **The evidence gate can block a legitimate `fail`.** A denied Bash call
      fires no `PostToolUse`, so an evaluator facing a broken `init.sh` may
      never reach the 3-event threshold — and then cannot write the fail
      verdict reporting that `init.sh` is broken, which `evaluate.md` step 2
      explicitly wants as a **major** finding. Burns the turn budget, yields a
      missing verdict, reads as a generic fail. *Fix: count attempted
      execution, or exempt verdicts whose summary reports a boot failure.*

### Minor
- [ ] `steer.sh` note can be consumed by any concurrent session, including an
      interactive one
- [ ] `escalate()` swallows a failed status-flip commit (`|| true`)
- [ ] Overlapping `--features` selections (e.g. `2,2-4`) enqueue duplicates
- [ ] Remote `feat/NNN` branches never deleted after an autonomous merge
- [ ] `Backend/`/`Frontend/` vs docs' `backend/`/`frontend/` case mismatch
- [ ] Evidence-gate regexes loose (`man curl` counts as execution evidence);
      threshold is 3 events / 1 exec
- [ ] `fm()` on a missing feature file passes an empty filename to awk — a
      `depends_on` pointing at a number with no file yields an awk error
      instead of "dependency 007 has no feature file"
- [ ] `--features-until` silently wins when `--features` is also passed
      (run.sh:71-72); no guard, no warning
- [ ] `kill.sh` is Windows-only (`taskkill`/`netstat -ano`/`ps -W`) and neither
      README says so — off Windows it just removes the lock. It also frees only
      5173, while run.sh now reaps `DEV_PORTS="5000 5173"`; the two should share
      one list
- [ ] README's gitignore list (README.md:42-50) omits `harness/.lock/`, which
      the real `.gitignore` does have

---

## Open — flow gaps

- [ ] **"Flag it in your summary" is a black hole.** The generator's one
      sanctioned dissent channel lands in a transcript log nobody reads — not
      parsed by the driver, not seen by the evaluator or the next round, not in
      the PR. *Fix: durable flags file the evaluator must weigh in on.*
- [ ] **Browser-verified criteria have no regression protection.** Criteria
      proven only through Playwright are never re-checked after their slice
      merges; later slices re-run only the automated suite. (Preservation
      criteria cover *modification* slices only.) *Fix: codify UI criteria as
      committed Playwright specs, or have the evaluator re-probe `depends_on`
      seams.*
- [ ] **Deferred `[env-gated]` criteria vanish** — recorded in verdicts that get
      deleted on merge; no accumulating rollup for post-deploy confirmation.
- [ ] **No preflight for overnight runs** — nothing verifies `init.sh` boots,
      `ci.yml` triggers on PRs to the integration branch, `gh` is authed, or the
      Playwright MCP resolves. Each fails mid-run after burning sessions.
      *Fix: a `--preflight` mode that runs those checks in ~2 minutes.*
- [ ] **Contract docs read at HEAD, not pinned per feature** — editing PLAN.md
      mid-rounds means round 3 is judged against a different contract than round
      1 built. (`/iterate` handles the batch-boundary case; the mid-rounds edit
      case is open.) *Fix: record doc commit hash at feature start, warn on
      resume mismatch.*
- [ ] **Abandoned escalated features block dependents forever** — no sanctioned
      drop/replace path outside hand-editing feature files.

---

## Open — requested work

Operator backlog, 2026-08-04. Feature work rather than defects.

- [ ] **Local Docker can't restore Omni.BuildingBlocks.** `Backend/nuget.config`
      authenticates the private `Assiance` GitHub Packages feed with
      `%GITHUB_TOKEN%`, but `Backend/Dockerfile:12` runs `dotnet restore` inside
      the build stage with no `ARG`/`ENV` for it, and `docker-compose.yml`'s
      `build:` block passes no `args`. CI works only because ci.yml:47 sets the
      env var on its restore step. *Fix: BuildKit secret mount
      (`RUN --mount=type=secret,id=github_token`) + `build.secrets` in compose —
      a plain `ARG`/`ENV` bakes the token into build-stage layer history. Human
      sets `GITHUB_TOKEN` once in their shell; `init.sh` passes it through, so
      agents still never read or hold it.*
- [ ] **No path to continue development after a rejected PR.** `wait_for_merge`
      recognizes only MERGED and CLOSED — request changes on a PR and the driver
      polls forever (default `MERGE_TIMEOUT_SECS=0`); close it and the driver
      exits 1. Human review comments have no route back into the loop, and
      `--restart` refuses to touch a branch under an open PR. *Fix: poll for
      `CHANGES_REQUESTED` reviews and new review comments, convert them into
      `features/NNN.verdict.json` findings tagged `source: "human_review"`, set
      status `failed`, and re-enter the implement→evaluate rounds — `/implement`
      step 2 already treats an existing verdict as a retry, so no prompt change
      is needed. Push updates the same PR. Cap with `HUMAN_REVIEW_ROUNDS`. Add a
      `harness/REVIEW.md` local channel for typing changes instead of
      commenting on GitHub.*
- [ ] **Test policy: unit + acceptance, not integration.** Already inconsistent
      in-repo — ci.yml:56-58 deliberately excludes the Testcontainers
      integration tests as local-only, while `/implement` step 6 tells the
      generator to write tests "at the unit/integration level" and `/evaluate`
      step 3 says run the **FULL** suite, which drags them back in (and depends
      on the Docker item above). *Fix: coordinated edits to implement.md step 6,
      evaluate.md step 3, and the `/conventions` output so the policy lands in
      CLAUDE.md — unit tests for logic, acceptance tests mapped 1:1 to criteria
      against the running app, integration tests only on request. Optional
      tripwire on new files under `*.Integration.Tests/`.*
- [ ] **`--restart` should confirm before destroying work.** It sets
      `fresh=true` → `git checkout -B "$branch" "$BASE_BRANCH"`, discarding every
      commit on `feat/NNN` with no prompt. *Fix: show `git log --oneline
      BASE..branch` + diffstat, require an explicit `y`, add
      `--yes`/`HARNESS_ASSUME_YES=1` for scripted use, and refuse rather than
      assume when stdin is not a TTY.*
- [ ] **`/refine` interview is too shallow.** Still capped at "typically 4-8
      questions. Do not pad" with a four-bullet topic list — thin for a document
      that cascades into every downstream feature. (`/plan` was deepened in the
      restructure above; `/refine` and `/design` were not.) *Fix: replace the
      count cap with a coverage checklist it must exhaust — primary workflow,
      data model + lifecycle, auth/multi-user, empty/error/offline states,
      failure modes, non-goals, quality bar, integration points — plus an
      unknowns ledger driven to zero before SPEC.md is written.*
- [ ] **No UI for evaluation or implementation results.** The harness produces
      `features/*.md`, `features/NNN.verdict.json`, and
      `harness/logs/<run>/*.jsonl`, and the only way to read any of it is a text
      editor. Wanted: per-feature eval results (criteria table with the
      evaluator's evidence line, findings by severity, deferred criteria) and
      implementation results (per generator round: final summary, tool-call
      counts, test results, duration and cost — all already in the `result`
      events). *Interim fix that fits the existing bash+jq shape: a
      `harness/report.sh` emitting one self-contained HTML file per run, no build
      step, no server. Overlaps the VS Code extension below — decide whether the
      report is a stopgap or the extension absorbs it.*
- [ ] **API UI for click-through testing** ("Postman xml"). Shape undecided —
      see open questions. Candidates: generate a Postman collection from the
      backend's OpenAPI document at build time, or wire Swagger UI into
      `init.sh` so the evaluator and the human share one API surface.

## Open questions

- [ ] **"(Don't worry about scope)"** — appeared on its own line in the operator
      backlog. Reads as a note on the Docker item (fix the auth, don't redesign
      the build), but could be a directive for `/refine`'s scope challenge or
      `/implement`'s "Scope discipline" rule. Needs a decision before either is
      touched.
- [ ] **"Postman xml"** — Postman collections are JSON (v2.1). Unclear whether
      this means the OpenAPI/Swagger doc the .NET API already exposes, an older
      export format, or something else.
- [ ] **Backlog lines 1-6 were cut off** in the source screenshot; items above
      "local docker doesn't have access to omni building blocks" are unrecorded.

## Decided, not built

- [ ] **VS Code extension** — chosen over a standalone web UI because it can run
      the interactive commands (`/iterate`, `/plan`) in a terminal and host doc
      review at every gate. Agreed design: read state from files + git, invoke
      `run.sh`/`kill.sh` verbatim, never reimplement harness logic (a second
      implementation of status/merge rules would drift); keep the core as plain
      data/action modules so the same webview can be served standalone later for
      overnight phone access. Build as its own repo — an extension's shape
      (TypeScript, esbuild, vsce) doesn't fit this template's pipeline.

## Deliberately dropped (YAGNI)

- Doc size budgets + `wc -l` tripwire in preflight
- Targeted per-D# reads in `/implement` and `/evaluate`
- Per-iteration document sets (`SPEC-v2.md`, milestone folders)
