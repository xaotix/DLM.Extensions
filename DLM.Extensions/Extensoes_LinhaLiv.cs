using Clipper2Lib;
using Conexoes;
using DLM.desenho;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Windows;
using System.Windows.Media;

namespace DLM.cam
{
    public static class ExtensoesLiv
    {

        public static Liv MoverY(this Liv p1, double valor)
        {
            var pt = p1.Clonar();
            pt.Origem = pt.Origem.MoverY(valor);
            return pt;
        }
        public static Liv MoverX(this Liv pt, double valor)
        {
            var p1 = pt.Clonar();
            p1.Origem = p1.Origem.MoverX(valor);
            return p1;
        }
        public static Liv Mover(this Liv p1, double angulo, double distancia, int decimais = 10)
        {
            var pt = p1.Clonar();
            pt.Origem = pt.Origem.Mover(angulo, distancia, decimais);
            return pt;
        }

        public static Liv Mover(this Liv p1, P3d distancia)
        {
            var pt = p1.Clonar();
            pt.Origem = pt.Origem.Mover(distancia);
            return pt;
        }
        public static Liv Round(this Liv p1, int decimais = 2)
        {
            var pt = p1.Clonar();
            pt.Raio = pt.Raio.Round(decimais);
            pt.Ang1 = pt.Ang1.Round(decimais);
            pt.Ang2 = pt.Ang2.Round(decimais);
            pt.Origem = pt.Origem.Round(decimais);

            return pt;
        }
        public static Liv SetY(this Liv p1, double valor)
        {
            var pt = p1.Clonar();
            pt.Origem = pt.Origem.SetY(valor);
            return pt;
        }
        public static Liv SetX(this Liv p1, double valor)
        {
            var pt = p1.Clonar();
            pt.Origem = pt.Origem.SetX(valor);
            return pt;
        }


        public static List<Liv> ToLiv(this List<P3d> p3Ds)
        {
            return new List<Liv>(p3Ds.Select(x => new Liv(x)));
        }

        public static List<Liv> Offset(this List<Liv> livs, double offset, bool fechar = false)
        {
            var retorno = new List<Liv>();
            var seqmento = livs.Segmentar();


            var rotacao = seqmento.GetQuadranteERotacao();
            var linhas = seqmento.GetLinhas();
            var ang_min = 3.0;
            foreach (var liv in linhas)
            {
                var ang = liv.Anterior.Angulo(liv).Abs();
                var angulo = ang;
                while (angulo > 90)
                {
                    angulo = angulo - 180;
                }
                angulo = angulo.Abs();
                if (angulo >= ang_min)
                {
                    var ponto = liv.OffSetInterno(offset, rotacao);
                    retorno.Add(new Liv(ponto));
                }
            }
            if (fechar && retorno.Count > 0)
            {
                retorno.Add(retorno.First().Clonar());
            }
            return retorno;
        }

        public static List<P3d> ToP3d(this List<Liv> livs)
        {
            var segment = livs.Segmentar();

            return segment.Select(x => x.Origem).ToList();
        }



