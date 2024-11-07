using Microsoft.EntityFrameworkCore;
using ReserveNow_API.Classes;
using System.Numerics;

namespace ReserveNow_API.Servises
{
    public class ApplicationContext : DbContext 
    {
        public DbSet<City> Cities { get; set; } = null!;
        public DbSet<Clients> Clients { get; set; } = null!;
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
            modelBuilder.Entity<City>().HasOne(u => u.Clients).WithOne(p => p.City).HasForeignKey<Clients>(p => p.CityKey);
            modelBuilder.Entity<City>().HasOne(u => u.Clients).WithOne(p => p.City).HasForeignKey<Administration>(p => p.CityKey);
            modelBuilder.Entity<Organization>().HasOne(u => u.Administrations).WithOne(p => p.Organizations).HasForeignKey<Administration>(p => p.OrganizationKey);
            modelBuilder.Entity<Table_reservation>().HasOne(u => u.Administrations).WithOne(p => p.Table_reservations).HasForeignKey<Administration>(p => p.Table_reservationKey);
        }
    }
}
