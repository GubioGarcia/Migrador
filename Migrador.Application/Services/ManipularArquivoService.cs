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

            // Extrai todos os valores de uma propriedade das listas e verifica igualdade dos elementos
            ManipularArquivoService.ValidarIgualdadeDoValorDeUmaPropriedade(dadosEtapa, dadosResposta, "PartitionKey");
            ManipularArquivoService.ValidarIgualdadeDoValorDeUmaPropriedade(dadosEtapa, dadosResposta, "NumDialogo");

            List<Dictionary<string, string>> dadosProcessadosEtapa = _etapaMigradorService.ConverterDadosIniciaisDosRegistros(dadosEtapa, numDialogo);
            List<Dictionary<string, string>> dadosProcessadosResposta = _respostaMigradorService.ConverterDadosIniciaisDosRegistros(dadosResposta, numDialogo);

            List<string> registrosAlteracoes = [];

            MigradorService.ConverterRegistrosEtapaEResposta(dadosProcessadosEtapa, dadosProcessadosResposta, registrosAlteracoes);

            byte[] arquivoEtapaProcessado = CsvService.SalvarCsvEmMemoria(dadosProcessadosEtapa);
            byte[] arquivoRespostaProcessado = CsvService.SalvarCsvEmMemoria(dadosProcessadosResposta);
            byte[] arquivoRegistroDeAlteracoes = CsvService.SalvarTxtDeRegistro(registrosAlteracoes);

            return CompactarArquivos(arquivoEtapaProcessado, arquivoRespostaProcessado, arquivoRegistroDeAlteracoes);
        }

        // Retorna um array de bytes contendo os arquivos de entrada compactados em um arquivo ZIP
        private static byte[] CompactarArquivos(byte[] arquivoEtapaProcessado, byte[] arquivoRespostaProcessado, byte[] arquivoRegistroDeAlteracoes)
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

                // cria uma entrada no arquivo ZIP para o arquivo de registro
                ZipArchiveEntry entryRegistro = zipArchive.CreateEntry("registroDeAlteracoes.txt");
                using (Stream entryStream = entryRegistro.Open())
                {
                    // escreve os dados do arquivo de saída da Resposta no fluxo de dados
                    entryStream.Write(arquivoRegistroDeAlteracoes, 0, arquivoRegistroDeAlteracoes.Length);
                }
            }

            // converte o contrúdo do MemoryStream para um array de bytes e retorna
            return memoryStream.ToArray();
        }

        // Extrai todos os valores de uma propriedade das listas e verifica igualdade dos elementos
        private static void ValidarIgualdadeDoValorDeUmaPropriedade(List<Dictionary<string, string>> dadosEtapa, List<Dictionary<string, string>> dadosResposta, string propriedade)
        {
            // Leva em consideração a ordem dos elementos
            List<string> partitionKeysEtapa = dadosEtapa.Where(d => d.ContainsKey(propriedade)).Select(d => d[propriedade]).Distinct().ToList();
            List<string> partitionKeysResposta = dadosResposta.Where(d => d.ContainsKey(propriedade)).Select(d => d[propriedade]).Distinct().ToList();
            if (!partitionKeysEtapa.SequenceEqual(partitionKeysResposta))
                throw new Exception($"Os arquivos de entrada possuem registros com '{propriedade}' diferentes.");

            // Acesso mais rápido. Ordem não é importante
            /*HashSet<string> partitionKeysEtapa = new(dadosEtapa.Where(d => d.ContainsKey(propriedade)).Select(d => d[propriedade]));
            HashSet<string> partitionKeysResposta = new(dadosResposta.Where(d => d.ContainsKey(propriedade)).Select(d => d[propriedade]));
            if (!partitionKeysEtapa.IsSubsetOf(partitionKeysResposta) || !partitionKeysResposta.IsSubsetOf(partitionKeysEtapa))
                throw new Exception($"Os arquivos de entrada possuem registros com '{propriedade}' diferentes.");*/
        }
    }
}