# Rubino Digital — Web Development Agency

A complete, production-ready starter for Francesco Rubino's web development
agency in London: visitors build their own quote online — pick a project,
add features, and watch the price change live — then send it as an enquiry.

**Built fresh from scratch for this agency in .NET 10 + Angular 18.**
No code was copied from any other project. Every library used is free and
open source (see "Free-only licences" below) — no paid APIs, fonts,
templates, 3D assets or services anywhere in this repo.

## Pricing (inc. VAT — the single source of truth is `pricing.json`)

| Project type | From | Typical timeline |
|---|---|---|
| Template | £349 | 1 week |
| Landing Page | £499 | 1 week |
| Business Website | £799 | 2 weeks |
| E-commerce | £1,499 | 4 weeks |
| Web App | £2,999 | 6 weeks |
| Custom Software | £4,999 | 8 weeks |

- Extra pages: **£120 each**
- Delivery: Standard (no extra charge), Fast **+15%**, Rush **+30%**
- **Bundle discount: 10% off add-ons when 4 or more add-ons are picked**
- Deposit: **30%** to start, balance 70% on delivery
- Monthly care plans (optional, not in the one-off total): Essential **£49/mo**,
  Growth **£99/mo**, Premium **£199/mo**

17 priced add-ons (£120–£800), including e-commerce/payments, booking,
CMS/blog, SEO, branding, 3D animation, accounts/login, admin dashboard,
Stripe, mobile app version, copywriting, analytics, hosting, multilingual,
live chat, integrations and accessibility/performance.

**Worked example (also covered by a test):** Business Website £799 + 2 extra
pages × £120 = **£1,039** total, £311.70 deposit, £727.30 balance, 3 weeks.

## What is in the repo

```
rubino-digital-dotnet-angular/
├── README.md  DEPLOY.md  TESTING.md  SECURITY.md  pricing.json  .env.example
├── Dockerfile  render.yaml  LICENSE (MIT)  RubinoDigital.sln
├── .github/workflows/ci.yml  .github/workflows/deploy-azure.yml
├── backend/                      # ASP.NET Core 10 API (RubinoDigital.Api)
│   ├── RubinoDigital.Api.csproj  # net10.0, EF Core SQLite, JWT, BCrypt
│   ├── Program.cs                # hosting, JWT, CORS, rate limiting, /health, wwwroot
│   ├── appsettings.json / .Development.json / .Production.json
│   ├── Models/  Data/  Services/  Controllers/  Dtos/  Middleware/
│   └── Services: PricingService (the pricing engine), AuthService,
│       EmailService (Log default; Resend/SendGrid free tiers only),
│       PaymentService (TEST-MODE STUB — no real payments)
├── frontend/                     # Angular 18, standalone components
│   ├── package.json  angular.json  tsconfig.json  proxy.conf.json
│   └── src/app/ models/ services/ guards/
│       components: landing, quote-builder, auth, dashboard
└── tests/RubinoDigital.Tests/    # xUnit pricing-engine tests
```

## 3D / cinematic design — no 3D library

The hero uses **pure CSS 3D transforms** (`perspective`, `transform-style:
preserve-3d`, keyframe rotation) plus scroll-reveal CSS animation. There is
**no Three.js, no WebGL library, no downloaded 3D asset and no font
download** — system fonts only. That means: no 3D licence to check, no
extra dependency to go vulnerable, nothing to pay for, and the Angular
build cannot break because of a 3D library. See `SECURITY.md` for the
full licence table.

## Free-only licences

| Dependency | Version | Licence | Cost |
|---|---|---|---|
| ASP.NET Core / EF Core SQLite / JwtBearer / OpenApi | 10.0.0 | MIT | Free |
| BCrypt.Net-Next | 4.0.3 | MIT | Free |
| Swashbuckle.AspNetCore | 7.2.0 | MIT | Free |
| Angular 18, zone.js | 18.2.0 / 0.14.10 | MIT | Free |
| RxJS | 7.8.0 | Apache-2.0 | Free |
| tslib | 2.3.0 | BSD-0-Clause | Free |
| TypeScript | 5.5.2 | Apache-2.0 | Free |
| xUnit + Microsoft.NET.Test.Sdk | 2.9.2 / 17.11.1 | Apache-2.0 / MIT | Free |
| Node 20-alpine, dotnet/sdk:10.0, dotnet/aspnet:10.0 (Docker) | — | MIT / Apache-2.0 | Free |

Email: Log provider by default (sends nothing). Resend / SendGrid are
**optional free tiers only** (100 emails/day). Payments: test-mode stub only.
Hosting: free tiers only — see `DEPLOY.md`.

## How to run locally (all free, all test mode)

Prerequisites: **.NET 10 SDK** (`dotnet --version` → 10.x.x), **Node.js 20+**,
Angular CLI 18 (`npm install -g @angular/cli@18`).

```bash
cp .env.example .env        # placeholders only — .env is gitignored
cd backend && dotnet restore && dotnet run    # API: http://localhost:5000
cd ../frontend && npm install && npm start   # App: http://localhost:4200
# Demo login: demo@rubinodigital.example / Demo123!
# Health: GET http://localhost:5000/health
```

> This build machine had Node but **no .NET SDK**, so the code was written
> for net10.0 and checked by careful reading (imports, paths, pricing
> numbers, appsettings keys) — it was **not compiled here**. Run
> `dotnet build` + `dotnet test` + `npm run build` on a machine with the
> SDK; CI does exactly that on every push (see `.github/workflows/ci.yml`).

## API endpoints

| Method | Route | Purpose |
|---|---|---|
| POST | `/api/auth/register`, `/login`, `/refresh`, `/logout` | Auth (15-min access + 7-day refresh) |
| POST | `/api/auth/verify-email`, `/forgot-password`, `/reset-password` | Email verification / reset |
| GET | `/api/auth/me` | Current user (auth required) |
| POST | `/api/quotes/calculate` | Server-side price (single source of truth) |
| POST | `/api/quotes` | Save a quote, returns reference e.g. `RD-2026-123456` |
| GET | `/api/quotes/{ref}` | Get a saved quote |
| GET | `/api/quotes/mine` | My saved quotes (auth required) |
| POST | `/api/enquiries` | Contact / quote enquiry (validated, stored) |
| GET | `/api/enquiries/mine` | My enquiries (auth required) |
| GET | `/api/portfolio`, `/api/products`, `/api/pricing` | Portfolio, ready-made products, pricing |
| POST | `/api/payments/checkout` | **Test-mode stub** — returns test info, takes no payment |
| GET | `/health`, `/api/health` | Health checks |

## Secrets — the rule

No real secret is in this repo. All secrets come from environment variables
/ `dotnet user-secrets` in dev (`Jwt__Key`, `Stripe__SecretKey`,
`Email__ApiKey`). `appsettings.json` and `appsettings.Production.json`
contain placeholders / empty strings only. The enquiry recipient is
configurable via `Agency__Email` and is **blank by default** — no business
email was invented. Never log a secret.

## Deploy free — see DEPLOY.md

Ranked: **1) Azure App Service Free F1**, **2) Render Free**,
**3) Split: Angular on GitHub Pages / Vercel (free) + API on Azure/Render
(free)**. No paid hosting and no paid add-ons anywhere in this guide.

## Testing — see TESTING.md and SECURITY.md

`dotnet test` covers the pricing engine, including the £799 + 2 pages =
£1,039 check, the 4+ add-on bundle discount, Fast +15%, and the 30%
deposit. `SECURITY.md` lists the dependency/licence audit and the
security measures in the code.
