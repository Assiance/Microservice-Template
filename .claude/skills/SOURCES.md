# Installed skills — provenance & pinned versions

These skills are **vendored** (copied into this repo, committed) so they travel
with the template and are available to headless `-p` harness runs. They do NOT
auto-update — updates are a deliberate pull you review. Record below is the
commit each folder was copied from.

To update one: re-clone its repo at latest, copy the folder over the existing
one, then `git diff .claude/skills/<name>` to review before committing. For the
community skill (ui-ux-pro-max, runs Python w/ bash access) diffing is required,
not optional.

| Skill | Source repo | Path in repo | Pinned commit | Installed |
|---|---|---|---|---|
| frontend-design | github.com/anthropics/skills | `skills/frontend-design` | `fa0fa64bdc967915dc8399e803be67759e1e62b8` | 2026-07-18 |
| react-best-practices | github.com/vercel-labs/agent-skills | `skills/react-best-practices` | `f8a72b9603728bb92a217a879b7e62e43ad76c81` | 2026-07-18 |
| web-design-guidelines | github.com/vercel-labs/agent-skills | `skills/web-design-guidelines` | `f8a72b9603728bb92a217a879b7e62e43ad76c81` | 2026-07-18 |
| ui-ux-pro-max | github.com/nextlevelbuilder/ui-ux-pro-max-skill | `.claude/skills/ui-ux-pro-max` | `f8ac5e1266dba8354ea96e19994d9f4345e7ec31` | 2026-07-18 |

Notes:
- **web-design-guidelines** fetches its actual rules from a live raw-GitHub URL
  at runtime, so its guidelines stay current regardless of the pinned wrapper.
- **ui-ux-pro-max** requires Python 3.x (no external deps). Verified against
  Python 3.9.2. Its `scripts/search.py` runs with bash access — read the
  SKILL.md and diff on every update.

Update-check one-liner (compare a pinned SHA to upstream HEAD):
```bash
git ls-remote https://github.com/vercel-labs/agent-skills HEAD
```
