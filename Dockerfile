# Multi-stage build: Angular -> .NET 10 SDK -> ASP.NET runtime.
# One image, one app: the Angular production build is served from wwwroot
# by the API, so this deploys anywhere that runs a container (Render Free,
# Azure Container Apps, a VPS) with no separate frontend hosting needed.
# All base images are free public images; no paid image or service is used.

# Stage 1 — build the Angular app
FROM node:20-alpine AS frontend
WORKDIR /src/frontend
COPY frontend/package.json frontend/package-lock.json* ./
RUN npm ci --no-audit --no-fund || npm install --no-audit --no-fund
COPY frontend/ ./
RUN npm run build

# Stage 2 — build the .NET 10 API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend
WORKDIR /src
COPY backend/*.csproj ./backend/
RUN dotnet restore backend/RubinoDigital.Api.csproj
COPY backend/ ./backend/
COPY pricing.json ./
# Copy the Angular build into wwwroot so the API serves it
RUN mkdir -p backend/wwwroot && cp -r frontend/dist/rubino-digital-web/browser/* backend/wwwroot/ 2>/dev/null || cp -r /src/frontend/dist/*/browser/* backend/wwwroot/ || true
WORKDIR /src/backend
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 3 — runtime only
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=backend /app/publish ./
# Render / Fly.io set $PORT; the app reads it in Program.cs.
EXPOSE 8080
ENTRYPOINT ["dotnet", "RubinoDigital.Api.dll"]
