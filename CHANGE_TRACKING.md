# Change Tracking

Log of notable changes: commit id, what changed, and the technical reason. Newest first (Entry 3 is the latest).
Add a new entry per commit. For uncommitted work, use `PENDING` and fill in the id after committing.

---

## Entry 5 — Workflows and prod config for the new Terraform-managed Azure environment

- **Commit:** `PENDING` (fill in after commit: `git log -1 --format=%h`)
- **Date:** 2026-11-27
- **Branch:** `introducing_Claude`

### Why
The old workflows authenticated to a deleted subscription (OIDC client/tenant/subscription secrets), pointed at the deleted app
`propertymaster-api-de09`, set Service Bus settings, and deployed a Function App that no longer exists. The old frontend host was also
in the API's CORS list, so a new Static Web App hostname would have been blocked by the browser.

### Changes

| File | Change |
|---|---|
| `.github/workflows/deploy-API.yml` | App name now from the repo variable `AZURE_WEBAPP_NAME`; auth via the `AZURE_WEBAPP_PUBLISH_PROFILE` secret (no OIDC). Removed the "Set App Service configuration secrets" step: Terraform now owns all app settings, so they are not overwritten on each deploy. Removed `ServiceBus__*` settings. |
| `.github/workflows/azure_web_app_deployment_propertymaster-activitylogfunc.yml` | Deleted. The Function App is not part of the new setup (Service Bus dropped). |
| `.github/workflows/deploy-angular-frontend.yml` | Token secret renamed to `AZURE_STATIC_WEB_APPS_API_TOKEN` (was tied to the old `BRAVE_ROCK` app). |
| `terraform/main.tf` | Added app setting `CorsSettings__AllowedOrigins__20` = new Static Web App URL (appends to the list in `appsettings.json`). |
| `terraform/README.md` (new) | Apply steps, the GitHub variable/secrets to set after apply, and how to switch Azure accounts. |
| `app/src/app/environments/environment.prod.ts` | `apiUrl` replaced with a **placeholder**: the real hostname is only known after `terraform apply`. |

### Not done / must do before deploying
- Run `terraform apply`, then set `apiUrl` in `environment.prod.ts` to `<api_url>/api/v1`. Until then the deployed frontend cannot reach the API.
- Create the GitHub variable `AZURE_WEBAPP_NAME` and secrets `AZURE_WEBAPP_PUBLISH_PROFILE` and `AZURE_STATIC_WEB_APPS_API_TOKEN`.
- `docker-build-push.yml` still builds the EmailWorker image (no longer used); left as is.
- Old secrets (`AZUREAPPSERVICE_*`, `AZURE_SERVICE_BUS_CONNECTION_STRING`, `AZURE_CREDENTIALS`) can be deleted from GitHub.
- Hardcoded old hosts remain in `appsettings.json` / `appsettings.Production.json` / `UserService.cs` (`brave-rock-...azurestaticapps.net`); the
  Terraform app settings override `FrontendSettings__BaseUrl` and add the new CORS origin.
- `appsettings.json` contains a demo `GuestSettings:Password` in plain text; consider moving it to secrets.

### Verification
- `terraform fmt` + `terraform validate`: success. `npm run build` (app): success. Workflows not run.

---

## Entry 4 — Send email from inside the API (no Service Bus / EmailWorker)

- **Commit:** `PENDING` (fill in after commit: `git log -1 --format=%h`)
- **Date:** 2026-11-27
- **Branch:** `introducing_Claude`

### Why
Service Bus is dropped in the new Azure setup (Entry 3). Account-activation emails used to be saved to the MongoDB
`EmailOutbox` collection and then published to the `email-queue` Service Bus queue for `EmailWorker`. With no queue,
nothing sent the email. Emails are already persisted in MongoDB first, so the API now sends them itself.

### Flow
`UserService` -> `EmailQueueService.QueueEmailAsync` (writes outbox row, status Queued) -> `IEmailOutboxSignal.Notify()`
-> `EmailOutboxProcessor` (hosted service in WebApi) claims the row -> `ResendHttpEmailSender` calls the Resend REST API.
The processor also polls every 30 s as a safety net.

