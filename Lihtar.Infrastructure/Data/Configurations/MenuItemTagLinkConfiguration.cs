using Lihtar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lihtar.Infrastructure.Data.Configurations;

public class MenuItemTagLinkConfiguration : IEntityTypeConfiguration<MenuItemTagLink>
{
    public void Configure(EntityTypeBuilder<MenuItemTagLink> b)
    {
        b.HasKey(x => new { x.MenuItemId, x.TagId });

        b.HasOne(x => x.MenuItem)
            .WithMany(x => x.TagLinks)
            .HasForeignKey(x => x.MenuItemId);

        b.HasOne(x => x.Tag)
            .WithMany(x => x.MenuItemLinks)
            .HasForeignKey(x => x.TagId);
    }
}
