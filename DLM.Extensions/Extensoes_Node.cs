using DLM.cam;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conexoes
{
    public static class Extensoes_Node
    {
        public static Perfil GetPerfil(this CAM_Node ShapeHeader, string Descricao)
        {
            var _Perfil = new Perfil();
            var Valores = new List<List<double>>();
            for (int i = 1; i < ShapeHeader.Nodes.Count; i++)
            {
                Valores.Add(ShapeHeader.Nodes[i].Valor.Split(' ').ToList().FindAll(x => x != "").Select(x => x.Double()).ToList());
            }

            var tipo = FuncoesDLMCam.GetTipoPerfilPorShape(ShapeHeader.Nodes[0].Valor.TrimEnd());

            switch (tipo)
            {
                case CAM_PERFIL_TIPO.Caixao:
                    _Perfil = DLM.cam.Perfil.Caixao(
                 Valores[0][0],
                 DLM.cam.Perfil.Chapa(Valores[0][1], Valores[0][4]),
                 DLM.cam.Perfil.Chapa(Valores[1][1], Valores[1][4]),
                 Valores[0][3], Valores[0][2]);
                    break;

                case CAM_PERFIL_TIPO.I_Soldado:
                    _Perfil = DLM.cam.Perfil.I_Soldado(Descricao, Valores[0][0], Valores[0][2], Valores[0][1], Valores[0][4], Valores[1][4], Valores[0][3]);
                    break;
                case CAM_PERFIL_TIPO.WLam:
                    _Perfil = DLM.cam.Perfil.Wlam(Descricao, Valores[0][0], Valores[0][1], Valores[0][4], Valores[0][3], Valores[0][5]);
                    break;
                case CAM_PERFIL_TIPO.INP:
                    _Perfil = DLM.cam.Perfil.INP(Descricao, Valores[0][0], Valores[0][1], Valores[0][4], Valores[0][3], Valores[0][5]);
                    break;
                case CAM_PERFIL_TIPO.C_Enrigecido:
                    _Perfil = DLM.cam.Perfil.C_Enrigecido(Descricao, Valores[0][0], Valores[0][1], Valores[0][3], Valores[0][2]);
                    break;
                case CAM_PERFIL_TIPO.Cartola:
                    _Perfil = DLM.cam.Perfil.Cartola(Descricao, Valores[0][0], Valores[0][1], Valores[0][3], Valores[0][2]);
                    break;
                case CAM_PERFIL_TIPO.Z_Purlin:
                    _Perfil = DLM.cam.Perfil.Z_Purlin(Descricao, Valores[0][0], Valores[1][1], Valores[0][1], Valores[0][3], Valores[1][2], Valores[0][2]);
                    _Perfil.Angulo = Valores[0][4];
                    break;
                case CAM_PERFIL_TIPO.UNP:
                    _Perfil = DLM.cam.Perfil.UNP(Descricao, Valores[0][0], Valores[0][1], Valores[0][3], Valores[0][4]);
                    break;
                case CAM_PERFIL_TIPO.U_Dobrado:
                    _Perfil = DLM.cam.Perfil.U_Dobrado(Descricao, Valores[0][0], Valores[0][1], Valores[0][3]);
                    break;
                case CAM_PERFIL_TIPO.UAP:
                    _Perfil = DLM.cam.Perfil.UAP(Descricao, Valores[0][0], Valores[0][1], Valores[0][3], Valores[0][4]);
                    break;
                case CAM_PERFIL_TIPO.Z_Dobrado:
                    _Perfil = DLM.cam.Perfil.Novo(tipo, Descricao, Valores[0][0], Valores[0][2], Valores[0][3], Valores[0][5], 0, 0, Valores[0][1]);
                    break;
                case CAM_PERFIL_TIPO.L_Laminado:
                    _Perfil = DLM.cam.Perfil.L_Laminado(Descricao, Valores[0][0], Valores[0][3], Valores[0][5]);
                    break;
                case CAM_PERFIL_TIPO.L_Dobrado:
                    _Perfil = DLM.cam.Perfil.L_Dobrado(Descricao, Valores[0][0], Valores[0][1], Valores[0][3]);
                    break;
                case CAM_PERFIL_TIPO.Tubo_Quadrado:
                    _Perfil = DLM.cam.Perfil.Tubo_Quadrado(Descricao, Valores[0][0], Valores[0][1], Valores[0][3]);
                    break;
                case CAM_PERFIL_TIPO.Tubo_Redondo:
                    _Perfil = DLM.cam.Perfil.Tubo_Redondo(Descricao, Valores[0][0], Valores[0][3]);
                    break;
                case CAM_PERFIL_TIPO.Barra_Redonda:
                    _Perfil = DLM.cam.Perfil.Barra_Redonda(Descricao, Valores[0][0]);
                    break;
                case CAM_PERFIL_TIPO.Barra_Chata:
                    _Perfil = DLM.cam.Perfil.Barra_Chata(Descricao, Valores[0][0], Valores[0][1]);
                    break;

                case CAM_PERFIL_TIPO.T_Soldado:
                    _Perfil = DLM.cam.Perfil.T_Soldado(Descricao, Valores[0][1], Valores[0][0], Valores[0][4], Valores[0][3], Valores[0][5]);
                    break;
                case CAM_PERFIL_TIPO.T_Aresta_Redondo:
                case CAM_PERFIL_TIPO.Z_Laminado:
                    _Perfil = DLM.cam.Perfil.Novo(tipo, Descricao, Valores[0][1], Valores[0][0], Valores[0][3], Valores[0][4], Valores[0][5]);
                    break;
                case CAM_PERFIL_TIPO.Especial_1:
                case CAM_PERFIL_TIPO.Especial_2:
                    _Perfil = DLM.cam.Perfil.Novo(tipo, Descricao, Valores[0][0], Valores[0][1], Valores[0][3], Valores[0][4], Valores[0][5]);
                    break;
                case CAM_PERFIL_TIPO.Especial_3:
                    _Perfil = DLM.cam.Perfil.Novo(tipo, Descricao, Valores[0][0], Valores[0][1], Valores[0][3], Valores[0][4], Valores[0][5], Valores[0][2]);
                    break;
                case CAM_PERFIL_TIPO.Generico:
                case CAM_PERFIL_TIPO.Elemento_Unitario:
                case CAM_PERFIL_TIPO._Desconhecido:
                case CAM_PERFIL_TIPO.Chapa_Xadrez:
                    _Perfil = DLM.cam.Perfil.Chapa(Valores[2][1], Valores[2][2], Descricao, tipo);
                    break;
                case CAM_PERFIL_TIPO.Chapa:
                    _Perfil = DLM.cam.Perfil.Chapa(Valores[2][1], Valores[2][2],"", tipo);
                    break;
            }

            return _Perfil;
        }

        public static List<Recorte> GetRecortes(this List<CAM_Node> blks, bool inverterY = false)
        {
            var Retorno = new List<Recorte>();
            foreach (var no in blks)
            {

                var pontos = new List<Liv>();
                foreach (var Linha in no.Nodes)
                {
                    List<string> Chaves = Linha.Valor.Split(' ').ToList();
                    Chaves = Chaves.Select(x => x.Replace(" ", "")).ToList().FindAll(y => y != "").ToList();

                    if (inverterY)
                    {
                        pontos.Add(new Liv(
                             Chaves[0].Double(),
                             Chaves[2].Double(),
                             Chaves[1].Double(),
                             Chaves[3].Double(),
                             Chaves[4].Double(),
                             Chaves[5].Double()
                             )
                             );
                    }
                    else
                    {
                        pontos.Add(new Liv(
                             Chaves[0].Double(),
                             Chaves[1].Double(),
                             Chaves[2].Double(),
                             Chaves[3].Double(),
                             Chaves[4].Double(),
                             Chaves[5].Double()
                             )
                             );
                    }

                }
                if (pontos.Count > 0)
                {
                    //double x1 = 0, y1 = 0;
                    //var valor = pontos.Zerar(out x1, out y1);
                    //var y0 = valor.FindAll(x => x.GetContraFlecha() == 0).Max(x => x.Origem.Y);
                    Retorno.Add(new Recorte(pontos));
                }

            }
            return Retorno;
        }
        public static List<Furo> GetFuros(this CAM_Node furos)
        {
            var Retorno = new List<Furo>();
            foreach (var f in furos.Nodes)
            {
                var Chaves = f.Valor.Split(' ').ToList();
                Chaves = Chaves.Select(x => x.Replace(" ", "")).ToList().FindAll(y => y != "").ToList();
                int y0 = 3;
                if (furos.Valor == Cfg.Init.CAM_H_LIV2 | furos.Valor == Cfg.Init.CAM_H_LIV3)
                {
                    y0 = 4;
                }
                /*arredondamento*/
                Retorno.Add(new Furo(Chaves[2].Double(0), Chaves[y0].Double(0), Chaves[1].Double(), Chaves[5].Double(), Chaves[6].Double()));
            }
            return Retorno;
        }
        public static List<Dobra> GetDobras(this CAM_Node dobras)
        {
            var Retorno = new List<Dobra>();
            foreach (var Linha in dobras.Nodes)
            {
                var Chaves = Linha.Valor.Split(' ').ToList();
                Chaves = Chaves.Select(x => x.Replace(" ", "")).ToList().FindAll(y => y != "").ToList();
                if (Chaves.Count > 6)
                {
                    Retorno.Add(new Dobra(
                                Chaves[0].Double(),
                                Chaves[1].Double(),
                                Chaves[2].Double(),
                                Chaves[3].Double(),
                                Chaves[4].Double(),
                                Chaves[5].Double(),
                                Chaves[6].Double()
                   )
                   );
                }
                else if (Chaves.Count > 5)
                {
                    Retorno.Add(new Dobra(
                                Chaves[0].Double(),
                                Chaves[1].Double(),
                                Chaves[2].Double(),
                                Chaves[3].Double(),
                                Chaves[4].Double(),
                                Chaves[5].Double(),
                                0
                   )
                   );
                }
                else if (Chaves.Count > 4)
                {
                    Retorno.Add(new Dobra(
                                Chaves[0].Double(),
                                Chaves[1].Double(),
                                Chaves[2].Double(),
                                Chaves[3].Double(),
                                Chaves[4].Double(),
                                0,
                                0
                   )
                   );
                }
            }
            return Retorno;
        }
        public static List<Solda> GetSoldas(this CAM_Node soldas)
        {
            var Retorno = new List<Solda>();
            foreach (var Linha in soldas.Nodes)
            {
                Retorno.Add(new Solda(Linha.Valor));
            }
            return Retorno;
        }
    }

}
