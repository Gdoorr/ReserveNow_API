using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveNow_API.Models.Classes;
namespace ReserveNow_API.Configuration
{
    public class AdministrationConfiguration : IEntityTypeConfiguration<Administration>
    {
        public void Configure(EntityTypeBuilder<Administration> builder)
        {
            builder.ToTable("Administrations", "public");
            builder.HasKey(x => x.ID);
            builder.Property(x => x.ID).HasColumnName("ID");
            builder.Property(x => x.OrganizationKey).HasColumnName("OrganizationKey");
            builder.Property(x => x.CityKey).HasColumnName("CityKey");
            builder.Property(x => x.Adress).HasColumnName("Adress");
            builder.Property(x => x.Table_reservationKey).HasColumnName("Table_reservationKey");
            builder.Property(x => x.Tables).HasColumnName("CityKey");
            builder.HasOne(x => x.Organizations);
            builder.HasOne(x => x.City);
            builder.HasOne(x => x.Table_reservations);
        }
    }
}
