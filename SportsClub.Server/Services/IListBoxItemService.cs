using SportsClub.Shared;

namespace SportsClub.Server.Services;

public interface IListBoxItemService
{
    Task<IReadOnlyList<ListBoxItemDto>> GetByBoardAsync(int boardId, CancellationToken ct = default);
    Task<ListBoxItemDto> AddAsync(int boardId, string text, CancellationToken ct = default);
    Task<ListBoxItemDto> UpdateAsync(int boardId, int itemId, string text, byte[] rowVersion, CancellationToken ct = default);
    Task DeleteAsync(int boardId, int itemId, CancellationToken ct = default);
}
