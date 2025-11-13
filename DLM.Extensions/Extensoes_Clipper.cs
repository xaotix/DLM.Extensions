using Clipper2Lib;
using DLM.cam;
using DLM.desenho;
using System.Collections.Generic;
using System.Linq;

namespace Conexoes
{
    public static class ExtensoesClipper
    {
        public static PathD GetPath(this List<P3d> p3Ds)
        {
            var retorno = new PathD(p3Ds.Select(x => new PointD(x.X, x.Y)));
            return retorno;
        }

        public static void AddRange(this PathsD path, List<Face> faces)
        {
            foreach (var face in faces)
            {
                path.Add(face.GetPath());
            }
        }

        public static PathD GetPath(this Face face)
        {
            return face.Liv.Select(x => x.Origem).ToList().GetPath();
        }
        public static PathsD GetPathsD(this PathD pointDs)
        {
            var retorno = new PathsD();
            retorno.Add(pointDs);
            return retorno;
        }
        public static PathsD GetPathsD(this List<Face> faces)
        {
            var retorno = new PathsD();
            retorno.AddRange(faces.Select(x => x.GetPath()));
            return retorno;
        }
        public static double Area(this PathD pointDs)
        {
            return Clipper2Lib.Clipper.Area(pointDs);
        }
        public static PathD GetPath(this List<Liv> p3Ds)
        {
            return p3Ds.Select(x => x.Origem).ToList().GetPath();
        }
        public static PathsD GetPaths(this List<PathD> Paths)
        {
            var retorno = new PathsD();
            foreach (var p in Paths)
            {
                retorno.Add(p);
            }
            return retorno;
        }
        public static PathsD Union(this PathsD subject, FillRule fillRule = Clipper2Lib.FillRule.NonZero)
        {
            return Clipper2Lib.Clipper.Union(subject, fillRule);
        }
        public static PathsD SimplifyPaths(this PathsD path, double epsilon = 0, bool isOpenPath = false)
        {
            return Clipper2Lib.Clipper.SimplifyPaths(path, epsilon, isOpenPath);
        }
        public static PathD SimplifyPath(this PathD path, double epsilon, bool isOpenPath = false)
        {
            return Clipper2Lib.Clipper.SimplifyPath(path, epsilon, isOpenPath);
        }
        public static PathsD Intersect(this PathsD subject, PathsD clip, FillRule fillRule = FillRule.NonZero, int precision = 2)
        {
            return Clipper2Lib.Clipper.Intersect(subject, clip, fillRule, precision);

        }
        public static PathsD Xor(this PathsD subject, PathsD clip, FillRule fillRule = FillRule.NonZero, int precision = 2)
        {
            return Clipper2Lib.Clipper.Xor(subject, clip, fillRule, precision);
        }
    }
}
