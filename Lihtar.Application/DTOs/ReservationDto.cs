using Lihtar.Domain.Enums;

namespace Lihtar.Application.DTOs;

public class ReservationDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid TableId { get; set; }

    public int TableNumber { get; set; }

    public int Seats { get; set; }

    public DateTime ReservationDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string? Comment { get; set; }

    public ReservationStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}