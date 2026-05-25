namespace Lihtar.Web.Areas.Admin.ViewModels.TableBoard;

public class TableBoardItemVm
{
    public Guid TableId { get; set; }

    public int TableNumber { get; set; }

    public int Seats { get; set; }

    public bool IsVip { get; set; }

    public bool IsActive { get; set; }

    public string Status { get; set; } = "Free";

    public Guid? ReservationId { get; set; }

    public DateTime? ReservationDate { get; set; }

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    public string? Comment { get; set; }
}