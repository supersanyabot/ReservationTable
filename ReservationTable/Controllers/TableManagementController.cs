using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationTable.Data;
using ReservationTable.Models;
using ReservationTable.ViewModels;
using System.Text.RegularExpressions;

namespace ReservationTable.Controllers;

public class TableManagementController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await BuildViewModelAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddZone(
        [Bind(Prefix = nameof(TableManagementViewModel.AddZoneForm))] AddZoneViewModel form)
    {
        form.ZoneCode = form.ZoneCode?.Trim().ToUpper() ?? string.Empty;

        ModelState.Clear();
        TryValidateModel(form, nameof(TableManagementViewModel.AddZoneForm));
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildViewModelAsync(addZoneForm: form));
        }

        var exists = await dbContext.TableZones.AnyAsync(z => z.ZoneCode == form.ZoneCode);
        if (exists)
        {
            ModelState.AddModelError("AddZoneForm.ZoneCode", $"โซน {form.ZoneCode} มีอยู่แล้ว");
            return View("Index", await BuildViewModelAsync(addZoneForm: form));
        }

        dbContext.TableZones.Add(new TableZone
        {
            ZoneCode = form.ZoneCode
        });
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = $"เพิ่มโซน {form.ZoneCode} เรียบร้อย";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTable(
        [Bind(Prefix = nameof(TableManagementViewModel.AddTableForm))] AddTableViewModel form)
    {
        form.TableCode = form.TableCode?.Trim().ToUpper() ?? string.Empty;

        ModelState.Clear();
        TryValidateModel(form, nameof(TableManagementViewModel.AddTableForm));
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildViewModelAsync(addTableForm: form));
        }

        var zone = await dbContext.TableZones.FirstOrDefaultAsync(z => z.Id == form.ZoneId);
        if (zone is null)
        {
            ModelState.AddModelError("AddTableForm.ZoneId", "ไม่พบโซนที่เลือก");
            return View("Index", await BuildViewModelAsync(addTableForm: form));
        }

        var exists = await dbContext.RestaurantTables.AnyAsync(t =>
            t.ZoneId == form.ZoneId && t.TableCode == form.TableCode);
        if (exists)
        {
            ModelState.AddModelError("AddTableForm.TableCode", $"โต๊ะ {zone.ZoneCode}-{form.TableCode} มีอยู่แล้ว");
            return View("Index", await BuildViewModelAsync(addTableForm: form));
        }

        dbContext.RestaurantTables.Add(new RestaurantTable
        {
            ZoneId = zone.Id,
            TableCode = form.TableCode,
            Status = TableStatus.Available
        });
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = $"เพิ่มโต๊ะ {zone.ZoneCode}-{form.TableCode} เรียบร้อย";
        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteZone(
        [Bind(Prefix = nameof(TableManagementViewModel.DeleteZoneForm))] DeleteZoneViewModel form)
    {
        ModelState.Clear();
        TryValidateModel(form, nameof(TableManagementViewModel.DeleteZoneForm));
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildViewModelAsync(deleteZoneForm: form));
        }

        var zone = await dbContext.TableZones
            .Include(z => z.Tables)
            .FirstOrDefaultAsync(z => z.Id == form.ZoneId);

        if (zone is null)
        {
            TempData["ErrorMessage"] = "ไม่พบโซนที่ต้องการลบ";
            return RedirectToAction(nameof(Index));
        }

        if (zone.Tables.Count > 0)
        {
            TempData["ErrorMessage"] = $"ลบโซน {zone.ZoneCode} ไม่ได้ เพราะยังมีโต๊ะอยู่ในโซน";
            return RedirectToAction(nameof(Index));
        }

        dbContext.TableZones.Remove(zone);
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = $"ลบโซน {zone.ZoneCode} เรียบร้อย";
        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int tableId, TableStatus status, string? customerName, string? phoneNumber)
    {
        var table = await dbContext.RestaurantTables
            .Include(t => t.Zone)
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

            TempData["SuccessMessage"] = $"เปลี่ยนสถานะโต๊ะ {BuildTableLabel(table)} เป็นว่าง และยกเลิกการจองที่มีอยู่แล้ว";
            return RedirectToAction(nameof(Index));
        }

        if (table.Status == TableStatus.Available && status == TableStatus.Reserved)
        {
            customerName = customerName?.Trim() ?? string.Empty;
            phoneNumber = phoneNumber?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(customerName))
            {
                TempData["ErrorMessage"] = $"กรุณากรอกชื่อผู้จองสำหรับโต๊ะ {BuildTableLabel(table)}";
                return RedirectToAction(nameof(Index));
            }

            if (Regex.IsMatch(customerName, @"\d"))
            {
                TempData["ErrorMessage"] = $"ชื่อผู้จองของโต๊ะ {BuildTableLabel(table)} ห้ามมีตัวเลข";
                return RedirectToAction(nameof(Index));
            }

            if (!Regex.IsMatch(phoneNumber, @"^0\d{9}$"))
            {
                TempData["ErrorMessage"] = $"เบอร์โทรของโต๊ะ {BuildTableLabel(table)} ต้องเป็นตัวเลข 10 หลักและขึ้นต้นด้วย 0";
                return RedirectToAction(nameof(Index));
            }

            var reservationCode = await GenerateReservationCodeAsync(table.Zone?.ZoneCode ?? string.Empty);
            dbContext.Reservations.Add(new Reservation
            {
                ReservationCode = reservationCode,
                CustomerName = customerName,
                PhoneNumber = phoneNumber,
                RestaurantTableId = table.Id
            });

            table.Status = TableStatus.Reserved;
            await dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = $"อัปเดตเป็นจองแล้วสำหรับโต๊ะ {BuildTableLabel(table)} (Code: {reservationCode})";
            return RedirectToAction(nameof(Index));
        }

        table.Status = status;
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = $"อัปเดตสถานะโต๊ะ {BuildTableLabel(table)} เรียบร้อย";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelReservation(int tableId)
    {
        var table = await dbContext.RestaurantTables
            .Include(t => t.Zone)
            .Include(t => t.Reservations)
            .FirstOrDefaultAsync(t => t.Id == tableId);

        if (table is null)
        {
            TempData["ErrorMessage"] = "ไม่พบโต๊ะที่ต้องการยกเลิกการจอง";
            return RedirectToAction(nameof(Index));
        }

        if (table.Reservations.Count == 0)
        {
            TempData["ErrorMessage"] = $"โต๊ะ {BuildTableLabel(table)} ไม่มีรายการจองให้ยกเลิก";
            return RedirectToAction(nameof(Index));
        }

        dbContext.Reservations.RemoveRange(table.Reservations);
        table.Status = TableStatus.Available;
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = $"ยกเลิกการจองของโต๊ะ {BuildTableLabel(table)} แล้ว";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTable(int tableId)
    {
        var table = await dbContext.RestaurantTables
            .Include(t => t.Zone)
            .Include(t => t.Reservations)
            .FirstOrDefaultAsync(t => t.Id == tableId);

        if (table is null)
        {
            TempData["ErrorMessage"] = "ไม่พบโต๊ะที่ต้องการลบ";
            return RedirectToAction(nameof(Index));
        }

        if (table.Reservations.Count > 0)
        {
            TempData["ErrorMessage"] = $"ลบโต๊ะ {BuildTableLabel(table)} ไม่ได้ เพราะมีการจองอยู่";
            return RedirectToAction(nameof(Index));
        }

        dbContext.RestaurantTables.Remove(table);
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = $"ลบโต๊ะ {BuildTableLabel(table)} เรียบร้อย";
        return RedirectToAction(nameof(Index));
    }

    private async Task<TableManagementViewModel> BuildViewModelAsync(
        AddZoneViewModel? addZoneForm = null,
        AddTableViewModel? addTableForm = null,
        DeleteZoneViewModel? deleteZoneForm = null)
    {
        return new TableManagementViewModel
        {
            Zones = await dbContext.TableZones
                .OrderBy(z => z.ZoneCode)
                .ToListAsync(),
            Tables = await dbContext.RestaurantTables
                .Include(t => t.Zone)
                .Include(t => t.Reservations)
                .OrderBy(t => t.Zone!.ZoneCode)
                .ThenBy(t => t.TableCode)
                .ToListAsync(),
            AddZoneForm = addZoneForm ?? new AddZoneViewModel(),
            AddTableForm = addTableForm ?? new AddTableViewModel(),
            DeleteZoneForm = deleteZoneForm ?? new DeleteZoneViewModel()
        };
    }

    private static string BuildTableLabel(RestaurantTable table)
    {
        var zoneCode = table.Zone?.ZoneCode ?? "-";
        return $"{zoneCode}-{table.TableCode}";
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


