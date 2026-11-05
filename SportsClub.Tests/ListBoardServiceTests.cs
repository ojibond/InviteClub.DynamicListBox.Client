using Microsoft.EntityFrameworkCore;
using SportsClub.Server;
using SportsClub.Server.Data;
using SportsClub.Server.Services;
using Xunit;

namespace SportsClub.Tests;

public class ListBoardServiceTests
{
    private static ListBoxDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<ListBoxDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ListBoxDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_trims_and_persists_board()
    {
        using var db = NewDb();
        var sut = new ListBoardService(db);

        var dto = await sut.CreateAsync("  Tennis League  ");

        Assert.Equal("Tennis League", dto.Name);
        Assert.Equal(0, dto.ItemCount);
        Assert.Single(db.Boards);
    }

    [Fact]
    public async Task CreateAsync_throws_conflict_on_duplicate_name()
    {
        using var db = NewDb();
        db.Boards.Add(new ListBoardEntity { Name = "Aquatics" });
        await db.SaveChangesAsync();
        var sut = new ListBoardService(db);

        await Assert.ThrowsAsync<ConflictException>(() => sut.CreateAsync("Aquatics"));
    }

    [Fact]
    public async Task GetByIdAsync_throws_not_found_when_missing()
    {
        using var db = NewDb();
        var sut = new ListBoardService(db);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetByIdAsync(123));
    }

    [Fact]
    public async Task GetAllAsync_orders_by_name_with_item_counts()
    {
        using var db = NewDb();
        db.Boards.Add(new ListBoardEntity
        {
            Name = "Zumba",
            Items = { new ListBoxItemEntity { Text = "A", SortOrder = 1 } }
        });
        db.Boards.Add(new ListBoardEntity { Name = "Aquatics" });
        await db.SaveChangesAsync();
        var sut = new ListBoardService(db);

        var all = await sut.GetAllAsync();

        Assert.Equal(new[] { "Aquatics", "Zumba" }, all.Select(b => b.Name));
        Assert.Equal(0, all[0].ItemCount);
        Assert.Equal(1, all[1].ItemCount);
    }

    [Fact]
    public async Task RenameAsync_changes_name()
    {
        using var db = NewDb();
        var board = new ListBoardEntity { Name = "Old" };
        db.Boards.Add(board);
        await db.SaveChangesAsync();
        var sut = new ListBoardService(db);

        var dto = await sut.RenameAsync(board.Id, "New", board.RowVersion);

        Assert.Equal("New", dto.Name);
    }

    [Fact]
    public async Task RenameAsync_throws_conflict_on_duplicate_name()
    {
        using var db = NewDb();
        db.Boards.Add(new ListBoardEntity { Name = "Aquatics" });
        var board = new ListBoardEntity { Name = "Fitness" };
        db.Boards.Add(board);
        await db.SaveChangesAsync();
        var sut = new ListBoardService(db);

        await Assert.ThrowsAsync<ConflictException>(() => sut.RenameAsync(board.Id, "Aquatics", board.RowVersion));
    }

    [Fact]
    public async Task DeleteAsync_removes_board()
    {
        using var db = NewDb();
        var board = new ListBoardEntity { Name = "Temp" };
        db.Boards.Add(board);
        await db.SaveChangesAsync();
        var sut = new ListBoardService(db);

        await sut.DeleteAsync(board.Id);

        Assert.Empty(db.Boards);
    }

    [Fact]
    public async Task DeleteAsync_throws_not_found_when_missing()
    {
        using var db = NewDb();
        var sut = new ListBoardService(db);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.DeleteAsync(999));
    }
}
