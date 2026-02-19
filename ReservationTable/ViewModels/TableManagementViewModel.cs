using System.ComponentModel.DataAnnotations;
using ReservationTable.Models;

namespace ReservationTable.ViewModels;

public class TableManagementViewModel
{
    public List<TableZone> Zones { get; set; } = new();
    public List<RestaurantTable> Tables { get; set; } = new();

    public AddZoneViewModel AddZoneForm { get; set; } = new();
    public AddTableViewModel AddTableForm { get; set; } = new();
    public DeleteZoneViewModel DeleteZoneForm { get; set; } = new();
}

public class AddZoneViewModel
{
    [Required(ErrorMessage = "กรุณากรอกรหัสโซน")]
    [StringLength(10, ErrorMessage = "รหัสโซนต้องไม่เกิน 10 ตัวอักษร")]
    [RegularExpression(@"^[A-Za-z]+[0-9]+$", ErrorMessage = "รหัสโซนต้องเป็นรูปแบบตัวอักษรตามด้วยตัวเลข เช่น A1, B1, VIP1")]
    public string ZoneCode { get; set; } = string.Empty;
}

public class AddTableViewModel
{
    [Required(ErrorMessage = "กรุณาเลือกโซน")]
    public int? ZoneId { get; set; }

    [Required(ErrorMessage = "กรุณากรอกรหัสโต๊ะ")]
    [StringLength(10, ErrorMessage = "รหัสโต๊ะต้องไม่เกิน 10 ตัวอักษร")]
    [RegularExpression(@"^\d+$", ErrorMessage = "รหัสโต๊ะต้องเป็นตัวเลขลำดับเท่านั้น เช่น 1, 2, 10")]
    public string TableCode { get; set; } = string.Empty;
}

public class DeleteZoneViewModel
{
    [Required(ErrorMessage = "กรุณาเลือกโซนที่ต้องการลบ")]
    public int? ZoneId { get; set; }
}
