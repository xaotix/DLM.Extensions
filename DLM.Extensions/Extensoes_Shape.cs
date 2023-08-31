using Clipper2Lib;
using DLM.cam;
using DLM.desenho;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conexoes
{
    public static class Extensoes_Shape
    {
        public static void AddFuroBorda(this Shape shape, bool somente_canto, double diam, double borda)
        {
            if (diam <= 0)
            {
                diam = Cfg.Init.Furo_Gabarito_Diametro;
            }
            if (borda <= 0)
            {
                borda = Cfg.Init.Furo_Gabarito_Angulo_Offset_Borda;
            }
            double ang_min = Cfg.Init.Furo_Gabarito_Angulo_Minimo;
            double distmin = Math.Abs(Cfg.Init.TestList_V_CAMS_Furacoes_Dist_Furo_Borda * diam);

            if (somente_canto)
            {
                //adiciona 1 furo no canto superior esquerdo
                var pt0 = new P3d(shape.LIV1.MinX, shape.LIV1.MaxY);
                var min_liv = shape.LIV1.Linhas.GetLivMaisProximo(pt0);
                var ponto = min_liv.OffSetInterno(borda, shape.LIV1.Rotacao);

                var proximos = shape.LIV1.Furacoes.FindAll(x => x.Origem.Distancia(ponto) <= distmin);
                if (proximos.Count == 0)
                {
                    shape.AddFuroLIV1(ponto, diam);
                }
            }
            else
            {
                foreach (var liv in shape.LIV1.Linhas)
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
                        var ponto = liv.OffSetInterno(borda, shape.LIV1.Rotacao);
                        var proximos = shape.LIV1.Furacoes.FindAll(x => x.Origem.Distancia(ponto) <= distmin);
                        if (proximos.Count == 0)
                        {
                            shape.AddFuroLIV1(ponto, diam);
                        }
                    }
                }
            }

            shape.LIV1.Furacoes = shape.LIV1.Furacoes.GroupBy(x => $"{x.Origem.X}@{x.Origem.Y}").Select(x => x.First()).ToList();
        }
        public static List<Face> GetLivs(this Shape shape)
        {
            var Retorno = new List<Face>();
            Retorno.Add(shape.LIV1);
            Retorno.Add(shape.LIV2);
            if (shape.Perfil.Faces > 2)
            {
                if (shape.LIV3.Comprimento > 0 && shape.LIV3.Largura > 0)
                {
                    Retorno.Add(shape.LIV3);
                }
            }

            if (shape.Perfil.Faces > 3)
            {
                if (shape.LIV4.Comprimento > 0 && shape.LIV4.Largura > 0)
                {
                    Retorno.Add(shape.LIV4);
                }
            }

            return Retorno;
        }
        public static double GetPeso(this Shape shape)
        {
            var liv1P = shape.LIV1_SemBordas().GetPeso(shape.Perfil);
            var liv2P = shape.LIV2.GetPeso(shape.Perfil);
            var liv3P = shape.LIV3.GetPeso(shape.Perfil);
            var liv4P = shape.LIV4.GetPeso(shape.Perfil);



            if (shape.Perfil.Tipo == CAM_PERFIL_TIPO.Barra_Redonda | shape.Perfil.Tipo == CAM_PERFIL_TIPO.Tubo_Redondo)
            {
                return Math.Round(shape.Perfil.GetPesoMetro() / 1000 * shape.Comprimento, Cfg.Init.CAM_Decimais_Peso);
            }
            else if (shape.Perfil.Primitivo == CAM_PRIMITIVO._)
            {
                return liv1P;
            }
            else if (shape.Perfil.Familia == CAM_FAMILIA.Laminado)
            {
                if (shape.Perfil.Tipo == CAM_PERFIL_TIPO.L_Laminado)
                {
                    return Math.Round(liv1P + liv2P, Cfg.Init.CAM_Decimais_Peso);
                }
                else if (shape.Perfil.Tipo == CAM_PERFIL_TIPO.Barra_Chata)
                {
                    return liv1P;
                }
                else if (shape.Perfil.Tipo == CAM_PERFIL_TIPO.INP | shape.Perfil.Tipo == CAM_PERFIL_TIPO.WLam)
                {
                    return Math.Round(liv1P + liv2P + liv3P, Cfg.Init.CAM_Decimais_Peso);
                }
                else if (shape.Perfil.Tipo == CAM_PERFIL_TIPO.UAP | shape.Perfil.Tipo == CAM_PERFIL_TIPO.UNP)
                {
                    return Math.Round(liv1P + liv2P + liv3P, Cfg.Init.CAM_Decimais_Peso);
                }
                else if (shape.Perfil.Tipo == CAM_PERFIL_TIPO.Tubo_Quadrado)
                {
                    return Math.Round((liv1P * 2) + liv2P + liv3P, Cfg.Init.CAM_Decimais_Peso);
                }

            }
            else
            {

                return shape.GetLivs().Sum(x => x.GetPeso(shape.Perfil));
            }
            return 0;
        }
        public static Face GetPlanificada(this Shape shape)
        {
            var originais = new List<Face>();
            var faces = new List<Face>();
            var pf = shape.Perfil;
            int multiplicador = 1000;
            var solution = new List<System.Collections.Generic.List<DLM.cam.Addons.IntPoint>>();
            var furos = new List<Furo>();
            var recortes = new List<Recorte>();
            var dobras = new List<Dobra>();
            var soldas = new List<Solda>();

            if (pf.Familia == CAM_FAMILIA.Dobrado)
            {

                var poligono = new DLM.cam.Addons.Clipper();
                var lista = new List<System.Collections.Generic.List<DLM.cam.Addons.IntPoint>>();
                if (pf.Tipo == CAM_PERFIL_TIPO.L_Dobrado)
                {
                    var ch2 = shape.GetLIV2_MesaParaChapa().RebaterY().Cortar(0, pf.Esp);
                    var ch3 = shape.LIV1.Cortar(pf.Esp, 0);

                    ch3 = ch3.MoverY(-ch2.Largura);

                    originais.Add(ch3);
                    originais.Add(ch2);

                    lista.Add(ch2.Liv.GetIntPoints(0, 0, multiplicador));
                    lista.Add(ch3.Liv.GetIntPoints(0, 0, multiplicador));


                    //LIV2
                    furos.AddRange(ch2.Furacoes);
                    recortes.AddRange(ch2.RecortesInternos);

                    //LIV1
                    furos.AddRange(ch3.Furacoes);
                    recortes.AddRange(ch3.RecortesInternos);



                    //dobras.Add(new Dobra(90, -ch3.Origem.Y, this.Cam));
                    dobras.Add(new Dobra(new P3d(ch2.MinX, ch3.Origem.Y), new P3d(ch2.MaxX, ch3.Origem.Y), 90));

                }
                else if (pf.Tipo == CAM_PERFIL_TIPO.U_Dobrado)
                {
                    var ch2 = shape.GetLIV2_MesaParaChapa().RebaterY().Cortar(0, pf.Esp);
                    var ch3 = shape.LIV1.Cortar(pf.Esp, pf.Esp);
                    var ch4 = shape.GetLIV3_MesaParaChapa().Cortar(pf.Esp, 0);

                    ch3 = ch3.MoverY(-ch2.Largura);
                    ch4 = ch4.MoverY(-ch2.Largura - ch3.Largura);

                    originais.Add(ch3);
                    originais.Add(ch2);
                    originais.Add(ch4);

                    lista.Add(ch2.Liv.GetIntPoints(0, 0, multiplicador));
                    lista.Add(ch3.Liv.GetIntPoints(0, 0, multiplicador));
                    lista.Add(ch4.Liv.GetIntPoints(0, 0, multiplicador));


                    //LIV2
                    furos.AddRange(ch2.Furacoes);
                    recortes.AddRange(ch2.RecortesInternos);

                    //LIV1
                    furos.AddRange(ch3.Furacoes);
                    recortes.AddRange(ch3.RecortesInternos);

                    //LIV3
                    furos.AddRange(ch4.Furacoes);
                    recortes.AddRange(ch4.RecortesInternos);


                    dobras.Add(new Dobra(new P3d(ch2.MinX, ch3.Origem.Y), new P3d(ch2.MaxX, ch3.Origem.Y), 90));
                    dobras.Add(new Dobra(new P3d(ch3.MinX, ch4.Origem.Y), new P3d(ch3.MaxX, ch4.Origem.Y), 90));
                }
                else if (pf.Tipo == CAM_PERFIL_TIPO.Z_Dobrado)
                {
                    var ch2 = shape.GetLIV2_MesaParaChapa().RebaterY().Cortar(0, pf.Esp);
                    var ch3 = shape.LIV1.Cortar(pf.Esp, pf.Esp);
                    var ch4 = shape.GetLIV3_MesaParaChapa().RebaterY().Cortar(pf.Esp, 0);

                    ch3 = ch3.MoverY(-ch2.Largura);
                    ch4 = ch4.MoverY(-ch2.Largura - ch3.Largura);

                    originais.Add(ch3);
                    originais.Add(ch2);
                    originais.Add(ch4);

                    lista.Add(ch2.Liv.GetIntPoints(0, 0, multiplicador));
                    lista.Add(ch3.Liv.GetIntPoints(0, 0, multiplicador));
                    lista.Add(ch4.Liv.GetIntPoints(0, 0, multiplicador));


                    //LIV2
                    furos.AddRange(ch2.Furacoes);
                    recortes.AddRange(ch2.RecortesInternos);

                    //LIV1
                    furos.AddRange(ch3.Furacoes);
                    recortes.AddRange(ch3.RecortesInternos);

                    //LIV3
                    furos.AddRange(ch4.Furacoes);
                    recortes.AddRange(ch4.RecortesInternos);

                    dobras.Add(new Dobra(new P3d(ch2.MinX, ch3.Origem.Y), new P3d(ch2.MaxX, ch3.Origem.Y), 270));
                    dobras.Add(new Dobra(new P3d(ch3.MinX, ch4.Origem.Y), new P3d(ch3.MaxX, ch4.Origem.Y), 90));
                }
                else if (pf.Tipo == CAM_PERFIL_TIPO.C_Enrigecido)
                {

                    var ch1 = shape.LIV2_Aba_Menor(true);
                    var ch2 = shape.GetLIV2_MesaParaChapa().RebaterY().Cortar(pf.Esp, pf.Esp);
                    var ch3 = shape.LIV1.Cortar(pf.Esp, pf.Esp);
                    var ch4 = shape.GetLIV3_MesaParaChapa().Cortar(pf.Esp, pf.Esp);
                    var ch5 = shape.LIV3_Aba_Menor(true);

                    ch2 = ch2.MoverY(-ch1.Largura);
                    ch3 = ch3.MoverY(-ch1.Largura - ch2.Largura);
                    ch4 = ch4.MoverY(-ch1.Largura - ch2.Largura - ch3.Largura);
                    ch5 = ch5.MoverY(-ch1.Largura - ch2.Largura - ch3.Largura - ch4.Largura);

                    originais.Add(ch1);
                    originais.Add(ch2);
                    originais.Add(ch3);
                    originais.Add(ch4);
                    originais.Add(ch5);

                    lista.Add(ch1.Liv.GetIntPoints(0, 0, multiplicador));
                    lista.Add(ch2.Liv.GetIntPoints(0, 0, multiplicador));
                    lista.Add(ch3.Liv.GetIntPoints(0, 0, multiplicador));
                    lista.Add(ch4.Liv.GetIntPoints(0, 0, multiplicador));
                    lista.Add(ch5.Liv.GetIntPoints(0, 0, multiplicador));

                    //LIV2
                    furos.AddRange(ch2.Furacoes);
                    recortes.AddRange(ch2.RecortesInternos);

                    //LIV1
                    furos.AddRange(ch3.Furacoes);
                    recortes.AddRange(ch3.RecortesInternos);

                    //LIV3
                    furos.AddRange(ch4.Furacoes);
                    recortes.AddRange(ch4.RecortesInternos);


                    dobras.Add(new Dobra(new P3d(ch4.MinX, ch5.Origem.Y), new P3d(ch4.MaxX, ch5.Origem.Y), pf.Angulo));
                    dobras.Add(new Dobra(new P3d(ch3.MinX, ch4.Origem.Y), new P3d(ch3.MaxX, ch4.Origem.Y), 90));
                    dobras.Add(new Dobra(new P3d(ch2.MinX, ch3.Origem.Y), new P3d(ch2.MaxX, ch3.Origem.Y), 90));
                    dobras.Add(new Dobra(new P3d(ch1.MinX, ch2.Origem.Y), new P3d(ch1.MaxX, ch2.Origem.Y), pf.Angulo));

                }
                else if (pf.Tipo == CAM_PERFIL_TIPO.Z_Purlin)
                {

                    var ch1 = shape.LIV2_Aba_Menor(true);
                    var ch2 = shape.GetLIV2_MesaParaChapa().RebaterY().Cortar(pf.Esp, pf.Esp);
                    var ch3 = shape.LIV1.Cortar(pf.Esp, pf.Esp);
                    var ch4 = shape.GetLIV3_MesaParaChapa().Cortar(pf.Esp, pf.Esp);
                    var ch5 = shape.LIV3_Aba_Menor(true);

                    ch2 = ch2.MoverY(-ch1.Largura);
                    ch3 = ch3.MoverY(-ch1.Largura - ch2.Largura);
                    ch4 = ch4.MoverY(-ch1.Largura - ch2.Largura - ch3.Largura);
                    ch5 = ch5.MoverY(-ch1.Largura - ch2.Largura - ch3.Largura - ch4.Largura);

                    originais.Add(ch1);
                    originais.Add(ch2);
                    originais.Add(ch3);
                    originais.Add(ch4);
                    originais.Add(ch5);

                    lista.Add(ch1.Liv.GetIntPoints(0, 0, multiplicador));
                    lista.Add(ch2.Liv.GetIntPoints(0, 0, multiplicador));
                    lista.Add(ch3.Liv.GetIntPoints(0, 0, multiplicador));
                    lista.Add(ch4.Liv.GetIntPoints(0, 0, multiplicador));
                    lista.Add(ch5.Liv.GetIntPoints(0, 0, multiplicador));

                    //LIV2
                    furos.AddRange(ch2.Furacoes);
                    recortes.AddRange(ch2.RecortesInternos);

                    //LIV1
                    furos.AddRange(ch3.Furacoes);
                    recortes.AddRange(ch3.RecortesInternos);

                    //LIV3
                    furos.AddRange(ch4.Furacoes);
                    recortes.AddRange(ch4.RecortesInternos);

                    dobras.Add(new Dobra(new P3d(ch4.MinX, ch5.Origem.Y), new P3d(ch4.MaxX, ch5.Origem.Y), 90));
                    dobras.Add(new Dobra(new P3d(ch3.MinX, ch4.Origem.Y), new P3d(ch3.MaxX, ch4.Origem.Y), 90));
                    dobras.Add(new Dobra(new P3d(ch2.MinX, ch3.Origem.Y), new P3d(ch2.MaxX, ch3.Origem.Y), 270));
                    dobras.Add(new Dobra(new P3d(ch1.MinX, ch2.Origem.Y), new P3d(ch1.MaxX, ch2.Origem.Y), 270));
                }
                else if (pf.Tipo == CAM_PERFIL_TIPO.Cartola)
                {
                    var ch1 = shape.LIV3_Cartola1();
                    var ch2 = shape.LIV1.Cortar(pf.Esp, pf.Esp).RebaterY();
                    var ch3 = shape.GetLIV2_MesaParaChapa().RebaterY().Cortar(pf.Esp, pf.Esp);
                    var ch4 = shape.LIV1.Cortar(pf.Esp, pf.Esp);
                    var ch5 = shape.LIV3_Cartola2();

                    ch2 = ch2.MoverY(-ch1.Largura);
                    ch3 = ch3.MoverY(-ch1.Largura - ch2.Largura);
                    ch4 = ch4.MoverY(-ch1.Largura - ch2.Largura - ch3.Largura);
                    ch5 = ch5.MoverY(-ch1.Largura - ch2.Largura - ch3.Largura - ch4.Largura);


                    originais.Add(ch1);
                    originais.Add(ch2);
                    originais.Add(ch3);
                    originais.Add(ch4);
                    originais.Add(ch5);

                    lista.Add(ch1.Liv.GetIntPoints(0, 0, multiplicador));
                    lista.Add(ch2.Liv.GetIntPoints(0, 0, multiplicador));
                    lista.Add(ch3.Liv.GetIntPoints(0, 0, multiplicador));
                    lista.Add(ch4.Liv.GetIntPoints(0, 0, multiplicador));
                    lista.Add(ch5.Liv.GetIntPoints(0, 0, multiplicador));


                    dobras.Add(new Dobra(new P3d(ch4.MinX, ch5.Origem.Y), new P3d(ch4.MaxX, ch5.Origem.Y), 270));
                    dobras.Add(new Dobra(new P3d(ch3.MinX, ch4.Origem.Y), new P3d(ch3.MaxX, ch4.Origem.Y), 90));
                    dobras.Add(new Dobra(new P3d(ch2.MinX, ch3.Origem.Y), new P3d(ch2.MaxX, ch3.Origem.Y), 90));
                    dobras.Add(new Dobra(new P3d(ch1.MinX, ch2.Origem.Y), new P3d(ch1.MaxX, ch2.Origem.Y), 270));
                }

                poligono.AddPaths(lista, DLM.cam.Addons.PolyType.ptSubject, true);
                poligono.Execute(DLM.cam.Addons.ClipType.ctUnion, solution,
                  DLM.cam.Addons.PolyFillType.pftNonZero, DLM.cam.Addons.PolyFillType.pftPositive);

            }


            foreach (var s in solution)
            {
                faces.Add(new Face(
                    s.Select(x => new Liv(x.X / multiplicador, x.Y / multiplicador)).ToList(),
                    FaceNum.LIV1,
                    recortes,
                    furos,
                    dobras,
                    soldas));
            }

            if (faces.Count == 1)
            {
                return faces[0];
            }
            else if (faces.Count > 1)
            {
                //não pode vir aqui.
                Conexoes.Utilz.Alerta($"Erro ao tentar desmembrar.");
                return faces[0];
            }

            return new Face();
        }
        public static Face GetLIV3_MesaParaChapa(this Shape shape)
        {
            return shape.LIV3.MesaParaChapa(shape.Perfil);
        }
        public static Face GetLIV2_MesaParaChapa(this Shape shape)
        {
            return shape.LIV2.MesaParaChapa(shape.Perfil);
        }
        public static Face LIV3_Cartola1(this Shape shape)
        {
            var pf = shape.Perfil;
            var s = shape.GetLIV3_MesaParaChapa().RebaterY().Cortar(0, pf.GetLarguraCartola() - pf.Aba_S + pf.Esp);
            return s;
        }
        public static Face LIV3_Cartola2(this Shape shape)
        {
            var pf = shape.Perfil;
            var s = shape.GetLIV3_MesaParaChapa().RebaterY().Cortar(pf.GetLarguraCartola() - pf.Aba_I + pf.Esp, 0);
            return s;
        }
        public static Face LIV3_Aba_Menor(this Shape shape, bool descontar_dobra)
        {
            var pf = shape.Perfil;
            if (pf.Tipo == CAM_PERFIL_TIPO.Z_Purlin | pf.Tipo == CAM_PERFIL_TIPO.C_Enrigecido)
            {
                var face = shape.GetLIV3_MesaParaChapa();
                if (face.Liv.Count == 0)
                {
                    face = new Face(shape.LIV2.Comprimento, pf.Aba_I, pf.Esp, shape, FaceNum.LIV3);
                }
                var pts = face.Liv.GroupBy(x => x.Origem.Y).OrderBy(x => x.Key).ToList();
                var max = pts.Min(x => x.Key);

                var pt = pts.Find(x => x.Key == max);

                return new Face(pt.Max(x => x.Origem.X) - pt.Min(x => x.Origem.X), pf.Aba_I - (descontar_dobra ? pf.Esp : 0), pf.Esp, shape, FaceNum.LIV4).MoverX(pt.Min(x => x.Origem.X));
            }
            return new Face(
                new List<Liv>(),
                FaceNum.LIV4
                );
        }
        public static Face LIV2_Aba_Menor(this Shape shape, bool descontar_dobra)
        {
            var pf = shape.Perfil;
            if (pf.Tipo == CAM_PERFIL_TIPO.Z_Purlin | pf.Tipo == CAM_PERFIL_TIPO.C_Enrigecido && shape.LIV2.Liv.Count > 3)
            {
                var pts = shape.GetLIV2_MesaParaChapa().RebaterY().Liv.GroupBy(x => x.Origem.Y).OrderBy(x => x.Key).ToList();
                var max = pts.Max(x => x.Key);

                var pt = pts.Find(x => x.Key == max);
                var x0 = pt.Min(x => x.Origem.X);


                var ret = new Face(pt.Max(x => x.Origem.X) - pt.Min(x => x.Origem.X), pf.Aba_S - (descontar_dobra ? pf.Esp : 0), pf.Esp, shape, FaceNum.LIV4).MoverX(x0);
                return ret;
            }
            return new Face(
                new List<Liv>(),
                FaceNum.LIV4);
        }
        public static Face LIV1_SemBordas(this Shape shape)
        {
            return shape.LIV1.GetFaceSemBordas(shape.Perfil);
        }
    }
}
