# Roadmap

SportsClub is an evolving reference for clean, testable .NET patterns. This is the
work planned next — a mix of features, infrastructure, and polish. Items are rough
priority order; checked boxes are done.

## Features

- [ ] **Authentication & authorization (JWT).** The API is currently anonymous by
  design; the middleware slot is marked in `Program.cs`. Add JWT bearer auth, a
  user entity, and `[Authorize]` on the write endpoints.
- [ ] **Real-time sync (SignalR).** Push board/item changes so multiple clients
  stay in sync without a manual refresh.
- [ ] **Drag-and-drop item reordering.** Reorder activities within a board and
  persist `SortOrder` (HTML5 drag-and-drop + a reorder endpoint).
- [ ] **Expose item editing in the UI.** The API already supports
  concurrency-checked updates (`PUT /api/boards/{boardId}/items/{itemId}`); surface
  inline rename in the client.
- [ ] **Pagination / filtering** for boards and items once datasets grow.

## Component library

- [ ] **Publish `SportsClub.Components` as a NuGet package** so `DynamicListBox`
  is consumable standalone.
- [ ] **Theming hooks** (CSS variables) so consumers can restyle without forking.

## Quality & infrastructure

- [ ] **CI via GitHub Actions** — `dotnet build` + `dotnet test` on push and PR.
- [ ] **Branch protection** requiring green CI before merging to `master`
  (once CI exists).
- [ ] **Integration-test the concurrency path** against a real SQL Server
  (LocalDB/Testcontainers), since EF InMemory doesn't emulate `rowversion`.
- [ ] **Containerization (Docker)** for the API + a deployment pipeline.

## Done

- [x] Rename/rebrand to **SportsClub**.
- [x] Extract `DynamicListBox` into a reusable Razor Class Library (CSS isolation,
  ARIA + keyboard navigation).
- [x] Service layer, request validation, and a global RFC-7807 `ProblemDetails`
  error handler.
- [x] Security/config hardening (scoped CORS, config-driven API URL, user-secrets).
- [x] **Named activity boards** — one-to-many domain with optimistic concurrency.
- [x] Service + `WebApplicationFactory` integration tests.
