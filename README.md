# SportsClub - Reusable Blazor DynamicListBox

A reference application showcasing a reusable, generic Blazor list-box component
(`DynamicListBox<TValue>`, in its own class library) used to build a small
**activity-boards** manager backed by a REST API and a database.

**Stack:** Blazor WebAssembly (standalone client), ASP.NET Core Web API, EF Core, SQL Server (LocalDB)

> The client and the API are two separate apps that communicate over HTTP
> (CORS-enabled); this is not the "hosted" Blazor template where one project serves the other.

## Solution structure

| Project | Purpose |
| --- | --- |
| `SportsClub.Components` | Razor Class Library holding the reusable `DynamicListBox<TValue>` component (CSS-isolated, with ARIA roles + keyboard navigation). Domain-neutral and NuGet-packagable. |
| `SportsClub.Client` | Blazor WebAssembly app: a boards-manager UI plus an offline component demo (`/component-demo`). |
| `SportsClub.Server` | ASP.NET Core Web API (boards + items) over EF Core, with a service layer and global RFC-7807 `ProblemDetails` error handling. |
| `SportsClub.Shared` | DTOs shared between client and server. |
| `SportsClub.Tests` | xUnit service tests (EF InMemory) and `WebApplicationFactory` HTTP integration tests. |

The `DynamicListBox<TValue>` component is generic and domain-neutral - the
SportsClub theme lives only in the app and seed data.

## Domain

A `ListBoard` (a named list) **owns many** activity items (one-to-many, cascade
delete). Board names are unique. Both entities carry a `RowVersion` concurrency
token.

## Features

- **Reusable component** - generic over `TValue`, CSS-isolated, keyboard-navigable
  (arrows / Home / End / Enter / Space) with full ARIA roles.
- **Layered API** - thin controllers over a service layer; request validation via
  data annotations (clean `400`s).
- **Optimistic concurrency** - board rename and item update check `RowVersion`; a
  stale edit returns `409`. Duplicate board names return `409`.
- **Centralized errors** - a global `IExceptionHandler` maps domain exceptions to
  RFC-7807 `ProblemDetails` (404 / 409 / 500).

## API

| Route | Verbs |
| --- | --- |
| `/api/boards` | GET (list), POST (create) |
| `/api/boards/{id}` | GET (with items), PUT (rename), DELETE |
| `/api/boards/{boardId}/items` | GET, POST |
| `/api/boards/{boardId}/items/{itemId}` | PUT (update), DELETE |

## Getting started

1. Ensure SQL Server LocalDB is available (ships with Visual Studio).
2. Run `SportsClub.Server` (the API). On first run it applies the EF Core
   migrations and seeds three sample boards with activities. Swagger is available
   in Development.
3. Run `SportsClub.Client`. Its API base URL comes from `wwwroot/appsettings.json`.

## How the component works

The list combines two item sources: DB items passed via `Items`, and declarative
items defined in Razor via `DynamicListBoxItemComponent`. Child items register with
the parent on render, so you can use pure markup when you don't want to data-bind.

Selection uses the standard Blazor pattern (`SelectedValue` / `SelectedValueChanged`,
or `SelectedValues` with `AllowMultiple`). Clicking or keyboard-activating an item
updates the bound value and highlights it. The list is a `role="listbox"` with
`role="option"` children, `aria-selected`, and arrow-key navigation via
`aria-activedescendant`.

For removal, the page calls `ShowRemoveVisualAsync`, which marks the selected item
as "pending," waits one second, then clears that state so the item can be removed.

Sizing is driven by the `Width` and `Height` parameters; the inner container uses
`overflow-y: auto` for a scrollbar when needed.

## Configuration

Secrets and environment-specific settings are kept out of committed base config.

**Server**
- The dev connection string lives in `appsettings.Development.json`. For other
  environments supply it via user-secrets or an environment variable:
  - `dotnet user-secrets set "ConnectionStrings:ListBoxDb" "<connection-string>"`
  - or env var `ConnectionStrings_ListBoxDb`
- Allowed CORS origins come from `Cors:AllowedOrigins`. The base `appsettings.json`
  ships empty (no cross-origin unless configured); dev origins are in
  `appsettings.Development.json`.

**Client**
- The API base URL is read from `wwwroot/appsettings.json` (`ApiBaseUrl`).
