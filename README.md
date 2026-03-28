# Purchase Order API

REST API built with **.NET 8** for managing a simple purchase order system (Products, Customers, Orders).

## Tech Stack

- .NET 8 Web API
- Entity Framework Core 8 + SQLite
- AutoMapper
- JWT Authentication
- Swagger / OpenAPI
- xUnit + Moq (unit tests)
- Docker

---

## Run locally

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [dotnet-ef CLI tool](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

```bash
dotnet tool install --global dotnet-ef
```

### Steps

```bash
# 1. Restore packages
cd PurchaseOrderApi
dotnet restore

# 2. Create and apply migrations
dotnet ef migrations add InitialCreate --project PurchaseOrderApi
dotnet ef database update --project PurchaseOrderApi

# 3. Run
dotnet run --project PurchaseOrderApi
```

API available at: `http://localhost:5000`
Swagger UI: `http://localhost:5000/swagger`

---

## Run with Docker

The Docker build runs all unit tests automatically. If any test fails, the image is not generated.

```bash
# Build image (runs tests during build)
docker build -t purchase-order-api .

# Run (SQLite persisted in a named volume)
docker run -p 5000:5000 -v purchase-order-data:/app/data purchase-order-api
```

Migrations are applied automatically on container startup.

To skip tests during local development:

```bash
docker build --target publish -t purchase-order-api .
```

---

## Unit Tests

26 tests covering Products, Customers and Orders service logic.

```bash
# Run from solution root
cd PurchaseOrderApi
dotnet test PurchaseOrderApi.Tests
```

Or from Visual Studio: **Test → Test Explorer → Run All**.

### Test coverage

| Service | Tests | Cases |
|---|---|---|
| `ProductService` | 8 | GetAll, GetById, Create, Update, Delete (found/not found) |
| `CustomerService` | 6 | GetAll, GetById, Create (found/not found) |
| `OrderService` | 12 | GetAll, GetById, customer not found, product not found, insufficient stock, total calculation, stock reduction |

---

## Migrations

```bash
# Create a new migration
dotnet ef migrations add <MigrationName> --project PurchaseOrderApi

# Apply migrations
dotnet ef database update --project PurchaseOrderApi

# Rollback last migration
dotnet ef migrations remove --project PurchaseOrderApi
```

---

## Authentication

All endpoints except `POST /api/auth/login` require a JWT Bearer token.

**Demo credentials:**

| Field    | Value      |
|----------|------------|
| username | `admin`    |
| password | `admin123` |

**Get a token:**

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

Use the returned token in subsequent requests:

```
Authorization: Bearer <token>
```

In Swagger UI, click **Authorize** and enter `<token>` (without "Bearer ").

---

## Endpoints

### Auth
| Method | Endpoint            | Description         | Auth |
|--------|---------------------|---------------------|------|
| POST   | /api/auth/login     | Get JWT token       | No   |

### Products
| Method | Endpoint              | Description           | Auth |
|--------|-----------------------|-----------------------|------|
| GET    | /api/products         | List all products     | Yes  |
| GET    | /api/products/{id}    | Get product by ID     | Yes  |
| POST   | /api/products         | Create product        | Yes  |
| PUT    | /api/products/{id}    | Update product        | Yes  |
| DELETE | /api/products/{id}    | Delete product        | Yes  |

### Customers
| Method | Endpoint              | Description           | Auth |
|--------|-----------------------|-----------------------|------|
| GET    | /api/customers        | List all customers    | Yes  |
| GET    | /api/customers/{id}   | Get customer by ID    | Yes  |
| POST   | /api/customers        | Create customer       | Yes  |

### Orders
| Method | Endpoint              | Description                              | Auth |
|--------|-----------------------|------------------------------------------|------|
| GET    | /api/orders           | List all orders                          | Yes  |
| GET    | /api/orders/{id}      | Get order detail                         | Yes  |
| POST   | /api/orders           | Create order (validates & reduces stock) | Yes  |

---

## Business Rules

- Creating an order validates that each product has sufficient stock.
- Order total is calculated automatically (`quantity × unit price`).
- Product stock is reduced immediately when the order is created.

---

## Deploy to Azure

### Prerequisites

- Azure CLI installed and logged in: `az login`
- Docker running

### Step 1 — Create Resource Group

```bash
az group create \
  --name rg-purchase-order \
  --location eastus
```

### Step 2 — Create Azure Container Registry (ACR)

```bash
az acr create \
  --resource-group rg-purchase-order \
  --name purchaseorderacr \
  --sku Basic \
  --admin-enabled true

# Get credentials
az acr credential show --name purchaseorderacr
```

### Step 3 — Build and push the image

```bash
az acr login --name purchaseorderacr

docker build -t purchaseorderacr.azurecr.io/purchase-order-api:latest .
docker push purchaseorderacr.azurecr.io/purchase-order-api:latest
```

> The build runs all unit tests. If any test fails, the push is aborted.

### Step 4 — Create App Service Plan

```bash
az appservice plan create \
  --name plan-purchase-order \
  --resource-group rg-purchase-order \
  --sku B1 \
  --is-linux
```

### Step 5 — Create Web App

```bash
az webapp create \
  --resource-group rg-purchase-order \
  --plan plan-purchase-order \
  --name purchase-order-api \
  --deployment-container-image-name purchaseorderacr.azurecr.io/purchase-order-api:latest
```

### Step 6 — Configure ACR credentials

```bash
az webapp config container set \
  --name purchase-order-api \
  --resource-group rg-purchase-order \
  --docker-custom-image-name purchaseorderacr.azurecr.io/purchase-order-api:latest \
  --docker-registry-server-url https://purchaseorderacr.azurecr.io \
  --docker-registry-server-user <ACR_USERNAME> \
  --docker-registry-server-password <ACR_PASSWORD>
```

### Step 7 — Set environment variables

```bash
az webapp config appsettings set \
  --name purchase-order-api \
  --resource-group rg-purchase-order \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:5000 \
    WEBSITES_PORT=5000 \
    ConnectionStrings__DefaultConnection="Data Source=/app/data/purchaseorders.db" \
    JwtSettings__SecretKey="<YOUR-SECRET-KEY-32-CHARS-MIN>" \
    JwtSettings__Issuer="PurchaseOrderApi" \
    JwtSettings__Audience="PurchaseOrderApi" \
    JwtSettings__ExpiresInHours="8"
```

### Step 8 — Persist SQLite with Azure File Share

```bash
az storage account create \
  --name purchaseorderstorage \
  --resource-group rg-purchase-order \
  --sku Standard_LRS

az storage share create \
  --name dbdata \
  --account-name purchaseorderstorage

STORAGE_KEY=$(az storage account keys list \
  --account-name purchaseorderstorage \
  --query "[0].value" --output tsv)

az webapp config storage-account add \
  --name purchase-order-api \
  --resource-group rg-purchase-order \
  --custom-id DbStorage \
  --storage-type AzureFiles \
  --share-name dbdata \
  --account-name purchaseorderstorage \
  --access-key $STORAGE_KEY \
  --mount-path /app/data
```

### Step 9 — Verify

```bash
az webapp show \
  --name purchase-order-api \
  --resource-group rg-purchase-order \
  --query defaultHostName \
  --output tsv

# Live logs
az webapp log tail \
  --name purchase-order-api \
  --resource-group rg-purchase-order
```

The API will be available at:
- `https://purchase-order-api.azurewebsites.net`
- `https://purchase-order-api.azurewebsites.net/swagger`

### Redeploy after changes

```bash
docker build -t purchaseorderacr.azurecr.io/purchase-order-api:latest .
docker push purchaseorderacr.azurecr.io/purchase-order-api:latest
az webapp restart --name purchase-order-api --resource-group rg-purchase-order
```

### Tear down

```bash
az group delete --name rg-purchase-order --yes --no-wait
```

### Azure resources summary

| Resource | Name | Description |
|---|---|---|
| Resource Group | `rg-purchase-order` | Container for all resources |
| Container Registry | `purchaseorderacr` | Stores the Docker image |
| App Service Plan | `plan-purchase-order` | B1 Linux plan |
| Web App | `purchase-order-api` | Runs the container |
| Storage Account | `purchaseorderstorage` | Persists the SQLite file |
