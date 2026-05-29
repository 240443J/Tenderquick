# New Project — AI Setup Guide

This document is the canonical instruction set for any AI assistant scaffolding or extending
this project. Read it fully before writing a single line of code.

---

## Stack

| Layer    | Technology                                 | Version / Notes                     |
|----------|--------------------------------------------|-------------------------------------|
| Frontend | React + Vite (SWC)                         | React 19, Vite 8, JSX (not TSX)     |
| Backend  | ASP.NET Core Web API (C#)                  | .NET 8                              |
| Database | MySQL via Entity Framework Core + Pomelo   | EF Core 8, Pomelo 8                 |
| Auth     | JWT Bearer tokens (BCrypt password hashing)| `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.2 |

---

## Folder Structure to Create

```
<ProjectName>/
├── client/                   # React + Vite frontend
│   ├── src/
│   │   ├── api/              # Axios call files, one per domain (e.g. auth.js, users.js)
│   │   ├── components/       # Shared reusable UI components
│   │   ├── context/          # React context providers (e.g. AuthContext)
│   │   ├── hooks/            # Custom hooks
│   │   ├── pages/            # Route-level page components
│   │   ├── utils/            # Pure helper functions
│   │   ├── App.jsx
│   │   ├── main.jsx
│   │   └── theme.js          # MUI theme customisation
│   ├── index.html
│   ├── vite.config.js
│   └── package.json
└── server/
    └── <ProjectName>Server/
        └── <ProjectName>Server/
            ├── Controllers/  # One controller per domain, route = api/<domain>
            ├── Data/         # DbContext
            ├── Models/       # EF entity classes + request/response DTOs
            ├── Services/     # Business logic injected as scoped services
            ├── Migrations/   # EF migrations (auto-generated, do not edit)
            ├── appsettings.json
            ├── appsettings.Development.json
            └── Program.cs
```

---

## Step 1 — Prerequisites (ask AI to verify / install)

```
node --version          # must be >= 20
dotnet --version        # must be 8.x
mysql --version         # must be running on port 3306
```

If any are missing the AI must guide the user to install them before proceeding.

---

## Step 2 — Scaffold the Frontend

```bash
# From the project root
npm create vite@latest client -- --template react-swc
cd client
npm install
npm install react-router-dom@^7 axios @tanstack/react-query \
  @mui/material @mui/icons-material @emotion/react @emotion/styled \
  @fontsource/inter framer-motion react-helmet-async formik yup
```

**`vite.config.js`** — must include the API proxy so `/api` and `/uploads` requests are
forwarded to the backend during development:

```js
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': { target: 'http://localhost:5043', changeOrigin: true },
      '/uploads': { target: 'http://localhost:5043', changeOrigin: true },
    },
  },
})
```

---

## Step 3 — Scaffold the Backend

```bash
# From the project root
mkdir -p server/<ProjectName>Server
cd server/<ProjectName>Server
dotnet new webapi -n <ProjectName>Server --no-https false
cd <ProjectName>Server

# Core NuGet packages (match exact versions)
dotnet add package Pomelo.EntityFrameworkCore.MySql --version 8.0.2
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.2
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.2
dotnet add package BCrypt.Net-Next --version 4.0.3
dotnet add package Swashbuckle.AspNetCore --version 6.6.2
dotnet add package MailKit --version 4.10.0
```

Optional packages (add when the feature is needed):
```bash
dotnet add package Stripe.net --version 51.0.0
dotnet add package QuestPDF --version 2026.2.4
dotnet add package CsvHelper --version 33.0.1
dotnet add package Google.Apis.Calendar.v3
```

---

## Step 4 — Create the Database Context

**`Data/AppDbContext.cs`**

```csharp
using Microsoft.EntityFrameworkCore;

namespace <ProjectName>Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Add DbSet<T> for each entity here, e.g.:
        // public DbSet<User> Users { get; set; }
    }
}
```

---

## Step 5 — `appsettings.json` Template

Replace all placeholder values before running.

```json
{
  "Logging": {
    "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "MyConnection": "server=localhost;port=3306;database=<db_name>;user=<db_user>;password=<db_password>"
  },
  "AllowedOrigins": [
    "http://localhost:5173"
  ],
  "Jwt": {
    "Key": "<32-char-random-secret>",
    "Issuer": "<ProjectName>Server",
    "Audience": "<ProjectName>Client",
    "ExpireMinutes": 1440
  }
}
```

`appsettings.Development.json` — same ConnectionStrings pointing to the local dev DB, plus
add dev-only origins.

---

## Step 6 — `Program.cs` Canonical Pattern

```csharp
using System.Text;
using <ProjectName>Server.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ───────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("MyConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString!, ServerVersion.AutoDetect(connectionString!)));

// ─── CORS ───────────────────────────────────────────────────
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader());
});

