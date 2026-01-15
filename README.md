# DynamicListBox Explanation

DynamicListBox keeps two sources of items: database items passed in via Items and markup-defined items registered by DynamicListBoxItem. On each render, the component merges them into _combinedItems. Child components register once through a cascading parent reference, so declarative items are discovered without JS.

Selection is handled via SelectedValue/SelectedValueChanged, so the parent page owns state and can react to changes. Clicking an item updates SelectedValue and the list uses CSS classes to highlight the selected item.

Removal feedback is triggered by the page calling ShowRemoveVisualAsync before deleting. The component marks the selected item as pending, re-renders, waits one second, then clears the pending state so the page can remove it from the data source.

UI stays in sync because Items is reloaded from the API after add/remove and OnParametersSet recomputes the combined list on every render.

Sizing is applied with Width and Height parameters, set inline on the container. The inner scroll region uses overflow-y:auto so vertical scrollbars appear when content exceeds the configured height.
