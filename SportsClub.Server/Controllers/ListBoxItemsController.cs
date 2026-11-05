using Microsoft.AspNetCore.Mvc;
using SportsClub.Server.Services;
using SportsClub.Shared;

namespace SportsClub.Server.Controllers;

[ApiController]
[Route("api/boards/{boardId:int}/items")]
public class ListBoxItemsController : ControllerBase
{
    private readonly IListBoxItemService _service;

    public ListBoxItemsController(IListBoxItemService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ListBoxItemDto>>> Get(int boardId, CancellationToken ct)
        => Ok(await _service.GetByBoardAsync(boardId, ct));

    [HttpPost]
    public async Task<ActionResult<ListBoxItemDto>> Add(int boardId, [FromBody] CreateListBoxItemRequest request, CancellationToken ct)
    {
        // [ApiController] already enforces the annotations; extra guard for whitespace-only.
        var text = (request.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(text))
            return BadRequest("Text is required.");

        var dto = await _service.AddAsync(boardId, text, ct);
        return CreatedAtAction(nameof(Get), new { boardId }, dto);
    }

    [HttpPut("{itemId:int}")]
    public async Task<ActionResult<ListBoxItemDto>> Update(int boardId, int itemId, [FromBody] UpdateListBoxItemRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(boardId, itemId, request.Text, Convert.FromBase64String(request.RowVersion), ct));

    [HttpDelete("{itemId:int}")]
    public async Task<IActionResult> Delete(int boardId, int itemId, CancellationToken ct)
    {
        await _service.DeleteAsync(boardId, itemId, ct);
        return NoContent();
    }
}
