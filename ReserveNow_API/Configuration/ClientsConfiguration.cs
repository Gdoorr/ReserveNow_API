using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveNow_API.Models.Classes;

namespace ReserveNow_API.Configuration
{
    public class ClientsConfiguration : IEntityTypeConfiguration<Clients>
    {
        public void Configure(EntityTypeBuilder<Clients> builder)
        {
            builder.ToTable("Clients", "public");
            builder.HasKey(x => x.ID);
            builder.Property(x => x.ID).HasColumnName("ID");
            builder.Property(x => x.Client_name).HasColumnName("Client_name");
            builder.Property(x => x.Phone_number).HasColumnName("Phone_number");
            builder.Property(x => x.Email).HasColumnName("Email");
            builder.Property(x => x.Password).HasColumnName("Password");
            builder.Property(x => x.CityKey).HasColumnName("CityKey");
            builder.HasOne(x => x.City);
        }
    }
}
