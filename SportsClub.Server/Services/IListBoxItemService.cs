using SportsClub.Shared;

namespace SportsClub.Server.Services;

public interface IListBoxItemService
{
    Task<PagedResult<ListBoxItemDto>> GetAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ListBoxItemDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ListBoxItemDto> AddAsync(string text, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
