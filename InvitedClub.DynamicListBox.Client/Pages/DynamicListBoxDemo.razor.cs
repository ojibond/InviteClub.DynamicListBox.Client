using System.Net.Http.Json;
using InvitedClub.DynamicListBox.Client.Components;
using InvitedClub.DynamicListBox.Shared;

namespace InvitedClub.DynamicListBox.Client.Pages
{
    public partial class DynamicListBoxDemo
    {
        private DynamicListBox<int>? _listBox;
        private List<ListBoxOption<int>> _dbItems = new();
        private int _selectedId;
        private int _selectedMarkupId;
        private string _newText = "";

        private string SelectedMarkupLabel => _selectedMarkupId switch
        {
            1 => "Markup Item A",
            2 => "Markup Item B",
            _ => "(none)"
        };

        protected override async Task OnInitializedAsync() => await ReloadAsync();

        private async Task ReloadAsync()
        {
            var rows = await Http.GetFromJsonAsync<List<ListBoxItemDto>>("api/ListboxItems") ?? new();
            _dbItems = rows.OrderBy(x => x.SortOrder)
                          .Select(x => new ListBoxOption<int>(x.Id, x.Text))
                          .ToList();
        }

        private async Task AddAsync()
        {
            var text = _newText.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            await Http.PostAsJsonAsync("api/ListboxItems", new CreateListBoxItemRequest(text));
            _newText = "";
            await ReloadAsync();
        }

        private async Task RemoveAsync()
        {
            if (_selectedId <= 0) return;

            // 1-second blue visual 
            if (_listBox is not null)
                await _listBox.ShowRemoveVisualAsync();

            // only delete DB-backed items      
            await Http.DeleteAsync($"api/ListboxItems/{_selectedId}");
            await ReloadAsync();

            // reset selection
            _selectedId = 0;
        }
    }
}
