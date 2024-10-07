using Conexoes;
using DLM.desenho;
using DLM.vars;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM.desenho
{
    public static class Dxf
    {
        public static MText GetTexto(List<string> Linhas, Vector2 posicao, double tam, TextStyle style)
        {
            MText retorno = new MText();
            retorno.Height = tam;
            retorno.Style = style;
            retorno.Value = string.Join(@"\P", Linhas);
            retorno.Position = new Vector3(posicao.X, posicao.Y, 0);

            return retorno;
        }
        public static List<Vector2> ToVector2(List<System.Windows.Point> Pontos)
        {
            return Pontos.Select(x => new Vector2(x.X, x.Y)).ToList();
        }
        public static List<P3d> GetOrigens(List<EntityObject> Objetos, bool cotas = false)
        {
            var Retorno = new List<P3d>();
            foreach (var ob in Objetos)
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

        public static DimensionStyle GetEstilo(string Nome, double escala)
        {
            DimensionStyle retorno = new DimensionStyle(Nome);
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
        public static DimensionStyle GetEstilo(DxfDocument doc, string Nome, double escala)
        {
            var s = doc.DimensionStyles.ToList().Find(X => X.Name.ToUpper().Replace(" ", "") == Nome.ToUpper().Replace(" ", ""));
            if (s != null)
            {
                return s;
            }
            var st = GetEstilo(Nome.ToUpper().Replace(" ", ""), escala);
            return st;
        }
        public static List<OrdinateDimension> CotasAcumuladas(List<P3d> Origens, double offset, Sentido sentido = Sentido.Horizontal, Layer Layer = null, DimensionStyle estilo = null, bool inverter = false)
        {
            List<OrdinateDimension> Retorno = new List<OrdinateDimension>();
            Origens = Origens.Select(x => x.Tratar()).ToList();
            if (sentido == Sentido.Horizontal)
            {
                var ps = Origens.GetHorizontaisTop();
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
                var ps = Origens.GetVerticaisRight();
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

                    Insert p = new Insert(block);
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
        public static Text Texto(P3d Origem, string valor, double tamanho = 10, Layer layer = null, TextStyle estilo = null, double angulo = 0, AciColor cor = null, TextAlignment alinhamento = TextAlignment.MiddleCenter)
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
                return Dxf.Oblongo(Diametro, Oblongo, Angulo, Origem.X, Origem.Y, Layer);
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


                Vetores.Add(new Vector3(Math.Round(p3d.X, 10), Math.Round(p3d.Y, 10), 0));
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
    }
}
