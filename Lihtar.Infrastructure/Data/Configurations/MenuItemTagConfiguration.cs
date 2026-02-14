using Lihtar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lihtar.Infrastructure.Data.Configurations;

public class MenuItemTagConfiguration : IEntityTypeConfiguration<MenuItemTag>
{
    public void Configure(EntityTypeBuilder<MenuItemTag> builder)
    {
        builder.ToTable("MenuItemTags");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
