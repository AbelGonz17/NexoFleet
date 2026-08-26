# NexoFleet

NexoFleet es una aplicación multitenant para gestionar empresas de transporte, empleados, vehículos, rutas recurrentes, viajes y reportes de pago.

## Tecnologías

- Backend: ASP.NET Core 10, Clean Architecture y dominio rico.
- Persistencia: Entity Framework Core y PostgreSQL.
- Frontend: Vue 3, TypeScript, Vue Router, Pinia y `fetch`.
- Pruebas: xUnit en backend.

## Estructura

```text
backend/
  src/
    NexoFleet.Domain/
    NexoFleet.Application/
    NexoFleet.Infrastructure/
    NexoFleet.Api/
  tests/
frontend/
docker-compose.yml
NexoFleet.sln
```

## Requisitos

- .NET SDK 10.0.203 o compatible.
- Node.js 24.11 o superior.
- Docker Desktop para PostgreSQL.

## Desarrollo local

1. Copiar `.env.example` a `.env` y ajustar las credenciales locales de PostgreSQL.
2. Iniciar PostgreSQL:

   ```bash
   docker compose up -d postgres
   ```

3. Restaurar y ejecutar el backend:

   ```bash
   dotnet restore NexoFleet.sln --configfile NuGet.Config
   dotnet run --project backend/src/NexoFleet.Api
   ```

4. Instalar y ejecutar el frontend:

   ```bash
   cd frontend
   npm install
   npm run dev
   ```

## Forma de trabajo

El proyecto se implementará mediante módulos verticales: dominio, Application, Infrastructure, API, pruebas y frontend se completarán juntos antes de avanzar al siguiente módulo.

## Módulo 1: autenticación

El acceso utiliza una cookie de sesión segura y `HttpOnly`. La API expone:

- `POST /api/v1/auth/login`
- `GET /api/v1/auth/me`
- `POST /api/v1/auth/logout`

Antes del primer inicio, aplica la migración:

```bash
dotnet ef database update --project backend/src/NexoFleet.Infrastructure --startup-project backend/src/NexoFleet.Api
```

El primer SuperAdmin se configura con secretos locales, que no se guardan en Git:

```bash
dotnet user-secrets set "BootstrapSuperAdmin:Enabled" "true" --project backend/src/NexoFleet.Api
dotnet user-secrets set "BootstrapSuperAdmin:Email" "tu-correo@empresa.com" --project backend/src/NexoFleet.Api
dotnet user-secrets set "BootstrapSuperAdmin:Password" "una-contraseña-segura" --project backend/src/NexoFleet.Api
dotnet user-secrets set "BootstrapSuperAdmin:FirstName" "Tu nombre" --project backend/src/NexoFleet.Api
dotnet user-secrets set "BootstrapSuperAdmin:LastName" "Tu apellido" --project backend/src/NexoFleet.Api
```

Ejecuta la API una vez y luego desactiva la creación inicial:

```bash
dotnet user-secrets set "BootstrapSuperAdmin:Enabled" "false" --project backend/src/NexoFleet.Api
```

## Documentación de la API

En el entorno `Development`, Swagger UI está disponible en:

```text
https://localhost:7034/swagger
```

El documento OpenAPI puede consultarse en:

```text
https://localhost:7034/swagger/v1/swagger.json
```

Para probar el inicio de sesión desde Swagger:

1. Ejecuta `GET /api/v1/auth/csrf`.
2. Copia el valor `token` de la respuesta.
3. Ejecuta `POST /api/v1/auth/login` y envíalo en el encabezado `X-XSRF-TOKEN`.

## Patrón Result

Los casos de uso devuelven `Result` o `Result<T>` y no utilizan excepciones para representar errores esperados. Cada error posee un código estable, descripción y tipo. La API transforma esos tipos en respuestas HTTP uniformes mediante `ProblemDetails`:

- `Validation` → 400.
- `Unauthorized` → 401.
- `Forbidden` → 403.
- `NotFound` → 404.
- `Conflict` → 409.
- `Locked` → 423.
- `Failure` → 500.
