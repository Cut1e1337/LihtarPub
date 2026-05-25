using Lihtar.Domain.Enums;

namespace Lihtar.Domain.Entities;

public class Reservation
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public Guid TableId { get; set; }
    public Table? Table { get; set; }

    public DateTime ReservationDate { get; set; } // дата броні
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public string? Comment { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


}
