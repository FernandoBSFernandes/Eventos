using Eventos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventos.Infrastructure.Configurations;

public class AcompanhanteConfiguration : IEntityTypeConfiguration<Acompanhante>
{
    public void Configure(EntityTypeBuilder<Acompanhante> builder)
    {
        builder.HasIndex(a => a.ConvidadoId)
            .HasDatabaseName("IX_Acompanhante_ConvidadoId");
    }
}
