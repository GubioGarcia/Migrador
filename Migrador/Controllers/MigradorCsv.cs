using Microsoft.AspNetCore.Mvc;
using Migrador.Application.Services;

namespace Migrador.Controllers
{
    public class MigradorCsv : Controller
    {
        private readonly CsvService _csvService;

        public MigradorCsv(CsvService csvService)
        {
            this._csvService = csvService;
        }

        [HttpPost("processarcsv")]
        public async Task<IActionResult> ProcessarCSV([FromForm] IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
                return BadRequest("Arquivo CSV inválido");

            var dados = await _csvService.SalvarArquivoCsv(arquivo);

            return Ok(dados);
        }

        [HttpPost("processarcsv/baixar")]
        public async Task<IActionResult> ProcessarCsvDowload([FromForm] IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
                return BadRequest("Arquivo CSV inválido");

            var novoArquivoProcessado = await _csvService.SalvarArquivoCsv(arquivo);
            return File(novoArquivoProcessado, "text/csv", "saida.csv");
        }
    }
}
