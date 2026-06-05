# Phase 0 — Foundation: Scaffold · Role Auth · Tender CRUD · Dashboard Shell · AuditLog

> Detailed build plan for Phase 0 of the [Implementation Plan](IMPLEMENTATION_PLAN.md).
> Follows [CLAUDE.md](../CLAUDE.md) conventions and the [Style Guide](STYLE_GUIDE.md).
>
> **Implemented DB-first (not mock-first).** MySQL (`tenderquick_db`) was linked earlier than the
> original plan assumed, so services are backed by **EF Core against `AppDbContext`** (e.g.
> `EfTenderService`) instead of the in-memory store. The service-interface architecture is
> unchanged — the only difference is the concrete implementation registered in DI. Seeding runs at
> startup via `DbInitializer` (migrate + seed) rather than from JSON files.

**Duration:** 3–4 days | **Status:** ✅ COMPLETE (verified end-to-end) | **Dependencies:** None

---

## Goal

Stand up the full React + ASP.NET Core stack with:
1. Working JWT login and **three roles** (Admin / Estimator / Viewer) enforced on both API and UI.
2. **Tender CRUD** end-to-end against a swappable in-memory store.
3. A styled **app shell** (left nav + top bar) and a **dashboard shell** with placeholder counters.
4. An **AuditLog** that records every mutation (who / what / when), proving the audit pattern works
   before any money or sign-off exists.

**Out of scope for Phase 0:** deadlines, calendar, inventory, quotations, AI, email ingestion,
MySQL/EF migrations. Entities for later phases are *not* created yet — only what Phase 0 needs.

---

## Sub-Phase Breakdown

| # | Sub-phase | Output | Day |
|---|-----------|--------|-----|
| 0.1 | Scaffold frontend + backend | Running dev servers, proxy wired, theme tokens | 1 |
| 0.2 | Backend foundation: store, audit, services | In-memory store, `IAuditService`, DI wiring | 1–2 |
| 0.3 | Auth + roles | `AuthController`, JWT, BCrypt, seeded users | 2 |
| 0.4 | Tender domain + CRUD API | `Tender` model, `ITenderService`, `TendersController` | 2–3 |
| 0.5 | Frontend shell + auth flow | `AppShell`, `AuthContext`, login, route guards, axios interceptor | 3 |
| 0.6 | Tender UI + Dashboard shell | List / detail / form pages, dashboard counters | 3–4 |
| 0.7 | Verification pass | Manual end-to-end run-through against success criteria | 4 |

---

## 0.1 — Scaffold

### Frontend
```bash
# from project root
npm create vite@latest client -- --template react-swc
cd client
npm install
npm install react-router-dom@^7 axios @tanstack/react-query \
  @mui/material @mui/icons-material @emotion/react @emotion/styled \
  @fontsource/inter framer-motion react-helmet-async formik yup
```

`client/vite.config.js` — proxy `/api` and `/uploads` to `http://localhost:5043` exactly as the
CLAUDE.md template specifies.

