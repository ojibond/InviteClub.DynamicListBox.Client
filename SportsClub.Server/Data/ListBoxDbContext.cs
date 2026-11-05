using Microsoft.EntityFrameworkCore;

namespace SportsClub.Server.Data;

public class ListBoxDbContext : DbContext
{
    public ListBoxDbContext(DbContextOptions<ListBoxDbContext> options) : base(options) { }

    public DbSet<ListBoardEntity> Boards => Set<ListBoardEntity>();
    public DbSet<ListBoxItemEntity> Items => Set<ListBoxItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ListBoardEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasIndex(x => x.Name).IsUnique();

            e.HasMany(x => x.Items)
             .WithOne(x => x.Board!)
             .HasForeignKey(x => x.ListBoardId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ListBoxItemEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Text).IsRequired().HasMaxLength(200);
            e.Property(x => x.SortOrder).IsRequired();
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasIndex(x => new { x.ListBoardId, x.SortOrder });
        });
    }
}
