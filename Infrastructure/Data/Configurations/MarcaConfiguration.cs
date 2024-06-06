using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class MarcaConfiguration : IEntityTypeConfiguration<Marca>
{
    public void Configure(EntityTypeBuilder<Marca> builder)
    {
        builder
            .ToTable("Marca");


        builder
            .Property(m => m.Id)
            .IsRequired();

        
        builder
            .Property(m => m.Nombre)
            .IsRequired()
            .HasMaxLength(100);
    }
}