using SportsClub.Shared;

namespace SportsClub.Server.Services;

public interface IListBoardService
{
    Task<IReadOnlyList<ListBoardSummaryDto>> GetAllAsync(CancellationToken ct = default);
    Task<ListBoardDetailDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ListBoardSummaryDto> CreateAsync(string name, CancellationToken ct = default);
    Task<ListBoardSummaryDto> RenameAsync(int id, string name, byte[] rowVersion, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
