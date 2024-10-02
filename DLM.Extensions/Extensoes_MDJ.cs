using DLM.desenho;
using DLM.mdj;
using DLM.medabar;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Conexoes
{
    public static class Extensoes_MDJ
    {
        public static List<double> XS(this List<MDJ_Furo> furos)
        {
            return furos.Select(x => x.X).Distinct().ToList().OrderBy(x => x).ToList();
        }
        public static List<double> EntreFuros(this List<MDJ_Furo> furos)
        {
            var ret = new List<double>();
            var frs = XS(furos);
            if (frs.Count > 1)
            {
                for (int i = 1; i < frs.Count; i++)
                {
                    ret.Add(Math.Round(frs[i] - frs[i - 1]));
                }
            }
            return ret;
        }
        public static netDxf.DxfDocument GetVista(this MDJ_Programa programa, Point origem, double escala, netDxf.DxfDocument retorno = null, netDxf.AciColor cor = null, bool inverter_acumuladas = false, string texto = "", bool criar_bloco = true)
        {
            if (retorno == null)
            {
                retorno = new netDxf.DxfDocument();
            }
            double x0 = 0;
            double y0 = 0;

            var Nomelayer = programa.Marca;
            if (Nomelayer == "")
            {
                Nomelayer = "BANZOS";
            }
            var layer = retorno.GetLayer(Nomelayer, netDxf.AciColor.Yellow, netDxf.Tables.Linetype.ByLayer);
            var cotas = retorno.GetLayer("COTAS", netDxf.AciColor.Cyan, netDxf.Tables.Linetype.ByLayer);
            var Nome_banzos = retorno.GetLayer("BANZOS", netDxf.AciColor.Red, netDxf.Tables.Linetype.ByLayer);

            if (cor == null)
            {
                cor = netDxf.AciColor.ByLayer;
            }




            double larg_alma = 60;
            double larg_mesa = 90;

            if (programa.Tipo_Banzo == MDJ_Tipo_Banzo.BSD)
            {
                larg_mesa = 110;
                larg_alma = 80;
            }


            var estilo = DLM.desenho.Dxf.GetEstilo("COTAS", escala);
            var coord_grupos = programa.Grupos.Select(x => new P3d(x.X0 + x0 + origem.X, origem.Y - larg_alma)).ToList();
            var furos = programa.GetFuros();
            var coord_furos = furos.Select(x => new P3d(x.X + x0 + origem.X, x.Y + origem.Y - larg_alma)).ToList();
            coord_furos.Insert(0, new P3d(x0 + origem.X, origem.Y - larg_alma));
            coord_furos.Add(new P3d(programa.Comprimento + x0 + origem.X, origem.Y - larg_alma));

            coord_grupos.Insert(0, new P3d(x0 + origem.X, origem.Y - larg_alma));
            coord_grupos.Add(new P3d(programa.Comprimento + x0 + origem.X, origem.Y - larg_alma));


            var cotas2 = DLM.desenho.Dxf.CotasHorizontais(coord_grupos, larg_mesa + y0 + origem.Y, escala * 5, escala, cotas, estilo);

            var acumuladas_furos = DLM.desenho.Dxf.CotasAcumuladas(coord_furos, escala * 5, Sentido.Horizontal, cotas, estilo, inverter_acumuladas);
            var acumuladas_grupos = DLM.desenho.Dxf.CotasAcumuladas(coord_grupos, escala * 15, Sentido.Horizontal, cotas, estilo, inverter_acumuladas);

            var rect = DLM.desenho.Dxf.Retangulo(programa.Comprimento, larg_mesa, x0 + origem.X, y0 + origem.Y, layer, cor);
            var rect_alma = DLM.desenho.Dxf.Retangulo(programa.Comprimento, larg_alma, x0 + origem.X, y0 + origem.Y - larg_alma, layer, cor);
            var txt = DLM.desenho.Dxf.Texto(new P3d(x0 + origem.X + programa.CentroX, y0 + larg_mesa + origem.Y), (texto != "" ? texto + " - " : "") + programa.Marca, escala * 5, Nome_banzos);

            var furosDXF = new List<netDxf.Entities.EntityObject>();

            foreach (var grp in programa.Grupos)
            {
                var corfr = cor = netDxf.AciColor.ByLayer;

                switch (grp.Tipo)
                {
                    case MDJ_Tipo_Grupo_Furo.Emenda_Inicial:
                        corfr = netDxf.AciColor.Green;
                        break;
                    case MDJ_Tipo_Grupo_Furo.Emenda_Final:
                        corfr = netDxf.AciColor.Green;
                        break;
                    case MDJ_Tipo_Grupo_Furo.No_Padrao:
                    case MDJ_Tipo_Grupo_Furo.No_Var_Ambos:
                    case MDJ_Tipo_Grupo_Furo.No_Var_D:
                    case MDJ_Tipo_Grupo_Furo.No_Var_E:
                        corfr = netDxf.AciColor.Red;
                        break;
                    case MDJ_Tipo_Grupo_Furo.Cadeirinha_Inicial:
                        corfr = netDxf.AciColor.Magenta;
                        break;
                    case MDJ_Tipo_Grupo_Furo.Cadeirinha_Final:
                        corfr = netDxf.AciColor.Magenta;
                        break;
                    case MDJ_Tipo_Grupo_Furo.Furo_Ponta_Inicial_Inferior:
                        corfr = netDxf.AciColor.Yellow;
                        break;
                    case MDJ_Tipo_Grupo_Furo.Furo_Ponta_Final_Inferior:
                        corfr = netDxf.AciColor.Yellow;
                        break;
                    case MDJ_Tipo_Grupo_Furo.Desconhecido:
                        corfr = netDxf.AciColor.LightGray;
                        break;
                    case MDJ_Tipo_Grupo_Furo.Marca:
                        break;
                    case MDJ_Tipo_Grupo_Furo.Indefinido:
                    case MDJ_Tipo_Grupo_Furo.No_Invalido:
                        corfr = netDxf.AciColor.DarkGray;
                        break;
                }
                foreach (var s in grp.GetFuros())
                {
                    var frDXF = DLM.desenho.Dxf.Furo(new P3d(origem.X + s.X + x0, origem.Y + s.Y), 14, 0, 0, layer, corfr);
                    furosDXF.AddRange(frDXF);
                }
                retorno.Entities.Add(DLM.desenho.Dxf.Texto(new P3d(grp.X0, origem.Y + 120), grp.Tipo.ToString(), escala * 5, cotas, null, 90, corfr, netDxf.Entities.TextAlignment.MiddleLeft));
            }





            foreach (var furo in programa.GetFurosAbaBanzoInferior(true).FindAll(x => x.X > 0))
            {
                var frDXF = DLM.desenho.Dxf.Furo(new P3d(origem.X + furo.X, origem.Y - furo.Y)/*(larg_alma / 2)*/, 14, 0, 0, layer, cor);
                retorno.Entities.Add(DLM.desenho.Dxf.Texto(new P3d(origem.X, origem.Y - (escala * 5)), origem.X.ToString(), escala * 5, cotas, null, 90, netDxf.AciColor.FromTrueColor(0)));
                furosDXF.AddRange(frDXF);
            }

            foreach (var furo in programa.GetFurosBordaBanzoInferior())
            {
                var frDXF = DLM.desenho.Dxf.Furo(new P3d(origem.X + furo.X, origem.Y - furo.Y), 14, 0, 0, layer, cor);
                furosDXF.AddRange(frDXF);
            }

            try
            {
                /*verifica se ja existe um bloco com mesmo Nome e cria o bloco*/
                string Nome = programa.Marca;
                int c = 0;
                if (Nome == "")
                {
                    Nome = programa.ToString();
                }

                bool continuar = true;
                while (continuar)
                {
                    var s = retorno.Blocks.ToList().Find(x => x.Name.ToUpper().Replace(" ", "") == Nome + (c > 0 ? "_" + c.String(2) : ""));
                    if (s != null)
                    {
                        c++;
                    }
                    else
                    {
                        continuar = false;
                    }
                }

                if (criar_bloco)
                {
                    var bloco = new netDxf.Blocks.Block(Nome + (c > 0 ? "_" + c.String(2) : ""));


                    bloco.Entities.Add(txt);
                    bloco.Entities.Add(rect);
                    bloco.Entities.Add(rect_alma);

                    foreach (var s in acumuladas_furos)
                    {
                        retorno.Entities.Add(s);
                    }
                    foreach (var s in cotas2)
                    {
                        retorno.Entities.Add(s);
                    }
                    foreach (var s in acumuladas_grupos)
                    {
                        retorno.Entities.Add(s);
                    }
                    foreach (var f in furosDXF)
                    {
                        bloco.Entities.Add(f);
                    }
                    retorno.Blocks.Add(bloco);

                    netDxf.Entities.Insert bb = new netDxf.Entities.Insert(bloco);
                    bb.Position = new netDxf.Vector3();
                    bb.Color = cor;

                    retorno.Entities.Add(bb);
                }
                else
                {
                    retorno.Entities.Add(txt);
                    retorno.Entities.Add(rect);
                    retorno.Entities.Add(rect_alma);

                    foreach (var s in acumuladas_furos)
                    {
                        retorno.Entities.Add(s);
                    }
                    foreach (var s in cotas2)
                    {
                        retorno.Entities.Add(s);
                    }
                    foreach (var s in acumuladas_grupos)
                    {
                        retorno.Entities.Add(s);
                    }
                    foreach (var f in furosDXF)
                    {
                        retorno.Entities.Add(f);
                    }
                }



            }
            catch (Exception)
            {

            }



            return retorno;
        }

        public static List<MDJ_Furo> EspelharFuros(this MDJ_Programa programa)
        {
            return programa.Grupos.SelectMany(y => y.GetFuros().Select(x => x.Espelhar(programa.Comprimento)).ToList()).ToList();
        }
        public static List<MDJ_Furo> FurosRebatidosNaoSimetricos(this MDJ_Programa programa)
        {
            var rebatidos = programa.EspelharFuros();
            var furos = programa.GetFuros();
            return rebatidos.FindAll(x => !x.SemY).FindAll(x => furos.Find(y => y.GetLinha() == x.GetLinha()) == null);
        }

        public static List<MDJ_Furo> GetFurosBatendo(this MDJ_Programa programa, double distmin)
        {
            var retorno = new List<MDJ_Furo>();
            var frs = programa.GetFuros();
            frs = frs.GroupBy(x => x.GetLinha()).Select(x => x.First()).ToList();
            frs = frs.OrderBy(x => x.X).ToList();
            for (int i = 0; i < frs.Count; i++)
            {

                for (int a = 0; a < frs.Count; a++)
                {
                    var dist = Math.Abs(frs[i].Distancia(frs[a]));
                    if (dist < distmin && dist > 1)
                    {
                        if (frs[i] != frs[a])
                        {
                            retorno.Add(frs[a]);
                        }
                    }
                }
            }
            return retorno.GroupBy(x => x.GetLinha()).Select(x => x.First()).ToList();
        }

        public static List<MDJ_Furo> GetDiferencas(this MDJ_Programa atual, MDJ_Programa programa, bool verificar_rebatido = false)
        {
            var frs = new List<MDJ_Furo>();
            var s1 = atual.GetFuros();
            var s2 = new List<MDJ_Furo>();
            s2.AddRange(programa.GetFuros());

            if (verificar_rebatido)
            {
                s2 = programa.EspelharFuros();
            }

            foreach (var fr in s1)
            {
                var igual = s2.Find(x => x.GetLinha() == fr.GetLinha());
                if (igual == null)
                {
                    igual = s2.Find(x => x.GetLinha() == fr.Clonar(1, true).GetLinha());
                }
                if (igual == null)
                {
                    igual = s2.Find(x => x.GetLinha() == fr.Clonar(-1, true).GetLinha());
                }
                if (igual == null)
                {
                    igual = s2.Find(x => x.Clonar(0, true).GetLinha() == fr.Clonar(1, true).GetLinha());
                }
                if (igual == null)
                {
                    igual = s2.Find(x => x.Clonar(0, true).GetLinha() == fr.Clonar(-1, true).GetLinha());
                }

                if (igual == null)
                {
                    frs.Add(fr);
                }
            }

            foreach (var fr in s2)
            {
                var igual = s1.Find(x => x.GetLinha() == fr.GetLinha());
                if (igual == null)
                {
                    igual = s1.Find(x => x.GetLinha() == fr.Clonar(1, true).GetLinha());
                }
                if (igual == null)
                {
                    igual = s1.Find(x => x.GetLinha() == fr.Clonar(-1, true).GetLinha());
                }
                if (igual == null)
                {
                    igual = s1.Find(x => x.Clonar(0, true).GetLinha() == fr.Clonar(1, true).GetLinha());
                }
                if (igual == null)
                {
                    igual = s1.Find(x => x.Clonar(0, true).GetLinha() == fr.Clonar(-1, true).GetLinha());
                }

                if (igual == null)
                {
                    frs.Add(fr);
                }
            }

            frs = frs.FindAll(x => !x.SemY).ToList();
            return frs;
        }


    }

}
