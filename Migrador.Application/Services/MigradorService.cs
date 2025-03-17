using Newtonsoft.Json.Linq;

namespace Migrador.Application.Services
{
    public abstract class MigradorService
    {
        public List<Dictionary<string, string>> ConverterDadosIniciaisDosRegistros(List<Dictionary<string, string>> dados, int numDialogoFuturo)
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

        public static void ConverterRegistrosEtapaEResposta(List<Dictionary<string, string>> etapa, List<Dictionary<string, string>> resposta, List<string> registrosAlteracoes)
        {
            // Itera registro a registro dos dados da Etapa
            foreach (var itemEtapa in etapa)
            {
                // Verifica se registro é do tipo pergunta
                if (itemEtapa["TipoEtapa"] == "0")
                {
                    ConverterRegistroDoTipoPergunta(itemEtapa, resposta, registrosAlteracoes);
                }
                else if (itemEtapa["TipoEtapa"] == "2")
                {
                    ConverterRegistroDoTipoApiComRetornoDeLista(itemEtapa, resposta, registrosAlteracoes);
                }
            }
        }

        private static void ConverterRegistroDoTipoPergunta(Dictionary<string, string> itemEtapa, List<Dictionary<string, string>> resposta, List<string> registrosAlteracoes)
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
                            {
                                if (itemResposta["Legenda"] == "Falar com um atendente")
                                    itemResposta["Legenda"] = "Falar com atendente";
                                else if (itemResposta["Legenda"] == "Finalizar o atendimento" || itemResposta["Legenda"] == "Finalizar atendimento")
                                    itemResposta["Legenda"] = "Encerrar atendimento";
                                else
                                    itemResposta["Legenda"] = itemResposta["Legenda"][..20];
                            }
                        }
                    }

                    // registra alteração da etapa
                    registrosAlteracoes.Add("Etapa: " + numEtapa);
                }
                else if (countRespostas > 3)
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

                    // registra alteração da etapa
                    registrosAlteracoes.Add("Etapa: " + numEtapa);
                }
            }
        }

        private static void ConverterRegistroDoTipoApiComRetornoDeLista(Dictionary<string, string> itemEtapa, List<Dictionary<string, string>> resposta, List<string> registrosAlteracoes)
        {

            // trata as chamadas de API's que possua retorno do tipo 3 (exibe lista de opções ao cliente)
            if (itemEtapa["RetornoAPI"] == "3")
            {
             #region 'Registra opção Voltar para a lista de retorno da API'
                itemEtapa["TipoResposta"] = "10";
                Dictionary<String, string> itemRespostaVoltar = new();
                itemRespostaVoltar.Add("PartitionKey", itemEtapa["PartitionKey"]);
                itemRespostaVoltar.Add("RowKey", itemEtapa["NumDialogo"] + "-" + itemEtapa["NumEtapa"] + "-1");
                itemRespostaVoltar.Add("Legenda", "Voltar");
                itemRespostaVoltar.Add("Lengenda@type", "String");
                itemRespostaVoltar.Add("NumDialogo", itemEtapa["NumDialogo"]);
                itemRespostaVoltar.Add("NumDialogo@type", "Int32");
                itemRespostaVoltar.Add("NumEtapa", itemEtapa["NumEtapa"]);
                itemRespostaVoltar.Add("NumEtapa@type", "Int32");
                itemRespostaVoltar.Add("NumProxEtapa", "11");
                itemRespostaVoltar.Add("NumProxEtapa@type", "Int32");
                itemRespostaVoltar.Add("NumResposta", "1");
                itemRespostaVoltar.Add("NumResposta@type", "Int32");
                itemRespostaVoltar.Add("Ordem", "1");
                itemRespostaVoltar.Add("Ordem@type", "Int32");
                itemRespostaVoltar.Add("PularValidacao", "false");
                itemRespostaVoltar.Add("PularValidacao@type", "Boolean");
                itemRespostaVoltar.Add("ValorArmazenado", "Voltar");
                itemRespostaVoltar.Add("ValorArmazenado@type", "String");
                itemRespostaVoltar.Add("ListaDescricao", "Voltar");
                itemRespostaVoltar.Add("ListaDescricao@type", "String");
                itemRespostaVoltar.Add("ListaSessao", "1");
                itemRespostaVoltar.Add("ListaSessao@type", "Int32");

                resposta.Add(itemRespostaVoltar);
             #endregion

                // coleta o ID da consulta SQL UAU
                if (itemEtapa["UrlAPI"].Contains("/RotinasGerais/ExecutarConsultaGeral"))
                {
                    JObject bodyApi = JObject.Parse(itemEtapa["IntegracaoAPI"]);
                    int id = (int)bodyApi["Id"];

                    // Trata consulta geral UAU para listagem de contratos
                    if (id == 1661 || id == 1662)
                    {
                        itemEtapa["ListaDescricao"] = "{@[Identificador_unid]} - {@[Descr_obr]}";
                        itemEtapa["ListaDescricao@type"] = "String";
                        itemEtapa["ListaSessao"] = "{@[Contratos]}";
                        itemEtapa["ListaSessao@type"] = "String";
                        itemEtapa["ListaTextoObjetoAPI"] = "Contrato-{@[NumVend_Itv]}";
                        itemEtapa["ListaTextoObjetoAPI@type"] = "String";
                        itemEtapa["ListaTitulo"] = "Ver contratos";
                        itemEtapa["ListaTitulo@type"] = "String";
                        itemEtapa["TextoObjetoAPI"] = "{@[Descr_obr]}\r\n    {@[Identificador_unid]} (contrato-{@[NumVend_Itv]})";
                        itemEtapa["TextoObjetoAPI@type"] = "String";
                    }
                    // Trata consulta geral UAU para 'Busca contratos que tenham boletos gerados'
                    else if (id == 1676)
                    {
                        itemEtapa["ListaDescricao"] = "{@[Identificador_unid]} - {@[descr_obr]}";
                        itemEtapa["ListaDescricao@type"] = "String";
                        itemEtapa["ListaSessao"] = "{@[Contratos]}";
                        itemEtapa["ListaSessao@type"] = "String";
                        itemEtapa["ListaTextoObjetoAPI"] = "Contrato-{@[Num_ven]}";
                        itemEtapa["ListaTextoObjetoAPI@type"] = "String";
                        itemEtapa["ListaTitulo"] = "Ver contratos";
                        itemEtapa["ListaTitulo@type"] = "String";
                        itemEtapa["TextoObjetoAPI"] = "{@[descr_obr]}\r\n    {@[Identificador_unid]} (contrato-{@[Num_ven]})";
                        itemEtapa["TextoObjetoAPI@type"] = "String";
                    }
                    // Trata consulta geral UAU para 'Busca o último boleto gerado'
                    else if (id == 1691)
                    {
                        itemEtapa["ListaDescricao"] = "Valor: {@[ValDoc_Bol]} - Vencimento: {@[DataVenc_bol]}";
                        itemEtapa["ListaDescricao@type"] = "String";
                        itemEtapa["ListaSessao"] = "{@[Boletos disponíveis]}";
                        itemEtapa["ListaSessao@type"] = "String";
                        itemEtapa["ListaTextoObjetoAPI"] = "Boleto: {@[Banco_Bol]}-{@[SeuNum_Bol]}";
                        itemEtapa["ListaTextoObjetoAPI@type"] = "String";
                        itemEtapa["ListaTitulo"] = "Boletos";
                        itemEtapa["ListaTitulo@type"] = "String";
                        itemEtapa["TextoObjetoAPI"] = "{@[DataVenc_bol]} - {@[ValDoc_Bol]}\r\n    Boleto Nº: {@[Banco_Bol]}-{@[SeuNum_Bol]}";
                        itemEtapa["TextoObjetoAPI@type"] = "String";
                        itemEtapa["Formato"] = "DT-d]@[N-C]@[]@[";
                        itemEtapa["Formato@type"] = "String";
                        itemEtapa["ListaDescricaoFormato"] = "N-C]@[DT-d";
                        itemEtapa["ListaDescricaoFormato@type"] = "String";
                        itemEtapa["ListaFormato"] = "]@[";
                        itemEtapa["ListaFormato@type"] = "String";
                    }
                }
                else
                {
                    // Trata API Iterup de listagem de parcelas do UAU
                    if (itemEtapa["UrlAPI"] == "https://api-iterup.azurewebsites.net/api/AnteciparParcelasUAU")
                    {
                        itemEtapa["ListaDescricao"] = "Valor: R$ {@[Valor_Prc]} - Vencimento: {@[Data_Prc]}";
                        itemEtapa["ListaDescricao@type"] = "String";
                        itemEtapa["ListaSessao"] = "{@[Parcelas]}";
                        itemEtapa["ListaSessao@type"] = "String";
                        itemEtapa["ListaTextoObjetoAPI"] = "{@[Tipo_reaj]}/{@[NumParc_reaj]}";
                        itemEtapa["ListaTextoObjetoAPI@type"] = "String";
                        itemEtapa["ListaTitulo"] = "Ver parcelas";
                        itemEtapa["ListaTitulo@type"] = "String";
                        itemEtapa["TextoObjetoAPI"] = "{@[Tipo_reaj]}/{@[NumParc_reaj]}: {@[Data_Prc]} - R$ {@[Valor_Prc]}";
                        itemEtapa["TextoObjetoAPI@type"] = "String";
                        itemEtapa["Formato"] = "]@[]@[DT-d]@[N-n";
                        itemEtapa["Formato@type"] = "String";
                        itemEtapa["ListaDescricaoFormato"] = "N-C]@[DT-d";
                        itemEtapa["ListaDescricaoFormato@type"] = "String";
                        itemEtapa["ListaFormato"] = "]@[";
                        itemEtapa["ListaFormato@type"] = "String";
                    }
                    // Trata demais API's como informações padrão a serem alteradas pelo usuário já no arquivo de retorno
                    else
                    {
                        itemEtapa["ListaDescricao"] = itemEtapa["TextoObjetoAPI"];
                        itemEtapa["ListaDescricao@type"] = "String";
                        itemEtapa["ListaSessao"] = "{@[ATENCAO_MUDAR]}";
                        itemEtapa["ListaSessao@type"] = "String";
                        itemEtapa["ListaTextoObjetoAPI"] = "{@[ATENCAO_MUDAR]}";
                        itemEtapa["ListaTextoObjetoAPI@type"] = "String";
                        itemEtapa["ListaTitulo"] = "ATENCAO_MUDAR";
                        itemEtapa["ListaTitulo@type"] = "String";
                        itemEtapa["Formato"] = "ATENCAO_MUDAR";
                        itemEtapa["Formato@type"] = "String";
                        itemEtapa["ListaDescricaoFormato"] = "ATENCAO_MUDAR";
                        itemEtapa["ListaDescricaoFormato@type"] = "String";
                        itemEtapa["ListaFormato"] = "ATENCAO_MUDAR";
                        itemEtapa["ListaFormato@type"] = "String";
                    }
                }

                // registra alteração da etapa
                registrosAlteracoes.Add("Etapa: " + itemEtapa["NumEtapa"]);
            }
        }
    }
}
