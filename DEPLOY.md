# Rubino Digital — free deployment guide

**Free only.** Every option below is a free tier as published at the time
of writing — check the provider's site before you rely on it. No paid
hosting, no paid add-ons, no paid domain is needed to go live (a custom
domain is optional, ~£10/year, and is not required).

## Recommended first: Azure App Service Free (F1) — best for .NET 10

Free limits (F1): **60 minutes of compute per day, 1 GB disk, 1 GB RAM
shared, no custom SSL, no Always On, no SLA.** Enough for a demo and your
first customers; the app sleeps when idle. When 60 min/day starts to bite,
upgrade to **Basic B1 (~£10/month)** — same app, no code change. That
upgrade is optional and is the only paid step in this whole guide.

```bash
# 1. Create the app (Azure CLI, or use the portal)
az webapp create --resource-group rubino-digital-rg --plan rubino-digital-plan \
  --name rubino-digital-demo --runtime "DOTNETCORE:10.0"

# 2. Set secrets as App Settings (never in code / git)
az webapp config appsettings set --name rubino-digital-demo --resource-group rubino-digital-rg \
  --settings Jwt__Key="<long-random-secret-32-plus-chars>" \
             Stripe__SecretKey="sk_test_..." \
             Stripe__PublishableKey="pk_test_..." \
             Email__Provider="Log" \
             Agency__Email="" \
             Frontend__Origin="https://rubino-digital-demo.azurewebsites.net"

# 3. Push to GitHub main — .github/workflows/deploy-azure.yml deploys it.
#    Secrets needed in GitHub: AZURE_WEBAPP_NAME, AZURE_WEBAPP_PUBLISH_PROFILE
```

## 2. Render Free — container, one click

`render.yaml` in this repo defines it. Free tier: **750 hours/month,
512 MB RAM, spins down after 15 min inactivity** (first request is slow),
no persistent disk — SQLite data resets on redeploy, so this is a demo
path only until you add a paid disk or a free Postgres add-on.

```bash
# Push the repo to GitHub, then in render.com: New -> Blueprint -> pick the repo.
# Set the env vars in the Render dashboard (same names as above).
```

## 3. Split (free): Angular on GitHub Pages / Vercel + API free

- Frontend: `ng build` → deploy `frontend/dist/` free on **GitHub Pages**
  or **Vercel** (both free for a static site).
- API: deploy the backend alone to Azure F1 or Render Free (above).
- Set `Frontend__Origin` on the API to the frontend URL, and point the
  Angular app at the API URL. The single-app options (1 and 2) are
  simpler — use this split only if you want the frontend on Pages/Vercel.

## Database: free to start, clear upgrade path

- **Now (free):** SQLite file `rubinodigital.db` — no server, no cost.
  On Render Free it resets on redeploy; on Azure F1 it persists in the
  app's 1 GB disk until you redeploy with a fresh publish.
- **Later:** set `ConnectionStrings__DefaultConnection` to Azure SQL /
  Postgres and change `UseSqlite` → `UseSqlServer` / `UseNpgsql` in
  `Program.cs`. The EF Core models do not change.

## Stripe — free test mode only

Create a **free** Stripe account (no monthly fee), stay in **Test mode**,
use `sk_test_…` / `pk_test_…`. Test card: `4242 4242 4242 4242`, any future
date, any CVC. No real money moves in test mode. Live payments need your
own Stripe account + bank details — see README.md.
