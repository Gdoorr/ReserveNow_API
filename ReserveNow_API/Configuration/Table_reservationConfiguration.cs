using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveNow_API.Models.Classes;

namespace ReserveNow_API.Configuration
{
    public class Table_reservationConfiguration:IEntityTypeConfiguration<Table_reservation>
    {
        public void Configure(EntityTypeBuilder<Table_reservation> builder)
        {
            builder.ToTable("Organization", "public");
            builder.HasKey(x => x.ID);
            builder.Property(x => x.Reservation).HasColumnName("Reservation");
            builder.HasMany(x => x.Administrations).WithOne().HasForeignKey(x=>x.Table_reservationKey);
        }
    }
}
