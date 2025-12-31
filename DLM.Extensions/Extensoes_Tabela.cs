using DLM.db;
using DLM.encoder;
using DLM.orc;
using DLM.vars;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Conexoes
{
    public static class Extensoes_Tabela
    {
        public static string GetJSON(this Celula celula)
        {
            //"localidade": "São Paulo"
            return Utilz._Aspas + celula.Coluna + Utilz._Aspas + ": " + Utilz._Aspas + celula.Valor + Utilz._Aspas;
        }
        public static string GetJSON(this Linha linha)
        {
            string p = "    {\n";
            for (int i = 0; i < linha.Count; i++)
            {
                p = p + (i > 0 ? ",\n" : "") + "      " + linha[i].GetJSON();
            }
            p = p + "\n    }";

            return p;
        }
        public static string GetJSON(this Tabela tabela)
        {
            string p = "{" +
                "\n  " + Utilz._Aspas + "Nome" + Utilz._Aspas + ":" + Utilz._Aspas + tabela.Nome + Utilz._Aspas + "," +
               "\n  " + Utilz._Aspas + "Valores" + Utilz._Aspas + ": \n  [\n";
            for (int i = 0; i < tabela.Count; i++)
            {
                p = p + (i > 0 ? ",\n" : "") + tabela[i].GetJSON();
            }
            p = p + "\n  ]\n}";
            return p;
        }
        public static List<List<object>> ToObjectList(this DLM.db.Tabela tabela)
        {
            return tabela.Select(x => x.GetValores()).ToList();
        }
        public static DLM.db.Tabela Unir(this List<DLM.db.Tabela> tabelas)
        {
            return new DLM.db.Tabela(tabelas);
        }
        public static List<MarcaTecnoMetal> GetMarcas(this DLM.db.Tabela consulta, ref List<Report> erros)
        {
            var _Marcas = new List<MarcaTecnoMetal>();

            List<MarcaTecnoMetal> lista_pecas = new List<MarcaTecnoMetal>();

            foreach (var linha in consulta)
            {
                lista_pecas.Add(new MarcaTecnoMetal(linha));
            }

            lista_pecas = lista_pecas.OrderBy(x => x.ToString()).ToList();

            erros.AddRange(lista_pecas.Select(x => x[Cfg.Init.CAD_ATT_ERRO].Valor).Distinct().ToList().FindAll(x => x.LenghtStr() > 0).Select(x => new Report("Erro", x, TipoReport.Critico)));

            var ms = lista_pecas.Select(x => x.Nome).Distinct().ToList();

            foreach (var m in ms)
            {
                var iguais = lista_pecas.FindAll(x => x.Nome == m);

                var marcas = iguais.FindAll(x => x.Nome_Posicao == "");
                var posicoes = iguais.FindAll(x => x.Nome_Posicao != "");
                if (m == "")
                {
                    erros.Add(new Report("Blocos com erros ou não foi possível ler os dados das marcas.", $" {marcas[0].Arquivo} - Qtd: {marcas.Count} Blocos: {string.Join("|", marcas.Select(x => x.NomeBloco).Distinct())}", TipoReport.Critico));
                }
                else if (posicoes.FindAll(x => x.Nome.Replace(" ", "") == "").Count > 0)
                {
                    erros.Add(new Report("Marca com posições com Blocos com erros ou não foi possível ler os dados das.", $" {marcas[0].Arquivo} - M: {m}", TipoReport.Critico));
                }
                else if (marcas.Count == 1)
                {
                    var marca = marcas[0];
                    marca.SetSubItems(posicoes);
                    if (posicoes.Count > 0)
                    {
                        marca.PesoUnit = marca.GetPosicoes().Sum(x => x.PesoUnit * x.Quantidade);
                        marca.Superficie = marca.GetPosicoes().Sum(x => x.Superficie * x.Quantidade);
                    }
                    foreach (var pos in posicoes)
                    {
                        pos.Pai = marca;
                    }
                    _Marcas.Add(marca);
                }
                else if (marcas.Count > 1)
                {
                    erros.Add(new Report("Marcas duplicadas", $" {marcas[0].Arquivo} - M: {m}", TipoReport.Critico));
                }
            }
            var posp = _Marcas.SelectMany(x => x.GetPosicoes()).GroupBy(x => x.Nome_Posicao);
            foreach (var posicao in posp)
            {
                var diferencas = posicao.ToList().GroupBy(x => x.GetChave()).ToList();
                if (diferencas.Count > 1)
                {
                    erros.Add(new Report($"Posição com divergências",
                        $"\nPrancha(s)={string.Join(", ", posicao.ToList().GroupBy(x => x.Arquivo).Select(x => x.Key))}" +
                        $"\nDivergências:\n{posicao.Key}\n {string.Join("\n", diferencas.Select(x => x.Key.TrimStart().TrimEnd()))}", DLM.vars.TipoReport.Critico));
                }
            }


            return _Marcas;
        }

        public static void Show(this Tabela tabela, bool display_names = false)
        {
            if (tabela.Count > 0)
            {
                var mm = new WPF.VerTabela(tabela);
                mm.Show();
            }
        }
        public static void Show(this List<Linha> linhas)
        {

            if (linhas.Count > 0)
            {
                var mm = new WPF.VerTabela(new Tabela(linhas));
                mm.Show();
            }
        }
        public static Linha ListaSelecionar(this Tabela tabela)
        {
            var mm = new WPF.VerTabela(tabela);
            mm.ShowDialog();

            return mm._grid.Selecao<Linha>();
        }
        public static List<DLM.db.Linha> Selecionar(this Tabela tabela, bool multiplas_linhas = true)
        {
            JanelasWinForms.DBTabelaGrid mm = new JanelasWinForms.DBTabelaGrid();
            mm.lista.Rows.Clear();
            mm.lista.Columns.Clear();
            mm.lista.MultiSelect = multiplas_linhas;

            var cols = tabela.GetColunas();

            mm.lista.Columns.Add("N_0", "l");
            for (int i = 0; i < cols.Count; i++)
            {
                mm.lista.Columns.Add($"N_{i + 1}", cols[i]);
            }
            List<List<string>> ls = new List<List<string>>();
            for (int i = 0; i < tabela.Count; i++)
            {
                List<string> l = new List<string>();
                l.Add(i.ToString());
                l.AddRange(tabela[i].Valores());
                ls.Add(l);
            }
            foreach (var L in ls)
            {
                mm.lista.Rows.Add(L.ToArray());
            }


            /*criar 1 filtro*/
            var campos = "Field = {0}";
            for (int i = 0; i < cols.Count; i++)
            {
                campos = campos + "{" + (i + 1) + "}";
            }
            mm.lista.Columns[0].Visible = false;


            mm.ShowDialog();
            List<DLM.db.Linha> retorno = new List<Linha>();
            if (mm.DialogResult == System.Windows.Forms.DialogResult.OK && mm.lista.SelectedRows.Count > 0)
            {
                ;
                for (int i = 0; i < mm.lista.RowCount; i++)
                {
                    var L = mm.lista.Rows[0];
                    if (L.Selected)
                    {
                        retorno.Add(tabela[i]);
                    }
                }

            }
            return retorno;
        }
        public static bool GerarExcel(this Tabela tabela, string destino = null, bool add_cab = true, bool abrir = false, bool congelar = true, bool set_null_if_zero = true, string arq_template = null, bool girar_cabecalho = false, bool auto_fit = true)
        {
            return GerarExcel(new List<Tabela> { tabela }, destino, add_cab, abrir, congelar, set_null_if_zero, arq_template, girar_cabecalho, false, auto_fit);
        }
        public static bool GerarExcel(this List<Tabela> tabelas, string destino = null, bool add_cab = true, bool abrir = false, bool congelar = true, bool set_null_if_zero = true, string arq_template = null, bool gira_cab = false, bool unir_abas = false, bool auto_fit = true)
        {
            if (destino == null)
            {
                destino = "xlsx".SalvarArquivo();
                abrir = true;
            }
            if (destino == null) { return false; }
            if (destino.LenghtStr() == 0) { return false; }


            var pasta = destino.getPasta();
            pasta.CreateDirectory();

            if (destino.Delete())
            {
                if (tabelas.Count > 0)
                {
                    if (tabelas.Count > 0)
                    {
                        if (set_null_if_zero)
                        {
                            foreach (var aba in tabelas)
                            {
                                aba.SetNullIfZero();
                            }
                        }

                    denovo:
                        var arquivo = new FileInfo(destino);
                        if (arquivo.Exists)
                        {
                            try
                            {
                                arquivo.Delete();
                            }
                            catch (Exception ex)
                            {
                                if ($"Ocorreu um erro ao tentar substituir o arquivo {arquivo.ToString()}\n\n{ex.Message}\n\nTentar Novamente?".Pergunta())
                                {
                                    goto denovo;
                                }

                                return false;
                            }
                        }
                        if (arq_template != null)
                        {
                            if (arq_template.Exists())
                            {
                                if (!arq_template.Copiar(destino))
                                {
                                    $"Não foi possível copiar o template {arq_template}  \n para o destino\n {destino}".Alerta();
                                    return false;
                                }
                            }
                        }

                        try
                        {
                            var nExcel = new OfficeOpenXml.ExcelPackage(arquivo);

                            for (int i = 0; i < tabelas.Count; i++)
                            {
                                if (tabelas[i].Nome.Replace(" ", "").LenghtStr() == 0)
                                {
                                    tabelas[i].Nome = $"ABA_{i.String(3)}";
                                }
                                else if (tabelas.FindAll(x => x.Nome.ToUpper() == tabelas[i].Nome.ToUpper()).Count > 1)
                                {
                                    tabelas[i].Nome += $"_{i}";
                                }
                            }
                            int l0 = 1;
                            int l_cab = 1;
                            int c0 = 1;
                            int c_cab = 1;

                            foreach (var tabela in tabelas)
                            {

                                var colunas = tabela.GetColunas();
                                if (colunas.Count == 0) { continue; }

                                ExcelWorksheet aba = null;

                                if (unir_abas)
                                {

                                    if (nExcel.Workbook.Worksheets.Count == 0)
                                    {
                                        aba = nExcel.Workbook.Worksheets.Add("Planilha1");
                                    }
                                    else
                                    {
                                        aba = nExcel.Workbook.Worksheets.First();
                                    }
                                    l0++;
                                    l0++;
                                    var tit = aba.Cells[l0, c0, l0, colunas.Count];
                                    tit.Merge = true;
                                    tit.SetValor(new Celula("ABA", tabela.Nome));
                                    tit.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                    SetBackground(tit);
                                    l0++;
                                }
                                else
                                {
                                    l0 = tabela.Excel_L0 + 1;
                                    c0 = tabela.Excel_C0 + 1;

                                    aba = nExcel.Workbook.Worksheets.ToList().Find(x => x.Name.ToUpper() == tabela.Nome.ToUpper());
                                    if (aba == null)
                                    {
                                        aba = nExcel.Workbook.Worksheets.Add(tabela.Nome);
                                    }
                                }

                                l_cab = l0;

                                if (add_cab && arq_template == null)
                                {
                                    for (int c = 0; c < colunas.Count; c++)
                                    {
                                        aba.Cells[l0, c + c0].Value = colunas[c];
                                    }
                                    l0++;
                                }
                                else if (arq_template != null)
                                {
                                    colunas = new List<string>();
                                    for (int i = 0; i < aba.Dimension.End.Column; i++)
                                    {
                                        var cel = aba.Cells[l0, i + 1];

                                        var valor = cel.Text;
                                        if (valor == null | valor == "")
                                        {
                                            valor = cel.Address;
                                        }
                                        colunas.Add(valor);
                                    }
                                    l0++;
                                }

                                for (int l = 0; l < tabela.Count; l++)
                                {
                                    for (int c = 0; c < colunas.Count; c++)
                                    {
                                        if (colunas[c] == "")
                                        {
                                            continue;
                                        }
                                        var nCel = new Celula(colunas[c], null);
                                        if (colunas[c] == tabela[l][c].ColunaUpper)
                                        {
                                            nCel = tabela[l][c];
                                        }
                                        else
                                        {
                                            nCel = tabela[l][colunas[c]];
                                        }

                                        aba.Cells[l0, c + c0].SetValor(nCel);
                                    }
                                    l0++;
                                }

                                if (add_cab && arq_template == null)
                                {
                                    try
                                    {
                                        var range_header = aba.Cells[l_cab, c_cab, l_cab, colunas.Count];

                                        range_header.AutoFilter = true;
                                        range_header.Style.WrapText = false;
                                        range_header.Style.ShrinkToFit = true;
                                        range_header.Style.TextRotation = gira_cab ? 90 : 0;

                                        SetBackground(range_header);

                                        if (auto_fit && l0 < 100000)
                                        {
                                            aba.Cells[aba.Dimension.Address].AutoFitColumns();
                                            //range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                        }
                                    }
                                    catch (Exception)
                                    {

                                    }

                                    if (congelar && !unir_abas)
                                    {
                                        aba.View.FreezePanes(2, 1);
                                    }
                                }
                            }
                            nExcel.Workbook.Properties.Author = $"DLM.excel v{System.Windows.Forms.Application.ProductVersion}";

                            if (nExcel.Workbook.Worksheets.Count > 0)
                            {
                                nExcel.Save();
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.Alerta("Gerar Excel");
                        }
                    }

                    if (abrir)
                    {
                        destino.Abrir();
                    }
                    return destino.Exists();
                }
            }

            return false;
        }

        private static void SetBackground(ExcelRange range)
        {
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightCyan);
            range.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        }

        public static void SetValor(this ExcelRange excel_cel, Celula cel)
        {
            if (cel.Valor != null)
            {
                switch (cel.Tipo)
                {
                    case Celula_Tipo_Valor.Data:
                    case Celula_Tipo_Valor.DataHora:
                        excel_cel.Style.Numberformat.Format = "dd/mm/yyyy";
                        excel_cel.Value = cel.DataNull();
                        break;
                    case Celula_Tipo_Valor.Texto:
                        excel_cel.Value = cel.Valor;
                        break;
                    case Celula_Tipo_Valor.Decimal:
                    case Celula_Tipo_Valor.Moeda:
                        excel_cel.Value = cel.DoubleNull();
                        break;
                    case Celula_Tipo_Valor.Inteiro:
                        var valor = cel.Long();
                        excel_cel.Value = valor;
                        if (valor > 99999999999)
                        {
                            excel_cel.Style.Numberformat.Format = "0";
                        }
                        break;

                    case Celula_Tipo_Valor.Booleano:
                        excel_cel.Value = cel.Boolean() ? "SIM" : "";
                        break;
                    case Celula_Tipo_Valor.NULL:
                        break;
                    case Celula_Tipo_Valor.Hora:
                        excel_cel.Value = cel.TimeSpanNull();
                        excel_cel.Style.Numberformat.Format = "hh:mm:ss";
                        break;
                    case Celula_Tipo_Valor.Binario:
                        break;
                    case Celula_Tipo_Valor.Array:
                        break;
                    case Celula_Tipo_Valor.Desconhecido:
                        break;
                }

                if (cel.Tipo == Celula_Tipo_Valor.Moeda | cel.StringFormat == "c0" | cel.StringFormat == "R$")
                {
                    excel_cel.Style.Numberformat.Format = "R$ #,##0.00;[Red]-R$ #,##0.00";
                }
                else if (cel.StringFormat == "$")
                {
                    excel_cel.Style.Numberformat.Format = "$ #,##0.00;[Red]-$ #,##0.00";
                }
                else if (cel.StringFormat == "Kg")
                {
                    excel_cel.Style.Numberformat.Format = "#,##";
                }
                else if (cel.StringFormat == "P")
                {
                    excel_cel.Style.Numberformat.Format = "0.00%;[Red]-0.00%";
                }
            }
        }

        public static Tabela GetTabela(this List<List<string>> linhas, bool primeira_linha_cabecalho = false)
        {
            var retorno = new Tabela();
            if (linhas.Count > 0)
            {
                var nColunas = linhas.Max(x => x.Count);
                var nLinhas = linhas.Count;

                var headers = new List<string>();
                if (primeira_linha_cabecalho)
                {
                    headers.AddRange(linhas[0]);
                    if (headers.Count < nColunas)
                    {
                        for (int i = linhas[0].Count - 1; i < nColunas; i++)
                        {
                            headers.Add($"COLUNA_{i + 1}");
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < nColunas; i++)
                    {
                        headers.Add($"COLUNA_{i + 1}");
                    }
                }


                for (int i = 0; i < nLinhas; i++)
                {
                    var l = linhas[i];
                    var nl = new Linha();
                    for (int c = 0; c < l.Count; c++)
                    {
                        nl.Add(headers[c], l[c]);
                    }
                    retorno.Add(nl);
                }


                return retorno;
            }
            else
            {
                return new Tabela();
            }
        }
    }
}
