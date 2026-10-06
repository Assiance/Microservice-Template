# What the harness does — full pipeline

Three phases. A **first-build phase** where you and Claude produce the contract
documents, the **autonomous phase** where `run.sh` drives a build → judge →
merge loop against those documents, and an **iteration phase** for every batch
after the first, where the docs evolve instead of fossilizing.

## Phase 1 — first build: produce the contracts

```mermaid
flowchart TD
    TPL["Golden template repo<br/>(Backend/ + Frontend/)"] --> CONV["/conventions (once per stack)<br/>distills the template's patterns"]
    CONV --> CMD["📄 CLAUDE.md ×3<br/>root + backend + frontend<br/>(+ init.sh, the one way to boot the app)"]

    IDEA["💡 raw idea"] --> REF["/refine<br/>skeptical interview,<br/>one question at a time"]
    REF --> SPEC["📄 SPEC.md<br/>Current state (as built) ·<br/>this iteration's scope ·<br/>quality bar"]
    SPEC --> DES["/design — establish mode<br/>interview + candidate design<br/>systems from design skills"]
    DES --> DMD["📄 DESIGN.md<br/>foundations, layout,<br/>interaction, component rules"]
    SPEC --> PLAN["/plan — TECHNICAL interview<br/>2-3 real options per decision,<br/>argued from scope + stack;<br/>you decide, it pushes back"]
    CMD --> PLAN
    PLAN --> PMD["📄 PLAN.md<br/>Decisions D1…Dn (ADR-style)<br/>+ slice plan with seams"]
    PMD --> SLICE["/slice — mechanical expansion<br/>(no re-planning; surfaces<br/>problems instead)"]
    DMD --> SLICE
    SLICE --> FEAT["📄 features/NNN-slug.md<br/>thin VERTICAL slices, build order,<br/>depends_on, acceptance criteria<br/>(decisions baked in as D# criteria)"]
```

The split matters: `/plan` is where you argue architecture (long polling vs
SignalR, soft vs hard deletes) *before* any code exists, because a fresh-context
generator building one thin slice would otherwise make those cross-slice
commitments implicitly. `/slice` then expands an approved plan — decisions
already made, so the expansion is a transformation, not a judgment call.

## Phase 2 — autonomous: run.sh drives the loop

```mermaid
flowchart TD
    FEAT["📄 features/NNN-*.md"] --> S

    subgraph D["run.sh driver — loops one feature at a time"]
        S["Select next feature<br/>(skip done · check depends_on)"] --> B["Cut branch feat/NNN<br/>from base"]
        B --> G["🤖 GENERATOR — Sonnet<br/>headless /implement NNN<br/>SPEC + PLAN + DESIGN + CLAUDE.md<br/>are BINDING; builds both sides,<br/>tests, self-checks, commits"]
        G --> T{"Mechanical tripwires:<br/>self-certified status?<br/>uncommitted work?<br/>deleted tests?"}
        T -- "tripped → synthetic fail,<br/>retry next round" --> G
        G -. "slice needs manual<br/>Render/Supabase setup" .-> HG["⏸ HUMAN GATE<br/>features/NNN.blocked<br/>driver exits; you do the steps,<br/>delete marker, re-run"]
        HG -.-> E
        T -- clean --> E["🔍 EVALUATOR — Opus<br/>headless /evaluate NNN<br/>locked-down permissions +<br/>evidence-gate hooks<br/>(must EXECUTE the app to verdict;<br/>checks design + architecture<br/>conformance in the diff)"]
        E --> V{"features/NNN<br/>.verdict.json"}
        V -- "fail — rounds left<br/>(max 3)" --> G
        V -- "fail — rounds exhausted" --> X["🚨 ESCALATED<br/>run stops, you intervene"]
        V -- pass --> C["Driver commits status: done<br/>pushes branch, opens PR"]
    end

    C --> M{"--mode"}
    M -- gated --> HM["PR → main<br/>👤 you review + merge each one"]
    M -- autonomous --> A["PR → harness/integration<br/>auto squash-merge only when<br/>ALL CI checks are green"]
    A --> PR2["end of run:<br/>👤 you test integration branch,<br/>promote to main as ONE PR"]
```

## Phase 3 — iteration: the docs evolve with the app

