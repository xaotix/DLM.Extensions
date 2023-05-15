using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Conexoes;
using DLM.vars;
using DLM.db;

namespace DLM.desenho
{
    public static class ExtensoesNetDxf
    {
        public static void Show(this netDxf.DxfDocument dxf)
        {
            var mm = new Conexoes.Janelas.VisualizarDXF();
            mm.DXF_view.Abrir(dxf);
            mm.Show();
        }
        public static void AddPerfil_Frontal(this DxfDocument document, netDxf.Tables.Layer Layer, netDxf.Tables.Layer Layer_Cantoneiras, Conexoes.Macros.Escada.Escada_Parte escada_pt)
        {
            //desenho do perfil
            document.AddRetangulo(Layer, escada_pt.Origem_Final.Acumulada_X, escada_pt.Origem_Final.Acumulada_Y, escada_pt.Largura, escada_pt.Comprimento_Final);
            document.AddRetangulo(Layer, escada_pt.Origem_Final.Acumulada_X + DLM.vars.Global.Escada.Degrau_Distancia_Horizontal + DLM.vars.Global.Escada.Perfil_U_Largura, escada_pt.Origem_Final.Acumulada_Y, escada_pt.Largura, escada_pt.Comprimento_Final);
            //linhas pontilhadas espessura perfil
            document.AddLine(Layer, escada_pt.Origem_Final.Acumulada_X + DLM.vars.Global.Escada.Perfil_U_Largura - DLM.vars.Global.Escada.Perfil_U_Espessura, escada_pt.Origem_Final.Acumulada_Y, escada_pt.Origem_Final.Acumulada_X + DLM.vars.Global.Escada.Perfil_U_Largura - DLM.vars.Global.Escada.Perfil_U_Espessura, escada_pt.Origem_Final.Acumulada_Y + escada_pt.Comprimento_Final, netDxf.Tables.Linetype.Dashed);
            document.AddLine(Layer, escada_pt.Origem_Final.Acumulada_X + DLM.vars.Global.Escada.Degrau_Distancia_Horizontal + DLM.vars.Global.Escada.Perfil_U_Largura + DLM.vars.Global.Escada.Perfil_U_Espessura, escada_pt.Origem_Final.Acumulada_Y, escada_pt.Origem_Final.Acumulada_X + DLM.vars.Global.Escada.Degrau_Distancia_Horizontal + DLM.vars.Global.Escada.Perfil_U_Largura + DLM.vars.Global.Escada.Perfil_U_Espessura, escada_pt.Origem_Final.Acumulada_Y + escada_pt.Comprimento_Final, netDxf.Tables.Linetype.Dashed);
            double y = escada_pt.Origem_Final.Acumulada_Y + (DLM.vars.Global.Escada.Cantoneira_Largura / 2);
            double y0 = escada_pt.Origem_Final.Acumulada_Y;
            double x1 = escada_pt.Origem_Final.Acumulada_X + DLM.vars.Global.Escada.Perfil_U_Largura;
            double x2 = escada_pt.Origem_Final.Acumulada_X + DLM.vars.Global.Escada.Degrau_Distancia_Horizontal + DLM.vars.Global.Escada.Perfil_U_Largura;

            //cantoneira da base
            if (escada_pt.Tipo == Tipo.Inicial | escada_pt.Tipo == Tipo.Simples)
            {

                //cantoneiras
                //esquerdo
                document.AddPerfil_L(Layer_Cantoneiras, x1 - DLM.vars.Global.Escada.Perfil_U_Espessura, y, Orientacao.SEI);
                //direito
                document.AddPerfil_L(Layer_Cantoneiras, x2 + DLM.vars.Global.Escada.Perfil_U_Espessura, y, Orientacao.SD);

                //furos base
                document.AddFuro(Layer_Cantoneiras, x1 - DLM.vars.Global.Escada.Cantoneira_Offset_Furo - DLM.vars.Global.Escada.Perfil_U_Espessura, y0, DLM.vars.Global.Escada.Cantoneira_Furo_Diametro, Desenho_Furo.Corte, true, Sentido.Vertical);
                document.AddFuro(Layer_Cantoneiras, x2 + DLM.vars.Global.Escada.Cantoneira_Offset_Furo + DLM.vars.Global.Escada.Perfil_U_Espessura, y0, DLM.vars.Global.Escada.Cantoneira_Furo_Diametro, Desenho_Furo.Corte, true, Sentido.Vertical);

            }

            foreach (var t in escada_pt.Furos_ESQ)
            {
                if (t.Face == Furo_Face.Aba1 | t.Face == Furo_Face.Aba2)
                {
                    document.AddFuro(Layer, x1 - (DLM.vars.Global.Escada.Perfil_U_Largura / 2), escada_pt.Origem_Final.Origem_Y + t.Origem.X, DLM.vars.Global.Escada.Cantoneira_Furo_Diametro, Desenho_Furo.Vista, true, Sentido.Horizontal);
                }
                else
                {
                    document.AddFuro(Layer, x1, escada_pt.Origem_Final.Origem_Y + t.Origem.X, DLM.vars.Global.Escada.Cantoneira_Furo_Diametro, Desenho_Furo.Corte, true, Sentido.Horizontal);

                }
            }
            foreach (var t in escada_pt.Furos_DIR)
            {
                if (t.Face == Furo_Face.Aba1 | t.Face == Furo_Face.Aba2)
                {
                    document.AddFuro(Layer, x2 + (DLM.vars.Global.Escada.Perfil_U_Largura / 2), escada_pt.Origem_Final.Origem_Y + t.Origem.X, DLM.vars.Global.Escada.Cantoneira_Furo_Diametro, Desenho_Furo.Vista, true, Sentido.Horizontal);
                }
                else
                {
                    document.AddFuro(Layer, x2, escada_pt.Origem_Final.Origem_Y + t.Origem.X, DLM.vars.Global.Escada.Cantoneira_Furo_Diametro, Desenho_Furo.Corte, true, Sentido.Horizontal);
                }

            }
        }
        public static List<netDxf.Entities.EntityObject> AddPerfil_Lateral(this DxfDocument document, netDxf.Tables.Layer Layer, Conexoes.Macros.Escada.Escada_Parte escada_pt, double offset_x)
        {
            var retorno = new List<netDxf.Entities.EntityObject>();

            retorno.Add(document.AddRetangulo(Layer, offset_x, escada_pt.Origem_Final.Acumulada_Y, DLM.vars.Global.Escada.Perfil_U_Secao, escada_pt.Comprimento_Final));
            double y1 = escada_pt.Origem_Final.Acumulada_Y;
            double y2 = escada_pt.Origem_Final.Acumulada_Y + escada_pt.Comprimento_Final;
            retorno.Add(document.AddLine(Layer, offset_x + DLM.vars.Global.Escada.Perfil_U_Espessura, y1, offset_x + DLM.vars.Global.Escada.Perfil_U_Espessura, y2));
            retorno.Add(document.AddLine(Layer, offset_x + DLM.vars.Global.Escada.Perfil_U_Secao - DLM.vars.Global.Escada.Perfil_U_Espessura, y1, offset_x + +Global.Escada.Perfil_U_Secao - DLM.vars.Global.Escada.Perfil_U_Espessura, y2));

            foreach (var furo in escada_pt.Furos_DIR)
            {
                if (furo.Face == Furo_Face.Alma)
                {
                    retorno.AddRange(document.AddFuro(Layer, offset_x + furo.Origem.Y, escada_pt.Origem_Final.Acumulada_Y + furo.Origem.X, furo.Diametro, Desenho_Furo.Vista));
                }
                else if (furo.Face == Furo_Face.Aba1)
                {
                    retorno.AddRange(document.AddFuro(Layer, offset_x, escada_pt.Origem_Final.Acumulada_Y + furo.Origem.X, furo.Diametro, Desenho_Furo.Corte));

                }
                else if (furo.Face == Furo_Face.Aba2)
                {
                    retorno.AddRange(document.AddFuro(Layer, offset_x + DLM.vars.Global.Escada.Perfil_U_Secao, escada_pt.Origem_Final.Acumulada_Y + furo.Origem.X, furo.Diametro, Desenho_Furo.Corte));

                }
            }

            foreach (var furo in escada_pt.Furos_ESQ)
            {
                if (furo.Face == Furo_Face.Alma)
                {
                    retorno.AddRange(document.AddFuro(Layer, offset_x + furo.Origem.Y, escada_pt.Origem_Final.Acumulada_Y + furo.Origem.X, furo.Diametro, Desenho_Furo.Vista));
                }
                else if (furo.Face == Furo_Face.Aba2)
                {
                    retorno.AddRange(document.AddFuro(Layer, offset_x, escada_pt.Origem_Final.Acumulada_Y + furo.Origem.X, furo.Diametro, Desenho_Furo.Corte));

                }
                else if (furo.Face == Furo_Face.Aba1)
                {
                    retorno.AddRange(document.AddFuro(Layer, offset_x + DLM.vars.Global.Escada.Perfil_U_Secao, escada_pt.Origem_Final.Acumulada_Y + furo.Origem.X, furo.Diametro, Desenho_Furo.Corte));
                }
            }

            return retorno;
        }
        public static void AddPerfil_U(this DxfDocument document, netDxf.Tables.Layer Layer, double X, double Y, Orientacao Lado, double Secao, double Espessura, double Largura = 0, double Comprimento = 0)
        {
            double m_larg = (Largura / 2);
            double m_alt = (Secao / 2);
            //double esp = DLM.vars.Vars.Escada.Perfil_U_Espessura;
            //double alt = DLM.vars.Vars.Escada.Perfil_U_Secao;
            //double larg = DLM.vars.Vars.Escada.Perfil_U_Largura;
            if (Lado == Orientacao.SD)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(X + m_larg, Y + m_alt, 0),
                            new Vector3(X - m_larg, Y + m_alt, 0),
                            new Vector3(X - m_larg, Y - m_alt, 0),
                            new Vector3(X + m_larg, Y - m_alt, 0),
                            new Vector3(X + m_larg, Y - m_alt + Espessura, 0),
                            new Vector3(X - m_larg+ Espessura, Y - m_alt + Espessura, 0),
                            new Vector3(X - m_larg+ Espessura, Y + m_alt - Espessura, 0),
                            new Vector3(X +  m_larg, Y + m_alt - Espessura, 0),
                            new Vector3(X +  m_larg, Y + m_alt, 0)
                        };
                document.AddEntity(new netDxf.Entities.Polyline(Vetores) { Layer = Layer, Color = AciColor.ByLayer });
            }
            else if (Lado == Orientacao.SE)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(X - m_larg, Y + m_alt, 0),
                            new Vector3(X + m_larg, Y + m_alt, 0),
                            new Vector3(X + m_larg, Y - m_alt, 0),
                            new Vector3(X - m_larg, Y - m_alt, 0),
                            new Vector3(X - m_larg, Y - m_alt + Espessura, 0),
                            new Vector3(X + m_larg-Espessura, Y - m_alt + Espessura, 0),
                            new Vector3(X + m_larg-Espessura, Y + m_alt - Espessura, 0),
                            new Vector3(X - m_larg, Y + m_alt - Espessura, 0),
                            new Vector3(X - m_larg, Y + m_alt, 0)
                        };
                document.AddEntity(new netDxf.Entities.Polyline(Vetores) { Layer = Layer, Color = AciColor.ByLayer });
            }
            else if (Lado == Orientacao.HD)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(X, Y + m_alt, 0),
                            new Vector3(X+Comprimento, Y + m_alt, 0),
                            new Vector3(X+Comprimento, Y - m_alt, 0),
                            new Vector3(X, Y - m_alt, 0),
                            new Vector3(X, Y + m_alt, 0)
                        };
                document.AddEntity(new netDxf.Entities.Polyline(Vetores) { Layer = Layer, Color = AciColor.ByLayer });
                document.AddLine(Layer, X, Y + m_alt - Espessura, X + Comprimento, Y + m_alt - Espessura);
                document.AddLine(Layer, X, Y - m_alt + Espessura, X + Comprimento, Y - m_alt + Espessura);
            }
            else if (Lado == Orientacao.HE)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(X, Y + m_alt, 0),
                            new Vector3(X-Comprimento, Y + m_alt, 0),
                            new Vector3(X-Comprimento, Y - m_alt, 0),
                            new Vector3(X, Y - m_alt, 0),
                            new Vector3(X, Y + m_alt, 0)
                        };
                document.AddEntity(new netDxf.Entities.Polyline(Vetores) { Layer = Layer, Color = AciColor.ByLayer });
                document.AddLine(Layer, X, Y + m_alt - Espessura, X - Comprimento, Y + m_alt - Espessura);
                document.AddLine(Layer, X, Y - m_alt + Espessura, X - Comprimento, Y - m_alt + Espessura);
            }
            else if (Lado == Orientacao.VD)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(X, Y, 0),
                            new Vector3(X+Secao, Y, 0),
                            new Vector3(X+Secao, Y - Comprimento, 0),
                            new Vector3(X, Y - Comprimento, 0),
                            new Vector3(X, Y, 0)
                        };
                document.AddEntity(new netDxf.Entities.Polyline(Vetores) { Layer = Layer, Color = AciColor.ByLayer });
                document.AddLine(Layer, X + Secao - Espessura, Y, X + Secao - Espessura, Y - Comprimento);
                document.AddLine(Layer, X + Espessura, Y, X + Espessura, Y - Comprimento);

            }
            else if (Lado == Orientacao.HDI && Comprimento > 0)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(X, Y + m_alt, 0),
                            new Vector3(X-Comprimento, Y + m_alt, 0),
                            new Vector3(X-Comprimento, Y - m_alt, 0),
                            new Vector3(X, Y - m_alt, 0),
                            new Vector3(X, Y + m_alt, 0)
                        };
                document.AddEntity(new netDxf.Entities.Polyline(Vetores) { Layer = Layer, Color = AciColor.ByLayer });
                document.AddLine(Layer, X, Y + m_alt - Espessura, X - Comprimento, Y + m_alt - Espessura);
                document.AddLine(Layer, X, Y - m_alt + Espessura, X - Comprimento, Y - m_alt + Espessura);
            }

        }
        public static void AddPerfil_L(this DxfDocument document, netDxf.Tables.Layer Layer, double X, double Y, Orientacao Lado, double Comprimento = 0)
        {

            double m_alt = (DLM.vars.Global.Escada.Cantoneira_Largura / 2);
            double esp = DLM.vars.Global.Escada.Cantoneira_Espessura;
            double alt = DLM.vars.Global.Escada.Cantoneira_Largura;
            double larg = DLM.vars.Global.Escada.Cantoneira_Largura;
            double m_comp = Comprimento / 2;
            if (Lado == Orientacao.SD)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(X, Y + m_alt, 0),
                             new Vector3(X, Y - m_alt, 0),
                             new Vector3(X+larg, Y - m_alt, 0),
                             new Vector3(X+larg, Y - m_alt+ esp, 0),
                             new Vector3(X+ esp, Y - m_alt+ esp, 0),
                             new Vector3(X+ esp, Y + m_alt, 0),
                             new Vector3(X, Y + m_alt, 0)

                        };
                document.AddEntity(new netDxf.Entities.Polyline(Vetores) { Layer = Layer, Color = AciColor.ByLayer });
            }
            else if (Lado == Orientacao.SDI)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(X, Y + m_alt, 0),
                             new Vector3(X, Y - m_alt, 0),
                             new Vector3(X+esp, Y - m_alt, 0),
                             new Vector3(X+esp, Y + m_alt - esp, 0),
                             new Vector3(X+larg, Y + m_alt - esp, 0),
                             new Vector3(X+larg, Y + m_alt, 0),
                             new Vector3(X, Y + m_alt, 0)


                        };
                document.AddEntity(new netDxf.Entities.Polyline(Vetores) { Layer = Layer, Color = AciColor.ByLayer });
            }
            else if (Lado == Orientacao.SE)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(X, Y + m_alt, 0),
                            new Vector3(X -larg, Y + m_alt, 0),
                            new Vector3(X -larg, Y + m_alt-esp, 0),
                            new Vector3(X -esp, Y + m_alt-esp, 0),
                            new Vector3(X -esp, Y - m_alt, 0),
                            new Vector3(X, Y - m_alt, 0),
                            new Vector3(X, Y + m_alt, 0)
                        };
                document.AddEntity(new netDxf.Entities.Polyline(Vetores) { Layer = Layer, Color = AciColor.ByLayer });
            }
            else if (Lado == Orientacao.SEI)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(X, Y + m_alt, 0),
                            new Vector3(X -esp, Y + m_alt, 0),
                            new Vector3(X -esp, Y - m_alt+esp, 0),
                            new Vector3(X -larg, Y - m_alt+esp, 0),
                            new Vector3(X -larg, Y - m_alt, 0),
                            new Vector3(X, Y - m_alt, 0),
                            new Vector3(X, Y + m_alt, 0)
                        };
                document.AddEntity(new netDxf.Entities.Polyline(Vetores) { Layer = Layer, Color = AciColor.ByLayer });
            }
            else if (Lado == Orientacao.HD | Lado == Orientacao.HE)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(X + m_comp, Y, 0),
                            new Vector3(X + m_comp, Y + larg, 0),
                            new Vector3(X - m_comp, Y + larg, 0),
                            new Vector3(X - m_comp, Y, 0),
                            new Vector3(X + m_comp, Y, 0),

                        };
                document.AddEntity(new netDxf.Entities.Polyline(Vetores) { Layer = Layer });
                document.AddLine(Layer, X - m_comp, Y + esp, X + m_comp, Y + esp, netDxf.Tables.Linetype.Dashed);
            }
            else if (Lado == Orientacao.HDI | Lado == Orientacao.HEI)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(X + m_comp, Y, 0),
                            new Vector3(X + m_comp, Y - larg, 0),
                            new Vector3(X - m_comp, Y - larg, 0),
                            new Vector3(X - m_comp, Y, 0),
                            new Vector3(X + m_comp, Y, 0),

                        };
                document.AddEntity(new netDxf.Entities.Polyline(Vetores) { Layer = Layer });
                document.AddLine(Layer, X - m_comp, Y - esp, X + m_comp, Y - esp, netDxf.Tables.Linetype.Dashed);

            }
            else if (Lado == Orientacao.HDV)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(X, Y, 0),
                            new Vector3(X + larg, Y, 0),
                            new Vector3(X + larg, Y-Comprimento, 0),
                            new Vector3(X, Y-Comprimento, 0),
                            new Vector3(X, Y, 0),
                        };
                document.AddEntity(new netDxf.Entities.Polyline(Vetores) { Layer = Layer });
                document.AddLine(Layer, X + esp, Y, X + esp, Y - Comprimento, netDxf.Tables.Linetype.Dashed);

            }
            else if (Lado == Orientacao.HEV)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(X, Y, 0),
                            new Vector3(X - larg, Y, 0),
                            new Vector3(X - larg, Y-Comprimento, 0),
                            new Vector3(X, Y-Comprimento, 0),
                            new Vector3(X, Y, 0),
                        };
                document.AddEntity(new netDxf.Entities.Polyline(Vetores) { Layer = Layer });
                document.AddLine(Layer, X - esp, Y, X - esp, Y - Comprimento, netDxf.Tables.Linetype.Dashed);

            }


        }
        public static netDxf.Entities.Leader AddLeader(this DxfDocument document, netDxf.Tables.Layer Layer, List<Vector2> Vetores, string Texto, double Tamanho, netDxf.Tables.DimensionStyle estilo)
        {
            var leader = new netDxf.Entities.Leader(Texto, Vetores, estilo);
            leader.Color = AciColor.Cyan;
            document.AddEntity(leader);
            return leader;
        }
        public static netDxf.Entities.AlignedDimension AddCota(this DxfDocument document, netDxf.Tables.Layer Layer, double x0, double y0, double x1, double y1, double offset, netDxf.Tables.DimensionStyle estilo)
        {
            offset = Math.Abs(offset);

            if (Math.Abs(new P3d(x0, y0).Distancia(new P3d(x1, y1))) > 0 && offset > 0)
            {
                var cota = new netDxf.Entities.AlignedDimension(new Vector2(x0, y0), new Vector2(x1, y1), offset);
                cota.Layer = Layer;
                cota.Style = estilo;
                document.AddEntity(cota);

                return cota;
            }
            return null;
        }
        public static netDxf.Entities.Polyline AddRetangulo(this DxfDocument document, netDxf.Tables.Layer Layer_Degraus, double X, double Y, double Comprimento, double Largura)
        {
            var Vetores = new List<Vector3>();
            Vetores = new List<Vector3> {
                            new Vector3(X, Y, 0),
                            new Vector3(X + Comprimento, Y, 0),
                            new Vector3(X + Comprimento, Y + Largura, 0),
                            new Vector3(X, Y+ Largura, 0),
                            new Vector3(X, Y, 0)

                        };
            var pol = new netDxf.Entities.Polyline(Vetores) { Layer = Layer_Degraus, Color = AciColor.ByLayer };
            document.AddEntity(pol);

            return pol;
        }
        public static Line AddLine(this DxfDocument document, netDxf.Tables.Layer Layer, double x1, double y1, double x2, double y2, netDxf.Tables.Linetype Tipo = null)
        {
            if (Tipo == null)
            {
                Tipo = netDxf.Tables.Linetype.ByLayer;
            }
            var l = new netDxf.Entities.Line(new Vector2(x1, y1), new Vector2(x2, y2)) { Layer = Layer, Color = AciColor.ByLayer, Linetype = Tipo };
            document.AddEntity(l);
            return l;
        }
        public static List<Line> AddXis(this DxfDocument document, netDxf.Tables.Layer Layer, double X, double Y, double Diametro)
        {
            var linhas = new List<Line>();
            double o = (Diametro / 4);

            linhas.Add(document.AddLine(Layer, X - o, Y - o, X + o, Y + o));
            linhas.Add(document.AddLine(Layer, X + o, Y - o, X - o, Y + o));

            return linhas;
        }
        public static List<netDxf.Entities.EntityObject> AddFuro(this DxfDocument document, netDxf.Tables.Layer Layer, double X, double Y, double Diametro, Desenho_Furo Tipo, bool Linhas_De_Centro = true, Sentido Sentido = Sentido.Horizontal)
        {
            var retorno = new List<netDxf.Entities.EntityObject>();
            if (Tipo == Desenho_Furo.Vista)
            {
                document.AddEntity(new netDxf.Entities.Circle(new Vector2(X, Y), Diametro / 2) { Layer = Layer, Color = AciColor.ByLayer });
                if (Linhas_De_Centro)
                {
                    double o = (Diametro / 2) + (Diametro / 3);

                    retorno.Add(document.AddLine(Layer, X - o, Y, X + o, Y, netDxf.Tables.Linetype.Dashed));
                    retorno.Add(document.AddLine(Layer, X, Y - o, X, Y + o, netDxf.Tables.Linetype.Dashed));
                }
            }
            else if (Tipo == Desenho_Furo.Corte)
            {
                double o = Diametro * 1.5;
                if (Sentido == Sentido.Horizontal)
                {
                    retorno.Add(document.AddLine(Layer, X - o, Y, X + o, Y));
                    retorno.AddRange(document.AddXis(Layer, X - o, Y, Diametro));
                    retorno.AddRange(document.AddXis(Layer, X + o, Y, Diametro));
                }
                else if (Sentido == Sentido.Vertical)
                {
                    retorno.Add(document.AddLine(Layer, X, Y - o, X, Y + o));
                    retorno.AddRange(document.AddXis(Layer, X, Y - o, Diametro));
                    retorno.AddRange(document.AddXis(Layer, X, Y + o, Diametro));
                }
            }
            return retorno;
        }
        public static void SetDimensionStyle(this DxfDocument Origem, netDxf.Tables.DimensionStyle Estilo)
        {
            foreach (var t in Origem.Dimensions)
            {
                t.Style = Estilo;
            }
            foreach (var t in Origem.Texts)
            {
                t.Style = Estilo.TextStyle;
                t.Height = Estilo.TextHeight * Estilo.DimScaleOverall;
                t.Color = Estilo.TextColor;
            }
            foreach (var t in Origem.MTexts)
            {
                t.Style = Estilo.TextStyle;
                t.Height = Estilo.TextHeight * Estilo.DimScaleOverall;
                t.Color = Estilo.TextColor;
            }
            foreach (var t in Origem.Leaders)
            {
                t.Style = Estilo;

                if (t.Annotation is MText)
                {
                    var s = t.Annotation as MText;

                    s.Style = Estilo.TextStyle;
                    s.Height = Estilo.TextHeight * Estilo.DimScaleOverall;
                    s.Color = Estilo.TextColor;
                }
                t.Update(true);

            }

        }
        public static netDxf.Entities.Text AddText(this DxfDocument document, netDxf.Tables.Layer Layer, double X, double Y, string Texto, double Tamanho, netDxf.Tables.TextStyle estilo = null)
        {
            var text = new netDxf.Entities.Text(Texto, new Vector2(X, Y), Tamanho);
            text.Color = AciColor.Cyan;
            if (estilo != null)
            {
                text.Style = estilo;
            }
            document.AddEntity(text);
            return text;
        }
        public static MText AddMText(this DxfDocument document, string mensagem, P3d posicao = null)
        {
            if (posicao == null)
            {
                posicao = new P3d();
            }
            return document.AddMText(new List<string> { mensagem }, posicao, true);
        }
        public static MText AddMText(this DxfDocument document, List<string> Linhas, P3d posicao = null, bool adicionar = true)
        {
            if (posicao == null)
            {
                posicao = new P3d();
            }
            var retorno = new MText();
            retorno.Height = document.DrawingVariables.TextSize;
            netDxf.Tables.TextStyle estilo;
            document.TextStyles.TryGetValue(document.DrawingVariables.TextStyle, out estilo);
            retorno.Style = estilo;
            retorno.Value = string.Join(@"\P", Linhas);
            retorno.Position = new Vector3(posicao.X, posicao.Y, 0);
            if (adicionar)
            {
                document.AddEntity(retorno);
            }
            return retorno;
        }

        public static List<netDxf.Tables.DimensionStyle> ClonarDimensionStyles(this DxfDocument Origem, DxfDocument Destino)
        {
            var retorno = new List<netDxf.Tables.DimensionStyle>();
            var entities = Origem.Layouts.GetReferences(netDxf.Objects.Layout.ModelSpaceName);


            foreach (var dim in Origem.DimensionStyles)
            {

                if (dim is netDxf.Tables.DimensionStyle)
                {
                    var t1 = (dim as netDxf.Tables.DimensionStyle).Clone();
                    Destino.DimensionStyles.Add(t1 as netDxf.Tables.DimensionStyle);
                    retorno.Add(t1 as netDxf.Tables.DimensionStyle);
                }

            }
            return retorno;
        }
        public static void SetAtributo(this Insert Bloco, string Atributo, string Valor)
        {
            var t = Bloco.Attributes.ToList().Find(x => x.Tag.ToUpper() == Atributo.ToUpper());
            if (t != null)
            {
                t.Value = Valor;
            }
        }
        public static EntityObject AddBlock(this DxfDocument Destino, string Arquivo, P3d Origem, double Escala = 1, double Angulo = 0)
        {
            try
            {
                var block = Destino.Blocks.ToList().Find(x => x.Name.ToUpper() == Arquivo.getNome().ToUpper());

                if (block == null)
                {
                    if (Arquivo.Existe())
                    {
                        block = new netDxf.Blocks.Block(Arquivo.getNome(), DxfDocument.Load(Arquivo).Clonar());
                    }
                }
                if (block != null)
                {

                    Insert p = new Insert(block);
                    p.Scale = new Vector3(Escala);
                    p.Position = new Vector3(Origem.X, Origem.Y, 0);
                    p.Rotation = Angulo;
                    Destino.AddEntity(p);
                    return p;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {

                return null;
            }
        }
        public static DxfDocument GetDxf(this string Arquivo, bool forcar_novo = false)
        {
            DxfDocument ndxf = null;

            if (!Arquivo.Existe() && !forcar_novo)
            {
                ndxf = new DxfDocument();
                ndxf.AddMText("Arquivo não existe.\n" + Arquivo);
            }
            else if (!Arquivo.Existe() && forcar_novo)
            {
                ndxf = new DxfDocument();
                return ndxf;
            }
            else
            {
                try
                {
                    ndxf = DxfDocument.Load(Arquivo);
                    if (ndxf != null)
                    {
                        return ndxf;
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        var tmbs = new netDxfMBS.DxfDocument();
                        tmbs.Load(Arquivo);
                        if (tmbs != null)
                        {
                            ndxf = new DxfDocument();
                            var entt = tmbs.Clonar();
                            foreach (var objeto in entt)
                            {
                                ndxf.AddEntity(objeto);
                            }
                            return ndxf;
                        }
                    }
                    catch (Exception ex2)
                    {
                        ndxf = new DxfDocument();
                        ndxf.AddMText(ex2.Message);

                    }
                }
            }

            return ndxf;
        }
        public static netDxf.Tables.Layer GetLayer(this DxfDocument doc, string Nome, netDxf.AciColor Cor, netDxf.Tables.Linetype Linha)
        {
            var ss = doc.Layers.ToList().Find(x => x.Name.ToUpper().Replace(" ", "") == Nome.ToUpper().Replace(" ", ""));
            if (ss != null)
            {
                return ss;
            }

            var retorno = new netDxf.Tables.Layer(Nome.Replace(" ", "").ToUpper());
            retorno.Color = Cor;
            retorno.Linetype = Linha;
            return retorno;
        }
        public static double GetAltura(this Insert block)
        {
            var p = block.Block.Entities.ToList().FindAll(x => x is netDxf.Entities.Line).Select(x => x as netDxf.Entities.Line);
            var coords = p.Select(x => x.StartPoint).ToList();
            coords.AddRange(p.Select(x => x.EndPoint));
            if (coords.Count > 0)
            {
                var v1 = coords.Max(x => x.Y) * block.Scale.Y;
                var v2 = coords.Min(x => x.Y) * block.Scale.Y;
                return v1 - v2;
            }
            return 0;
        }
        public static void SetCor(this EntityObject obj, AciColor cor)
        {
            cor = cor.Clone() as AciColor;
            if (obj is Line)
            {
                var s = obj as Line;
                s.Color = cor;
            }
            else if (obj is Insert)
            {
                var s = obj as Insert;
                s.Color = cor;
            }
            else if (obj is Text)
            {
                var s = obj as Text;
                s.Color = cor;
            }
            else if (obj is MText)
            {
                var s = obj as MText;
                s.Color = cor;
            }
            else if (obj is Arc)
            {
                var s = obj as Arc;
                s.Color = cor;
            }
            else if (obj is Circle)
            {
                var s = obj as Circle;
                s.Color = cor;
            }
            else if (obj is Ellipse)
            {
                var s = obj as Ellipse;
                s.Color = cor;
            }
            else if (obj is LwPolyline)
            {
                var s = obj as LwPolyline;
                s.Color = cor;
            }
            else if (obj is Polyline)
            {
                var s = obj as Polyline;
                s.Color = cor;
            }

        }
        public static DxfDocument ClonarDados(this DxfDocument Origem, DxfDocument Destino)
        {
            List<DxfObject> entities = Origem.Layouts.GetReferences(netDxf.Objects.Layout.ModelSpaceName);
            foreach (DxfObject o in entities)
            {
                EntityObject entity = o as EntityObject;
                var t1 = entity.Clone() as EntityObject;

                Destino.AddEntity(t1);
            }
            return Destino;
        }
        public static List<EntityObject> Clonar(this DxfDocument Origem)
        {
            List<DxfObject> entities = Origem.Layouts.GetReferences(netDxf.Objects.Layout.ModelSpaceName);
            List<EntityObject> Destino = new List<EntityObject>();
            foreach (DxfObject o in entities)
            {
                EntityObject entity = o as EntityObject;
                var t1 = entity.Clone() as EntityObject;

                Destino.Add(t1);
            }
            return Destino;
        }
        public static List<EntityObject> Clonar(this netDxfMBS.DxfDocument Origem)
        {
            var Destino = new List<EntityObject>();

            foreach (var t in Origem.Arcs)
            {
                var m = t.Get();
                Destino.Add(m);
            }
            foreach (var t in Origem.Circles)
            {
                var m = t.Get();
                Destino.Add(m);
            }
            foreach (var t in Origem.Ellipses)
            {
                var m = t.Get();
                Destino.Add(m);
            }
            foreach (var t in Origem.Lines)
            {
                var m = t.Get();
                Destino.Add(m);
            }
            foreach (var t in Origem.Points)
            {
                var m = t.Get();
            }
            foreach (var t in Origem.Texts)
            {
                var m = t.Get();
                Destino.Add(m);
            }
            foreach (var t in Origem.Polylines)
            {
                /*VER*/
                //var m = new Polyline(t)
                if (t is netDxfMBS.Entities.Polyline)
                {
                    var s = t as netDxfMBS.Entities.Polyline;
                    var m = s.Get();
                    Destino.Add(m);
                }

            }
            foreach (var a in Origem.Inserts)
            {
                foreach (var s in a.Block.Entities)
                {
                    if (s is netDxfMBS.Entities.Arc)
                    {
                        var t = (s as netDxfMBS.Entities.Arc).Get(new Vector3(a.InsertionPoint.X, a.InsertionPoint.Y, a.InsertionPoint.Z));
                        Destino.Add(t);
                    }
                    else if (s is netDxfMBS.Entities.Circle)
                    {
                        var t = (s as netDxfMBS.Entities.Circle).Get(new Vector3(a.InsertionPoint.X, a.InsertionPoint.Y, a.InsertionPoint.Z));
                        Destino.Add(t);
                    }
                    else if (s is netDxfMBS.Entities.Ellipse)
                    {
                        var t = (s as netDxfMBS.Entities.Ellipse).Get(new Vector3(a.InsertionPoint.X, a.InsertionPoint.Y, a.InsertionPoint.Z));
                        Destino.Add(t);
                    }
                    else if (s is netDxfMBS.Entities.Line)
                    {
                        var t = (s as netDxfMBS.Entities.Line).Get(new Vector3(a.InsertionPoint.X, a.InsertionPoint.Y, a.InsertionPoint.Z));
                        Destino.Add(t);
                    }
                    else if (s is netDxfMBS.Entities.Point)
                    {
                        var t = (s as netDxfMBS.Entities.Point).Get(new Vector3(a.InsertionPoint.X, a.InsertionPoint.Y, a.InsertionPoint.Z));
                        Destino.Add(t);
                    }
                    else if (s is netDxfMBS.Entities.Polyline)
                    {
                        var t = (s as netDxfMBS.Entities.Polyline).Get(new Vector3(a.InsertionPoint.X, a.InsertionPoint.Y, a.InsertionPoint.Z));
                        Destino.Add(t);
                    }
                    else if (s is netDxfMBS.Entities.Text)
                    {
                        var t = (s as netDxfMBS.Entities.Text).Get();
                        Destino.Add(t);
                    }
                    else
                    {

                    }
                }
            }


            return Destino;
        }
        public static MText Get(this Text text)
        {
            MText retorno = new MText();
            retorno.Style = text.Style;
            retorno.Value = text.Value;

            switch (text.Alignment)
            {
                case netDxf.Entities.TextAlignment.TopLeft:
                    retorno.AttachmentPoint = MTextAttachmentPoint.TopLeft; break;
                case netDxf.Entities.TextAlignment.TopCenter:
                    retorno.AttachmentPoint = MTextAttachmentPoint.TopCenter; break;
                case netDxf.Entities.TextAlignment.TopRight:
                    retorno.AttachmentPoint = MTextAttachmentPoint.TopRight; break;
                case netDxf.Entities.TextAlignment.MiddleLeft:
                    retorno.AttachmentPoint = MTextAttachmentPoint.MiddleLeft; break;
                case netDxf.Entities.TextAlignment.MiddleCenter:
                    retorno.AttachmentPoint = MTextAttachmentPoint.MiddleCenter; break;
                case netDxf.Entities.TextAlignment.MiddleRight:
                    retorno.AttachmentPoint = MTextAttachmentPoint.MiddleRight; break;
                case netDxf.Entities.TextAlignment.BottomLeft:
                    retorno.AttachmentPoint = MTextAttachmentPoint.BottomLeft; break;
                case netDxf.Entities.TextAlignment.BottomCenter:
                    retorno.AttachmentPoint = MTextAttachmentPoint.BottomCenter; break;
                case netDxf.Entities.TextAlignment.BottomRight:
                    retorno.AttachmentPoint = MTextAttachmentPoint.BottomRight; break;
                case netDxf.Entities.TextAlignment.BaselineLeft:
                    retorno.AttachmentPoint = MTextAttachmentPoint.BottomLeft; break;
                case netDxf.Entities.TextAlignment.BaselineCenter:
                    retorno.AttachmentPoint = MTextAttachmentPoint.BottomCenter; break;
                case netDxf.Entities.TextAlignment.BaselineRight:
                    retorno.AttachmentPoint = MTextAttachmentPoint.BottomRight; break;
                case netDxf.Entities.TextAlignment.Aligned:
                    retorno.AttachmentPoint = MTextAttachmentPoint.MiddleLeft; break;
                case netDxf.Entities.TextAlignment.Middle:
                    retorno.AttachmentPoint = MTextAttachmentPoint.MiddleCenter; break;
                case netDxf.Entities.TextAlignment.Fit:
                    retorno.AttachmentPoint = MTextAttachmentPoint.MiddleCenter; break;
            }
            retorno.Rotation = text.Rotation;
            retorno.Position = new Vector3(text.Position.X, text.Position.Y, text.Position.Z);
            retorno.Color = text.Color;
            retorno.Height = text.Height;
            retorno.Layer = text.Layer;



            return retorno;
        }

        public static double Comprimento(this netDxf.Entities.Line line)
        {

            return line.StartPoint.P3d().Distancia(line.EndPoint.P3d());
        }
        public static double Angulo(this netDxf.Entities.Line line)
        {
            return line.StartPoint.P3d().GetAngulo(line.EndPoint.P3d());
        }

        public static P3d P3d(this Vector3 vector3)
        {
            return new P3d(vector3.X, vector3.Y, vector3.Z);
        }

        public static double DistMaxX(this netDxf.Entities.Line L, P3d L2)
        {
            List<double> dists = new List<double>();
            dists.Add(Math.Abs(L.StartPoint.X - L2.X));
            dists.Add(Math.Abs(L.EndPoint.X - L2.X));
            return dists.Max();
        }
        public static double DistMaxY(this netDxf.Entities.Line L, P3d L2)
        {
            List<double> dists = new List<double>();
            dists.Add(Math.Abs(L.StartPoint.Y - L2.Y));
            dists.Add(Math.Abs(L.EndPoint.Y - L2.Y));
            return dists.Max();
        }
        public static double DistMinX(this netDxf.Entities.Line L, P3d L2)
        {
            List<double> dists = new List<double>();
            dists.Add(Math.Abs(L.StartPoint.X - L2.X));
            dists.Add(Math.Abs(L.EndPoint.X - L2.X));
            return dists.Min();
        }
        public static double DistMinY(this netDxf.Entities.Line L, P3d L2)
        {
            List<double> dists = new List<double>();
            dists.Add(Math.Abs(L.StartPoint.Y - L2.Y));
            dists.Add(Math.Abs(L.EndPoint.Y - L2.Y));
            return dists.Min();
        }
        public static double DistX(this netDxf.Entities.Insert L, P3d L2)
        {
            return Math.Abs(L.Position.X - L2.X);
        }
        public static double DistX(this netDxf.Entities.Text L, P3d L2)
        {
            return Math.Abs(L.Position.X - L2.X);
        }
        public static double DistY(this netDxf.Entities.Insert L, P3d L2)
        {
            return Math.Abs(L.Position.Y - L2.Y);
        }
        public static double DistY(this netDxf.Entities.Text L, P3d L2)
        {
            return Math.Abs(L.Position.Y - L2.Y);
        }


        public static bool Horizontal(this netDxf.Entities.Line s)
        {
            return Math.Abs(s.Angulo()) == 0 | Math.Abs(s.Angulo()) == 180;
        }

        public static bool Vertical(this netDxf.Entities.Line x)
        {
            return Math.Abs(x.Angulo()) == 90 | Math.Abs(x.Angulo()) == 270;
        }
        public static double MinX(this netDxf.Entities.Line L)
        {
            return L.StartPoint.X < L.EndPoint.X ? L.StartPoint.X : L.EndPoint.X;
        }


        public static double MinX(this netDxf.DxfDocument doc)
        {
            var coords = doc.GetObjetos().SelectMany(x => x.GetPontos()).ToList();
            if (coords.Count > 0)
            {
                return coords.Min(x => x.X);
            }
            return 0;
        }
        public static double MinY(this netDxf.DxfDocument doc)
        {
            var coords = doc.GetObjetos().SelectMany(x => x.GetPontos()).ToList();
            if (coords.Count > 0)
            {
                return coords.Min(x => x.Y);
            }
            return 0;
        }
        public static double MaxY(this netDxf.DxfDocument doc)
        {
            var coords = doc.GetObjetos().SelectMany(x => x.GetPontos()).ToList();
            if (coords.Count > 0)
            {
                return coords.Max(x => x.Y);
            }
            return 0;
        }
        public static double MaxX(this netDxf.DxfDocument doc)
        {
            var coords = doc.GetObjetos().SelectMany(x => x.GetPontos()).ToList();
            if (coords.Count > 0)
            {
                return coords.Max(x => x.X);
            }
            return 0;
        }

        public static P3d GetTopDir(this netDxf.DxfDocument doc)
        {
            var X = MaxX(doc);
            var Y = MaxY(doc);
            return new P3d(X, Y);
        }
        public static P3d GetBotEsq(this netDxf.DxfDocument doc)
        {
            var X = MinX(doc);
            var Y = MinY(doc);

            return new P3d(X, Y);
        }
        public static List<netDxf.Entities.EntityObject> GetTextos(this netDxf.DxfDocument doc, string contem = null)
        {
            var retorno = new List<netDxf.Entities.EntityObject>();
            if (contem == null)
            {
                retorno.AddRange(doc.MTexts);
                retorno.AddRange(doc.Texts);
            }
            else
            {
                retorno.AddRange(doc.MTexts.ToList().FindAll(x => x.Value.ToUpper().Contains(contem.ToUpper())));
                retorno.AddRange(doc.MTexts.ToList().FindAll(x => x.Value.ToUpper().Contains(contem.ToUpper())));
            }


            return retorno;
        }
        public static List<netDxf.Entities.EntityObject> GetObjetos(this netDxf.DxfDocument doc)
        {
            var retorno = new List<netDxf.Entities.EntityObject>();
            retorno.AddRange(doc.Arcs);
            retorno.AddRange(doc.Circles);
            retorno.AddRange(doc.Dimensions);
            retorno.AddRange(doc.Ellipses);
            retorno.AddRange(doc.Faces3d);
            retorno.AddRange(doc.Hatches);
            retorno.AddRange(doc.Images);
            retorno.AddRange(doc.Inserts);
            retorno.AddRange(doc.Leaders);
            retorno.AddRange(doc.Lines);
            retorno.AddRange(doc.LwPolylines);
            retorno.AddRange(doc.Meshes);
            retorno.AddRange(doc.MLines);
            retorno.AddRange(doc.MTexts);
            retorno.AddRange(doc.Points);
            retorno.AddRange(doc.PolyfaceMeshes);
            retorno.AddRange(doc.Polylines);
            retorno.AddRange(doc.Rays);
            retorno.AddRange(doc.Shapes);
            retorno.AddRange(doc.Solids);
            retorno.AddRange(doc.Splines);
            retorno.AddRange(doc.Texts);
            retorno.AddRange(doc.Tolerances);
            retorno.AddRange(doc.XLines);

            return retorno;
        }
        public static List<P3d> GetPontos(this netDxf.Entities.EntityObject obj)
        {
            int Precisao = 10;
            var retorno = new List<P3d>();
            if (obj is netDxf.Entities.Arc)
            {
                retorno.AddRange((obj as netDxf.Entities.Arc).PolygonalVertexes(Precisao).Select(x => new P3d(x.X, x.Y, 0)));
            }
            else if (obj is netDxf.Entities.Circle)
            {
                retorno.AddRange((obj as netDxf.Entities.Circle).PolygonalVertexes(Precisao).Select(x => new P3d(x.X, x.Y, 0)));
            }
            else if (obj is netDxf.Entities.Ellipse)
            {
                retorno.AddRange((obj as netDxf.Entities.Ellipse).PolygonalVertexes(Precisao).Select(x => new P3d(x.X, x.Y, 0)));
            }
            else if (obj is netDxf.Entities.Line)
            {
                retorno.Add((obj as netDxf.Entities.Line).StartPoint.P3d());
                retorno.Add((obj as netDxf.Entities.Line).EndPoint.P3d());
            }
            else if (obj is netDxf.Entities.LwPolyline)
            {
                retorno.AddRange((obj as netDxf.Entities.LwPolyline).Vertexes.Select(x => new P3d(x.Position.X, x.Position.Y, 0)));
            }
            else if (obj is netDxf.Entities.MLine)
            {
                retorno.AddRange((obj as netDxf.Entities.MLine).Vertexes.Select(x => new P3d(x.Position.X, x.Position.Y, 0)));
            }
            else if (obj is netDxf.Entities.Point)
            {
                retorno.Add((obj as netDxf.Entities.Point).Position.P3d());
            }
            else if (obj is netDxf.Entities.Polyline)
            {
                retorno.AddRange((obj as netDxf.Entities.Polyline).Vertexes.Select(x => new P3d(x.Position.X, x.Position.Y, 0)));
            }
            else if (obj is netDxf.Entities.Spline)
            {
                retorno.AddRange((obj as netDxf.Entities.Spline).PolygonalVertexes(Precisao).Select(x => new P3d(x.X, x.Y, 0)));
            }

            else if (obj is netDxf.Entities.Insert)
            {
                var blk = obj as netDxf.Entities.Insert;
                foreach (var e in blk.Block.Entities)
                {
                    retorno.AddRange(e.GetPontos().Select(x => new P3d(x.X + blk.Position.X, x.Y + blk.Position.Y, x.Z + blk.Position.Z)));
                }
            }
            return retorno;
        }

        public static double MaxX(this netDxf.Entities.Line L)
        {
            return L.StartPoint.X > L.EndPoint.X ? L.StartPoint.X : L.EndPoint.X;
        }
        public static double MinY(this netDxf.Entities.Line L)
        {
            return L.StartPoint.X < L.EndPoint.Y ? L.StartPoint.Y : L.EndPoint.Y;
        }
        public static double MaxY(this netDxf.Entities.Line L)
        {
            return L.StartPoint.Y > L.EndPoint.Y ? L.StartPoint.Y : L.EndPoint.Y;
        }
        public static double AnguloAbs(this netDxf.Entities.Line L)
        {
            double ang = Angulo(L);
            while (ang > 360)
            {
                ang = ang - 360;
            }

            while (ang < -360)
            {
                ang = ang + 360;
            }
            return ang;
        }
    }
}
