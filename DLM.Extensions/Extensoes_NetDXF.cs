using Conexoes;
using DLM.macros;
using DLM.vars;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DLM.desenho
{
    public static class ExtensoesNetDxf
    {
        private static Dictionary<System.Windows.Media.Color, AciColor> _cores_aci { get; set; } = new Dictionary<System.Windows.Media.Color, AciColor>();

        public static List<P3d> GetOrigens(this List<EntityObject> objs, bool cotas = false)
        {
            var Retorno = new List<P3d>();
            foreach (var ob in objs)
            {
                if (ob is Line)
                {
                    var t = ob as Line;
                    Retorno.Add(new P3d(t.StartPoint.X, t.StartPoint.Y));
                    Retorno.Add(new P3d(t.EndPoint.X, t.EndPoint.Y));
                }
                else if (ob is Polyline3D)
                {
                    var t = ob as Polyline3D;
                    Retorno.AddRange(t.Vertexes.Select(x => new P3d(x.X, x.Y, x.Z)));
                }
                else if (ob is Polyline2D)
                {
                    var t = ob as Polyline2D;
                    Retorno.AddRange(t.Vertexes.Select(x => new P3d(x.Position.X, x.Position.Y)));
                }
                else if (ob is Circle)
                {
                    var t = ob as Circle;
                    Retorno.Add(new P3d(t.Center.X, t.Center.Y));
                }
                else if (ob is Insert)
                {
                    var t = ob as Insert;
                    Retorno.Add(new P3d(t.Position.X, t.Position.Y));
                }
                else if (ob is Arc)
                {
                    var t = ob as Arc;
                    Retorno.Add(new P3d(t.Center.X, t.Center.Y));
                    Retorno.Add(new P3d(t.Center.X, t.Center.Y).Mover(t.StartAngle, t.Radius));
                    Retorno.Add(new P3d(t.Center.X, t.Center.Y).Mover(t.EndAngle, t.Radius));
                }

                if (cotas)
                {
                    if (ob is AlignedDimension)
                    {
                        var t = ob as AlignedDimension;
                        Retorno.Add(new P3d(t.FirstReferencePoint.X, t.FirstReferencePoint.Y));
                        Retorno.Add(new P3d(t.SecondReferencePoint.X, t.SecondReferencePoint.Y));
                    }
                    else if (ob is OrdinateDimension)
                    {
                        var t = ob as OrdinateDimension;
                        Retorno.Add(new P3d(t.Origin.X, t.Origin.Y));
                        Retorno.Add(new P3d(t.FeaturePoint.X, t.FeaturePoint.Y));
                    }
                    else if (ob is Text)
                    {
                        var t = ob as Text;
                        Retorno.Add(new P3d(t.Position.X, t.Position.Y));
                    }
                    else if (ob is MText)
                    {
                        var t = ob as MText;
                        Retorno.Add(new P3d(t.Position.X, t.Position.Y));
                    }
                }
            }

            return Retorno;
        }
        public static DimensionStyle GetEstilo(this DxfDocument doc, string Nome, double escala)
        {
            var s = doc.DimensionStyles.ToList().Find(X => X.Name.ToUpper().Replace(" ", "") == Nome.ToUpper().Replace(" ", ""));
            if (s != null)
            {
                return s;
            }
            var st = GetEstilo(Nome.ToUpper().Replace(" ", ""), escala);
            return st;
        }

        public static MText GetTexto(List<string> Linhas, Vector2 posicao, double tam, TextStyle style)
        {
            MText retorno = new MText();
            retorno.Height = tam;
            retorno.Style = style;
            retorno.Value = string.Join(@"\P", Linhas);
            retorno.Position = new Vector3(posicao.X, posicao.Y, 0);

            return retorno;
        }
        public static Insert InserirBloco(string Nome, DxfDocument Arquivo_Origem, DxfDocument Arquivo_Destino, P3d Ponto, double Escala = 1, double Angulo = 0)
        {
            try
            {
                var block = Arquivo_Destino.Blocks.ToList().Find(x => x.Name.ToUpper() == Nome.ToUpper());

                if (block == null)
                {
                    block = Arquivo_Origem.Blocks.ToList().Find(x => x.Name.ToUpper() == Nome.ToUpper());
                    block = block.Clone() as netDxf.Blocks.Block;
                }

                if (block != null)
                {

                    var p = new Insert(block);
                    p.Position = new Vector3(0, 0, 0);

                    p.Scale = new Vector3(Escala);

                    p.Position = new Vector3(Ponto.X, Ponto.Y, 0);
                    p.TransformAttributes();
                    p.Rotation = Angulo;
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
        public static List<Vector2> ToVector2(List<System.Windows.Point> pts)
        {
            return pts.Select(x => new Vector2(x.X, x.Y)).ToList();
        }

        public static DimensionStyle GetEstilo(string nome, double escala)
        {
            DimensionStyle retorno = new DimensionStyle(nome);
            retorno.DimLineExtend = 2;
            retorno.DimBaselineSpacing = 2;
            retorno.ExtLineExtend = 2.5;
            retorno.ExtLineOffset = 2.5;
            retorno.ArrowSize = 1;
            retorno.TextHeight = 2;
            retorno.TextStyle = new TextStyle("ROMANS", "romans__.ttf");
            retorno.TextStyle.Height = 0;
            retorno.TextStyle.WidthFactor = 0.7;
            retorno.TextColor = AciColor.Cyan;
            retorno.TextHorizontalPlacement = DimensionStyleTextHorizontalPlacement.Centered;
            retorno.TextDirection = DimensionStyleTextDirection.LeftToRight;
            retorno.DimScaleLinear = 1;
            retorno.DimScaleOverall = escala;
            retorno.LengthPrecision = 0;
            retorno.TextOffset = 1;
            retorno.TextVerticalPlacement = DimensionStyleTextVerticalPlacement.Outside;

            return retorno;

        }
        public static List<OrdinateDimension> CotasAcumuladas(List<P3d> pts, double offset, Sentido sentido = Sentido.Horizontal, Layer Layer = null, DimensionStyle estilo = null, bool inverter = false)
        {
            List<OrdinateDimension> Retorno = new List<OrdinateDimension>();
            pts = pts.Select(x => x.Tratar()).ToList();
            if (sentido == Sentido.Horizontal)
            {
                var ps = pts.GetHorizontaisTop();
                if (inverter)
                {
                    ps = ps.OrderByDescending(x => x.X).ToList();
                }

                for (int i = 0; i < ps.Count; i++)
                {
                    OrdinateDimension pp = new OrdinateDimension(new Vector2(ps[0].X, ps[0].Y), new Vector2(ps[i].X, ps[i].Y), -offset, OrdinateDimensionAxis.X);


                    if (Layer != null)
                    {
                        pp.Layer = Layer;
                    }
                    if (estilo != null)
                    {
                        pp.Style = estilo;
                    }
                    Retorno.Add(pp);
                }
            }
            else if (sentido == Sentido.Vertical)
            {
                var ps = pts.GetVerticaisRight();
                ps = ps.OrderByDescending(x => x.Y).ToList();

                for (int i = 0; i < ps.Count; i++)
                {
                    OrdinateDimension pp = new OrdinateDimension(new Vector2(ps[0].X, ps[0].Y), new Vector2(ps[i].X, ps[i].Y), -offset, OrdinateDimensionAxis.Y);
                    if (Layer != null)
                    {
                        pp.Layer = Layer;
                    }
                    if (estilo != null)
                    {
                        pp.Style = estilo;
                    }
                    Retorno.Add(pp);
                }
            }
            return Retorno;
        }


        public static List<AlignedDimension> CotasHorizontais(List<P3d> pontos, double y, double offset, double escala = 0, Layer layer = null, DimensionStyle estilo = null)
        {
            var xs = pontos.Select(x => x.X).Distinct().ToList().OrderBy(x => x).ToList();
            var tt = new List<AlignedDimension>();
            for (int i = 1; i < xs.Count(); i++)
            {
                var s = Cota(new P3d(xs[i - 1], y), new P3d(xs[i], y), offset, layer, estilo);
                tt.Add(s);
            }
            return tt;
        }





        public static AlignedDimension Cota(P3d p0, P3d p1, double offset, Layer Layer = null, DimensionStyle estilo = null)
        {
            p0 = p0.Tratar();
            p1 = p1.Tratar();
            double dist = p0.Distancia(p1);

            if (dist == 0)
            {
                /**/
                p1.X = p1.X + 1;
                p1.Y = p1.Y + 1;
            }
            var t = new AlignedDimension(new Vector2(p0.X, p0.Y), new Vector2(p1.X, p1.Y), offset);

            if (Layer != null)
            {
                t.Layer = Layer;
            }
            if (estilo != null)
            {
                t.Style = estilo;

            }

            return t;
        }
        public static Leader Leader(string Texto, double Tamanho, P3d Origem, Layer Layer = null, DimensionStyle estilo = null, int offset = 3)
        {
            Origem = Origem.Tratar();

            var p1 = Origem.Mover(45, Tamanho * offset);
            return Leader(Texto, Tamanho, new List<P3d> { new P3d(Origem.X, Origem.Y), new P3d(p1.X, p1.Y) }, Layer, estilo);
        }
        public static Leader Leader(string Texto, double Tamanho, List<P3d> Vetores, Layer Layer = null, DimensionStyle estilo = null)
        {
            var leader = new Leader(Texto, Vetores.Select(x => x.ToVector2()));

            leader.Color = AciColor.Cyan;
            if (Layer != null)
            {
                leader.Layer = Layer;
            }
            if (estilo != null)
            {
                leader.Style = estilo;
            }

            return leader;

        }
        public static Text Texto(P3d Origem, string valor, double tamanho = 10, Layer layer = null, TextStyle estilo = null, double angulo = 0, AciColor cor = null, netDxf.Entities.TextAlignment alinhamento = netDxf.Entities.TextAlignment.MiddleCenter)
        {
            var texto = new Text(valor, Origem.ToVector2(), tamanho);
            texto.Alignment = alinhamento;
            texto.Color = AciColor.Cyan;

            if (angulo > 0)
            {
                texto.Rotation = angulo;
            }
            if (layer != null)
            {
                texto.Layer = layer;
            }
            if (estilo != null)
            {
                texto.Style = estilo;
            }
            if (cor != null)
            {
                texto.Color = cor;
            }
            texto.Rotation = angulo;

            return texto;
        }

        public static List<Line> Cruz(P3d origem, double Diametro, Layer Layer = null)
        {
            double o = (Diametro / 4);
            var ret = new List<Line>();
            ret.Add(Linha(new P3d(origem.X - o, origem.Y - o), new P3d(origem.X + o, origem.Y + o), Layer));
            ret.Add(Linha(new P3d(origem.X + o, origem.Y - o), new P3d(origem.X - o, origem.Y + o), Layer));
            return ret;

        }

        public static List<EntityObject> Oblongo(double Diametro, double Oblongo, double Angulo, double X, double Y, Layer Layer = null)
        {
            var retorno = new List<EntityObject>();

            var Origem = new P3d(X, Y);
            var x0 = new P3d(X, Y).Mover(Angulo, -Oblongo / 2);
            var x1 = new P3d(X, Y).Mover(Angulo, Oblongo / 2);

            var t = new Arc(new Vector2(x0.X, x0.Y), Diametro / 2, 90 + Angulo, -90 + Angulo);
            var t2 = new Arc(new Vector2(x1.X, x1.Y), Diametro / 2, -90 + Angulo, 90 + Angulo);

            if (Layer != null)
            {
                t.Layer = Layer;
                t2.Layer = Layer;
            }

            retorno.Add(Linha(x0.Mover(Angulo - 90, Diametro / 2), x1.Mover(Angulo - 90, Diametro / 2), Layer));
            retorno.Add(Linha(x0.Mover(Angulo + 90, Diametro / 2), x1.Mover(Angulo + 90, Diametro / 2), Layer));

            /*linhas de centro*/
            double c0 = Diametro + Oblongo;
            retorno.Add(Linha(Origem.Mover(Angulo, c0 * .75), Origem.Mover(Angulo, -c0 * .75), Layer));
            retorno.Add(Linha(Origem.Mover(Angulo + 90, Diametro * 0.75), Origem.Mover(Angulo + 90, -Diametro * .75), Layer));
            retorno.Add(t);
            retorno.Add(t2);
            return retorno;
        }

        public static Circle Circulo(double Diametro, double X, double Y, Layer Layer = null, AciColor Cor = null)
        {
            var t = new Circle(new Vector2(X, Y), Diametro / 2);
            if (Layer != null)
            {
                t.Layer = Layer;
            }
            if (Cor != null)
            {
                t.Color = Cor;
            }
            else
            {
                t.Color = AciColor.ByLayer;
            }
            return t;
        }
        public static Ellipse Elipse(double diam1, double diam2, double X, double Y, Layer Layer = null, AciColor Cor = null)
        {
            var t = new Ellipse(new Vector2(X, Y), diam1, diam2);

            if (Layer != null)
            {
                t.Layer = Layer;
            }
            if (Cor != null)
            {
                t.Color = Cor;
            }
            else
            {
                t.Color = AciColor.ByLayer;
            }
            return t;
        }
        public static List<EntityObject> FuroCorte(P3d Origem, double Diametro, double Angulo, Layer Layer = null, AciColor Cor = null)
        {
            var retorno = new List<EntityObject>();
            var p1 = Origem.Mover(Angulo + 90, Diametro);
            var p2 = Origem.Mover(Angulo + 90, -Diametro);
            retorno.Add(Linha(p1, p2, Layer, Linetype.ByLayer, Cor));

            retorno.AddRange(Xis(p1, Diametro, Angulo, Layer, Cor));
            retorno.AddRange(Xis(p2, Diametro, Angulo, Layer, Cor));

            return retorno;
        }

        public static List<EntityObject> Xis(P3d Origem, double Diametro, double Angulo = 0, Layer Layer = null, AciColor cor = null)
        {
            var retorno = new List<EntityObject>();
            retorno.Add(
                Linha(
                Origem.Mover(Angulo + 90 + 45, Diametro / 3),
                Origem.Mover(Angulo + 90 + 45, -Diametro / 3),
                Layer,
                Linetype.ByLayer,
                cor
            )
            );

            retorno.Add(
                Linha(
                Origem.Mover(Angulo + 90 - 45, Diametro / 3),
                Origem.Mover(Angulo + 90 - 45, -Diametro / 3),
                Layer,
                Linetype.ByLayer,
                cor
            )
            );

            return retorno;
        }

        public static List<EntityObject> Furo(P3d Origem, double Diametro, double Oblongo, double Angulo, Layer Layer = null, AciColor Cor = null, Desenho_Furo Tipo = Desenho_Furo.Vista, bool Linhas_De_Centro = true, Sentido Sentido = Sentido.Horizontal)
        {
            if (Oblongo > 0)
            {
                return ExtensoesNetDxf.Oblongo(Diametro, Oblongo, Angulo, Origem.X, Origem.Y, Layer);
            }

            List<EntityObject> retorno = new List<EntityObject>();
            if (Tipo == Desenho_Furo.Vista)
            {
                retorno.Add(Circulo(Diametro, Origem.X, Origem.Y, Layer, Cor));
                if (Linhas_De_Centro)
                {
                    double o = (Diametro / 2) + (Diametro / 3);
                    retorno.Add(Linha(new P3d(Origem.X - o, Origem.Y), new P3d(Origem.X + o, Origem.Y), Layer, Linetype.Dashed, Cor));
                    retorno.Add(Linha(new P3d(Origem.X, Origem.Y - o), new P3d(Origem.X, Origem.Y + o), Layer, Linetype.Dashed, Cor));
                }
            }
            else if (Tipo == Desenho_Furo.Corte)
            {
                double o = Diametro * 1.5;
                if (Sentido == Sentido.Horizontal)
                {
                    retorno.Add(Linha(new P3d(Origem.X - o, Origem.Y), new P3d(Origem.X + o, Origem.Y), Layer));
                    retorno.AddRange(Cruz(new P3d(Origem.X - o, Origem.Y), Diametro, Layer));
                    retorno.AddRange(Cruz(new P3d(Origem.X + o, Origem.Y), Diametro, Layer));
                }
                else if (Sentido == Sentido.Vertical)
                {
                    retorno.Add(Linha(new P3d(Origem.X, Origem.Y - o), new P3d(Origem.X, Origem.Y + o)));
                    retorno.AddRange(Cruz(new P3d(Origem.X, Origem.Y - o), Diametro, Layer));
                    retorno.AddRange(Cruz(new P3d(Origem.X, Origem.Y + o), Diametro, Layer));
                }
            }
            return retorno;
        }
        public static Line Linha(P3d p1, P3d p2, Layer layer = null, Linetype tipo = null, AciColor cor = null)
        {
            if (tipo == null)
            {
                tipo = Linetype.ByLayer;
            }
            var t = new Line(new Vector2(p1.X, p1.Y), new Vector2(p2.X, p2.Y));
            if (layer != null)
            {
                t.Layer = layer;
            }

            if (tipo != null)
            {
                t.Linetype = tipo;
            }
            else
            {
                t.Linetype = Linetype.ByLayer;
            }

            if (cor != null)
            {
                t.Color = cor;
            }

            return t;
        }

        public static Polyline3D Retangulo(double Comprimento, double Largura, double X = 0, double Y = 0, Layer Layer = null, AciColor Cor = null)
        {
            var Vetores = new List<Vector3>();
            Vetores = new List<Vector3> {
                            new Vector3(X, Y, 0),
                            new Vector3(X + Comprimento, Y, 0),
                            new Vector3(X + Comprimento, Y + Largura, 0),
                            new Vector3(X, Y+ Largura, 0),
                            new Vector3(X, Y, 0)

                        };
            var poly = new Polyline3D(Vetores);

            if (Layer != null)
            {
                poly.Layer = Layer;
            }

            if (Cor != null)
            {
                poly.Color = Cor;
            }
            else
            {
                poly.Color = AciColor.ByLayer;
            }

            return poly;
        }
        public static Polyline3D Polilinha(List<P3d> Origens, Layer Layer = null, AciColor Cor = null, Linetype tipo = null)
        {
            var Vetores = new List<Vector3>();
            foreach (var p3d in Origens)
            {


                Vetores.Add(new Vector3(p3d.X.Round(8), p3d.Y.Round(8), 0));
            }
            var poly = new Polyline3D(Vetores);

            if (Layer != null)
            {
                poly.Layer = Layer;
            }
            if (Cor != null)
            {
                poly.Color = Cor;
            }
            else
            {
                poly.Color = AciColor.ByLayer;
            }

            if (tipo != null)
            {
                poly.Linetype = tipo;
            }

            return poly;
        }

        public static AciColor ToAciColor(this System.Windows.Media.SolidColorBrush mediacolor)
        {
            var dColor = System.Drawing.Color.FromArgb(
                                     mediacolor.Color.A,
                                     mediacolor.Color.R,
                                     mediacolor.Color.G,
                                     mediacolor.Color.B
                                     );
            var retorno = new AciColor(dColor);
            return retorno;
        }
        public static AciColor ToAciColor(this Janela_Cor Cor)
        {

            if (Cor == Janela_Cor.Amarelo)
            {
                return AciColor.Yellow;
            }
            else if (Cor == Janela_Cor.Cyan)
            {
                return AciColor.Cyan;
            }
            else if (Cor == Janela_Cor.Green)
            {
                return AciColor.Green;
            }
            else if (Cor == Janela_Cor.Magenta)
            {
                return AciColor.Magenta;
            }
            else if (Cor == Janela_Cor.Red)
            {
                return AciColor.Red;
            }
            else if (Cor == Janela_Cor.Yellow)
            {
                return AciColor.Yellow;
            }
            else
            {
                return AciColor.ByLayer;
            }


        }
        public static AciColor ToAciColor(this System.Windows.Media.Color cor)
        {
            AciColor ret = null;
            var igual = _cores_aci.TryGetValue(cor, out ret);
            if (ret != null)
            {
                return ret;
            }

            AciColor retorno = new AciColor(cor.R, cor.G, cor.B);
            _cores_aci.Add(cor, retorno);

            return retorno;
        }
        public static void Show(this DxfDocument dxf)
        {
            var mm = new Conexoes.Janelas.VisualizarDXF();
            mm.DXF_view.Abrir(dxf);
            mm.Show();
        }

        public static List<EntityObject> AddPerfil_UFrontal(this DxfDocument dxf, Layer l, Layer lC, EM2Parte parte, EM2 esc)
        {

            double y0 = parte.Y + esc.caB;
            double y1 = parte.Y;
            double x1 = parte.X+ esc.puL;
            double x2 = parte.X+ esc.deH + esc.puL;


            var retorno = new List<EntityObject>();
            //desenho do perfil
            var comp = parte.Comp_Fim;
            //esquerda
            retorno.Add(dxf.AddRec(l, 0, 0, esc.puL, comp));
            retorno.Add(dxf.AddLine(l, esc.puL - esc.puE, 0, esc.puL - esc.puE, comp, Linetype.Dashed));
            foreach (var furo in parte.ESQ.Furos)
            {
                if (furo.Face == Furo_Face.Aba1 | furo.Face == Furo_Face.Aba2)
                {
                    retorno.AddRange(dxf.AddFuro(l, esc.puL - esc.puLM, furo.Origem.X, esc.caD, Desenho_Furo.Vista, true, Sentido.Horizontal));
                }
                else
                {
                    retorno.AddRange(dxf.AddFuro(l, esc.puL, furo.Origem.X, esc.caD, Desenho_Furo.Corte, true, Sentido.Horizontal));
                }
            }

            parte.ESQ.DxfVista = dxf.CreateBlock(retorno, parte.ESQ.Nome, new desenho.P3d(parte.X, parte.Y), true, new db.Linha("HASH", parte.ESQ.Hash));
            retorno.Clear();

            //direita
            retorno.Add(dxf.AddRec(l, 0, 0, esc.puL, comp));
            retorno.Add(dxf.AddLine(l, esc.puE, 0, esc.puE, comp, Linetype.Dashed));
            foreach (var furo in parte.DIR.Furos)
            {
                if (furo.Face == Furo_Face.Aba1 | furo.Face == Furo_Face.Aba2)
                {
                    retorno.AddRange(dxf.AddFuro(l, esc.puLM, furo.Origem.X, esc.caD, Desenho_Furo.Vista, true, Sentido.Horizontal));
                }
                else
                {
                    retorno.AddRange(dxf.AddFuro(l, 0, furo.Origem.X, esc.caD, Desenho_Furo.Corte, true, Sentido.Horizontal));
                }
            }

            parte.DIR.DxfVista = dxf.CreateBlock(retorno, parte.DIR.Nome, new desenho.P3d(parte.X+ esc.deH + esc.puL, parte.Y), true, new db.Linha("HASH", parte.DIR.Hash));
            retorno.Clear();


            //cantoneira da base
            if ((parte.Tipo == EM2_Tipo.Parte_Inicial | parte.Tipo == EM2_Tipo.Parte_Simples) && esc.Considerar_Cantoneira_Base)
            {
                //cantoneiras
                //esquerdo
                retorno.AddRange(dxf.AddPerfilL(lC, x1 - esc.puE, y0, Orientacao.SEI, esc.caC, esc.caL, esc.caE));
                //direito
                retorno.AddRange(dxf.AddPerfilL(lC, x2 + esc.puE, y0, Orientacao.SDS, esc.caC, esc.caL, esc.caE));

                //furos base
                retorno.AddRange(dxf.AddFuro(lC, x1 - esc.caO - esc.puE, y1, esc.caD, Desenho_Furo.Corte, true, Sentido.Vertical));
                retorno.AddRange(dxf.AddFuro(lC, x2 + esc.caO + esc.puE, y1, esc.caD, Desenho_Furo.Corte, true, Sentido.Vertical));
            }
            return retorno;
        }
        public static List<EntityObject> AddPerfil_ULateral(this DxfDocument dxf, Layer l, EM2Parte parte, double x, EM2 esc)
        {

            var retorno = new List<netDxf.Entities.EntityObject>();

            var comp = parte.Comp_Fim;
            double y1 = 0;
            double y2 = comp;


            retorno.Add(dxf.AddLine(l, esc.puS - esc.puE, y1, esc.puS - esc.puE, y2));
            retorno.Add(dxf.AddRec(l, 0, 0, esc.puS, comp));
            retorno.Add(dxf.AddLine(l, esc.puE, y1, esc.puE, y2));
            foreach (var furo in parte.ESQ.Furos)
            {
                if (furo.Face == Furo_Face.Alma)
                {
                    retorno.AddRange(dxf.AddFuro(l, furo.Origem.Y, furo.Origem.X, furo.Diametro, Desenho_Furo.Vista));
                }
                else if (furo.Face == Furo_Face.Aba2)
                {
                    retorno.AddRange(dxf.AddFuro(l, 0, furo.Origem.X, furo.Diametro, Desenho_Furo.Corte));
                }
                else if (furo.Face == Furo_Face.Aba1)
                {
                    retorno.AddRange(dxf.AddFuro(l, esc.puS, furo.Origem.X, furo.Diametro, Desenho_Furo.Corte));
                }
            }
            parte.ESQ.DxfCorte = dxf.CreateBlock(retorno, parte.ESQ.Nome, new desenho.P3d(x, parte.Y), true, new db.Linha("HASH", parte.ESQ.Hash));

            retorno.Clear();



            retorno.Add(dxf.AddLine(l, esc.puS - esc.puE, y1, esc.puS - esc.puE, y2));
            retorno.Add(dxf.AddRec(l, 0, 0, esc.puS, comp));
            retorno.Add(dxf.AddLine(l, esc.puE, y1, esc.puE, y2));

            foreach (var furo in parte.DIR.Furos)
            {
                if (furo.Face == Furo_Face.Alma)
                {
                    retorno.AddRange(dxf.AddFuro(l, furo.Origem.Y, furo.Origem.X, furo.Diametro, Desenho_Furo.Vista));
                }
                else if (furo.Face == Furo_Face.Aba1)
                {
                    retorno.AddRange(dxf.AddFuro(l, 0, furo.Origem.X, furo.Diametro, Desenho_Furo.Corte));
                }
                else if (furo.Face == Furo_Face.Aba2)
                {
                    retorno.AddRange(dxf.AddFuro(l, 0 + esc.puS, furo.Origem.X, furo.Diametro, Desenho_Furo.Corte));
                }
            }
            parte.DxfCorte = dxf.CreateBlock(retorno, parte.DIR.Nome, new desenho.P3d(x, parte.Y), true, new db.Linha("HASH", parte.DIR.Hash));




            return retorno;
        }
        public static List<EntityObject> AddPerfil_U(this DxfDocument dxf, Layer l, double X, double Y, Orientacao lado, double alt, double esp, double larg = 0, double comp = 0)
        {
            var retorno = new List<EntityObject>();

            double m_larg = (larg / 2);
            double m_alt = (alt / 2);



            string prefix = $"U_{alt.String(0)}X{larg.String(0)}X{esp.String(2)}_{lado}";
            if (comp > 0)
            {
                prefix += $"x{comp.String(0)}";
            }
            var nome = prefix;


            var igual = dxf.Blocks.ToList().Find(x => x.Name.ToUpper() == nome);
            if (igual != null)
            {
                var insert1 = new netDxf.Entities.Insert(igual);
                insert1.Position = new Vector3(X, Y, 0);
                dxf.Entities.Add(insert1);
                return new List<EntityObject>() { insert1 };
            }


            var block = new netDxf.Blocks.Block(nome);
            if (lado == Orientacao.SDS)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(0 + m_larg, 0 + m_alt, 0),
                            new Vector3(0 - m_larg, 0 + m_alt, 0),
                            new Vector3(0 - m_larg, 0 - m_alt, 0),
                            new Vector3(0 + m_larg, 0 - m_alt, 0),
                            new Vector3(0 + m_larg, 0 - m_alt + esp, 0),
                            new Vector3(0 - m_larg+ esp, 0 - m_alt + esp, 0),
                            new Vector3(0 - m_larg+ esp, 0 + m_alt - esp, 0),
                            new Vector3(0 +  m_larg, 0 + m_alt - esp, 0),
                            new Vector3(0 +  m_larg, 0 + m_alt, 0)
                        };
                block.Entities.Add(new netDxf.Entities.Polyline3D(Vetores) { Layer = l, Color = AciColor.ByLayer });
            }
            else if (lado == Orientacao.SES)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(0 - m_larg, 0 + m_alt, 0),
                            new Vector3(0 + m_larg, 0 + m_alt, 0),
                            new Vector3(0 + m_larg, 0 - m_alt, 0),
                            new Vector3(0 - m_larg, 0 - m_alt, 0),
                            new Vector3(0 - m_larg, 0 - m_alt + esp, 0),
                            new Vector3(0 + m_larg-esp, 0 - m_alt + esp, 0),
                            new Vector3(0 + m_larg-esp, 0 + m_alt - esp, 0),
                            new Vector3(0 - m_larg, 0 + m_alt - esp, 0),
                            new Vector3(0 - m_larg, 0 + m_alt, 0)
                        };
                block.Entities.Add(new netDxf.Entities.Polyline3D(Vetores) { Layer = l, Color = AciColor.ByLayer });
            }
            else if (lado == Orientacao.HDS)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(0, 0 + m_alt, 0),
                            new Vector3(0+comp, 0 + m_alt, 0),
                            new Vector3(0+comp, 0 - m_alt, 0),
                            new Vector3(0, 0 - m_alt, 0),
                            new Vector3(0, 0 + m_alt, 0)
                        };
                block.Entities.Add(new netDxf.Entities.Polyline3D(Vetores) { Layer = l, Color = AciColor.ByLayer });
                block.AddLine(l, 0, 0 + m_alt - esp, 0 + comp, 0 + m_alt - esp);
                block.AddLine(l, 0, 0 - m_alt + esp, 0 + comp, 0 - m_alt + esp);
            }
            else if (lado == Orientacao.HES)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(0, 0 + m_alt, 0),
                            new Vector3(0-comp, 0 + m_alt, 0),
                            new Vector3(0-comp, 0 - m_alt, 0),
                            new Vector3(0, 0 - m_alt, 0),
                            new Vector3(0, 0 + m_alt, 0)
                        };
                block.Entities.Add(new netDxf.Entities.Polyline3D(Vetores) { Layer = l, Color = AciColor.ByLayer });
                block.AddLine(l, 0, 0 + m_alt - esp, 0 - comp, 0 + m_alt - esp);
                block.AddLine(l, 0, 0 - m_alt + esp, 0 - comp, 0 - m_alt + esp);
            }
            else if (lado == Orientacao.VDS)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(0, 0, 0),
                            new Vector3(0+alt, 0, 0),
                            new Vector3(0+alt, 0 - comp, 0),
                            new Vector3(0, 0 - comp, 0),
                            new Vector3(0, 0, 0)
                        };
                block.Entities.Add(new netDxf.Entities.Polyline3D(Vetores) { Layer = l, Color = AciColor.ByLayer });
                block.AddLine(l, 0 + alt - esp, 0, 0 + alt - esp, 0 - comp);
                block.AddLine(l, 0 + esp, 0, 0 + esp, 0 - comp);

            }
            else if (lado == Orientacao.HDI && comp > 0)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(0, 0 + m_alt, 0),
                            new Vector3(0-comp, 0 + m_alt, 0),
                            new Vector3(0-comp, 0 - m_alt, 0),
                            new Vector3(0, 0 - m_alt, 0),
                            new Vector3(0, 0 + m_alt, 0)
                        };
                block.Entities.Add(new netDxf.Entities.Polyline3D(Vetores) { Layer = l, Color = AciColor.ByLayer });
                block.AddLine(l, 0, 0 + m_alt - esp, 0 - comp, 0 + m_alt - esp);
                block.AddLine(l, 0, 0 - m_alt + esp, 0 - comp, 0 - m_alt + esp);
            }


            dxf.Blocks.Add(block);
            var block_ent = new netDxf.Entities.Insert(block);
            block_ent.Position = new Vector3(X, Y, 0);
            dxf.Entities.Add(block_ent);
            retorno.Add(block_ent);
            return retorno;
        }
        public static List<EntityObject> AddPerfilL(this DxfDocument dxf, Layer l, double X, double Y, Orientacao lado, double comp = 0, double larg = 0, double esp = 0)
        {
            var retorno = new List<EntityObject>();


            if (larg == 0) { larg = Global.EM1.Cantoneira_Largura; };
            if (esp == 0) { esp = Global.EM1.Cantoneira_Espessura; };

            double alt = larg;
            double m_alt = (alt / 2);
            double m_comp = comp / 2;


            string prefix = $"L_{alt.String(0)}X{larg.String(0)}X{esp.String(2)}_{lado}";
            if (comp > 0)
            {
                prefix += $"x{comp.String(0)}";
            }
            var nome = prefix;


            var igual = dxf.Blocks.ToList().Find(x => x.Name.ToUpper() == nome);
            if (igual != null)
            {
                var insert1 = new Insert(igual);
                insert1.Position = new Vector3(X, Y, 0);
                dxf.Entities.Add(insert1);
                return new List<EntityObject>() { insert1 };
            }

            int c = 1;
            while (dxf.Blocks.ToList().Find(x => x.Name.ToUpper() == nome) != null)
            {
                nome = $"{prefix}{c}";
                c++;
            }
            var block = new netDxf.Blocks.Block(nome);
            if (lado == Orientacao.SDS)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                             new Vector3(0, 0 + m_alt, 0),
                             new Vector3(0, 0 - m_alt, 0),
                             new Vector3(0+larg, 0 - m_alt, 0),
                             new Vector3(0+larg, 0 - m_alt+ esp, 0),
                             new Vector3(0+ esp, 0 - m_alt+ esp, 0),
                             new Vector3(0+ esp, 0 + m_alt, 0),
                             new Vector3(0, 0 + m_alt, 0)

                        };
                block.Entities.Add(new netDxf.Entities.Polyline3D(Vetores) { Layer = l, Color = AciColor.ByLayer });
            }
            else if (lado == Orientacao.SDI)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(0, 0 + m_alt, 0),
                             new Vector3(0, 0 - m_alt, 0),
                             new Vector3(0+esp, 0 - m_alt, 0),
                             new Vector3(0+esp, 0 + m_alt - esp, 0),
                             new Vector3(0+larg, 0 + m_alt - esp, 0),
                             new Vector3(0+larg, 0 + m_alt, 0),
                             new Vector3(0, 0 + m_alt, 0)


                        };
                block.Entities.Add(new Polyline3D(Vetores) { Layer = l, Color = AciColor.ByLayer });
            }
            else if (lado == Orientacao.SES)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(0, 0 + m_alt, 0),
                            new Vector3(0 -larg, 0 + m_alt, 0),
                            new Vector3(0 -larg, 0 + m_alt-esp, 0),
                            new Vector3(0 -esp, 0 + m_alt-esp, 0),
                            new Vector3(0 -esp, 0 - m_alt, 0),
                            new Vector3(0, 0 - m_alt, 0),
                            new Vector3(0, 0 + m_alt, 0)
                        };
                block.Entities.Add(new Polyline3D(Vetores) { Layer = l, Color = AciColor.ByLayer });
            }
            else if (lado == Orientacao.SEI)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(0, 0 + m_alt, 0),
                            new Vector3(0 -esp, 0 + m_alt, 0),
                            new Vector3(0 -esp, 0 - m_alt+esp, 0),
                            new Vector3(0 -larg, 0 - m_alt+esp, 0),
                            new Vector3(0 -larg, 0 - m_alt, 0),
                            new Vector3(0, 0 - m_alt, 0),
                            new Vector3(0, 0 + m_alt, 0)
                        };
                block.Entities.Add(new Polyline3D(Vetores) { Layer = l, Color = AciColor.ByLayer });
            }
            else if (lado == Orientacao.HDS | lado == Orientacao.HES)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(0 + m_comp, 0, 0),
                            new Vector3(0 + m_comp, 0 + larg, 0),
                            new Vector3(0 - m_comp, 0 + larg, 0),
                            new Vector3(0 - m_comp, 0, 0),
                            new Vector3(0 + m_comp, 0, 0),

                        };
                block.Entities.Add(new Polyline3D(Vetores) { Layer = l });
                block.AddLine(l, 0 - m_comp, 0 + esp, 0 + m_comp, 0 + esp, Linetype.Dashed);
            }
            else if (lado == Orientacao.HDI | lado == Orientacao.HEI)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(0 + m_comp, 0, 0),
                            new Vector3(0 + m_comp, 0 - larg, 0),
                            new Vector3(0 - m_comp, 0 - larg, 0),
                            new Vector3(0 - m_comp, 0, 0),
                            new Vector3(0 + m_comp, 0, 0),

                        };
                block.Entities.Add(new Polyline3D(Vetores) { Layer = l });
                block.AddLine(l, 0 - m_comp, 0 - esp, 0 + m_comp, 0 - esp, Linetype.Dashed);

            }
            else if (lado == Orientacao.HDV)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(0, 0, 0),
                            new Vector3(0 + larg, 0, 0),
                            new Vector3(0 + larg, 0-comp, 0),
                            new Vector3(0, 0-comp, 0),
                            new Vector3(0, 0, 0),
                        };
                block.Entities.Add(new Polyline3D(Vetores) { Layer = l });
                block.AddLine(l, 0 + esp, 0, 0 + esp, 0 - comp, Linetype.Dashed);

            }
            else if (lado == Orientacao.HEV)
            {
                var Vetores = new List<Vector3>();
                Vetores = new List<Vector3> {
                            new Vector3(0, 0, 0),
                            new Vector3(0 - larg, 0, 0),
                            new Vector3(0 - larg, 0-comp, 0),
                            new Vector3(0, 0-comp, 0),
                            new Vector3(0, 0, 0),
                        };
                block.Entities.Add(new Polyline3D(Vetores) { Layer = l });
                block.AddLine(l, 0 - esp, 0, 0 - esp, 0 - comp, Linetype.Dashed);

            }

            dxf.Blocks.Add(block);
            var insert = new Insert(block);
            insert.Position = new Vector3(X, Y, 0);
            dxf.Entities.Add(insert);

            retorno.Add(insert);

            return retorno;
        }

        public static Insert CreateBlock(this DxfDocument dxf, List<EntityObject> entities, string prefix = "BLOCO", P3d origem = null, bool criar_se_existir = true, DLM.db.Linha atributos = null)
        {

            if (origem == null)
            {
                origem = new P3d();
            }
            var nome = prefix;
            int c = 1;
            if (!criar_se_existir)
            {
                var blk = dxf.Blocks.ToList().Find(x => x.Name.ToUpper() == nome.ToUpper());
                if (blk != null)
                {
                    var ent = new Insert(blk);
                    ent.Position = origem.ToVector3();
                    dxf.Entities.Add(ent);
                    dxf.Remove(entities);
                    return ent;
                }
            }
            while (dxf.Blocks.ToList().Find(x => x.Name.ToUpper() == nome.ToUpper()) != null)
            {
                nome = $"{prefix}_{c}";
                c++;
            }

            var block = new netDxf.Blocks.Block(nome);
            foreach (var ent in entities.FindAll(x => x != null))
            {
                try
                {
                    var n_ent = (EntityObject)ent.Clone();
                    block.Entities.Add(n_ent);
                }
                catch (Exception)
                {

                }
            }

            if (atributos != null)
            {
                if (atributos.Count > 0)
                {

                    foreach (var cel in atributos)
                    {
                        var tag = cel.Coluna.ToUpper();

                        var natt = new netDxf.Entities.AttributeDefinition(cel.Coluna);
                        natt.Value = cel.Valor;
                        natt.Height = 0.0001;
                        block.AttributeDefinitions.Add(natt);
                    }
                }

            }


            var insert = new netDxf.Entities.Insert(block);
            insert.Position = origem.ToVector3();

            insert.Sync();

            dxf.Entities.Add(insert);

            dxf.Remove(entities);

            return insert;
        }

        public static void Remove(this DxfDocument dxf, List<EntityObject> entities)
        {
            dxf.Entities.Remove(entities);
        }


        public static Polyline2D AddRec(this DxfDocument dxf, Layer l, double X, double Y, double comp, double larg)
        {
            var Vetores = new List<Polyline2DVertex>();
            Vetores = new List<Polyline2DVertex> {
                            new Polyline2DVertex(X, Y, 0),
                            new Polyline2DVertex(X + comp, Y, 0),
                            new Polyline2DVertex(X + comp, Y + larg, 0),
                            new Polyline2DVertex(X, Y+ larg, 0),
                            new Polyline2DVertex(X, Y, 0)

                        };
            var pol = new netDxf.Entities.Polyline2D(Vetores) { Layer = l, Color = AciColor.ByLayer };

            dxf.Entities.Add(pol);

            return pol;
        }
        public static Polyline2D AddPol(this DxfDocument dxf, Layer l, double thick = 0, params P3d[] pts)
        {
            var Vetores = new List<Vector2>();
            foreach (var pt in pts)
            {
                Vetores.Add(pt.ToVector2());
            }




            var pol = new netDxf.Entities.Polyline2D(Vetores) { Layer = l, Color = AciColor.ByLayer };
            if (thick > 0)
            {
                pol.SetConstantWidth(thick);
            }

            dxf.Entities.Add(pol);

            return pol;
        }
        public static Line AddLine(this DxfDocument document, Layer l, double x1, double y1, double x2, double y2, Linetype type = null)
        {
            var nl = NewLine(l, x1, y1, x2, y2, ref type);
            document.Entities.Add(nl);
            return nl;
        }
        public static List<Line> AddXis(this DxfDocument dxf, Layer Layer, double X, double Y, double diam)
        {
            var linhas = new List<Line>();
            double o = (diam / 4);

            linhas.Add(dxf.AddLine(Layer, X - o, Y - o, X + o, Y + o));
            linhas.Add(dxf.AddLine(Layer, X + o, Y - o, X - o, Y + o));

            return linhas;
        }
        public static List<EntityObject> AddFuro(this DxfDocument dxf, Layer l, double X, double Y, double diam, Desenho_Furo tipo, bool linhaDeCentro = true, Sentido sentido = Sentido.Horizontal)
        {
            var nome_furo = $"FURO_{diam.Round(0)}_{tipo}_{sentido}";
            var igual = dxf.Blocks.ToList().Find(x => x.Name == nome_furo);
            if (igual != null)
            {
                var insert = new netDxf.Entities.Insert(igual);
                insert.Position = new Vector3(X, Y, 0);
                dxf.Entities.Add(insert);
                return new List<EntityObject> { insert };
            }
            var itens_bloco = new List<netDxf.Entities.EntityObject>();
            if (tipo == Desenho_Furo.Vista)
            {
                var circulo = new netDxf.Entities.Circle(new Vector2(0, 0), diam / 2) { Layer = l, Color = AciColor.ByLayer };
                dxf.Entities.Add(circulo);
                itens_bloco.Add(circulo);
                if (linhaDeCentro)
                {
                    double o = (diam / 2) + (diam / 3);

                    itens_bloco.Add(dxf.AddLine(l, -o, 0, +o, 0, Linetype.Dashed));
                    itens_bloco.Add(dxf.AddLine(l, 0, -o, 0, +o, Linetype.Dashed));
                }
            }
            else if (tipo == Desenho_Furo.Corte)
            {
                double o = diam * 1.5;
                if (sentido == Sentido.Horizontal)
                {
                    itens_bloco.Add(dxf.AddLine(l, -o, 0, +o, 0));
                    itens_bloco.AddRange(dxf.AddXis(l, -o, 0, diam));
                    itens_bloco.AddRange(dxf.AddXis(l, +o, 0, diam));
                }
                else if (sentido == Sentido.Vertical)
                {
                    itens_bloco.Add(dxf.AddLine(l, 0, -o, 0, +o));
                    itens_bloco.AddRange(dxf.AddXis(l, 0, -o, diam));
                    itens_bloco.AddRange(dxf.AddXis(l, 0, +o, diam));
                }
            }


            return new List<EntityObject> { dxf.CreateBlock(itens_bloco, nome_furo, new desenho.P3d(X, Y)) };
        }
        public static Line AddLine(this netDxf.Blocks.Block block, Layer Layer, double x1, double y1, double x2, double y2, Linetype type = null)
        {
            Line l = NewLine(Layer, x1, y1, x2, y2, ref type);
            block.Entities.Add(l);
            return l;
        }
        
        private static Line NewLine(Layer l, double x1, double y1, double x2, double y2, ref Linetype type)
        {
            if (type == null)
            {
                type = Linetype.ByLayer;
            }
            var nl = new netDxf.Entities.Line(new Vector2(x1, y1), new Vector2(x2, y2)) { Layer = l, Color = AciColor.ByLayer, Linetype = type };
            return nl;
        }


        public static Leader AddLeader(this DxfDocument dxf, Layer l, P3d origem, double offset, string texto, double tamanho, DimensionStyle style)
        {
            return AddLeader(dxf, l, new List<Vector2>() { origem.ToVector2(), origem.MoverX(offset).MoverY(offset).ToVector2() }, texto, tamanho, style);
        }
        public static Leader AddLeader(this DxfDocument dxf, Layer l, List<Vector2> Vetores, string Texto, double Tamanho, DimensionStyle style)
        {
            var leader = new netDxf.Entities.Leader(Texto, Vetores, style);
            leader.Color = AciColor.Cyan;
            dxf.Entities.Add(leader);
            return leader;
        }

        public static LinearDimension AddCotaLinear(this DxfDocument dxf, Layer l, P3d p1, P3d p2, double offset, DimensionStyle style)
        {
            if (p1.Distancia(p2) > 0)
            {
                var ang = p1.GetAngulo(p2);
                var linha = new Line(p1.ToVector2(), p2.ToVector2());
                var cota = new netDxf.Entities.LinearDimension(linha, offset, ang, style);
                cota.Layer = l;
                cota.Style = style;
                dxf.Entities.Add(cota);

                return cota;
            }
            return null;
        }
        public static AlignedDimension AddCota(this DxfDocument dxf, Layer l, double x0, double y0, double x1, double y1, double offset, DimensionStyle style)
        {
            //offset = Math.Abs(offset);

            if (Math.Abs(new P3d(x0, y0).Distancia(new P3d(x1, y1))) > 0/* && offset > 0*/)
            {
                var cota = new netDxf.Entities.AlignedDimension(new Vector2(x0, y0), new Vector2(x1, y1), offset);
                cota.Layer = l;
                cota.Style = style;
                dxf.Entities.Add(cota);

                return cota;
            }
            return null;
        }
        public static void SetDimensionStyle(this DxfDocument dxf, DimensionStyle style)
        {
            foreach (var t in dxf.Entities.Dimensions)
            {
                t.Style = style;
            }
            foreach (var t in dxf.Entities.Texts)
            {
                t.Style = style.TextStyle;
                t.Height = style.TextHeight * style.DimScaleOverall;
                t.Color = style.TextColor;
            }
            foreach (var t in dxf.Entities.MTexts)
            {
                t.Style = style.TextStyle;
                t.Height = style.TextHeight * style.DimScaleOverall;
                t.Color = style.TextColor;
            }
            foreach (var t in dxf.Entities.Leaders)
            {
                t.Style = style;

                if (t.Annotation is MText)
                {
                    var s = t.Annotation as MText;

                    s.Style = style.TextStyle;
                    s.Height = style.TextHeight * style.DimScaleOverall;
                    s.Color = style.TextColor;
                }
                t.Update(true);

            }

        }
        public static Text AddText(this DxfDocument dxf, Layer l, double X, double Y, string txt, double tam, TextStyle style = null)
        {
            var text = new netDxf.Entities.Text(txt, new Vector2(X, Y), tam);
            text.Color = AciColor.Cyan;
            if (style != null)
            {
                text.Style = style;
            }
            dxf.Entities.Add(text);
            return text;
        }
        public static MText AddMText(this DxfDocument dxf, string msg, P3d posicao = null)
        {
            if (posicao == null)
            {
                posicao = new P3d();
            }
            return dxf.AddMText(new List<string> { msg }, posicao, true);
        }
        public static MText AddMText(this DxfDocument dxf, string valor, P3d posicao, double tamanho, Layer layer = null, MTextAttachmentPoint origem = MTextAttachmentPoint.MiddleRight)
        {
            var novo = dxf.AddMText(new List<string> { valor }, posicao, true, layer);
            if(tamanho>0)
            {
                novo.Height = tamanho;
            }
            novo.AttachmentPoint = origem;

            novo.Color = AciColor.Cyan;

            return novo;
        }
        public static MText AddMText(this DxfDocument dxf, List<string> linhas, P3d posicao = null, bool adicionar = true, Layer layer = null)
        {
            if (posicao == null)
            {
                posicao = new P3d();
            }
            var retorno = new MText();
            retorno.Height = dxf.DrawingVariables.TextSize;
            TextStyle estilo;
            dxf.TextStyles.TryGetValue(dxf.DrawingVariables.TextStyle, out estilo);
            retorno.Style = estilo;
            retorno.Value = string.Join(@"\P", linhas);
            retorno.Position = new Vector3(posicao.X, posicao.Y, 0);
            if(layer!=null)
            {
                retorno.Layer = layer;
            }
            if (adicionar)
            {
                dxf.Entities.Add(retorno);
            }
            return retorno;
        }

        public static List<DimensionStyle> ClonarDimensionStyles(this DxfDocument origem, DxfDocument destino)
        {
            var retorno = new List<DimensionStyle>();
            var entities = origem.Layouts.GetReferences(netDxf.Objects.Layout.ModelSpaceName);


            foreach (var dim in origem.DimensionStyles)
            {

                if (dim is DimensionStyle)
                {
                    var t1 = (dim as DimensionStyle).Clone();
                    destino.DimensionStyles.Add(t1 as DimensionStyle);
                    retorno.Add(t1 as DimensionStyle);
                }

            }
            return retorno;
        }
        public static void SetAtributo(this Insert ins, string atributo, string Valor)
        {
            var t = ins.Attributes.ToList().Find(x => x.Tag.ToUpper() == atributo.ToUpper());
            if (t != null)
            {
                t.Value = Valor;
            }
        }
        public static Insert AddBlock(this DxfDocument destino, string arquivo, P3d origem, double escala = 1, double ang = 0, Layer l = null, bool layout = false)
        {
            try
            {
                var block = destino.Blocks.ToList().Find(x => x.Name.ToUpper() == arquivo.getNome().ToUpper());

                if (block == null)
                {
                    if (arquivo.Exists())
                    {
                        var dxf = DxfDocument.Load(arquivo);
                        block = new netDxf.Blocks.Block(arquivo.getNome(), dxf.CloneAll(), dxf.GetModelAttributes());
                    }
                }
                if (block != null)
                {

                    var n_Bloco = new Insert(block);
                    n_Bloco.Scale = new Vector3(escala);
                    n_Bloco.Position = new Vector3(origem.X, origem.Y, 0);
                    n_Bloco.Rotation = ang;
                    if (l != null)
                    {
                        n_Bloco.Layer = l;
                    }
                    var layouts = destino.Layouts.ToList().FindAll(x=>x.AssociatedBlock.Name.ToUpper() == "*PAPER_SPACE");

                    if (layout && layouts.Count > 0)
                    {
                        layouts[0].AssociatedBlock.Entities.Add(n_Bloco);
                    }
                    else
                    {
                        destino.Entities.Add(n_Bloco);
                    }
                    return n_Bloco;
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
        public static DxfDocument GetDxf(this string arquivo, bool forcar_novo = false)
        {
            DxfDocument ndxf = null;

            if (!arquivo.Exists() && !forcar_novo)
            {
                ndxf = new DxfDocument();
                ndxf.AddMText("Arquivo não existe.\n" + arquivo);
            }
            else if (!arquivo.Exists() && forcar_novo)
            {
                ndxf = new DxfDocument();
                return ndxf;
            }
            else
            {
                try
                {
                    ndxf = DxfDocument.Load(arquivo);
                    if (ndxf != null)
                    {
                        return ndxf;
                    }
                }
                catch (Exception)
                {
                    try
                    {
                        var tmbs = new netDxfMBS.DxfDocument();
                        tmbs.Load(arquivo);
                        if (tmbs != null)
                        {
                            ndxf = new DxfDocument();
                            var entt = tmbs.Converter();
                            foreach (var objeto in entt)
                            {
                                ndxf.Entities.Add(objeto);
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
        public static Layer GetLayer(this DxfDocument dxf, string name, netDxf.AciColor color, Linetype line)
        {
            var ss = dxf.Layers.ToList().Find(x => x.Name.ToUpper().Replace(" ", "") == name.ToUpper().Replace(" ", ""));
            if (ss != null)
            {
                return ss;
            }

            var retorno = new Layer(name.Replace(" ", "").ToUpper());
            retorno.Color = color;
            retorno.Linetype = line;
            return retorno;
        }
        public static double GetAltura(this Insert ins)
        {
            var p = ins.Block.Entities.ToList().FindAll(x => x is netDxf.Entities.Line).Select(x => x as netDxf.Entities.Line);
            var coords = p.Select(x => x.StartPoint).ToList();
            coords.AddRange(p.Select(x => x.EndPoint));
            if (coords.Count > 0)
            {
                var v1 = coords.Max(x => x.Y) * ins.Scale.Y;
                var v2 = coords.Min(x => x.Y) * ins.Scale.Y;
                return v1 - v2;
            }
            return 0;
        }
        public static void SetCor(this EntityObject obj, AciColor color)
        {
            color = color.Clone() as AciColor;
            if (obj is Line)
            {
                var s = obj as Line;
                s.Color = color;
            }
            else if (obj is Insert)
            {
                var s = obj as Insert;
                s.Color = color;
            }
            else if (obj is Text)
            {
                var s = obj as Text;
                s.Color = color;
            }
            else if (obj is MText)
            {
                var s = obj as MText;
                s.Color = color;
            }
            else if (obj is Arc)
            {
                var s = obj as Arc;
                s.Color = color;
            }
            else if (obj is Circle)
            {
                var s = obj as Circle;
                s.Color = color;
            }
            else if (obj is Ellipse)
            {
                var s = obj as Ellipse;
                s.Color = color;
            }
            else if (obj is Polyline2D)
            {
                var s = obj as Polyline2D;
                s.Color = color;
            }
            else if (obj is Polyline3D)
            {
                var s = obj as Polyline3D;
                s.Color = color;
            }

        }

        public static DxfDocument CloneAll(this DxfDocument dxf, DxfDocument destiny)
        {
            var entities = dxf.Layouts.GetReferences(netDxf.Objects.Layout.ModelSpaceName);
            foreach (var obj in dxf.Entities.All)
            {
                var entity = obj as EntityObject;
                var t1 = entity.Clone() as EntityObject;

                destiny.Entities.Add(t1);
            }
            return destiny;
        }
        public static List<EntityObject> CloneAll(this DxfDocument dxf)
        {
            var list = new List<EntityObject>();
            foreach (var obj in dxf.Entities.All)
            {
                var entity = obj as EntityObject;
                var t1 = entity.Clone() as EntityObject;

                list.Add(t1);
            }

            return list;
        }

        private static List<AttributeDefinition> GetModelAttributes(this DxfDocument dxf)
        {
            var atts = new List<AttributeDefinition>();
            var model = dxf.Blocks["*Model_Space"];

            foreach (var obj in model.AttributeDefinitions)
            {
                var entity = obj.Value as AttributeDefinition;


                atts.Add(entity.Clone() as AttributeDefinition);
            }

            return atts;
        }

        public static MText Get(this Text txt)
        {
            MText retorno = new MText();
            retorno.Style = txt.Style;
            retorno.Value = txt.Value;

            switch (txt.Alignment)
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
            retorno.Rotation = txt.Rotation;
            retorno.Position = new Vector3(txt.Position.X, txt.Position.Y, txt.Position.Z);
            retorno.Color = txt.Color;
            retorno.Height = txt.Height;
            retorno.Layer = txt.Layer;



            return retorno;
        }

        public static P3d GetPosition(this EntityObject entityObject)
        {
            switch (entityObject.Type)
            {
                case EntityType.Line:
                    return (entityObject as Line).StartPoint.ToP3d();

                case EntityType.Ellipse:
                    return (entityObject as Ellipse).Center.ToP3d();
                case EntityType.Arc:
                    return (entityObject as Arc).Center.ToP3d();

                case EntityType.Circle:
                    return (entityObject as Circle).Center.ToP3d();

                case EntityType.Spline:
                    return (entityObject as netDxf.Entities.Spline).PolygonalVertexes(10)[0].ToP3d();

                case EntityType.Dimension:
                    ///to-do: falta mapear os blocos das cotas e descobrir a posição central
                    var obj = entityObject as Dimension;
                    switch (obj.DimensionType)
                    {
                        case DimensionType.Linear:
                            var obj1 = entityObject as LinearDimension;
                            break;
                        case DimensionType.Aligned:
                            var obj2 = entityObject as AlignedDimension;


                            break;
                        case DimensionType.Angular:
                            var obj3 = entityObject as Angular2LineDimension;

                            break;
                        case DimensionType.Angular3Point:
                            var obj3a = entityObject as Angular3PointDimension;

                            break;
                        case DimensionType.Diameter:
                            var obj4 = entityObject as DiametricDimension;

                            break;
                        case DimensionType.Radius:
                            var obj5 = entityObject as RadialDimension;
                            break;

                        case DimensionType.Ordinate:
                            var obj6 = entityObject as OrdinateDimension;

                            break;
                        case DimensionType.ArcLength:
                            var obj7 = entityObject as ArcLengthDimension;
                            break;
                    }


                    return (entityObject as Dimension).DefinitionPoint.P3d();


                case EntityType.Tolerance:
                    return (entityObject as netDxf.Entities.Tolerance).Position.ToP3d();
                case EntityType.Shape:
                    return (entityObject as netDxf.Entities.Shape).Position.ToP3d();
                case EntityType.Image:
                    return (entityObject as Image).Position.ToP3d();

                case EntityType.Insert:
                    return (entityObject as Insert).Position.ToP3d();

                case EntityType.MText:
                    return (entityObject as MText).Position.ToP3d();
                case EntityType.Text:
                    return (entityObject as netDxf.Entities.Text).Position.ToP3d();

                case EntityType.Point:
                    return (entityObject as netDxf.Entities.Point).Position.ToP3d();
                case EntityType.Underlay:
                    return (entityObject as netDxf.Entities.Underlay).Position.ToP3d();

                case EntityType.Ray:
                    return (entityObject as netDxf.Entities.Ray).Origin.ToP3d();

                case EntityType.Trace:
                    return (entityObject as netDxf.Entities.Trace).FirstVertex.P3d();

                case EntityType.Viewport:
                    return (entityObject as netDxf.Entities.Viewport).UcsOrigin.ToP3d();
                case EntityType.XLine:
                    return (entityObject as netDxf.Entities.XLine).Origin.ToP3d();


                case EntityType.Face3D:
                    return (entityObject as Face3D).FirstVertex.ToP3d();
                case EntityType.Solid:
                    return (entityObject as netDxf.Entities.Solid).FirstVertex.P3d();


                case EntityType.Polyline2D:
                    return (entityObject as netDxf.Entities.Polyline2D).Vertexes[0].P3d();
                case EntityType.Polyline3D:
                    return (entityObject as netDxf.Entities.Polyline3D).Vertexes[0].ToP3d();
                case EntityType.PolyfaceMesh:
                    return (entityObject as netDxf.Entities.PolygonMesh).Vertexes[0].ToP3d();
                case EntityType.PolygonMesh:
                    return (entityObject as netDxf.Entities.PolygonMesh).Vertexes[0].ToP3d();
                case EntityType.Leader:
                    return (entityObject as Leader).Vertexes[0].P3d();
                case EntityType.Mesh:
                    return (entityObject as Mesh).Vertexes[0].ToP3d();
                case EntityType.MLine:
                    return (entityObject as MLine).Vertexes[0].Position.P3d();


                case EntityType.Wipeout:
                    return (entityObject as Wipeout).Owner.Origin.ToP3d();
                case EntityType.Hatch:
                    return (entityObject as Hatch).Owner.Origin.ToP3d();
            }

            return new P3d();
        }

        public static double Comprimento(this Line line)
        {

            return line.StartPoint.ToP3d().Distancia(line.EndPoint.ToP3d());
        }
        public static double Angulo(this Line line)
        {
            return line.StartPoint.ToP3d().GetAngulo(line.EndPoint.ToP3d());
        }


        public static List<P3d> ToP3d(this List<Vector2> vector2s)
        {
            return vector2s.Select(x => new desenho.P3d(x.X, x.Y)).ToList();
        }
        public static P3d ToP3d(this Vector3 vector3)
        {
            return new P3d(vector3.X, vector3.Y, vector3.Z);
        }
        public static P3d P3d(this Polyline2DVertex vector3)
        {
            return new P3d(vector3.Position.X, vector3.Position.Y, 0);
        }
        public static P3d P3d(this Vector2 vector3)
        {
            return new P3d(vector3.X, vector3.Y, 0);
        }


        public static double DistMaxX(this Line l, P3d L2)
        {
            var dists = new List<double>();
            dists.Add(Math.Abs(l.StartPoint.X - L2.X));
            dists.Add(Math.Abs(l.EndPoint.X - L2.X));
            return dists.Max();
        }
        public static double DistMaxY(this Line l, P3d L2)
        {
            var dists = new List<double>();
            dists.Add(Math.Abs(l.StartPoint.Y - L2.Y));
            dists.Add(Math.Abs(l.EndPoint.Y - L2.Y));
            return dists.Max();
        }
        public static double DistMinX(this Line l, P3d L2)
        {
            var dists = new List<double>();
            dists.Add(Math.Abs(l.StartPoint.X - L2.X));
            dists.Add(Math.Abs(l.EndPoint.X - L2.X));
            return dists.Min();
        }
        public static double DistMinY(this Line l, P3d L2)
        {
            List<double> dists = new List<double>();
            dists.Add(Math.Abs(l.StartPoint.Y - L2.Y));
            dists.Add(Math.Abs(l.EndPoint.Y - L2.Y));
            return dists.Min();
        }
        public static double DistX(this Insert l, P3d L2)
        {
            return Math.Abs(l.Position.X - L2.X);
        }
        public static double DistX(this Text txt, P3d L2)
        {
            return Math.Abs(txt.Position.X - L2.X);
        }
        public static double DistY(this Insert l, P3d L2)
        {
            return Math.Abs(l.Position.Y - L2.Y);
        }
        public static double DistY(this Text txt, P3d L2)
        {
            return Math.Abs(txt.Position.Y - L2.Y);
        }


        public static bool Horizontal(this Line l)
        {
            return Math.Abs(l.Angulo()) == 0 | Math.Abs(l.Angulo()) == 180;
        }

        public static bool Vertical(this Line l)
        {
            return Math.Abs(l.Angulo()) == 90 | Math.Abs(l.Angulo()) == 270;
        }
        public static double MinX(this Line l)
        {
            return l.StartPoint.X < l.EndPoint.X ? l.StartPoint.X : l.EndPoint.X;
        }


        public static double MinX(this DxfDocument dxf)
        {
            var coords = dxf.GetObjetos().SelectMany(x => x.GetBounds()).ToList();
            if (coords.Count > 0)
            {
                return coords.Min(x => x.X);
            }
            return 0;
        }
        public static double MinY(this DxfDocument dxf)
        {
            var coords = dxf.GetObjetos().SelectMany(x => x.GetBounds()).ToList();
            if (coords.Count > 0)
            {
                return coords.Min(x => x.Y);
            }
            return 0;
        }
        public static double MaxY(this DxfDocument dxf)
        {
            var coords = dxf.GetObjetos().SelectMany(x => x.GetBounds()).ToList();
            if (coords.Count > 0)
            {
                return coords.Max(x => x.Y);
            }
            return 0;
        }
        public static double MaxX(this DxfDocument dxf)
        {
            var coords = dxf.GetObjetos().SelectMany(x => x.GetBounds()).ToList();
            if (coords.Count > 0)
            {
                return coords.Max(x => x.X);
            }
            return 0;
        }

        public static P3d GetTopDir(this DxfDocument dxf)
        {
            var X = MaxX(dxf);
            var Y = MaxY(dxf);
            return new P3d(X, Y);
        }
        public static P3d GetBotEsq(this DxfDocument dxf)
        {
            var X = MinX(dxf);
            var Y = MinY(dxf);

            return new P3d(X, Y);
        }
        public static List<EntityObject> GetTextos(this DxfDocument dxf, string contem = null)
        {
            var retorno = new List<netDxf.Entities.EntityObject>();
            if (contem == null)
            {
                retorno.AddRange(dxf.Entities.MTexts);
                retorno.AddRange(dxf.Entities.Texts);
            }
            else
            {
                retorno.AddRange(dxf.Entities.MTexts.ToList().FindAll(x => x.Value.ToUpper().Contains(contem.ToUpper())));
                retorno.AddRange(dxf.Entities.MTexts.ToList().FindAll(x => x.Value.ToUpper().Contains(contem.ToUpper())));
            }


            return retorno;
        }
        public static List<EntityObject> GetObjetos(this DxfDocument dxf)
        {
            var retorno = new List<netDxf.Entities.EntityObject>();
            retorno.AddRange(dxf.Entities.Arcs);
            retorno.AddRange(dxf.Entities.Circles);
            retorno.AddRange(dxf.Entities.Dimensions);
            retorno.AddRange(dxf.Entities.Ellipses);
            retorno.AddRange(dxf.Entities.Faces3D);
            retorno.AddRange(dxf.Entities.Hatches);
            retorno.AddRange(dxf.Entities.Images);
            retorno.AddRange(dxf.Entities.Inserts);
            retorno.AddRange(dxf.Entities.Leaders);
            retorno.AddRange(dxf.Entities.Lines);
            retorno.AddRange(dxf.Entities.Polylines2D);
            retorno.AddRange(dxf.Entities.Meshes);
            retorno.AddRange(dxf.Entities.MLines);
            retorno.AddRange(dxf.Entities.MTexts);
            retorno.AddRange(dxf.Entities.Points);
            retorno.AddRange(dxf.Entities.PolyfaceMeshes);
            retorno.AddRange(dxf.Entities.Polylines3D);
            retorno.AddRange(dxf.Entities.Rays);
            retorno.AddRange(dxf.Entities.Shapes);
            retorno.AddRange(dxf.Entities.Solids);
            retorno.AddRange(dxf.Entities.Splines);
            retorno.AddRange(dxf.Entities.Texts);
            retorno.AddRange(dxf.Entities.Tolerances);
            retorno.AddRange(dxf.Entities.XLines);

            return retorno;
        }
        public static List<P3d> GetBounds(this EntityObject obj, int Precisao = 30)
        {
            if (Precisao < 5)
            {
                Precisao = 5;
            }
            var origem = obj.GetPosition();
            var single_list = new List<P3d>();
            switch (obj.Type)
            {
                case EntityType.Insert:
                    single_list.AddRange((obj as netDxf.Entities.Insert).Explode().SelectMany(x => x.GetBounds(Precisao)));
                    break;
                case EntityType.Polyline2D:
                    single_list.AddRange((obj as netDxf.Entities.Polyline2D).Explode().SelectMany(x => x.GetBounds(Precisao)));
                    break;
                case EntityType.Polyline3D:
                    single_list.AddRange((obj as netDxf.Entities.Polyline3D).Explode().SelectMany(x => x.GetBounds(Precisao)));
                    break;
                case EntityType.Face3D:
                case EntityType.Dimension:
                case EntityType.Image:
                case EntityType.Text:
                case EntityType.Tolerance:
                case EntityType.Trace:
                case EntityType.Underlay:
                case EntityType.Viewport:
                case EntityType.Wipeout:
                case EntityType.XLine:
                case EntityType.MText:
                case EntityType.Point:
                case EntityType.Solid:
                case EntityType.Ray:
                case EntityType.Hatch:
                case EntityType.Shape:
                    single_list.Add(origem);
                    break;
                case EntityType.Arc:
                    single_list.AddRange((obj as netDxf.Entities.Arc).PolygonalVertexes(Precisao).Select(x => x.P3d().Mover(origem)));
                    break;
                case EntityType.Circle:
                    single_list.AddRange((obj as netDxf.Entities.Circle).PolygonalVertexes(Precisao).Select(x => x.P3d().Mover(origem)));
                    break;
                case EntityType.Ellipse:
                    single_list.AddRange((obj as netDxf.Entities.Ellipse).PolygonalVertexes(Precisao).Select(x => x.P3d().Mover(origem)));
                    break;

                //talvez este cara não precise mover. mas só testando.
                case EntityType.Spline:
                    single_list.AddRange((obj as netDxf.Entities.Spline).PolygonalVertexes(Precisao).Select(x => x.ToP3d().Mover(origem)));
                    break;


                case EntityType.Line:
                    var l = obj as Line;
                    single_list.Add(l.StartPoint.ToP3d());
                    single_list.Add(l.EndPoint.ToP3d());
                    break;

                case EntityType.Leader:
                    single_list.AddRange((obj as netDxf.Entities.Leader).Vertexes.Select(x => x.P3d()));
                    break;
                case EntityType.Mesh:
                    single_list.AddRange((obj as netDxf.Entities.Mesh).Vertexes.Select(x => x.ToP3d()));
                    break;
                case EntityType.MLine:
                    single_list.AddRange((obj as netDxf.Entities.MLine).Vertexes.Select(x => x.Position.P3d()));
                    break;
                case EntityType.PolyfaceMesh:
                    single_list.AddRange((obj as netDxf.Entities.PolygonMesh).Vertexes.Select(x => x.ToP3d()));
                    break;
                case EntityType.PolygonMesh:
                    single_list.AddRange((obj as netDxf.Entities.PolygonMesh).Vertexes.Select(x => x.ToP3d()));
                    break;



            }

            return single_list;
        }

        public static string GetAtributo(this Insert insert, string atributo)
        {
            var igual = insert.Attributes.ToList().Find(x => x.Tag.ToUpper() == atributo.ToUpper());
            if (igual != null)
            {
                return igual.Value;
            }

            return "";
        }

        public static double MaxX(this Line l)
        {
            return l.StartPoint.X > l.EndPoint.X ? l.StartPoint.X : l.EndPoint.X;
        }
        public static double MinY(this Line l)
        {
            return l.StartPoint.X < l.EndPoint.Y ? l.StartPoint.Y : l.EndPoint.Y;
        }
        public static double MaxY(this Line l)
        {
            return l.StartPoint.Y > l.EndPoint.Y ? l.StartPoint.Y : l.EndPoint.Y;
        }
        public static double AnguloAbs(this Line l)
        {
            double ang = Angulo(l);
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
