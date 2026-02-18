using ReservationTable.Models;

namespace ReservationTable.ViewModels;

public class ReservationPageViewModel
{
    public List<RestaurantTable> Tables { get; set; } = new();
    public CreateReservationViewModel ReservationForm { get; set; } = new();
}
