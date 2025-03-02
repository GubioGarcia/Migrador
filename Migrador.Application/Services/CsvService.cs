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
    public static class CsvService
    {
        public static async Task<List<Dictionary<string, string>>> LerCsvAsync(IFormFile arquivo)
        {
            using StreamReader reader = new(arquivo.OpenReadStream());
            using CsvReader csv = new(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true
            });

            List<Dictionary<string, string>> registros = [];
            await csv.ReadAsync();
            csv.ReadHeader();
            while (await csv.ReadAsync())
            {
                var row = csv.GetRecord<dynamic>() as IDictionary<string, object>;
                registros.Add(row.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? ""));
            }
            return registros;
        }

        public static byte[] SalvarCsvEmMemoria(List<Dictionary<string, string>> dados)
        {
            using MemoryStream memoryStream = new();
            using StreamWriter writer = new(memoryStream);
            using CsvWriter csv = new(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            });

            if (dados.Count > 0)
            {
                foreach (string chave in dados[0].Keys)
                {
                    csv.WriteField(chave);
                }
                csv.NextRecord();
            }

            foreach (Dictionary<string, string> item in dados)
            {
                foreach (string chave in item.Keys)
                {
                    csv.WriteField(item[chave]);
                }
                csv.NextRecord();
            }

            writer.Flush();
            return memoryStream.ToArray();
        }
    }
}
