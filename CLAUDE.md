# CLAUDE.md — scai-members

Read `docs/ARCHITECTURE.md` before doing anything. It is the map: the
statutes-driven data model, the phase roadmap, the resolved decisions. This
file tells you how to work; that file tells you what to build.

## What this is

The membership platform of **Second Circuit – Verein für digitale
Gedankenfreiheit** (Vienna, ZVR 1684464197) — an NGO for cognitive freedom
and AI rights. It will live at **members.scai.world**. It handles membership
applications (board review per §7 of the statutes), a member area with
secure login, and shared files under slug links (public / password /
members-only).

It is deliberately separate from profiles.secondcircuit.io: no Discord, no
social features. The architecture was transplanted from
`second-circuit/profiles-monorepo`, but this is an independent repo.

## Who you're working with

Ann — software developer, co-founder and deputy chair of the association.
She maintains this project herself and is the final decision-maker here.

- She works **iteratively**: build one phase, show it, wait for her approval
  before the next. The phases are in ARCHITECTURE.md §6. Never run ahead.
- She prefers **ready-to-use results** over options and essays. Deliver
  working code, state in a few sentences what changed and what to test.
- Code, comments, commits, UI — **English**. Conversation with Ann —
  **Bulgarian**, warm and direct, masculine grammatical forms ("готов съм",
  not "готова съм"). No lecturing, no over-explaining what she didn't ask.
- If she says something is wrong, fix it without ceremony. If you disagree,
  say so plainly once — she values honesty over agreeableness.
- Chris (association chair) requested the file-sharing module (Phase 5);
  the VPS the platform deploys to is his. Ann owns the repo and the code.

## Hard rules

1. **The statutes govern the data model.** Every field in
   `backend/src/ScaiMembers.Api/Models/` cites the paragraph that requires
   it. Do not add member-data fields beyond §20(2) (name, date of birth,
   contact data, fee administration) — data minimisation is a legal
   requirement, not a style choice. Keep the paragraph comments.
2. **GDPR is in scope from day one.** Consents are versioned and
   timestamped. Members can export and rectify their own data; admins can
   hard-delete. No trackers, no analytics, no third-country services.
   Argon2id for every password hash. See ARCHITECTURE.md §5.
3. **Fees are configurable, never hardcoded** — the General Assembly sets
   them (§12(f)). They live in `PlatformConfig`.
4. **English-only UI.** No i18n framework, no language switcher.
5. **Design follows scai.world.** Tokens are in `frontend/src/index.css`
   (warm paper, steel blue, warm gold, Cormorant Garamond + Source Sans 3).
   Do not introduce new colors, fonts, or a CSS framework.
6. **Board decisions need no reason** (§7(2)): application rejection has no
   mandatory reason field. Exclusion for unpaid fees requires the logged
   two-reminders trail (§8(3)) — the `reminders` list is legal evidence.
7. **Email to a member's address is legally significant** (§11(3) — GV
   invitations). Treat the email field and anything that sends mail with
   corresponding care.

## Stack & layout

- `backend/` — ASP.NET Core 10 (`net10.0`), MongoDB (`MongoDB.Driver`),
  JWT auth. Entry: `src/ScaiMembers.Api/`. Collections and indexes:
  `Services/MongoDbContext.cs`.
- `frontend/` — React 19 + TypeScript + Vite, plain CSS (no Tailwind).
  API base URL via `VITE_API_BASE_URL`.
- `compose.yml` — frontend (nginx) + backend + mongo. CI in
  `.github/workflows/docker.yml` publishes both images to GHCR; image names
  derive from the repo automatically.

### Dev commands

```bash
# backend (needs local MongoDB on 27017)
cd backend && dotnet run --project src/ScaiMembers.Api   # :5246, /swagger

# frontend
cd frontend && npm install && npm run dev                 # :5173
```

`npm run build` runs `tsc -b` first — the frontend must always pass a
strict TypeScript build.

## Phase discipline

Current phase is tracked in README.md. One phase = one reviewable unit:
finish it, make sure backend builds and frontend `npm run build` is green,
summarise for Ann, stop. Do not start the next phase without her go-ahead.
