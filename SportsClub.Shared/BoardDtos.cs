using System.ComponentModel.DataAnnotations;

namespace SportsClub.Shared;

public record ListBoardSummaryDto(int Id, string Name, int ItemCount, string RowVersion);

public record ListBoardDetailDto(int Id, string Name, string RowVersion, IReadOnlyList<ListBoxItemDto> Items);

public record CreateListBoardRequest(
    [property: Required]
    [property: StringLength(100, MinimumLength = 1, ErrorMessage = "Name is required and must be 1-100 characters.")]
    string Name);

public record UpdateListBoardRequest(
    [property: Required]
    [property: StringLength(100, MinimumLength = 1, ErrorMessage = "Name is required and must be 1-100 characters.")]
    string Name,
    [property: Required] string RowVersion);
