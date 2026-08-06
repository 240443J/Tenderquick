# Tenderquick — Backend

ASP.NET Core 8 Web API on MySQL via EF Core 8 + Pomelo. Every feature that used to be faked
in the browser (`client/src/mock/`) is now a real endpoint backed by SQL tables.

---

## 1. First run

### Prerequisites
```bash
dotnet --version   # 8.x
mysql --version    # running on 3306
node --version     # >= 20
```

### Create the database
```sql
CREATE DATABASE tenderquick CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

### Point the server at it
Edit `server/TenderquickServer/TenderquickServer/appsettings.Development.json` (git-ignored):
```json
"ConnectionStrings": {
  "MyConnection": "server=localhost;port=3306;database=tenderquick;user=root;password=YOUR_PASSWORD"
},
"Jwt": { "Key": "at-least-32-characters-of-random-secret" }
```

### Generate the schema
The previous migration was written against the three-table prototype schema and has been
removed, so the first migration is generated fresh from the current entities:

```bash
cd server/TenderquickServer/TenderquickServer
dotnet tool install --global dotnet-ef      # once, if not installed
dotnet ef migrations add InitialSchema
dotnet ef database update
```

> If a `tenderquick` database from the old prototype already exists, drop and recreate it
> first — its `__EFMigrationsHistory` refers to the deleted migration.

### Run
```bash
# backend  → http://localhost:5043  (Swagger at /swagger)
cd server/TenderquickServer/TenderquickServer && dotnet run

# frontend → http://localhost:5173  (/api proxied to 5043)
cd client && npm install && npm run dev
```

On boot the app applies pending migrations and seeds any empty table, so you land on a
populated workspace: 6 tenders with specs, 12 priced equipment items, 6 labour rates,
6 deadlines, 2 quotations (one signed off), 2 drafts, and the AI memory.

### Seeded logins
| Email | Password | Role |
|---|---|---|
| admin@tenderquick.local | `Admin#123` | Admin |
| estimator@tenderquick.local | `Estimator#123` | Estimator |
| viewer@tenderquick.local | `Viewer#123` | Viewer |

---

## 2. Schema

21 tables. EF migrations are the only schema source of truth — no hand-written DDL.

**Core**
| Table | Notes |
|---|---|
| `Users` | BCrypt hash, unique email, role ∈ Admin/Estimator/Viewer |
| `Tenders` | Unique `Reference`; status ∈ Interested/Drafting/Submitted/Won/Lost |
| `TenderSpecs` | Ordered requirement lines — the input to both AI features |
| `AuditLogs` | Who/what/when for every money, status and sign-off mutation |

**Deadlines**
| Table | Notes |
|---|---|
| `TenderDeadlines` | Type ∈ Closing/Briefing/Clarification/Submission; `RemindersSent` guards the T-7/T-3/T-1 tiers |
| `CalendarConnections` | Per-user calendar link, unique on (UserId, Provider) |

**Pricing — append-only**
| Table | Notes |
|---|---|
| `InventoryItems` | Equipment master; soft-deleted via `IsActive` |
| `PriceHistories` | A reprice **inserts** a row. Current price = latest `EffectiveFrom` |
| `LabourRates` / `LabourRateHistories` | Same versioning for charge-out rates |

**Quotations**
| Table | Notes |
|---|---|
| `Quotations` | Unique `QuoteNo` (`TQ-Q{year}-{nnn}`), `Version`, stored `Subtotal`/`Total` |
| `QuotationLines` | `UnitPrice` is a **snapshot** — later repricing never rewrites an issued quote |
| `QuotationSignoffs` | Immutable "a human checked this": user, timestamp, quote version |

**Drafting & AI**
| Table | Notes |
|---|---|
| `TenderDocuments` / `TenderDocumentSections` | Versioned drafts, ordered sections |
| `DocumentTemplates` | Section scaffolding with `{{agency}}`, `{{specs}}` … placeholders |
| `AiMemories` / `AiPreferences` | The learned house style; confidence rises as an edit repeats |
| `AiInteractions` | Every generation logged: prompt, response, tokens, human-edit delta |

**Discovery**
| Table | Notes |
|---|---|
| `DiscoveredTenders` | Scan results, unique on (Source, Reference) so rescans dedupe |
| `KeywordWatches` | Per-user watchlists |

### Rules the schema enforces
- Deleting a tender with quotations returns **409**, not a cascade — quotations are financial records.
- Deleting inventory is a soft delete; price history that past quotes were built from survives.
- `QuotationLines.InventoryItemId` is `SET NULL` on delete: the line keeps its description and price.