        public static Liv ResetAngulo(this Liv p1, int round = 2)
        {
            var pt = p1.Clonar();
            pt.Raio = 0;
            pt.Ang1 = 0;
            pt.Ang2 = 0;
            pt.Origem = pt.Origem.Round(round);

            return pt;
        }
        public static List<Liv> SegmentosArco(this Liv liv)
        {
            var _Segmentos = new List<Liv>();

            if (liv.Raio > 0)
            {
                int segmentos = (int)(liv.Anterior.Origem.Distancia(liv.Proximo.Origem) / Cfg.Init.CAM_Multiplo_Segmentos).Round(0);

                if (segmentos < Cfg.Init.CAM_Min_Segmentos)
                {
                    segmentos = Cfg.Init.CAM_Min_Segmentos;
                }
                else if (segmentos > Cfg.Init.CAM_Max_Segmentos)
                {
                    segmentos = Cfg.Init.CAM_Max_Segmentos;
                }

                var normang1 = liv.Ang1.Round(0).Normalizar();
                var normang2 = liv.Ang2.Round(0).Normalizar();
                var angulo_arco = liv.Ang2 - liv.Ang1;
                if (liv.Ang1 < 0 && liv.Ang2 < 0)
                {
                    angulo_arco = normang2 - normang1;
                }
                else if (liv.Ang2 < 0 && liv.Ang1 >= 0)
                {
                    angulo_arco = normang2 - liv.Ang1;
                }
                else if (liv.Ang1 < 0 && liv.Ang2 >= 0 && liv.Ang2 >= normang1)
                {
                    angulo_arco = liv.Ang2 - normang1;
                }


                var angseg = angulo_arco / segmentos;
                var angseg_old = (liv.Ang2 - liv.Ang1) / segmentos;

                var ang0 = liv.Ang1;

                if (liv.Ang1 > liv.Ang2)
                {

                }

                for (int i = 1; i < segmentos; i++)
                {
                    _Segmentos.Add(liv.Mover(ang0, liv.Raio, 1).ResetAngulo(1));
                    ang0 = ang0 + angseg;
                }

                _Segmentos.Add(liv.Mover(ang0, liv.Raio, 1).ResetAngulo(1));

                if (_Segmentos.Count > 0)
                {
                    var dist_anterior = _Segmentos.First().Origem.Distancia(liv.Anterior.Origem);
                    var dist_proximo = _Segmentos.First().Origem.Distancia(liv.Proximo.Origem);
                    if (dist_anterior > dist_proximo)
                    {
                        _Segmentos.Reverse();
                    }
                    //remove os segmentos das pontas.
                    if (_Segmentos.Count > 3)
                    {
                        _Segmentos.RemoveAt(0);
                        _Segmentos.RemoveAt(_Segmentos.Count - 1);
                    }
                }
                _Segmentos.Aninhar();
            }
            else
            {
                _Segmentos.Add(liv);
            }
            return _Segmentos;
        }

