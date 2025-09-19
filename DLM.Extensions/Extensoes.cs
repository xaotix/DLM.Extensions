using DLM;
using DLM.cam;
using DLM.cam.Addons;
using DLM.db;
using DLM.desenho;
using DLM.encoder;
using DLM.macros;
using DLM.sap;
using DLM.vars;
using Ionic.Zip;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;
using static System.Net.WebRequestMethods;

namespace Conexoes
{
    public static class Extensoes_SAP
    {
        public static void GetMaterialBase(this List<SAP_Material> materiais)
        {
            var pck_mats = materiais.FindAll(x => x.SAP > 100000).Quebrar(200);
            foreach (var pck in pck_mats)
            {
                var bases = DLM.SAP.MontarMateriaisExplodidos(pck.Select(x => x.SAP).ToList());
                var bases_sap = bases.FindAll(x => x.Codigo != "").ToList();
                foreach (var base_sap in bases_sap)
                {
                    var mp = pck.Find(x => x.SAP == base_sap.Pai.Int());

                    if (mp != null)
                    {
                        mp.SAP_Base = base_sap.Codigo.Int();
                    }
                }
            }
        }
        public static DLM.db.Tabela GetTabela(this IRfcFunction funcao, string tabela)
        {
            try
            {
                var rfc_tbl = funcao.GetTable(tabela);
                var tbl = rfc_tbl.GetTabela();
                tbl.Nome = tabela;
                return tbl;
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            return new Tabela(tabela);
        }
        private static DLM.db.Tabela GetTabela(this IRfcTable tabela)
        {
            var retorno = new DLM.db.Tabela();
            retorno.Nome = tabela.Metadata.Name;
            if (retorno.Nome == "")
            {
                retorno.Nome = tabela.Metadata.LineType.Name;
            }
            foreach (var l in tabela.ToList())
            {
                var nl = new DLM.db.Linha();
                var valores = l.ToList();

                foreach (var v in valores)
                {
                    var type_cel = Celula_Tipo_Valor.NULL;
                    var valor = v.GetValue();
                    var valorstr = "";
                    if (!valor.Vazio())
                    {
                        valorstr = valor.ToString();
                        var type = v.Metadata.DataType;
                        switch (type)
                        {
                            case RfcDataType.XSTRING:
                            case RfcDataType.BYTE:
                                type_cel = Celula_Tipo_Valor.Texto;
                                break;

                            case RfcDataType.STRING:
                                type_cel = Celula_Tipo_Valor.Texto;
                                break;
                            case RfcDataType.CHAR:
                            case RfcDataType.NUM:
                                if (valorstr.ESoNumero())
                                {
                                    type_cel = Celula_Tipo_Valor.Inteiro;
                                }
                                else
                                {
                                    type_cel = Celula_Tipo_Valor.Texto;
                                }
                                break;


                            case RfcDataType.BCD:
                            case RfcDataType.FLOAT:
                            case RfcDataType.DECF16:
                            case RfcDataType.DECF34:
                                type_cel = Celula_Tipo_Valor.Decimal;
                                break;

                            case RfcDataType.INT1:
                            case RfcDataType.INT2:
                            case RfcDataType.INT4:
                            case RfcDataType.INT8:
                            case RfcDataType.DTWEEK:
                            case RfcDataType.TSECOND:
                            case RfcDataType.DTMONTH:
                            case RfcDataType.TMINUTE:
                                type_cel = Celula_Tipo_Valor.Inteiro;
                                break;

                            case RfcDataType.UTCLONG:
                            case RfcDataType.UTCSECOND:
                            case RfcDataType.UTCMINUTE:
                            case RfcDataType.DATE:
                            case RfcDataType.CDAY:
                            case RfcDataType.DTDAY:
                                type_cel = Celula_Tipo_Valor.Data;
                                break;

                            case RfcDataType.TIME:
                                break;

                            case RfcDataType.STRUCTURE:
                            case RfcDataType.TABLE:
                            case RfcDataType.CLASS:
                            case RfcDataType.UNKNOWN:
                                break;
                        }
                    }
                    else
                    {
                        valorstr = null;
                    }

                    var nc = nl.Add(v.Metadata.Name, valorstr, type_cel);
                }
                retorno.Add(nl);
            }

            return retorno;

        }
    }
    public static class Extensoes
    {
        public static string GetBackgroundColorHex(this ExcelRange celula)
        {
            try
            {
                if (celula != null)
                {
                    if (celula.Style != null)
                    {
                        if (celula.Style.Fill != null)
                        {
                            if (celula.Style.Fill.PatternType == ExcelFillStyle.Solid)
                            {
                                if (celula.Style.Fill.BackgroundColor != null)
                                {
                                    var corDeFundo = celula.Style.Fill.BackgroundColor.Rgb;
                                    return $"#{corDeFundo}";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            return "";
        }
        public static void ColarExcel(this System.Windows.Forms.DataGridView grid)
        {
            try
            {
                var obj = (System.Windows.DataObject)System.Windows.Clipboard.GetDataObject();
                var colunas = grid.ColumnCount - 1;
                if (grid.Rows.Count > 0)
                {
                    if (Conexoes.Utilz.Pergunta("Substituir valores atuais?"))
                    {
                        grid.Rows.Clear();
                    }
                }
                if (obj.GetDataPresent(System.Windows.DataFormats.Text))
                {
                    int c0 = grid.Rows.Count;
                    var linhas = Regex.Split(obj.GetData(System.Windows.DataFormats.Text).ToString().Replace("\r", "").Replace("\n", ""), "\r\n").Select(x => x.Split(new char[] { '\t' }).ToList()).ToList().FindAll(x => string.Join("", x).Replace(" ", "") != "");
                    foreach (var linha in linhas)
                    {
                        if (linha.Count <= colunas)
                        {
                            grid.Rows.Add(linha.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Alerta();
            }
        }
        public static System.Windows.Media.Color GetColor(this System.Drawing.Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
        public static System.Drawing.Color GetColor(this System.Windows.Media.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
        public static void OtimizarComprimentos(this List<Tirante> Tirantes)
        {
            foreach (var tr in Tirantes)
            {
                tr.CompUser = 0;
            }
            var comps = Tirantes.Select(x => x.CompCalculado).ToList();
            var comps_otimizados = comps.AgruparPorDistancia(Cfg.CTV2.Tirante_Multiplo_Otimizar);
            foreach (var cmps in comps_otimizados)
            {
                if (cmps.Count > 0)
                {
                    foreach (var cmp in cmps)
                    {
                        var tirs = Tirantes.FindAll(x => x.CompCalculado == cmp);
                        foreach (var tr in tirs)
                        {
                            tr.CompUser = cmps.Max();
                        }
                    }
                }
            }
        }
        public static DLM.cam.Header GetHeader(this SubEtapaTecnoMetal etapa)
        {
            var header = new DLM.cam.Header();
            header.Cliente = etapa.GetPedido().GetObra().Cliente;
            header.Data = DateTime.Now.ToShortDateString();
            header.Equipe = etapa.GetPedido().Equipe;
            header.Etapa = etapa.Nome;
            header.Local = etapa.GetPedido().GetObra().Lugar;
            header.Obra = etapa.GetPedido().GetObra().Nome;
            header.Pedido = etapa.GetPedido().NomePedido;
            return header;
        }
        public static Lado Inverter(this Lado lado)
        {
            return lado == Lado.Direito ? Lado.Esquerdo : Lado.Direito;
        }
        public static string GetPropriedade(this System.DirectoryServices.SearchResult search, string propriedade)
        {
            var props = search.Properties[propriedade];
            if (props.Count > 0)
            {
                return props[0].ToString();
            }

            return "";
        }

        public static List<FuroGage> GetGages(this List<Furo> Furacoes)
        {
            var _Gages = new List<FuroGage>();
            var tipos = Furacoes.GroupBy(x => x.Origem.X.ArredondarMultiplo(2));

            foreach (var tp in tipos)
            {
                var furos = tp.ToList().OrderBy(x => x.Origem.Y).ToList();
                _Gages.Add(new FuroGage(furos));
            }
            _Gages = _Gages.OrderBy(x => x.X).ToList();
            return _Gages;
        }
        public static string CopiarTMP(this string Arquivo, bool abrir = false)
        {
            if (Arquivo.Exists())
            {
                var Extensao = Arquivo.getExtensao();
                var nome = Arquivo.getNome();
                var Pasta = Cfg.Init.TMP_DWG();

                var dest = $"{Pasta}{nome}.{Extensao}";
                int c = 1;
                while (!Arquivo.Copiar(dest, false))
                {
                    dest = $"{Pasta}{nome}_{c}.{Extensao}";
                    c++;
                }
                if (abrir)
                {
                    dest.Abrir();
                }
                return dest;
            }
            return "";
        }
        public static IEnumerable<T> GetChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in GetChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        public static childItem GetChild<childItem>(this DependencyObject obj)
            where childItem : DependencyObject
        {
            foreach (childItem child in GetChildren<childItem>(obj))
            {
                return child;
            }

            return null;
        }

        public static childItem FindVisualChild<childItem>(this DependencyObject obj)

        where childItem : DependencyObject

        {

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)

            {

                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child != null && child is childItem)

                    return (childItem)child;

                else

                {

                    childItem childOfChild = FindVisualChild<childItem>(child);

                    if (childOfChild != null)

                        return childOfChild;

                }

            }

            return null;

        }


        public static void AddKeyEvent<T>(this Window window, System.Windows.Input.KeyEventHandler KeyDown)
        {
            var elementos = window.GetChildren<T>();
            foreach (var elemento in elementos)
            {
                (elemento as FrameworkElement).PreviewKeyDown += KeyDown;
            }
        }


        public static bool Is<T>(this object en)
        {
            return en is T;
        }
        public static List<T> As<T>(this List<object> readData)
        {
            if (readData is List<T>)
            {
                return readData.Cast<T>().ToList();
            }
            try
            {
                return (List<T>)Convert.ChangeType(readData, typeof(T));
            }
            catch (InvalidCastException)
            {
                return default(List<T>);
            }
        }
        public static T As<T>(this object readData)
        {
            if (readData is T)
            {
                return (T)readData;
            }
            try
            {
                return (T)Convert.ChangeType(readData, typeof(T));
            }
            catch (InvalidCastException)
            {
                return default(T);
            }

            //if (readData.Is<T>())
            //{
            //    return (T)Convert.ChangeType(readData, typeof(T));
            //}
            //return default(T);
        }
        public static void MoverParaAPosicaoDoMouse(this Window window)
        {
            var pt = Utilz.GetMousePosition();
            var transform = PresentationSource.FromVisual(window).CompositionTarget.TransformFromDevice;
            var mouse = transform.Transform(pt);
            window.Left = mouse.X - window.ActualWidth;
            window.Top = mouse.Y - window.ActualHeight;
        }




        public static string RemoverSufixDesmembrado(this string nome)
        {
            var nNome = nome
                    .Replace("_1", "")
                    .Replace("_2", "")
                    .Replace("_3", "")
                    .Replace("_4", "")
                    .Replace("_5", "")
                    .Replace("_6", "")
                    .Replace("_7", "")
                    .Replace("_8", "")
                    .Replace("_9", "")
                    .Replace("_10", "")
                    .Replace("_11", "")
                    .Replace("_12", "")
                    .Replace("_13", "")
                    .Replace("_14", "")
                    .Replace("_15", "")
                    .Replace("_16", "")
                    .Replace("_17", "")
                    .Replace("_18", "")
                    .Replace("_19", "")
                    .Replace("_U", "");
            return nNome;
        }
        public static bool Backup(this string arquivo)
        {
            if (arquivo == null)
            {
                return false;
            }
            if (!arquivo.Exists())
            {
                return false;
            }
            var Pasta = arquivo.getPasta();
            var nome = arquivo.getNome();
            var Ext = arquivo.getExtensao();
            var arquivo_backup = $"{Pasta.GetSubPasta(Cfg.Init.PASTA_BACKUPS)}{nome}.BKP.ZIP";
            return Conexoes.Utilz.FazerBackup(Pasta, arquivo_backup, $"*{nome}.{Ext}");
        }

        public static BitmapImage GetImageSource(this string file)
        {
            return new BitmapImage(new Uri(file));
        }





        /// <summary>
        /// Classifica a peça pelo final do Nome, usando o padrão TecnoMetal de CAM
        /// </summary>
        /// <param name="NomeCAM"></param>
        /// <returns></returns>
        public static CAM_TIPO_DESMEMBRADO GetTipoDesmembrado(this string NomeCAM)
        {
            if (
                NomeCAM.EndsWith("_1") |
                NomeCAM.EndsWith("_4") |
                NomeCAM.EndsWith("_7") |
                NomeCAM.EndsWith("_10") |
                NomeCAM.EndsWith("_13") |
                NomeCAM.EndsWith("_16") |
                NomeCAM.EndsWith("_19") |
                NomeCAM.EndsWith("_22") |
                NomeCAM.EndsWith("_25")
                )
            {
                return CAM_TIPO_DESMEMBRADO.Alma;
            }
            else if (
                NomeCAM.EndsWith("_2") |
                NomeCAM.EndsWith("_5") |
                NomeCAM.EndsWith("_8") |
                NomeCAM.EndsWith("_11") |
                NomeCAM.EndsWith("_14") |
                NomeCAM.EndsWith("_17") |
                NomeCAM.EndsWith("_20") |
                NomeCAM.EndsWith("_23") |
                NomeCAM.EndsWith("_26")

                )
            {
                return CAM_TIPO_DESMEMBRADO.Mesa_S;
            }
            else if (
               NomeCAM.EndsWith("_3") |
               NomeCAM.EndsWith("_6") |
               NomeCAM.EndsWith("_9") |
               NomeCAM.EndsWith("_12") |
               NomeCAM.EndsWith("_15") |
               NomeCAM.EndsWith("_18") |
               NomeCAM.EndsWith("_21") |
               NomeCAM.EndsWith("_24") |
               NomeCAM.EndsWith("_27")
                )
            {
                return CAM_TIPO_DESMEMBRADO.Mesa_I;
            }

            else if (NomeCAM.EndsWith("_U"))
            {
                return CAM_TIPO_DESMEMBRADO.Planificado;
            }
            return CAM_TIPO_DESMEMBRADO.Normal;
        }


        public static TAB_FAMILIA_GRP_MERCADORIA GetFamilia(this TAB_MAKTX MAKTX)
        {

            switch (MAKTX)
            {
                case TAB_MAKTX._INVALIDO:
                    return TAB_FAMILIA_GRP_MERCADORIA._INVALIDO;





                case TAB_MAKTX.BARJOIST:
                    return TAB_FAMILIA_GRP_MERCADORIA.BARJOIST;



                case TAB_MAKTX.CANTONEIRA:
                    return TAB_FAMILIA_GRP_MERCADORIA.CANTONEIRA;

                case TAB_MAKTX.CHUMBADOR:
                case TAB_MAKTX.INSERTO:
                    return TAB_FAMILIA_GRP_MERCADORIA.CHUMBADOR_INSERTO;


                case TAB_MAKTX.ESCADA:
                case TAB_MAKTX.ESCADA_MARINHEIRO:
                case TAB_MAKTX.LATERAL_DE_ESCADA:
                case TAB_MAKTX.ESCADA_MARINHEIRO_PULTRUDADA:
                case TAB_MAKTX.PASSARELA_PULTRUDADA:
                case TAB_MAKTX.GUARDA_CORPO_PULTRUDADO:
                case TAB_MAKTX.PLATAFORMA_PASSARELA:
                case TAB_MAKTX.DEGRAU:
                    return TAB_FAMILIA_GRP_MERCADORIA.ESCADA;



                case TAB_MAKTX.ILUMINACAO_ZENITAL:
                case TAB_MAKTX.TELHA_POLICARBONATO:
                    return TAB_FAMILIA_GRP_MERCADORIA.ILUMINACAO;


                case TAB_MAKTX.LANTERNIN:
                case TAB_MAKTX.QUADRO_DE_TELA:
                    return TAB_FAMILIA_GRP_MERCADORIA.LANTERNIN;

                case TAB_MAKTX.DIAGONAL_MEDAJOIST:
                case TAB_MAKTX.BANZO_INFERIOR_MEDAJOIST:
                case TAB_MAKTX.BANZO_SUPERIOR_MEDAJOIST:
                    return TAB_FAMILIA_GRP_MERCADORIA.MEDAJOIST;

                case TAB_MAKTX.DIAGONAL_MEDABAR:
                case TAB_MAKTX.BANZO_INFERIOR_MEDABAR:
                case TAB_MAKTX.BANZO_SUPERIOR_MEDABAR:
                    return TAB_FAMILIA_GRP_MERCADORIA.MEDABAR;


                case TAB_MAKTX.GUARDA_CORPO:
                case TAB_MAKTX.TERCA_COM_ACABAMENTO:
                case TAB_MAKTX.SUPORTE:
                case TAB_MAKTX.CALHA_TIPO_SE:
                case TAB_MAKTX.GRADE_DE_PISO:
                case TAB_MAKTX.CHAPA_DE_PISO:
                case TAB_MAKTX.CHAPA:
                case TAB_MAKTX.TALA:
                case TAB_MAKTX.CONECTOR:
                case TAB_MAKTX.CARTOLA:
                case TAB_MAKTX.CABO_GUIA:
                case TAB_MAKTX.FLANGE_BRACE:
                case TAB_MAKTX.PERFIL_DOBRADO:
                case TAB_MAKTX.CORRENTE_RIGIDA_MISC:
                case TAB_MAKTX.TIRANTE:
                case TAB_MAKTX.CONTRAVENTO_MSC:
                case TAB_MAKTX.MAO_FRANCESA_MISCELANEA:
                    return TAB_FAMILIA_GRP_MERCADORIA.MISCELANEA;


                case TAB_MAKTX.PILARETE:
                case TAB_MAKTX.PONTALETE:
                    return TAB_FAMILIA_GRP_MERCADORIA.MISCELANEA_PERFIL;


                case TAB_MAKTX.COLUNA_SOLDADA:
                case TAB_MAKTX.VIGA_SOLDADA:
                case TAB_MAKTX.MAO_FRANCESA_SOLD:
                case TAB_MAKTX.CONTRAVENTO_SOLD:
                case TAB_MAKTX.MONOVIA_SOLD:
                case TAB_MAKTX.VIGA_DE_ROLAMENTO_SOLD:
                case TAB_MAKTX.VIGA_CAIXAO_SOLD:
                case TAB_MAKTX.COLUNA_CAIXAO_SOLD:
                    return TAB_FAMILIA_GRP_MERCADORIA.PERFIL_SOLDADO;


                case TAB_MAKTX.CONTRAVENTO_LAM_GEN:
                case TAB_MAKTX.CORRENTE_RIGIDA_LAM:
                case TAB_MAKTX.MAO_FRANCESA_LAM_GEN:
                case TAB_MAKTX.TIRANTE_LAM:
                case TAB_MAKTX.TRILHO:
                case TAB_MAKTX.VIGA_LAM_GEN:
                case TAB_MAKTX.COLUNA_LAM_GEN:
                    return TAB_FAMILIA_GRP_MERCADORIA.PERFIL_LAM_GENERICO;


                case TAB_MAKTX.VIGA_LAM_W:
                case TAB_MAKTX.PERFIL_DE_ROLAMENTO_LAM:
                case TAB_MAKTX.COLUNA_LAM_W:
                case TAB_MAKTX.MONOVIA_LAM:
                case TAB_MAKTX.MAO_FRANCESA_LAM_W:
                case TAB_MAKTX.CONTRAVENTO_LAM_W:
                case TAB_MAKTX.TERCA_PERFIL_W:
                    return TAB_FAMILIA_GRP_MERCADORIA.PERFIL_LAM_W;


                case TAB_MAKTX.TERCA_PURLIN_C:
                case TAB_MAKTX.TERCA_PURLIN_Z:
                    return TAB_FAMILIA_GRP_MERCADORIA.PURLIN;


                case TAB_MAKTX.PANEL_RIB_II:
                    return TAB_FAMILIA_GRP_MERCADORIA.PANEL_RIB_II;
                case TAB_MAKTX.PANEL_RIB_III:
                    return TAB_FAMILIA_GRP_MERCADORIA.PANEL_RIB_III;



                case TAB_MAKTX.SSR1:
                case TAB_MAKTX.SSR1BF:
                case TAB_MAKTX.SSR1BM:
                case TAB_MAKTX.SSR1F:
                case TAB_MAKTX.SSR1M:
                    return TAB_FAMILIA_GRP_MERCADORIA.SSR1;
                case TAB_MAKTX.SSR2:
                case TAB_MAKTX.CP_SSR2:
                    return TAB_FAMILIA_GRP_MERCADORIA.SSR2;
                case TAB_MAKTX.STEEL_DECK:
                    return TAB_FAMILIA_GRP_MERCADORIA.STEEL_DECK;
                case TAB_MAKTX.TELHA_FORRO:
                    return TAB_FAMILIA_GRP_MERCADORIA.TELHA_FORRO;
                case TAB_MAKTX.TELHA_ONDULADA:
                    return TAB_FAMILIA_GRP_MERCADORIA.TELHA_ONDULADA;



                case TAB_MAKTX.VIGA_LAM_W_TRELICA:
                case TAB_MAKTX.VIGA_LAM_GEN_TRELICA:
                case TAB_MAKTX.CHAPA_TRELICA:
                case TAB_MAKTX.PERFIL_DOBRADO_TRELICA:
                case TAB_MAKTX.VIGA_SOLDADA_TRELICA:
                    return TAB_FAMILIA_GRP_MERCADORIA.TRELICA_PARAFUSADA;


                case TAB_MAKTX.TRELICA_SOLDADA:
                    return TAB_FAMILIA_GRP_MERCADORIA.TRELICA_SOLDADA;


                case TAB_MAKTX.CALHA:
                case TAB_MAKTX.ARREMATE:
                case TAB_MAKTX.CUMEEIRA:
                case TAB_MAKTX.CARTOLA_TRINS:
                case TAB_MAKTX.CAIXA_DE_COLETA:
                case TAB_MAKTX.TAMPAS:
                    return TAB_FAMILIA_GRP_MERCADORIA.TRINS;


                case TAB_MAKTX.VENEZIANA_FIXA:
                case TAB_MAKTX.VENEZIANA_MOVEL:
                    return TAB_FAMILIA_GRP_MERCADORIA.VENEZIANA;
                case TAB_MAKTX.SIE_RM:
                    break;
                case TAB_MAKTX.SUPORTE_KANBAN:
                    return TAB_FAMILIA_GRP_MERCADORIA.MISCELANEA_KANBAN;
                case TAB_MAKTX.BANZO_INFERIOR_MEDAJ:
                case TAB_MAKTX.BANZO_SUPERIOR_MEDAJ:
                    return TAB_FAMILIA_GRP_MERCADORIA.MEDAJOIST;
            }


            return TAB_FAMILIA_GRP_MERCADORIA._DESCONHECIDO;
        }


        public static List<T> GetChildren<T>(this Window window)
        {
            if (window.Content is Panel)
            {
                var item = window.Content as Panel;
                return GetChildren<T>(item);
            }
            else if (window.Content is Grid)
            {
                var item = window.Content as Grid;
                return GetChildren<T>(item);
            }
            else if (window.Content is TabControl)
            {
                var item = window.Content as TabControl;
                return GetChildren<T>(item);
            }
            return new List<T>();
        }
        public static List<T> GetChildren<T>(this Panel panel)
        {
            List<T> Retorno = new List<T>();
            if (panel == null) { return Retorno; }

            var children = panel.Children.Cast<FrameworkElement>().ToList();
            Retorno.AddRange(children.OfType<T>());

            var panels = new List<Xceed.Wpf.AvalonDock.DockingManager>();
            panels.AddRange(children.OfType<Xceed.Wpf.AvalonDock.DockingManager>());
            foreach (var subpanel in panels)
            {
                children.AddRange(GetChildren(subpanel));
            }

            foreach (var subpanel in children.OfType<Panel>())
            {
                Retorno.AddRange(GetChildren<T>(subpanel));
            }
            foreach (var subpanel in children.OfType<Selector>())
            {
                Retorno.AddRange(GetChildren<T>(subpanel));
            }

            foreach (var subpanel in children.OfType<GroupBox>())
            {
                if (subpanel.Content is Panel)
                {
                    Retorno.AddRange(GetChildren<T>(subpanel.Content as Panel));
                }
                else if (subpanel.Content is Selector)
                {
                    Retorno.AddRange(GetChildren<T>(subpanel.Content as Selector));
                }
            }

            return Retorno;
        }

        private static List<FrameworkElement> GetChildren(Xceed.Wpf.AvalonDock.DockingManager subpanel)
        {
            List<FrameworkElement> children = new List<FrameworkElement>();
            var layoutpanel = subpanel.Layout.RootPanel;
            var la = new List<Xceed.Wpf.AvalonDock.Layout.LayoutAnchorable>();
            var ld = new List<Xceed.Wpf.AvalonDock.Layout.LayoutContent>();
            la.AddRange(layoutpanel.Children.OfType<Xceed.Wpf.AvalonDock.Layout.LayoutAnchorablePane>().SelectMany(x => x.Children.ToList()));
            la.AddRange(layoutpanel.Children.OfType<Xceed.Wpf.AvalonDock.Layout.LayoutAnchorablePaneGroup>().SelectMany(x => x.Children.OfType<Xceed.Wpf.AvalonDock.Layout.LayoutAnchorablePane>().SelectMany(y => y.Children)));

            ld.AddRange(layoutpanel.Children.OfType<Xceed.Wpf.AvalonDock.Layout.LayoutDocumentPane>().SelectMany(x => x.Children.ToList()));
            ld.AddRange(layoutpanel.Children.OfType<Xceed.Wpf.AvalonDock.Layout.LayoutDocumentPaneGroup>().SelectMany(x => x.Children.OfType<Xceed.Wpf.AvalonDock.Layout.LayoutDocumentPane>()).SelectMany(x => x.Children));

            children.AddRange(la.Select(x => x.Content).OfType<FrameworkElement>());
            children.AddRange(ld.Select(x => x.Content).OfType<FrameworkElement>());
            return children;
        }

        public static List<T> GetChildren<T>(this Selector subpanel)
        {
            List<T> Retorno = new List<T>();
            foreach (var tab in subpanel.Items)
            {
                if (tab is T)
                {
                    Retorno.Add(tab.As<T>());
                }
            }

            foreach (var tab in subpanel.Items.OfType<HeaderedContentControl>())
            {
                if (tab.Header is Panel)
                {
                    Retorno.AddRange(GetChildren<T>(tab.Header as Panel));
                }
                if (tab.Content is T)
                {
                    Retorno.Add(tab.Content.As<T>());
                }
                if ((tab.Content is Panel))
                {
                    Retorno.AddRange(GetChildren<T>(tab.Content as Panel));
                }
                else if (tab.Content is Selector)
                {
                    Retorno.AddRange(GetChildren<T>(tab.Content as Selector));
                }
            }
            if (subpanel.ContextMenu is T)
            {
                Retorno.Add(subpanel.ContextMenu.As<T>());
            }

            return Retorno;
        }

        public static List<TreeViewItem> GetAllNodes(this TreeView treeView)
        {
            List<TreeViewItem> retorno = new List<TreeViewItem>();

            foreach (TreeViewItem item in treeView.Items)
            {
                retorno.Add(item);
                retorno.AddRange(item.GetAllNodes());
            }
            return retorno;
        }
        public static List<TreeViewItem> GetAllNodes(this TreeViewItem treeView)
        {
            List<TreeViewItem> retorno = new List<TreeViewItem>();

            foreach (var item in treeView.Items)
            {
                if (item is TreeViewItem)
                {
                    var node = item as TreeViewItem;
                    retorno.Add(node);
                    retorno.AddRange(node.GetAllNodes());
                }

            }
            return retorno;
        }
        public static void Selecionar(this TreeViewItem sel)
        {
            sel.IsSelected = true;
            while (sel.Parent is TreeViewItem)
            {
                TreeViewItem pai = sel.Parent as TreeViewItem;
                pai.IsExpanded = true;
                sel = pai;
            }
        }




        public static void CriarCopiaRev(this string Arquivo)
        {
            if (Arquivo.Exists())
            {
                var pasta = Arquivo.getPasta();
                pasta = pasta.GetSubPasta("REVISOES");
                var nome = Arquivo.getNome();
                var extensao = Arquivo.getExtensao();
                var rv = "R00";
                var arquivo = $"{pasta}{nome}.{rv}.{extensao}";
                int c = 1;
                while (arquivo.Exists())
                {
                    c++;
                    arquivo = $"{pasta}{nome}.R{c.String(2)}{extensao}";
                }
                /*Cria uma cópia do arquivo atual*/
                Arquivo.Copiar(arquivo);
            }
        }





        //public static double Prompt(this double valor)
        //{
        //    bool status = false;
        //    var ret = valor.Prompt(out status);
        //    return status ? ret : valor;
        //}



        public static void Add(this ObservableCollection<Report> reports, string propriedades = "", string mensagem = "", TipoReport report = TipoReport.Status)
        {
            reports.Add(new Report(propriedades, mensagem, report));
        }
        /// <param name="Datagrid"></param>


        public static T GetEnum<T>(this int Nome)
        {
            var valores = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            var igual = valores.Find(x => Convert.ToInt32(x) == Nome);
            if (igual != null)
            {
                return igual;
            }
            return default(T);
        }
        public static T GetEnum<T>(this string Nome)
        {
            try
            {

                var tipos = Conexoes.Utilz.GetLista_Enumeradores<T>();
                foreach (var t in tipos)
                {
                    if (t.ToString() == Nome)
                    {
                        return t;
                    }
                }
            }
            catch (Exception)
            {


            }
            return default(T);
        }

        public static void CarregarIni<T>(this T objeto, string arquivo)
        {
            objeto.CopiarVars(arquivo.CarregarIni<T>());
        }



        public static bool Valido(this DateTime data)
        {
            return data > Cfg.Init.DataDummy;
        }
        public static bool Valido(this DateTime? data)
        {
            return data > Cfg.Init.DataDummy;
        }
        public static DateTime GetValue(this DateTime? data)
        {
            if (data == null)
            {
                return Cfg.Init.DataDummy;
            }
            return data.Value;
        }

        public static void SalvarLista(this List<string> linhas, string arquivo, bool substituir = true)
        {
            Conexoes.Utilz.Arquivo.Gravar(arquivo, linhas, null, substituir);
        }


        public static void SetPtBr(this System.Windows.Window window)
        {
            window.Language = XmlLanguage.GetLanguage(Conexoes.Utilz._BR.IetfLanguageTag);
        }
        public static void SetEnUs(this System.Windows.Window window)
        {
            window.Language = XmlLanguage.GetLanguage(Conexoes.Utilz._US.IetfLanguageTag);
        }
    }
}
