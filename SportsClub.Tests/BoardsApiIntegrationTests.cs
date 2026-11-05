using System.Net;
using System.Net.Http.Json;
using SportsClub.Shared;
using Xunit;

namespace SportsClub.Tests;

public class BoardsApiIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public BoardsApiIntegrationTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task GetBoards_returns_seeded_boards()
    {
        var boards = await _client.GetFromJsonAsync<List<ListBoardSummaryDto>>("/api/boards");

        Assert.NotNull(boards);
        Assert.Contains(boards!, b => b.Name == "Racquet Sports");
    }

    [Fact]
    public async Task GetBoard_by_missing_id_returns_404_problem_details()
    {
        var resp = await _client.GetAsync("/api/boards/99999");

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        Assert.Equal("application/problem+json", resp.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task CreateBoard_with_blank_name_returns_400()
    {
        var resp = await _client.PostAsJsonAsync("/api/boards", new CreateListBoardRequest(""));

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task CreateBoard_then_duplicate_name_returns_409()
    {
        var name = "Board-" + Guid.NewGuid().ToString("N")[..8];

        var created = await _client.PostAsJsonAsync("/api/boards", new CreateListBoardRequest(name));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var duplicate = await _client.PostAsJsonAsync("/api/boards", new CreateListBoardRequest(name));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task AddItem_to_existing_board_returns_201()
    {
        var boards = await _client.GetFromJsonAsync<List<ListBoardSummaryDto>>("/api/boards");
        var boardId = boards!.First().Id;

        var resp = await _client.PostAsJsonAsync(
            $"/api/boards/{boardId}/items", new CreateListBoxItemRequest("New Activity"));

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task AddItem_to_missing_board_returns_404()
    {
        var resp = await _client.PostAsJsonAsync(
            "/api/boards/99999/items", new CreateListBoxItemRequest("x"));

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }
}
