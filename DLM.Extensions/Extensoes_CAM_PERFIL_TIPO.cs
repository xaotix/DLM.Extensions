using DLM;
using DLM.vars;
using System.Collections.Generic;
using System.Windows.Media;

namespace Conexoes
{
    public static class Extensoes_CAM_PERFIL_TIPO
    {
        public static ImageSource GetImagem(this CAM_PERFIL_TIPO tipo)
        {
            switch (tipo)
            {
                case CAM_PERFIL_TIPO.Z_Purlin:
                    return BufferImagem.dbase_ico_Z_Purlin;
                case CAM_PERFIL_TIPO.Z_Dobrado:
                    return BufferImagem.dbase_ico_Z_Dobrado;
                case CAM_PERFIL_TIPO.C_Enrigecido:
                    return BufferImagem.dbase_ico_C;
                case CAM_PERFIL_TIPO.L_Dobrado:
                    return BufferImagem.dbase_ico_L_Dobrado;
                case CAM_PERFIL_TIPO.Cartola:
                    return BufferImagem.dbase_ico_OMEGA;
                case CAM_PERFIL_TIPO.Tubo_Redondo:
                    return BufferImagem.dbase_ico_TUBO_REDONDO;
                case CAM_PERFIL_TIPO.Tubo_Quadrado:
                    return BufferImagem.dbase_ico_TUBO_RETANGULAR;
                case CAM_PERFIL_TIPO.Caixao:
                    return BufferImagem.dbase_ico_CAIXAO;
                case CAM_PERFIL_TIPO.I_Soldado:
                    return BufferImagem.dbase_ico_I_SOLDADO;
                case CAM_PERFIL_TIPO.Barra_Chata:
                    return BufferImagem.dbase_ico_FERRO_CHATO;
                case CAM_PERFIL_TIPO.T_Soldado:
                    return BufferImagem.dbase_ico_T;
                case CAM_PERFIL_TIPO.Barra_Redonda:
                    return BufferImagem.dbase_ico_REDONDO;
                case CAM_PERFIL_TIPO.L_Laminado:
                    return BufferImagem.dbase_ico_L;
                case CAM_PERFIL_TIPO.INP:
                    return BufferImagem.dbase_ico_INP;
                case CAM_PERFIL_TIPO.WLam:
                    return BufferImagem.dbase_ico_W;
                case CAM_PERFIL_TIPO.UNP:
                    return BufferImagem.dbase_ico_UNP;
                case CAM_PERFIL_TIPO.Chapa_Xadrez:
                    return BufferImagem.dbase_ico_UNITARIO;
                case CAM_PERFIL_TIPO.Chapa:
                    return BufferImagem.dbase_ico_M2;
                case CAM_PERFIL_TIPO.U_Dobrado:
                    return BufferImagem.dbase_ico_U;
                case CAM_PERFIL_TIPO.UAP:
                    return BufferImagem.dbase_ico_UAP;
                case CAM_PERFIL_TIPO._Desconhecido:
                    return BufferImagem.question;
                case CAM_PERFIL_TIPO._Erro:
                    return BufferImagem.dialog_warning;
                case CAM_PERFIL_TIPO.Generico:
                    return BufferImagem.dbase_ico_GENERICO;
                case CAM_PERFIL_TIPO.T_Aresta_Redondo:
                    return BufferImagem.dbase_ico_T_ARESTA_REDONDO;
                case CAM_PERFIL_TIPO.Z_Laminado:
                    return BufferImagem.dbase_ico_Z_Laminado;
                case CAM_PERFIL_TIPO.Elemento_Unitario:
                    return BufferImagem.dbase_ico_UNITARIO;
                case CAM_PERFIL_TIPO.Especial_2:
                    return BufferImagem.dbase_ico_ESPECIAL_2;
                case CAM_PERFIL_TIPO.Especial_3:
                    return BufferImagem.dbase_ico_ESPECIAL_3;
                default:
                    return BufferImagem.question;
            }
        }
        public static CAM_FAMILIA GetFamilia(this CAM_PERFIL_TIPO Tipo)
        {

            switch (Tipo)
            {
                case CAM_PERFIL_TIPO.Z_Purlin:
                    return CAM_FAMILIA.Dobrado;
                case CAM_PERFIL_TIPO.Especial_1:
                    return CAM_FAMILIA.Especial;
                case CAM_PERFIL_TIPO.Especial_2:
                    return CAM_FAMILIA.Especial;
                case CAM_PERFIL_TIPO.Especial_3:
                    return CAM_FAMILIA.Especial;
                case CAM_PERFIL_TIPO.Z_Dobrado:
                    return CAM_FAMILIA.Dobrado;
                case CAM_PERFIL_TIPO.C_Enrigecido:
                    return CAM_FAMILIA.Dobrado;
                case CAM_PERFIL_TIPO.L_Dobrado:
                    return CAM_FAMILIA.Dobrado;
                case CAM_PERFIL_TIPO.Cartola:
                    return CAM_FAMILIA.Dobrado;
                case CAM_PERFIL_TIPO.Tubo_Redondo:
                    return CAM_FAMILIA.Laminado;
                case CAM_PERFIL_TIPO.Tubo_Quadrado:
                    return CAM_FAMILIA.Laminado;
                case CAM_PERFIL_TIPO.Caixao:
                    return CAM_FAMILIA.Soldado;
                case CAM_PERFIL_TIPO.I_Soldado:
                    return CAM_FAMILIA.Soldado;
                case CAM_PERFIL_TIPO.Barra_Chata:
                    return CAM_FAMILIA.Laminado;
                case CAM_PERFIL_TIPO.T_Soldado:
                    return CAM_FAMILIA.Soldado;
                case CAM_PERFIL_TIPO.Barra_Redonda:
                    return CAM_FAMILIA.Laminado;
                case CAM_PERFIL_TIPO.L_Laminado:
                    return CAM_FAMILIA.Laminado;
                case CAM_PERFIL_TIPO.INP:
                    return CAM_FAMILIA.Laminado;
                case CAM_PERFIL_TIPO.WLam:
                    return CAM_FAMILIA.Laminado;
                case CAM_PERFIL_TIPO.UNP:
                    return CAM_FAMILIA.Laminado;
                case CAM_PERFIL_TIPO._Desconhecido:
                    return CAM_FAMILIA._Desconhecido;
                case CAM_PERFIL_TIPO.Chapa_Xadrez:
                    return CAM_FAMILIA.Chapa;
                case CAM_PERFIL_TIPO.Chapa:
                    return CAM_FAMILIA.Chapa;
                case CAM_PERFIL_TIPO.U_Dobrado:
                    return CAM_FAMILIA.Dobrado;
                case CAM_PERFIL_TIPO.UAP:
                    return CAM_FAMILIA.Laminado;
                case CAM_PERFIL_TIPO._Erro:
                    return CAM_FAMILIA._Erro;
                case CAM_PERFIL_TIPO.Generico:
                    return CAM_FAMILIA.Generico;
                case CAM_PERFIL_TIPO.T_Aresta_Redondo:
                    return CAM_FAMILIA.Especial;
                case CAM_PERFIL_TIPO.Z_Laminado:
                    return CAM_FAMILIA.Especial;
                case CAM_PERFIL_TIPO.Elemento_Unitario:
                    return CAM_FAMILIA.Generico;
            }

            return CAM_FAMILIA._Desconhecido;
        }
        public static OrigemLiv Origem(this CAM_PERFIL_TIPO tipo, FaceNum face)
        {
            switch (face)
            {
                case FaceNum.LIV1:
                    return OrigemLiv.Cima_Baixo;

                case FaceNum.LIV2:
                    if (tipo == CAM_PERFIL_TIPO.C_Enrigecido
                        | tipo == CAM_PERFIL_TIPO.L_Dobrado
                        | tipo == CAM_PERFIL_TIPO.L_Laminado
                        | tipo == CAM_PERFIL_TIPO.Tubo_Quadrado
                        | tipo == CAM_PERFIL_TIPO.U_Dobrado
                        | tipo == CAM_PERFIL_TIPO.UAP
                        | tipo == CAM_PERFIL_TIPO.UNP
                        | tipo == CAM_PERFIL_TIPO.Z_Dobrado
                        | tipo == CAM_PERFIL_TIPO.Z_Purlin
                        )
                    {
                        return OrigemLiv.Baixo_Cima;
                    }
                    else if (tipo == CAM_PERFIL_TIPO.Caixao
                        | tipo == CAM_PERFIL_TIPO.Cartola
                        | tipo == CAM_PERFIL_TIPO.I_Soldado
                        | tipo == CAM_PERFIL_TIPO.WLam
                        | tipo == CAM_PERFIL_TIPO.INP
                        | tipo == CAM_PERFIL_TIPO.T_Soldado
                        | tipo == CAM_PERFIL_TIPO.T_Aresta_Redondo
                        )
                    {
                        return OrigemLiv.Centro;
                    }
                    else
                    {
                        return OrigemLiv.NaoTem;
                    }

                case FaceNum.LIV3:
                    if (tipo == CAM_PERFIL_TIPO.Z_Purlin)
                    {
                        return OrigemLiv.Baixo_Cima;
                    }
                    else if (
                            tipo == CAM_PERFIL_TIPO.Caixao
                          | tipo == CAM_PERFIL_TIPO.Cartola
                          | tipo == CAM_PERFIL_TIPO.I_Soldado
                          | tipo == CAM_PERFIL_TIPO.WLam
                          | tipo == CAM_PERFIL_TIPO.INP
                          )
                    {
                        return OrigemLiv.Centro;
                    }
                    else if (
                           tipo == CAM_PERFIL_TIPO.C_Enrigecido
                         | tipo == CAM_PERFIL_TIPO.Tubo_Quadrado
                         | tipo == CAM_PERFIL_TIPO.U_Dobrado
                         | tipo == CAM_PERFIL_TIPO.UAP
                         | tipo == CAM_PERFIL_TIPO.UNP
                         )
                    {
                        return OrigemLiv.Cima_Baixo;
                    }
                    else if (tipo == CAM_PERFIL_TIPO.Z_Dobrado)
                    {
                        return OrigemLiv.Cima_BaixoRebatido;
                    }
                    else
                    {
                        return OrigemLiv.NaoTem;
                    }

                case FaceNum.LIV4:
                    return OrigemLiv.NaoTem;
                case FaceNum.LIV1_Recortado:
                    return OrigemLiv.Cima_Baixo;
                case FaceNum.LIV4_Recortado:
                    return OrigemLiv.Cima_Baixo;
            }
            return OrigemLiv.NaoTem;
        }
        public static CAM_PRIMITIVO GetPrimitivo(this CAM_PERFIL_TIPO tipo)
        {
            switch (tipo)
            {
                case CAM_PERFIL_TIPO.Z_Purlin:
                    return CAM_PRIMITIVO.Z1;

                case CAM_PERFIL_TIPO.Z_Dobrado:
                    return CAM_PRIMITIVO.Z;
                case CAM_PERFIL_TIPO.C_Enrigecido:
                    return CAM_PRIMITIVO.C;
                case CAM_PERFIL_TIPO.L_Dobrado:
                    return CAM_PRIMITIVO.L;
                case CAM_PERFIL_TIPO.Cartola:
                    return CAM_PRIMITIVO.Omega;
                case CAM_PERFIL_TIPO.Tubo_Redondo:
                    return CAM_PRIMITIVO.O;
                case CAM_PERFIL_TIPO.Tubo_Quadrado:
                    return CAM_PRIMITIVO.II;
                case CAM_PERFIL_TIPO.Caixao:
                    return CAM_PRIMITIVO.II;
                case CAM_PERFIL_TIPO.I_Soldado:
                    return CAM_PRIMITIVO.H;
                case CAM_PERFIL_TIPO.Barra_Chata:
                    return CAM_PRIMITIVO._;
                case CAM_PERFIL_TIPO.T_Soldado:
                    return CAM_PRIMITIVO.T;
                case CAM_PERFIL_TIPO.Barra_Redonda:
                    return CAM_PRIMITIVO.O;
                case CAM_PERFIL_TIPO.L_Laminado:
                    return CAM_PRIMITIVO.L;
                case CAM_PERFIL_TIPO.INP:
                    return CAM_PRIMITIVO.H;
                case CAM_PERFIL_TIPO.WLam:
                    return CAM_PRIMITIVO.H;
                case CAM_PERFIL_TIPO.UNP:
                    return CAM_PRIMITIVO.U;
                case CAM_PERFIL_TIPO.Chapa_Xadrez:
                    return CAM_PRIMITIVO._;
                case CAM_PERFIL_TIPO.Chapa:
                    return CAM_PRIMITIVO._;
                case CAM_PERFIL_TIPO.U_Dobrado:
                    return CAM_PRIMITIVO.U;
                case CAM_PERFIL_TIPO.UAP:
                    return CAM_PRIMITIVO.U;
                case CAM_PERFIL_TIPO._Erro:
                    return CAM_PRIMITIVO.ERR;
                case CAM_PERFIL_TIPO.Especial_1:
                    return CAM_PRIMITIVO.U;
                case CAM_PERFIL_TIPO.Especial_2:
                    return CAM_PRIMITIVO.G;
                case CAM_PERFIL_TIPO.Especial_3:
                    return CAM_PRIMITIVO.W;
                case CAM_PERFIL_TIPO._Desconhecido:
                case CAM_PERFIL_TIPO.Generico:
                case CAM_PERFIL_TIPO.Elemento_Unitario:
                    return CAM_PRIMITIVO.ERR;
                case CAM_PERFIL_TIPO.T_Aresta_Redondo:
                    return CAM_PRIMITIVO.T;
                case CAM_PERFIL_TIPO.Z_Laminado:
                    return CAM_PRIMITIVO.Z;
            }

            return CAM_PRIMITIVO.ERR;
        }
        public static List<string> SubCams(this CAM_PERFIL_TIPO Tipo, string Nome)
        {
            List<string> Retorno = new List<string>();

            var Familia = GetFamilia(Tipo);

            if (Familia == CAM_FAMILIA.Dobrado)
            {
                Retorno.Add(Nome + "_U");
            }
            else if (Familia == CAM_FAMILIA.Soldado)
            {
                if (Tipo == CAM_PERFIL_TIPO.T_Soldado)
                {
                    Retorno.Add(Nome + "_1");
                    Retorno.Add(Nome + "_2");
                }
                else if (Tipo == CAM_PERFIL_TIPO.I_Soldado)
                {
                    Retorno.Add(Nome + "_1");
                    Retorno.Add(Nome + "_2");
                    Retorno.Add(Nome + "_3");
                }
                else if (Tipo == CAM_PERFIL_TIPO.Caixao)
                {
                    Retorno.Add(Nome + "_1");
                    Retorno.Add(Nome + "_2");
                    Retorno.Add(Nome + "_3");
                    Retorno.Add(Nome + "_4");
                }
            }

            return Retorno;
        }
        public static int GetDobras(this CAM_PERFIL_TIPO Tipo)
        {

            switch (Tipo)
            {
                case CAM_PERFIL_TIPO.Z_Purlin:
                    return 4;
                case CAM_PERFIL_TIPO.Especial_1:
                    return 2;
                case CAM_PERFIL_TIPO.Especial_2:
                    return 3;
                case CAM_PERFIL_TIPO.Especial_3:
                    return 3;
                case CAM_PERFIL_TIPO.Z_Dobrado:
                    return 2;
                case CAM_PERFIL_TIPO.C_Enrigecido:
                    return 4;
                case CAM_PERFIL_TIPO.L_Dobrado:
                    return 1;
                case CAM_PERFIL_TIPO.Cartola:
                    return 4;
                case CAM_PERFIL_TIPO.U_Dobrado:
                    return 2;


                case CAM_PERFIL_TIPO.Tubo_Redondo:
                case CAM_PERFIL_TIPO.Tubo_Quadrado:
                case CAM_PERFIL_TIPO.Caixao:
                case CAM_PERFIL_TIPO.I_Soldado:
                case CAM_PERFIL_TIPO.Barra_Chata:
                case CAM_PERFIL_TIPO.T_Soldado:
                case CAM_PERFIL_TIPO.Barra_Redonda:
                case CAM_PERFIL_TIPO.L_Laminado:
                case CAM_PERFIL_TIPO.INP:
                case CAM_PERFIL_TIPO.WLam:
                case CAM_PERFIL_TIPO.UNP:
                case CAM_PERFIL_TIPO._Desconhecido:
                case CAM_PERFIL_TIPO.Chapa_Xadrez:
                case CAM_PERFIL_TIPO.Chapa:
                case CAM_PERFIL_TIPO.UAP:
                case CAM_PERFIL_TIPO._Erro:
                case CAM_PERFIL_TIPO.Generico:
                case CAM_PERFIL_TIPO.T_Aresta_Redondo:
                case CAM_PERFIL_TIPO.Z_Laminado:
                case CAM_PERFIL_TIPO.Elemento_Unitario:
                    break;
            }

            return 0;
        }
        public static int GetFaces(this CAM_PERFIL_TIPO Tipo)
        {
            switch (Tipo)
            {
                case CAM_PERFIL_TIPO.Z_Purlin:
                    return 3;
                case CAM_PERFIL_TIPO.Especial_1:
                    return 3;
                case CAM_PERFIL_TIPO.Especial_2:
                    return 3;
                case CAM_PERFIL_TIPO.Especial_3:
                    return 3;
                case CAM_PERFIL_TIPO.Z_Dobrado:
                    return 3;
                case CAM_PERFIL_TIPO.C_Enrigecido:
                    return 3;
                case CAM_PERFIL_TIPO.L_Dobrado:
                    return 2;
                case CAM_PERFIL_TIPO.Cartola:
                    return 3;
                case CAM_PERFIL_TIPO.Tubo_Redondo:
                    return 1;
                case CAM_PERFIL_TIPO.Tubo_Quadrado:
                    return 3;
                case CAM_PERFIL_TIPO.Caixao:
                    return 3;
                case CAM_PERFIL_TIPO.I_Soldado:
                    return 3;
                case CAM_PERFIL_TIPO.Barra_Chata:
                    return 1;
                case CAM_PERFIL_TIPO.T_Soldado:
                    return 2;
                case CAM_PERFIL_TIPO.Barra_Redonda:
                    return 1;
                case CAM_PERFIL_TIPO.L_Laminado:
                    return 2;
                case CAM_PERFIL_TIPO.INP:
                    return 3;
                case CAM_PERFIL_TIPO.WLam:
                    return 3;
                case CAM_PERFIL_TIPO.UNP:
                    return 3;
                case CAM_PERFIL_TIPO.Chapa_Xadrez:
                    return 1;
                case CAM_PERFIL_TIPO.Chapa:
                    return 1;
                case CAM_PERFIL_TIPO.U_Dobrado:
                    return 3;
                case CAM_PERFIL_TIPO.UAP:
                    return 3;
                case CAM_PERFIL_TIPO._Desconhecido:
                    return 1;
                case CAM_PERFIL_TIPO._Erro:
                    return 1;
                case CAM_PERFIL_TIPO.Generico:
                    break;
                case CAM_PERFIL_TIPO.Elemento_Unitario:
                    break;
                case CAM_PERFIL_TIPO.T_Aresta_Redondo:
                    return 2;
                case CAM_PERFIL_TIPO.Z_Laminado:
                    return 3;
            }
            return 1;
        }
        public static ImageSource GetIcone(this CAM_PERFIL_TIPO Tipo)
        {
            switch (Tipo)
            {
                case CAM_PERFIL_TIPO.Z_Purlin:
                    return BufferImagem.dbase_ico_Z_Purlin;
                case CAM_PERFIL_TIPO.Especial_1:
                    return BufferImagem.dbase_ico_ESPECIAL_1;
                case CAM_PERFIL_TIPO.Especial_2:
                    return BufferImagem.dbase_ico_ESPECIAL_2;
                case CAM_PERFIL_TIPO.Especial_3:
                    return BufferImagem.dbase_ico_ESPECIAL_3;
                case CAM_PERFIL_TIPO.Z_Dobrado:
                    return BufferImagem.dbase_ico_Z_Dobrado;
                case CAM_PERFIL_TIPO.C_Enrigecido:
                    return BufferImagem.dbase_ico_C;
                case CAM_PERFIL_TIPO.L_Dobrado:
                    return BufferImagem.dbase_ico_L_Dobrado;
                case CAM_PERFIL_TIPO.Cartola:
                    return BufferImagem.dbase_ico_OMEGA;
                case CAM_PERFIL_TIPO.Tubo_Redondo:
                    return BufferImagem.dbase_ico_TUBO_REDONDO;
                case CAM_PERFIL_TIPO.Tubo_Quadrado:
                    return BufferImagem.dbase_ico_TUBO_RETANGULAR;
                case CAM_PERFIL_TIPO.Caixao:
                    return BufferImagem.dbase_ico_CAIXAO;
                case CAM_PERFIL_TIPO.I_Soldado:
                    return BufferImagem.dbase_ico_I_SOLDADO;
                case CAM_PERFIL_TIPO.Barra_Chata:
                    return BufferImagem.dbase_ico_FERRO_CHATO;
                case CAM_PERFIL_TIPO.T_Soldado:
                    return BufferImagem.dbase_ico_T;
                case CAM_PERFIL_TIPO.Barra_Redonda:
                    return BufferImagem.dbase_ico_REDONDO;
                case CAM_PERFIL_TIPO.L_Laminado:
                    return BufferImagem.dbase_ico_L;
                case CAM_PERFIL_TIPO.INP:
                    return BufferImagem.dbase_ico_INP;
                case CAM_PERFIL_TIPO.WLam:
                    return BufferImagem.dbase_ico_W;
                case CAM_PERFIL_TIPO.UNP:
                    return BufferImagem.dbase_ico_UNP;
                case CAM_PERFIL_TIPO.U_Dobrado:
                    return BufferImagem.dbase_ico_U;
                case CAM_PERFIL_TIPO.UAP:
                    return BufferImagem.dbase_ico_UAP;
                case CAM_PERFIL_TIPO.T_Aresta_Redondo:
                    return BufferImagem.dbase_ico_T_ARESTA_REDONDO;
                case CAM_PERFIL_TIPO.Z_Laminado:
                    return BufferImagem.dbase_ico_Z_Laminado;

                case CAM_PERFIL_TIPO.Chapa_Xadrez:
                case CAM_PERFIL_TIPO.Chapa:
                    return BufferImagem.chapa;
                case CAM_PERFIL_TIPO._Desconhecido:
                case CAM_PERFIL_TIPO.Generico:
                case CAM_PERFIL_TIPO.Elemento_Unitario:
                    return BufferImagem.nut_and_bolt_16x16;

                case CAM_PERFIL_TIPO._Erro:
                    return BufferImagem.dialog_error;


            }
            return BufferImagem.erro_cinza;

        }
        public static string GetTipoCode(this CAM_PERFIL_TIPO Tipo)
        {
            switch (Tipo)
            {
                case CAM_PERFIL_TIPO.Z_Purlin:
                    return Cfg.Init.CAM_TipoPerfil_Z_Purlin;
                case CAM_PERFIL_TIPO.Especial_1:
                    return Cfg.Init.CAM_TipoPerfil_Especial_1;
                case CAM_PERFIL_TIPO.Especial_2:
                    return Cfg.Init.CAM_TipoPerfil_Especial_2;
                case CAM_PERFIL_TIPO.Especial_3:
                    return Cfg.Init.CAM_TipoPerfil_Especial_3;
                case CAM_PERFIL_TIPO.Z_Dobrado:
                    return Cfg.Init.CAM_TipoPerfil_Z_Dobrado;
                case CAM_PERFIL_TIPO.C_Enrigecido:
                    return Cfg.Init.CAM_TipoPerfil_C_Enrigecido;
                case CAM_PERFIL_TIPO.L_Dobrado:
                    return Cfg.Init.CAM_TipoPerfil_L_Dobrado;
                case CAM_PERFIL_TIPO.Cartola:
                    return Cfg.Init.CAM_TipoPerfil_Cartola;
                case CAM_PERFIL_TIPO.Tubo_Redondo:
                    return Cfg.Init.CAM_TipoPerfil_Tubo_Redondo;
                case CAM_PERFIL_TIPO.Tubo_Quadrado:
                    return Cfg.Init.CAM_TipoPerfil_Tubo_Quadrado;
                case CAM_PERFIL_TIPO.Caixao:
                    return Cfg.Init.CAM_TipoPerfil_Caixao;
                case CAM_PERFIL_TIPO.I_Soldado:
                    return Cfg.Init.CAM_TipoPerfil_I_Soldado;
                case CAM_PERFIL_TIPO.Barra_Chata:
                    return Cfg.Init.CAM_TipoPerfil_Barra_Chata;
                case CAM_PERFIL_TIPO.T_Soldado:
                    return Cfg.Init.CAM_TipoPerfil_T_Soldado;
                case CAM_PERFIL_TIPO.Barra_Redonda:
                    return Cfg.Init.CAM_TipoPerfil_Barra_Redonda;
                case CAM_PERFIL_TIPO.L_Laminado:
                    return Cfg.Init.CAM_TipoPerfil_L_Laminado;
                case CAM_PERFIL_TIPO.INP:
                    return Cfg.Init.CAM_TipoPerfil_INP;
                case CAM_PERFIL_TIPO.WLam:
                    return Cfg.Init.CAM_TipoPerfil_WLam;
                case CAM_PERFIL_TIPO.UNP:
                    return Cfg.Init.CAM_TipoPerfil_UNP;
                case CAM_PERFIL_TIPO.Chapa_Xadrez:
                    return Cfg.Init.CAM_TipoPerfil_Chapa_Xadrez;
                case CAM_PERFIL_TIPO.U_Dobrado:
                    return Cfg.Init.CAM_TipoPerfil_U_Dobrado;
                case CAM_PERFIL_TIPO.UAP:
                    return Cfg.Init.CAM_TipoPerfil_UAP;
                case CAM_PERFIL_TIPO.T_Aresta_Redondo:
                    return Cfg.Init.CAM_TipoPerfil_T_Aresta_Redondo;
                case CAM_PERFIL_TIPO.Z_Laminado:
                    return Cfg.Init.CAM_TipoPerfil_Z_Laminado;

                case CAM_PERFIL_TIPO.Chapa:
                case CAM_PERFIL_TIPO.Elemento_Unitario:
                case CAM_PERFIL_TIPO._Desconhecido:
                case CAM_PERFIL_TIPO._Erro:
                case CAM_PERFIL_TIPO.Generico:
                default:
                    return Cfg.Init.CAM_TipoPerfil_Chapa;
            }
        }
        public static ImageSource GetImagemCadastro(this CAM_PERFIL_TIPO Tipo)
        {
            switch (Tipo)
            {
                case CAM_PERFIL_TIPO.Z_Purlin:
                    return BufferImagem.dbase_Z_Purlin;
                case CAM_PERFIL_TIPO.Especial_1:
                    return BufferImagem.dbase_ESPECIAL_1;
                case CAM_PERFIL_TIPO.Especial_2:
                    return BufferImagem.dbase_ESPECIAL_2;
                case CAM_PERFIL_TIPO.Especial_3:
                    return BufferImagem.dbase_ESPECIAL_3;
                case CAM_PERFIL_TIPO.Z_Dobrado:
                    return BufferImagem.dbase_Z_Dobrado;
                case CAM_PERFIL_TIPO.C_Enrigecido:
                    return BufferImagem.dbase_C;
                case CAM_PERFIL_TIPO.L_Dobrado:
                    return BufferImagem.dbase_L_Dobrado;
                case CAM_PERFIL_TIPO.Cartola:
                    return BufferImagem.dbase_OMEGA;
                case CAM_PERFIL_TIPO.Tubo_Redondo:
                    return BufferImagem.dbase_TUBO_REDONDO;
                case CAM_PERFIL_TIPO.Tubo_Quadrado:
                    return BufferImagem.dbase_TUBO_RETANGULAR;
                case CAM_PERFIL_TIPO.Caixao:
                    return BufferImagem.dbase_CAIXAO;
                case CAM_PERFIL_TIPO.I_Soldado:
                    return BufferImagem.dbase_I_SOLDADO;
                case CAM_PERFIL_TIPO.Barra_Chata:
                    return BufferImagem.dbase_FERRO_CHATO;
                case CAM_PERFIL_TIPO.T_Soldado:
                    return BufferImagem.dbase_T;
                case CAM_PERFIL_TIPO.Barra_Redonda:
                    return BufferImagem.dbase_REDONDO;
                case CAM_PERFIL_TIPO.L_Laminado:
                    return BufferImagem.dbase_L;
                case CAM_PERFIL_TIPO.INP:
                    return BufferImagem.dbase_INP;
                case CAM_PERFIL_TIPO.WLam:
                    return BufferImagem.dbase_W;
                case CAM_PERFIL_TIPO.UNP:
                    return BufferImagem.dbase_UNP;
                case CAM_PERFIL_TIPO.U_Dobrado:
                    return BufferImagem.dbase_U;
                case CAM_PERFIL_TIPO.UAP:
                    return BufferImagem.dbase_UAP;
                case CAM_PERFIL_TIPO.T_Aresta_Redondo:
                    return BufferImagem.dbase_T_ARESTA_REDONDO;
                case CAM_PERFIL_TIPO.Z_Laminado:
                    return BufferImagem.dbase_Z_Laminado;

                case CAM_PERFIL_TIPO.Chapa_Xadrez:
                case CAM_PERFIL_TIPO.Chapa:
                case CAM_PERFIL_TIPO._Desconhecido:
                case CAM_PERFIL_TIPO.Generico:
                case CAM_PERFIL_TIPO.Elemento_Unitario:
                    return BufferImagem.dbase_UNITARIO;

                case CAM_PERFIL_TIPO._Erro:
                    return BufferImagem.dialog_error;


            }
            return BufferImagem.erro_cinza;

        }
    }

}
