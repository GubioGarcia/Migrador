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

            List<Dictionary<string, string>> dadosProcessadosEtapa = _etapaMigradorService.ConverterDadosIniciaisDosRegistros(dadosEtapa, numDialogo);
            List<Dictionary<string, string>> dadosProcessadosResposta = _respostaMigradorService.ConverterDadosIniciaisDosRegistros(dadosResposta, numDialogo);

            MigradorService.ConverterRegistrosEtapaEResposta(dadosProcessadosEtapa, dadosProcessadosResposta);

            byte[] arquivoEtapaProcessado = CsvService.SalvarCsvEmMemoria(dadosProcessadosEtapa);
            byte[] arquivoRespostaProcessado = CsvService.SalvarCsvEmMemoria(dadosProcessadosResposta);

            return CompactarArquivos(arquivoEtapaProcessado, arquivoRespostaProcessado);
        }

        // Retorna um array de bytes contendo os arquivos de entrada compactados em um arquivo ZIP
        private static byte[] CompactarArquivos(byte[] arquivoEtapaProcessado, byte[] arquivoRespostaProcessado)
        {
            // cria um MemoryStream para armazenar os dados do arquivo ZIP
            using MemoryStream memoryStream = new();
            // cria um ZipArchive no MemoryStream para compactar os arquivos, permite a criação de novas entradas no zip
            using (ZipArchive zipArchive = new(memoryStream, ZipArchiveMode.Create, true))
            {
                // cria uma entrada no arquivo ZIP para o arquivo de saída da Etapa
                ZipArchiveEntry entryEtapa = zipArchive.CreateEntry("saidaEtapaComLista.csv");
                // abre um fluxo de dados para a entrada no arquivo ZIP
                using (Stream entryStream = entryEtapa.Open())
                {
                    // escreve os dados do arquivo de saída da Etapa no fluxo de dados
                    entryStream.Write(arquivoEtapaProcessado, 0, arquivoEtapaProcessado.Length);
                }

                // cria uma entrada no arquivo ZIP para o arquivo de saída da Resposta
                ZipArchiveEntry entryResposta = zipArchive.CreateEntry("saidaRespostaComLista.csv");
                // abre um fluxo de dados para a entrada no arquivo ZIP
                using (Stream entryStream = entryResposta.Open())
                {
                    // escreve os dados do arquivo de saída da Resposta no fluxo de dados
                    entryStream.Write(arquivoRespostaProcessado, 0, arquivoRespostaProcessado.Length);
                }
            }

            // converte o contrúdo do MemoryStream para um array de bytes e retorna
            return memoryStream.ToArray();
        }
    }
}