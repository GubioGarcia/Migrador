using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrador.Application.Interfaces
{
    public interface IManipularArquivoService
    {
        Task<byte[]> ProcessarArquivosCsvAsync(IFormFile arquivoEtapa, IFormFile arquivoResposta, int numDialogo);
    }
}