        private static void ProcessarFigura(this List<Point> points, GeneralTransform transform, PathGeometry geometry)
        {
            if (points.Count == 0) return;
            var result = new PolyLineSegment();
            var prev = points[0];
            for (int i = 1; i < points.Count; ++i)
            {
                var current = points[i];
                if (current == prev) continue;
                result.Points.Add(transform.Transform(current));
                prev = current;
            }
            if (result.Points.Count == 0) return;
            geometry.Figures.Add(new PathFigure(transform.Transform(points[0]), new PathSegment[] { result }, true));
        }
        private static void ProcessarLinha(this Point pt1, Point pt2, List<Point> esq, List<Point> dir)
        {
            if (pt1.X >= 0 && pt2.X >= 0)
            {
                dir.Add(pt1);
                dir.Add(pt2);
            }
            else if (pt1.X < 0 && pt2.X < 0)
            {
                esq.Add(pt1);
                esq.Add(pt2);
            }
            else if (pt1.X < 0)
            {
                double c = (Math.Abs(pt1.X) * Math.Abs(pt2.Y - pt1.Y)) / Math.Abs(pt2.X - pt1.X);
                double y = pt1.Y + c * Math.Sign(pt2.Y - pt1.Y);
                var p = new Point(0, y);
                esq.Add(pt1);
                esq.Add(p);
                dir.Add(p);
                dir.Add(pt2);
            }
            else
            {
                double c = (Math.Abs(pt1.X) * Math.Abs(pt2.Y - pt1.Y)) / Math.Abs(pt2.X - pt1.X);
                double y = pt1.Y + c * Math.Sign(pt2.Y - pt1.Y);
                var p = new Point(0, y);
                dir.Add(pt1);
                dir.Add(p);
                esq.Add(p);
                esq.Add(pt2);
            }
        }
        public static void Quebrar(this Geometry geo, Point pt1, Point pt2, out PathGeometry PedacoEsq, out PathGeometry PedacoDir)
        {
            double Angulo = 360.0 + 90.0 - (180.0 / Math.PI * Math.Atan2(pt2.Y - pt1.Y, pt2.X - pt1.X));
            var tranform = new TransformGroup();
            tranform.Children.Add(new TranslateTransform(-pt1.X, -pt1.Y));
            tranform.Children.Add(new RotateTransform(Angulo));
            var inverse = tranform.Inverse;
            PedacoEsq = new PathGeometry();
            PedacoDir = new PathGeometry();
            foreach (var figure in geo.GetFlattenedPathGeometry().Figures)
            {
                var left = new List<Point>();
                var right = new List<Point>();
                var lastPt = tranform.Transform(figure.StartPoint);
                try
                {
                    foreach (PolyLineSegment segment in figure.Segments)
                    {
                        foreach (var currentPtOrig in segment.Points)
                        {
                            var currentPt = tranform.Transform(currentPtOrig);
                            ProcessarLinha(lastPt, currentPt, left, right);
                            lastPt = currentPt;
                        }
                    }
                    ProcessarFigura(left, inverse, PedacoEsq);
                    ProcessarFigura(right, inverse, PedacoDir);
                }
                catch (Exception)
                {
                }

            }
        }
        public static List<Liv> ToLiv(this PathGeometry Geometria, bool ajustaCoords = true)
        {
            var Pontos = new List<Point>();

            foreach (PathFigure figure in Geometria.Figures)
            {
                Pontos.Add(figure.StartPoint);
                foreach (PathSegment segment in figure.Segments)
                {
                    foreach (var point in ((PolyLineSegment)segment).Points)
                    {
                        Pontos.Add(point);
                    }
                }
            }
            var Retorno = new List<Liv>();
            foreach (var p in Pontos)
            {
                var xt = new Liv();
                if (p.Y > 0)
                {
                    xt.Origem.Y = -p.Y;
                }
                else
                {
                    xt.Origem.Y = p.Y;
                }
                xt.Origem.X = p.X;
                //xt.Y = p.Y;
                Retorno.Add(xt);
            }
            if (ajustaCoords)
            {
                Retorno = Retorno.Zerar();
            }
            return Retorno;
        }
        public static PathGeometry GetPathGeometry(this List<Liv> Formato, bool ajustaCoords = true)
        {
            try
            {
                var livs = new List<Liv>();
                livs.AddRange(Formato);

                if (livs.Count == 4)
                {
                    if (livs.First().GetCid() != livs.Last().GetCid())
                    {
                        livs.Add(livs.First().Clonar());
                    }
                }

                if (ajustaCoords)
                {
                    double xmin = 0;
                    double ymin = 0;
                    livs = livs.Zerar(out xmin, out ymin);

                }

                var figura = new PathFigure();
                if (livs.Count > 0)
                {
                    double X0 = livs[0].Origem.X;
                    double Y0 = Math.Abs(livs[0].Origem.Y);

                    figura.StartPoint = new Point(X0, Y0);
                    for (int i = 0; i < livs.Count; i++)
                    {
                        if (livs[i].Raio == 0)
                        {
                            figura.Segments.Add(new LineSegment(new Point(livs[i].Origem.X, Math.Abs(livs[i].Origem.Y)), true));
                        }

                    }

                }

                var geometria = new PathGeometry();
                geometria.Figures.Add(figura);
                return geometria;
            }
            catch (Exception)
            {

                return null;
            }

        }
        public static List<Liv> ChapaParaMesa(this List<Liv> livs, double algura, double largura_mesa)
        {
            var retorno = new List<Liv>();
            if (livs.Count >= 3)
            {
                var meio = largura_mesa / 2;
                foreach (var liv in livs)
                {
                    var nliv = new Liv(liv.Origem.X, algura, liv.Origem.Y + meio);
                    retorno.Add(nliv);
                }
            }

            return retorno;
        }
        public static List<Liv> MesaParaChapa(this List<Liv> Mesa, bool zerarX = true)
        {
            var Retorno = new List<Liv>();
            if (!Mesa.isMesa())
            {
                var ms = Mesa.Select(x => x.Clonar()).ToList();
                ms.Aninhar();
                return ms;
            }
            foreach (Liv L in Mesa)
            {
                Retorno.Add(new Liv(L.Origem.X, L.Origem.Z));
            }


            double MinX = Retorno.Min(x => x.Origem.X);
            double MaxX = Retorno.Max(x => x.Origem.X);

            double MinY = Retorno.Min(x => x.Origem.Y);
            double MaxY = Retorno.Max(x => x.Origem.Y);

            if (MinX != 0 && zerarX)
            {
                foreach (Liv L in Retorno)
                {
                    L.Origem.X = L.Origem.X - MinX;
                }
            }

            if (MaxY > 0)
            {
                foreach (Liv L in Retorno)
                {
                    L.Origem.Y = L.Origem.Y - MaxY;
                }
            }

            return Retorno;
        }
        public static void QuebrarHorizontal(this List<Liv> Formato, double Profundidade, out List<Liv> Lado1, out List<Liv> Lado2)
        {
            var p1 = new P3d(0, Profundidade);
            var p2 = new P3d(Formato.Comprimento(), Profundidade);
            Formato.Quebrar(p1, p2, out Lado1, out Lado2);
        }
        public static void Quebrar(this List<Liv> Formato, P3d p1, P3d p2, out List<Liv> Lado1, out List<Liv> Lado2, bool ajustaCoords = true, bool ajusta_retorno_coords = true)
        {
            PathGeometry inferior;
            PathGeometry superior;

            List<Liv> pts = new List<Liv>();
            if (Formato.Count > 0)
            {
                pts.AddRange(Formato);
                //adiciona o primeiro ponto por ultimo
                if (pts[0].GetCid() != pts[pts.Count - 1].GetCid())
                {
                    pts.Add(Formato[0]);
                }
            }


            pts.GetPathGeometry(ajustaCoords).Quebrar(p1.GetPoint(), p2.GetPoint(), out inferior, out superior);
            Lado2 = inferior.ToLiv(ajusta_retorno_coords);
            Lado1 = superior.ToLiv(ajusta_retorno_coords);
        }

