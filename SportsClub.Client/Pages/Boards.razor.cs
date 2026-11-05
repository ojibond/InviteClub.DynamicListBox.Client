using System.Net.Http.Json;
using SportsClub.Components;
using SportsClub.Shared;

namespace SportsClub.Client.Pages
{
    public partial class Boards
    {
        private List<ListBoardSummaryDto> _boards = new();
        private ListBoardDetailDto? _selectedBoard;
        private List<ListBoxOption<int>> _items = new();
        private DynamicListBox<int>? _listBox;

        private int _selectedItemId;
        private string _newBoardName = "";
        private string _newItemText = "";
        private string _boardNameEdit = "";
        private string? _message;

        private int? SelectedBoardId => _selectedBoard?.Id;

        protected override async Task OnInitializedAsync()
        {
            await LoadBoardsListAsync();
            if (_boards.FirstOrDefault() is { } first)
                await SelectBoardAsync(first.Id);
        }

        private async Task LoadBoardsListAsync()
            => _boards = await Http.GetFromJsonAsync<List<ListBoardSummaryDto>>("api/boards") ?? new();

        private async Task SelectBoardAsync(int id)
        {
            _message = null;
            _selectedBoard = await Http.GetFromJsonAsync<ListBoardDetailDto>($"api/boards/{id}");
            _boardNameEdit = _selectedBoard?.Name ?? "";
            _selectedItemId = 0;
            _items = (_selectedBoard?.Items ?? Array.Empty<ListBoxItemDto>())
                .Select(i => new ListBoxOption<int>(i.Id, i.Text))
                .ToList();
        }

        private async Task AddBoardAsync()
        {
            var name = _newBoardName.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            var resp = await Http.PostAsJsonAsync("api/boards", new CreateListBoardRequest(name));
            if (resp.IsSuccessStatusCode)
            {
                _newBoardName = "";
                var created = await resp.Content.ReadFromJsonAsync<ListBoardSummaryDto>();
                await LoadBoardsListAsync();
                if (created is not null) await SelectBoardAsync(created.Id);
            }
            else
            {
                _message = await ReadProblemAsync(resp);
            }
        }

        private async Task DeleteBoardAsync(int id)
        {
            var resp = await Http.DeleteAsync($"api/boards/{id}");
            if (!resp.IsSuccessStatusCode)
            {
                _message = await ReadProblemAsync(resp);
                return;
            }

            if (_selectedBoard?.Id == id) _selectedBoard = null;
            await LoadBoardsListAsync();
            if (_selectedBoard is null && _boards.FirstOrDefault() is { } first)
                await SelectBoardAsync(first.Id);
        }

        private async Task RenameBoardAsync()
        {
            if (_selectedBoard is null) return;
            var name = _boardNameEdit.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            var resp = await Http.PutAsJsonAsync(
                $"api/boards/{_selectedBoard.Id}",
                new UpdateListBoardRequest(name, _selectedBoard.RowVersion));

            _message = resp.IsSuccessStatusCode ? null : await ReadProblemAsync(resp);

            var boardId = _selectedBoard.Id;
            await LoadBoardsListAsync();
            await SelectBoardAsync(boardId); // refresh name + rowVersion (also after a 409)
        }

        private async Task AddItemAsync()
        {
            if (_selectedBoard is null) return;
            var text = _newItemText.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            var boardId = _selectedBoard.Id;
            var resp = await Http.PostAsJsonAsync(
                $"api/boards/{boardId}/items", new CreateListBoxItemRequest(text));
            if (resp.IsSuccessStatusCode) _newItemText = "";
            else _message = await ReadProblemAsync(resp);

            await SelectBoardAsync(boardId);
            await LoadBoardsListAsync();
        }

        private async Task RemoveItemAsync()
        {
            if (_selectedBoard is null || _selectedItemId <= 0) return;
            var boardId = _selectedBoard.Id;
            var itemId = _selectedItemId;

            if (_listBox is not null)
                await _listBox.ShowRemoveVisualAsync();

            var resp = await Http.DeleteAsync($"api/boards/{boardId}/items/{itemId}");
            if (!resp.IsSuccessStatusCode) _message = await ReadProblemAsync(resp);

            await SelectBoardAsync(boardId);
            await LoadBoardsListAsync();
        }

        private static async Task<string> ReadProblemAsync(HttpResponseMessage resp)
        {
            try
            {
                var problem = await resp.Content.ReadFromJsonAsync<ProblemResponse>();
                return problem?.Title ?? $"Request failed ({(int)resp.StatusCode}).";
            }
            catch
            {
                return $"Request failed ({(int)resp.StatusCode}).";
            }
        }

        private record ProblemResponse(string? Title, int? Status);
    }
}
