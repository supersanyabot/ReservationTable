using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationTable.Data;
using ReservationTable.Models;
using ReservationTable.ViewModels;

namespace ReservationTable.Controllers;

public class ReservationController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var tables = await dbContext.RestaurantTables
            .Include(t => t.Reservations)
            .OrderBy(t => t.ZoneCode)
            .ToListAsync();

        var vm = new ReservationPageViewModel
        {
            Tables = tables
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationPageViewModel vm)
    {
        vm.Tables = await dbContext.RestaurantTables
            .Include(t => t.Reservations)
            .OrderBy(t => t.ZoneCode)
            .ToListAsync();

        if (!ModelState.IsValid)
        {
            return View("Index", vm);
        }

        var tableId = vm.ReservationForm.RestaurantTableId!.Value;
        var table = await dbContext.RestaurantTables.FirstOrDefaultAsync(t => t.Id == tableId);
        if (table is null || table.Status != TableStatus.Available)
        {
            ModelState.AddModelError("ReservationForm.RestaurantTableId", "โต๊ะนี้ไม่ว่าง กรุณาเลือกโต๊ะอื่น");
            return View("Index", vm);
        }

        var reservationCode = await GenerateReservationCodeAsync(table.ZoneCode);
        var reservation = new Reservation
        {
            ReservationCode = reservationCode,
            CustomerName = vm.ReservationForm.CustomerName.Trim(),
            PhoneNumber = vm.ReservationForm.PhoneNumber.Trim(),
            RestaurantTableId = table.Id
        };

        table.Status = TableStatus.Reserved;
        dbContext.Reservations.Add(reservation);
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = $"จองสำเร็จ Reservation Code: {reservationCode}";
        return RedirectToAction(nameof(Index));
    }

    private async Task<string> GenerateReservationCodeAsync(string zoneCode)
    {
        var todayPrefix = DateTime.UtcNow.ToString("ddMM");
        var prefix = $"{todayPrefix}{zoneCode.ToUpper()}";

        var lastReservationCode = await dbContext.Reservations
            .Where(r => r.ReservationCode.StartsWith(prefix))
            .OrderByDescending(r => r.ReservationCode)
            .Select(r => r.ReservationCode)
            .FirstOrDefaultAsync();

        var sequence = 1;
        if (!string.IsNullOrWhiteSpace(lastReservationCode) && lastReservationCode.Length >= 3)
        {
            var seqText = lastReservationCode[^3..];
            if (int.TryParse(seqText, out var lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"{prefix}{sequence:000}";
    }
}
