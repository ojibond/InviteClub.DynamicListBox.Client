using Microsoft.EntityFrameworkCore;
using SportsClub.Server.Data;
using SportsClub.Shared;

namespace SportsClub.Server.Services;

public class ListBoxItemService : IListBoxItemService
{
    private readonly ListBoxDbContext _dbContext;

    public ListBoxItemService(ListBoxDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<ListBoxItemDto>> GetByBoardAsync(int boardId, CancellationToken ct = default)
    {
        await EnsureBoardExistsAsync(boardId, ct);

        return await _dbContext.Items
            .Where(i => i.ListBoardId == boardId)
            .OrderBy(i => i.SortOrder)
            .Select(i => new ListBoxItemDto(i.Id, i.Text, i.SortOrder, Convert.ToBase64String(i.RowVersion)))
            .ToListAsync(ct);
    }

    public async Task<ListBoxItemDto> AddAsync(int boardId, string text, CancellationToken ct = default)
    {
        await EnsureBoardExistsAsync(boardId, ct);

        text = text.Trim();
        var maxSort = await _dbContext.Items
            .Where(i => i.ListBoardId == boardId)
            .MaxAsync(i => (int?)i.SortOrder, ct) ?? 0;

        var entity = new ListBoxItemEntity
        {
            Text = text,
            SortOrder = maxSort + 1,
            ListBoardId = boardId
        };

        _dbContext.Items.Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        return new ListBoxItemDto(entity.Id, entity.Text, entity.SortOrder, Convert.ToBase64String(entity.RowVersion));
    }

    public async Task<ListBoxItemDto> UpdateAsync(int boardId, int itemId, string text, byte[] rowVersion, CancellationToken ct = default)
    {
        var item = await _dbContext.Items
            .FirstOrDefaultAsync(i => i.Id == itemId && i.ListBoardId == boardId, ct)
            ?? throw new NotFoundException($"Item {itemId} was not found on board {boardId}.");

        item.Text = text.Trim();
        _dbContext.Entry(item).Property(i => i.RowVersion).OriginalValue = rowVersion;

        try
        {
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("This item was modified by someone else. Please reload and try again.");
        }

        return new ListBoxItemDto(item.Id, item.Text, item.SortOrder, Convert.ToBase64String(item.RowVersion));
    }

    public async Task DeleteAsync(int boardId, int itemId, CancellationToken ct = default)
    {
        var item = await _dbContext.Items
            .FirstOrDefaultAsync(i => i.Id == itemId && i.ListBoardId == boardId, ct)
            ?? throw new NotFoundException($"Item {itemId} was not found on board {boardId}.");

        _dbContext.Items.Remove(item);
        await _dbContext.SaveChangesAsync(ct);
    }

    private async Task EnsureBoardExistsAsync(int boardId, CancellationToken ct)
    {
        if (!await _dbContext.Boards.AnyAsync(b => b.Id == boardId, ct))
            throw new NotFoundException($"Board {boardId} was not found.");
    }
}
