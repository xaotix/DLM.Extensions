using DLM.db;
using DLM.encoder;
using DLM.vars;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Conexoes
{
    public static class Extensoes_Tabela
    {
        public static List<List<object>> ToList(this DLM.db.Tabela tabela)
        {
            return tabela.Linhas.Select(x => x.GetValores()).ToList();
        }
        public static DLM.db.Tabela Unir(this List<DLM.db.Tabela> tabelas)
        {
            return new DLM.db.Tabela(tabelas);
        }
        public static List<MarcaTecnoMetal> GetMarcas(this DLM.db.Tabela consulta, ref List<Report> erros)
        {
            var _Marcas = new List<MarcaTecnoMetal>();

            List<MarcaTecnoMetal> lista_pecas = new List<MarcaTecnoMetal>();

            foreach (var linha in consulta.Linhas)
            {
                lista_pecas.Add(new MarcaTecnoMetal(linha));
            }

            lista_pecas = lista_pecas.OrderBy(x => x.ToString()).ToList();

            erros.AddRange(lista_pecas.Select(x => x[Cfg.Init.CAD_ATT_ERRO].Valor).Distinct().ToList().FindAll(x => x.Length > 0).Select(x => new Report("Erro", x, TipoReport.Critico)));

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
                    erros.Add(new Report($"{posicao.Key} => Posição com divergências", string.Join("\n", diferencas.Select(x => x.Key)), DLM.vars.TipoReport.Critico));
                }
            }


            return _Marcas;
        }

        public static void Show(this Tabela tabela)
        {
            var mm = new WPF.VerTabela(tabela);
            mm.Show();
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

            mm.lista.Columns.Add("N_0", "L");
            for (int i = 0; i < cols.Count; i++)
            {
                mm.lista.Columns.Add($"N_{i + 1}", cols[i]);
            }
            List<List<string>> ls = new List<List<string>>();
            for (int i = 0; i < tabela.Count; i++)
            {
                List<string> l = new List<string>();
                l.Add(i.ToString());
                l.AddRange(tabela.Linhas[i].Valores());
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
        public static bool GerarExcel(this Tabela tabela, string destino = null, bool cabecalho = true, bool abrir = false, bool congelar = true, bool zerar_se_null = true, string template = null)
        {
            if (destino == null)
            {
                destino = "xlsx".SalvarArquivo();
            }
            if (destino == null) { return false; }
            if (destino.Length == 0) { return false; }


            var pasta = destino.getPasta();
            pasta.CreateDirectory();

            if (destino.Delete())
            {
                if (tabela.Count > 0)
                {
                    Utilz.Excel.GerarExcel(destino, new List<Tabela> { tabela }, template, cabecalho, zerar_se_null, congelar);
                    if (abrir)
                    {
                        destino.Abrir();
                    }
                    return destino.Exists();
                }
            }

            return false;
        }
        public static bool GerarExcel(this List<Tabela> tabelas, string destino = null, bool cabecalho = true, bool abrir = false, bool congelar = true, bool zerar_se_null = true, string template = null)
        {
            if (destino == null)
            {
                destino = "xlsx".SalvarArquivo();
            }
            if (destino == null) { return false; }
            if (destino.Length == 0) { return false; }


            var pasta = destino.getPasta();
            pasta.CreateDirectory();

            if (destino.Delete())
            {
                if (tabelas.Count > 0)
                {
                    Utilz.Excel.GerarExcel(destino, tabelas, template, cabecalho, zerar_se_null, congelar);
                    if (abrir)
                    {
                        destino.Abrir();
                    }
                    return destino.Exists();
                }
            }

            return false;
        }


        public static Tabela GetTabela(this string arquivo)
        {
            var linhas = Utilz.Arquivo.Ler(arquivo).Select(x => x.Split(";".ToCharArray()).ToList().Select(y => y.TrimEnd().TrimStart()).ToList()).ToList();
            return linhas.GetTabela();
        }

        public static Tabela GetTabela(this List<List<string>> linhas, bool primeira_linha_cabecalho = false)
        {
            var retorno = new Tabela();
            if (linhas.Count > 0)
            {
                var nColunas = linhas.Max(x => x.Count);
                var nLinhas = linhas.Count;

                var headers = new List<string>();
                if(primeira_linha_cabecalho)
                {
                    headers.AddRange(linhas[0]);
                    if(headers.Count< nColunas)
                    {
                        for (int i = linhas[0].Count-1; i < nColunas; i++)
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