        public static void DividirHorizontal(this List<Liv> Formato, out List<Liv> LadoSuperior, out List<Liv> LadoInferior)
        {
            LadoSuperior = new List<Liv>();
            LadoInferior = new List<Liv>();
            double MeioY = Math.Abs(Largura(Formato)) / 2;
            for (int i = 0; i < Formato.Count; i++)
            {
                if (Math.Abs(Formato[i].Origem.Y) > MeioY)
                {
                    LadoInferior.Add(Formato[i]);
                }
                else
                {
                    LadoSuperior.Add(Formato[i]);
                }
            }
        }
        public static double ContraFlecha(this List<Liv> livs)
        {
            if (livs.Count > 0)
            {
                return livs.Max(x => x.GetContraFlecha().Abs());
            }
            return 0;
        }
        public static List<Liv> Zerar(this List<Liv> chapa)
        {
            double p1, p2;
            return Zerar(chapa, out p1, out p2);
        }
        public static List<Liv> Zerar(this List<Liv> Chapa, out double MinX, out double MaxY)
        {
            List<Liv> Retorno = new List<Liv>();
            MinX = 0;
            MaxY = 0;
            double MinY = 0;
            double MaxX = 0;
            double MinZ = 0;
            double MaxZ = 0;

            if (Chapa.Count == 0)
            {

                return new List<Liv>();
            }

            try
            {
                MinX = Chapa.Min(x => x.Origem.X);
                MaxX = Chapa.Max(x => x.Origem.X);

                MinY = Chapa.Min(x => x.Origem.Y);
                MaxY = Chapa.Max(x => x.Origem.Y);

                MinZ = Chapa.Min(x => x.Origem.Z);
                MaxZ = Chapa.Max(x => x.Origem.Z);

                foreach (var L in Chapa)
                {
                    Retorno.Add(L.Clonar());
                }

                if (MinX != 0)
                {
                    foreach (var L in Retorno)
                    {
                        L.Origem.X = L.Origem.X - MinX;
                    }
                }

                if (MaxY != 0)
                {
                    foreach (var L in Retorno)
                    {
                        L.Origem.Y = L.Origem.Y - MaxY;
                    }
                }

                if (MinZ != 0)
                {
                    foreach (var L in Retorno)
                    {
                        L.Origem.Z = L.Origem.Z - MinZ;
                    }
                }
            }
            catch (Exception)
            {

            }

            return Retorno;
        }
        public static List<DLM.cam.Addons.IntPoint> GetIntPoints(this List<DLM.cam.Liv> livs, double offsetX = 0, double offsetY = 0, double multiplicar = 0)
        {
            var ret = livs.Select(x => new DLM.cam.Addons.IntPoint((x.Origem.X + offsetX) * multiplicar, (x.Origem.Y + offsetY) * multiplicar)).ToList();
            return ret;
        }
        public static List<Liv> Segmentar(this List<Liv> Liv)
        {

            var original = new List<Liv>();
            var Retorno = new List<Liv>();
            original.AddRange(Liv.Select(x => x.Clonar()));
            original = original.RemoveSobrePostos();
            original.Aninhar();

            if (ContraFlecha(original) > 0 && !isMesa(original))
            {
                var geometria = new List<Liv>();
                foreach (var liv in original)
                {
                    if (liv.Raio > 0)
                    {

                        var valores = liv.SegmentosArco();
                        geometria.AddRange(valores);
                    }
                    else
                    {
                        if (geometria.Count > 0)
                        {
                            //ignora linhas que tenham distancia menor
                            if (geometria.Last().Origem.Distancia(liv.Origem).Abs() < 3)
                            {
                                continue;
                            }
                        }
                        geometria.Add(liv);
                    }
                }

                if (geometria.Count > 2)
                {
                    Retorno.AddRange(geometria);
                }
                else
                {
                    Retorno.AddRange(original);
                }
            }
            else
            {
                Retorno.AddRange(original);
            }

            Retorno.Aninhar();

            return Retorno;
        }

