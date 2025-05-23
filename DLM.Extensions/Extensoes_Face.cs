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
    public static class Extensoes_Face
    {

        public static double GetPeso(this Face face, Perfil Perfil)
        {
            if (Perfil.Cilindro)
            {
                return (Perfil.GetPesoMetro() * face.Comprimento / 1000).Round(Cfg.Init.TEC_DECIMAIS_PESO_MARCAS);
            }
            else
            {
                var esp = face.GetEspessura(Perfil);
                var area = face.GetArea();
                var peso = esp * area * Cfg.Init.Peso_Especifico;
                if (Cfg.Init.CAM_DescontarPesoRecortesInternos)
                {
                    var peso_rec = face.RecortesInternos.Sum(x => x.GetPeso(face.GetEspessura(Perfil)));
                    peso = peso - peso_rec;
                }

                return peso;
            }
        }
        public static Face RebaterY(this Face face,OrigemLiv OrigemLiv = OrigemLiv.Cima_Baixo)
        {
            var pts = new List<Liv>();
            var frs = new List<Furo>();
            var recs = new List<Recorte>();
            var soldas = new List<Solda>();
            var dobs = new List<Dobra>();
            if (OrigemLiv == OrigemLiv.Cima_Baixo | OrigemLiv == OrigemLiv.Baixo_Cima | OrigemLiv == OrigemLiv.Cima_BaixoRebatido)
            {
                pts = face.Liv.Select(x => x.SetY(-x.Origem.Y - face.Largura)).ToList();
                frs = face.Furacoes.Select(x => x.SetY(-x.Origem.Y - face.Largura)).ToList();
                recs = face.RecortesInternos.Select(x => x.SetY(-x.Origem.Y - face.Largura)).ToList();
                soldas = face.Soldas.Select(x => x.SetY(-x.Linha.P1.Y - face.Largura, -x.Linha.P2.Y - face.Largura)).ToList();
            }
            else if (OrigemLiv == OrigemLiv.Centro)
            {
                pts = face.Liv.Select(x => x.SetY(-x.Origem.Y + face.Largura)).ToList();
                frs = face.Furacoes.Select(x => x.SetY(-x.Origem.Y)).ToList();
                recs = face.RecortesInternos.Select(x => x.SetY(Math.Abs(x.Origem.Y) - face.Largura)).ToList();
                soldas = face.Soldas.Select(x => x.SetY(Math.Abs(x.Linha.P1.Y) - face.Largura, Math.Abs(x.Linha.P2.Y) - face.Largura)).ToList();
            }

            foreach (var furo in frs)
            {
                if (furo.Angulo != 0)
                {
                    furo.Angulo = 180 - furo.Angulo;
                }
            }

            return new Face(
                pts,
                face.FaceNum,
                recs,
                frs,
                dobs,
                soldas);
        }
        public static Face RebaterX(this Face face)
        {
            var pts = new List<Liv>();
            var frs = new List<Furo>();
            var recs = new List<Recorte>();
            var soldas = new List<Solda>();
            var dobs = new List<Dobra>();
            pts = face.Liv.Select(x => x.SetX(face.Comprimento - x.Origem.X)).ToList();
            frs = face.Furacoes.Select(x => x.SetX(face.Comprimento - x.Origem.X)).ToList();
            recs = face.RecortesInternos.Select(x => x.SetX(face.Comprimento - x.Origem.X)).ToList();
            soldas = face.Soldas.Select(x => x.Clonar()).ToList();
            foreach(var furo in frs)
            {
                if(furo.Angulo!=0)
                {
                    furo.Angulo = 180 - furo.Angulo;
                }
            }
            return new Face(
                pts,
                face.FaceNum,
                recs,
                frs,
                dobs,
                soldas);
        }
        public static Face AlinharFuros_X(this Face face,double tolerancia)
        {
            var ptos = new List<Liv>();
            var fros = new List<Furo>();
            var recs = new List<Recorte>();
            var sold = new List<Solda>();
            var dobs = new List<Dobra>();
            ptos = face.Liv.Select(x => x.Clonar()).ToList();
            fros = face.Furacoes.Select(x => x.Clonar()).ToList().AlinharX(tolerancia);
            recs = face.RecortesInternos.Select(x => x.Clonar()).ToList();
            sold = face.Soldas.Select(x => x.Clonar()).ToList();

            return new Face(
                ptos,
                face.FaceNum,
                recs,
                fros,
                dobs,
                sold);
        }
        public static Face AjustarOrigens(this Face face)
        {
            if (face.Mesa)
            {
                return face.MoverY(0);
            }
            double x0, y0;
            var l0 = face.LivSegmentada.Zerar(out x0, out y0);
            return new Face(
                l0,
                face.FaceNum,
                face.RecortesInternos.Select(x => x.MoverX(-x0).MoverY(-y0)).ToList(),
                face.Furacoes.Select(x => x.MoverX(-x0).MoverY(-y0)).ToList(),
                face.Dobras.Select(x => x.MoverX(-x0).MoverY(-y0)).ToList(),
                face.Soldas.Select(x => x.MoverX(-x0).MoverY(-y0)).ToList()
                )
               ;
        }
        public static Face MesaParaChapa(this Face face, Perfil Perfil)
        {

            if (face.Mesa)
            {
                var liv = face.LivSegmentada.MesaParaChapa(false);
                return new Face(
                    liv,
                    FaceNum.LIV1,
                    face.RecortesInternos.Select(x => x.Clonar()).ToList(),
                    face.FurosMesaParaChapa(Perfil),
                    face.Dobras,
                    face.Soldas.Select(x => x.MesaParaChapa()).ToList());
            }
            else
            {
                var pp = new Face(
                    face.Liv,
                    FaceNum.LIV1,
                    face.RecortesInternos,
                    face.Furacoes,
                    face.Dobras,
                    face.Soldas
                    );
                return pp;
            }
        }
        public static Face GetFaceSemBordas(this Face face, Perfil Perfil)
        {
            var ps = Perfil;
            double descontar = 0.1;
            List<Liv> l1;
            List<Liv> l2;



            if ((face.FaceNum == FaceNum.LIV1 | face.FaceNum == FaceNum.LIV4))
            {
                var furos_desloc_mesaS = face.FurosMesaParaChapa(Perfil).Select(x => x.MoverY(ps.Esp_MI)).ToList();
                if (
                      Perfil.Tipo == CAM_PERFIL_TIPO.I_Soldado
                    | Perfil.Tipo == CAM_PERFIL_TIPO.Caixao
                    | Perfil.Tipo == CAM_PERFIL_TIPO.Cartola
                    | Perfil.Tipo == CAM_PERFIL_TIPO.C_Enrigecido
                    | Perfil.Tipo == CAM_PERFIL_TIPO.INP
                    | Perfil.Tipo == CAM_PERFIL_TIPO.UAP
                    | Perfil.Tipo == CAM_PERFIL_TIPO.UNP
                    | Perfil.Tipo == CAM_PERFIL_TIPO.U_Dobrado
                    | Perfil.Tipo == CAM_PERFIL_TIPO.WLam
                    | Perfil.Tipo == CAM_PERFIL_TIPO.Z_Dobrado
                    | Perfil.Tipo == CAM_PERFIL_TIPO.Z_Purlin
                    | Perfil.Tipo == CAM_PERFIL_TIPO.Tubo_Quadrado
                    )
                {


                    face.LivSegmentada.Quebrar(
                        new P3d(-face.Comprimento * 10, ps.Esp_MI + descontar),
                        new P3d(face.Comprimento * 10, ps.Esp_MI + descontar),
                        out l1, out l2, true, false);
                    l2.Quebrar(
                        new P3d(-face.Comprimento * 10, ps.Altura - ps.Esp_MI - ps.Esp_MS - 2 * descontar),
                        new P3d(face.Comprimento * 10, ps.Altura - ps.Esp_MI - ps.Esp_MS - 2 * descontar),
                        out l1, out l2, true, false);


                    if (l1.Count > 3)
                    {
                        var s = new Face(l1,
                            FaceNum.LIV1_Recortado,
                            face.RecortesInternos.Select(x => x.MoverY(Perfil.Esp_MI)).ToList(), furos_desloc_mesaS,
                            face.Dobras.Select(x => x.MoverY(Perfil.Esp_MI)).ToList(),
                            face.Soldas.Select(x => x.MoverY(Perfil.Esp_MI)).ToList());
                        return s;
                    }
                }
                else if (Perfil.Tipo == CAM_PERFIL_TIPO.L_Dobrado | Perfil.Tipo == CAM_PERFIL_TIPO.L_Laminado)
                {
                    face.LivSegmentada.Quebrar(
                         new P3d(-face.Comprimento * 10, ps.Esp + descontar),
                         new P3d(face.Comprimento * 10, ps.Esp + descontar), out l1, out l2, true, false);

                    //ver se tem que descontar invertido
                    if (l2.Count > 3)
                    {
                        var s = new Face(l2,
                            FaceNum.LIV1_Recortado,
                            face.RecortesInternos.Select(x => x.MoverY(Perfil.Esp)).ToList(), furos_desloc_mesaS,
                            face.Dobras.Select(x => x.MoverY(Perfil.Esp)).ToList(),
                            face.Soldas.Select(x => x.MoverY(Perfil.Esp)).ToList());
                        return s;
                    }
                }
                else if (Perfil.Tipo == CAM_PERFIL_TIPO.T_Soldado)
                {
                    face.LivSegmentada.Quebrar(
                        new P3d(-face.Comprimento * 10, ps.Esp_M + descontar),
                        new P3d(face.Comprimento * 10, ps.Esp_M + descontar), out l1, out l2, true, false);
                    if (l2.Count > 3)
                    {
                        var nface = new Face(l2,
                            FaceNum.LIV1_Recortado,
                            face.RecortesInternos.Select(x => x.MoverY(Perfil.Esp_M)).ToList(),
                            furos_desloc_mesaS,
                            face.Dobras.Select(x => x.MoverY(Perfil.Esp_M)).ToList(),
                            face.Soldas.Select(x => x.MoverY(Perfil.Esp_M)).ToList());
                        return nface;
                    }
                }




            }
            /*No caso as mesas (LIV2 e LIV3 não precisa descontar)*/
            return new Face(
                face.LivSegmentada,
                face.FaceNum,
                face.RecortesInternos,
                face.Furacoes,
                face.Dobras,
                face.Soldas);
        }
        public static Face Deformar(this Face face, double ContraFlecha)
        {

            var Retorno = new List<Liv>();
            double Comprimento = face.Comprimento;
            double Largura = face.Largura;
            var LivDeformado = new List<Liv>();
            foreach (Liv L in face.Liv)
            {
                Retorno.Add(new Liv(L));
            }
            Retorno.Aninhar();
            #region Separa a lista de coordenadas em 2 horizontalmente
            var Superior2 = new List<Liv>();
            var Inferior2 = new List<Liv>();
            Retorno.DividirHorizontal(out Superior2, out Inferior2);
            #endregion

            //ponto onde inserir a linha do contra flecha - lado superior
            int sup1 = -1;
            int sup2 = -1;
            double supdist = -1;
            //ponto onde inserir a linha do contra flecha - lado inferior
            int inf1 = -1;
            int inf2 = -1;
            double infdist = -1;
            #region Acha os pontos onde inserir a linha do contra-flecha
            for (int i = 0; i < Retorno.Count; i++)
            {
                if (i == Retorno.Count - 1)
                {
                    Liv p1 = Retorno[i];
                    Liv p2 = Retorno[0];
                    if (Superior2.Find(x => x.GetCid() == p1.GetCid()) != null && Superior2.Find(x => x.GetCid() == p2.GetCid()) != null)
                    {
                        double dist = p1.Origem.Distancia(p2.Origem).Abs();
                        if (supdist < dist)
                        {
                            supdist = dist;
                            sup1 = i;
                            sup2 = 0;
                        }

                    }

                }
                else
                {
                    var p1 = Retorno[i];
                    var p2 = Retorno[i + 1];
                    if (Superior2.Find(x => x.GetCid() == p1.GetCid()) != null && Superior2.Find(x => x.GetCid() == p2.GetCid()) != null)
                    {
                        double dist = p1.Origem.Distancia(p2.Origem).Abs();
                        if (supdist < dist)
                        {
                            supdist = dist;
                            sup1 = i;
                            sup2 = i + 1;
                        }

                    }
                }
            }

            //10-05-2018 - removi o -1 da contagem por causa do problema que tava dando nas primeiras linhas
            for (int i = 0; i < Retorno.Count; i++)
            {
                if (i == Retorno.Count - 1)
                {
                    var p1 = Retorno[i];
                    var p2 = Retorno[0];
                    if (Inferior2.Find(x => x.GetCid() == p1.GetCid()) != null && Inferior2.Find(x => x.GetCid() == p2.GetCid()) != null)
                    {
                        double dist = p1.Origem.Distancia(p2.Origem).Abs();
                        if (infdist < dist)
                        {
                            infdist = dist;
                            inf1 = i;
                            inf2 = 0;
                        }

                    }

                }
                else
                {
                    var p1 = Retorno[i];
                    var p2 = Retorno[i + 1];
                    if (Inferior2.Find(x => x.GetCid() == p1.GetCid()) != null && Inferior2.Find(x => x.GetCid() == p2.GetCid()) != null)
                    {
                        double dist = p1.Origem.Distancia(p2.Origem).Abs();
                        if (infdist < dist)
                        {
                            infdist = dist;
                            inf1 = i;
                            inf2 = i + 1;
                        }
                    }
                }
            }
            #endregion

            if (sup1 >= 0 && sup2 >= 0 && inf1 >= 0 && inf2 >= 0)
            {
                //acha a diferença y do ponto inicial e final
                double psup1y = FuncoesDLMCam.Arco.DifY(Comprimento, ContraFlecha, Retorno[sup1].Origem.X);
                double psup2y = FuncoesDLMCam.Arco.DifY(Comprimento, ContraFlecha, Retorno[sup2].Origem.X);

                double pinf1y = FuncoesDLMCam.Arco.DifY(Comprimento, ContraFlecha, Retorno[inf1].Origem.X);
                double pinf2y = FuncoesDLMCam.Arco.DifY(Comprimento, ContraFlecha, Retorno[inf2].Origem.X);

                double Raio = FuncoesDLMCam.Arco.Raio(ContraFlecha, Comprimento);

                //double AnguloSup = Funcoes.Graus(Math.Atan(Funcoes.Arco.DifY(supdist, ContraFlecha, 1) / 1));
                //double AnguloInf = Funcoes.Graus(Math.Atan(Funcoes.Arco.DifY(infdist, ContraFlecha, 1) / 1));

                double AnguloSup = FuncoesDLMCam.Arco.Angulo2(Raio, supdist) / 2;
                double AnguloInf = FuncoesDLMCam.Arco.Angulo2(Raio, infdist) / 2;



                Liv sup = new Liv(Comprimento / 2, -Raio, 0, Raio, 90 - AnguloSup, 90 + AnguloSup);
                Liv inf = new Liv(Comprimento / 2, -Raio - Largura, 0, Raio, 90 - AnguloInf, 90 + AnguloInf);

                for (int i = 0; i < Retorno.Count; i++)
                {
                    Retorno[i].Origem.Y = Retorno[i].Origem.Y - ContraFlecha;
                }

                //ajusta diferenças em Y
                Retorno[sup1].Origem.Y = Retorno[sup1].Origem.Y + FuncoesDLMCam.Arco.DifY(Comprimento, ContraFlecha, Retorno[sup1].Origem.X);
                Retorno[sup2].Origem.Y = Retorno[sup2].Origem.Y + FuncoesDLMCam.Arco.DifY(Comprimento, ContraFlecha, Retorno[sup2].Origem.X);

                Retorno[inf1].Origem.Y = Retorno[inf1].Origem.Y + FuncoesDLMCam.Arco.DifY(Comprimento, ContraFlecha, Retorno[inf1].Origem.X);
                Retorno[inf2].Origem.Y = Retorno[inf2].Origem.Y + FuncoesDLMCam.Arco.DifY(Comprimento, ContraFlecha, Retorno[inf2].Origem.X);



                for (int i = 0; i < Retorno.Count; i++)
                {
                    LivDeformado.Add(Retorno[i]);
                    if (i == sup1)
                    {
                        LivDeformado.Add(sup);
                    }
                    if (i == inf1)
                    {
                        LivDeformado.Add(inf);
                    }
                }
            }




            var furos = new List<Furo>();
            var recortes = new List<Recorte>();
            foreach (Furo furo in face.Furacoes)
            {
                //Deslocamento Inicial
                var FR = furo.Clonar();
                FR.Origem.Y = FR.Origem.Y - ContraFlecha;

                double offY = FuncoesDLMCam.Arco.DifY(Comprimento, ContraFlecha, FR.Origem.X);
                FR.Origem.Y = FR.Origem.Y + offY;
                if (Cfg.Init.CAM_CTF_AlinharFuros)
                {
                    if (FR.Validado == false)
                    {
                        FR.Validado = true;
                        var Proximos = face.Furacoes.FindAll(x => Math.Abs(Math.Abs(x.Origem.Y) - Math.Abs(FR.Origem.Y)) <= Cfg.Init.CAM_CTF_AlinharFurosTolerancia && x.Validado == false);
                        foreach (Furo F in Proximos)
                        {
                            F.Origem.Y = FR.Origem.Y;
                            F.Validado = true;
                        }
                    }
                }
                furos.Add(FR);
            }


            foreach (Recorte recorte in face.RecortesInternos)
            {
                //Deslocamento Inicial
                var FR = recorte.MoverY(-ContraFlecha);
                double offY = FuncoesDLMCam.Arco.DifY(Comprimento, ContraFlecha, FR.Origem.X);
                FR = FR.MoverY(offY);
                recortes.Add(FR);
            }

            var o = new Face(
                LivDeformado,
                FaceNum.LIV1,
                recortes,
                furos);
            return o;
        }
        public static Face Cortar(this Face face, double cima, double baixo)
        {
            double descontar = 0.0;
            List<Liv> l1;
            List<Liv> l2;
            if (cima > 0 | baixo > 0)
            {
                double xmin = 0;
                double ymax = 0;
                var l0 = face.LivSegmentada.Zerar(out xmin, out ymax);
                l0.Quebrar(new P3d(-face.Comprimento * 10, cima + descontar), new P3d(face.Comprimento * 10, cima + descontar), out l1, out l2, true, false);
                if (baixo == 0)
                {
                    double x2, y2;
                    l0 = l2.Zerar(out x2, out y2);
                    if (x2 > xmin)
                    {
                        //só os cartolas caem aqui, porque são tudo bugado.
                        xmin = x2;
                        ymax = y2;
                    }

                }
                l2.Quebrar(
                    new P3d(-face.Comprimento * 10, face.Largura - cima - baixo - 2 * descontar),
                    new P3d(face.Comprimento * 10, face.Largura - cima - baixo - 2 * descontar), out l1, out l2, true, false);


                if (l1.Count > 3)
                {
                    var s = new Face(
                        l1,
                        FaceNum.LIV1_Recortado,

                        face.RecortesInternos.Select(x => x.MoverY(cima).MoverX(-xmin)).ToList(),
                        face.Furacoes.Select(x => x.MoverY(cima).MoverX(-xmin)).ToList(),
                        face.Dobras.Select(x => x.MoverY(cima)).ToList(),
                        face.Soldas.Select(x => x.MoverY(cima)).ToList());
                    return s.MoverX(xmin);
                }
            }

            return new Face(
                face.Liv,
                face.FaceNum,
                face.RecortesInternos,
                face.Furacoes,
                face.Dobras,
                face.Soldas);
        }
        public static Face MoverX(this Face face, double valor)
        {
            var nFace = new Face(
                face.Liv.Select(x => x.MoverX(valor)).ToList(),
                face.FaceNum,
                face.RecortesInternos.Select(x => x.MoverX(valor)).ToList(),
                face.Furacoes.Select(x => x.MoverX(valor)).ToList(),
                face.Dobras.Select(x => x.MoverX(valor)).ToList(),
                face.Soldas.Select(x => x.MoverX(valor)).ToList());
            return nFace;
        }
        public static Face MoverY(this Face face, double valor)
        {
            var nFace = new Face(
                face.Liv.Select(x => x.MoverY(valor)).ToList(),
                face.FaceNum,
                face.RecortesInternos.Select(x => x.MoverY(valor)).ToList(),
                face.Furacoes.Select(x => x.MoverY(valor)).ToList(),
                face.Dobras.Select(x => x.MoverY(valor)).ToList(),
                face.Soldas.Select(x => x.MoverY(valor)).ToList());
            return nFace;
        }
        public static double GetArea(this Face face, bool cilindro = false)
        {
            if (cilindro)
            {
                double areaCirculo = 2 * Math.PI * (face.Largura / 2) + face.Comprimento;
                return areaCirculo;
            }
            else
            {
                return face.LivSegmentada.Area();
            }
        }
        public static Face Rotacionar(this Face face, double angulo)
        {
            var origem = face.Origem.Clonar();

            var nLivs = new List<Liv>();
            var nFuracoes = new List<Furo>();
            var nRecortes = new List<Recorte>();
            var nSoldas = new List<Solda>();
            var nDobras = new List<Dobra>();

            foreach(var liv in face.Liv)
            {
                var nliv = liv.Clonar();
                nliv.Origem = nliv.Origem.Rotacionar(origem, angulo);
                nLivs.Add(nliv);
            }
            foreach(var furo in face.Furacoes)
            {
                var nfuro = furo.Clonar();
                nfuro.Origem = nfuro.Origem.Rotacionar(origem, angulo);
                if(nfuro.Oblongo>0)
                {
                    nfuro.Angulo += angulo;
                    nfuro.Angulo = nfuro.Angulo.Normalizar(180);
                }
                nFuracoes.Add(nfuro);
            }
            foreach(var recorte in face.RecortesInternos)
            {
                var livs = recorte.Coordenadas.Segmentar();



                foreach (var pt in livs)
                {
                    pt.Origem = pt.Origem.Rotacionar(origem, angulo);
                }

                var ncorte = new Recorte(livs);
                nRecortes.Add(ncorte);
            }
            //falta fazer girar
            foreach (var solda in face.Soldas)
            {
                var nsolda = solda.Clonar();
                //soldas.Add(nsolda);
            }
            //falta fazer girar
            foreach (var dobra in face.Dobras)
            {
                var ndobra = dobra.Clonar();
                //dobras.Add(ndobra);
            }

            var nface = new Face(nLivs, face.FaceNum, nRecortes, nFuracoes, nDobras, nSoldas);

            return nface;
        }
        public static List<Furo> RebaterFuros(this Face face, Sentido_Espelho sentido)
        {
            var comp = face.Comprimento;
            var larg = face.Largura;

            var furos = new List<Furo>();

            foreach (var fr in face.Furacoes)
            {
                var novo = fr.Clonar();
                switch (sentido)
                {
                    case Sentido_Espelho.X:
                        novo.Origem.X = comp - novo.Origem.X;
                        break;
                    case Sentido_Espelho.Y:
                        if (face.Mesa)
                        {
                            novo.Origem.Y = -novo.Origem.Y;
                        }
                        else
                        {
                            novo.Origem.Y = -larg - novo.Origem.Y;
                        }
                        break;
                    case Sentido_Espelho.XY:
                        novo.Origem.X = comp - novo.Origem.X;
                        if (face.Mesa)
                        {
                            novo.Origem.Y = -novo.Origem.Y;
                        }
                        else
                        {
                            novo.Origem.Y = -larg - novo.Origem.Y;
                        }
                        break;
                }

                furos.Add(novo);
            }

            return furos;
        }
    }
}
