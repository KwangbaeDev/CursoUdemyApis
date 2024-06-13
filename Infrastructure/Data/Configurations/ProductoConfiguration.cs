using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder
            .ToTable("Producto");


        builder
            .HasKey(p => p.Id)
            .HasName("Producto_pkey");

        
        builder
            .Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        
        builder
            .Property(p => p.Precio)
            .HasColumnType("decimal(18,2)");

        
        // builder
        //     .Property(p => p.FechaCreacion)
        //     .HasColumnType("timestamp");


        builder
            .HasOne(p => p.Marca)
            .WithMany(m => m.Productos)
            .HasForeignKey(p => p.MarcaId);


        builder
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId); 
    }
}