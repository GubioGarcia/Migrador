using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrador.Application.Services
{
    public class EtapaMigrador
    {
        public List<Dictionary<string, string>> ProcessarDados(List<Dictionary<string, string>> dados)
        {
            ArgumentNullException.ThrowIfNull(dados);

            foreach (var item in dados)
            {
                // Se "NumDialogo" estiver vazio ou não existir, define como "0"
                if (!item.ContainsKey("NumDialogo") || string.IsNullOrWhiteSpace(item["NumDialogo"]))
                    item["NumDialogo"] = "0";

                // Incrementa "NumDialogo" em 1 se for um número válido
                if (int.TryParse(item["NumDialogo"], out int numDialogo))
                {
                    numDialogo += 2;
                    item["NumDialogo"] = numDialogo.ToString();
                }
            }

            return dados;
        }
    }
}