        /// <summary>
        /// Obs.: As coordenadas precisam estar em sequência dos pontos dos polígonos.
        /// </summary>
        /// <param name="Pts"></param>
        /// <returns></returns>
        public static double Area(this List<Liv> Pts)
        {

            bool Mesa = Pts.isMesa();

            double Retorno = 0;
            if (Pts.Count > 0)
            {
                double xmin = 0;
                double ymin = 0;
                var points = new List<Liv>();

                points.AddRange(Pts);


                if (Mesa)
                {
                    points = Pts.Segmentar().MesaParaChapa().Zerar(out xmin, out ymin);
                }
                else
                {
                    points = Pts.Segmentar().Zerar(out xmin, out ymin);
                }



                points = points.RemoveSobrePostos();

                //fechar o contorno
                if (points.Last().GetCid() != points.First().GetCid())
                {
                    points.Add(points.First().Clonar());
                }

                var area_win = points.GetPath().Area().Abs();

                //Clipper2Lib.Clipper.Area(points.GetPath());

                //var area_clipper = DLM.cam.Addons.Clipper.Area(points.Select(xs => new Addons.IntPoint(xs.Origem.X * 1000, xs.Origem.Y * 1000)).ToList()) / 1000 / 1000;
                //var area_win = points.GetPathGeometry().GetArea();

                return area_win;
            }

            return Retorno;
        }
        public static double Comprimento(this List<Liv> Pts)
        {
            double Retorno = 0;
            if (Pts.Count > 1)
            {
                double MaxX = Pts.FindAll(x => x.Angulo == 0).OrderBy(x => x.Origem.X).Select(x => x.Origem.X).Last();
                double MinX = Pts.FindAll(x => x.Angulo == 0).OrderBy(x => x.Origem.X).Select(x => x.Origem.X).First();
                Retorno = MaxX - MinX;
            }
            return Retorno;
        }
        public static bool TemRecorte(this List<Liv> Formato)
        {
            bool Retorno = false;
            try
            {
                double MinX = Formato.Select(x => x.Origem.X).ToList().OrderBy(x => x).First();
                double MaxX = Formato.Select(x => x.Origem.X).ToList().OrderBy(x => x).Last();
                double MinY = Formato.Select(x => x.Origem.Y).ToList().OrderBy(x => x).First();
                double MaxY = Formato.Select(x => x.Origem.Y).ToList().OrderBy(x => x).Last();

                List<double> XX = Formato.Select(x => x.Origem.X).Distinct().ToList();
                List<double> YY = Formato.Select(x => x.Origem.Y).Distinct().ToList();

                //verifica em X
                if (XX.FindAll(x => x != MinX && x != MaxX).Count > 0)
                {
                    Retorno = true;
                }

                //Verifica em Y
                if (YY.FindAll(y => y != MinY && y != MaxY).Count > 0)
                {
                    Retorno = true;
                }

                //círculos
                if (Formato.FindAll(x => x.Raio > 0).Count > 0)
                {
                    Retorno = true;
                }
            }
            catch (Exception)
            {

                //throw;
            }


            return Retorno;


        }

