using Eventos.Application.Enums;

namespace Eventos.Application.Interfaces;

public interface IRelatorioFactory
{
    IRelatorioStrategy Criar(FormatoRelatorio formato);
}
