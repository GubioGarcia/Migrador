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
        // Lê arquivo CSV e retorna uma lista de dicionários com os registros
        public static async Task<List<Dictionary<string, string>>> LerCsvAsync(IFormFile arquivo)
        {
            // cria um StreamReader para ler o arquivo
            using StreamReader reader = new(arquivo.OpenReadStream());
            // cria um CsvReader para ler o arquivo CSV
            using CsvReader csv = new(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // define o delimitador como ponto e vírgula e indica que o csv possui cabeçalho
                Delimiter = ";",
                HasHeaderRecord = true
            });

            List<Dictionary<string, string>> registros = [];
            await csv.ReadAsync(); // lê a primeira linha do arquivo
            csv.ReadHeader(); // lê o cabeçalho do arquivo
            // enquanto houver registros no arquivo
            while (await csv.ReadAsync())
            {
                // lê uma linha do CSV como um objeto dinâmico e a converte para um dicionário
                var row = csv.GetRecord<dynamic>() as IDictionary<string, object>;
                // converte o objeto dinâmico para um dicionário de string e adiciona à lista
                registros.Add(row.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? ""));
            }
            return registros;
        }

        // Salva uma lista de dicionários em um arquivo CSV na memória e retorna o array de bytes contendo o contúdo do arquivo
        public static byte[] SalvarCsvEmMemoria(List<Dictionary<string, string>> dados)
        {
            // cria um MemoryStream para armazenar os dados do arquivo da memória
            using MemoryStream memoryStream = new();
            // cria um StreamWriter para escrever os dados do arquivo na memória
            using StreamWriter writer = new(memoryStream);
            // cria um CsvWriter para escrever StreamWriter e força o delimitador como ponto e vírgula
            using CsvWriter csv = new(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            });

            if (dados.Count > 0)
            {
                // escreve o cabeçalho do arquivo com as chaves do primeiro registro
                foreach (string chave in dados[0].Keys)
                {
                    csv.WriteField(chave);
                }
                csv.NextRecord();
            }

            // escreve os registros no arquivo
            foreach (Dictionary<string, string> item in dados)
            {
                // para cada diciónario, itera sobre as chaves e escreve os valores no arquivo
                foreach (string chave in item.Keys)
                {
                    csv.WriteField(item[chave]);
                }
                csv.NextRecord();
            }

            // limpa o buffer do StreamWriter e retorna o array de bytes do arquivo
            writer.Flush();
            return memoryStream.ToArray();
        }
    }
}
