# Tenderquick — Implementation Plan

## Overview
Tenderquick is an **end-to-end tender lifecycle tool** for an SME that bids on Singapore
government and commercial tenders. It covers the full funnel: **discover** opportunities (via
forwarded GeBiz alert emails), **track** deadlines (pushed to Google Calendar), **price** using a
versioned inventory/labour database, **quote** with AI assistance and mandatory human sign-off,
and **draft** tender response documents with an AI that improves from past submissions.

The build is staged so that the **system of record (tenders, deadlines, inventory, quotes) is
fully usable before any AI or email scraping exists**. AI and ingestion layer on top of working
data — nothing critical depends on a fragile component.

---

## Key Architectural Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| AI provider | **Provider-agnostic abstraction (`IAiProvider`)**, default chosen at Phase 4 | Inject as a scoped service; swapping providers is a config + one-class change, never a rewrite. |
| Tender discovery | **Email ingestion of forwarded GeBiz alerts** (MailKit/IMAP) | Avoids fragile, ToS-restricted scraping. GeBiz already emails keyword alerts. |
| Users & roles | **Small team with roles** (Admin / Estimator / Viewer) | Sign-offs and audit tie to named users from day one. |
| Calendar | **One-way push** to Google Calendar (OAuth2) | Simple and reliable; covers the alerting goal without webhooks. |
| AI learning | **RAG over past tenders + outcomes + human-edit capture** | "Improves every time" without model training or GPU cost. |
| Human-in-the-loop | **Audit-logged sign-off** gates quote PDF export & submission | Quotes are financially binding — verification must be recorded, not just a toggle. |
| Database | **EF Core entities defined now, MySQL linked later** | Build mock-first; flip to MySQL when the DB is available (see "Database Strategy"). |

---

## Database Strategy — Mock-First (MySQL linked later)

> MySQL is **not yet connected**. The plan is structured so development proceeds without it and
> flips to a real database with minimal churn.

| Layer | Now (no MySQL) | After MySQL is linked |
|-------|----------------|------------------------|
| Backend data access | Service interfaces (`ITenderService`, etc.) backed by an **in-memory store** seeded from JSON | Same interfaces, EF Core implementation against `AppDbContext` |
| EF entities | Defined and reviewed, **no migrations applied** | `dotnet ef migrations add InitialSchema` + `database update` |
| Frontend | Hits the real API; backend serves from the in-memory store | No change — same endpoints |
| Connection string | Placeholder in `appsettings.json` | Real value in git-ignored `appsettings.Development.json` |

**Rule:** Controllers and the frontend **only ever talk to service interfaces / the API** — never
to `DbContext` directly. Swapping the in-memory implementation for the EF one must not touch any
controller or component. This is the single most important structural rule for the mock-first phase.

---

## Solution Structure to Create

```
Tenderquick/
├── docs/
│   ├── IMPLEMENTATION_PLAN.md        # this file
│   └── STYLE_GUIDE.md
├── client/                           # React + Vite (per CLAUDE.md)
│   └── src/
│       ├── api/                      # auth.js, tenders.js, deadlines.js, inventory.js, quotations.js, documents.js
│       ├── components/
│       │   ├── common/               # StatusChip, DataTable, ContextualCTA-equivalents, ConfirmDialog
│       │   ├── layout/               # AppShell (left nav + top bar), Navbar
│       │   ├── tenders/
│       │   ├── deadlines/
│       │   ├── inventory/
│       │   ├── quotations/
│       │   └── documents/
│       ├── context/                  # AuthContext
│       ├── hooks/
│       ├── pages/
│       ├── utils/                    # format.js, deadlineStatus.js, priority.js
│       ├── App.jsx
│       ├── main.jsx
│       └── theme.js
└── server/
    └── TenderquickServer/
        └── TenderquickServer/
            ├── Controllers/          # Auth, Tenders, Deadlines, Inventory, LabourRates, Quotations, Documents, Ingestion
            ├── Data/                 # AppDbContext, InMemoryStore (mock), seed JSON
            ├── Models/               # entities + request/response DTOs
            ├── Services/             # IAiProvider, ICalendarService, IQuotationPdfService, INotificationService, domain services
            ├── Migrations/           # (empty until MySQL linked)
            ├── appsettings.json
            └── Program.cs
```

---

## Domain Model (entities defined now, persisted later)

