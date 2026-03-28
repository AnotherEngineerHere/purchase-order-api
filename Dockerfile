# ── Build stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore — separate layer so it's cached when only source changes
COPY PurchaseOrderApi/PurchaseOrderApi.csproj           PurchaseOrderApi/
COPY PurchaseOrderApi.Tests/PurchaseOrderApi.Tests.csproj PurchaseOrderApi.Tests/
RUN dotnet restore "PurchaseOrderApi/PurchaseOrderApi.csproj"
RUN dotnet restore "PurchaseOrderApi.Tests/PurchaseOrderApi.Tests.csproj"

# Copy full source
COPY PurchaseOrderApi/       PurchaseOrderApi/
COPY PurchaseOrderApi.Tests/ PurchaseOrderApi.Tests/

# ── Test stage ────────────────────────────────────────────────────────────────
FROM build AS test
WORKDIR /src
RUN dotnet test "PurchaseOrderApi.Tests/PurchaseOrderApi.Tests.csproj" \
    --no-restore \
    --verbosity normal

# ── Publish stage ─────────────────────────────────────────────────────────────
FROM test AS publish
RUN dotnet publish "PurchaseOrderApi/PurchaseOrderApi.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Directory for the SQLite database file — mount a volume here to persist data
RUN mkdir -p /app/data

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/purchaseorders.db"

EXPOSE 5000

# On startup: EF migrations run automatically via db.Database.Migrate() in Program.cs
ENTRYPOINT ["dotnet", "PurchaseOrderApi.dll"]
