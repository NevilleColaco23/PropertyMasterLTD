# Change Tracking

Log of notable changes: commit id, what changed, and the technical reason. Newest first.
Add a new entry per commit. For uncommitted work, use `PENDING` and fill in the id after committing.

---

## Entry 2 — Remove ~11 s delay on logged API calls (dead Azure Service Bus)

- **Commit:** `PENDING` (fill in after commit: `git log -1 --format=%h`)
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
