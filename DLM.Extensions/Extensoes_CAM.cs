using Clipper2Lib;
using Conexoes;
using DLM.cam;
using DLM.desenho;
using DLM.encoder;
using DLM.vars;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Conexoes
{

    public static class ExtensoesCAM
    {
        public static List<Report> Verificar(this List<Furo> furos)
        {
            var reports = new List<Report>();
       
            for (int i = 0; i < furos.Count; i++)
            {
                var f1 = furos[i];
                for (int k = 0; k < furos.Count; k++)
                {
                    var f2 = furos[k];
                    var txt1 = $"[{f1}]:";
                    var txt2 = new List<string>();
                    if (k != i)
                    {
                        if (!f2.Validado)
                        {
                            var min = ((f2.Diametro / 2 + f1.Diametro / 2) * Cfg.Init.TestList_V_CAMS_Furacoes_Dist_Entre_furos_Borda).Round(1);
                            var dist = f2.DistanciaBorda(f1);
                            var dist_fr = f1.Origem.Distancia(f2.Origem);

                            if (dist <= min | dist <= 0 | dist_fr == 0)
                            {
                                f2.Validado = true;
                                txt2.Add(
                                    $"\n[{f2}] -> [Dist.:{dist_fr}, Borda:{dist}, Mín.: {min}]"
                                    );
                            }
                        }
                    }

                    if (txt2.Count > 0)
                    {
                        txt2.Insert(0, txt1);
                        reports.Add(new Report("Inferferências furações", $"{string.Join("", txt2)}\n\n", TipoReport.Crítico));
                    }
                }
            }
            for (int i = 0; i < furos.Count; i++)
            {
                furos[i].Validado = false;
            }
            return reports;
        }
        public static List<Cam> Quebrar(this ReadCAM Origem, double comp_max)
        {
            var Retorno = new List<Cam>();
            var Prefix = Origem.Nome + Cfg.Init.CAM_Quebra_Sufix;

            if (comp_max <= 0)
            {
                comp_max = Cfg.Init.CAM_Quebra_Compmax;
            }
            if ((Origem.Perfil.Tipo == DLM.vars.CAM_PERFIL_TIPO.Chapa | Origem.Perfil.Tipo == DLM.vars.CAM_PERFIL_TIPO.Chapa_Xadrez) && Origem.Comprimento > comp_max)
            {
                int cCam = 1;
                var Pedacos = Origem.Formato.LIV1.Quebrar(comp_max);
                foreach (var Pedaco in Pedacos)
                {
                    string Arquivo = $"{Origem.Pasta}{Prefix}{cCam}.{Cfg.Init.EXT_CAM}";
                    var n_CAM = new Cam(Arquivo, Pedaco, Origem.Espessura);
                    n_CAM.CopiarInfo(Origem);
                    cCam++;
                    Retorno.Add(n_CAM);
                   
                }
            }
            return Retorno;
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
        public static List<P3d> GetContornoConvexo(this List<Face> faces)
        {
            if (faces.Count == 0)
            {
                return new List<P3d>();
            }
            return faces.SelectMany(x => x.Liv).Select(x => x.Origem).ToList().GetContornoConvexoHull(faces.First().Mesa ? TipoLiv.Z : TipoLiv.Y);
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
            if(uniao.Count==1)
            {
                faces.AddRange(uniao[0].Liv);
            }
            else
            {
                faces.AddRange(uniao.SelectMany(x => x.Liv));
            }



            return faces;
        }

        public static List<Liv> RemoveSobrePostos(this List<Liv> Lista)
        {
            var Retorno = new List<Liv>();

            if (Lista.Count == 0)
            {
                return Lista;
            }

            for (int i = 0; i < Lista.Count; i++)
            {
                if (i == 0)
                {
                    Retorno.Add(Lista[i]);
                }
                else
                {
                    if (Retorno.Last().GetCid() != Lista[i].GetCid())
                    {
                        if (i == Lista.Count - 1)
                        {
                            if (Retorno.First().GetCid() != Lista[i].GetCid())
                            {
                                Retorno.Add(Lista[i]);
                            }
                        }
                        else
                        {
                            Retorno.Add(Lista[i]);
                        }
                    }
                }
            }
            return Retorno;
        }
        public static Face GetFace(this PathD points)
        {
            return points.Select(x => new P3d(x.x, x.y)).ToList().GetFace();
        }

        public static List<ReadCAM> GetCAMs(this List<string> programas)
        {
            var _CAMs = new List<ReadCAM>();
            programas = programas.Select(x => x.ToUpper()).Distinct().ToList();
            var lista = programas.Quebrar(500);
            foreach (var pack in lista)
            {
                var Tarefas = new List<Task>();
                var cams_map = new ConcurrentBag<DLM.cam.ReadCAM>();
                foreach (var programa in pack)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    {
                        if (programa.Exists())
                        {
                            var ncam = new ReadCAM(programa);
                            cams_map.Add(ncam);
                            ncam.GetDesmembrados();
                        }
                    }));
                }

                Task.WaitAll(Tarefas.ToArray());

                _CAMs.AddRange(cams_map);
                _CAMs.AddRange(cams_map.SelectMany(x=>x.GetDesmembrados()));
                //_CAMs = _CAMs.OrderBy(x => x.Nome).ToList();
            }
            return _CAMs;
        }



        public static Face GetBordas(this List<Face> faces, double offset_Y = 0)
        {
            if (!faces.First().Mesa)
            {
                var ptss = faces.SelectMany(x => x.Liv.Select(y => y.Origem)).ToList();

                var x0 = ptss.OrderBy(x => x.Y).ThenBy(x => x.X).First().MoverY(-offset_Y);
                var x1 = ptss.OrderBy(x => x.Y).ThenByDescending(x => x.X).First().MoverY(-offset_Y);

                var x2 = ptss.OrderByDescending(x => x.Y).ThenByDescending(x => x.X).First().MoverY(offset_Y);
                var x3 = ptss.OrderByDescending(x => x.Y).ThenBy(x => x.X).First().MoverY(offset_Y);

                var pontos = new List<P3d> { x0, x1, x2, x3 };


                return pontos.GetFace();
            }

            return GetContornoExterno(faces).GetFace();
        }

        public static Face GetFace(this List<P3d> pts)
        {
            return new Face(pts);
        }

        public static List<Face> GetFaces(this PathsD pointDs)
        {
            return pointDs.Select(x => x.GetFace()).ToList();
        }

        public static Cam RebaterFuros(this ReadCAM readcam, string destino, Sentido_Espelho sentido = Sentido_Espelho.X, bool apagar_antigos = false)
        {
            var cam = readcam.GetCam(destino);
            cam.Formato.LIV1.RebaterFuros(sentido, apagar_antigos);

            return cam;
        }
        public static Cam AddFuroBorda(this ReadCAM readcam, bool somente_canto, string destino = null)
        {

            if (destino == null)
            {
                destino = readcam.Pasta.GetSubPasta(Cfg.Init.PASTA_FURO_BORDA);
            }


            double diam = Cfg.Init.Furo_Gabarito_Diametro;
            double dist = Cfg.Init.Furo_Gabarito_Angulo_Offset_Borda;
            double ang_min = Cfg.Init.Furo_Gabarito_Angulo_Minimo;
            double dist_min = Math.Abs(Cfg.Init.TestList_V_CAMS_Furacoes_Dist_Furo_Borda * diam);
            var cam = readcam.GetCam(destino);
            if (somente_canto)
            {
                //adiciona 1 furo no canto superior esquerdo
                var pt0 = new P3d(readcam.Formato.LIV1.MinX, readcam.Formato.LIV1.MaxY);
                var min_liv = cam.Formato.LIV1.Linhas.GetLivMaisProximo(pt0);
                var ponto = min_liv.OffSetInterno(dist, cam.Formato.LIV1.Rotacao);

                var proximos = cam.Formato.LIV1.Furacoes.FindAll(x => x.Origem.Distancia(ponto) <= dist_min);
                if (proximos.Count == 0)
                {
                    cam.Formato.AddFuroLIV1(ponto, diam);
                }
            }
            else
            {
                foreach (var liv in cam.Formato.LIV1.Linhas)
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
                        var ponto = liv.OffSetInterno(dist, cam.Formato.LIV1.Rotacao);
                        var proximos = cam.Formato.LIV1.Furacoes.FindAll(x => x.Origem.Distancia(ponto) <= dist_min);
                        if (proximos.Count == 0)
                        {
                            cam.Formato.AddFuroLIV1(ponto, diam);
                        }
                    }
                }
            }
            return cam;
        }


        public static List<Cam> Unir(this List<PosicaoPerfilSoldado> lista, string destino_cam, double folga_Y = 20, double folga_X = 5, double comp_max = 12000, double comp_min = 2000)
        {
            var retorno = new List<Cam>();
            var grupo_soldados = lista.FindAll(x => x.Ok && x.Comprimento <= comp_min).GroupBy(x => x.Perfil).ToList();

            int s = 1;
            foreach (var soldados in grupo_soldados)
            {
                var lista_lado_a_lado = new List<PosicaoPerfilSoldado>();

                foreach (var cam in soldados)
                {
                    var qtd = cam.Quantidade;

                    while (qtd > 0)
                    {
                        qtd += -1;
                        lista_lado_a_lado.Add(cam);
                    }
                }

                var lista_cams = new List<List<PosicaoPerfilSoldado>>();
                for (int i = 0; i < lista_lado_a_lado.Count; i++)
                {
                    var comp = 0.0;
                    var cams_pedaco = new List<PosicaoPerfilSoldado>();
                    comp += lista_lado_a_lado[i].Comprimento + folga_X;
                    while (comp < comp_max && i < lista_lado_a_lado.Count)
                    {
                        cams_pedaco.Add(lista_lado_a_lado[i]);
                        i++;
                        if (i < lista_lado_a_lado.Count)
                        {
                            comp += lista_lado_a_lado[i].Comprimento + folga_X;
                        }
                    }
                    lista_cams.Add(cams_pedaco);
                    i--;
                }

                int c = 1;
                foreach (var lista_cam in lista_cams)
                {
                    double acum = 0;
                    var c0 = lista_cam.First();

                    var n_cam_1 = new Cam($"{destino_cam}I.{s.ToString().PadLeft(2, '0')}.{c.String(2)}_1.CAM", c0.Alma.Perfil, 1000);
                    var n_cam_2 = new Cam($"{destino_cam}I.{s.ToString().PadLeft(2, '0')}.{c.String(2)}_2.CAM", c0.Mesa_S.Perfil, 1000);
                    var n_cam_3 = new Cam($"{destino_cam}I.{s.ToString().PadLeft(2, '0')}.{c.String(2)}_3.CAM", c0.Mesa_I.Perfil, 1000);

                    var faces_1 = new List<Face>();
                    var faces_2 = new List<Face>();
                    var faces_3 = new List<Face>();


                    var nota = $"{string.Join("|", lista_cam.GroupBy(x => x.Nome).Select(x => $"{x.Key}({x.Count()})"))}";


                    try
                    {

                        for (int i = 0; i < lista_cam.Count; i++)
                        {
                            faces_1.Add(lista_cam[i].Alma.Formato.LIV1.MoverX(acum));
                            faces_2.Add(lista_cam[i].Mesa_S.Formato.LIV1.MoverX(acum));
                            faces_3.Add(lista_cam[i].Mesa_I.Formato.LIV1.MoverX(acum));

                            acum += lista_cam[i].Comprimento + folga_X;
                        }

                        var f1 = faces_1.UnirComFolga(folga_Y);
                        var f2 = faces_2.UnirComFolga(folga_Y);
                        var f3 = faces_3.UnirComFolga(folga_Y);

                        n_cam_1.Comprimento = f1.Comprimento;
                        n_cam_2.Comprimento = f2.Comprimento;
                        n_cam_3.Comprimento = f3.Comprimento;

                        n_cam_1.Formato.LIV1.Furacoes.AddRange(f1.Furacoes);
                        n_cam_2.Formato.LIV1.Furacoes.AddRange(f2.Furacoes);
                        n_cam_3.Formato.LIV1.Furacoes.AddRange(f3.Furacoes);

                        n_cam_1.Formato.LIV1.Liv.AddRange(f1.Liv);
                        n_cam_2.Formato.LIV1.Liv.AddRange(f2.Liv);
                        n_cam_3.Formato.LIV1.Liv.AddRange(f3.Liv);

                        n_cam_1.Formato.LIV1.RecortesInternos.AddRange(f1.RecortesInternos);
                        n_cam_2.Formato.LIV1.RecortesInternos.AddRange(f2.RecortesInternos);
                        n_cam_3.Formato.LIV1.RecortesInternos.AddRange(f3.RecortesInternos);

                        n_cam_1.Cabecalho.Notas = nota;
                        n_cam_2.Cabecalho.Notas = nota;
                        n_cam_3.Cabecalho.Notas = nota;

                        retorno.Add(n_cam_1);
                        retorno.Add(n_cam_2);
                        retorno.Add(n_cam_3);

                    }
                    catch (Exception ex)
                    {
                        Conexoes.Utilz.Alerta(ex);
                    }
                    c++;
                }
            }
            s++;


            return retorno;
        }



        /// <summary>
        /// Método juntando Perfis compostos
        /// </summary>
        /// <param name="cams"></param>
        /// <param name="destino_cam"></param>
        /// <param name="folga_Y"></param>
        /// <param name="folga_X"></param>
        /// <param name="comp_max"></param>
        /// <returns></returns>
        public static List<Cam> UnirPFS(this List<ReadCAM> cams, string destino_cam, double folga_Y = 20, double folga_X = 5, double comp_max = 12000)
        {
            var retorno = new List<Cam>();
            var c0 = cams.First();

            var comp_total = cams.Sum(x => x.Comprimento * x.Quantidade);
            var folgas = folga_X * cams.Sum(x => x.Quantidade);

            var lista_lado_a_lado = new List<ReadCAM>();

            foreach (var cam in cams)
            {
                var qtd = cam.Quantidade;

                while (qtd > 0)
                {
                    qtd += -1;
                    lista_lado_a_lado.Add(cam);
                }
            }
            var lista_cams = new List<List<ReadCAM>>();
            for (int i = 0; i < lista_lado_a_lado.Count; i++)
            {
                var comp = 0.0;
                var cams_pedaco = new List<ReadCAM>();
                comp += lista_lado_a_lado[i].Comprimento + folga_X;
                while (comp < comp_max && i < lista_lado_a_lado.Count)
                {
                    cams_pedaco.Add(lista_lado_a_lado[i]);
                    i++;
                    if (i < lista_lado_a_lado.Count)
                    {
                        comp += lista_lado_a_lado[i].Comprimento + folga_X;
                    }
                }
                lista_cams.Add(cams_pedaco);
                i--;
            }

            int c = 1;
            foreach (var lista_cam in lista_cams)
            {
                var n_cam = new Cam($"{destino_cam}.{c.ToString().PadLeft(2, '0')}.CAM", c0.Perfil, 1000);
                double acum = 0;
                var faces1 = new List<Face>();
                var faces2 = new List<Face>();
                var faces3 = new List<Face>();
                var faces4 = new List<Face>();

                n_cam.Cabecalho.Notas = $"{string.Join("|", lista_cam.GroupBy(x => x.Nome).Select(x => $"{x.Key}({x.Count()})"))}";
                try
                {

                    for (int i = 0; i < lista_cam.Count; i++)
                    {
                        var cam = lista_cam[i];

                        var f1 = cam.Formato.LIV1.MoverX(acum);
                        var f2 = cam.Formato.LIV2.MoverX(acum);
                        var f3 = cam.Formato.LIV3.MoverX(acum);
                        var f4 = cam.Formato.LIV4.MoverX(acum);

                        faces1.Add(f1);
                        faces2.Add(f2);
                        faces3.Add(f3);
                        faces4.Add(f4);

                        n_cam.Formato.LIV1.AddRange(f1.Furacoes);
                        n_cam.Formato.LIV2.AddRange(f2.Furacoes);
                        n_cam.Formato.LIV3.AddRange(f3.Furacoes);
                        n_cam.Formato.LIV4.AddRange(f4.Furacoes);

                        acum += cam.Comprimento + folga_X;
                    }

                    if (faces1.Count > 0)
                    {
                        var bordas = faces1.GetBordas();
                        var bordas_corte = (c0.Perfil.Esp_M + folga_Y);

                        var contorno_iterno = bordas.Cortar(bordas_corte, bordas_corte).MoverY(-folga_Y);
                        var p_contorno_interno = contorno_iterno.GetPath().GetPathsD();

                        var p_contorno = bordas.GetPath().GetPathsD();
                        p_contorno.AddRange(faces1);
                        
                        var p_contornos_pecas = new PathsD();
                        p_contornos_pecas.AddRange(faces1);

                        var p_contorno_final = p_contorno.Union();
                        var f_final_simplificado = p_contorno_final.SimplifyPaths(0).GetFaces();
                        var f_final = p_contorno_final.GetFaces();

                        if (f_final_simplificado.Count == 1)
                        {
                            n_cam.Formato.LIV1.AddRange(f_final_simplificado[0].Liv);
                        }
                        else if (f_final.Count == 1)
                        {
                            n_cam.Formato.LIV1.AddRange(f_final[0].Liv);
                        }
                        else
                        {

                        }

                        var recortes_internos = p_contorno_interno.Intersect(p_contorno);
                        var emendas = recortes_internos.Xor(p_contornos_pecas);
                        var emendas_internas = p_contorno_interno.Intersect(emendas);

                        foreach (var p in emendas_internas)
                        {
                            n_cam.Formato.LIV1.RecortesInternos.Add(new Recorte(p.Select(x => new Liv(x.x, x.y)).ToList()));
                        }
                    }

                    if (faces2.FindAll(x => x.Liv.Count > 0).Count > 0)
                    {
                        n_cam.Formato.LIV2.AddRange(faces2.GetBordas().Liv);
                    }

                    if (faces3.FindAll(x => x.Liv.Count > 0).Count > 0)
                    {
                        n_cam.Formato.LIV3.AddRange(faces3.GetBordas().Liv);
                    }

                    if (faces4.FindAll(x => x.Liv.Count > 0).Count > 0)
                    {
                        n_cam.Formato.LIV4.AddRange(faces4.GetBordas().Liv);
                    }

                    retorno.Add(n_cam);

                }
                catch (Exception ex)
                {
                    Conexoes.Utilz.Alerta(ex);
                }


                n_cam.Comprimento = n_cam.Formato.Comprimento;
                c++;
            }




            return retorno;

        }

        public static List<Report> Comparar(this DLM.cam.ReadCAM p1, DLM.cam.ReadCAM p2)
        {
            List<Report> Reports = new List<Report>();

            if (p1.Largura != p2.Largura)
            {
                Reports.Add(new Report("Largura Divergente", p1.Nome + " - " + p1.Largura + " / " + p2.Nome + " - " + p2.Largura));
            }

            if (p1.Comprimento != p2.Comprimento)
            {
                Reports.Add(new Report("Comprimento Divergente", p1.Nome + " - " + p1.Comprimento + " / " + p2.Nome + " - " + p2.Comprimento));
            }
            if (p1.Descricao != p2.Descricao)
            {
                Reports.Add(new Report("Descricao Divergente", p1.Nome + " - " + p1.Comprimento + " / " + p2.Nome + " - " + p2.Descricao));
            }

            foreach (var s in p1.Formato.LIV1.Furacoes.FindAll(x => p2.Formato.LIV1.Furacoes.Find(y =>
              y.GetLinha() == x.GetLinha()
          ) == null))
            {
                Reports.Add(new Report("Furo Divergente", p1.Nome + " - " + s.ToString()));
            }

            foreach (var s in p2.Formato.LIV1.Furacoes.FindAll(x => p1.Formato.LIV1.Furacoes.Find(y =>
             y.GetLinha() == x.GetLinha()
         ) == null))
            {
                Reports.Add(new Report("Furo Divergente", p2.Nome + " - " + s.ToString()));
            }

            if (p1.Espessura != p2.Espessura)
            {
                Reports.Add(new Report("Espessura Divergente", p1.Nome + " - " + p1.Espessura + " / " + p2.Nome + " - " + p2.Espessura));
            }

            foreach (var s in p1.Formato.LIV1.Dobras
                .FindAll(x => p2.Formato.LIV1.Dobras
                .Find(y => y.GetLinhaCAM() == x.GetLinhaCAM()
         ) == null))
            {
                Reports.Add(new Report("Dobra Divergente", p2.Nome + " - " + s.ToString()));
            }
            foreach (var s in p2.Formato.LIV1.Dobras
                .FindAll(x => p1.Formato.LIV1.Dobras
                .Find(y =>
             y.GetLinhaCAM() == x.GetLinhaCAM()
         ) == null))
            {
                Reports.Add(new Report("Dobra Divergente", p2.Nome + " - " + s.ToString()));
            }

            return Reports;
        }
    }
}
