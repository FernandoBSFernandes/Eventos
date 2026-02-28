using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventos.Application.DTOs.Response
{
    public class BaseResponse
    {
        public int CodigoStatus { get; set; }
        public string Mensagem { get; set; }

        public BaseResponse(int codigoStatus, string mensagem)
        {
            CodigoStatus = codigoStatus;
            Mensagem = mensagem;
        }
    }
}
