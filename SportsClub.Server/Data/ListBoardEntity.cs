namespace SportsClub.Server.Data;

/// <summary>
/// A named list ("board") that owns many <see cref="ListBoxItemEntity"/> items.
/// </summary>
public class ListBoardEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Optimistic concurrency token (SQL Server rowversion).
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<ListBoxItemEntity> Items { get; set; } = new List<ListBoxItemEntity>();
}