`client/src/theme.js` — implement the Style Guide tokens now (don't defer): `tokens` object with
the indigo brand palette, the status palette (`statusOverdue/urgent/soon/onTrack/draft/neutral`
+ their `*Bg` tints), typography (incl. the `mono` variant), spacing, and radii. Wrap the app in
`ThemeProvider` + `CssBaseline` and `@fontsource/inter`.

### Backend
```bash
# from project root
mkdir server/TenderquickServer
cd server/TenderquickServer
dotnet new webapi -n TenderquickServer --no-https false
cd TenderquickServer
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.2
dotnet add package BCrypt.Net-Next --version 4.0.3
dotnet add package Swashbuckle.AspNetCore --version 6.6.2
# EF + Pomelo + MailKit are added now per CLAUDE.md but EF is NOT used until MySQL is linked
dotnet add package Pomelo.EntityFrameworkCore.MySql --version 8.0.2
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.2
```

> QuestPDF / MailKit are added in their own phases (3 / 6) to keep the Phase 0 surface minimal.

`Program.cs` — the CLAUDE.md canonical pattern, with these Phase-0 specifics:
- CORS origins from `AllowedOrigins` config (never hardcoded).
- JWT bearer configured from the `Jwt` config section.
- **Register services against interfaces** (see 0.2) so the EF swap later is one line each.
- `MapControllers()` then `MapFallbackToFile("index.html")` **last**.

`appsettings.json` — placeholders only (real secrets go in git-ignored `appsettings.Development.json`):
```json
{
  "ConnectionStrings": { "MyConnection": "server=localhost;port=3306;database=tenderquick;user=<db_user>;password=<db_password>" },
  "AllowedOrigins": [ "http://localhost:5173" ],
  "Jwt": { "Key": "<32-char-random-secret>", "Issuer": "TenderquickServer", "Audience": "TenderquickClient", "ExpireMinutes": 1440 },
  "DataStore": "InMemory"
}
```
`DataStore` flag lets us flip `InMemory` → `Mysql` in DI later without code edits.

`.gitignore` — ensure `appsettings.Development.json`, `bin/`, `obj/`, `node_modules/`, `dist/` are ignored.

---

## 0.2 — Backend Foundation: In-Memory Store, Audit, DI

### Data store (mock-first)
- `Data/InMemoryStore.cs` — a singleton holding `ConcurrentDictionary` collections for `User`,
  `Tender`, `AuditLog`, seeded on construction from `Data/seed/*.json`.
- Domain services read/write the store, **never** a `DbContext`. Controllers depend only on the
  service interfaces.
- `Data/AppDbContext.cs` — authored now (DbSets for `User`, `Tender`, `AuditLog`) but **not wired**
  to a provider until MySQL is linked. It exists so entities have a single declared schema home.

### AuditLog (load-bearing pattern, introduced now)

`Models/AuditLog.cs`
| Field | Type | Notes |
|-------|------|-------|
| `Id` | `int` | |
| `UserId` | `int?` | null for system actions |
| `UserName` | `string` | denormalised for easy display |
| `Action` | `string` | e.g. `Tender.Created`, `Tender.Updated`, `Tender.Deleted`, `Auth.Login` |
| `EntityType` | `string` | e.g. `Tender` |
| `EntityId` | `int?` | |
| `At` | `DateTime` | `DateTime.UtcNow` |
| `MetaJson` | `string?` | optional JSON snapshot of changed fields |

`Services/IAuditService.cs` + `AuditService.cs`
```
Task LogAsync(string action, string entityType, int? entityId, object? meta = null);
Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 50);
```
- Resolves the current user from `IHttpContextAccessor` (claims) so callers don't pass identity.
- Called from every mutating service method. Audit failures are swallowed-and-logged — they must
  never break the primary operation.

### DI registration (in `Program.cs`)
```
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<InMemoryStore>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITenderService, InMemoryTenderService>();   // ← swap target for EF later
```

---

## 0.3 — Authentication & Roles

### Model
`Models/User.cs`
| Field | Type | Notes |
|-------|------|-------|
| `Id` | `int` | |
| `Name` | `string` | |
| `Email` | `string` | unique, login identifier |
| `PasswordHash` | `string` | BCrypt |
| `Role` | `string` | `Admin` / `Estimator` / `Viewer` |
| `CreatedAt` | `DateTime` | |

`Models/Auth/` DTOs (records):
```
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, UserDto User);
public record UserDto(int Id, string Name, string Email, string Role);
public record CreateUserRequest(string Name, string Email, string Password, string Role); // Admin only
```

### Roles constant
`Models/Roles.cs` — `public static class Roles { public const string Admin="Admin", Estimator="Estimator", Viewer="Viewer"; }`
Use these constants in `[Authorize(Roles=...)]`, never string literals scattered in controllers.

### Service
`Services/IAuthService.cs` + `AuthService.cs`
- `Task<AuthResponse?> LoginAsync(LoginRequest req)` — look up by email, `BCrypt.Verify`, issue JWT.
- `string GenerateToken(User user)` — claims: `sub` (id), `name`, `email`, `ClaimTypes.Role`.
  Expiry from `Jwt:ExpireMinutes` (1440). Signed with `Jwt:Key` (HMAC-SHA256).
- `Task<UserDto> CreateUserAsync(CreateUserRequest req)` — Admin-only path; BCrypt-hash password.

### Controller
`Controllers/AuthController.cs` — `[Route("api/auth")]`
| Method | Route | Auth | Purpose |
|--------|-------|------|---------|
| POST | `/api/auth/login` | Anonymous | Returns `AuthResponse` or `401` |
| GET | `/api/auth/me` | Any authenticated | Returns current `UserDto` from claims |
| POST | `/api/auth/users` | `Admin` | Create a user |
| GET | `/api/auth/users` | `Admin` | List users (no hashes) |

- Login writes an `Auth.Login` audit row. Never return `PasswordHash` in any payload.

### Seed users (`Data/seed/users.json`)
| Name | Email | Password (dev) | Role |
|------|-------|----------------|------|
| Admin User | admin@tenderquick.local | `Admin#123` | Admin |
| Est User | estimator@tenderquick.local | `Estimator#123` | Estimator |
| View User | viewer@tenderquick.local | `Viewer#123` | Viewer |

> Seed passwords are BCrypt-hashed at store startup. These are **dev-only** credentials — documented
> here, not real secrets.

---

## 0.4 — Tender Domain & CRUD API

### Model
`Models/Tender.cs`
| Field | Type | Notes |
|-------|------|-------|
| `Id` | `int` | |
| `Reference` | `string` | tender ref no (unique-ish, validated) |
| `Title` | `string` | |
| `Agency` | `string` | issuing agency |
| `Source` | `string` | `Manual` (Phase 0) / `EmailIngest` (Phase 6) |
| `Status` | `string` | `Interested` / `Drafting` / `Submitted` / `Won` / `Lost` |
| `EstValue` | `decimal?` | estimated contract value (SGD) |
| `ClosingAt` | `DateTime?` | submission close (deadlines proper come in Phase 1) |
| `Notes` | `string?` | |
| `CreatedAt` / `UpdatedAt` | `DateTime` | |

`Models/Tenders/` DTOs:
```
public record CreateTenderRequest(string Reference, string Title, string Agency, decimal? EstValue, DateTime? ClosingAt, string? Notes);
public record UpdateTenderRequest(string Title, string Agency, string Status, decimal? EstValue, DateTime? ClosingAt, string? Notes);
public record TenderListItem(int Id, string Reference, string Title, string Agency, string Status, decimal? EstValue, DateTime? ClosingAt);
```
Status enum-like validation lives in a `TenderStatus` constants class; reject unknown values with `400`.

### Service
`Services/ITenderService.cs` + `Data/InMemoryTenderService.cs`
```
Task<IEnumerable<TenderListItem>> GetAllAsync(string? status, string? search);
Task<Tender?> GetByIdAsync(int id);
Task<Tender> CreateAsync(CreateTenderRequest req);
Task<Tender?> UpdateAsync(int id, UpdateTenderRequest req);
Task<bool> DeleteAsync(int id);
```
- Create/Update/Delete each call `IAuditService.LogAsync(...)`.
- `GetAllAsync` supports filtering by `status` and a free-text `search` over reference/title/agency.

### Controller
`Controllers/TendersController.cs` — `[ApiController]` `[Route("api/tenders")]` `[Authorize]`
| Method | Route | Roles | Returns |
|--------|-------|-------|---------|
| GET | `/api/tenders?status=&search=` | Any authenticated (incl. Viewer) | `Ok(IEnumerable<TenderListItem>)` |
| GET | `/api/tenders/{id}` | Any authenticated | `Ok(Tender)` / `NotFound` |
| POST | `/api/tenders` | `Admin`, `Estimator` | `CreatedAtAction(...)` / `BadRequest` / `Conflict` (dup ref) |
| PUT | `/api/tenders/{id}` | `Admin`, `Estimator` | `Ok(Tender)` / `NotFound` / `BadRequest` |
| DELETE | `/api/tenders/{id}` | `Admin` | `NoContent` / `NotFound` |

- Viewer can read but every mutating route excludes Viewer (returns `403`).
- Return typed results — never raw ints (CLAUDE.md controller convention).

### Seed tenders (`Data/seed/tenders.json`)
5–8 realistic Singapore-flavoured sample tenders across the status pipeline (e.g. an NEA cleaning
contract `Interested`, an HDB lighting upgrade `Drafting`, a won SportSG AV install `Won`) so the
list, filters, and dashboard counters have data immediately.

---

## 0.5 — Frontend Shell, Auth Flow, Routing

### API layer (`client/src/api/`)
- `axios.js` — base instance + request interceptor attaching `Bearer` token from `localStorage`;
  response interceptor that on `401` clears the token and redirects to `/login`.
- `auth.js` — `login`, `me`, `listUsers`, `createUser`.
- `tenders.js` — `getAll`, `getById`, `create`, `update`, `remove` (thin Axios per CLAUDE.md §9).

### Auth context & guards (`client/src/context/AuthContext.jsx`)
- Holds `{ user, token, login(), logout() }`; persists token in `localStorage`; hydrates `user` via
  `/api/auth/me` on load.
- `components/common/ProtectedRoute.jsx` — redirects unauthenticated users to `/login`.
- `components/common/RoleGate.jsx` — renders children only if the user's role is allowed
  (used to hide create/edit/delete controls from Viewer).

### App shell (`client/src/components/layout/`)
- `AppShell.jsx` — fixed left nav (240px, collapsible) + top bar (64px) per Style Guide §5.
- `SideNav.jsx` — nav items: Dashboard, Tenders. (Inventory, Quotations, Documents added later as
  disabled/"coming soon" entries to establish the IA.)
- `TopBar.jsx` — app title, current user + role chip, logout.

### Common components (`client/src/components/common/`)
- `StatusChip.jsx` — renders tender status via the status palette (Style Guide §8).
- `DataTable.jsx` — standardised table wrapper (sticky header, `offWhite` header, hover row).
- `ConfirmDialog.jsx` — reused for delete confirmation.
- `EmptyState.jsx` — mandatory empty states (Style Guide §9).

### Utils (`client/src/utils/`)
- `format.js` — `formatCurrency` (SGD `S$`, 2dp, tabular), `formatDate`, `formatRelativeDeadline`.
- `tenderStatus.js` — maps `Tender.Status` → StatusChip status token + label.
- (`deadlineStatus.js` / `priority.js` arrive in later phases.)

### Routing (`client/src/App.jsx`)
| Route | Component | Guard |
|-------|-----------|-------|
| `/login` | `LoginPage` | Anonymous |
| `/` | `DashboardPage` | Protected |
| `/tenders` | `TendersListPage` | Protected |
| `/tenders/new` | `TenderFormPage` | Protected + RoleGate(Admin, Estimator) |
| `/tenders/:id` | `TenderDetailPage` | Protected |
| `/tenders/:id/edit` | `TenderFormPage` | Protected + RoleGate(Admin, Estimator) |
| `*` | `NotFoundPage` | — |

Wrap the tree in `QueryClientProvider`, `AuthProvider`, `ThemeProvider`, `BrowserRouter`,
`HelmetProvider`.

---

## 0.6 — Tender UI & Dashboard Shell

### Pages (`client/src/pages/`)
- `LoginPage.jsx` — Formik + Yup email/password form; on success store token, route to `/`.
- `TendersListPage.jsx` — `DataTable` of tenders with status filter + search box; `StatusChip` per
  row; "New Tender" button gated by role; row click → detail; `EmptyState` when none.
- `TenderDetailPage.jsx` — single-column detail (Style Guide §5) with a right-rail metadata panel
  (status, agency, est. value via `formatCurrency`, closing date); Edit / Delete (role-gated; delete
  Admin-only) behind `ConfirmDialog`.
- `TenderFormPage.jsx` — shared create/edit form (Formik + Yup): reference, title, agency, status
  (select), est. value, closing date, notes. Server validation surfaced (e.g. duplicate reference).

### Tender components (`client/src/components/tenders/`)
- `TenderTable.jsx`, `TenderStatusFilter.jsx`, `TenderForm.jsx` — extracted for reuse/testability.

### Dashboard shell (`client/src/pages/DashboardPage.jsx`)
- Counter cards driven by real tender data now; placeholders for later phases:
  | Card | Data source (Phase 0) |
  |------|------------------------|
  | Active tenders | count of non-`Won`/`Lost` |
  | Closing soon | count with `ClosingAt` within 7 days (basic; full logic in Phase 1) |
  | Drafting | count `Status = Drafting` |
  | Won (this year) | count `Status = Won` |
- A "Recent activity" panel backed by `GET /api/audit/recent` (Admin sees it; others get a simple
  recent-tenders list) — demonstrates the AuditLog visibly.

### Audit endpoint (so the dashboard can show it)
`Controllers/AuditController.cs` — `[Route("api/audit")]`
| Method | Route | Roles | Purpose |
|--------|-------|-------|---------|
| GET | `/api/audit/recent?limit=50` | `Admin` | Recent audit rows for the activity panel |

---

## Files to Create

### Backend
| File | Purpose |
|------|---------|
| `server/.../Program.cs` | App config, DI, middleware (CLAUDE.md pattern) |
| `server/.../appsettings.json` + `.Development.json` | Config + dev secrets (latter git-ignored) |
| `server/.../Data/AppDbContext.cs` | EF context authored now, wired later |
| `server/.../Data/InMemoryStore.cs` | Singleton mock store, seeds from JSON |
| `server/.../Data/InMemoryTenderService.cs` | `ITenderService` mock impl |
| `server/.../Data/seed/users.json` · `tenders.json` | Seed data |
| `server/.../Models/User.cs` · `Tender.cs` · `AuditLog.cs` · `Roles.cs` | Entities + constants |
| `server/.../Models/Auth/*.cs` · `Models/Tenders/*.cs` | DTO records |
| `server/.../Services/IAuthService.cs` · `AuthService.cs` | Auth + JWT |
| `server/.../Services/ITenderService.cs` | Tender service contract |
| `server/.../Services/IAuditService.cs` · `AuditService.cs` | Audit logging |
| `server/.../Controllers/AuthController.cs` · `TendersController.cs` · `AuditController.cs` | API |

### Frontend
| File | Purpose |
|------|---------|
| `client/vite.config.js` | API proxy |
| `client/src/theme.js` | Style Guide tokens |
| `client/src/main.jsx` · `App.jsx` | Providers + routing |
| `client/src/api/axios.js` · `auth.js` · `tenders.js` | API layer |
| `client/src/context/AuthContext.jsx` | Auth state |
| `client/src/components/common/{ProtectedRoute,RoleGate,StatusChip,DataTable,ConfirmDialog,EmptyState}.jsx` | Shared UI |
| `client/src/components/layout/{AppShell,SideNav,TopBar}.jsx` | Shell |
| `client/src/components/tenders/{TenderTable,TenderStatusFilter,TenderForm}.jsx` | Tender UI |
| `client/src/pages/{LoginPage,DashboardPage,TendersListPage,TenderDetailPage,TenderFormPage,NotFoundPage}.jsx` | Pages |
| `client/src/utils/{format,tenderStatus}.js` | Helpers |

---

## Role Permission Matrix (Phase 0)

| Action | Admin | Estimator | Viewer |
|--------|:-----:|:---------:|:------:|
| Log in / view dashboard | ✅ | ✅ | ✅ |
| View tenders (list + detail) | ✅ | ✅ | ✅ |
| Create / edit tender | ✅ | ✅ | ❌ |
| Delete tender | ✅ | ❌ | ❌ |
| Manage users | ✅ | ❌ | ❌ |
| View audit log | ✅ | ❌ | ❌ |

Enforced **on the API** via `[Authorize(Roles=...)]` (source of truth) and mirrored in the UI via
`RoleGate` (convenience only — never the sole gate).

---

## Edge Cases & Error Handling (Phase 0)

| Scenario | Handling |
|----------|----------|
| Bad login credentials | `401`; generic "Invalid email or password" (no user enumeration) |
| Expired / tampered JWT | `401`; axios interceptor clears token + redirects to `/login` |
| Viewer hits a mutating endpoint directly | `403` from `[Authorize(Roles)]`; UI control hidden via `RoleGate` |
| Duplicate tender reference on create | Service returns conflict → controller `409 Conflict`; form shows field error |
| Unknown tender status value | `400 BadRequest` with validation message |
| Delete a non-existent tender | `404 NotFound` |
| Audit write fails | Swallowed + logged; primary operation still succeeds |
| Backend not running / proxy fails | Frontend shows a toast + retry (React Query error state) |
| Empty tender list | `EmptyState` with "Add your first tender" CTA (role-gated) |
| Direct nav to a guarded route while logged out | `ProtectedRoute` redirects to `/login`, returns after auth |

---

## Verification Checklist (0.7)

Run both servers (`dotnet run` on :5043, `npm run dev` on :5173) and confirm:

- [ ] Login as each seeded role returns a JWT and lands on the dashboard.
- [ ] `GET /api/auth/me` returns the correct role for each user.
- [ ] Admin can create, edit, and delete a tender; changes persist in the in-memory store across requests.
- [ ] Estimator can create/edit but **cannot** delete (button hidden + API returns `403`).
- [ ] Viewer can browse but sees no create/edit/delete controls; direct API mutation returns `403`.
- [ ] Duplicate reference create returns `409` and shows a field error.
- [ ] Tender list filter + search work; `StatusChip` colours match the Style Guide.
- [ ] Dashboard counters reflect seeded + newly created tenders.
- [ ] Every create/update/delete and each login appears in `GET /api/audit/recent` (as Admin).
- [ ] Money renders as `S$` via `formatCurrency`; dates via `formatDate`; no hardcoded hex in components.
- [ ] Swagger lists Auth, Tenders, Audit endpoints and login works from Swagger.

---

## Definition of Done

Phase 0 is complete when:
1. The verification checklist passes end-to-end.
2. All API ↔ data access goes through **service interfaces** — no controller touches a store/context
   directly, so the future EF swap is a one-line DI change per service.
3. The app shell, theme tokens, `StatusChip`, and formatting utils are in place per the Style Guide.
4. The AuditLog pattern is proven on a real mutation flow, ready to carry sign-off audit in Phase 3.

> **Next:** Phases 1 (Tracker + Calendar) and 2 (Inventory) can then proceed in parallel —
> both depend only on this foundation.