        public static bool isMesa(this List<Liv> Liv)
        {
            if (Liv.Count == 0) { return false; }

            var mesa = Math.Abs(Liv.Max(x => x.Origem.Y) - Liv.Min(x => x.Origem.Y));

            return mesa == 0;
        }
        public static List<LinhaLiv> GetLinhas(this List<Liv> Liv)
        {
            var retorno = new List<LinhaLiv>();
            var lista = Liv.RemoveSobrePostos();
            if (lista.Count > 0)
            {
                for (int i = 1; i < lista.Count; i++)
                {
                    retorno.Add(new LinhaLiv(lista[i - 1], lista[i]));
                }
                retorno.Add(new LinhaLiv(lista.Last(), lista.First()));
            }

            retorno.Encadear();

            return retorno;
        }
        public static void Aninhar(this List<Liv> _Liv)
        {

            for (int i = 0; i < _Liv.Count; i++)
            {
                if (i == 0)
                {
                    _Liv[i].Anterior = _Liv[_Liv.Count - 1];
                }
                else
                {
                    _Liv[i].Anterior = _Liv[i - 1];
                }

                if (i == _Liv.Count - 1)
                {
                    _Liv[i].Proximo = _Liv[0];
                }
                else
                {
                    _Liv[i].Proximo = _Liv[i + 1];
                }
            }
        }
        public static double Largura(this List<Liv> Pts)
        {
            double Retorno = 0;
            var Mesa = Pts.isMesa();
            if (Pts.Count > 1)
            {
                if (Mesa)
                {
                    double MaxX = Pts.OrderBy(x => x.Origem.Z).Select(x => x.Origem.Z).Last();
                    double MinX = Pts.OrderBy(x => x.Origem.Z).Select(x => x.Origem.Z).First();
                    Retorno = MaxX - MinX;
                }
                else
                {
                    double MaxX = Math.Abs(Pts.OrderBy(x => x.Origem.Y).Select(x => x.Origem.Y).First());
                    double MinX = Math.Abs(Pts.OrderBy(x => x.Origem.Y).Select(x => x.Origem.Y).Last());
                    Retorno = MaxX - MinX;
                }
            }

            return Retorno;
        }
    }
    public static class ExtensoesLinhaLiv
    {
        public static P3d GetPonto(this LinhaLiv linha, double dist)
        {
            return linha.P1.Mover(linha.Angulo, dist);
        }
        public static P3d Interseccao(this LinhaLiv linha, P3d obj, double angulo = 90)
        {
            var Liv2 = new LinhaLiv(obj, obj.Mover(angulo, 100));
            return linha.P1.Interseccao(linha.P2, Liv2.P1, Liv2.P2);
        }
        public static P3d Interseccao(this LinhaLiv l1, LinhaLiv l2)
        {
            return l1.P1.Interseccao(l1.P2, l2.P1, l2.P2);
        }


