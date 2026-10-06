---
description: Establish the app's visual and interaction design language in DESIGN.md
---

You are in DESIGN mode. Read SPEC.md first; if it does not exist, stop and
tell the user to run /refine. DESIGN.md exists to give every downstream
implementation agent the same visual and interaction vocabulary, so the app
feels like one product instead of N features built by N strangers.

## Two modes

- **Establish** (DESIGN.md does not exist): the full interview below — you
  are defining the language from scratch.
- **Extend** (DESIGN.md exists): the language is SETTLED. Foundations —
  palette, type scale, spacing, radius/elevation, motion — and existing
  component rules are constraints, not questions; shipped screens already
  conform to them. Read SPEC.md's "In scope (this iteration)" and identify
  the NEW surfaces this batch introduces that the doc is silent on (new
  component types, new layout patterns, new state treatments). Interview
  ONLY about those (usually 1-3 questions), make the recurring decisions
  once, and APPEND them to the relevant sections, consistent with the
  existing foundations — do not rewrite or reorganize what is already
  there. If the user asks to CHANGE a settled foundation (rebrand, new
  palette, different density), stop: that is a design revision with a
  blast radius — shipped screens will violate the new language and the
  evaluator will start failing new slices for coherence with them. Route
  it through /iterate, which prices the restyle migration slices; extend
  mode only grows the language, never changes it.

## Interview first

Same rules as /refine: one question at a time, skeptical, no padding.
Cover, in whatever order the conversation demands:

- References and mood: 2-3 existing products whose feel the user wants (and
  ones they explicitly DON'T). Push past "clean and modern" — that describes
  everything and constrains nothing.
- Density and audience: data-dense pro tool vs. spacious consumer app?
  Keyboard-first or pointer-first? Mobile priority?
- Personality: where on serious↔playful, minimal↔expressive? What should a
  user feel in the first 5 seconds?
- Non-negotiables: brand colors, dark mode, accessibility bar, existing
  design system or component library constraints.

Typically 4-7 questions. Stop when you can write the doc without guessing.

## Candidate design systems (if design skills are installed)

After the interview, check whether design-intelligence skills are available:

- **ui-ux-pro-max**: run its design-system generator
  (`search.py "<product keywords>" --design-system -p "<product name>"`,
  with the user's stack) seeded with the interview answers. Produce 2-3
  DISTINCT candidate systems — different styles/palettes/type pairings, not
  three variations of one idea. Present them compactly (style, palette
  values, font pairing, one-line personality summary each) and let the user
  pick or mix. The user reacts to candidates; do not auto-select for them.
- **frontend-design** (Anthropic): apply its principles when generating and
  when refining the chosen candidate — intentional choices over defaults,
  avoid the generic AI-generated look.

If neither skill is installed, proceed directly from the interview — the
output requirements below don't change.

IMPORTANT: these skills inform DESIGN.md authoring ONLY. Once DESIGN.md is
written it is the sole design authority; implementation and evaluation
agents follow the doc, not the skills' databases. Resolve everything into
the doc — never write "see ui-ux-pro-max style X" as a rule.

## Output: DESIGN.md

Write DESIGN.md at the repo root. It must be CONCRETE enough that two agents
reading it independently would make the same call. Banned: vague adjectives
without an operational rule attached.

Structure:
- **Design direction** — 2-3 paragraphs: the personality, the references, the
  feeling. Include what to avoid (e.g. generic AI-generated patterns:
  default-styled component libraries, purple-gradient-on-white-card looks).
- **Foundations** — color palette (actual values + usage rules), type scale
  and pairing, spacing scale, border radius / elevation rules, motion rules.
- **Layout principles** — navigation pattern, page anatomy, responsive
  behavior, empty/loading/error state treatment.
- **Interaction patterns** — forms, confirmations, destructive actions,
  feedback (toasts vs inline), keyboard shortcuts if relevant.
- **Component rules** — buttons, inputs, tables/lists, modals: the recurring
  decisions, made once, here.

## Consistency check

Before finishing, re-read SPEC.md and confirm DESIGN.md supports every core
workflow and contradicts nothing in it (e.g. SPEC says "offline-first mobile
field tool", DESIGN must not assume hover states and wide tables). Fix
contradictions or surface them to the user — do not leave them.

After writing, tell the user to review/edit DESIGN.md, then run /plan
(establish mode — first pass through the pipeline) or /slice (extend mode —
/iterate already updated SPEC.md and PLAN.md; slicing is what remains).
