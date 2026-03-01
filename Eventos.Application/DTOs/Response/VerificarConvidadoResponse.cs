namespace Eventos.Application.DTOs.Response
{
    public class VerificarConvidadoResponse : BaseResponse
    {
        public bool Existe { get; set; }

        public VerificarConvidadoResponse(int codigoStatus, string mensagem, bool existe)
            : base(codigoStatus, mensagem)
        {
            Existe = existe;
        }
    }
}
