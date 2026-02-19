using System.ComponentModel.DataAnnotations;

namespace ReservationTable.Models;

public class TableZone
{
    public int Id { get; set; }

    [Required]
    [MaxLength(10)]
    public string ZoneCode { get; set; } = string.Empty;

    public ICollection<RestaurantTable> Tables { get; set; } = new List<RestaurantTable>();
}
