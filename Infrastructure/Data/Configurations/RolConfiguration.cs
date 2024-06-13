using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder
            .ToTable("Rol");

        builder
            .HasKey(r => r.Id)
            .HasName("Rol_pkey");

        builder
            .Property(r => r.Nombre)
            .IsRequired()
            .HasMaxLength(200);
    }
}