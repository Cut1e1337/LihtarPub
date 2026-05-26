using Lihtar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lihtar.Infrastructure.Data.Configurations;

public class FavoriteMenuItemConfiguration : IEntityTypeConfiguration<FavoriteMenuItem>
{
    public void Configure(EntityTypeBuilder<FavoriteMenuItem> b)
    {
        b.HasKey(x => x.Id);

        b.HasIndex(x => new { x.UserId, x.MenuItemId })
            .IsUnique();

        b.HasOne(x => x.MenuItem)
            .WithMany(x => x.Favorites)
            .HasForeignKey(x => x.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}