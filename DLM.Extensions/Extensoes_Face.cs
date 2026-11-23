using DLM.cam;
using DLM.desenho;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Conexoes
{
    public static class Extensoes_Face
    {
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

        public static Face RebaterY(this Face face, OrigemLiv OrigemLiv = OrigemLiv.Cima_Baixo)
        {
            var pts = new List<Liv>();
            var frs = new List<Furo>();
            var recs = new List<Recorte>();
            var soldas = new List<Solda>();
            var dobs = new List<Dobra>();
            if (
                   OrigemLiv == OrigemLiv.Cima_Baixo 
                || OrigemLiv == OrigemLiv.Baixo_Cima 
                || OrigemLiv == OrigemLiv.Cima_BaixoRebatido
                )
            {
                pts = face.Liv.Select(x => x.SetY(-x.Origem.Y - face.Largura)).ToList();
                frs = face.Furacoes.Select(x => x.SetY(-x.Origem.Y - face.Largura)).ToList();
                recs = face.RecortesInternos.Select(x => x.SetY(-x.Origem.Y - face.Largura)).ToList();
                soldas = face.Soldas.Select(x => x.SetY(-x.Linha.P1.Y - face.Largura, -x.Linha.P2.Y - face.Largura)).ToList();
            }
            else if (OrigemLiv == OrigemLiv.Centro)
            {
                pts = face.Liv.Select(x => x.SetZ(x.Origem.Z * -1)).ToList();
                frs = face.Furacoes.Select(x => x.SetY(x.Origem.Y * -1)).ToList();
                recs = face.RecortesInternos.Select(x => x.SetY(Math.Abs(x.Origem.Y) * -1)).ToList();
                soldas = face.Soldas.Select(x => x.SetY(Math.Abs(x.Linha.P1.Y) * -1, Math.Abs(x.Linha.P2.Y) * -1)).ToList();
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
        public static Face AlinharFuros_X(this Face face, double tolerancia)
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
            if (face.IsMesa())
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
        public static Face MesaParaChapa(this Face face, Perfil Perfil, double comprimento, double largura, double esp)
        {

            if (face.IsMesa())
            {
                var liv = face.LivSegmentada.MesaParaChapa(false);
                if (liv.Count == 0)
                {
                    liv = Retangulo.New(comprimento, largura, esp).Get(TipoREC.Cima_Baixo);
                }
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
                var liv = new List<Liv>();
                liv.AddRange(face.Liv);
                if (liv.Count == 0)
                {
                    liv = Retangulo.New(comprimento, largura, esp).Get(TipoREC.Cima_Baixo);
                }
                var pp = new Face(
                    liv,
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
                    || Perfil.Tipo == CAM_PERFIL_TIPO.Caixao
                    || Perfil.Tipo == CAM_PERFIL_TIPO.Cartola
                    || Perfil.Tipo == CAM_PERFIL_TIPO.C_Enrigecido
                    || Perfil.Tipo == CAM_PERFIL_TIPO.INP
                    || Perfil.Tipo == CAM_PERFIL_TIPO.UAP
                    || Perfil.Tipo == CAM_PERFIL_TIPO.UNP
                    || Perfil.Tipo == CAM_PERFIL_TIPO.U_Dobrado
                    || Perfil.Tipo == CAM_PERFIL_TIPO.WLam
                    || Perfil.Tipo == CAM_PERFIL_TIPO.Z_Dobrado
                    || Perfil.Tipo == CAM_PERFIL_TIPO.Z_Purlin
                    || Perfil.Tipo == CAM_PERFIL_TIPO.Tubo_Quadrado
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
                else if (Perfil.Tipo == CAM_PERFIL_TIPO.L_Dobrado || Perfil.Tipo == CAM_PERFIL_TIPO.L_Laminado)
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
            return Deformar(face, ContraFlecha, null, true);
        }

        /// <summary>
        /// Aplica contra-flecha (arco circular) em uma Face de Chapa, deformando todos os vértices do contorno,
        /// furos, recortes internos e soldas em torno de um centro de distorção configurável (centroDistorsaoX).
        /// </summary>
        public static Face Deformar(this Face face, double ContraFlecha, double? centroDistorsaoX, bool arcoParaCima = true)
        {
            if (face == null || Math.Abs(ContraFlecha) < 0.01)
            {
                return face;
            }

            double f = Math.Abs(ContraFlecha);
            var origVerts = new List<Liv>();
            var fonteLiv = (face.Liv != null && face.Liv.Count >= 3) ? face.Liv : face.LivSegmentada;
            if (fonteLiv == null || fonteLiv.Count < 3)
            {
                return face;
            }

            foreach (var L in fonteLiv)
            {
                if (L.Raio > 0.05) continue; // ignora centros de arco anteriores
                origVerts.Add(new Liv(L));
            }
            if (origVerts.Count < 3) return face;

            // Remove ponto de fechamento repetido se existir
            bool fechado = Math.Abs(origVerts[0].Origem.X - origVerts[origVerts.Count - 1].Origem.X) < 0.05 &&
                           Math.Abs(origVerts[0].Origem.Y - origVerts[origVerts.Count - 1].Origem.Y) < 0.05;
            if (fechado && origVerts.Count > 3)
            {
                origVerts.RemoveAt(origVerts.Count - 1);
            }

            double minX = origVerts.Min(v => v.Origem.X);
            double maxX = origVerts.Max(v => v.Origem.X);
            double minY = origVerts.Min(v => v.Origem.Y);
            double maxY = origVerts.Max(v => v.Origem.Y);
            double comp = Math.Max(1.0, maxX - minX);
            double larg = Math.Max(1.0, maxY - minY);

            double x0 = centroDistorsaoX.HasValue
                ? Math.Max(minX, Math.Min(maxX, centroDistorsaoX.Value))
                : (minX + comp * 0.5);

            double lMax = Math.Max(x0 - minX, maxX - x0);
            if (lMax < 1.0) lMax = comp * 0.5;

            // Raio exato do arco circular passando por (x0, 0) com flecha f em lMax
            double raio = (f * f + lMax * lMax) / (2.0 * f);

            Func<double, double> deslocY = (x) =>
            {
                double dx = x - x0;
                double rad = raio * raio - dx * dx;
                double yArco = (rad > 0 ? Math.Sqrt(rad) : 0.0) - (raio - f);
                return arcoParaCima ? (-f + yArco) : (-yArco);
            };

            // Deforma TODOS os vértices do polígono
            var defVerts = new List<Liv>(origVerts.Count);
            for (int i = 0; i < origVerts.Count; i++)
            {
                var v = new Liv(origVerts[i]);
                v.Raio = 0;
                v.Ang1 = 0;
                v.Ang2 = 0;
                v.Origem.Y = Math.Round(v.Origem.Y + deslocY(v.Origem.X), 2);
                defVerts.Add(v);
            }

            // Reconstrói o contorno inserindo o arco circular exato nos segmentos horizontais/principais (superior e inferior)
            var livDeformado = new List<Liv>();
            int n = defVerts.Count;
            double minSegHorizontal = Math.Max(40.0, comp * 0.20);

            for (int i = 0; i < n; i++)
            {
                var p1Orig = origVerts[i];
                var p2Orig = origVerts[(i + 1) % n];
                var p1 = defVerts[i];
                var p2 = defVerts[(i + 1) % n];

                livDeformado.Add(new Liv(p1));

                double dxOrig = Math.Abs(p2Orig.Origem.X - p1Orig.Origem.X);
                double dyOrig = Math.Abs(p2Orig.Origem.Y - p1Orig.Origem.Y);

                // Segmento longitudinal (horizontal ou suavemente inclinado) recebe o arco circular exato de raio R
                if (dxOrig >= minSegHorizontal && dyOrig <= Math.Max(8.0, dxOrig * 0.25))
                {
                    double x1 = p1.Origem.X, y1 = p1.Origem.Y;
                    double x2 = p2.Origem.X, y2 = p2.Origem.Y;
                    double chord = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
                    if (chord > 1.0 && chord < 2.0 * raio - 0.1)
                    {
                        double mx = (x1 + x2) * 0.5;
                        double my = (y1 + y2) * 0.5;
                        double h = Math.Sqrt(Math.Max(0.0, raio * raio - (chord * 0.5) * (chord * 0.5)));

                        // Normal unitária perpendicular à corda apontando para o centro da curvatura
                        double nx = -(y2 - y1) / chord;
                        double ny = (x2 - x1) / chord;
                        // Para arcoParaCima, o centro do círculo está ABAIXO (ny < 0); para baixo, ACIMA (ny > 0)
                        if ((arcoParaCima && ny > 0) || (!arcoParaCima && ny < 0))
                        {
                            nx = -nx;
                            ny = -ny;
                        }

                        double cx = mx + nx * h;
                        double cy = my + ny * h;

                        double a1 = Math.Atan2(y1 - cy, x1 - cx) * 180.0 / Math.PI;
                        double a2 = Math.Atan2(y2 - cy, x2 - cx) * 180.0 / Math.PI;
                        if (a1 < 0) a1 += 360.0;
                        if (a2 < 0) a2 += 360.0;

                        // Na convenção DXF/CAM, Ang1 -> Ang2 sempre percorre CCW; escolher o menor arco (<= 180°)
                        double sweep12 = a2 - a1;
                        if (sweep12 <= 0) sweep12 += 360.0;
                        double angIni, angFim;
                        if (sweep12 <= 180.0)
                        {
                            angIni = a1;
                            angFim = a2;
                        }
                        else
                        {
                            angIni = a2;
                            angFim = a1;
                        }

                        livDeformado.Add(new Liv(Math.Round(cx, 2), Math.Round(cy, 2), 0, Math.Round(raio, 2), Math.Round(angIni, 2), Math.Round(angFim, 2)));
                    }
                }
            }

            // Fecha o polígono no primeiro vértice
            if (livDeformado.Count > 0)
            {
                livDeformado.Add(new Liv(livDeformado[0]));
            }

            // Deforma furos (clonando sem mutar face.Furacoes original)
            var furos = new List<Furo>();
            if (face.Furacoes != null)
            {
                foreach (var furo in face.Furacoes)
                {
                    var fr = furo.Clonar();
                    fr.Origem.Y = Math.Round(fr.Origem.Y + deslocY(fr.Origem.X), 2);
                    furos.Add(fr);
                }

                if (Cfg.Init.CAM_CTF_AlinharFuros && furos.Count > 1)
                {
                    foreach (var fr in furos)
                    {
                        if (fr.Validado) continue;
                        fr.Validado = true;
                        var proximos = furos.FindAll(x => !x.Validado && Math.Abs(Math.Abs(x.Origem.Y) - Math.Abs(fr.Origem.Y)) <= Cfg.Init.CAM_CTF_AlinharFurosTolerancia);
                        foreach (var p in proximos)
                        {
                            p.Origem.Y = fr.Origem.Y;
                            p.Validado = true;
                        }
                    }
                }
            }

            // Deforma recortes internos pelo centro de cada recorte (preservando círculos/retângulos sem distorcer)
            var recortes = new List<Recorte>();
            if (face.RecortesInternos != null)
            {
                foreach (var rec in face.RecortesInternos)
                {
                    double offRec = deslocY(rec.Origem.X);
                    recortes.Add(rec.MoverY(Math.Round(offRec, 2)));
                }
            }

            // Deforma soldas proporcionalmente
            var soldas = new List<Solda>();
            if (face.Soldas != null)
            {
                foreach (var s in face.Soldas)
                {
                    var sc = s.Clonar();
                    sc.Linha.P1.Y = Math.Round(sc.Linha.P1.Y + deslocY(sc.Linha.P1.X), 2);
                    sc.Linha.P2.Y = Math.Round(sc.Linha.P2.Y + deslocY(sc.Linha.P2.X), 2);
                    soldas.Add(sc);
                }
            }

            // Normaliza Y para que o topo máximo da chapa deformada fique em Y = 0
            double topoMax = livDeformado.Where(v => v.Raio <= 0.05).Max(v => v.Origem.Y);
            if (Math.Abs(topoMax) > 0.05)
            {
                double ajuste = -topoMax;
                foreach (var lv in livDeformado) lv.Origem.Y = Math.Round(lv.Origem.Y + ajuste, 2);
                foreach (var fr in furos) fr.Origem.Y = Math.Round(fr.Origem.Y + ajuste, 2);
                recortes = recortes.Select(r => r.MoverY(Math.Round(ajuste, 2))).ToList();
                foreach (var s in soldas)
                {
                    s.Linha.P1.Y = Math.Round(s.Linha.P1.Y + ajuste, 2);
                    s.Linha.P2.Y = Math.Round(s.Linha.P2.Y + ajuste, 2);
                }
            }

            var nFace = new Face(livDeformado, FaceNum.LIV1, recortes, furos, face.Dobras, soldas);
            return nFace;
        }
        public static Face Cortar(this Face face, double cima, double baixo)
        {
            double descontar = 0.0;
            List<Liv> l1;
            List<Liv> l2;
            if (cima > 0 || baixo > 0)
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
        public static Face Rotacionar(this Face face, double angulo)
        {
            var origem = face.Origem.Clonar();

            var nLivs = new List<Liv>();
            var nFuracoes = new List<Furo>();
            var nRecortes = new List<Recorte>();
            var nSoldas = new List<Solda>();
            var nDobras = new List<Dobra>();

            foreach (var liv in face.Liv)
            {
                var nliv = liv.Clonar();
                nliv.Origem = nliv.Origem.Rotacionar(origem, angulo);
                nLivs.Add(nliv);
            }
            foreach (var furo in face.Furacoes)
            {
                var nfuro = furo.Clonar();
                nfuro.Origem = nfuro.Origem.Rotacionar(origem, angulo);
                if (nfuro.Oblongo > 0)
                {
                    nfuro.Angulo += angulo;
                    nfuro.Angulo = nfuro.Angulo.Normalizar(180);
                }
                nFuracoes.Add(nfuro);
            }
            foreach (var recorte in face.RecortesInternos)
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
        public static List<Furo> GetFurosRebatidos(this Face face, Sentido_Espelho sentido)
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
                        if (face.IsMesa())
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
                        if (face.IsMesa())
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

        /// <summary>
        /// Funciona somente em LIV1
        /// </summary>
        /// <param name="faces"></param>
        /// <param name="folga_Y"></param>
        /// <returns></returns>
        public static Face UnirComFolga(this List<Face> faces, double folga_Y)
        {
            var retorno = new Face();

            retorno.AddRange(faces.SelectMany(x => x.Furacoes));

            if (faces.Count > 0)
            {
                var bordas = faces.GetBordas();
                var bordas_corte = bordas.Cortar(folga_Y, folga_Y).MoverY(-folga_Y);

                var p_contorno = faces.GetPathsD();
                p_contorno.Add(bordas.GetPath());

                var p_contorno_interno = faces.GetPathsD();
                p_contorno_interno.Add(bordas_corte.GetPath());

                var p_contornos_pecas = faces.GetPathsD();


                var p_final = p_contorno.Union();
                var f_final_simplificado = p_final.SimplifyPaths(0).GetFaces();
                var f_final = p_final.GetFaces();

                if (f_final_simplificado.Count == 1)
                {
                    retorno.AddRange(f_final_simplificado[0].Liv);
                }
                else if (f_final.Count == 1)
                {
                    retorno.AddRange(f_final[0].Liv);
                }
                else
                {
                    //não deveria chegar aqui
                }

                var p_recortes_internos = p_contorno_interno.Intersect(p_contorno);
                var p_emendas = p_recortes_internos.Xor(p_contornos_pecas);
                var p_recortes_internos_corte = p_contorno_interno.Intersect(p_emendas);

                foreach (var emenda_interna in p_recortes_internos_corte)
                {
                    retorno.Add(new Recorte(emenda_interna.Select(x => new Liv(x.x, x.y)).ToList()));
                }
            }

            return retorno;
        }
        public static List<Face> Rotacionar(this List<Face> faces, double angulo)
        {
            return faces.Select(x => x.Rotacionar(angulo)).ToList();
        }

        public static Face GetBordas(this List<Face> faces, double offset_Y = 0)
        {
            if (!faces.First().IsMesa())
            {
                var ptss = faces.SelectMany(x => x.Liv.Select(y => y.Origem)).ToList();

                var x0 = ptss.OrderBy(x => x.Y).ThenBy(x => x.X).First().MoverY(-offset_Y);
                var x1 = ptss.OrderBy(x => x.Y).ThenByDescending(x => x.X).First().MoverY(-offset_Y);

                var x2 = ptss.OrderByDescending(x => x.Y).ThenByDescending(x => x.X).First().MoverY(offset_Y);
                var x3 = ptss.OrderByDescending(x => x.Y).ThenBy(x => x.X).First().MoverY(offset_Y);

                var pontos = new List<P3d> { x0, x1, x2, x3 };


                return pontos.GetFace();
            }

            return faces.GetContornoExterno().GetFace();
        }
        public static List<Face> Quebrar(this Face Origem, double comp_max)
        {


            if (comp_max <= 0)
            {
                comp_max = Cfg.Init.CAM_Quebra_Compmax;
            }
            int Qtd = Math.Ceiling(Origem.Comprimento / comp_max).Int();
            var Pedacos = new List<List<Liv>>();
            var Resto_Direito = new List<Liv>();

            var Geometria = new List<Liv>();
            Geometria.AddRange(Origem.LivSegmentada);


            for (int i = 0; i < Qtd; i++)
            {

                var p1 = new System.Windows.Point(comp_max, -5000);
                var p2 = new System.Windows.Point(comp_max, 50000);
                PathGeometry Esq;
                PathGeometry Dir;
                if (Qtd > 2)
                {
                    if (i == 0)
                    {


                        Geometria.GetPathGeometry().Quebrar(p1, p2, out Esq, out Dir);
                        Pedacos.Add(Esq.ToLiv());
                        Resto_Direito = Dir.ToLiv();
                    }
                    else
                    {
                        if (Resto_Direito.Comprimento() > comp_max)
                        {
                            Resto_Direito.GetPathGeometry(false).Quebrar(p1, p2, out Esq, out Dir);
                            Pedacos.Add(Esq.ToLiv());
                            Resto_Direito = Dir.ToLiv();
                        }
                        else
                        {
                            Pedacos.Add(Resto_Direito);
                        }
                    }
                }
                else
                {
                    Geometria.GetPathGeometry().Quebrar(p1, p2, out Esq, out Dir);
                    Pedacos.Add(Esq.ToLiv());
                    Pedacos.Add(Dir.ToLiv());
                    break;
                }


            }

            double P0 = 0;
            double P1 = comp_max;

            var retorno = new List<Face>();
            var furos = new List<Furo>();
            furos.AddRange(Origem.Furacoes);
            foreach (var Pedaco in Pedacos)
            {
                var n_face = new Face(Pedaco);
                var Furacoes = furos.FindAll(x => x.Origem.X >= P0 && x.Origem.X <= P1);


                foreach (var furo in Furacoes)
                {
                    var nf = new Furo(furo);
                    nf.Origem.X = nf.Origem.X - P0;
                    n_face.Add(nf);
                    furos.Remove(furo);
                }

                retorno.Add(n_face);
                P0 += comp_max;
                P1 += comp_max;
            }

            return retorno;
        }
        public static Face MoverEUnir(this List<Face> Faces, Sentido sentido = Sentido.Horizontal)
        {
            var nfaces = new List<Face>();

            double Acum = 0;
            foreach (var Face in Faces)
            {
                var nFace = new Face();
                if (sentido == Sentido.Horizontal)
                {
                    nFace = Face.MoverX(Acum);
                    Acum += Face.Comprimento;
                }
                else
                {
                    Face.MoverY(-Acum);
                    Acum += Face.Largura;
                }
                nfaces.Add(nFace);
            }


            var contorno = nfaces.GetPathsD();
            var uniao = contorno.Union(Clipper2Lib.FillRule.EvenOdd).GetFaces();

            var faces = new Face();

            faces.AddRange(nfaces.SelectMany(x => x.Furacoes));
            faces.AddRange(nfaces.SelectMany(x => x.RecortesInternos));
            faces.AddRange(nfaces.SelectMany(x => x.Dobras));
            faces.AddRange(nfaces.SelectMany(x => x.Soldas));
            if (uniao.Count == 1)
            {
                faces.AddRange(uniao[0].Liv);
            }
            else
            {
                faces.AddRange(uniao.SelectMany(x => x.Liv));
            }



            return faces;
        }

        public static List<P3d> GetContornoConvexo(this List<Face> faces)
        {
            if (faces.Count == 0)
            {
                return new List<P3d>();
            }
            return faces.SelectMany(x => x.Liv).Select(x => x.Origem).ToList().GetContornoConvexoHull(faces.First().IsMesa() ? TipoLiv.Z : TipoLiv.Y);
        }
        public static List<P3d> GetContornoExterno(this List<Face> faces, double offset_X = 0, double offset_Y = 0, double offset_Z = 0)
        {
            var pts = new List<P3d>();
            if (faces.Count > 0)
            {
                pts.Add(new P3d(faces.Min(x => x.MinX - offset_X), faces.Min(x => x.MinY - offset_Y), faces.Min(x => x.MinZ - offset_Z)));
                pts.Add(new P3d(faces.Max(x => x.MaxX + offset_X), faces.Min(x => x.MinY - offset_Y), faces.Min(x => x.MinZ - offset_Z)));
                pts.Add(new P3d(faces.Max(x => x.MaxX + offset_X), faces.Max(x => x.MaxY + offset_Y), faces.Max(x => x.MaxZ + offset_Z)));
                pts.Add(new P3d(faces.Min(x => x.MinX - offset_X), faces.Max(x => x.MaxY + offset_Y), faces.Max(x => x.MaxZ + offset_Z)));
            }
            return pts;
        }
    }
}
