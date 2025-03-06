using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveNow_API.Models.Classes;
namespace ReserveNow_API.Configuration
{
    public class OrganizationConfiguration:IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.ToTable("Organization", "public");
            builder.HasKey(x => x.ID);
            builder.Property(x => x.Org_name).HasColumnName("Org_name");
            builder.HasOne(x => x.Administrations).WithOne().HasForeignKey<Administration>(x=>x.OrganizationKey);
        }
    }
}
