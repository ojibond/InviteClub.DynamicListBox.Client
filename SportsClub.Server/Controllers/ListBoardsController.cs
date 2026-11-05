using Microsoft.AspNetCore.Mvc;
using SportsClub.Server.Services;
using SportsClub.Shared;

namespace SportsClub.Server.Controllers;

[ApiController]
[Route("api/boards")]
public class ListBoardsController : ControllerBase
{
    private readonly IListBoardService _service;

    public ListBoardsController(IListBoardService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ListBoardSummaryDto>>> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ListBoardDetailDto>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<ListBoardSummaryDto>> Create([FromBody] CreateListBoardRequest request, CancellationToken ct)
    {
        var dto = await _service.CreateAsync(request.Name, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ListBoardSummaryDto>> Rename(int id, [FromBody] UpdateListBoardRequest request, CancellationToken ct)
        => Ok(await _service.RenameAsync(id, request.Name, Convert.FromBase64String(request.RowVersion), ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
