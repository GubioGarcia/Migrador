using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrador.Application.Services
{
    public class RespostaMigradorService : MigradorService
    {
        // Método para adicionar colunas obrigatórias
        protected override void AdicionarColunasObrigatorias(Dictionary<string, string> item)
        {
            string[] colunasObrigatorias = ["ListaDescricao", "ListaDescricao@type", "ListaSessao", "ListaSessao@type"];
            foreach (var coluna in colunasObrigatorias)
            {
                if (!item.ContainsKey(coluna)) item[coluna] = "";
            }
        }
    }
}
