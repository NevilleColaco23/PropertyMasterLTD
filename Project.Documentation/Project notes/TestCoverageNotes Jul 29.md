# Unit Test Coverage Notes

## Status (as of this session)

The `APIUnitProject.Tests` project has coverage for:

- **Controllers** (mediator pass-through tests): Account, Activity, ActivityAnalytics, Bookings, Dashboard, Guests, Health, Menu, MenuPermissions, Messages, Partner, Product, Property, Rooms, Test, Transaction, Users.
- **Application handlers** (`IUnitOfWork`/repository-based, mockable with Moq): Partners, Products, Property (Create/Update/Delete), Transactions, Users (Create/GetByUserId), MenuPermissions (Create/Update/Delete).

## Deliberately Skipped: Bookings / Dashboard / UserActivity Mongo-driven handlers

The following Application-layer handlers were **intentionally not covered** with unit tests:

- `Bookings\Commands\CancelBookingCommandHandler`
- `Bookings\Commands\MoveBookingCommandHandler`
- `Bookings\Commands\UpdateBookingDatesCommandHandler`
- `Dashboard\Queries\*QueryHandlers` (DashboardActivityQueryHandlers, DashboardKpiQueryHandlers, DashboardQueryHandlers, GetBookingsWithGuestsQueryHandler, GetRoomsByPropertyQueryHandler, etc.)
- `UserActivity\Handlers\UserActivityQueryHandlers` / `UserActivityAnalyticsQueryHandlers` (the underlying Mongo-query implementations, as opposed to the controller endpoints, which **are** covered)

### Why

These handlers call `IMongoDatabase.GetCollection<T>()` directly and chain raw MongoDB driver operations (`Find().Project().FirstOrDefaultAsync()`, `UpdateOneAsync()`, aggregation pipelines, etc.). Unlike the `IUnitOfWork`/repository-based handlers, there is no seam here that Moq can mock meaningfully — `IMongoCollection<T>`, `IFindFluent<T,T>`, and `IAsyncCursor<T>` rely on extension methods and sealed driver types, so mocking would only prove "a method was called," not that the query/update logic is correct.

Making these testable with mocks would require introducing a wrapper/repository abstraction around the raw Mongo calls in these handlers — a production code refactor, not just a test addition.

### Alternative considered: Mongo2Go / Testcontainers integration tests

Spinning up a real (ephemeral) MongoDB instance via `Mongo2Go` or Testcontainers and running these handlers against it would give genuine coverage of the query/update behavior without changing production code.

### Decision: Skipped for now

After discussion, we decided **not** to add this coverage at this time because:

- No known bugs or planned changes to this booking/dashboard/user-activity Mongo logic.
- These handlers are comparatively stable/narrow-purpose and lower change frequency than the `IUnitOfWork`-based handlers already covered.
- Adding Mongo2Go/Testcontainers introduces new test infrastructure cost: extra dependency, slower test runs, and a Mongo binary download requirement in CI.
- Current priority is the Azure Web App deployment work (branch `Azure_web_App_Deployment`), not maximizing test coverage.

### Revisit if

- Bugs are found in booking cancel/move/date-update flows or dashboard/user-activity queries.
- These handlers are refactored or significantly modified.
- The team decides query correctness here is business-critical enough to justify the Mongo2Go/Testcontainers investment.

If revisiting, the recommended approach is Option 2 (integration tests via `Mongo2Go`), not a mock-based refactor of the handlers, since it validates real query behavior without touching production code.
