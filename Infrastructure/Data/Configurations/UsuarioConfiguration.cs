using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder
            .ToTable("Usuario");

        builder
            .HasKey(u => u.Id)
            .HasName("Usuario_pkey");

        builder
            .Property(u => u.Nombres)
            .IsRequired()
            .HasMaxLength(200);

        builder
            .Property(u => u.ApellidoPaterno)
            .IsRequired()
            .HasMaxLength(200);

        builder
            .Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(200);

        builder
            .Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder
            .HasMany(u => u.Roles)
            .WithMany(r => r.Usuarios)
            .UsingEntity<UsuariosRoles>(
                j => j
                    .HasOne(ur => ur.Rol)
                    .WithMany(r => r.UsuariosRoles)
                    .HasForeignKey(ur => ur.RolId)
            );
    }
}