using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveNow_API.Models.Classes;

namespace ReserveNow_API.Configuration
{
    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.ToTable("Cities","public");
            builder.HasKey(x=>x.ID);
            builder.Property(x => x.ID).HasColumnName("ID");
            builder.Property(x=>x.City_name).HasColumnName("City_name");
            builder.HasOne(x => x.Clients).WithOne().HasForeignKey<Clients>(x=>x.CityKey);
            builder.HasOne(x => x.Administration).WithOne().HasForeignKey<Administration>(x=>x.CityKey);
        }
    }
}