### Changes

| File | Change |
|---|---|
| `classfiles/Infrastructure/Services/EmailQueueService.cs` | Removed the `IServiceBusPublisher` dependency and publish; now calls `IEmailOutboxSignal.Notify()`. Constructor signature changed. |
| `classfiles/Infrastructure/Services/EmailOutboxSignal.cs` (new) | `IEmailOutboxSignal` / `EmailOutboxSignal` (SemaphoreSlim) to wake the processor immediately. |
| `WebApi/Email/EmailOutboxProcessor.cs` (new) | `BackgroundService`. Atomically claims one due row with `FindOneAndUpdate` (Status Pending/Queued, `NextRunAtUtc <= now`, `Attempts < 5`; also recovers rows stuck in Processing after the 2 min lock), sends, then sets Sent, or re-queues with exponential backoff (30 s x 2^n), or marks Failed after 5 attempts. If `Resend:ApiKey` is empty it leaves rows queued and logs one warning. |
| `WebApi/Email/ResendHttpEmailSender.cs` (new) | Calls `POST https://api.resend.com/emails` with `HttpClient` (no new NuGet package). Uses `Resend:ApiKey` and `Email:From` (default `onboarding@resend.dev`). |
| `WebApi/Startup.cs` | Registers the signal (singleton), the typed HttpClient sender and the hosted processor. |

### Notes
- Safe with several API instances: the claim is atomic, so a row is sent once.
- `EmailWorker/` is no longer needed by the API. It still requires a Service Bus connection string and is left untouched for now.
- Needs `Resend__ApiKey` (and optionally `Email__From`) in the API app settings; Terraform already sets them.

### Verification
- `dotnet build WebApi` (to a separate output folder): 0 errors.
- Unit tests **not run**: IIS Express / Visual Studio had `WebApi/bin` locked. Run `dotnet test APIUnitProject.Tests` after stopping IIS Express.
- Not tested end to end (no real send yet).

---

## Entry 3 — Terraform for the new Azure environment; remove exposed secrets from EmailWorker README

- **Commit:** `a4026e4` ("commit terraform files")
- **Date:** 2026-11-27
- **Branch:** `introducing_Claude`

### Why
All Azure resources from the free-credit month were deleted, so the app is offline. Re-creating them by hand
is error-prone and not repeatable. Terraform makes the environment reproducible, so switching or rebuilding
an Azure account is one `terraform apply`. Chosen constraints: pay-as-you-go account, cheapest viable
setup, and Service Bus dropped (see "Decisions").

### Decisions
- **Cheapest design:** App Service F1 (free, no always-on, so cold starts), Static Web App Free tier, no Function
  App, no Container App, no Service Bus. Expected Azure cost is about 0; the main cost is MongoDB Atlas, which is
  outside Azure.
- **Service Bus dropped.** Activity logs already fall back to direct MongoDB writes when no connection string is set.
  Email is already saved to the MongoDB `EmailOutbox` collection first; the plan is for the API to send via
  Resend from the outbox with a retry loop, which makes `EmailWorker` unnecessary. **That code change is not
  done yet**, so email sending is not working in the new setup until it is.
- **State is local** (`terraform/terraform.tfstate`, gitignored). It must survive the Azure account, so it is not
  stored in an Azure storage account.
- **Cost guardrail:** a monthly budget (default USD 5) on the resource group with email alerts at 50/80/100%.
  Azure budgets only alert; they do not stop spending.
- **Data stays in MongoDB Atlas** (`pmcluster0`, database `ListingDB`), so it is independent of the Azure account.

### Changes

