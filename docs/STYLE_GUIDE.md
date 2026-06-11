# Tenderquick — Style Guide

> Professional, trust-first B2B tooling aesthetic.
> Light base with a deep-indigo brand colour, status-driven accents, and dense-but-legible
> data layouts — this is a working tool for estimators, not a marketing site.

---

## 1. Design Philosophy

- **Tool, not brochure**: Tenderquick is used daily to track deadlines, build quotes, and draft
  documents. Optimise for scannability, density, and speed — not hero animations.
- **Status is the UI**: A tender's state (deadline proximity, quote sign-off, won/lost) is the
  single most important signal on every screen. Status colour is a first-class design token.
- **Trust through clarity**: Money and legal deadlines are on the line. Every figure, date, and
  AI-generated suggestion must be unambiguous about its source and whether a human has verified it.
- **AI is a draft, never an authority**: Anything produced by AI is visually marked as a draft
  until a human signs off. Never let AI output look like a confirmed fact.

---

## 2. Colour Tokens

All colours are defined in `client/src/theme.js` via the exported `tokens` object.
**Always import and use `tokens.*` instead of hardcoding hex values.**

### Surfaces (Light theme)

| Token          | Value     | Usage                              |
| -------------- | --------- | ---------------------------------- |
| `white`        | `#FFFFFF` | Page background, card background    |
| `offWhite`     | `#FAFAFA` | Subtle alternate background, table stripes |
| `lightGray`    | `#F3F4F6` | Elevated section backgrounds, app shell |
| `borderLight`  | `#E5E7EB` | Card borders, dividers, table lines |
| `borderMedium` | `#D1D5DB` | Hover borders, input borders        |

### Dark Sections (App bar, side nav, sign-off banners)

| Token          | Value     | Usage                            |
| -------------- | --------- | -------------------------------- |
| `navy`         | `#0F172A` | Side navigation, dark headers     |
| `darkCharcoal` | `#1E293B` | Dark section gradients            |
| `slate`        | `#334155` | Secondary dark surfaces           |

### Brand Accent

| Token              | Value     | Usage                              |
| ------------------ | --------- | ---------------------------------- |
| `accentIndigo`     | `#4F46E5` | Primary CTA, active nav, links     |
| `accentIndigoHover`| `#4338CA` | Hovered primary buttons            |
| `accentIndigoLight`| `#E0E7FF` | Light tint backgrounds, chips      |
| `accentIndigoSubtle`| `#EEF2FF`| Selected rows, subtle highlights   |

### Status Tokens — the core of this app

Status colour drives deadline tracking, quote state, and tender pipeline. Use these consistently
everywhere a state is shown (chips, row accents, calendar dots, dashboard counters).

| Token            | Value     | Meaning                                            |
| ---------------- | --------- | -------------------------------------------------- |
| `statusOverdue`  | `#DC2626` | Deadline passed / action required now              |
| `statusUrgent`   | `#EA580C` | Due within 3 days                                  |
| `statusSoon`     | `#CA8A04` | Due within 7 days                                  |
| `statusOnTrack`  | `#16A34A` | Comfortable lead time / submitted / won            |
| `statusNeutral`  | `#6B7280` | No deadline / informational / closed-lost          |
| `statusDraft`    | `#7C3AED` | AI-generated, awaiting human review                |

> **Rule:** Deadline proximity colour is computed in one place (`utils/deadlineStatus.js`) and
> reused — never re-derive "is this urgent?" logic inside a component.

### Text on Light Backgrounds

| Token           | Value     | Usage                          |
| --------------- | --------- | ------------------------------ |
| `textPrimary`   | `#111827` | Headings, primary body, figures |
| `textSecondary` | `#6B7280` | Descriptions, captions, labels  |
| `textMuted`     | `#9CA3AF` | Meta info, timestamps, hints    |

### Text on Dark Backgrounds

| Token             | Value                   | Usage                       |
| ----------------- | ----------------------- | --------------------------- |
| `textOnDark`      | `#FFFFFF`               | Nav labels, dark headers     |
| `textOnDarkMuted` | `rgba(255,255,255,0.6)` | Secondary nav text           |
| `textOnDarkSubtle`| `rgba(255,255,255,0.7)` | Sub-labels on dark           |

> **Rule:** Use `tokens.textSecondary` — never write `color: 'text.secondary'` inline.

