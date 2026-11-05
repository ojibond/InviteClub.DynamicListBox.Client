namespace SportsClub.Server.Data;

public class ListBoxItemEntity
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Owning board (one-to-many).
    public int ListBoardId { get; set; }
    public ListBoardEntity? Board { get; set; }

    // Optimistic concurrency token (SQL Server rowversion).
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
