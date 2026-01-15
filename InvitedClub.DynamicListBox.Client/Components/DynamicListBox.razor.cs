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
        [Parameter] public IReadOnlyList<TValue> SelectedValues { get; set; } = Array.Empty<TValue>();
        [Parameter] public EventCallback<IReadOnlyList<TValue>> SelectedValuesChanged { get; set; }
        [Parameter] public bool AllowMultiple { get; set; }

        // Configurable sizing
        [Parameter] public string Width { get; set; } = "320px";
        [Parameter] public string Height { get; set; } = "240px";
        [Parameter] public bool StopClickPropagation { get; set; }

        // ChildContent items (markup-defined) register into this list
        private readonly List<ListBoxOption<TValue>> _childItems = new();

        private List<ListBoxOption<TValue>> _combinedItems = new();

        // removal visual state
        private bool _pendingRemoval;
        private HashSet<TValue>? _pendingRemovalValues;

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

        private async Task SelectAsync(TValue value)
        {
            if (AllowMultiple)
            {
                var comparer = EqualityComparer<TValue>.Default;
                var next = SelectedValues.ToList();

                var existingIndex = next.FindIndex(x => comparer.Equals(x, value));
                if (existingIndex >= 0)
                    next.RemoveAt(existingIndex);
                else
                    next.Add(value);

                SelectedValues = next;
                await SelectedValuesChanged.InvokeAsync(next);
                return;
            }

            SelectedValue = value;
            await SelectedValueChanged.InvokeAsync(value);
        }

        // Called by demo page before it deletes the selected record(s)
        public async Task ShowRemoveVisualAsync()
        {
            if (AllowMultiple)
            {
                if (SelectedValues.Count == 0) return;
                _pendingRemovalValues = new HashSet<TValue>(SelectedValues, EqualityComparer<TValue>.Default);
            }
            else
            {
                if (SelectedValue is null) return;
                _pendingRemovalValues = new HashSet<TValue>(EqualityComparer<TValue>.Default) { SelectedValue! };
            }

            _pendingRemoval = true;
            StateHasChanged();

            await Task.Delay(1000);

            _pendingRemoval = false;
            _pendingRemovalValues = null;
            StateHasChanged();
        }

        private string GetItemClass(ListBoxOption<TValue> item)
        {
            var comparer = EqualityComparer<TValue>.Default;

            var isSelected = AllowMultiple
                ? SelectedValues.Any(selected => comparer.Equals(item.Value, selected))
                : SelectedValue is not null && comparer.Equals(item.Value, SelectedValue);
            var isPending = _pendingRemoval && _pendingRemovalValues is not null && _pendingRemovalValues.Contains(item.Value);

            if (isPending) return "dlb-item dlb-pending";
            if (isSelected) return "dlb-item dlb-selected";
            return "dlb-item";
        }
    }
}