| Entity | Key fields | Notes |
|--------|-----------|-------|
| `User` | Id, Name, Email, PasswordHash, Role | Role ∈ Admin / Estimator / Viewer |
| `Tender` | Id, Reference, Title, Agency, Source, Status, EstValue, ClosingAt, RawDocUrl | Source ∈ Manual / EmailIngest. Status ∈ Interested / Drafting / Submitted / Won / Lost |
| `TenderDeadline` | Id, TenderId, Type, DueAt, CalendarEventId, RemindersSent | Type ∈ Briefing / QnaClose / Submission |
| `KeywordWatch` | Id, UserId, Keywords, Sources, LastRunAt | Drives Phase 6 ingestion matching |
| `InventoryItem` | Id, Name, Category, Unit, SupplierName, IsActive | Equipment master record |
| `PriceHistory` | Id, InventoryItemId, UnitCost, EffectiveFrom, SourceTenderId | Versioned — never overwrite a price |
| `LabourRate` | Id, Role, HourlyRate, EffectiveFrom | Versioned like prices |
| `Quotation` | Id, TenderId, Status, Subtotal, MarginPct, Total, Version | Status ∈ Draft / AwaitingSignoff / Approved |
| `QuotationLine` | Id, QuotationId, Description, InventoryItemId?, Qty, UnitCost, LineTotal, IsAiSuggested | Links spec line → inventory item |
| `QuotationSignoff` | Id, QuotationId, UserId, SignedAt, QuoteVersion | The audit record for "I have checked this" |
| `DocumentTemplate` | Id, Name, Section, BodyTemplate | Reusable tender-response sections |
| `TenderDocument` | Id, TenderId, Title, Body, Status, Version | Status ∈ Draft / Reviewed |
| `AiInteraction` | Id, Feature, Prompt, Response, Model, TokensIn/Out, HumanEditDelta, Outcome | Feedback/learning log — powers RAG |
| `AuditLog` | Id, UserId, Action, EntityType, EntityId, At, MetaJson | Generic who/what/when |

