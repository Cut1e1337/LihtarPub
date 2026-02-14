using Lihtar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lihtar.Infrastructure.Data.Configurations;

public class EventReviewConfiguration : IEntityTypeConfiguration<EventReview>
{
    public void Configure(EntityTypeBuilder<EventReview> builder)
    {
        builder.ToTable("EventReviews");

        builder.HasKey(x => new { x.EventId, x.ReviewId });

        builder.HasOne(x => x.Event)
            .WithMany(x => x.EventReviews)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Review)
            .WithMany() // якщо в Review немає колекції EventReviews — так і треба
            .HasForeignKey(x => x.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
