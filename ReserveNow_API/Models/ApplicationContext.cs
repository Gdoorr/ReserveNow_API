using Microsoft.EntityFrameworkCore;
using ReserveNow_API.Models.Classes;
using System.Numerics;
using ReserveNow_API.Configuration;
using ReserveNow_API.Models.Classes;

namespace ReserveNow_API.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<City> Cities { get; set; } = null!;
        public DbSet<Clients> Client { get; set; } = null!;
        public DbSet<Organization> Organizations { get; set; } = null!;
        public DbSet<Table_reservation> Table_Reservations { get; set; } = null!;
        public DbSet<Administration> Administrations { get; set; } = null!;

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=ReserveNow;Username=postgres;Password=1");

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Clients>().HasKey(p => p.ID);
            modelBuilder.Entity<City>().HasKey(p => p.ID);
            modelBuilder.Entity<Organization>().HasKey(p => p.ID);
            modelBuilder.Entity<Administration>().HasKey(p => p.ID);
            modelBuilder.Entity<Table_reservation>().HasKey(p => p.ID);
            modelBuilder.Entity<City>().HasMany(u => u.Clients).WithOne(p => p.City).HasForeignKey(p => p.CityKey);
            modelBuilder.Entity<City>().HasMany(u => u.Administration).WithOne(p => p.City).HasForeignKey(p => p.CityKey);
            modelBuilder.Entity<Organization>().HasMany(u => u.Administrations).WithOne(p => p.Organizations).HasForeignKey(p => p.OrganizationKey);
            modelBuilder.Entity<Table_reservation>().HasMany(u => u.Administrations).WithOne(p => p.Table_reservations).HasForeignKey(p => p.Table_reservationKey);
        }
    }
}
