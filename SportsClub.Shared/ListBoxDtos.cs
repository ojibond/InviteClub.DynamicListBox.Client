using System.ComponentModel.DataAnnotations;

namespace SportsClub.Shared;

public record ListBoxItemDto(int Id, string Text, int SortOrder, string RowVersion);

public record CreateListBoxItemRequest(
    [property: Required]
    [property: StringLength(200, MinimumLength = 1, ErrorMessage = "Text is required and must be 1-200 characters.")]
    string Text);

public record UpdateListBoxItemRequest(
    [property: Required]
    [property: StringLength(200, MinimumLength = 1, ErrorMessage = "Text is required and must be 1-200 characters.")]
    string Text,
    [property: Required] string RowVersion);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
