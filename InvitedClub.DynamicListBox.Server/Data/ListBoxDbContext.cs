using Microsoft.EntityFrameworkCore;

namespace InvitedClub.DynamicListBox.Server.Data;

public class ListBoxDbContext : DbContext
{
    public ListBoxDbContext(DbContextOptions<ListBoxDbContext> options) : base(options) { }

    public DbSet<ListBoxItemEntity> Items => Set<ListBoxItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ListBoxItemEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Text).IsRequired().HasMaxLength(200);
            e.Property(x => x.SortOrder).IsRequired();
            e.HasIndex(x => x.SortOrder);
        });
    }
}
