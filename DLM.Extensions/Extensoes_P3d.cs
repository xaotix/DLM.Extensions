using Conexoes;
using DLM;
using DLM.desenho;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Conexoes
{
    public static class ExtensoesP3d
    {
        public static P3d Tratar(this P3d origem, int decimais = 5)
        {
            var p0 = new P3d(origem.X, origem.Y);
            if (Double.IsNaN(p0.X) | Double.IsInfinity(p0.X))
            {
                p0.X = 0;
            }
            if (Double.IsNaN(p0.Y) | Double.IsInfinity(p0.Y))
            {
                p0.Y = 0;
            }
            if (Double.IsNaN(p0.Z) | Double.IsInfinity(p0.Z))
            {
                p0.Z = 0;
            }
            p0 = new P3d(Math.Round(p0.X, decimais), Math.Round(p0.Y, decimais), Math.Round(p0.Z, decimais));
            return p0;
        }
        public static List<P3d> GetHorizontaisTop(this List<P3d> Origens)
        {
            var xs = Origens.Select(x => x.X).Distinct().ToList().OrderBy(x => x).ToList();
            var ys = Origens.Select(x => x.Y).Distinct().ToList().OrderBy(x => x).ToList();
            var origens = new List<P3d>();
            for (int i = 0; i < xs.Count; i++)
            {
                origens.Add(new P3d(xs[i], ys[0]));
            }
            return origens;
        }
        public static List<P3d> GetVerticaisRight(this List<P3d> Origens)
        {
            var xs = Origens.Select(x => x.X).Distinct().ToList().OrderBy(x => x).ToList();
            var ys = Origens.Select(x => x.Y).Distinct().ToList().OrderBy(x => x).ToList();
            var origens = new List<P3d>();
            for (int i = 0; i < ys.Count; i++)
            {
                origens.Add(new P3d(xs[xs.Count - 1], ys[i]));
            }
            return origens;
        }
        public static List<P3d> Offset(this List<P3d> Origem, double offset, bool raio_cantos = false)
        {

            var Retorno = new List<P3d>();
            var pp = new DLM.cam.Addons.PolyTree();

            var points = new List<DLM.cam.Addons.IntPoint>();
            double escala = 1024;
            points.AddRange(Origem.Select(x => new DLM.cam.Addons.IntPoint(x.X * escala, x.Y * escala)));
            var solution = new List<System.Collections.Generic.List<DLM.cam.Addons.IntPoint>>();
            var clipper = new DLM.cam.Addons.ClipperOffset();

            clipper.AddPath(points, raio_cantos ? DLM.cam.Addons.JoinType.jtRound : DLM.cam.Addons.JoinType.jtMiter, DLM.cam.Addons.EndType.etOpenButt);
            clipper.Execute(ref solution, offset * escala);

            foreach (DLM.cam.Addons.IntPoint point in solution[0])
            {
                Retorno.Add(new P3d(point.X / escala, point.Y / escala));
            }

            if (Retorno.Count > 0)
            {
                Retorno.Add(new P3d(Retorno[0].X, Retorno[0].Y));
            }

            return Retorno;
        }
        public static netDxf.Vector2 ToVector2(this P3d  p3D)
        {
            return new netDxf.Vector2(p3D.X, p3D.Y);
        }

        public static netDxf.Vector3 ToVector3(this P3d p3D)
        {
            return new netDxf.Vector3(p3D.X, p3D.Y, p3D.Z);
        }
        public static double GetAngulo(this P3d p1)
        {
            return GetAngulo(new DLM.desenho.P3d(), p1);
        }
        public static double GetAngulo(this P3d p1, P3d p2)
        {
            double xDiff = p2.X - p1.X;
            double yDiff = p2.Y - p1.Y;
            double ret = Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;
            if (double.IsNaN(ret))
            {
                ret = 0;
            }
            return ret;
        }
        public static List<P3d> Mover(this List<P3d> Origem, double X, double Y)
        {
            return Origem.Select(x => new P3d(x.X + X, x.Y + Y)).ToList();
        }

        public static double AngX(this P3d p1, P3d p2)
        {
            double Retorno = 0;
            double CA = p1.X - p2.X;
            double CO = p1.Y - p2.Y;
            double tan = CO / CA;

            if (tan != 0)
            {
                Retorno = Math.Atan(tan).RadianosParaGraus();
            }

            return Retorno;
        }
        public static List<P3d> Aninhar(this List<P3d> pontos)
        {
            for (int i = 0; i < pontos.Count; i++)
            {
                if (pontos.Count == 1)
                {

                }
                else if (pontos.Count == 2 && i > 0)
                {

                }
                else if (i == 0)
                {
                    pontos[i].Proximo = pontos[i + 1];
                }

                else if (i > 0 && i < pontos.Count - 2)
                {
                    pontos[i].Proximo = pontos[i + 1];
                    pontos[i].Anterior = pontos[i - 1];

                }
                else
                {
                    pontos[i].Anterior = pontos[i - 1];
                }
                pontos[i].id = i;
            }
            return pontos;
        }
        public static List<P3d> Rotacionar(this List<P3d> p3Ds, P3d Centro, double Angulo)
        {
            return p3Ds.Select(x => x.Rotacionar(Centro, Angulo)).ToList().Aninhar();
        }
        public static List<P3d> GetContornoHull(this List<P3d> linhas, int Escala_Contorno = 5, double Concavidade_Contorno = 1)
        {
            var retorno = new List<P3d>();
            var calculo = new DLM.desenho.Contorno.Hull(linhas);
            var contorno_perfil = calculo.GetPontos(Concavidade_Contorno, Escala_Contorno);
            retorno.AddRange(contorno_perfil);
            retorno = retorno.RemoverRepetidos();
            return retorno.Aninhar();
        }
        public static List<P3d> GetContornoConvexoHull(this List<P3d> pontos, TipoLiv tipo = TipoLiv.Y)
        {

            double escala = 1000;

            if (pontos.Count > 0)
            {
                var pointList = pontos.Select(x => new P3d(x.X, (tipo == TipoLiv.Y ? x.Y : x.Z))).ToList();

                var X0 = pointList.X0();
                pointList = pointList.Normalizar();
                pointList = pointList.OrderBy(p => p.X).ToList();
                pointList = pointList.GroupBy(x => x.ToString()).Select(x => x.First()).ToList();

                pointList = pointList.Select(x => new P3d(x.X, -x.Y)).ToList();

                var convexHull = new ConvexHullSolver();
                var solution = convexHull.getHull(pointList);
                var valores = solution.getPoints();

                return valores.Select(x => x.Mover(X0)).Select(x => new P3d(x.X, tipo == TipoLiv.Y ? -x.Y : 0, tipo == TipoLiv.Z ? -x.Y : 0)).ToList();
            }

            if (pontos.Count > 0)
            {
                var juntar = DLM.desenho.Contorno.GrahamScan.convexHull(pontos.Select(x => new DLM.desenho.Contorno.Node(x.X * escala, (tipo == TipoLiv.Y ? x.Y : x.Z) * escala, 0)).ToList());
                var fim = juntar.Select(x => new P3d(x.x / escala, tipo == TipoLiv.Y ? x.y / escala : 0, tipo == TipoLiv.Z ? x.y / escala : 0)).ToList();
                return fim.Aninhar();
            }

            return new List<P3d>();
        }

        public static P3d Rotacionar(this P3d p3d, P3d Centro, double Angulo)
        {
            double radianos = Angulo.GrausParaRadianos();
            double cosTheta = Math.Cos(radianos);
            double senoTheta = Math.Sin(radianos);
            return new P3d
            {
                X =
                    (int)
                    (cosTheta * (p3d.X - Centro.X) -
                    senoTheta * (p3d.Y - Centro.Y) + Centro.X),
                Y =
                    (int)
                    (senoTheta * (p3d.X - Centro.X) +
                    cosTheta * (p3d.Y - Centro.Y) + Centro.Y)
            };
        }
        public static double Distancia(this P3d p3d, P3d p2, int precisao = 4)
        {
            return Math.Round(Math.Sqrt(Math.Pow(p2.X - p3d.X, 2) + Math.Pow(p2.Y - p3d.Y, 2) + Math.Pow(p2.Z - p3d.Z, 2)), precisao);
        }
        public static double DistanciaX(this P3d p3d, P3d p1)
        {
            if (p1 == null) { return 0; }
            return p3d.X - p1.X;
        }
        public static double DistanciaY(this P3d p3d, P3d p1)
        {
            if (p1 == null) { return 0; }
            return p3d.Y - p1.Y;
        }
        public static double DistanciaZ(this P3d p3d, P3d p1)
        {
            if (p1 == null) { return 0; }
            return p3d.Z - p1.Z;
        }
        public static P3d MoverX(this P3d p3d, double valor)
        {
            var pt = p3d.Clonar();
            pt.X += valor;
            return pt;
        }
        public static P3d MoverY(this P3d p3d, double valor)
        {
            var pt = p3d.Clonar();
            pt.Y += valor;
            return pt;
        }
        public static P3d MoverZ(this P3d p3d, double valor)
        {
            var pt = p3d.Clonar();
            pt.Z += valor;
            return pt;
        }
        public static P3d SetX(this P3d p3d, double valor)
        {
            var pt = p3d.Clonar();
            pt.X = valor;
            return pt;
        }
        public static P3d SetZ(this P3d p3d, double valor)
        {
            var pt = p3d.Clonar();
            pt.Z = valor;
            return pt;
        }
        public static P3d SetY(this P3d p3d, double valor)
        {
            var pt = p3d.Clonar();
            pt.Y = valor;
            return pt;
        }
        public static P3d Round(this P3d p3d, int decimais)
        {
            return new P3d(p3d.X.Round(decimais), p3d.Y.Round(decimais), p3d.Z.Round(decimais));
        }
        public static P3d Escala(this P3d p3d, double valor)
        {
            return new P3d(p3d.X * valor, p3d.Y * valor, p3d.Z * valor);
        }

        public static System.Drawing.PointF ToPointF(this P3d p3d)
        {
            return new System.Drawing.PointF((float)p3d.X, (float)p3d.Y);
        }



        public static P3d Mover(this P3d p3d, double Angulo, double Distancia, int decimais = 10)
        {
            double angleRadians = Angulo.GrausParaRadianos();
            P3d ret = new P3d();

            ret.Y = (p3d.Y + (Math.Sin(angleRadians) * Distancia));
            ret.X = (p3d.X + (Math.Cos(angleRadians) * Distancia));
            return new P3d(Math.Round(ret.X, decimais), Math.Round(ret.Y, decimais), p3d.Z);
        }
        public static P3d Mover(this P3d p3d, P3d Distancia)
        {
            var pt = new P3d(p3d);
            return pt.MoverX(Distancia.X).MoverY(Distancia.Y);
        }
        public static P3d Mover(this P3d p3d, Vetor3D vetor, double distancia, bool arredondar = false)
        {
            P3d pt = p3d.Clonar();
            pt.X += vetor.X * distancia;
            pt.Y += vetor.Y * distancia;
            pt.Z += vetor.Z * distancia;
            if (arredondar) pt = new P3d(pt.X, pt.Y, pt.Z, true);
            return pt;
        }
        public static P3d Mover(this P3d p3d, P3d p1, P3d p2, double Distancia)
        {
            Vetor3D VetorL1a = p1.GetVetor(p2);
            VetorL1a.Normalize();
            Vetor3D VetorOrigem = new Vetor3D(-VetorL1a.Y, VetorL1a.X, 0);
            return p3d.Mover(VetorL1a, Distancia);
        }
        public static List<P3d> Mover(this List<P3d> pts, P3d X0)
        {
            List<P3d> retorno = new List<P3d>();

            foreach (var pt in pts)
            {
                var p1 = pt.Mover(X0);
                retorno.Add(p1);
            }
            return retorno.Aninhar();
        }
        public static P3d Mover(this P3d p1, P3d p2, double Distancia, bool fim = false)
        {
            var Origem = new P3d(p1.X, p1.Y, 0);
            if (fim)
            {
                Origem = new P3d(p2.X, p2.Y, 0);
            }
            var VetorL1a = new Vetor3D(new P3d(p1.X, p1.Y, 0), new P3d(p2.X, p2.Y, 0));
            VetorL1a.Normalize();
            var VetorOrigem = new Vetor3D(-VetorL1a.Y, VetorL1a.X, 0);
            return Origem.Mover(VetorL1a, Distancia);
        }



        public static P3d Centro(this P3d p3d, P3d p2)
        {
            var p1 = p3d.Clonar();
            var Retorno = new P3d();
            Retorno.X = (p1.X + p2.X) / 2;
            Retorno.Y = (p1.Y + p2.Y) / 2;
            Retorno.Z = (p1.Z + p2.Z) / 2;
            return Retorno;
        }

        public static P3d P3d(this System.Windows.Media.Media3D.Point3D point)
        {
            return new P3d(point.X, point.X, point.Z);
        }

        public static P3d P3d(this System.Windows.Point point, double Z = 0)
        {
            return new P3d(point.X, point.X, Z);
        }
        public static Vetor3D GetVetor(this P3d p3d, P3d ponto)
        {
            System.Windows.Media.Media3D.Vector3D vetor = new System.Windows.Media.Media3D.Vector3D(p3d.X - ponto.X, p3d.Y - ponto.Y, p3d.Z - ponto.Z);
            vetor.Normalize();
            return new Vetor3D(vetor);
        }
        public static System.Windows.Point GetPoint(this P3d p3d, double escala = 1, double offsetX = 0, double offsetY = 0)
        {
            return new System.Windows.Point((p3d.X + offsetX) * escala, (p3d.Y + offsetY) * escala);
        }
        public static System.Windows.Media.Media3D.Point3D GetPoint3D(this P3d p3d, double factor = 1)
        {
            if (factor == 0)
            {
                factor = 1;
            }
            return new System.Windows.Media.Media3D.Point3D(p3d.X / factor, p3d.Y / factor, p3d.Z / factor);
        }
        public static System.Windows.Point GetPoint(this P3d p3d, P3d p0, double escala = 1)
        {
            return new System.Windows.Point((p3d.X + p0.X) * escala, (p3d.Y + p0.Y) * escala);
        }
        public static P3d MoverSC(this P3d p3d, P3d mover, double escala = 1)
        {
            return new P3d((p3d.X + mover.X) * escala, (p3d.Y + mover.Y) * escala);
        }
        public static double GetAngulo(this P3d p3d, P3d p2, int precisao = 2)
        {
            double xDiff = p2.X - p3d.X;
            double yDiff = p2.Y - p3d.Y;
            double ret = Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;
            if (double.IsNaN(ret))
            {
                ret = 0;
            }
            ret = ret.Round(precisao);
            return ret;
        }
        public static P3d Inverter(this P3d p3d)
        {
            return new P3d(-p3d.X, -p3d.Y, -p3d.Z);
        }
        public static List<P3d> AlinharX(this List<P3d> pontos, double tolerancia)
        {
            var grp_X = pontos.Select(x => x.X.Round(0)).Distinct().ToList();
            List<P3d> lista_controle = new List<P3d>();
            List<P3d> lista_alinhada = new List<P3d>();
            foreach (var fr in grp_X)
            {
                var frs = pontos.FindAll(x => lista_controle.Find(y => y.ToString() == x.ToString()) == null).FindAll(x => x.X.Round(0) == fr);
                lista_controle.AddRange(frs);
                lista_alinhada.AddRange(frs.Select(x => x.SetX(fr)));
                var frs_prox = pontos.FindAll(x => lista_controle.Find(y => y.ToString() == x.ToString()) == null).FindAll(x => x.X.Round(0) >= fr - tolerancia && x.X.Round(0) <= fr + tolerancia);
                lista_alinhada.AddRange(frs_prox.Select(x => x.SetX(fr)));
            }
            return lista_alinhada;
        }
        public static List<P3d> Bordas(this List<P3d> Pontos)
        {
            List<P3d> Bordas = new List<P3d>();
            if (Pontos.Count > 0)
            {
                Bordas.Add(new P3d(Pontos.Min(x => x.X), Pontos.Max(x => x.Y)));
                Bordas.Add(new P3d(Pontos.Max(x => x.X), Pontos.Max(x => x.Y)));
                Bordas.Add(new P3d(Pontos.Max(x => x.X), Pontos.Min(x => x.Y)));
                Bordas.Add(new P3d(Pontos.Min(x => x.X), Pontos.Min(x => x.Y)));
            }

            return Bordas;
        }
        public static double Perimetro(this List<P3d> lista)
        {
            double retorno = 0;

            for (int i = 0; i < lista.Count; i++)
            {
                if (i > 0)
                {
                    retorno += lista[i - 0].Distancia(lista[i]).Abs();
                }
            }

            return retorno;
        }

        public static P3d Min(this List<P3d> lista)
        {
            if (lista.Count > 0)
            {
                return new P3d(lista.Min(x => x.X), lista.Min(x => x.Y), lista.Min(x => x.Z));
            }
            return new P3d();
        }
        public static P3d Max(this List<P3d> lista)
        {
            if (lista.Count > 0)
            {
                return new P3d(lista.Max(x => x.X), lista.Max(x => x.Y), lista.Max(x => x.Z));
            }
            return new P3d();
        }



        public static List<P3d> ArredondarJuntar(this List<P3d> origem, int decimais_X = 0, int decimais_Y = 0)
        {
            try
            {
                return origem.Select(x => new P3d(Math.Round(x.X, decimais_X), Math.Round(x.Y, decimais_Y), 0)).GroupBy(x => "X: " + x.X + " Y:" + x.Y).Select(x => x.First()).ToList();

            }
            catch (System.Exception)
            {


            }
            return new List<P3d>();
        }

        public static List<P3d> RemoverRepetidos(this List<P3d> pts)
        {
            List<P3d> lista = new List<P3d>();
            for (int i = 0; i < pts.Count; i++)
            {
                var p = pts[i];
                if (i > 0 && lista.Count > 0)
                {
                    var p0 = lista[lista.Count - 1];

                    if (p0.X == p.X && p0.Y == p.Y)
                    {

                    }
                    else
                    {
                        lista.Add(p);
                    }
                }
                else
                {
                    lista.Add(p);
                }
            }

            return lista;
        }

        /// <summary>
        /// Normaliza as coordenadas pelo menor ponto no canto superior esquerdo
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static List<P3d> Normalizar(this List<P3d> pts, int decimais = 2, double x = 0, double y = 0)
        {
            P3d X0 = pts.X0().Inverter();

            var retorno = new List<P3d>();

            foreach (var pt in pts)
            {
                var p1 = pt.Mover(X0);
                retorno.Add(p1.MoverX(x).MoverY(y).Round(decimais));
            }
            retorno = retorno.RemoverRepetidos();
            return retorno;
        }
        /// <summary>
        /// Gira os pontos utilizando como vértice o X0.
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="Angulo"></param>
        /// <returns></returns>
        public static List<P3d> Rotacionar(this List<P3d> pts, double Angulo)
        {
            var X0 = pts.X0();
            List<P3d> retorno = new List<P3d>();


            foreach (var pt in pts)
            {
                var mov = pt.Rotacionar(X0, Angulo);
                retorno.Add(mov);
            }
            return retorno;
        }





        private static int TemIntersec2d(this P3d InicioLinha1, P3d FinalLinha1, P3d InicioLinha2, P3d FinalLinha2, out double IntersecReta1, out double IntersecReta2)
        {
            double retorno;
            retorno = (FinalLinha2.X - InicioLinha2.X) * (FinalLinha1.Y - InicioLinha1.Y) - (FinalLinha2.Y - InicioLinha2.Y) * (FinalLinha1.X - InicioLinha1.X);

            if (retorno == 0.0)
            {
                IntersecReta1 = 0;
                IntersecReta2 = 0;
                return 0; // não há intersecção
            }

            IntersecReta1 = ((FinalLinha2.X - InicioLinha2.X) * (InicioLinha2.Y - InicioLinha1.Y) - (FinalLinha2.Y - InicioLinha2.Y) * (InicioLinha2.X - InicioLinha1.X)) / retorno;
            IntersecReta2 = ((FinalLinha1.X - InicioLinha1.X) * (InicioLinha2.Y - InicioLinha1.Y) - (FinalLinha1.Y - InicioLinha1.Y) * (InicioLinha2.X - InicioLinha1.X)) / retorno;

            return 1; // há intersecção
        }
        private static P3d GetInterseccao(this P3d InicioLinha1, P3d FinalLinha1, double ParamReta1)
        {
            P3d retorno = new P3d();
            retorno.X = InicioLinha1.X + (FinalLinha1.X - InicioLinha1.X) * ParamReta1;
            retorno.Y = InicioLinha1.Y + (FinalLinha1.Y - InicioLinha1.Y) * ParamReta1;

            return retorno;
        }
        public static P3d Interseccao(this P3d iL1, P3d fL1, P3d iL2, P3d fL2)
        {
            double intersecReta1, intersecReta2;
            if (TemIntersec2d(iL1, fL1, iL2, fL2, out intersecReta1, out intersecReta2) == 1)
            {
                return GetInterseccao(iL1, fL1, intersecReta1);
            }
            return new P3d();
        }


        public static P3d X0(this List<P3d> pts)
        {
            return new P3d(pts.Min().X, pts.Max().Y);
        }






        //public static P3d Centro(this List<P3d> Bordas)
        //{
        //    if (Bordas.Count > 0)
        //    {
        //        double totalX = 0, totalY = 0;
        //        foreach (var p in Bordas)
        //        {
        //            totalX += p.X;
        //            totalY += p.Y;
        //        }
        //        double centerX = totalX / Bordas.Count;
        //        double centerY = totalY / Bordas.Count;
        //        return new P3d(centerX, centerY);
        //    }
        //    return new P3d();
        //}
        /// <summary>
        /// Retorna o centro entre X e Y
        /// </summary>
        /// <param name="pontos"></param>
        /// <returns></returns>
        public static P3d Centro(this List<P3d> pontos)
        {
            if (pontos.Count < 2)
            {
                return new P3d();
            }
            else if (pontos.Count < 3)
            {
                return
                      pontos.First().Centro(pontos.Last());
            }
            var se = new P3d(pontos.Min(x => x.X), pontos.Max(x => x.Y), 0);
            var sd = new P3d(pontos.Max(x => x.X), pontos.Max(x => x.Y), 0);

            var ie = new P3d(pontos.Min(x => x.X), pontos.Min(x => x.Y), 0);
            var id = new P3d(pontos.Max(x => x.X), pontos.Min(x => x.Y), 0);

            var seXid = se.Centro(id);
            var sdXie = sd.Centro(ie);
            var centros = seXid.Centro(sdXie);

            if (pontos.Count == 4)
            {
                return centros;
            }

            // Add the first point at the end of the array.
            int num_points = pontos.Count;
            P3d[] pts = new P3d[num_points + 1];
            pontos.CopyTo(pts, 0);
            pts[num_points] = pontos[0];

            // Find the centroid.
            double X = 0;
            double Y = 0;
            double second_factor;
            for (int i = 0; i < num_points; i++)
            {
                second_factor =
                    pts[i].X * pts[i + 1].Y -
                    pts[i + 1].X * pts[i].Y;
                X += (pts[i].X + pts[i + 1].X) * second_factor;
                Y += (pts[i].Y + pts[i + 1].Y) * second_factor;
            }

            // Divide by 6 times the polygon's area.
            double polygon_area = pontos.Area();
            X /= (6 * polygon_area);
            Y /= (6 * polygon_area);


            if ((centros.X > 0 && X < 0) | (centros.X < 0 && X > 0))
            {
                X = -X;
            }

            if ((centros.Y > 0 && Y < 0) | (centros.Y < 0 && Y > 0))
            {
                Y = -Y;
            }
            var centro = new P3d(X, Y, 0);


            return centro;
        }

        /// <summary>
        /// Retorna a área 2d
        /// </summary>
        /// <param name="Points"></param>
        /// <returns></returns>
        public static double Area(this List<P3d> Points)
        {
            // Add the first point to the end.
            int num_points = Points.Count;
            P3d[] pts = new P3d[num_points + 1];
            Points.CopyTo(pts, 0);
            pts[num_points] = Points[0];

            // Get the areas.
            double area = 0;
            for (int i = 0; i < num_points; i++)
            {
                area +=
                    (pts[i + 1].X - pts[i].X) *
                    (pts[i + 1].Y + pts[i].Y) / 2;
            }

            // Return the result.
            return area;
        }
        public static double Comprimento(this List<P3d> lista)
        {
            return lista.Max().X - lista.Min().X;
        }
        public static double Largura(this List<P3d> lista)
        {
            return lista.Max().Y - lista.Min().Y;
        }

        public static double Peso(this List<P3d> lista, double Espessura)
        {
            return lista.Area() * Espessura * Cfg.Init.Peso_Especifico;
        }

    }
}
