using System.ComponentModel.DataAnnotations;
using ReservationTable.Models;

namespace ReservationTable.ViewModels;

public class TableManagementViewModel
{
    public List<RestaurantTable> Tables { get; set; } = new();

    [Required(ErrorMessage = "กรุณากรอกชื่อโซนโต๊ะ")]
    [StringLength(10, ErrorMessage = "ชื่อโซนโต๊ะต้องไม่เกิน 10 ตัวอักษร")]
    [RegularExpression(@"^[a-zA-Z]+\d+$", ErrorMessage = "รูปแบบโซนไม่ถูกต้อง เช่น A1, B12, VIP1")]
    public string NewZoneCode { get; set; } = string.Empty;
}
