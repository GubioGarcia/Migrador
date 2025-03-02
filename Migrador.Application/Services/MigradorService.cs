using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrador.Application.Services
{
    public abstract class MigradorService
    {
        public List<Dictionary<string, string>> ProcessarDados(List<Dictionary<string, string>> dados, int numDialogoFuturo)
        {
            ArgumentNullException.ThrowIfNull(dados);

            foreach (var item in dados)
            {
                // Se "NumDialogo" estiver vazio ou não existir, define como "0"
                if (!item.ContainsKey("NumDialogo") || string.IsNullOrWhiteSpace(item["NumDialogo"]))
                    item["NumDialogo"] = "0";

                // "NumDialogo" recebe o valor passado como parâmetro
                if (int.TryParse(item["NumDialogo"], out int numDialogo))
                {
                    numDialogo = numDialogoFuturo;
                    item["NumDialogo"] = numDialogo.ToString();
                }

                // Atualiza o valor referente ao NumDialogo contido na propriedade RowKey
                AlterarNumDialogoDaPropriedadeRowKey(item, numDialogo);

                // Adiciona colunas obrigatórias aos arquivos, caso não existam
                AdicionarColunasObrigatorias(item);
            }

            return dados;
        }

        // Método para adicionar colunas obrigatórias
        protected abstract void AdicionarColunasObrigatorias(Dictionary<string, string> item);

        // Método para atualizar "NumDialogo" da Propriedade RowKey
        protected static void AlterarNumDialogoDaPropriedadeRowKey(Dictionary<string, string> item, int numDialogoFuturo)
        {
            if (!item.ContainsKey("RowKey"))
                throw new Exception("Esta não é a propriedade RowKey.");

            string[] partes = item["RowKey"].Split('-');

            if (partes.Length > 0)
            {
                partes[0] = numDialogoFuturo.ToString();
                item["RowKey"] = string.Join("-", partes);
            }
        }

        public static void ConversorRegistrosEtapaEResposta(List<Dictionary<string, string>> etapa, List<Dictionary<string, string>> resposta)
        {
            // Itera registro a registro dos dados da Etapa
            foreach (var itemEtapa in etapa)
            {
                // Verifica se registro é do tipo pergunta
                if (itemEtapa["TipoEtapa"] == "0")
                {
                    // Verifica se registro é uma pergunta do tipo botão
                    if (itemEtapa["TipoResposta"] == "2")
                    {
                        string numEtapa = itemEtapa["NumEtapa"];

                        // Conta qntd de resposta atribuída a etapa e grava na propriedade 'QtdeOpcoes'
                        int countRespostas = resposta.Count(r => r["NumEtapa"] == numEtapa);
                        itemEtapa["QtdeOpcoes"] = countRespostas.ToString();
                        itemEtapa["QtdeOpcoes@type"] = "Int32";

                        if (countRespostas <= 3)
                        {
                            // Itera registro a registro dos dados da Resposta
                            foreach (var itemResposta in resposta)
                            {
                                // Verifica se resposta está vinculada a etapa e sobreescreve a propriedade 'legenda' apenas com os 20 primeiros dígitos originais
                                if (itemResposta["NumEtapa"] == numEtapa)
                                {
                                    if (itemResposta.ContainsKey("Legenda") && itemResposta["Legenda"].Length > 20)
                                        itemResposta["Legenda"] = itemResposta["Legenda"][..20];
                                }
                            }
                        }
                        else if (countRespostas > 3/* && countRespostas <= 10*/)
                        {
                            // Aplica valores das propriedades obrigatórias da lista para a Etapa
                            itemEtapa["TipoResposta"] = "10";
                            itemEtapa["ListaHeader"] = "Lista de opções";
                            itemEtapa["ListaHeader@type"] = "String";
                            itemEtapa["ListaSessao"] = "{@[Opções]}";
                            itemEtapa["ListaSessao@type"] = "String";
                            itemEtapa["ListaTitulo"] = "Ver opções";
                            itemEtapa["ListaTitulo@type"] = "String";

                            // Itera registro a registro dos dados da Resposta
                            foreach (var itemResposta in resposta)
                            {
                                // Aplica valores das propriedades obrigatórias da lista para a Resposta
                                if (itemResposta["NumEtapa"] == numEtapa)
                                {
                                    itemResposta["ListaDescricao"] = itemResposta["Legenda"];
                                    itemResposta["ListaDescricao@type"] = "String";
                                    itemResposta["ListaSessao"] = "1";
                                    itemResposta["ListaSessao@type"] = "Int32";

                                    // Verifica se resposta está vinculada a etapa e sobreescreve a propriedade 'legenda' apenas com os 20 primeiros dígitos originais
                                    if (itemResposta.ContainsKey("Legenda") && itemResposta["Legenda"].Length > 20)
                                    {
                                        // Trata casos em que o último caracter seja um espaço em branco
                                        if (itemResposta["Legenda"][19] == ' ')
                                            itemResposta["Legenda"] = itemResposta["Legenda"][..19];
                                        else
                                            itemResposta["Legenda"] = itemResposta["Legenda"][..20];
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
