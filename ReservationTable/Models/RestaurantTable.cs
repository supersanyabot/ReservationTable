using System.ComponentModel.DataAnnotations;

namespace ReservationTable.Models;

public class RestaurantTable
{
    public int Id { get; set; }

    [Required]
    [MaxLength(10)]
    public string ZoneCode { get; set; } = string.Empty;

    public TableStatus Status { get; set; } = TableStatus.Available;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