// ─── Authentication / JWT ────────────────────────────────────
var jwtConfig = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtConfig["Issuer"],
        ValidAudience            = jwtConfig["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtConfig["Key"]!))
    };
});
builder.Services.AddAuthorization();

// ─── Controllers + Swagger ──────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html"); // SPA fallback

app.Run();
```

---

## Step 7 — Controller Convention

Every controller follows this pattern:

```csharp
using Microsoft.AspNetCore.Mvc;
using <ProjectName>Server.Data;

namespace <ProjectName>Server.Controllers
{
    [ApiController]
    [Route("api/<resource>")]          // e.g. api/users
    public class <Resource>Controller : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public <Resource>Controller(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // GET api/<resource>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SomeEntity>>> GetAll() { ... }

        // GET api/<resource>/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SomeEntity>> GetById(int id) { ... }

        // POST api/<resource>
        [HttpPost]
        public async Task<ActionResult<SomeEntity>> Create([FromBody] CreateRequest req) { ... }

        // PUT api/<resource>/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<SomeEntity>> Update(int id, [FromBody] UpdateRequest req) { ... }

        // DELETE api/<resource>/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) { ... }
    }
}
```

- Protected routes: add `[Authorize]` at class or method level.
- Return `CreatedAtAction`, `Ok`, `NotFound`, `BadRequest`, `Conflict` — never raw 200/404 ints.
- Always use `async/await` with EF Core calls.

---

## Step 8 — Model / Entity Convention

```csharp
namespace <ProjectName>Server.Models
{
    public class SomeEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Request DTOs live alongside the entity in the same Models file or a nested DTOs file
    public record CreateRequest(string Name);
    public record UpdateRequest(string Name);
}
```

---

## Step 9 — Frontend API Call Convention

All backend calls go through thin Axios files in `src/api/`:

```js
// src/api/someResource.js
import axios from 'axios'

const BASE = '/api/some-resource'

export const getAll    = ()        => axios.get(BASE)
export const getById   = (id)      => axios.get(`${BASE}/${id}`)
export const create    = (data)    => axios.post(BASE, data)
export const update    = (id, data)=> axios.put(`${BASE}/${id}`, data)
export const remove    = (id)      => axios.delete(`${BASE}/${id}`)
```

Consume these with `@tanstack/react-query` in components.

Auth tokens are stored in `localStorage` and attached via an Axios interceptor placed in
`main.jsx` or a dedicated `src/api/axios.js` setup file:

```js
axios.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})
```

---

## Step 10 — EF Migrations

After adding/changing any entity:

```bash
cd server/<ProjectName>Server/<ProjectName>Server

# Create migration
dotnet ef migrations add <MigrationName>

# Apply to database
dotnet ef database update
```

---

## Step 11 — Running the Project

**Backend:**
```bash
cd server/<ProjectName>Server/<ProjectName>Server
dotnet run
# Listens on http://localhost:5043 (or the port in launchSettings.json)
# Swagger: http://localhost:5043/swagger
```

**Frontend:**
```bash
cd client
npm run dev
# Runs on http://localhost:5173
# /api requests are proxied to backend automatically
```

---

## Conventions & Rules for the AI

1. **No TypeScript** — the frontend uses `.jsx` files only, matching this project.
2. **No CSS-in-JS beyond MUI** — use MUI's `sx` prop or `styled()` for component styles.
3. **No comments** unless the WHY is non-obvious (hidden constraint, workaround).
4. **No extra packages** beyond what is listed unless the user explicitly requests a feature that needs one.
5. **Secrets never in source control** — `appsettings.json` connection strings and JWT keys are placeholders; real values go in environment variables or `appsettings.Development.json` (git-ignored).
6. **EF migrations are the only schema source of truth** — never write raw `CREATE TABLE` SQL for the application schema.
7. **JWT expiry is 1440 minutes (24 h)** — do not change without being asked.
8. **CORS origins come from config** — never hardcode them in `Program.cs`.
9. **SPA fallback** (`MapFallbackToFile`) must always be the last middleware — after `MapControllers`.
10. **Axios base URL is relative `/api/...`** — the Vite proxy handles the port, so no absolute URLs in frontend code.
