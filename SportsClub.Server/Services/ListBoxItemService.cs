using Microsoft.EntityFrameworkCore;
using SportsClub.Server.Data;
using SportsClub.Shared;

namespace SportsClub.Server.Services;

public class ListBoxItemService : IListBoxItemService
{
    private const int MaxPageSize = 200;

    private readonly ListBoxDbContext _dbContext;

    public ListBoxItemService(ListBoxDbContext dbContext) => _dbContext = dbContext;

    public async Task<PagedResult<ListBoxItemDto>> GetAsync(int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = _dbContext.Items.OrderBy(x => x.SortOrder);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ListBoxItemDto(x.Id, x.Text, x.SortOrder))
            .ToListAsync(ct);

        return new PagedResult<ListBoxItemDto>(items, page, pageSize, totalCount);
    }

    public async Task<ListBoxItemDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _dbContext.Items
            .Where(x => x.Id == id)
            .Select(x => new ListBoxItemDto(x.Id, x.Text, x.SortOrder))
            .FirstOrDefaultAsync(ct);

    public async Task<ListBoxItemDto> AddAsync(string text, CancellationToken ct = default)
    {
        text = text.Trim();

        var maxSort = await _dbContext.Items.MaxAsync(x => (int?)x.SortOrder, ct) ?? 0;

        var entity = new ListBoxItemEntity
        {
            Text = text,
            SortOrder = maxSort + 1
        };

        _dbContext.Items.Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        return new ListBoxItemDto(entity.Id, entity.Text, entity.SortOrder);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _dbContext.Items.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        _dbContext.Items.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }
}