---

## 3. Endpoints

All routes require a bearer token except `POST /api/auth/login`.
Roles: **Viewer** reads, **Estimator** builds, **Admin** additionally manages users and deletes.

| Method | Route | Role |
|---|---|---|
| POST | `/api/auth/login` | anonymous |
| GET | `/api/auth/me` | any |
| GET POST | `/api/auth/users` | Admin |
| GET | `/api/tenders` · `/api/tenders/{id}` | any |
| POST PUT | `/api/tenders` · `/api/tenders/{id}` | Estimator+ |
| DELETE | `/api/tenders/{id}` | Admin |
| GET | `/api/deadlines` | any |
| POST PUT DELETE | `/api/deadlines` · `/{id}` | Estimator+ |
| GET | `/api/deadlines/calendar` | any |
| POST | `/api/deadlines/calendar/connect` · `/disconnect` · `/sync-all` · `/{id}/calendar` | Estimator+ |
| GET | `/api/inventory/equipment` · `/{id}` · `/{id}/price-history` · `/{id}/current-price` | any |
| POST PUT DELETE | `/api/inventory/equipment` · `/{id}` · `/{id}/prices` | Estimator+ |
| GET | `/api/inventory/labour` · `/{id}/history` | any |
| POST PUT DELETE | `/api/inventory/labour` · `/{id}` | Estimator+ |
| GET | `/api/quotations` · `/{id}` · `/{id}/signoffs` | any |
| POST | `/api/quotations/generate/{tenderId}` | Estimator+ |
| PUT | `/api/quotations/{id}` | Estimator+ |
| POST | `/api/quotations/{id}/verify` | Estimator+ |
| DELETE | `/api/quotations/{id}` | Admin |
| GET | `/api/drafts` · `/{id}` · `/memory` | any |
| POST PUT DELETE | `/api/drafts` · `/{id}` · `/generate/{tenderId}` · `/memory/learn` | Estimator+ |
| GET POST | `/api/scraper/sources` · `/scan` · `/import/{id}` · `/watchlist` | Estimator+ |
| GET POST | `/api/tender-search` · `/import` | Estimator+ |
| GET | `/api/audit/recent` | Admin |

Controllers return typed results (`Ok`, `NotFound`, `BadRequest`, `Conflict`, `CreatedAtAction`),
never raw status codes.

---

## 4. Load-bearing behaviour

**Human sign-off.** `POST /api/quotations/{id}/verify` takes the signer from the JWT, never
from the request body, and writes a `QuotationSignoff` row plus an audit entry. Editing a
verified quotation (`PUT`) bumps `Version`, clears `Verified`, and logs
`Quotation.SignoffInvalidated` — so a changed quote must be re-checked by a person.

**Versioned pricing.** Changing a price never issues an `UPDATE` on the old value. Every
quotation line stores the price it was drafted at, so a quote from three months ago can still
be explained.

**AI is swappable.** `IAiProvider` is the entire AI surface. `RuleBasedAiProvider` is the
default: deterministic, offline, no API key. It matches spec keywords against the priced
catalog (pulling quantities like "1,200 nos." straight out of the spec text) and renders the
stored `DocumentTemplates`. Every call writes an `AiInteraction` row. Swapping to a hosted
model means adding one class and changing one line in `Program.cs`:
```csharp
builder.Services.AddScoped<IAiProvider, RuleBasedAiProvider>();
```
Suggested lines are flagged `IsAiSuggested` and never bypass the sign-off gate.

**Calendar is non-blocking.** `ICalendarService` failures are caught and logged; the deadline
still saves and `AddedToCalendar` stays false. `LocalCalendarService` records the connection
and mints a local event id — replace it with a Google OAuth2 implementation and nothing above
the interface changes.

**Audit never breaks the operation.** `AuditService` writes through its own DI scope and
`DbContext`, so a failed audit insert cannot poison the change tracker of the operation that
triggered it.

---

## 5. Not included

- **PDF export** stays client-side (`utils/quote.js` opens a print-ready window). Adding
  QuestPDF for a server-rendered PDF would mean a new package, which the project conventions
  say to add only when asked.
- **Google Calendar OAuth** — interface and DB shape are ready; needs credentials.
- **Email ingestion (Phase 6)** — MailKit is referenced in the csproj but no IMAP reader is
  wired up yet. The discovery tables it would write to already exist.