---

## 3. Typography

**Font Family:** `"Inter", -apple-system, BlinkMacSystemFont, "system-ui", sans-serif`

All values are defined in `theme.js`. Do NOT add inline `fontSize` / `lineHeight` overrides —
the theme governs these.

| Variant   | Size      | Weight | Line Height | Letter Spacing | Usage                       |
| --------- | --------- | ------ | ----------- | -------------- | --------------------------- |
| `h1`      | 2.25rem   | 700    | 1.2         | -0.02em        | Page titles                 |
| `h2`      | 1.75rem   | 700    | 1.25        | -0.02em        | Section titles              |
| `h3`      | 1.375rem  | 600    | 1.3         | —              | Sub-section / panel headings|
| `h4`      | 1.125rem  | 600    | 1.4         | —              | Card titles, table captions |
| `body1`   | 1rem      | 400    | 1.6         | —              | Primary body text           |
| `body2`   | 0.875rem  | 400    | 1.55        | —              | Table cells, secondary text |
| `mono`    | 0.875rem  | 500    | 1.5         | —              | Money, quantities, tender refs |
| `button`  | 0.875rem  | 600    | —           | 0.3px          | Button labels (no UPPERCASE)|

**Rules:**
- **Money, quantities, and tender reference numbers use a tabular/monospace style** so columns
  align. Define a `mono` typography variant; use `fontVariantNumeric: 'tabular-nums'`.
- Page titles use `variant="h1"`; section headings within a page use `variant="h2"`.
- No ALL-CAPS button text. Uppercase is reserved for small status chips (letter-spacing `0.06em`).

---

## 4. Spacing Standards

**Base unit:** `8px` (MUI default `theme.spacing(1) = 8px`)

This is an app, not a landing page — sections are tighter than a marketing site.

### Page / Section Padding

| Context             | Value                | Pixels   |
| ------------------- | -------------------- | -------- |
| App content padding | `{ xs: 2, md: 4 }`   | 16/32px  |
| Card internal `p`   | `3`                  | 24px     |
| Dense card `p`      | `2`                  | 16px     |

### Internal Spacing

| Property                    | Value | Pixels | Usage                          |
| --------------------------- | ----- | ------ | ------------------------------ |
| Page title → content `mb`   | `3`   | 24px   | Below every page `h1`          |
| Section heading → body `mb` | `2`   | 16px   | Below an `h2`                  |
| Grid / card gap (`spacing`) | `2.5` | 20px   | Between cards and form rows    |
| Form field vertical gap     | `2`   | 16px   | Between stacked inputs         |

---

## 5. Layout & Container Widths

Tenderquick uses a **persistent left navigation + top app bar** shell (not a marketing scroll).

| Element        | Spec                                                            |
| -------------- | -------------------------------------------------------------- |
| App shell      | Fixed left nav (`240px`, collapsible to icons), top app bar (`64px`) |
| Content max    | `xl` for list/table pages; `lg` for forms and detail pages     |
| Detail pages   | `maxWidth="lg"`, single column with right-rail metadata panel   |
| Forms / wizards| `maxWidth="md"`, centred                                       |

> **Rule:** List and table views (Tenders, Inventory, Quotations) use `maxWidth="xl"`.
> Editing/detail views use `maxWidth="lg"`. Wizards (planner-style stepped forms) use `maxWidth="md"`.

---

## 6. Border Radius Scale

| Value                | Pixels | Usage                                       |
| -------------------- | ------ | ------------------------------------------- |
| `12` (theme default) | 12px   | `MuiCard` — list cards, panels              |
| `2` (×8)             | 16px   | Content boxes, selected option cards         |
| `3` (×8)             | 24px   | Outer Paper wrappers (wizards, sign-off panel)|
| `8` (component)      | 8px    | Buttons, inputs, chips                       |
| `'50%'`              | —      | Avatars, status dots                         |

---

## 7. Buttons

### Primary (Contained)
- Background `tokens.accentIndigo`; text `#FFFFFF`; hover `tokens.accentIndigoHover`.
- Border radius `8px`; no box-shadow.
- CTA padding `px: 3, py: 1.25`.

### Secondary (Outlined)
- Border `1px solid tokens.borderMedium`; text `tokens.textPrimary`; hover border `accentIndigo`.

