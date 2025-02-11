using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Migrador.Application.Services
{
    public class CsvService
    {
        private readonly EtapaMigrador _etapaMigrador;

        public CsvService(EtapaMigrador etapaMigrador)
        {
            _etapaMigrador = etapaMigrador;
        }

        public async Task<List<Dictionary<string, string>>> LerArquivoCsv(IFormFile arquivo)
        {
            using var reader = new StreamReader(arquivo.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true
            });

            var registros = new List<Dictionary<string, string>>();
            while (csv.Read())
            {
                var linha = csv.GetRecord<dynamic>() as IDictionary<string, object>;
                registros.Add(linha.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? ""));
            }

            return _etapaMigrador.ProcessarDados(registros);
        }

        public async Task<byte[]> SalvarArquivoCsv(IFormFile arquivo)
        {
            var arquivoLido = await LerArquivoCsv(arquivo);

            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            if (arquivoLido.Count > 0)
            {
                foreach (var chave in arquivoLido[0].Keys)
                    csv.WriteField(chave);
                csv.NextRecord();
            }

            foreach (var item in arquivoLido)
            {
                foreach (var chave in item.Keys)
                    csv.WriteField(item[chave]);
                csv.NextRecord();
            }

            await writer.FlushAsync();
            return memoryStream.ToArray();
        }
    }
}
