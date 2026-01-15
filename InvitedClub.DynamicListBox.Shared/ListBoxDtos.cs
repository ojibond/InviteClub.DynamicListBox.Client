namespace InvitedClub.DynamicListBox.Shared;

public record ListBoxItemDto(int Id, string Text, int SortOrder);

public record CreateListBoxItemRequest(string Text);
