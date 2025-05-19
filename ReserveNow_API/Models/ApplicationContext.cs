using Microsoft.EntityFrameworkCore;
using ReserveNow_API.Models.Classes;
using System.Numerics;

namespace ReserveNow_API.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Client> Client { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<RefreshTokenModel> RefreshTokens { get; set; }
        //public DbSet<ResetPasswordRequest> Reset { get; set; }
        //public DbSet<ForgotPasswordRequest> Forgot { get; set; }

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


            modelBuilder.Entity<Client>().HasKey(p => p.ID);
            modelBuilder.Entity<City>().HasKey(p => p.ID);
            modelBuilder.Entity<Menu>().HasKey(p => p.ID);
            modelBuilder.Entity<Reservation>().HasKey(p => p.ID);
            modelBuilder.Entity<Review>().HasKey(p => p.ID);
            modelBuilder.Entity<Table>().HasKey(p => p.ID);
            modelBuilder.Entity<Restaurant>().HasKey(p => p.ID);
            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.City)
                .WithMany(c => c.Restaurants)
                .HasForeignKey(r => r.CityId);

            // Остальные связи остаются без изменений
            modelBuilder.Entity<Table>()
        .HasOne(t => t.Restaurant)
        .WithMany(r => r.Tables)
        .HasForeignKey(t => t.RestaurantId)
        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Client)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Table)
                .WithMany(t => t.Reservations)
                .HasForeignKey(r => r.TableId);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Client)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Restaurant)
                .WithMany(rest => rest.Reviews)
                .HasForeignKey(r => r.RestaurantId);

            modelBuilder.Entity<Menu>()
                .HasOne(m => m.Restaurant)
                .WithMany(r => r.Menus)
                .HasForeignKey(m => m.RestaurantId);
        }
    }
}
