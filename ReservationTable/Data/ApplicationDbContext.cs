using Microsoft.EntityFrameworkCore;
using ReservationTable.Models;

namespace ReservationTable.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<TableZone> TableZones => Set<TableZone>();
    public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TableZone>()
            .HasIndex(z => z.ZoneCode)
            .IsUnique();

        modelBuilder.Entity<RestaurantTable>()
            .HasIndex(t => new { t.ZoneId, t.TableCode })
            .IsUnique();

        modelBuilder.Entity<RestaurantTable>()
            .HasOne(t => t.Zone)
            .WithMany(z => z.Tables)
            .HasForeignKey(t => t.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reservation>()
            .HasIndex(r => r.ReservationCode)
            .IsUnique();

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.RestaurantTable)
            .WithMany(t => t.Reservations)
            .HasForeignKey(r => r.RestaurantTableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