```mermaid
flowchart TD
    WANT["💡 next batch of work"] --> IT["/iterate"]

    subgraph ITC["/iterate — the brownfield entry point"]
        TRI["1 · TRIAGE (mechanical)<br/>every feature classified:<br/>done · in-flight · parked ·<br/>escalated · not started"]
        TRI --> INT["2 · INTERVIEW<br/>new goals · decision pressure<br/>(migrate vs conform vs scope) ·<br/>design surface gaps"]
        INT --> BR["3 · BLAST RADIUS<br/>per feature state, before<br/>any doc is touched"]
    end

    IT --> TRI
    BR --> APPR{"👤 you approve"}
    APPR --> UPD["4 · UPDATE DOCS<br/>SPEC: shipped scope → Current state<br/>PLAN: old decision superseded,<br/>successor D-record appended"]
    UPD --> DES2["/design — extend mode<br/>ONLY if a design gap or<br/>approved revision was flagged;<br/>foundations stay settled"]
    UPD --> SL2["/slice<br/>incremental: adds new slices,<br/>never renumbers or rewrites<br/>non-pending features"]
    DES2 --> SL2
    SL2 --> RUN["./harness/run.sh<br/>(+ --restart for in-flight<br/>features you chose to rebuild)"]
    RUN -.-> WANT

    BR -.-> RAD["blast radius per state:<br/>done → migration slice (w/ preservation criteria)<br/>not started → regenerated free<br/>in-flight → RESTART (work discarded)<br/>or DEFER — never silent<br/>parked/escalated → explicit keep/restart/drop"]

    CMP["/compact<br/>run when docs grow fat:<br/>collapses done slices + superseded<br/>decisions only; never the live<br/>contract; you review the git diff"] -.-> UPD
```

## The eight commands at a glance

| Command | Who runs it | Reads | Writes | Purpose |
|---|---|---|---|---|
| `/conventions [stack]` | you, once per stack | template codebase | `CLAUDE.md` ×3, `init.sh` | Distill golden-template patterns so every fresh-context agent inherits them |
| `/refine <idea>` | you, first build only | your answers | `SPEC.md` | Skeptical interview → product spec. Stops if SPEC.md exists (use `/iterate`) |
| `/design` | you | `SPEC.md`, design skills | `DESIGN.md` | **Establish**: define the visual language. **Extend**: append rules for new surfaces only; foundations are settled |
| `/plan` | you | `SPEC`, `DESIGN`, `CLAUDE.md` | `PLAN.md` | Technical interview → ADR-style decisions. Defers to `/iterate` once features have shipped |
| `/slice` | you | `PLAN`, `SPEC`, `DESIGN` | `features/NNN-*.md` | Faithful expansion of the approved plan; decisions become D# criteria |
| `/iterate [goal]` | you, existing app | all docs + git + `features/` | `SPEC.md`, `PLAN.md` | Triage → interview → blast radius → doc updates → hand off to `/design`+`/slice` |
| `/compact` | you, occasionally | `SPEC`, `PLAN`, `features/` | `SPEC.md`, `PLAN.md` | Collapse shipped history; never the live contract. Left uncommitted for your review |
| `/implement NNN` | **run.sh** (Sonnet) | feature file, all docs, prior verdict | code, tests, commits | Build one slice end to end; never push/PR |
| `/evaluate NNN` | **run.sh** (Opus) | feature file, all docs, diff, running app | `NNN.verdict.json` | Execute every criterion against the running app; judge, never fix |

## The two agents, deliberately unequal

| | Generator | Evaluator |
|---|---|---|
| Model | Sonnet (cheap, separate quota) | Opus (judgment is the expensive part) |
| Permissions | `acceptEdits` + bash allowlist | Write **only** under `features/`; git mutations denied |
| Job | Build the slice, test it, commit | Run the app, execute every criterion, write verdict — **never fix** |
| Trust | Assumed to cut corners → driver tripwires | Assumed to rubber-stamp → evidence-gate hooks |

## Who owns what (the single-writer rule)

```mermaid
flowchart LR
    U["👤 you + interactive<br/>commands"] --> D1["SPEC.md<br/>DESIGN.md<br/>PLAN.md"]
    SLW["/slice ONLY"] --> D2["features/NNN-*.md<br/>(content)"]
    EV["evaluator ONLY"] --> D3["NNN.verdict.json<br/>status: passed/failed"]
    DR["run.sh driver ONLY"] --> D4["status: done<br/>branches · PRs · merges"]
```

Every artifact has exactly one writer. That's what makes the tripwires
meaningful: a generator setting `status: done`, or a verdict appearing without
execution evidence, is a boundary violation the driver or a hook can catch
mechanically.

## Cross-cutting controls (any time during a run)

```mermaid
flowchart LR
    subgraph OP["Operator controls"]
        STOP["touch harness/STOP<br/>kill-switch hook denies every<br/>tool call → agent halts NOW"]
        STEER["write harness/STEER.md<br/>note injected into the running<br/>agent's next tool call (one-shot)"]
        KILL["./harness/kill.sh<br/>force-kill agent tree +<br/>dev servers, free port, drop lock"]
    end
    subgraph AUTO["Automatic guards"]
        WD["per-agent wall-clock watchdog<br/>(1h cap → kill + retry ×3)"]
        LOCK["single-instance lock<br/>harness/.lock"]
        LOGS["stream-json transcripts<br/>harness/logs/&lt;run&gt;/"]
    end
```

**Key idea throughout:** every safeguard is *mechanical* (permissions, hooks,
diff checks, CI, single-writer ownership), not a prompt asking the model to
behave. State lives in git — feature frontmatter, committed `done` flips — so
an interrupted run resumes by re-running `run.sh`, and `/iterate` can
reconstruct the whole picture months later.
