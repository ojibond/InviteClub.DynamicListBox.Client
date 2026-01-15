using Microsoft.AspNetCore.Components;

namespace InvitedClub.DynamicListBox.Client.Components
{
    public partial class DynamicListBox<TValue> : ComponentBase
    {
        [Parameter] public RenderFragment? ChildContent { get; set; }

        // DB-bound items passed from page
        [Parameter] public IReadOnlyList<ListBoxOption<TValue>> Items { get; set; } = Array.Empty<ListBoxOption<TValue>>();

        // Binding support
        [Parameter] public TValue? SelectedValue { get; set; }
        [Parameter] public EventCallback<TValue?> SelectedValueChanged { get; set; }

        // Configurable sizing
        [Parameter] public string Width { get; set; } = "320px";
        [Parameter] public string Height { get; set; } = "240px";
        [Parameter] public bool StopClickPropagation { get; set; }

        // ChildContent items (markup-defined) register into this list
        private readonly List<ListBoxOption<TValue>> _childItems = new();

        private List<ListBoxOption<TValue>> _combinedItems = new();

        // removal visual state
        private bool _pendingRemoval;
        private TValue? _pendingRemovalValue;

        protected override void OnParametersSet()
        {
            // Combine DB items + declarative child items each render
            _combinedItems = Items.Concat(_childItems).ToList();
        }

        internal void RegisterChildItem(ListBoxOption<TValue> option)
        {
            // avoid duplicates on re-render
            if (_childItems.Any(x => EqualityComparer<TValue>.Default.Equals(x.Value, option.Value)))
                return;

            _childItems.Add(option);
            _combinedItems = Items.Concat(_childItems).ToList();
            StateHasChanged();
        }

        private async Task SelectAsync(TValue? value)
        {
            SelectedValue = value;
            await SelectedValueChanged.InvokeAsync(value);
        }

        // Called by demo page before it deletes the selected record
        public async Task ShowRemoveVisualAsync()
        {
            if (SelectedValue is null) return;

            _pendingRemovalValue = SelectedValue;
            _pendingRemoval = true;
            StateHasChanged();

            await Task.Delay(1000);

            _pendingRemoval = false;
            _pendingRemovalValue = default;
            StateHasChanged();
        }

        private string GetItemClass(ListBoxOption<TValue> item)
        {
            var comparer = EqualityComparer<TValue>.Default;

            var isSelected = SelectedValue is not null && comparer.Equals(item.Value, SelectedValue);
            var isPending = _pendingRemoval && _pendingRemovalValue is not null && comparer.Equals(item.Value, _pendingRemovalValue);

            if (isPending) return "dlb-item dlb-pending";
            if (isSelected) return "dlb-item dlb-selected";
            return "dlb-item";
        }
    }
}
