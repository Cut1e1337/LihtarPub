using Lihtar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lihtar.Infrastructure.Data.Configurations;

public class MenuItemReviewConfiguration : IEntityTypeConfiguration<MenuItemReview>
{
    public void Configure(EntityTypeBuilder<MenuItemReview> b)
    {
        b.HasKey(x => new { x.ReviewId, x.MenuItemId });

        b.HasOne(x => x.Review)
            .WithMany(x => x.MenuItemReviews)
            .HasForeignKey(x => x.ReviewId);

        b.HasOne(x => x.MenuItem)
            .WithMany(x => x.MenuItemReviews)
            .HasForeignKey(x => x.MenuItemId);
    }
}
