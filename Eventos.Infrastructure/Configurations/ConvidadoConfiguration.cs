using Eventos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventos.Infrastructure.Configurations;

public class ConvidadoConfiguration : IEntityTypeConfiguration<Convidado>
{
    public void Configure(EntityTypeBuilder<Convidado> builder)
    {
        builder.HasIndex(c => c.PresencaConfirmada)
            .HasDatabaseName("IX_Convidado_PresencaConfirmada");

        builder.HasIndex(c => c.Nome)
            .HasDatabaseName("IX_Convidado_Nome");
    }
}