| File | Change |
|---|---|
| `terraform/providers.tf` | `azurerm ~> 4.0`, subscription id from a variable, local state. |
| `terraform/variables.tf` | Subscription, region, prefix, SKU, budget, alert emails, `dotnet_version`, and sensitive app-setting inputs. |
| `terraform/main.tf` | Resource group, Linux App Service plan and web app (health check `/health`, `always_on` off for F1/D1), Static Web App (Free), resource-group budget with alerts. App settings mirror the keys the deploy-API workflow sets, with no `ServiceBus__*`. |
| `terraform/outputs.tf` | API URL, API app name, frontend URL, and the Static Web App deploy token (sensitive). |
| `terraform/terraform.tfvars.example` | Template; real `terraform.tfvars` is gitignored. |
| `.gitignore` | Ignore Terraform state, `.terraform/`, `*.tfvars` (except the example) and plan files. |
| `EmailWorker/README.md` | Replaced a real MongoDB password and a Resend API key with placeholders. |

### Security note
The removed MongoDB password (Railway host) and Resend key remain in git history, so they must be **rotated**.
The same Railway MongoDB password is still present in `DEPLOYMENT_CHECKLIST.md`, `RAILWAY_DEPLOYMENT.md`,
`RAILWAY_DEPLOYMENT_GUIDE.md` and `WORKER_RAILWAY_DEPLOYMENT.md`; not yet cleaned.

### Status / not done yet
- Nothing has been created in Azure: `terraform init -backend=false` and `terraform validate` only; no `plan` or `apply`.
- Default region `francecentral` is a placeholder; set `location` to the Azure region closest to the Atlas cluster.
- `dotnet_version` defaults to `6.0` to match `WebApi` (net6.0, end of support). If Azure rejects it, upgrade the API to net8.0.
- F1 quota can be zero on some new subscriptions; the SKU is a one-line variable change.
- Still to do: the email-sending code change, update the GitHub workflows (API app name and secrets, Angular token, remove the
  Function App workflow), point `app/src/app/environments/environment.prod.ts` at the new API URL, and clean the four docs above.

### Verification
- `terraform fmt` and `terraform validate`: success.

---

## Entry 2 — Remove ~11 s delay on logged API calls (dead Azure Service Bus)

- **Commit:** `d52630e` ("Fix ~11s delay on login and property list caused by dead Service Bus")
- **Date:** 2026-11-26
- **Branch:** `introducing_Claude`

### Symptom
`POST /account/login` and `GET /Property/accessible` each took about 11.6 s (DevTools → Network),
about 24 s from clicking login to the property selector. After the fix: 1.27 s and 1.28 s.
Every navigation also produced a `404` on `POST /accessLog/accessLog`.

### Root cause
- The Azure resources (free-credit month) were deleted, including the Service Bus namespace
  `namespace-servicebus-accesslog.servicebus.windows.net`. DNS returned "Non-existent domain".
- `ServiceBus:ConnectionString` was still set in user secrets and in `appsettings.json` /
  `appsettings.Development.json`, so the API treated Service Bus as configured.
- `ActivityLoggingActionFilter` runs for every action marked `[LogList]` / `[LogCreate]` / `[ActivityLog]`
  and **awaited** `IServiceBusPublisher.SendAsync(...)` before the response was released.
  The Azure SDK retry/timeout against the dead host cost about 11 s per request. The publisher's
  `catch` swallowed the error, so requests still returned 200 and the cause was invisible.
- `ServiceBusPublisher` also created and disposed a new `ServiceBusSender` per message, so each send
  paid a new AMQP connection and link setup.
- `logging.service.ts` posted to `/accessLog/accessLog`, an endpoint removed from the API when access
  logs moved to `AccessLogFunction`, so it returned 404 on every navigation.

### Changes

