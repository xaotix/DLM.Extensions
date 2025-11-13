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

namespace Conexoes
{

    public static class ExtensoesCAM
    {
        public static bool IsAlma(this ReadCAM cam)
        {
            return cam.Perfil.Tipo == CAM_PERFIL_TIPO.Chapa && cam.NORMT() == TAB_NORMT.VIGA_ALMA;
        }
        public static bool IsMesa(this ReadCAM cam)
        {
            return cam.Perfil.Tipo == CAM_PERFIL_TIPO.Chapa && cam.NORMT() == TAB_NORMT.VIGA_MESA;
        }
        public static List<CAM_Node> GetAll(this List<CAM_Node> nodes, string chave)
        {
            return nodes.SelectMany(x => x.GetAll(chave)).ToList();
        }
        public static TAB_MAKTX GetMAKTX(this TAB_NORMT normt)
        {
            var maktx = TAB_MAKTX._INVALIDO;
            switch (normt)
            {
                case TAB_NORMT._INVALIDO:
                    break;
                case TAB_NORMT._VAZIO:
                    break;
                case TAB_NORMT.CHAPA:
                    maktx = TAB_MAKTX.CHAPA;
                    break;
                case TAB_NORMT.PERFIL_DOBRADO:
                    maktx = TAB_MAKTX.PERFIL_DOBRADO;
                    break;


                case TAB_NORMT.CHAPA_XADREZ:
                    maktx = TAB_MAKTX.CHAPA_DE_PISO;
                    break;
                case TAB_NORMT.FERRO_REDONDO:
                    maktx = TAB_MAKTX.TIRANTE;
                    break;
                case TAB_NORMT.BARRA_CHATA:
                    maktx = TAB_MAKTX.CHAPA;
                    break;
                case TAB_NORMT.PERFIL_I_LAMINADO:
                    maktx = TAB_MAKTX.VIGA_LAM_W;
                    break;
                case TAB_NORMT.VIGA_ALMA:
                case TAB_NORMT.VIGA_MESA:
                case TAB_NORMT.PERFIL_I_SOLDADO:
                    maktx = TAB_MAKTX.VIGA_SOLDADA;
                    break;
                case TAB_NORMT.PERFIL_C_PADRAO_165:
                case TAB_NORMT.PERFIL_C_PADRAO_216:
                case TAB_NORMT.PERFIL_C_PADRAO_292:
                case TAB_NORMT.PERFIL_C_PADRAO_125:
                case TAB_NORMT.PERFIL_C_PADRAO_185:
                    maktx = TAB_MAKTX.TERCA_PURLIN_C;
                    break;
                case TAB_NORMT.PERFIL_Z_PADRAO_165:
                case TAB_NORMT.PERFIL_Z_PADRAO_216:
                case TAB_NORMT.PERFIL_Z_PADRAO_292:
                case TAB_NORMT.PERFIL_Z_ENRIJECIDO_185:
                case TAB_NORMT.PERFIL_Z_ENRIJECIDO_360:
                case TAB_NORMT.PERFIL_Z_ENRIJECIDO_245:
                case TAB_NORMT.PERFIL_Z_ENRIJECIDO_300:
                    maktx = TAB_MAKTX.TERCA_PURLIN_Z;
                    break;

                case TAB_NORMT.TUBO_RETANGULAR:
                case TAB_NORMT.TUBO_REDONDO:
                case TAB_NORMT.PERFIL_C_LAMINADO:
                case TAB_NORMT.PERFIL_T_LAMINADO:
                    maktx = TAB_MAKTX.VIGA_LAM_GEN;
                    break;


                case TAB_NORMT.DIAGONAL_MEDAJOIST:
                    maktx = TAB_MAKTX.DIAGONAL_MEDAJOIST;
                    break;
                case TAB_NORMT.BANZO_INFERIOR_TIPO_D:
                    maktx = TAB_MAKTX.BANZO_SUPERIOR_MEDAJOIST;
                    break;
                case TAB_NORMT.BANZO_SUPERIOR_TIPO_D:
                    maktx = TAB_MAKTX.BANZO_INFERIOR_MEDAJOIST;
                    break;

                case TAB_NORMT.BANZO_INFERIOR_MEDABAR:
                    maktx = TAB_MAKTX.BANZO_INFERIOR_MEDABAR;
                    break;
                case TAB_NORMT.BANZO_SUPERIOR_MEDABAR:
                    maktx = TAB_MAKTX.BANZO_SUPERIOR_MEDABAR;
                    break;
                case TAB_NORMT.DIAGONAL_MEDABAR:
                    maktx = TAB_MAKTX.DIAGONAL_MEDABAR;
                    break;





                case TAB_NORMT.CANTONEIRA_LAMINADA:
                case TAB_NORMT.TUBO_QUADRADO:
                    maktx = TAB_MAKTX.VIGA_LAM_GEN;
                    break;


                case TAB_NORMT.CHAPA_EXPANDIDA:
                    maktx = TAB_MAKTX.CHAPA_DE_PISO;
                    break;
                case TAB_NORMT.SSR1:
                    maktx = TAB_MAKTX.SSR1;
                    break;
                case TAB_NORMT.SSR2:
                    maktx = TAB_MAKTX.SSR2;
                    break;
                case TAB_NORMT.SSR1M:
                    maktx = TAB_MAKTX.SSR1M;
                    break;
                case TAB_NORMT.SSR1BM:
                    maktx = TAB_MAKTX.SSR1BM;
                    break;
                case TAB_NORMT.SSR1F:
                    maktx = TAB_MAKTX.SSR1F;
                    break;
                case TAB_NORMT.SSR1BF:
                    maktx = TAB_MAKTX.SSR1BF;
                    break;
                case TAB_NORMT.STEEL_DECK:
                    maktx = TAB_MAKTX.STEEL_DECK;
                    break;
                case TAB_NORMT.PANEL_RIB_II:
                    maktx = TAB_MAKTX.PANEL_RIB_II;
                    break;
                case TAB_NORMT.PANEL_RIB_III:
                    maktx = TAB_MAKTX.PANEL_RIB_III;
                    break;
                case TAB_NORMT.TELHA_ONDULADA:
                    maktx = TAB_MAKTX.TELHA_ONDULADA;
                    break;
                case TAB_NORMT.GRADE:
                    maktx = TAB_MAKTX.GRADE_DE_PISO;
                    break;
                case TAB_NORMT.TELA:
                case TAB_NORMT.TELHA_FORRO:
                    maktx = TAB_MAKTX.QUADRO_DE_TELA;
                    break;
                    break;
                case TAB_NORMT.TRINS:
                    maktx = TAB_MAKTX.ARREMATE;
                    break;
                case TAB_NORMT.MARCA_KIT_PIE:
                case TAB_NORMT.ALMOX_FABRICACAO:
                case TAB_NORMT.KIT_PIE:
                case TAB_NORMT._DESCONTINUADO_1:
                case TAB_NORMT._DESCONTINUADO_2:
                    maktx = TAB_MAKTX._INVALIDO;
                    break;
                default:
                    break;
            }

            return maktx;
        }
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
                            var min_borda = f1.GetMinBorda(f2);
                            var dist = f2.DistanciaBorda(f1).Abs();
                            var dist_fr = f1.Origem.Distancia(f2.Origem);

