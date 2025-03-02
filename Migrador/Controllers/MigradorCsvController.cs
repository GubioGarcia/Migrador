using Microsoft.AspNetCore.Mvc;
using Migrador.Application.Interfaces;

namespace Migrador.Controllers
{
    public class MigradorCsvController : ControllerBase
    {
        private readonly IManipularArquivoService _manipularArquivoService;

        public MigradorCsvController(IManipularArquivoService manipularArquivoService)
        {
            _manipularArquivoService = manipularArquivoService;
        }

        [HttpPost("ConversorRegistrosEtapaEResposta")]
        public async Task<IActionResult> UploadCsv(IFormFile arquivoEtapa, IFormFile arquivoResposta, int numDialogo)
        {
            if (arquivoEtapa == null || arquivoEtapa.Length == 0)
                return BadRequest("Arquivo Etapa CSV inválido");
            if (arquivoResposta == null || arquivoResposta.Length == 0)
                return BadRequest("Arquivo Resposta CSV inválido");
            if (numDialogo == null || numDialogo <= 0)
                return BadRequest("Número de Diálogo inválido. Informe um número positivo e maior que zero.");

            byte[] zipFile = await _manipularArquivoService.ProcessarArquivosCsvAsync(arquivoEtapa, arquivoResposta, numDialogo);

            return File(zipFile, "application/zip", "arquivos_processados.zip");
        }
    }
}
