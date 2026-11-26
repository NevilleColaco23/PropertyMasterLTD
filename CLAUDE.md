# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Property Master: a property/hotel management system (bookings, rooms, guests, dashboards, social feed). The backend is .NET (Clean Architecture + CQRS via MediatR) on MongoDB, and the frontend is an Angular 21 standalone-component SPA (SSR files are scaffolded but not enabled; see Frontend architecture). Async work goes through Azure Service Bus to Azure Functions and a worker.

## Commands

Solution: `testAngularAPI.sln` (repo root).

```bash
# Backend API (netexi6.0). Local dev listens on https://localhost:44346 (IIS Express profile) or the port in $PORT (default 5000)
dotnet build WebApi/WebApi.csproj
dotnet run --project WebApi

# Tests (xUnit + Moq, net8.0). They reference WebApi/WebApi.csproj
dotnet test APIUnitProject.Tests
dotnet test APIUnitProject.Tests --filter "FullyQualifiedName~BookingsControllerTests"
dotnet test APIUnitProject.Tests --filter "FullyQualifiedName~BookingsControllerTests.GetBookings_ReturnsOkWithMediatorResult"

# Background services (net8.0)
dotnet run --project EmailWorker
# AccessLogFunction / ActivityLogFunction are Azure Functions; run them with `func start` from their folder

# Frontend (run from app/)
npm install
npm start          # ng serve
npm run build      # ng build
npm test           # ng test (Karma/Jasmine)
```

The dev frontend calls `https://localhost:44346/api/v1` (`app/src/app/environments/environment.ts`).

## Repository layout gotchas

- **The API project is `WebApi/` at the repo root.** `classfiles/` holds the shared libraries (Domain, Application, Infrastructure, MongoDBBackend, SampleData).
- `testAngularAPI.Server/` is a separate, older net8.0 server project. `AccessLogWorker/` has only build output; access logs are now handled by `AccessLogFunction`.
- The many `*.md` files and `*.js` files at the root are historical implementation notes and ad-hoc MongoDB shell scripts (seeding, data fixes, debugging). They are not part of the build. `scripts/` and `Database/Scripts/` hold more mongo shell scripts.
- C# namespaces use the legacy `MyWarehouse.*` prefix (e.g. `MyWarehouse.Application.Bookings.Commands`), even though the product is Property Master.

## Backend architecture

Project dependency chain (everything is in `classfiles/` except `WebApi/`, `Messaging.Shared/` and the workers, which are at the repo root):
`Domain` ← `MongoDBBackend` (generic Mongo repository/UoW/specification) ← `Application` ← `Infrastructure` ← `WebApi`. `SampleData` feeds Infrastructure seeding. `Messaging.Shared` (multi-target net6/net8) holds the Service Bus/RabbitMQ contracts, event models (`UserActivityEvent`, `EmailQueuedEvent`) and queue names, and every service shares it.

- **Controllers** (`WebApi/API/V1/*Controller.cs`) are thin. They inject `IMediator`, use the route `api/v{v:apiversion}/<Entity>` with `[ApiVersion("1.0")]`, and send commands/queries. Action names live in the route (e.g. `Bookings/GetBookings`, `Bookings/CreateBooking`).
- **Application** is organised by feature folder (`Bookings/Commands`, `Property`, `Partners`, `Posts`, `Groups`, `Dashboard`, `UserActivity`, …). Each command has a `XCommand` + `XCommandHandler` (+ `XRequest` DTO) next to it. Shared pieces (pipeline behaviors, mapping, repository interfaces under `Common/Dependencies/DataAccess`) live in `Application/Common`. Mongo collection names are constants in `Application/MongoCollections.cs`.
- **Infrastructure** registers everything via the `AddMy*` / `UseMy*` extension methods called from `WebApi/Startup.cs` (auth, Swagger, versioning, CORS, error handling, persistence, application dependencies). Configuration flows through the static `Config` wrapper (`Config.Init(configuration)`) plus `AddMyInfrastructureConfiguration` (Azure Key Vault, user secrets). MongoDB uses `IgnoreExtraElementsConvention` globally, and the database name comes from `AppSettings:MongoDbDatabaseName` (default `ListingDB`).
- **Activity logging**: controller actions carry attributes such as `[LogList("Bookings")]` / `[LogCreate(...)]` / `[ActivityLog(...)]` (`Application/UserActivity/Attributes`). `Infrastructure/Filters/ActivityLoggingActionFilter.cs` turns them into `UserActivityEvent`s. These go to Azure Service Bus (consumed by `ActivityLogFunction` / `AccessLogFunction`), or are written straight to MongoDB when Service Bus isn't configured (local dev). Human-readable messages are built by `ActivityDisplayMessageBuilder` / `ActivityMessageTemplates`.
- **Email**: `EmailQueueService` publishes `EmailQueuedEvent` to the `email-queue` Service Bus queue. `EmailWorker` consumes it and sends via Resend or SMTP.
- The SignalR hub is at `/chatHub` and the health check at `/health`.
- **Tests** in `APIUnitProject.Tests` construct controllers or handlers directly with a mocked `IMediator`/repositories (`ControllersTest/`, `ApplicationTests/`, `DomainTests/`).

