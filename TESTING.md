# Rubino Digital — testing

**Rule: nothing here touches real money or real inboxes until Francesco
swaps in his own free test/live keys (see "Swap to live" at the bottom).
Test keys are placeholders — `sk_test_…`, `pk_test_…` — never a real secret.**

## 0. Prerequisites (on Francesco's machine)

```bash
dotnet --version   # must print 10.x.x — .NET 10 SDK
node --version     # 20+
npm install -g @angular/cli@18
```

This build machine had Node but **no .NET SDK**, so the steps below were
**not** run end-to-end here. They are the exact steps to run on a machine
with the .NET 10 SDK. What *was* verified here is listed in "What was
actually verified here" at the bottom.

## 1. Run it locally (all free / test mode)

```bash
cp .env.example .env
cd backend && dotnet restore && dotnet run        # API on http://localhost:5000
cd ../frontend && npm install && npm start       # Angular on http://localhost:4200
# Health: curl http://localhost:5000/health  -> { "status": "ok", ... }
```

## 2. Pricing — check by hand (must match the tests)

| Case | Expected |
|---|---|
| Business Website £799 + 2 extra pages | **£1,039** total, £311.70 deposit, £727.30 balance, 3 weeks |
| 4 add-ons: CMS £150 + SEO £200 + Analytics £120 + Copywriting £180 | Add-ons £650, bundle discount **£65**, total £1,384 |
| Fast delivery on the £1,039 quote | **£1,194.85** total, £155.85 surcharge |
| Rush delivery on a £499 landing page | **£648.70** total |
| Growth care plan | £99/month, one-off total unchanged |

```bash
dotnet test   # xUnit pricing tests — 8 tests, all must pass
# Or by API:
curl -X POST http://localhost:5000/api/quotes/calculate \
  -H "Content-Type: application/json" \
  -d '{"projectType":"business","extraPages":2,"addOnIds":[],"delivery":"standard"}'
# -> total 1039, deposit 311.70, balance 727.30, timelineWeeks 3
```

## 3. Auth — register / verify / login / refresh / reset

| Flow | How | Success looks like |
|---|---|---|
| Register | UI: Create account, or `POST /api/auth/register` `{fullName, email, password}` (8+ chars, incl. a letter and a number) | 201. Verification email is **logged, not sent** (Email provider = Log). |
| Login | Demo account: `demo@rubinodigital.example` / `Demo123!` | 200 + JWT access token (15 min) + refresh-token cookie. |
| Refresh | `POST /api/auth/refresh` (cookie or body token) | 200 + new access token; old refresh token is revoked (reuse fails). |
| Lockout | 5 wrong passwords in a row | 6th attempt → locked 15 minutes. |
| Forgot / reset | UI or `POST /api/auth/forgot-password` → check API log for the reset link | Reset link works once, expires in 1 hour. |

## 4. Quote builder + enquiries

1. On the home page, pick **Business Website**, tick 4 add-ons, add 2 pages.
   The total must update live and match the server calculation above.
2. Click **Add to quote** on the SMASH template — the builder must load
   Template + Booking + 3D animation.
3. Fill in name/email and **Send enquiry** — you should get a reference
   like `ENQ-2026-123456`, and the quote should be saved (`RD-2026-…`).
4. Log in and open **Dashboard** — your saved quote and enquiry appear.

## 5. Payments — test-mode stub

`POST /api/payments/checkout` with a saved quote reference returns
`mode: "test"`, the 30% deposit amount, `checkoutUrl: null`, and test card
`4242 4242 4242 4242`. **No payment is taken.** That is intentional — see
README.md for how to implement real Stripe Checkout (free test mode).

## Swap to live (free accounts only, no code changes)

| Area | Swap (env-var values only) |
|---|---|
| JWT | Set a new long random `Jwt__Key` in Azure/Render App Settings. |
| Stripe | Free Stripe account → test keys `sk_test_…` / `pk_test_…`; live keys only after you add your bank details. |
| Email | `Email__Provider=Resend` (or SendGrid, free 100/day) + `Email__ApiKey` + verified `Email__From`. |
| Enquiry inbox | Set `Agency__Email` to your business email — blank by default, nothing is invented. |
| Deploy | Azure F1 (free) first, Render Free second — see DEPLOY.md. |

## What was actually verified here

- `pricing.json`, `angular.json`, `tsconfig.json`, `package.json` parse as
  valid JSON; all 6 project types, 17 add-ons and 3 care plans are present
  in `pricing.json`, in the backend `PricingService`, and in the Angular
  pricing model, with identical prices (checked by reading, not by running).
- The £799 + 2×£120 = £1,039 calculation, bundle discount, Fast +15% and
  30% deposit are asserted in the xUnit tests and mirrored in the Angular
  `calculateQuote` function with the same rule order and rounding.
- `dotnet build`, `dotnet test` and `ng build` were **not** run here (no
  .NET SDK on this machine). CI runs all three on every push — a green CI
  run is the proof they pass.
