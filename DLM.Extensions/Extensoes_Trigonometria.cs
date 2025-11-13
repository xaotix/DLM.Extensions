using System;

namespace Conexoes
{
    public static class Extensoes_Trigonometria
    {
        public static double Dist(this double X0, double Ang)
        {
            double Retorno = 0;
            if (Ang != 0)
            {
                Retorno = Math.Tan(Ang.GrausParaRadianos()) * X0;
            }
            return Retorno;
        }
        public static double GetDescontoDobra(this double Angulo, double Espessura)
        {
            var ang_norm = Angulo.Normalizar(90);
            var tan = Math.Tan(ang_norm / 2);
            var rads = tan.GrausParaRadianos();
            var angulo = Math.Abs(rads * (Espessura / 2));

            if (angulo > 180)
            {
                return angulo;
            }
            return -angulo;
        }
        public static double GrausParaRadianos(this double angulo, int decimais = 6)
        {
            return (Math.PI * angulo / 180.0).Round(decimais);
        }
        public static double RadianosParaGraus(this double radiano, int decimais = 6)
        {
            return (radiano * (180.0 / Math.PI)).Round(decimais);
        }
        public static double Normalizar(this double Grau, double max = 360)
        {

            while (Grau < 0) Grau += max;

            while (Grau > max) Grau -= max;

            return Grau;
        }
    }
}
