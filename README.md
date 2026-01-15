# DynamicListBox Explanation

Here’s the approach I took: the list combines two sources of items - DB items passed in via `Items`, and declarative items defined in Razor via `DynamicListBoxItemComponent`. Child items register with the parent on render, so you can use pure markup when you don’t want to data‑bind.

Selection uses the standard Blazor pattern (`SelectedValue` / `SelectedValueChanged`). Clicking an item updates the bound value and applies a visual highlight.

For removal, the page first calls `ShowRemoveVisualAsync`, which marks the selected item as “pending,” waits one second, then clears that state so the item can be removed from the data source.

The UI stays in sync because the page reloads `Items` after add/remove and the component recomputes the combined list every render.

Sizing is driven by the `Width` and `Height` parameters, and the inner container uses `overflow-y: auto` to show a vertical scrollbar when needed.
