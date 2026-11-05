using Microsoft.EntityFrameworkCore;
using SportsClub.Server.Data;
using SportsClub.Shared;

namespace SportsClub.Server.Services;

public class ListBoardService : IListBoardService
{
    private readonly ListBoxDbContext _dbContext;

    public ListBoardService(ListBoxDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<ListBoardSummaryDto>> GetAllAsync(CancellationToken ct = default)
        => await _dbContext.Boards
            .OrderBy(b => b.Name)
            .Select(b => new ListBoardSummaryDto(
                b.Id, b.Name, b.Items.Count, Convert.ToBase64String(b.RowVersion)))
            .ToListAsync(ct);

    public async Task<ListBoardDetailDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var board = await _dbContext.Boards
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new NotFoundException($"Board {id} was not found.");

        var items = board.Items
            .OrderBy(i => i.SortOrder)
            .Select(i => new ListBoxItemDto(i.Id, i.Text, i.SortOrder, Convert.ToBase64String(i.RowVersion)))
            .ToList();

        return new ListBoardDetailDto(board.Id, board.Name, Convert.ToBase64String(board.RowVersion), items);
    }

    public async Task<ListBoardSummaryDto> CreateAsync(string name, CancellationToken ct = default)
    {
        name = name.Trim();
        if (await _dbContext.Boards.AnyAsync(b => b.Name == name, ct))
            throw new ConflictException($"A board named '{name}' already exists.");

        var board = new ListBoardEntity { Name = name };
        _dbContext.Boards.Add(board);
        await _dbContext.SaveChangesAsync(ct);

        return new ListBoardSummaryDto(board.Id, board.Name, 0, Convert.ToBase64String(board.RowVersion));
    }

    public async Task<ListBoardSummaryDto> RenameAsync(int id, string name, byte[] rowVersion, CancellationToken ct = default)
    {
        var board = await _dbContext.Boards
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new NotFoundException($"Board {id} was not found.");

        name = name.Trim();
        if (await _dbContext.Boards.AnyAsync(b => b.Name == name && b.Id != id, ct))
            throw new ConflictException($"A board named '{name}' already exists.");

        board.Name = name;
        // Enforce optimistic concurrency against the version the caller last saw.
        _dbContext.Entry(board).Property(b => b.RowVersion).OriginalValue = rowVersion;

        try
        {
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("This board was modified by someone else. Please reload and try again.");
        }

        return new ListBoardSummaryDto(board.Id, board.Name, board.Items.Count, Convert.ToBase64String(board.RowVersion));
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var board = await _dbContext.Boards.FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new NotFoundException($"Board {id} was not found.");

        _dbContext.Boards.Remove(board);
        await _dbContext.SaveChangesAsync(ct);
    }
}