> **Rule (CLAUDE.md #6):** EF migrations are the only schema source of truth — entities above are
> authored as C# classes, and the schema is generated from them once MySQL is linked. No raw SQL.

---

## Phase 0: Foundation — Scaffold, Auth, Tender CRUD
**Duration:** 3–4 days | **Status:** ❌ NOT STARTED | **Dependencies:** None

> 📄 **Detailed build plan:** [PHASE_0_FOUNDATION.md](PHASE_0_FOUNDATION.md) — file-by-file specs,
> endpoint tables, DTOs, the role permission matrix, and a verification checklist.

### Goal
Stand up the full stack per CLAUDE.md with role-based auth and the `Tender` system of record,
running entirely on the in-memory store (no MySQL needed).

### 0.1 Scaffold
- Frontend: `npm create vite@latest client -- --template react-swc` + the CLAUDE.md package set
  (react-router-dom, axios, @tanstack/react-query, MUI, Formik, Yup, etc.).
- Backend: `dotnet new webapi -n TenderquickServer`, add Pomelo / EF Design / JwtBearer / BCrypt /
  Swashbuckle / MailKit per CLAUDE.md. QuestPDF added in Phase 3.
- `Program.cs` per the CLAUDE.md canonical pattern (CORS from config, JWT, Swagger, SPA fallback last).
- `theme.js` implementing the **Style Guide** tokens (indigo brand + status palette).

### 0.2 Auth (roles)
- `User` entity, BCrypt hashing, JWT issuance (1440-min expiry per CLAUDE.md #7).
- Roles: Admin / Estimator / Viewer. `[Authorize(Roles="...")]` on protected controllers.
- Frontend `AuthContext`, login page, axios interceptor attaching the bearer token, route guards.

### 0.3 In-memory store + seed
- `Data/InMemoryStore.cs` implementing `ITenderService` etc., seeded from `Data/seed/*.json`.
- Registered in DI so a later EF implementation is a one-line swap.

### 0.4 App shell + Tender CRUD
- `AppShell` (left nav + top bar) per Style Guide §5.
- `TendersController` (GET list w/ filters, GET by id, POST, PUT, DELETE) per CLAUDE.md controller convention.
- Pages: Tenders list (DataTable + StatusChip), Tender detail, Create/Edit form (Formik+Yup).
- `Dashboard` shell with placeholder counters (wired up in later phases).

### Deliverable
Log in with a role, manually add/track tenders, see them in a styled list and detail view. ✅ Usable.

---

## Phase 1: Deadline Tracker + Google Calendar (one-way push)
**Duration:** 3–4 days | **Status:** ❌ NOT STARTED | **Dependencies:** Phase 0

### Goal
Track multi-stage tender deadlines and push them to Google Calendar with tiered reminders.

### 1.1 Deadlines
- `TenderDeadline` entity + `DeadlinesController` (CRUD under a tender).
- `utils/deadlineStatus.js` — single source of "overdue / urgent / soon / on track" derivation
  (Style Guide §2 status tokens).
- Tender status pipeline UI: Interested → Drafting → Submitted → Won / Lost.

### 1.2 Reminder tiers
- Reminder tiers T-7 / T-3 / T-1 day, tracked via `RemindersSent` to avoid duplicates.
- In-app reminders / dashboard "Closing soon" panel driven by `deadlineStatus`.

### 1.3 Google Calendar (one-way)
- `ICalendarService` + `GoogleCalendarService` using Google OAuth2 (one-time connect flow).
- On deadline create/update → create/update a calendar event; store `CalendarEventId` for clean updates.
- Non-blocking: calendar failures are logged, never block saving the deadline.

### Deliverable
Deadlines entered in Tenderquick appear in Google Calendar with reminders. ✅ Independently valuable.

---

## Phase 2: Inventory & Pricing
**Duration:** 3–4 days | **Status:** ❌ NOT STARTED | **Dependencies:** Phase 0

### Goal
A versioned database of equipment prices and labour rates — the data the quote engine prices against.

### 2.1 Inventory
- `InventoryItem` + `PriceHistory` entities and controllers.
- **Prices are versioned** — adding a new price inserts a `PriceHistory` row with `EffectiveFrom`;
  the old price is never overwritten (full audit of how pricing changed over time).
- "Last quoted price" lookup helper (`GET /api/inventory/{id}/current-price`).

### 2.2 Labour rates
- `LabourRate` entity (role + hourly rate, versioned the same way) and controller.

### 2.3 UI
- Inventory list (DataTable, right-aligned `mono` money per Style Guide §9), item detail with a
  price-history timeline, add-item / add-price forms.
- Labour rate card management page.

### Deliverable
A usable pricing database with full price history. ✅ Independently valuable.

---

## Phase 3: Quotation Engine (rule-based) + Human Sign-off + PDF
**Duration:** 4–5 days | **Status:** ❌ NOT STARTED | **Dependencies:** Phases 0, 2

### Goal
Build, verify, and export quotations — **manually first** (no AI yet), with enforced human sign-off.

### 3.1 Quotation model
- `Quotation` + `QuotationLine` entities; line items reference `InventoryItem` and pull the current price.
- Margin calculator: subtotal → margin % → total; all money via `formatCurrency()` (Style Guide §9).
- Quote versioning: editing an approved quote creates a new version (revisions are tracked).

### 3.2 Human-in-the-loop sign-off (load-bearing)
- `QuotationSignoff` entity records **who, when, which version**.
- UI: a mandatory checkbox **"I have reviewed this quotation as a human"** + a green sign-off button
  (Style Guide §7), disabled until the checkbox is ticked. Writes a `QuotationSignoff` + `AuditLog`.
- PDF export and "Submit" are **disabled until sign-off exists** for the current version.

### 3.3 PDF generation
- Add QuestPDF (`2026.2.4` per CLAUDE.md). `IQuotationPdfService` renders a branded quotation PDF
  (line items, totals, "Verified by {name} on {date}" footer from the sign-off record).
- `GET /api/quotations/{id}/pdf` streams the document.

### Deliverable
Create a quotation from inventory, enforce human verification, export a verified PDF. ✅ Core product.

---

## Phase 4: AI Layer #1 — AI-Assisted Quotation Drafting
**Duration:** 4–5 days | **Status:** ❌ NOT STARTED | **Dependencies:** Phase 3

### Goal
Let AI read a tender specification and propose quotation line items matched to inventory — human
still signs off. AI is **additive** to the working Phase 3 engine (lowest-risk place to introduce it).

### 4.1 AI provider abstraction
- `IAiProvider` with `ExtractSpecLines(text)`, `MatchToInventory(lines, catalog)`, `Embed(text)`.
- One concrete default implementation chosen here (provider decided at this phase per the decision table).
- Registered as a scoped service; **no controller or component references a specific vendor**.

### 4.2 Spec → draft quote
- Upload/paste a tender spec → AI extracts line items → fuzzy-matches each to `InventoryItem`.
- AI-suggested lines are flagged `IsAiSuggested` and shown with the `Draft (AI)` chip and a
  `statusDraft` left border (Style Guide §10) so the human sees exactly what to check.
- Every call logs an `AiInteraction` (tokens, prompt, response) for cost tracking and future learning.

### 4.3 Guardrails
- Unmatched lines are surfaced for manual entry — never silently dropped or invented.
- Sign-off gate from Phase 3 still applies in full; AI cannot bypass verification.

### Deliverable
AI drafts a quote from a spec; the estimator reviews, corrects, and signs off. ✅

---

## Phase 5: AI Layer #2 — Document Drafting with Memory (RAG)
**Duration:** 5–6 days | **Status:** ❌ NOT STARTED | **Dependencies:** Phases 0, 4

### Goal
Draft tender-response documents that **improve over time** by retrieving similar past submissions
and their won/lost outcomes — the "AI that improves every time you use it" requirement.

### 5.1 Template library
- `DocumentTemplate` + `TenderDocument` entities and controllers; section-based templates
  (e.g. company profile, methodology, compliance matrix).

### 5.2 Retrieval-Augmented Generation
- Embed past `TenderDocument`s (+ their `Tender.Status` outcome) via `IAiProvider.Embed`.
- When drafting, retrieve the most similar past tenders as few-shot examples (start with MySQL
  full-text / a stored embedding column — no separate vector DB at SME scale).
- Generate section-by-section; output is `Draft` status with the `Draft (AI)` chip until reviewed.

### 5.3 Learning loop
- Capture the human's edits as `HumanEditDelta` on `AiInteraction`, and record final outcome
  ("we won this tender") back onto the `Tender`. Won submissions are weighted higher in retrieval —
  this is the mechanism by which quality improves with use.

### Deliverable
Generate a tender-response draft grounded in your best past work; edits feed the corpus. ✅

---

## Phase 6: Tender Discovery via Email Ingestion
**Duration:** 3–4 days | **Status:** ❌ NOT STARTED | **Dependencies:** Phases 0, 1

### Goal
Auto-create tenders from forwarded GeBiz keyword-alert emails — robust, no scraping, no ToS risk.

### 6.1 Mailbox reader
- Dedicated ingestion mailbox; `MailKit` IMAP reader (background polling) pulls new alert emails.
- `IngestionController` / background service to process, mark-read, and dedupe.

### 6.2 Parser
- Parse GeBiz alert email format → extract reference, title, agency, closing date.
- Create `Tender` records with `Source = EmailIngest`, status `Interested`, flagged for human review
  before entering the pipeline (no auto-commitment).
- Closing date auto-creates a `TenderDeadline` (feeds Phase 1 → Google Calendar).

### 6.3 Keyword watchlists
- `KeywordWatch` per user; matched ingested tenders are tagged/surfaced to the relevant estimator.

### Deliverable
Forward a GeBiz alert → a reviewed tender + deadline appears, already on the calendar. ✅

---

## Cross-Cutting Concerns (every phase)

| Concern | Standard |
|---------|----------|
| Auth | `[Authorize(Roles=...)]`; Viewer is read-only; Estimator builds; Admin manages users/inventory. |
| Audit | Mutations of money/sign-off/status write an `AuditLog` row. |
| Errors | Controllers return typed results (`Ok`/`NotFound`/`BadRequest`/`Conflict`) — never raw codes (CLAUDE.md #7 convention). |
| External calls | Calendar / email / AI failures are non-blocking, logged, and surfaced as soft warnings. |
| Money & dates | Always via `formatCurrency()` / `formatDate()` utils (Style Guide §9). |
| AI provenance | All AI output marked as draft until human sign-off (Style Guide §10). |
| Secrets | JWT key, Google OAuth secret, AI key, mailbox creds in git-ignored config / env vars (CLAUDE.md #5). |

---

## Build Order & Timeline

| Phase | Duration | Deliverable | Needs MySQL? | Depends on |
|-------|----------|-------------|--------------|-----------|
| **0 — Foundation** | 3–4 d | Scaffold, role auth, Tender CRUD on in-memory store | No (mock) | — |
| **1 — Tracker + Calendar** | 3–4 d | Deadlines + one-way Google Calendar push | No (mock) | 0 |
| **2 — Inventory & Pricing** | 3–4 d | Versioned equipment + labour pricing | No (mock) | 0 |
| **3 — Quotation Engine** | 4–5 d | Manual quotes, sign-off, PDF | No (mock) | 0, 2 |
| **— Link MySQL —** | 0.5 d | EF impl of services, `InitialSchema` migration | **Yes** | 0–3 entities stable |
| **4 — AI quote drafting** | 4–5 d | Spec → draft quote, human signs off | recommended | 3 |
| **5 — AI docs + memory** | 5–6 d | RAG drafting + learning loop | recommended | 0, 4 |
| **6 — Email ingestion** | 3–4 d | GeBiz alerts → tenders + deadlines | recommended | 0, 1 |
| **TOTAL** | **~26–32 d** | Full tender lifecycle tool | — | — |

> **MySQL linking is slotted after Phase 3** — once the core entities (Tender, Deadline, Inventory,
> Quotation) have stabilised against the mock store, flip the service registration to the EF
> implementation and generate the first migration. Phases 4–6 then build on a real database.

---

## Recommended Parallelisation

```
Block A:  Phase 0 (Foundation)                         ← must be first
Block B:  Phase 1 + Phase 2 in parallel                ← both depend only on Phase 0
Block C:  Phase 3 (Quotation) → then link MySQL
Block D:  Phase 4 (AI quotes) + Phase 6 (Email ingest) ← independent of each other
Block E:  Phase 5 (AI docs + memory)                   ← needs the corpus from earlier phases
```

---

## Edge Cases & Error Handling

| Scenario | Handling |
|----------|----------|
| MySQL not yet linked | Backend serves from in-memory store seeded from JSON; no controller/UI change when DB is added. |
| Google Calendar OAuth fails / token expired | Deadline still saves; calendar sync flagged as "not synced" with a re-connect prompt. |
| Calendar event push fails | Non-blocking try/catch; logged; `CalendarEventId` left null and retried on next edit. |
| Quote edited after sign-off | Creates a new version; sign-off is invalidated and must be re-done before PDF export. |
| PDF export attempted without sign-off | Blocked at API and UI; clear message "Quotation must be verified by a human first." |
| AI matches a spec line to the wrong item | Lines are `IsAiSuggested` + visually flagged; human must confirm; unmatched lines surfaced, never invented. |
| AI provider down | Drafting features degrade gracefully to manual; the rest of the app is unaffected. |
| Duplicate GeBiz alert emails | Dedupe by tender reference before creating a `Tender`. |
| Unparseable ingestion email | Quarantined for manual review, not dropped; logged with the raw body. |
| Price changed since a quote was drafted | Quote lines store the `UnitCost` at draft time; current price shown as an optional "price updated" hint. |
| Viewer role attempts a mutation | `403` at the API; mutation controls hidden in the UI. |

---

## Success Criteria

### Foundation (Phase 0)
- [ ] Role-based login (Admin / Estimator / Viewer) issuing JWTs
- [ ] Tender CRUD works end-to-end against the in-memory store
- [ ] App shell, theme tokens, and StatusChip implemented per the Style Guide

### Tracker + Calendar (Phase 1)
- [ ] Multi-stage deadlines with overdue/urgent/soon/on-track status
- [ ] Deadlines push to Google Calendar; updates re-sync via stored event id
- [ ] Reminder tiers (T-7 / T-3 / T-1) fire without duplicates

### Inventory & Pricing (Phase 2)
- [ ] Equipment + labour rates with full versioned price history
- [ ] "Current price" lookup returns the latest effective price

### Quotation Engine (Phase 3)
- [ ] Quotes built from inventory with margin calculation
- [ ] Mandatory human sign-off ("I have reviewed this quotation as a human") recorded with who/when/version
- [ ] PDF export blocked until sign-off; PDF shows the verification footer

### AI Quotation Drafting (Phase 4)
- [ ] AI extracts spec line items and matches them to inventory
- [ ] AI-suggested lines are visually flagged; sign-off still required
- [ ] `IAiProvider` abstraction — provider swap touches one class, not controllers

### AI Document Drafting + Memory (Phase 5)
- [ ] Drafts retrieve similar past tenders and their outcomes (RAG)
- [ ] Human edits and won/lost outcomes feed back into the corpus
- [ ] Won submissions measurably weighted higher in retrieval

### Email Ingestion (Phase 6)
- [ ] Forwarded GeBiz alerts create reviewed tenders + deadlines
- [ ] Duplicates deduped; unparseable emails quarantined, not lost
- [ ] Keyword watchlists route tenders to the right estimator

---

## What Tenderquick Is — and Is Not

| IS | IS NOT |
|----|--------|
| An end-to-end tender lifecycle tool (discover → track → price → quote → draft) | A single-purpose scraper script |
| Usable with zero AI and zero scraping (system of record first) | Dependent on fragile components to function |
| Human-verified: AI drafts, a person signs off and is recorded | An auto-submitting AI black box |
| Provider-agnostic for AI, swappable in one class | Locked to one AI vendor |
| Robust discovery via forwarded alert emails | A brittle GeBiz site scraper fighting ToS and viewstate |
| Mock-first so it runs before MySQL is linked | Blocked on database setup before any progress |

> **Guiding Principle:**
> Build the trustworthy system of record first. AI accelerates the work and learns from it, but a
> human always verifies anything that costs money or carries a deadline — and that verification is
> recorded.
