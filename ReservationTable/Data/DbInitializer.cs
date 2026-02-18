using Microsoft.EntityFrameworkCore;
using ReservationTable.Models;

namespace ReservationTable.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.RestaurantTables.AnyAsync())
        {
            return;
        }

        var seedTables = new[]
        {
            new RestaurantTable { ZoneCode = "A1", Status = TableStatus.Available },
            new RestaurantTable { ZoneCode = "A2", Status = TableStatus.Available },
            new RestaurantTable { ZoneCode = "A3", Status = TableStatus.Available },
            new RestaurantTable { ZoneCode = "B1", Status = TableStatus.Available },
            new RestaurantTable { ZoneCode = "B2", Status = TableStatus.Available },
            new RestaurantTable { ZoneCode = "B3", Status = TableStatus.Available }
        };

        await dbContext.RestaurantTables.AddRangeAsync(seedTables);
        await dbContext.SaveChangesAsync();
    }
}