## Frontend architecture (`app/`)

- Standalone-component Angular 21 (no `standalone: false` components; two unused legacy `*-routing.module.ts` files remain). SSR is scaffolded (`server.ts`, `main.server.ts`, `app.routes.server.ts`) but **not enabled**: `angular.json` has no `server`/`ssr` option, so `npm run build` makes a client-only SPA in `dist/propertyMasterV3.0/browser` (deployed to Azure Static Web Apps) and the `serve:ssr:*` script has nothing to run. Uses Angular Material, angular-gridster2 for dashboard widgets, and chart.js/ng2-charts.
- Routing: `app.routes.ts` holds only the auth/landing entry routes (login, `propertySelector`, `create-user`, `activate`). **Every feature route must be nested as a child of `propertyLanding`** in `property/property-landing/property-landing.routes.ts`. That keeps the `PropertyLandingComponent` nav shell mounted and avoids menu re-fetching and flicker. Legacy top-level URLs redirect into `propertyLanding/...`.
- HTTP interceptors in `core/interceptors/`: `property-context.interceptor.ts` adds `X-Selected-Property` from `localStorage.selectedPropertyIds` to every request, and the others handle loading state and activity messages. Auth (JWT + Google) and guards live in `core/auth/`.
- Services build URLs from `environment.apiUrl`, which already includes `/api/v1` (e.g. `${environment.apiUrl}/Bookings`).
- The app shell menu is data-driven from MongoDB (`Menus` / `MenuPermissions` collections), so a new page usually also needs a menu document and permissions. The root `Add-*-MenuItem.js` / `Copy-MenuPermissions-ForUser.js` scripts are the pattern to follow.
- UI convention: social/feed features (`profile/`, team feed) should follow LinkedIn's visual style: card-based posts, distinct spacing, comment sections and stat rows, not flat continuous tiles.

## Deployment

All GitHub Actions workflows (`.github/workflows/`) trigger on pushes to the **`Azure_web_App_Deployment`** branch, not `master`. They deploy the API to Azure App Service (`dotnet publish WebApi -f net6.0`), the Angular app to Azure Static Web Apps, and ActivityLogFunction to Azure Functions, and they build/push the Docker image. There are also Railway configs (`WebApi/railway.json`, root `Dockerfile`) and Vercel configs (`vercel.json`).


## Working rules

- After any backend change, run `dotnet build WebApi/WebApi.csproj`; after changing handlers or controllers, also run the relevant tests in `APIUnitProject.Tests`.
- After frontend changes, run `npm run build` from `app/`.
- Never push to `Azure_web_App_Deployment` — it triggers production deployment. Ask first.
- Never put secrets, connection strings or API keys in code or appsettings files; they come from Azure Key Vault / user secrets.
- Keep changes small and focused on the task; don't refactor or rename unrelated code.
- New backend features follow the existing pattern: thin controller → MediatR command/query + handler in the matching `Application/<Feature>` folder → unit test.
- New pages need a route under `propertyLanding` plus a menu document and permissions (see the `Add-*-MenuItem.js` scripts).


