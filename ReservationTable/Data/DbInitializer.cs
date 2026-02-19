using Microsoft.EntityFrameworkCore;
using ReservationTable.Models;

namespace ReservationTable.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.TableZones.AnyAsync())
        {
            return;
        }

        var seedZones = new[]
        {
            new TableZone { ZoneCode = "A1" },
            new TableZone { ZoneCode = "A2" },
            new TableZone { ZoneCode = "A3" },
            new TableZone { ZoneCode = "B1" },
            new TableZone { ZoneCode = "B2" },
            new TableZone { ZoneCode = "B3" }
        };

        await dbContext.TableZones.AddRangeAsync(seedZones);
        await dbContext.SaveChangesAsync();

        var seedTables = seedZones
            .SelectMany(zone => Enumerable.Range(1, 6).Select(tableNumber => new RestaurantTable
            {
                ZoneId = zone.Id,
                TableCode = tableNumber.ToString(),
                Status = TableStatus.Available
            }))
            .ToArray();

        await dbContext.RestaurantTables.AddRangeAsync(seedTables);
        await dbContext.SaveChangesAsync();
    }
}
