using Microsoft.AspNetCore.Http;
using Migrador.Application.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Migrador.Application.Services
{
    public class ManipularArquivoService : IManipularArquivoService
    {
        private readonly EtapaMigradorService _etapaMigradorService;
        private readonly RespostaMigradorService _respostaMigradorService;

        public ManipularArquivoService(EtapaMigradorService etapaMigradorService, RespostaMigradorService respostaMigradorService)
        {
            _etapaMigradorService = etapaMigradorService;
            _respostaMigradorService = respostaMigradorService;
        }

        public async Task<byte[]> ProcessarArquivosCsvAsync(IFormFile arquivoEtapa, IFormFile arquivoResposta, int numDialogo)
        {
            List<Dictionary<string, string>> dadosEtapa = await CsvService.LerCsvAsync(arquivoEtapa);
            List<Dictionary<string, string>> dadosResposta = await CsvService.LerCsvAsync(arquivoResposta);

            List<Dictionary<string, string>> dadosProcessadosEtapa = _etapaMigradorService.ProcessarDados(dadosEtapa, numDialogo);
            List<Dictionary<string, string>> dadosProcessadosResposta = _respostaMigradorService.ProcessarDados(dadosResposta, numDialogo);

            MigradorService.ConversorRegistrosEtapaEResposta(dadosProcessadosEtapa, dadosProcessadosResposta);

            byte[] arquivoEtapaProcessado = CsvService.SalvarCsvEmMemoria(dadosProcessadosEtapa);
            byte[] arquivoRespostaProcessado = CsvService.SalvarCsvEmMemoria(dadosProcessadosResposta);

            return CompactarArquivos(arquivoEtapaProcessado, arquivoRespostaProcessado);
        }

        private static byte[] CompactarArquivos(byte[] arquivoEtapaProcessado, byte[] arquivoRespostaProcessado)
        {
            using MemoryStream memoryStream = new();
            using (ZipArchive zipArchive = new(memoryStream, ZipArchiveMode.Create, true))
            {
                ZipArchiveEntry entryEtapa = zipArchive.CreateEntry("saidaEtapaComLista.csv");
                using (Stream entryStream = entryEtapa.Open())
                {
                    entryStream.Write(arquivoEtapaProcessado, 0, arquivoEtapaProcessado.Length);
                }

                ZipArchiveEntry entryResposta = zipArchive.CreateEntry("saidaRespostaComLista.csv");
                using (Stream entryStream = entryResposta.Open())
                {
                    entryStream.Write(arquivoRespostaProcessado, 0, arquivoRespostaProcessado.Length);
                }
            }
            return memoryStream.ToArray();
        }
    }
}