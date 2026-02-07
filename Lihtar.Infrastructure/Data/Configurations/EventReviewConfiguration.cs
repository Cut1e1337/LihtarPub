using Lihtar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lihtar.Infrastructure.Data.Configurations;

public class EventReviewConfiguration : IEntityTypeConfiguration<EventReview>
{
    public void Configure(EntityTypeBuilder<EventReview> b)
    {
        b.HasKey(x => new { x.ReviewId, x.EventId });

        b.HasOne(x => x.Review)
            .WithMany(x => x.EventReviews)
            .HasForeignKey(x => x.ReviewId);

        b.HasOne(x => x.Event)
            .WithMany(x => x.EventReviews)
            .HasForeignKey(x => x.EventId);
    }
}
