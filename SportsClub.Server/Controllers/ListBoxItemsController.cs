using Microsoft.AspNetCore.Mvc;
using SportsClub.Server.Services;
using SportsClub.Shared;

namespace SportsClub.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListBoxItemsController : ControllerBase
{
    private readonly IListBoxItemService _service;

    public ListBoxItemsController(IListBoxItemService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ListBoxItemDto>>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
        => Ok(await _service.GetAsync(page, pageSize, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ListBoxItemDto>> GetById(int id, CancellationToken ct = default)
    {
        var dto = await _service.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ListBoxItemDto>> Add(
        [FromBody] CreateListBoxItemRequest request,
        CancellationToken ct = default)
    {
        // [ApiController] already enforces the DTO's data annotations (400 on invalid).
        // Extra guard: whitespace-only input passes [Required] but trims to empty.
        var text = (request.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(text))
            return BadRequest("Text is required.");

        var dto = await _service.AddAsync(text, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        => await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