        public static netDxf.DxfDocument GetDxf(this Face face, P3d origem = null)
        {
            double dist_min = 500;
            double tamanho = 150;
            var doc = new netDxf.DxfDocument();

            if (origem == null)
            {
                origem = new P3d(0, 50 + (tamanho * 2));
            }

            foreach (var linha in face.Linhas)
            {
                doc.Entities.Add(GetLine(linha, netDxf.AciColor.Green, origem));
            }

            foreach (var rec in face.RecortesInternos)
            {
                foreach (var l in rec.GetLinhas())
                {
                    doc.Entities.Add(GetLine(l, netDxf.AciColor.Green, origem));
                }
            }

            foreach (var furo in face.Furacoes)
            {
                var contorno = new List<netDxf.Entities.Line>();
                foreach (var l in furo.GetContorno())
                {
                    contorno.Add(GetLine(l, netDxf.AciColor.Magenta, origem));
                }
                foreach (var c in contorno)
                {
                    doc.Entities.Add(c);
                }
                furo.Bag.Clear();
                furo.Bag.AddRange(contorno);
            }

            var furos = face.Furacoes.OrderBy(x => x.Origem.X).ToList();
            var furos_grp = furos.Select(x => x.Origem.X.Round(0)).Distinct().ToList();
            var coords = new List<double>();

            for (int i = 0; i < furos_grp.Count; i++)
            {
                var pt = furos_grp[i];
                if (i == 0)
                {
                    coords.Add(pt);
                }
                else if (coords.Count > 0)
                {
                    if ((pt - coords.Last()).Abs() > dist_min)
                    {
                        coords.Add(pt);
                    }
                }
            }

            foreach (var x in coords)
            {
                var pt0 = origem.MoverX(x).MoverY(1);
                var pt1 = pt0.MoverY(50);
                var txt = GetText($"{x.String(0)}", pt0.MoverY(50).MoverX(20), tamanho, 90);
                doc.Entities.Add(txt);
                doc.Entities.Add(GetLine(pt0, pt1));
            }
            return doc;
        }
        public static netDxf.Entities.Text GetText(string Texto, P3d origem, double Tamanho, double angulo = 0)
        {
            var texto = new netDxf.Entities.Text(Texto, new netDxf.Vector2(origem.X, origem.Y), Tamanho);
            texto.Rotation = angulo;
            texto.Alignment = netDxf.Entities.TextAlignment.TopLeft;
            return texto;
        }
        public static netDxf.Entities.Line GetLine(this LinhaLiv l, netDxf.AciColor cor = null, P3d origem = null)
        {

            return GetLine(l.P1, l.P2, cor, origem);
        }
        public static netDxf.Entities.Line GetLine(this P3d P1, P3d P2, netDxf.AciColor cor = null, P3d origem = null)
        {
            if (origem == null)
            {
                origem = new P3d();
            }
            var linha = new netDxf.Entities.Line(new netDxf.Vector2(P1.X + origem.X, P1.Y + origem.Y), new netDxf.Vector2(P2.X + origem.X, P2.Y + origem.Y));
            if (cor != null)
            {
                linha.Color = cor;
            }
            return linha;
        }
        public static Rotacao GetQuadranteERotacao(this List<Liv> livs)
        {
            var Centro = livs.Select(x => x.Origem).ToList().Centro();



            for (int i = 0; i < livs.Count; i++)
            {
                livs[i].Quadrante = livs[i].Origem.GetQuadrante(Centro);
            }


            Rotacao rotacao = Rotacao.Desconhecido;
            if (livs.Count == 0)
            {
                return Rotacao.Desconhecido;
            }
            var quadrantes = new List<Quadrante>();
            for (int i = 0; i < livs.Count; i++)
            {
                if (i == 0 && livs[i].Quadrante != Quadrante._INVALIDO)
                {
                    quadrantes.Add(livs[i].Quadrante);
                }
                else
                {
                    if (quadrantes.Count == 0)
                    {
                        if (livs[i].Quadrante != Quadrante._INVALIDO)
                            quadrantes.Add(livs[i].Quadrante);
                    }
                    else if (quadrantes.Last() != livs[i].Quadrante && livs[i].Quadrante != Quadrante._INVALIDO)
                    {
                        quadrantes.Add(livs[i].Quadrante);
                    }
                }
            }

            if (quadrantes.Count > 1)
            {
                var p1 = quadrantes[0];
                var p2 = quadrantes[1];
                switch (p1)
                {
                    case Quadrante.SUP_ESQ:
                        switch (p2)
                        {
                            case Quadrante.SUP_DIR:
                                return Rotacao.Horario;
                            case Quadrante.INF_DIR:
                                return Rotacao.Horario;
                            case Quadrante.INF_ESQ:
                                return Rotacao.AntiHorario;
                        }
                        break;
                    case Quadrante.SUP_DIR:
                        switch (p2)
                        {
                            case Quadrante.SUP_ESQ:
                                return Rotacao.AntiHorario;
                            case Quadrante.INF_DIR:
                                return Rotacao.Horario;
                            case Quadrante.INF_ESQ:
                                return Rotacao.Horario;
                        }
                        break;
                    case Quadrante.INF_DIR:
                        switch (p2)
                        {
                            case Quadrante.SUP_ESQ:
                                return Rotacao.AntiHorario;
                            case Quadrante.SUP_DIR:
                                return Rotacao.AntiHorario;
                            case Quadrante.INF_ESQ:
                                return Rotacao.Horario;
                        }
                        break;
                    case Quadrante.INF_ESQ:
                        switch (p2)
                        {
                            case Quadrante.SUP_ESQ:
                                return Rotacao.Horario;
                            case Quadrante.SUP_DIR:
                                return Rotacao.Horario;
                            case Quadrante.INF_DIR:
                                return Rotacao.AntiHorario;
                        }
                        break;
                }
            }




            return rotacao;
        }
        public static Quadrante GetQuadrante(this P3d ponto, P3d centro)
        {
            var distx = ponto.DistanciaX(centro);
            var disty = ponto.DistanciaY(centro);

            if (distx >= 0 && disty >= 0)
            {
                return Quadrante.SUP_DIR;
            }
            else if (distx >= 0 && disty < 0)
            {
                return Quadrante.INF_DIR;
            }
            else if (distx < 0 && disty >= 0)
            {
                return Quadrante.SUP_ESQ;
            }
            else if (distx < 0 && disty < 0)
            {
                return Quadrante.INF_ESQ;
            }

            return Quadrante._INVALIDO;
        }
        public static List<P3d> Centralizar(this List<P3d> pts, P3d centro)
        {
            List<P3d> retorno = new List<P3d>();
            foreach (var pt in pts)
            {
                retorno.Add(new P3d(pt.DistanciaX(centro), pt.DistanciaY(centro), pt.DistanciaZ(centro)));
            }

            return retorno;
        }
        public static List<P3d> GetPontos(this List<LinhaLiv> linhas, bool fechar = true)
        {
            var pts = new List<P3d>();
            for (int i = 0; i < linhas.Count; i++)
            {
                if (i == 0 && fechar)
                {
                    pts.Add(linhas[i].P1);
                }
                pts.Add(linhas[i].P2);
            }
            return pts;
        }
        public static double Angulo(this LinhaLiv s1, LinhaLiv s2)
        {
            double theta1 = Math.Atan2(s1.P1.Y - s1.P2.Y, s1.P1.X - s1.P2.X);
            double theta2 = Math.Atan2(s2.P1.Y - s2.P2.Y, s2.P1.X - s2.P2.X);

            var ang = Math.Abs(theta2 - theta1) * 180 / Math.PI;

            return 180 - ang;
        }
        /// <summary>
        /// Lança 1 ponto para dentro do vértice, utilizando como alinhamento o ângulo central entre o vértice
        /// e o vértice anterior
        /// </summary>
        /// <param name="min_liv"></param>
        /// <param name="dist"></param>
        /// <param name="rotacao"></param>
        /// <returns></returns>
        public static P3d OffSetInterno(this LinhaLiv min_liv, double dist, Rotacao rotacao)
        {
            //P3d ponto = new P3d(min_liv.GetPonto(dist));
            //se o dp2 é menor que o dp1, é sentido anti horario
            //se o dp1 é menor, é sentido horário
            //var angulo = min_liv.Angulo + (rotacao== Rotacao.AntiHorario?90:-90);
            //var ret = ponto.Mover(angulo , dist);
            //return ret;

            var angbtw = min_liv.Anterior.Angulo(min_liv);
            var ponto = min_liv.P1.Clonar();
            var angulo = angbtw.Abs() / 2;
            var ang_fim = min_liv.Angulo + (rotacao == Rotacao.AntiHorario ? angulo : -angulo);


            var hipotenusa = dist / Math.Sin(angulo.GrausParaRadianos());

            ponto = ponto.Mover(ang_fim, hipotenusa.Abs());

            return ponto;
        }


        public static LinhaLiv GetLivMaisProximo(this List<LinhaLiv> Linhas, P3d pt)
        {
            if (Linhas.Count == 0) { return new cam.LinhaLiv(); }
            var livs = Linhas.OrderBy(x => x.P1.Distancia(pt)).ToList();
            return livs.First();
        }
        public static List<LinhaLiv> Encadear(this List<LinhaLiv> lista)
        {
            if (lista.Count > 1)
            {
                for (int i = 0; i < lista.Count; i++)
                {
                    if (i > 0)
                    {
                        lista[i].Anterior = lista[i - 1];

                    }
                    else
                    {
                        lista[i].Anterior = lista.Last();
                    }
                    if (i < lista.Count - 1)
                    {
                        lista[i].Proximo = lista[i + 1];
                    }
                    else
                    {
                        lista[i].Proximo = lista.First();
                    }
                }
            }
            return lista;
        }
    }
}
