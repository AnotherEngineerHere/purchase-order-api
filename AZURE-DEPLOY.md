# Guía de despliegue en Azure

Esta guía cubre el despliegue de la API usando **Azure Container Registry (ACR)** +
**Azure App Service** (contenedor Docker).

---

## Prerrequisitos

- Cuenta de Azure activa
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) instalado
- Docker instalado y corriendo
- Haber hecho `az login`

```bash
az login
```

---

## Paso 1 — Crear el Resource Group

```bash
az group create \
  --name rg-purchase-order \
  --location eastus
```

---

## Paso 2 — Crear Azure Container Registry (ACR)

```bash
az acr create \
  --resource-group rg-purchase-order \
  --name purchaseorderacr \
  --sku Basic \
  --admin-enabled true
```

Obtener las credenciales del registry:

```bash
az acr credential show --name purchaseorderacr
# Anota el username y password
```

---

## Paso 3 — Build y push de la imagen

```bash
# Iniciar sesión en ACR
az acr login --name purchaseorderacr

# Build de la imagen (desde la carpeta que contiene el Dockerfile)
docker build -t purchaseorderacr.azurecr.io/purchase-order-api:latest .

# Push al registry
docker push purchaseorderacr.azurecr.io/purchase-order-api:latest
```

> El build ejecuta los tests automáticamente. Si algún test falla,
> el build se detiene y no se genera la imagen.

---

## Paso 4 — Crear App Service Plan

```bash
az appservice plan create \
  --name plan-purchase-order \
  --resource-group rg-purchase-order \
  --sku B1 \
  --is-linux
```

---

## Paso 5 — Crear el Web App (con contenedor)

```bash
az webapp create \
  --resource-group rg-purchase-order \
  --plan plan-purchase-order \
  --name purchase-order-api \
  --deployment-container-image-name purchaseorderacr.azurecr.io/purchase-order-api:latest
```

---

## Paso 6 — Configurar credenciales de ACR en el Web App

```bash
az webapp config container set \
  --name purchase-order-api \
  --resource-group rg-purchase-order \
  --docker-custom-image-name purchaseorderacr.azurecr.io/purchase-order-api:latest \
  --docker-registry-server-url https://purchaseorderacr.azurecr.io \
  --docker-registry-server-user <ACR_USERNAME> \
  --docker-registry-server-password <ACR_PASSWORD>
```

---

## Paso 7 — Configurar variables de entorno

```bash
az webapp config appsettings set \
  --name purchase-order-api \
  --resource-group rg-purchase-order \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:5000 \
    ConnectionStrings__DefaultConnection="Data Source=/app/data/purchaseorders.db" \
    JwtSettings__SecretKey="<TU-SECRET-KEY-SEGURA-DE-32-CHARS>" \
    JwtSettings__Issuer="PurchaseOrderApi" \
    JwtSettings__Audience="PurchaseOrderApi" \
    JwtSettings__ExpiresInHours="8"
```

---

## Paso 8 — Configurar el puerto

Azure App Service espera que la app escuche en el puerto `8080` por defecto,
o bien configurar el puerto manualmente:

```bash
az webapp config appsettings set \
  --name purchase-order-api \
  --resource-group rg-purchase-order \
  --settings WEBSITES_PORT=5000
```

---

## Paso 9 — Verificar el despliegue

```bash
# Ver URL de la app
az webapp show \
  --name purchase-order-api \
  --resource-group rg-purchase-order \
  --query defaultHostName \
  --output tsv
```

La API estará disponible en:
```
https://purchase-order-api.azurewebsites.net
https://purchase-order-api.azurewebsites.net/swagger
```

Ver logs en tiempo real:

```bash
az webapp log tail \
  --name purchase-order-api \
  --resource-group rg-purchase-order
```

---

## Persistencia de la base de datos (SQLite)

SQLite guarda el archivo `.db` dentro del contenedor. Para que persista entre reinicios,
monta un **Azure File Share** como volumen:

```bash
# 1. Crear Storage Account
az storage account create \
  --name purchaseorderstorage \
  --resource-group rg-purchase-order \
  --sku Standard_LRS

# 2. Crear File Share
az storage share create \
  --name dbdata \
  --account-name purchaseorderstorage

# 3. Obtener la key
STORAGE_KEY=$(az storage account keys list \
  --account-name purchaseorderstorage \
  --query "[0].value" --output tsv)

# 4. Montar el share en el Web App
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

---

## Actualizar la imagen (nuevo deploy)

```bash
docker build -t purchaseorderacr.azurecr.io/purchase-order-api:latest .
docker push purchaseorderacr.azurecr.io/purchase-order-api:latest

# Forzar reinicio para que tome la nueva imagen
az webapp restart \
  --name purchase-order-api \
  --resource-group rg-purchase-order
```

---

## Limpiar recursos

```bash
az group delete --name rg-purchase-order --yes --no-wait
```

---

## Resumen de recursos creados

| Recurso | Nombre | Descripción |
|---|---|---|
| Resource Group | `rg-purchase-order` | Contenedor de todos los recursos |
| Container Registry | `purchaseorderacr` | Almacena la imagen Docker |
| App Service Plan | `plan-purchase-order` | Plan B1 Linux |
| Web App | `purchase-order-api` | App corriendo el contenedor |
| Storage Account | `purchaseorderstorage` | Persistencia del archivo SQLite |
