using Lihtar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lihtar.Infrastructure.Data.Configurations;

public class EventCategoryConfiguration : IEntityTypeConfiguration<EventCategory>
{
    public void Configure(EntityTypeBuilder<EventCategory> builder)
    {
        builder.ToTable("EventCategories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasMany(x => x.Events)
            .WithOne(x => x.EventCategory)
            .HasForeignKey(x => x.EventCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // (опційно) уникальний індекс по назві
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
