using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationTable.Data;
using ReservationTable.Models;
using ReservationTable.ViewModels;

namespace ReservationTable.Controllers;

public class TableManagementController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var vm = new TableManagementViewModel
        {
            Tables = await LoadTablesAsync()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int tableId, TableStatus status)
    {
        var table = await dbContext.RestaurantTables
            .Include(t => t.Reservations)
            .FirstOrDefaultAsync(t => t.Id == tableId);

        if (table is null)
        {
            TempData["ErrorMessage"] = "ไม่พบโต๊ะที่ต้องการแก้ไข";
            return RedirectToAction(nameof(Index));
        }

        if (status == TableStatus.Available && table.Reservations.Count > 0)
        {
            dbContext.Reservations.RemoveRange(table.Reservations);
            table.Status = TableStatus.Available;
            await dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = $"เปลี่ยนสถานะโต๊ะ {table.ZoneCode} เป็นว่าง และยกเลิกการจองที่มีอยู่แล้ว";
            return RedirectToAction(nameof(Index));
        }

        table.Status = status;
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = $"อัปเดตสถานะโต๊ะ {table.ZoneCode} เรียบร้อย";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelReservation(int tableId)
    {
        var table = await dbContext.RestaurantTables
            .Include(t => t.Reservations)
            .FirstOrDefaultAsync(t => t.Id == tableId);

        if (table is null)
        {
            TempData["ErrorMessage"] = "ไม่พบโต๊ะที่ต้องการยกเลิกการจอง";
            return RedirectToAction(nameof(Index));
        }

        if (table.Reservations.Count == 0)
        {
            TempData["ErrorMessage"] = $"โต๊ะ {table.ZoneCode} ไม่มีรายการจองให้ยกเลิก";
            return RedirectToAction(nameof(Index));
        }

        dbContext.Reservations.RemoveRange(table.Reservations);
        table.Status = TableStatus.Available;
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = $"ยกเลิกการจองของโต๊ะ {table.ZoneCode} แล้ว";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTable(TableManagementViewModel vm)
    {
        vm.NewZoneCode = vm.NewZoneCode?.Trim().ToUpper() ?? string.Empty;

        if (!ModelState.IsValid)
        {
            vm.Tables = await LoadTablesAsync();
            return View("Index", vm);
        }

        var exists = await dbContext.RestaurantTables.AnyAsync(t => t.ZoneCode == vm.NewZoneCode);
        if (exists)
        {
            ModelState.AddModelError(nameof(vm.NewZoneCode), $"โซนโต๊ะ {vm.NewZoneCode} มีอยู่แล้ว");
            vm.Tables = await LoadTablesAsync();
            return View("Index", vm);
        }

        dbContext.RestaurantTables.Add(new RestaurantTable
        {
            ZoneCode = vm.NewZoneCode,
            Status = TableStatus.Available
        });
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = $"เพิ่มโต๊ะ {vm.NewZoneCode} เรียบร้อย (สถานะเริ่มต้น: ว่าง)";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTable(int tableId)
    {
        var table = await dbContext.RestaurantTables
            .Include(t => t.Reservations)
            .FirstOrDefaultAsync(t => t.Id == tableId);

        if (table is null)
        {
            TempData["ErrorMessage"] = "ไม่พบโต๊ะที่ต้องการลบ";
            return RedirectToAction(nameof(Index));
        }

        if (table.Reservations.Count > 0)
        {
            TempData["ErrorMessage"] = $"ลบโต๊ะ {table.ZoneCode} ไม่ได้ เพราะมีการจองอยู่";
            return RedirectToAction(nameof(Index));
        }

        dbContext.RestaurantTables.Remove(table);
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = $"ลบโต๊ะ {table.ZoneCode} เรียบร้อย";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<RestaurantTable>> LoadTablesAsync()
    {
        return await dbContext.RestaurantTables
            .Include(t => t.Reservations)
            .OrderBy(t => t.ZoneCode)
            .ToListAsync();
    }
}
