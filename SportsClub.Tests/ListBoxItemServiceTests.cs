using Microsoft.EntityFrameworkCore;
using SportsClub.Server;
using SportsClub.Server.Data;
using SportsClub.Server.Services;
using Xunit;

namespace SportsClub.Tests;

public class ListBoxItemServiceTests
{
    private static ListBoxDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<ListBoxDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ListBoxDbContext(options);
    }

    private static async Task<int> SeedBoardAsync(ListBoxDbContext db, string name = "Board")
    {
        var board = new ListBoardEntity { Name = name };
        db.Boards.Add(board);
        await db.SaveChangesAsync();
        return board.Id;
    }

    [Fact]
    public async Task AddAsync_trims_text_and_assigns_next_sort_order_within_board()
    {
        using var db = NewDb();
        var boardId = await SeedBoardAsync(db);
        db.Items.Add(new ListBoxItemEntity { Text = "Existing", SortOrder = 5, ListBoardId = boardId });
        await db.SaveChangesAsync();
        var sut = new ListBoxItemService(db);

        var dto = await sut.AddAsync(boardId, "  Yoga  ");

        Assert.Equal("Yoga", dto.Text);
        Assert.Equal(6, dto.SortOrder);
    }

    [Fact]
    public async Task AddAsync_throws_not_found_for_missing_board()
    {
        using var db = NewDb();
        var sut = new ListBoxItemService(db);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.AddAsync(999, "x"));
    }

    [Fact]
    public async Task GetByBoardAsync_returns_only_that_boards_items_ordered()
    {
        using var db = NewDb();
        var a = await SeedBoardAsync(db, "A");
        var b = await SeedBoardAsync(db, "B");
        db.Items.AddRange(
            new ListBoxItemEntity { Text = "a2", SortOrder = 2, ListBoardId = a },
            new ListBoxItemEntity { Text = "a1", SortOrder = 1, ListBoardId = a },
            new ListBoxItemEntity { Text = "b1", SortOrder = 1, ListBoardId = b });
        await db.SaveChangesAsync();
        var sut = new ListBoxItemService(db);

        var items = await sut.GetByBoardAsync(a);

        Assert.Equal(new[] { "a1", "a2" }, items.Select(i => i.Text));
    }

    [Fact]
    public async Task UpdateAsync_changes_text()
    {
        using var db = NewDb();
        var boardId = await SeedBoardAsync(db);
        var item = new ListBoxItemEntity { Text = "Old", SortOrder = 1, ListBoardId = boardId };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        var sut = new ListBoxItemService(db);

        var dto = await sut.UpdateAsync(boardId, item.Id, "New", item.RowVersion);

        Assert.Equal("New", dto.Text);
    }

    [Fact]
    public async Task UpdateAsync_throws_not_found_when_item_is_on_a_different_board()
    {
        using var db = NewDb();
        var a = await SeedBoardAsync(db, "A");
        var b = await SeedBoardAsync(db, "B");
        var item = new ListBoxItemEntity { Text = "x", SortOrder = 1, ListBoardId = a };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        var sut = new ListBoxItemService(db);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.UpdateAsync(b, item.Id, "y", item.RowVersion));
    }

    [Fact]
    public async Task DeleteAsync_removes_item()
    {
        using var db = NewDb();
        var boardId = await SeedBoardAsync(db);
        var item = new ListBoxItemEntity { Text = "x", SortOrder = 1, ListBoardId = boardId };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        var sut = new ListBoxItemService(db);

        await sut.DeleteAsync(boardId, item.Id);

        Assert.Empty(db.Items);
    }

    [Fact]
    public async Task DeleteAsync_throws_not_found_when_item_missing()
    {
        using var db = NewDb();
        var boardId = await SeedBoardAsync(db);
        var sut = new ListBoxItemService(db);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.DeleteAsync(boardId, 999));
    }
}
