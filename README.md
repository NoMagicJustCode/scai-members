# scai-members — members.scai.world

Membership platform of **Second Circuit – Verein für digitale Gedankenfreiheit**
(Vienna, ZVR 1684464197).

Standalone from profiles.secondcircuit.io — no Discord, no social features.
Architecture transplanted from `second-circuit/profiles-monorepo` (GPL-3.0) as an
independent repository. English only.

**What it does (when complete):**
1. Membership applications — public form → board review → approval (§7 of the statutes)
2. Member area — secure login, documents, own-data management (GDPR)
3. File sharing — HTML/PDF uploads under slug links, public / password / members-only

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for the full plan, the
statutes-driven data model, and the phase roadmap.

---

## Current phase: 0 — skeleton

- ✅ Backend: ASP.NET Core 10 + MongoDB, JWT wired, `/api/status` health endpoint
- ✅ Frontend: React 19 + TypeScript + Vite, scai.world design tokens, landing placeholder
- ✅ Docker Compose (frontend + backend + mongo), GitHub Actions CI
- ⏭ Phase 1: member model endpoints + application submission + email verification

---

## Local development

### Backend
```bash
cd backend
dotnet run --project src/ScaiMembers.Api    # → http://localhost:5246
# Swagger UI: http://localhost:5246/swagger
```
Requires .NET 10 SDK and a local MongoDB (`mongodb://localhost:27017`), or:
```bash
docker run -d -p 27017:27017 --name mongo mongo:8
```

### Frontend
```bash
cd frontend
cp .env.example .env     # VITE_API_BASE_URL=http://localhost:5246
npm install
npm run dev              # → http://localhost:5173
```

### Everything at once (after images exist in GHCR)
```bash
cp .env.example .env     # set JWT_SECRET
docker compose pull && docker compose up -d
```

---

## First push (one-time setup)

1. Create an **empty** repository `scai-members` under your GitHub account.
2. ```bash
   git init && git add -A && git commit -m "Phase 0: repo skeleton"
   git branch -M main
   git remote add origin git@github.com:YOUR_GH_USERNAME/scai-members.git
   git push -u origin main
   ```
3. GitHub Actions builds and publishes both images to GHCR automatically
   (image names derive from the repo — nothing to configure in the workflow).
4. In repo **Settings → Secrets and variables → Actions → Variables**, set
   `VITE_API_BASE_URL` (e.g. `https://api.members.scai.world`) and re-run the
   frontend build once.
5. On the VPS: copy `compose.yml` + `.env`, replace `YOUR_GH_USERNAME` in
   `compose.yml`, then `docker compose up -d`. Point the reverse proxy:
   - `members.scai.world` → frontend (port 8081)
   - `api.members.scai.world` → backend (port 8082)

---

## Phase roadmap

| Phase | Deliverable |
|---|---|
| 0 | Repo skeleton, compose runs, CI green ✅ |
| 1 | Member model + application endpoints + email verification |
| 2 | Public frontend: landing + application form (scai.world design) |
| 3 | Admin: application review + member list → the board can admit members |
| 4 | Auth + member area (`/me`, documents) |
| 5 | File module: uploads, slug links, public/password/members |
| 6 | Payments & reminders log, GDPR export/delete, polish |

---

## Licence

GPL-3.0 (inherited from profiles-monorepo, whose code this project adapts).
