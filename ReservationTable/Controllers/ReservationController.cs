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
        var vm = new ReservationPageViewModel
        {
            Zones = await dbContext.TableZones.OrderBy(z => z.ZoneCode).ToListAsync(),
            Tables = await LoadTablesAsync()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationPageViewModel vm)
    {
        vm.Zones = await dbContext.TableZones.OrderBy(z => z.ZoneCode).ToListAsync();
        vm.Tables = await LoadTablesAsync();

        if (!ModelState.IsValid)
        {
            return View("Index", vm);
        }

        var tableId = vm.ReservationForm.RestaurantTableId!.Value;
        var zoneId = vm.ReservationForm.ZoneId!.Value;
        var table = await dbContext.RestaurantTables
            .Include(t => t.Zone)
            .FirstOrDefaultAsync(t => t.Id == tableId);

        if (table is null || table.Zone is null || table.ZoneId != zoneId || table.Status != TableStatus.Available)
        {
            ModelState.AddModelError("ReservationForm.RestaurantTableId", "โต๊ะนี้ไม่ว่างหรือไม่ตรงกับโซนที่เลือก");
            return View("Index", vm);
        }

        var zoneCode = table.Zone.ZoneCode;
        var reservationCode = await GenerateReservationCodeAsync(zoneCode);

        dbContext.Reservations.Add(new Reservation
        {
            ReservationCode = reservationCode,
            CustomerName = vm.ReservationForm.CustomerName.Trim(),
            PhoneNumber = vm.ReservationForm.PhoneNumber.Trim(),
            RestaurantTableId = table.Id
        });

        table.Status = TableStatus.Reserved;
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = $"จองสำเร็จ Reservation Code: {reservationCode}";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<RestaurantTable>> LoadTablesAsync()
    {
        return await dbContext.RestaurantTables
            .Include(t => t.Zone)
            .Include(t => t.Reservations)
            .OrderBy(t => t.Zone!.ZoneCode)
            .ThenBy(t => t.TableCode)
            .ToListAsync();
    }

    private async Task<string> GenerateReservationCodeAsync(string zoneCode)
    {
        var todayPrefix = DateTime.UtcNow.ToString("ddMM");
        var prefix = $"{todayPrefix}{zoneCode.Trim().ToUpper()}";

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
