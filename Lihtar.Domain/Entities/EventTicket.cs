using Lihtar.Domain.Enums;

namespace Lihtar.Domain.Entities;

public class EventTicket
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }
    public Event? Event { get; set; }

    public Guid UserId { get; set; } // Identity user
    public decimal Price { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    public string QRCode { get; set; } = default!;
    public TicketStatus Status { get; set; } = TicketStatus.Active;
}
