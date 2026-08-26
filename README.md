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

1. Copiar `.env.example` a `.env` y ajustar las credenciales locales.
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

