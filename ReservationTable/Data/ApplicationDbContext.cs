using Microsoft.EntityFrameworkCore;
using ReservationTable.Models;

namespace ReservationTable.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RestaurantTable>()
            .HasIndex(t => t.ZoneCode)
            .IsUnique();

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