### Destructive
- Use `statusOverdue` for delete / discard actions; always behind a confirmation dialog.

### Sign-off (special)
- The human-verification action ("I have reviewed and approve this quotation") uses a
  **contained `statusOnTrack` button** and is disabled until required checks pass. See §10.

> **Rule:** The left-nav and top-bar buttons are intentionally smaller — do NOT apply CTA padding to them.

---

## 8. Status Chips

A single `<StatusChip status={...} />` component renders every state across the app.

| Status        | Background            | Text / Dot       | Example label   |
| ------------- | --------------------- | ---------------- | --------------- |
| Overdue       | tint of `statusOverdue` | `statusOverdue`| "Overdue"       |
| Urgent        | tint of `statusUrgent`  | `statusUrgent` | "Due in 2d"     |
| Soon          | tint of `statusSoon`    | `statusSoon`   | "Due in 6d"     |
| On track      | tint of `statusOnTrack` | `statusOnTrack`| "Submitted"     |
| Draft (AI)    | tint of `statusDraft`   | `statusDraft`  | "AI Draft"      |
| Neutral       | `lightGray`             | `statusNeutral`| "Closed"        |

- Chip: small, `borderRadius: 8px`, uppercase label, letter-spacing `0.06em`, leading dot.
- **Tints** are the `accentIndigoSubtle`-style 10–15% backgrounds of each status colour, defined as
  `statusXxxBg` tokens — never compute opacity inline.

---

## 9. Data Tables & Money

Tables are central (tender lists, quote line items, price history). Standardise them.

- Use one `<DataTable>` wrapper (built on MUI `Table` / DataGrid) with consistent header styling:
  `offWhite` header background, `body2` cells, `borderLight` row dividers, hover row `accentIndigoSubtle`.
- **Numeric columns are right-aligned** and use the `mono` variant with `tabular-nums`.
- Money is always rendered through a `formatCurrency()` util (`utils/format.js`) — never inline
  `toLocaleString`. Default currency SGD, symbol `S$`, 2 decimals.
- Dates render through `formatDate()` / `formatRelativeDeadline()` — never raw `Date` in JSX.
- Empty states are mandatory: every table has a friendly empty state with a primary action
  (e.g. "No tenders yet — Add your first tender").

---

## 10. AI & Human-in-the-Loop Conventions

These rules are load-bearing for the product's trustworthiness.

| Rule | Detail |
| ---- | ------ |
| Mark AI output | Any AI-generated text/quote shows a `Draft (AI)` `statusDraft` chip until signed off. |
| Show provenance | AI suggestions display a small "Generated by AI · review before use" caption. |
| Sign-off gate | PDF export / "Submit" is **disabled** until a human ticks the verification checkbox AND clicks the green sign-off button. |
| Audit the sign-off | Sign-off records who, when, and which version (see implementation plan). The UI shows "Verified by {name} on {date}". |
| Editable, not locked | AI drafts are always editable. Never present AI output as read-only fact. |
| No silent autofill | When AI pre-fills a quote, changed/AI-touched fields are subtly highlighted (`statusDraft` left border) so the human sees what to check. |

---

## 11. Frontend Code Conventions (recap of CLAUDE.md, applied here)

- **`.jsx` only** — no TypeScript.
- **MUI `sx` + `styled()` only** — no other CSS-in-JS; no hardcoded hex (use `tokens.*`).
- **No comments** unless the *why* is non-obvious.
- API calls go through thin Axios files in `src/api/`, consumed via `@tanstack/react-query`.
- Axios base URLs are relative (`/api/...`); the Vite proxy handles the port.
- Forms use Formik + Yup; all wizard steps validate before advancing.
- Shared formatting/derivation logic (currency, dates, deadline status, priority) lives in
  `src/utils/` and is imported — never duplicated in components.

---

## 12. What This UI Is — and Is Not

| IS                                              | IS NOT                                      |
| ----------------------------------------------- | ------------------------------------------- |
| A dense, fast internal tool for estimators       | A marketing landing page                    |
| Status-driven (deadlines, sign-off, pipeline)    | Decorative / animation-led                  |
| Explicit about what is AI draft vs human-verified| A black box that auto-submits AI output     |
| Consistent tables, money, and date formatting    | Ad-hoc `toLocaleString` scattered in JSX    |
| Token-driven colour and spacing                  | Hardcoded hex and magic numbers             |
