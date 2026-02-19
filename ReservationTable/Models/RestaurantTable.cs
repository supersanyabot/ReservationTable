using System.ComponentModel.DataAnnotations;

namespace ReservationTable.Models;

public class RestaurantTable
{
    public int Id { get; set; }

    [Required]
    [MaxLength(10)]
    public string TableCode { get; set; } = string.Empty;

    public int ZoneId { get; set; }
    public TableZone? Zone { get; set; }

    public TableStatus Status { get; set; } = TableStatus.Available;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
