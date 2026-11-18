# Waytokaaba

Umrah packages and Makkah/Madinah hotel discovery platform — **MVP**.

Visitors browse Umrah packages, search hotels in Makkah and Madinah (filters, sorting, pagination), view details,
and send **inquiries** (no online booking or payment). Admins sign in to manage hotels, rooms, packages and inquiries.

| Part | Technology |
| --- | --- |
| Frontend | Next.js 16 (App Router, Cache Components), React 19, TypeScript, Tailwind CSS 4 |
| Backend | ASP.NET Core Web API on **.NET 10 (LTS)**, EF Core 10, FluentValidation, JWT auth, Swagger |
| Database | PostgreSQL 17 (Npgsql), EF Core migrations, demo seed data |
| Tests | xUnit + Testcontainers (real PostgreSQL) for the API · Playwright end-to-end tests for the website |
| Free hosting | **Vercel** (frontend) · **Render** (API, Docker) · **Neon** (PostgreSQL) |

> **Why .NET 10 instead of .NET 9?** .NET 9 reaches end of support on 10 Nov 2026; .NET 10 is the current
> long-term-support release (supported until Nov 2028). The code and architecture are otherwise unchanged.

---

## Contents

1. [Project structure](#project-structure)
2. [Run locally](#run-locally)
3. [Architecture](#architecture)
4. [API reference](#api-reference)
5. [Admin area](#admin-area)
6. [Testing](#testing)
7. [Deploy for free (Neon → Render → Vercel)](#deploy-for-free)
8. [Environment variables](#environment-variables)
9. [Final deployment checklist](#final-deployment-checklist)
10. [Demo content & images](#demo-content--images)
11. [MVP assumptions](#mvp-assumptions)
12. [Extending the platform](#extending-the-platform)

---

## Project structure

The backend and the frontend are **two separate folders (and two separate git repositories)**:

```
C:\Users\...\source\repos\
│
├── WayToKaaba\                         ← BACKEND — open WayToKaaba.slnx in Visual Studio
│   ├── WayToKaaba.slnx
│   ├── src\
│   │   ├── Waytokaaba.Core\            Entities and enums (no dependencies)
│   │   ├── Waytokaaba.Application\     DTOs, validators, services (business logic), IAppDbContext
│   │   ├── Waytokaaba.Infrastructure\  EF Core DbContext, configurations, migrations, seed data, JWT, hashing
│   │   └── Waytokaaba.API\             Controllers, error handling, auth, CORS, rate limiting, Swagger, Dockerfile
│   ├── tests\Waytokaaba.Tests\         API integration tests (Testcontainers PostgreSQL)
│   ├── Waytokaaba.postman_collection.json
│   ├── .env.example                    Production environment variables for the API
│   ├── docker-compose.yml              Local PostgreSQL (and optional API container)
│   ├── render.yaml                     Render blueprint for the API
│   └── docs\IMPLEMENTATION_CHECKLIST.md
│
└── Waytokaaba-frontend\                ← FRONTEND — open this folder in VS Code
    ├── src\app\(site)\                 Public website pages
    ├── src\app\admin\                  Admin area (login, inquiries, hotels, rooms, packages)
    ├── src\components\                 Reusable UI components
    ├── src\lib\                        API client, types, formatting, config
    ├── src\proxy.ts                    Protects /admin routes
    ├── e2e\                            Playwright end-to-end tests
    └── .env.example
```

## Run locally

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download), [Node.js 20.9+](https://nodejs.org),
[Docker Desktop](https://www.docker.com/products/docker-desktop/).

```bash
# In the backend folder (WayToKaaba\)
# 1. Start PostgreSQL
docker compose up -d db

# 2. Start the API  →  http://localhost:5062  (Swagger: http://localhost:5062/swagger)
dotnet run --project src/Waytokaaba.API --launch-profile http
#    (or open WayToKaaba.slnx in Visual Studio, set Waytokaaba.API as startup project, choose the "http" profile, F5)
```

In Development the API automatically applies migrations, seeds demo data and creates a local admin
(`admin@waytokaaba.local` / `Dev-Admin-Pass-123` — **local only**, see `appsettings.Development.json`).

```bash
# 3. Start the website  →  http://localhost:3000   (in the Waytokaaba-frontend folder, e.g. VS Code terminal)
cp .env.example .env.local     # points to http://localhost:5062
npm install
npm run dev
```

Admin area: http://localhost:3000/admin

## Architecture

### Backend (clean architecture)

```
API  ──►  Application  ──►  Core
 │             ▲
 └──►  Infrastructure (implements IAppDbContext, ITokenService, IPasswordHasher)
```

- **Controllers** are thin: they bind requests and call application services. Entities are never exposed — only DTOs.
- **Validation:** FluentValidation validators run for every request body and query object (`ValidationFilter`).
- **Errors:** `GlobalExceptionHandler` converts exceptions to RFC 7807 `ProblemDetails`
  (400 validation, 401, 404, 409 conflicts, 500 without internal details).
- **Security:** JWT bearer auth with an `Admin` policy on every mutating/admin endpoint, PBKDF2 password hashing
  (ASP.NET Identity hasher), CORS limited to configured origins, per-IP rate limiting on inquiries and login,
  honeypot spam field, parameterised queries via EF Core, secrets only from environment variables.
- **Data:** PostgreSQL with snake_case naming, enums stored as text, `numeric(12,2)` money, `timestamptz` audit
  timestamps, `date` for check-in/out, check constraints, unique slugs and indexes for common filters.

### Database schema

| Table | Purpose / key constraints |
| --- | --- |
| `hotels` | City, stars (1–5), "from" price, distance (meters) + `is_distance_verified`, amenities (`text[]`), featured/active/demo flags. Unique `slug`. Indexes on (city, price), (city, stars). |
| `hotel_images` | Gallery, ordered. Cascade delete with hotel. |
| `rooms` | Room types: capacity (1–20), price, beds. Cascade delete with hotel. |
| `packages` | Type, duration, Makkah/Madinah nights (`makkah + madinah ≤ duration`), price. Unique `slug`. |
| `package_hotels` | Hotels in a package + nights. Unique (package, hotel). Hotels in use **cannot be deleted** (restrict). |
| `package_services` / `package_images` | Included services and gallery, ordered. |
| `inquiries` | Customer details, type, optional package/hotel (set null if listing deleted), dates, guests, status. |
| `admin_users` | Admin accounts (unique email, password hash). |

### Frontend

- Pages are **server components** that call the API at request time and stream results behind skeleton loaders
  (Partial Prerendering). The build never calls the API, so a sleeping free-tier API can't break deployments.
- Filters, sorting and pagination live in the URL (shareable, back-button friendly); filter controls update
  optimistically so the UI responds instantly.
- Inquiry forms post directly from the browser to the API (so rate limits apply per visitor).
- Admin uses **server actions**; the JWT is kept in an **httpOnly cookie** and never exposed to browser JavaScript.
- Images use a custom `next/image` loader that requests correctly sized images from the image CDN
  (no Vercel image-optimisation quota is used).

## API reference

Interactive docs: **`/swagger`**. A Postman collection is in `Waytokaaba.postman_collection.json`
(run *Auth → Login* first; it stores the token for admin requests).

| Method | Endpoint | Access | Notes |
| --- | --- | --- | --- |
| GET | `/api/packages` · `/api/packages/search` | Public | `q, minPrice, maxPrice, minDays, maxDays, type (repeatable), featured, sort, page, pageSize` |
| GET | `/api/packages/featured` | Public | `limit` |
| GET | `/api/packages/{idOrSlug}` | Public | |
| POST / PUT / DELETE | `/api/packages[/{id}]` | Admin | Includes hotels, services, images |
| GET | `/api/hotels` · `/api/hotels/search` | Public | `city, q, minPrice, maxPrice, stars (repeatable), guests, featured, sort, page, pageSize` |
| GET | `/api/hotels/city/{city}` | Public | `makkah` / `madinah` + same filters |
| GET | `/api/hotels/featured` | Public | `city, limit` |
| GET | `/api/hotels/{idOrSlug}` | Public | Includes rooms and gallery |
| GET | `/api/hotels/{hotelId}/rooms` | Public | |
| POST / PUT / DELETE | `/api/hotels[/{id}]` | Admin | Delete returns 409 if the hotel is used in a package |
| POST / PUT / DELETE | `/api/hotels/{hotelId}/rooms[/{roomId}]` | Admin | |
| POST | `/api/inquiries` | Public | Rate limited (5/min/IP by default) |
| GET | `/api/inquiries` | Admin | `status, type, page, pageSize` |
| PATCH | `/api/inquiries/{id}/status` | Admin | `New`, `Contacted`, `Closed` |
| POST | `/api/auth/login` | Public | Returns JWT; rate limited |
| GET | `/api/admin/hotels[/{id}]` · `/api/admin/packages[/{id}]` | Admin | Includes inactive records |
| GET | `/health` | Public | Database health check |

Sort values — hotels: `recommended, price_asc, price_desc, stars_desc, distance_asc`;
packages: `recommended, price_asc, price_desc, duration_asc, duration_desc`.
Paged responses: `{ items, page, pageSize, totalCount, totalPages, hasNextPage, hasPreviousPage }`.

## Admin area

`/admin` → sign in with the admin account configured via environment variables.

- **Inquiries:** view all inquiries (newest first), filter by status, mark as Contacted/Closed.
- **Hotels & rooms:** add/edit/hide/delete hotels, gallery, facilities, verified distance; add/edit/delete room types.
- **Packages:** add/edit/hide/delete packages with included hotels (nights), services and gallery.

To add another admin account in this MVP, change `Database__AdminEmail` / `Database__AdminPassword` and restart
the API (existing accounts are kept). A user-management screen is a natural next step.

## Testing

```bash
# API integration tests — in WayToKaaba\ (needs Docker running; spins up a throwaway PostgreSQL)
dotnet test

# Frontend checks — in Waytokaaba-frontend\
npm run lint && npx tsc --noEmit && npm run build

# End-to-end tests — in Waytokaaba-frontend\ (API + website running; uses installed Microsoft Edge, or PW_CHANNEL=chrome)
npm run test:e2e
# against another URL:  E2E_BASE_URL=http://localhost:3001 npm run test:e2e
```

Current status: **34/34 API tests** and **19/19 end-to-end tests** passing (desktop + mobile).

---

## Deploy for free

All three services have free plans that need no credit card at the time of writing (check each provider's
current terms). Order matters: **database → API → website → connect CORS**.

### 0. Push both folders to GitHub (two repositories)

Create two empty repositories on GitHub, e.g. `waytokaaba-api` and `waytokaaba-frontend`, then:

```bash
# in WayToKaaba\ (backend)
git remote add origin https://github.com/<your-account>/waytokaaba-api.git
git push -u origin main

# in Waytokaaba-frontend\
git remote add origin https://github.com/<your-account>/waytokaaba-frontend.git
git push -u origin main
```

### 1. Database — Neon (free PostgreSQL)

1. Sign up at <https://neon.tech> → **New project** (Postgres 17, region close to your users, e.g. *Singapore* or *Frankfurt*).
2. Open **Connect** and copy the connection string. Use the **direct** (non-pooled) host — the one *without* `-pooler` —
   so migrations run reliably. It looks like:
   `postgresql://neondb_owner:xxxx@ep-xxxx.ap-southeast-1.aws.neon.tech/neondb?sslmode=require`

> Neon's free database does not expire (unlike Render's free PostgreSQL, which is deleted after 30 days).

### 2. API — Render (free web service, Docker)

1. Sign up at <https://render.com> with GitHub → **New → Blueprint** → select the **backend** repository (`waytokaaba-api`).
   Render reads `render.yaml` and creates the `waytokaaba-api` service.
2. When prompted, set:
   - `DATABASE_URL` → the Neon connection string
   - `Database__AdminEmail` / `Database__AdminPassword` → the client's admin login (strong password, 10+ chars)
   - `Cors__AllowedOrigins` → leave as `https://example.com` for now (updated in step 4)
   - `Jwt__SigningKey` is generated automatically.
3. Deploy. On first start the API **applies migrations and seeds demo data** automatically.
4. Check `https://<your-service>.onrender.com/health` → `Healthy`, and `/swagger`.

*(Without the blueprint: New → Web Service → Docker, Dockerfile path `src/Waytokaaba.API/Dockerfile`,
Docker build context `.`, health check `/health`, and add the variables from `.env.example`.)*

**Free-tier note:** Render's free services sleep after ~15 minutes without traffic and take up to ~1 minute to wake.
The website shows loading skeletons meanwhile, but the first visitor may wait. To keep it awake for free, create a
monitor at <https://uptimerobot.com> that requests `https://<your-service>.onrender.com/health` every 5 minutes
(one always-on free service fits within Render's 750 free hours/month).

#### Database migrations

Migrations are applied automatically at startup (`Database__ApplyMigrationsOnStartup=true`). To apply them manually instead:

```bash
dotnet tool install --global dotnet-ef
# in WayToKaaba\
ConnectionStrings__DefaultConnection="Host=...;Database=...;Username=...;Password=...;SSL Mode=Require" \
  dotnet ef database update -p src/Waytokaaba.Infrastructure -s src/Waytokaaba.API
```

Create a new migration after changing entities:
`dotnet ef migrations add <Name> -p src/Waytokaaba.Infrastructure -s src/Waytokaaba.API -o Persistence/Migrations`

### 3. Website — Vercel (free Hobby plan)

1. Sign up at <https://vercel.com> with GitHub → **Add New → Project** → import the **frontend** repository (`waytokaaba-frontend`).
2. Framework is detected as Next.js; keep the default root directory and build settings.
3. Environment variables:
   - `NEXT_PUBLIC_API_BASE_URL` = `https://<your-service>.onrender.com` (no trailing slash — **not localhost**)
   - `NEXT_PUBLIC_SITE_URL` = `https://<your-project>.vercel.app` (or the custom domain)
4. Deploy and note the URL, e.g. `https://waytokaaba.vercel.app`.

> `NEXT_PUBLIC_*` values are built into the site — if you change them later, **redeploy** on Vercel.

### 4. Connect the two (CORS)

In Render → `waytokaaba-api` → **Environment**, set `Cors__AllowedOrigins` to your Vercel URL
(e.g. `https://waytokaaba.vercel.app`; add a custom domain separated by a comma). Save — Render redeploys.

### 5. Verify

Open the Vercel URL and check: homepage loads packages and hotels · `/hotels/makkah` filters work ·
a detail page opens · submit an inquiry · sign in at `/admin` and see the inquiry.

### Custom domain (optional)

Add it in Vercel → Project → Domains, then add it to `Cors__AllowedOrigins` and `NEXT_PUBLIC_SITE_URL` (redeploy).

---

## Environment variables

### API (`.env.example` in WayToKaaba)

| Variable | Required | Description |
| --- | --- | --- |
| `DATABASE_URL` *or* `ConnectionStrings__DefaultConnection` | ✅ | PostgreSQL URL (`postgresql://…?sslmode=require`) or Npgsql connection string |
| `Jwt__SigningKey` | ✅ | 32+ random characters for signing admin tokens |
| `Cors__AllowedOrigins` | ✅ | Comma-separated frontend URLs |
| `Database__AdminEmail`, `Database__AdminPassword` | first run | Initial admin account (password 10+ chars) |
| `Database__ApplyMigrationsOnStartup` | | `true` to migrate on startup (default `true` in Production config) |
| `Database__SeedDemoData` | | `true` to insert demo hotels/packages into an empty database |
| `RateLimits__InquiriesPerMinute`, `RateLimits__LoginsPer5Minutes` | | Defaults 5 and 10 per IP |
| `Swagger__Enabled` | | Default `true` (admin endpoints still require a token) |
| `PORT` | | Set automatically by Render; container listens on 8080 otherwise |
| `Uploads__Directory` | | Folder for admin-uploaded photos. Default `uploads` under the app folder (`/app/uploads` in Docker). **Must be a persistent volume** in production (Coolify: Persistent Storage mounted at `/app/uploads`) and included in backups |
| `Uploads__PublicBaseUrl` | ✓ in production | Public API origin used in photo URLs, e.g. `https://api.way2kaaba.com`. Needed when the website calls the API on an internal address |
| `Uploads__MaxBytes` | | Largest upload in bytes, default 10 MB. Photos are stored as WebP, at most 1920 px on the longest side |

### Website (`.env.example` in Waytokaaba-frontend)

| Variable | Required | Description |
| --- | --- | --- |
| `NEXT_PUBLIC_API_BASE_URL` | ✅ | Public API URL (Render) |
| `NEXT_PUBLIC_SITE_URL` | ✅ | Public website URL (for metadata) |
| `API_BASE_URL` | | Optional server-only API URL |

## Final deployment checklist

- [ ] Both repositories (backend + frontend) pushed to GitHub (no `.env` files committed)
- [ ] Neon project created; direct connection string copied
- [ ] Render service deployed; `/health` returns `Healthy`; `/swagger` loads
- [ ] Strong `Database__AdminPassword` set; admin login works
- [ ] `Jwt__SigningKey` set (generated, 32+ chars)
- [ ] Vercel project deployed from the frontend repository
- [ ] `NEXT_PUBLIC_API_BASE_URL` points to the Render URL (not localhost)
- [ ] `Cors__AllowedOrigins` contains the exact Vercel URL (https, no trailing slash)
- [ ] Inquiry submitted on the live site appears in `/admin`
- [ ] Mobile menu and hotel filters checked on a phone
- [ ] (Optional) UptimeRobot monitor on `/health` to avoid cold starts
- [ ] Before real launch: replace placeholder contact details and social links (`Footer.tsx`, `contact/page.tsx`),
      review Privacy/Terms text, replace demo listings with verified data and set `Database__SeedDemoData=false`

## Demo content & images

- The seeded hotels are **fictional**, prices are **indicative (PKR)**, and every seeded record is flagged as demo
  content — the site shows a **"Sample listing"** badge on them. Distances are stored but **unverified**, so they
  are hidden publicly until an admin enters a verified distance and ticks *"Distance is verified"*.
- Photos are from **Unsplash** (Unsplash License: free for commercial use, no attribution required). Each URL was
  checked during development. All image URLs are stored in the database and editable in the admin area.
- To start with a clean database, set `Database__SeedDemoData=false` before the first deploy, or hide/delete the
  demo listings in the admin area.

## MVP assumptions

- **Inquiry-based, no bookings or payments.** Search dates are carried into the inquiry form; availability is
  confirmed by staff. No real-time inventory is claimed.
- Hotel "price per night" is an indicative **from** price set by the admin; room types have their own prices.
- Package prices are per person. `Makkah nights + Madinah nights ≤ total days` (the remainder is travel).
- Currency defaults to **PKR** and is stored per record, so other currencies can be added later.
- Single admin role. Contact details and social links are placeholders until the client provides real ones.

## Extending the platform

The layering keeps new features local to a module:

| Future feature | Where it fits |
| --- | --- |
| Customer accounts & dashboards | New `Customer` entity + ASP.NET Identity or an auth provider; extend JWT roles/policies |
| Online reservations & availability | `Booking` / `RoomAvailability` entities in Core; services in Application; replaces inquiry flow per listing |
| Payment gateway | Port interface in Application, provider adapter in Infrastructure (e.g. Stripe, local PK gateways) |
| Hotel supplier APIs | `IHotelSupplier` adapter in Infrastructure; map supplier data to existing DTOs |
| Email / WhatsApp notifications | Notification service interface + provider implementation; trigger after inquiry creation |
| Reviews | `Review` entity linked to hotels/packages + moderation in admin |
| Multilingual (Urdu/Arabic) | Next.js i18n routing + translated content fields |
