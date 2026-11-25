# Rubino Digital — security & free-licence audit

Checked by reading every file in this repo (no build was run here — see
TESTING.md). Re-check with `dotnet list package --vulnerable` and
`npm audit` on a machine with the SDK before going live.

## 1. Free-only licences — every dependency

| Dependency | Version | Licence | Cost |
|---|---|---|---|
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.0 | MIT | Free |
| Microsoft.AspNetCore.OpenApi | 10.0.0 | MIT | Free |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.0 | MIT | Free |
| Microsoft.EntityFrameworkCore.Design | 10.0.0 | MIT | Free |
| BCrypt.Net-Next | 4.0.3 | MIT | Free |
| Swashbuckle.AspNetCore | 7.2.0 | MIT | Free |
| Angular 18 (@angular/*) | 18.2.0 | MIT | Free |
| zone.js | 0.14.10 | MIT | Free |
| RxJS | 7.8.0 | Apache-2.0 | Free |
| tslib | 2.3.0 | BSD-0-Clause | Free |
| TypeScript | 5.5.2 | Apache-2.0 | Free |
| Microsoft.NET.Test.Sdk | 17.11.1 | MIT | Free |
| xunit / xunit.runner.visualstudio | 2.9.2 / 2.8.2 | Apache-2.0 | Free |

**3D library chosen: none.** The cinematic hero is pure CSS 3D
transforms (`perspective`, `preserve-3d`, keyframes) written in
`frontend/src/styles.css`. No Three.js, no WebGL, no downloaded 3D asset,
no font download (system fonts only). Licence: n/a — no library, nothing
to pay for, nothing to go vulnerable.

**No paid dependency was found:** no Stripe.net package is referenced
(payments are a stub), no paid font/template/asset, no paid API. Email
providers (Resend/SendGrid) are optional free tiers only and are off by
default (Log provider). Hosting is free tiers only (see DEPLOY.md).

## 2. Known-vulnerable packages — what was checked

- All NuGet packages are the current 10.0.0 / latest-stable versions
  published for .NET 10 at the time of writing; no package is pinned to
  an old major version, and no package with a widely-known CVE at that
  version is used. No `Newtonsoft.Json`, no old `System.Text.Json`.
- Angular 18.2.0 / TypeScript 5.5.2 are the current 18.x line; no
  deprecated Angular package is used.
- **Before going live, run:** `dotnet list package --vulnerable` and
  `npm audit` and fix anything they flag — versions move, and this
  static check goes stale.

## 3. Secrets — none in the repo

Checked: `appsettings.json` / `appsettings.Production.json` contain only
placeholders and empty strings; `.env.example` contains placeholders
only; no `sk_live_`, `sk_test_` real key, password or token appears
anywhere. `grep -r "sk_live\|sk_test_[A-Za-z0-9]\{8,\}"` finds only the
placeholder `sk_test_` in docs. `.gitignore` covers `.env`,
`appsettings.Production.json`, `*.db`, `bin/`, `obj/`, `node_modules/`,
`dist/`, `backend/wwwroot/`.

## 4. Security measures in the code

- **Auth:** BCrypt passwords; JWT access 15 min + refresh 7 days, refresh
  tokens stored as SHA-256 hashes and rotated on use; email verification
  and password-reset tokens stored hashed, single-use, time-limited
  (24 h / 1 h); lockout after 5 failed logins (15 min); password rule
  8+ chars with a letter and a number.
- **Input validation:** `[Required]`, `[EmailAddress]`, `[StringLength]`,
  `[Range(0, 50)]` on DTOs; unknown project types, add-ons, delivery
  options and care plans are rejected with 400; extra pages are clamped
  to 0–50; enquiry messages are length-limited.
- **Rate limiting:** auth 5/min, enquiries 10/min per IP (429 on excess).
- **CORS:** locked to the configured `Frontend:Origin` — never
  `AllowAnyOrigin`.
- **Headers:** `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`,
  `Referrer-Policy: strict-origin-when-cross-origin`,
  `Permissions-Policy` (camera/microphone/geolocation off), a basic CSP
  (`default-src 'self'`, no external scripts), HSTS in production.
- **Errors:** global exception middleware returns a generic message and
  never a stack trace or a secret in production; full errors go to the
  server log only.
- **Output encoding:** email templates HTML-encode user-supplied names,
  emails and messages.
- **Production guard:** the app refuses to start in production without a
  32+ character `Jwt:Key`.
- **Payments:** test-mode stub only — no card data is ever collected or
  stored by this code.
