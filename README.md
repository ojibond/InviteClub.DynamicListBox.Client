# SportsClub - Reusable Blazor DynamicListBox

A small reference application showcasing a reusable, generic Blazor list-box
component (`DynamicListBox<TValue>`) backed by a REST API and a database.

**Stack:** Blazor WebAssembly (standalone client), ASP.NET Core Web API, EF Core, SQL Server (LocalDB)

> Note: the client and the API are two separate apps that communicate over HTTP
> (CORS-enabled); this is not the "hosted" Blazor template where one project serves the other.

## Solution structure

| Project | Purpose |
| --- | --- |
| `SportsClub.Client` | Blazor WebAssembly app that hosts the `DynamicListBox` component and its demo page. |
| `SportsClub.Server` | ASP.NET Core Web API exposing list-box items (`/api/ListBoxItems`) over EF Core. |
| `SportsClub.Shared` | DTOs shared between client and server. |

The `DynamicListBox<TValue>` component is domain-neutral (generic over `TValue`),
so it is reusable in any app — the SportsClub theme lives only in the demo and seed data.

## Getting started

1. Ensure SQL Server LocalDB is available (ships with Visual Studio).
2. Run `SportsClub.Server` (the API). On first run it applies the EF Core migration
   and seeds a few sample activities. Swagger is available in Development.
3. Run `SportsClub.Client` (the Blazor app). It calls the API at the URL configured
   in `Program.cs`.

## How it works

The list combines two sources of items: DB items passed in via `Items`, and
declarative items defined in Razor via `DynamicListBoxItemComponent`. Child items
register with the parent on render, so you can use pure markup when you don't want
to data-bind.

Selection uses the standard Blazor pattern (`SelectedValue` / `SelectedValueChanged`,
or `SelectedValues` with `AllowMultiple`). Clicking an item updates the bound value
and applies a visual highlight.

For removal, the page first calls `ShowRemoveVisualAsync`, which marks the selected
item as "pending," waits one second, then clears that state so the item can be removed
from the data source.

The UI stays in sync because the page reloads `Items` after add/remove and the
component recomputes the combined list every render. Sizing is driven by the `Width`
and `Height` parameters, and the inner container uses `overflow-y: auto` to show a
vertical scrollbar when needed.

## Design notes

1. **Items/state:** the component merges DB-backed `Items` with child markup items
   registered on render; selection is tracked via `SelectedValue` (single) or
   `SelectedValues` with `AllowMultiple` (multi).
2. **Visual feedback/timing:** `ShowRemoveVisualAsync` sets a pending flag, re-renders,
   waits 1 second, then clears the flag; the page removes the item after the delay.
3. **UI updates:** after add/remove, the page reloads `Items`; the component recomputes
   the combined list in `OnParametersSet`, and child registration calls `StateHasChanged`.
4. **Persistence:** `ListBoxItemEntity` (Id, Text, SortOrder, CreatedDate) is stored in
   `ListBoxDbContext`; an EF Core migration creates the table; the controller handles
   API reads/writes.
5. **Sizing/scrolling:** `Width`/`Height` parameters are applied to the container, and
   the inner scroll div uses `overflow-y: auto`.

## Configuration

Secrets and environment-specific settings are kept out of committed base config.

**Server**
- The database connection string lives in `appsettings.Development.json` for local dev. For other environments, supply it via user-secrets or an environment variable rather than committing it:
  - `dotnet user-secrets set "ConnectionStrings:ListBoxDb" "<your-connection-string>"`
  - or env var `ConnectionStrings__ListBoxDb`
- Allowed CORS origins are read from `Cors:AllowedOrigins` (an array). The base `appsettings.json` ships empty — no cross-origin requests are allowed unless configured. Dev origins are set in `appsettings.Development.json`.

**Client**
- The API base URL is read from `wwwroot/appsettings.json` (`ApiBaseUrl`), so it can be changed per environment without code changes.
