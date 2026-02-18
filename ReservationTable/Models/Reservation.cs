using System.ComponentModel.DataAnnotations;

namespace ReservationTable.Models;

public class Reservation
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string ReservationCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    public int RestaurantTableId { get; set; }
    public RestaurantTable? RestaurantTable { get; set; }
}
