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

## Current phase: 3 — board area

- ✅ Board sign-in (`/admin/login`): email + password (Argon2id), session as JWT in
  an httpOnly cookie, CSRF header check, rate-limited; admin rights re-checked in
  the database on every request
- ✅ `/admin` — pending applications (confirmed emails only), admit / decline with
  inline confirmation, no reason field (§7(2)); applicant notified by email;
  decision logged in the audit log
- ✅ Admitting creates the Member record (consents carried over); decided
  applications are purged after 30 days (TTL), as the privacy policy states
- ✅ `/admin/members` — member list, active count and one-tenth threshold (§9(3), §11(2))
- ✅ `/admin/settings` — fees (§12(f)) and document versions
- ⏭ Phase 4: member sign-in for all members, `/me`, documents, password reset

### First board account

There is no admin yet to admit one through the UI, so the first account is
created on the server console (it asks for email, name and password):

```bash
# local
cd backend && dotnet run --project src/ScaiMembers.Api -- create-admin
# on the VPS
docker compose exec -it backend dotnet ScaiMembers.Api.dll create-admin
```

Run it again with an existing member's email to make them an admin or reset
their password.

### Phase 2 — public frontend

- ✅ `/` landing (membership classes per §6, joining steps, fees from config —
  shown as "set by the General Assembly" until entered)
- ✅ `/apply` application form (person / organisation), `/apply/confirm` double opt-in
- ✅ `/privacy` (v1.0, matches `PlatformConfig.privacyPolicyVersion`) and `/impressum`
- ✅ Design mirrors the scai-world repo (tokens, nav, page-hero, content sections, footer)
- ✅ Fonts self-hosted via `@fontsource` — no Google Fonts requests (GDPR)
- ⚠ Before going live: switch contacts to office@scai.world once it exists;
  confirm with Chris the log retention (14 days) and EU email provider stated in `/privacy`

### Phase 1 — applications API

- ✅ `POST /api/applications` — application (§7) with 18+ check, organisation
  representative, three required consents recorded with document version + timestamp
- ✅ `POST /api/applications/verify` — double opt-in; unconfirmed applications
  are deleted automatically after 48 h (TTL index)
- ✅ `GET /api/config/public` — fees and document versions (from `PlatformConfig`, §12(f))
- ✅ Email via SMTP when `Smtp__Host` is set; in Development, mail is written to the log
- ✅ Per-IP rate limits on the public endpoints

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
docker run -d -p 27017:27017 -v scai-mongo-data:/data/db --name mongo --restart unless-stopped mongo:8
```
Without SMTP settings, emails (e.g. the confirmation link) are printed to the
backend console instead of being sent.

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
| 1 | Member model + application endpoints + email verification ✅ |
| 2 | Public frontend: landing + application form (scai.world design) ✅ |
| 3 | Admin: application review + member list → the board can admit members ✅ |
| 4 | Auth + member area (`/me`, documents) |
| 5 | File module: uploads, slug links, public/password/members |
| 6 | Payments & reminders log, GDPR export/delete, polish |

---

## Licence

GPL-3.0 (inherited from profiles-monorepo, whose code this project adapts).
