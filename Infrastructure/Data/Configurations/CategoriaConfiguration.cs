using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder
            .ToTable("Categoria");


        builder
            .Property(c => c.Id)
            .IsRequired();

        
        builder
            .Property(c => c.Nombre)
            .IsRequired()
            .HasMaxLength(100);
    }
}