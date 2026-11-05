using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace SportsClub.Components
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

        // keyboard navigation (ARIA active-descendant pattern)
        private readonly string _listboxId = "dlb-" + Guid.NewGuid().ToString("N")[..8];
        private int _activeIndex = -1;

        private string OptionId(int index) => $"{_listboxId}-opt-{index}";

        private string? ActiveDescendantId =>
            _activeIndex >= 0 && _activeIndex < _combinedItems.Count ? OptionId(_activeIndex) : null;

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

        private bool IsSelected(ListBoxOption<TValue> item)
        {
            var comparer = EqualityComparer<TValue>.Default;
            return AllowMultiple
                ? SelectedValues.Any(selected => comparer.Equals(item.Value, selected))
                : SelectedValue is not null && comparer.Equals(item.Value, SelectedValue);
        }

        private async Task OnOptionClickAsync(int index)
        {
            _activeIndex = index;
            await SelectAsync(_combinedItems[index].Value);
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

        private void OnFocus()
        {
            // On focus, start keyboard navigation at the selected item, else the first.
            if (_activeIndex < 0 && _combinedItems.Count > 0)
            {
                var selectedIndex = _combinedItems.FindIndex(IsSelected);
                _activeIndex = selectedIndex >= 0 ? selectedIndex : 0;
            }
        }

        private async Task OnKeyDownAsync(KeyboardEventArgs e)
        {
            if (_combinedItems.Count == 0) return;

            switch (e.Key)
            {
                case "ArrowDown":
                    _activeIndex = _activeIndex < 0 ? 0 : Math.Min(_activeIndex + 1, _combinedItems.Count - 1);
                    break;
                case "ArrowUp":
                    _activeIndex = _activeIndex <= 0 ? 0 : _activeIndex - 1;
                    break;
                case "Home":
                    _activeIndex = 0;
                    break;
                case "End":
                    _activeIndex = _combinedItems.Count - 1;
                    break;
                case "Enter":
                case " ":
                case "Spacebar":
                    if (_activeIndex >= 0 && _activeIndex < _combinedItems.Count)
                        await SelectAsync(_combinedItems[_activeIndex].Value);
                    break;
            }
        }

        // Called by the page before it deletes the selected record(s)
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

        private string GetItemClass(ListBoxOption<TValue> item, int index)
        {
            var isPending = _pendingRemoval && _pendingRemovalValues is not null && _pendingRemovalValues.Contains(item.Value);

            var classes = new List<string> { "dlb-item" };
            if (isPending) classes.Add("dlb-pending");
            else if (IsSelected(item)) classes.Add("dlb-selected");
            if (index == _activeIndex) classes.Add("dlb-active");

            return string.Join(' ', classes);
        }
    }
}
