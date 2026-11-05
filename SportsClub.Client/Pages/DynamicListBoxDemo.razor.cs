using System.Net.Http.Json;
using SportsClub.Components;
using SportsClub.Shared;

namespace SportsClub.Client.Pages
{
    public partial class DynamicListBoxDemo
    {
        private DynamicListBox<int>? _listBox;
        private List<ListBoxOption<int>> _dbItems = new();
        private int _selectedId;
        private IReadOnlyList<int> _selectedMarkupIds = Array.Empty<int>();
        private string _newText = "";

        private string SelectedMarkupLabel => _selectedMarkupIds.Count == 0
            ? "(none)"
            : string.Join(", ", _selectedMarkupIds.Select(GetMarkupLabel));

        private static string GetMarkupLabel(int value) => value switch
        {
            1 => "Markup Item A",
            2 => "Markup Item B",
            _ => value.ToString()
        };

        private void ClearMarkupSelection()
        {
            _selectedMarkupIds = Array.Empty<int>();
        }

        protected override async Task OnInitializedAsync() => await ReloadAsync();

        private async Task ReloadAsync()
        {
            var result = await Http.GetFromJsonAsync<PagedResult<ListBoxItemDto>>(
                "api/ListBoxItems?pageSize=200");

            _dbItems = (result?.Items ?? Array.Empty<ListBoxItemDto>())
                .OrderBy(x => x.SortOrder)
                .Select(x => new ListBoxOption<int>(x.Id, x.Text))
                .ToList();
        }

        private async Task AddAsync()
        {
            var text = _newText.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            var response = await Http.PostAsJsonAsync(
                "api/ListBoxItems", new CreateListBoxItemRequest(text));

            if (response.IsSuccessStatusCode)
                _newText = "";

            await ReloadAsync();
        }

        private async Task RemoveAsync()
        {
            if (_selectedId <= 0) return;

            var removedId = _selectedId;

            // Show the ~1-second pending highlight before deleting.
            if (_listBox is not null)
                await _listBox.ShowRemoveVisualAsync();

            try
            {
                var response = await Http.DeleteAsync($"api/ListBoxItems/{removedId}");
                if (response.IsSuccessStatusCode)
                    _selectedId = 0;
            }
            finally
            {
                // Always resync with the server so the UI reflects the true state,
                // whether the delete succeeded, failed, or the row was already gone.
                await ReloadAsync();
            }
        }
    }
}
