using System.ComponentModel.DataAnnotations;

namespace ReservationTable.ViewModels;

public class CreateReservationViewModel
{
    [Required(ErrorMessage = "กรุณากรอกชื่อ")]
    [RegularExpression(@"^[^\d]+$", ErrorMessage = "ชื่อห้ามมีตัวเลข")]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณากรอกเบอร์โทร")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "เบอร์โทรต้องเป็นตัวเลข 10 หลักและขึ้นต้นด้วย 0")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณาเลือกโซนโต๊ะ")]
    public int? RestaurantTableId { get; set; }
}
