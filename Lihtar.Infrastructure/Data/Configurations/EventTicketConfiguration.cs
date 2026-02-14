using Lihtar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lihtar.Infrastructure.Data.Configurations;

public class EventTicketConfiguration : IEntityTypeConfiguration<EventTicket>
{
    public void Configure(EntityTypeBuilder<EventTicket> builder)
    {
        builder.ToTable("EventTickets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.QRCode)
            .IsRequired()
            .HasMaxLength(256);

        // (опційно) індекси для швидкого пошуку квитків
        builder.HasIndex(x => x.EventId);
        builder.HasIndex(x => x.UserId);
    }
}
