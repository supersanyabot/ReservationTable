namespace ReservationTable.Models;

public static class TableStatusExtensions
{
    public static string ToDisplayText(this TableStatus status) => status switch
    {
        TableStatus.Available => "ว่าง",
        TableStatus.Reserved => "จองแล้ว",
        _ => "ไม่ทราบสถานะ"
    };

    public static string ToDetailText(this TableStatus status) => status switch
    {
        TableStatus.Available => "พร้อมรับการจอง",
        TableStatus.Reserved => "มีการจองอยู่",
        _ => "-"
    };

    public static string ToBadgeCss(this TableStatus status) => status switch
    {
        TableStatus.Available => "bg-success",
        TableStatus.Reserved => "bg-warning text-dark",
        _ => "bg-secondary"
    };

    public static string ToTableCardCss(this TableStatus status) => status switch
    {
        TableStatus.Available => "table-card-soft table-card-available",
        TableStatus.Reserved => "table-card-soft table-card-reserved",
        _ => "table-card-soft"
    };
}
