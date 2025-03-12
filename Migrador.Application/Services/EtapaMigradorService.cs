using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrador.Application.Services
{
    public class EtapaMigradorService : MigradorService
    {
        // Método para adicionar colunas obrigatórias
        protected override void AdicionarColunasObrigatorias(Dictionary<string, string> item)
        {
            string[] colunasObrigatorias = ["ListaHeader", "ListaHeader@type", "ListaSessao", "ListaSessao@type", "ListaTitulo", "ListaTitulo@type", "QtdeOpcoes", "QtdeOpcoes@type",
                                            "ListaDescricao", "ListaDescricao@type", "ListaDescricaoFormato", "ListaDescricaoFormato@type", "ListaFormato", "ListaFormato@type",
                                            "ListaTextoObjetoAPI", "ListaTextoObjetoAPI@type"];
            foreach (var coluna in colunasObrigatorias)
            {
                if (!item.ContainsKey(coluna)) item[coluna] = "";
            }
        }
    }
}
