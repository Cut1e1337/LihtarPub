namespace Lihtar.Application.DTOs;

public class TableDto
{
    public Guid Id { get; set; }

    public int TableNumber { get; set; }

    public int Seats { get; set; }

    public bool IsVip { get; set; }

    public bool IsActive { get; set; } = true;
}