| File | Change | Why |
|---|---|---|
| `classfiles/Infrastructure/Filters/ActivityLoggingActionFilter.cs` | Service Bus send is fire-and-forget (`_ = SendAsync(...)`) | Audit logging must never delay the HTTP response. The event is a plain object and the publisher catches/logs its own errors, so no request-scoped state is used afterwards. |
| `WebApi/Messaging Queue/ServiceBusPublisher.cs` | One cached sender per queue (`ConcurrentDictionary`); `MaxRetries = 1`, `TryTimeout = 5 s`; no client is built and `SendAsync` is a no-op if the connection string is empty or a `<placeholder>`; dispose senders and client | Avoid per-message AMQP setup; fail fast on an unreachable namespace; safe when Service Bus is not configured. |
| `WebApi/appsettings.json`, `WebApi/appsettings.Development.json` | `ServiceBus:ConnectionString` set to `""` | The namespace no longer exists. An empty value makes the filter use its existing fallback (`UserActivityService.LogActivityAsync` writes to MongoDB). Also removes a connection string from tracked files (CLAUDE.md: no secrets in appsettings). |
| `app/src/app/core/system/service/logging.service.ts` | `logPageNavigation` is now a no-op (signature kept) | Removes the 404 on every navigation. Page-access logging is handled server-side by the activity filter. |
| User secrets (not in git) | `dotnet user-secrets remove "ServiceBus:ConnectionString" --project WebApi` | Remove the dead connection string locally. |

### Behaviour after the change
- No Service Bus connection string: activity logs are written directly to MongoDB (local-dev path).
- Connection string present: the event is published in the background. A failure is logged and
  does not affect the response, and there is no automatic fallback to MongoDB for that event.

### Follow-ups / open items
- If Service Bus is recreated, set `ServiceBus:ConnectionString` through user secrets or Key Vault only. The
  consumers `ActivityLogFunction` / `AccessLogFunction` must be deployed for queued events to be stored.
- Remaining ~1.3 s per call is most likely round-trip latency to MongoDB Atlas (`pmcluster0`).
  `GetAccessibleProperties` also runs a synchronous `$lookup` aggregation
  (`GetPropertyQueryByUserIdUsingMongoQueryString`); consider an async handler and a single `$in` filter.
- Browser console showed 135 errors in the latest screenshot (not yet investigated).
- Backend tests (`APIUnitProject.Tests`) not yet run for this change.

### Verification
- `dotnet build` succeeded (built to a separate output folder because IIS Express/VS had the DLLs locked).
- `npm run build` from `app/` succeeded.
- DevTools Network: login 11.61 s → 1.27 s, accessible 11.71 s → 1.28 s, no `accessLog` 404.

---

## Entry 1 — Stop the global loader from re-appearing and sticking

- **Commit:** `eeeb304` ("loader improvements"), 2026-11-26
  (`b9be381` in the same session only touched `CLAUDE.md`.)

### Symptom
The full-screen "Loading — Preparing the page — this may take a moment" overlay kept appearing
and sometimes stayed up for a long time.

### Root cause
- `loadingInterceptor` called `LoaderService.show()` for every HTTP request, including background
  polling: team feed every 15 s, activity feed every 30 s, activity stream every 60 s.
- `LoaderService` is a request counter, so any slow request kept the overlay up. There was no upper bound.
- `app.ts` called `show()` on `RouteConfigLoadStart` but only `hide()` on `RouteConfigLoadEnd`. A cancelled
  or failed navigation could leave the counter above zero indefinitely.

### Changes
| File | Change |
|---|---|
| `app/src/app/core/interceptors/loading.interceptor.ts` | Added `SKIP_GLOBAL_LOADER` (`HttpContextToken`) and `silentLoadingContext()`; marked requests skip the overlay. |
| `app/src/app/profile/posts.service.ts` | Polling request uses `silentLoadingContext()`. |
| `app/src/app/services/activity.service.ts` | `getWidgetData` and `getActivitySummary` take an optional `silent` flag. |
| `app/src/app/widgets/activity-feed-widget/...component.ts`, `activity-stream-widget/...component.ts` | Timed refreshes pass `silent = true`. |
| `app/src/app/app.ts` | Tracks outstanding lazy route loads and releases them on `NavigationCancel` / `NavigationError`. |
| `app/src/app/core/auth/services/loader-service.service.ts` | 15 s safety cap (`maxVisibleMs`) force-hides the overlay and resets the counter. |

### Not changed
- The analytics widget (5-minute refresh, 7 parallel calls) still uses the overlay.
