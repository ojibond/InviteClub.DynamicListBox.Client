using Microsoft.EntityFrameworkCore;
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

    [Fact]
    public async Task AddAsync_trims_text_and_assigns_next_sort_order()
    {
        using var db = NewDb();
        db.Items.Add(new ListBoxItemEntity { Text = "Golf", SortOrder = 5 });
        await db.SaveChangesAsync();
        var sut = new ListBoxItemService(db);

        var dto = await sut.AddAsync("  Tennis  ");

        Assert.Equal("Tennis", dto.Text);
        Assert.Equal(6, dto.SortOrder);
    }

    [Fact]
    public async Task AddAsync_on_empty_db_starts_sort_order_at_one()
    {
        using var db = NewDb();
        var sut = new ListBoxItemService(db);

        var dto = await sut.AddAsync("First");

        Assert.Equal(1, dto.SortOrder);
    }

    [Fact]
    public async Task GetAsync_orders_by_sort_order_and_reports_total()
    {
        using var db = NewDb();
        db.Items.AddRange(
            new ListBoxItemEntity { Text = "B", SortOrder = 2 },
            new ListBoxItemEntity { Text = "A", SortOrder = 1 },
            new ListBoxItemEntity { Text = "C", SortOrder = 3 });
        await db.SaveChangesAsync();
        var sut = new ListBoxItemService(db);

        var result = await sut.GetAsync(page: 1, pageSize: 10);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(new[] { "A", "B", "C" }, result.Items.Select(x => x.Text));
    }

    [Fact]
    public async Task GetAsync_returns_requested_page()
    {
        using var db = NewDb();
        for (var i = 1; i <= 5; i++)
            db.Items.Add(new ListBoxItemEntity { Text = $"Item {i}", SortOrder = i });
        await db.SaveChangesAsync();
        var sut = new ListBoxItemService(db);

        var page2 = await sut.GetAsync(page: 2, pageSize: 2);

        Assert.Equal(5, page2.TotalCount);
        Assert.Equal(2, page2.Page);
        Assert.Equal(new[] { "Item 3", "Item 4" }, page2.Items.Select(x => x.Text));
    }

    [Fact]
    public async Task DeleteAsync_returns_false_when_item_missing()
    {
        using var db = NewDb();
        var sut = new ListBoxItemService(db);

        Assert.False(await sut.DeleteAsync(999));
    }

    [Fact]
    public async Task DeleteAsync_removes_existing_item()
    {
        using var db = NewDb();
        var entity = new ListBoxItemEntity { Text = "Golf", SortOrder = 1 };
        db.Items.Add(entity);
        await db.SaveChangesAsync();
        var sut = new ListBoxItemService(db);

        var removed = await sut.DeleteAsync(entity.Id);

        Assert.True(removed);
        Assert.Empty(db.Items);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_missing()
    {
        using var db = NewDb();
        var sut = new ListBoxItemService(db);

        Assert.Null(await sut.GetByIdAsync(123));
    }
}
