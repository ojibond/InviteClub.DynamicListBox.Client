using InvitedClub.DynamicListBox.Server.Data;
using InvitedClub.DynamicListBox.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvitedClub.DynamicListBox.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListBoxItemsController : ControllerBase
{
    private readonly ListBoxDbContext _dbContext;
    public ListBoxItemsController(ListBoxDbContext dbContext) => _dbContext = dbContext;

    [HttpGet]
    public async Task<ActionResult<List<ListBoxItemDto>>> Get()
    {
        var rows = await _dbContext.Items
            .OrderBy(x => x.SortOrder)
            .Select(x => new ListBoxItemDto(x.Id, x.Text, x.SortOrder))
            .ToListAsync();

        return rows;
    }

    [HttpPost]
    public async Task<ActionResult<ListBoxItemDto>> Add([FromBody] CreateListBoxItemRequest request)
    {
        var text = (request.Text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(text))
            return BadRequest("Text is required.");

        var maxSort = await _dbContext.Items.MaxAsync(x => (int?)x.SortOrder) ?? 0;

        var entity = new ListBoxItemEntity
        {
            Text = text,
            SortOrder = maxSort + 1
        };

        _dbContext.Items.Add(entity);
        await _dbContext.SaveChangesAsync();

        var dto = new ListBoxItemDto(entity.Id, entity.Text, entity.SortOrder);

        return CreatedAtAction(nameof(Get), new { id = entity.Id }, dto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteListBox(int id)
    {
        var entity = await _dbContext.Items.FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return NotFound();

        _dbContext.Items.Remove(entity);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
