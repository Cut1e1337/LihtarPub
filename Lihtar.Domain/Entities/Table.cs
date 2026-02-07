namespace Lihtar.Domain.Entities;

public class Table
{
    public Guid Id { get; set; }
    public int TableNumber { get; set; }
    public int Seats { get; set; }
    public bool IsVip { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
