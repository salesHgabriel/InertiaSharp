# ─── Stage 1: Build frontend ──────────────────────────────────────────────────
FROM node:22-alpine AS frontend-build
WORKDIR /app/ClientApp

COPY sample/InertiaSharp.Sample/ClientApp/package*.json ./
RUN npm ci

COPY sample/InertiaSharp.Sample/ClientApp/ ./
RUN npm run build
# Output: /app/ClientApp (files go to ../../wwwroot/dist via vite.config.ts)

# ─── Stage 2: Build .NET app ───────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src

# Restore dependencies (cached layer)
COPY InertiaSharp.sln .
COPY src/InertiaSharp/InertiaSharp.csproj src/InertiaSharp/
COPY sample/InertiaSharp.Sample/InertiaSharp.Sample.csproj sample/InertiaSharp.Sample/
RUN dotnet restore InertiaSharp.sln

# Copy source and built frontend assets
COPY src/ src/
COPY sample/ sample/
COPY --from=frontend-build /app/wwwroot/dist/ sample/InertiaSharp.Sample/wwwroot/dist/

# Publish (Release, self-contained)
RUN dotnet publish sample/InertiaSharp.Sample/InertiaSharp.Sample.csproj \
    -c Release \
    -o /publish \
    --no-build-dependencies \
    --no-restore \
    /p:SkipFrontendBuild=true   # We already built the frontend above

# ─── Stage 3: Runtime image ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN useradd -m appuser && chown -R appuser /app
USER appuser

COPY --from=backend-build --chown=appuser /publish .

# SQLite database directory (mount a volume in production for persistence)
RUN mkdir -p /data
ENV ConnectionStrings__Default="Data Source=/data/app.db"

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "InertiaSharp.Sample.dll"]