                            if (dist <= min_borda | dist <= 0 | dist_fr == 0)
                            {
                                f2.Validado = true;
                                txt2.Add(
                                    $"\n[{f2}] -> [Dist.:{dist_fr}, Borda:{dist}, Mín.: {min_borda}]"
                                    );
                            }
                        }
                    }

                    if (txt2.Count > 0)
                    {
                        txt2.Insert(0, txt1);
                        reports.Add(new Report("Inferferências furações", $"{string.Join("", txt2)}\n\n", TipoReport.Critico));
                    }
                }
            }
            for (int i = 0; i < furos.Count; i++)
            {
                furos[i].Validado = false;
            }
            return reports;
        }
        public static double GetMinBorda(this Furo f1, Furo f2)
        {
            return ((f2.Diametro / 2 + f1.Diametro / 2) * Cfg.Init.TestList_V_CAMS_Furacoes_Dist_Entre_furos_Borda).Round(1).Abs();
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
        public static List<Liv> RemoveSobrePostos(this List<Liv> lista)
        {
            var retorno = new List<Liv>();

            if (lista.Count == 0)
            {
                return lista;
            }

            for (int i = 0; i < lista.Count; i++)
            {
                if (i == 0)
                {
                    retorno.Add(lista[i]);
                }
                else
                {
                    if (retorno.Last().GetCid() != lista[i].GetCid())
                    {
                        if (i == lista.Count - 1)
                        {
                            if (retorno.First().GetCid() != lista[i].GetCid())
                            {
                                retorno.Add(lista[i]);
                            }
                        }
                        else
                        {
                            retorno.Add(lista[i]);
                        }
                    }
                }
            }
            return retorno;
        }

        public static List<ReadCAM> GetCAMs(this List<string> programas)
        {
            var items = new List<ReadCAM>();
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

                items.AddRange(cams_map);
                items.AddRange(cams_map.SelectMany(x => x.GetDesmembrados()));
                //_CAMs = _CAMs.OrderBy(x => x.Nome).ToList();
            }
            return items;
        }
        public static List<NC1> GetNCs(this List<string> programas)
        {
            var items = new List<NC1>();
            programas = programas.Select(x => x.ToUpper()).Distinct().ToList();
            var lista = programas.Quebrar(500);
            foreach (var pack in lista)
            {
                var Tarefas = new List<Task>();
                var cams_map = new ConcurrentBag<DLM.cam.NC1>();
                foreach (var programa in pack)
                {
                    Tarefas.Add(Task.Factory.StartNew(() =>
                    {
                        if (programa.Exists())
                        {
                            var ncam = new NC1(programa);
                            cams_map.Add(ncam);
                        }
                    }));
                }

                Task.WaitAll(Tarefas.ToArray());

                items.AddRange(cams_map);
            }
            return items;
        }



        public static Face GetFace(this List<P3d> pts)
        {
            return new Face(pts);
        }
        public static Face GetFace(this PathD points)
        {
            return points.Select(x => new P3d(x.x, x.y)).ToList().GetFace();
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
                foreach (var linha in cam.Formato.LIV1.Linhas)
                {
                    var ang = linha.Anterior.Angulo(linha).Abs();
                    var angulo = ang;
                    while (angulo > 90)
                    {
                        angulo = angulo - 180;
                    }
                    angulo = angulo.Abs();
                    if (angulo >= ang_min)
                    {
                        var ponto = linha.OffSetInterno(dist, cam.Formato.LIV1.Rotacao);
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

                    var n_cam_1 = new Cam($"{destino_cam}I.{s.String(2)}.{c.String(2)}_1.CAM", c0.Alma.Perfil, 1000);
                    var n_cam_2 = new Cam($"{destino_cam}I.{s.String(2)}.{c.String(2)}_2.CAM", c0.Mesa_S.Perfil, 1000);
                    var n_cam_3 = new Cam($"{destino_cam}I.{s.String(2)}.{c.String(2)}_3.CAM", c0.Mesa_I.Perfil, 1000);

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

                        n_cam_1.Cabecalho.Nota_Principal = nota;
                        n_cam_2.Cabecalho.Nota_Principal = nota;
                        n_cam_3.Cabecalho.Nota_Principal = nota;

                        retorno.Add(n_cam_1);
                        retorno.Add(n_cam_2);
                        retorno.Add(n_cam_3);

                    }
                    catch (Exception ex)
                    {
                        ex.Alerta();
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
                var n_cam = new Cam($"{destino_cam}.{c.String(2)}.CAM", c0.Perfil, 1000);
                double acum = 0;
                var faces1 = new List<Face>();
                var faces2 = new List<Face>();
                var faces3 = new List<Face>();
                var faces4 = new List<Face>();

                n_cam.Cabecalho.Nota_Principal = $"{string.Join("|", lista_cam.GroupBy(x => x.Nome).Select(x => $"{x.Key}({x.Count()})"))}";
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
                    ex.Alerta();
                }


                n_cam.Comprimento = n_cam.Formato.Comprimento;
                c++;
            }




            return retorno;

        }

        public static List<Report> Comparar(this DLM.cam.ReadCAM p1, DLM.cam.ReadCAM p2)
        {
            var reports = new List<Report>();

            if (p1.Largura != p2.Largura)
            {
                reports.Add(new Report("Largura Divergente", p1.Nome + " - " + p1.Largura + " / " + p2.Nome + " - " + p2.Largura));
            }

            if (p1.Comprimento != p2.Comprimento)
            {
                reports.Add(new Report("Comprimento Divergente", p1.Nome + " - " + p1.Comprimento + " / " + p2.Nome + " - " + p2.Comprimento));
            }
            if (p1.Descricao != p2.Descricao)
            {
                reports.Add(new Report("Descricao Divergente", p1.Nome + " - " + p1.Comprimento + " / " + p2.Nome + " - " + p2.Descricao));
            }

            foreach (var s in p1.Formato.LIV1.Furacoes.FindAll(x => p2.Formato.LIV1.Furacoes.Find(y =>
              y.GetLinha() == x.GetLinha()
          ) == null))
            {
                reports.Add(new Report("Furo Divergente", p1.Nome + " - " + s.ToString()));
            }

            foreach (var s in p2.Formato.LIV1.Furacoes.FindAll(x => p1.Formato.LIV1.Furacoes.Find(y =>
             y.GetLinha() == x.GetLinha()
         ) == null))
            {
                reports.Add(new Report("Furo Divergente", p2.Nome + " - " + s.ToString()));
            }

            if (p1.Espessura != p2.Espessura)
            {
                reports.Add(new Report("Espessura Divergente", p1.Nome + " - " + p1.Espessura + " / " + p2.Nome + " - " + p2.Espessura));
            }

            foreach (var s in p1.Formato.LIV1.Dobras
                .FindAll(x => p2.Formato.LIV1.Dobras
                .Find(y => y.GetLinhaCAM() == x.GetLinhaCAM()
         ) == null))
            {
                reports.Add(new Report("Dobra Divergente", p2.Nome + " - " + s.ToString()));
            }
            foreach (var s in p2.Formato.LIV1.Dobras
                .FindAll(x => p1.Formato.LIV1.Dobras
                .Find(y =>
             y.GetLinhaCAM() == x.GetLinhaCAM()
         ) == null))
            {
                reports.Add(new Report("Dobra Divergente", p2.Nome + " - " + s.ToString()));
            }

            return reports;
        }


        public static List<Liv> MoverY(this List<Liv> lista, double valor)
        {
            return lista.Select(x => x.MoverY(valor)).ToList();
        }
        public static List<Liv> MoverX(this List<Liv> lista, double valor)
        {
            return lista.Select(x => x.MoverX(valor)).ToList();
        }
        public static List<Liv> MoverZ(this List<Liv> lista, double valor)
        {
            return lista.Select(x => x.MoverZ(valor)).ToList();
        }
    }
}
