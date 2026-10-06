# members.scai.world — Architecture & Build Plan

**Project:** Membership platform for Second Circuit – Verein für digitale Gedankenfreiheit (ZVR 1684464197)
**Status:** v1.1 — decisions resolved, Phase 0 built
**Date:** 2026-10-05

---

## 1. What we are building

A standalone membership platform for the NGO, separate from profiles.secondcircuit.io (no Discord, no social features). It reuses the proven architecture of `second-circuit/profiles-monorepo` as a **new repository with transplanted modules** — not a fork.

Three jobs:

1. **Membership applications** — public form → board review → approval/rejection (§7 of the statutes)
2. **Member area** — secure login, access to documents, own data management
3. **File sharing** — upload HTML/PDF, slug links (`scai.world/f/...`), public or password-protected (Chris's feature request)

Everything in **English only**. Design follows scai.world: light warm palette, steel blue + warm gold, Cormorant Garamond.

---

## 2. Repository structure

Repo: `scai-members`, in Ann's personal GitHub account (separate from the scai.world site repo)

```
scai-members/
├── backend/                  # ASP.NET Core 10 (transplanted + adapted)
│   └── src/ScaiMembers.Api/
│       ├── Auth/             # NEW: email+password / magic link (replaces Discord OAuth)
│       ├── Applications/     # ADAPTED from profiles: application forms + review
│       ├── Members/          # ADAPTED from profiles: user model → member model
│       ├── Files/            # NEW: upload, slugs, password protection
│       ├── Admin/            # ADAPTED: admin panel API
│       └── Audit/            # NEW: payment reminders log, consent records
├── frontend/                 # React 19 + TypeScript + Vite (new, scai.world design)
├── compose.yml               # Docker: frontend (nginx) + backend + MongoDB
├── .github/workflows/        # CI from profiles-monorepo, adapted image names
└── docs/
```

**Transplanted from profiles-monorepo:** MongoDB layer, application-form + admin-review logic, admin panel skeleton, JWT/session infrastructure, Docker + GitHub Actions CI.
**Removed:** Discord OAuth, thoughts, social graph, feed, Academy sync.
**New:** email auth, file module, member lifecycle (statutes-driven), audit log.

---

## 3. Data model (driven by the statutes)

### Member

| Field | Source | Notes |
|---|---|---|
| `type` | §7(1) | `person` \| `organisation` |
| `fullName` / `orgName` + `representative` | §20(2) | |
| `dateOfBirth` | §20(2), §7(1) | persons only; validates 18+ |
| `email` | §11(3) | **legally significant** — GV invitations go here; member can update it themselves |
| `postalAddress` | §20(2) | contact data |
| `membershipClass` | §6 | `ordinary` \| `supporting` \| `honorary` |
| `status` | §7, §8 | `applied` → `active` → `resigned` \| `excluded` \| `ended` |
| `joinedAt`, `endedAt` | | |
| `consents[]` | GDPR | each: `{type, statuteVersion/policyVersion, timestamp}` |

Nothing beyond §20(2). No fields "just in case".

### Application
`memberDraft` + `motivation` (short, optional) + required checkboxes:
- ☑ I share the values and goals of the association (§7(1) — verbatim requirement)
- ☑ I have read the statutes (link to PDF)
- ☑ I consent to processing of my data per the privacy policy (GDPR Art. 6(1)(b))

Status: `pending` → `approved` / `rejected` (no reason required — §7(2): board may refuse without stating grounds). Decision recorded with board member + timestamp.

### Payment & Reminder log (minimal, §8(3))
- `payments[]`: `{year, amount, receivedAt, method}` — manually entered by admin (bank transfer reference), no payment processor in v1
- `reminders[]`: `{sentAt, channel}` — required evidence for the two-written-reminders rule before exclusion

### SharedFile (Chris's module)
`{slug, originalName, mimeType (pdf|html), visibility: public|password|members, passwordHash?, uploadedBy, uploadedAt, downloadCount}`
- Files stored on disk (Docker volume) in v1; S3-compatible later if needed
- `members` visibility = requires login — this is where login + files meet beautifully

### Config (admin-editable, not hardcoded — §12(f))
`joiningFee`, `annualFee.ordinary`, `annualFee.supporting`, statute PDF version, privacy policy version.

---

## 4. Pages (frontend)

**Public:**
- `/` — short landing: what membership means, classes, fees, link to statutes PDF
- `/apply` — application form (person/organisation toggle)
- `/apply/confirm` — email double-opt-in landing
- `/f/{slug}` — file access (direct if public; password prompt if protected; login redirect if members-only)
- `/privacy`, `/imprint` — GDPR + Austrian Impressum (required)

**Member area (login):**
- `/login` — email + password, rate-limited; password reset via email
- `/me` — own data, **edit email** (legally significant), download own data (GDPR Art. 20), resign membership (§8(2) — written notice, 1 month; the button generates the written notice with timestamp)
- `/documents` — statutes PDF (always — §9(2)), GV invitations, members-only files

**Admin (board):**
- `/admin/applications` — pending list, approve/reject
- `/admin/members` — list, member count (⅒ threshold matters — §9(3), §11(2)), payment entry, reminder sending, exclude/end membership, **delete member + data** (GDPR Art. 17)
- `/admin/files` — upload, slug, visibility, password
- `/admin/settings` — fees, document versions

---

## 5. Security & GDPR (concrete)

- **Auth:** Argon2id password hashing; httpOnly + Secure + SameSite cookies; short-lived JWT + refresh; rate limiting on login & apply; email verification before any account is active
- **Transport:** HTTPS only (reverse proxy, HSTS)
- **Data location:** everything on the existing Hetzner infrastructure → EU, no third-country transfers
- **Minimisation:** schema above is the whole schema
- **Consent:** versioned, timestamped, stored per member
- **Rights:** self-service export (Art. 20), admin hard-delete (Art. 17), rectification via `/me` (Art. 16)
- **Retention:** on membership end — personal data deleted after legal retention (accounting records 7 years under BAO → payment records kept, profile data deleted); documented in privacy policy
- **No trackers.** No Google Analytics. Nothing.
- **Backups:** MongoDB dump, encrypted, EU-stored; file volume included
- **Email sending:** via ann@scai.world infrastructure (SPF/DKIM already set up) — transactional only: verification, application result, GV invitations, payment reminders

---

## 6. Build order (iterative, your style)

| Phase | Deliverable | You can see/test |
|---|---|---|
| 0 | Repo skeleton, Docker compose runs, CI green | `docker compose up` shows hello-world |
| 1 | Backend: member model + application endpoints + email verification | API testable |
| 2 | Frontend: landing + application form, scai.world design | the public face, ready to show Chris |
| 3 | Admin: applications review + member list | board can actually admit members 🎉 |
| 4 | Auth + member area (`/me`, documents) | members can log in |
| 5 | File module (Chris's request) + `/f/{slug}` proxy route on scai.world | Chris gets his feature |
| 6 | Payments/reminders log, GDPR export/delete, polish | legally complete |

Each phase ends with your approval before the next. Phases 2–3 already make the NGO operational; 5 can be pulled earlier if Chris is impatient.

---

## 7. Decisions (resolved 2026-10-05)

1. **Subdomain:** `members.scai.world` ✅
2. **Deploy:** Chris's VPS, next to the existing sites (Beyond stays personal) ✅
3. **Login:** password-only in v1; magic link only if members ask later ✅
4. **Repo:** separate repo `scai-members` in Ann's personal account ✅
