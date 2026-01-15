namespace InvitedClub.DynamicListBox.Server.Data;

public class ListBoxItemEntity
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
