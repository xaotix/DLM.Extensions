using DLM.cam;
using DLM.vars;

namespace Conexoes
{
    public static partial class Extensoes_NORMT
    {
        public static CAM_PERFIL_PRIMITIVO GetPrimitivo(this TAB_NORMT NORMT)
        {
            switch (NORMT)
            {
                case TAB_NORMT._INVALIDO:
                case TAB_NORMT._VAZIO:
                case TAB_NORMT._DESCONTINUADO_1:
                case TAB_NORMT._DESCONTINUADO_2:
                    return CAM_PERFIL_PRIMITIVO._Desconhecido;


                case TAB_NORMT.TUBO_REDONDO:
                case TAB_NORMT.CANTONEIRA_LAMINADA:
                case TAB_NORMT.FERRO_REDONDO:
                case TAB_NORMT.BARRA_CHATA:
                case TAB_NORMT.PERFIL_I_LAMINADO:
                case TAB_NORMT.PERFIL_C_LAMINADO:
                case TAB_NORMT.TUBO_QUADRADO:
                case TAB_NORMT.TUBO_RETANGULAR:
                case TAB_NORMT.CHAPA_EXPANDIDA:
                case TAB_NORMT.PERFIL_T_LAMINADO:
                case TAB_NORMT.GRADE:
                    return CAM_PERFIL_PRIMITIVO.LAMINADO;


                case TAB_NORMT.PERFIL_I_SOLDADO:
                    return CAM_PERFIL_PRIMITIVO.SOLDADO;


                case TAB_NORMT.CHAPA:
                case TAB_NORMT.CHAPA_XADREZ:
                case TAB_NORMT.PERFIL_DOBRADO:
                case TAB_NORMT.BANZO_INFERIOR_TIPO_D:
                case TAB_NORMT.BANZO_SUPERIOR_TIPO_D:
                case TAB_NORMT.VIGA_ALMA:
                case TAB_NORMT.VIGA_MESA:
                case TAB_NORMT.PERFIL_C_PADRAO_165:
                case TAB_NORMT.PERFIL_Z_PADRAO_165:
                case TAB_NORMT.PERFIL_C_PADRAO_216:
                case TAB_NORMT.PERFIL_Z_PADRAO_216:
                case TAB_NORMT.PERFIL_C_PADRAO_292:
                case TAB_NORMT.PERFIL_Z_PADRAO_292:
                case TAB_NORMT.PERFIL_Z_ENRIJECIDO_185:
                case TAB_NORMT.PERFIL_Z_ENRIJECIDO_360:
                case TAB_NORMT.PERFIL_Z_ENRIJECIDO_245:
                case TAB_NORMT.PERFIL_Z_ENRIJECIDO_300:
                case TAB_NORMT.BANZO_INFERIOR_MEDABAR:
                case TAB_NORMT.DIAGONAL_MEDABAR:
                case TAB_NORMT.BANZO_SUPERIOR_MEDABAR:
                case TAB_NORMT.DIAGONAL_MEDAJOIST:
                case TAB_NORMT.PERFIL_C_PADRAO_125:
                case TAB_NORMT.PERFIL_C_PADRAO_185:
                    return CAM_PERFIL_PRIMITIVO.CHAPA;


                case TAB_NORMT.SSR1:
                case TAB_NORMT.SSR2:
                case TAB_NORMT.SSR1M:
                case TAB_NORMT.SSR1BM:
                case TAB_NORMT.SSR1F:
                case TAB_NORMT.SSR1BF:
                case TAB_NORMT.STEEL_DECK:
                case TAB_NORMT.PANEL_RIB_II:
                case TAB_NORMT.PANEL_RIB_III:
                case TAB_NORMT.TELHA_ONDULADA:
                case TAB_NORMT.TELA:
                case TAB_NORMT.TELHA_FORRO:
                case TAB_NORMT.TRINS:
                    return CAM_PERFIL_PRIMITIVO.CHAPA;


                case TAB_NORMT.MARCA_KIT_PIE:
                case TAB_NORMT.ALMOX_FABRICACAO:
                case TAB_NORMT.KIT_PIE:
                    return CAM_PERFIL_PRIMITIVO.ALMOX;
            }
            return CAM_PERFIL_PRIMITIVO._Desconhecido;
        }
        public static CAM_FAMILIA GetFamilia(this TAB_NORMT NORMT)
        {
            switch (NORMT)
            {
                case TAB_NORMT.DIAGONAL_MEDAJOIST:
                case TAB_NORMT.BANZO_SUPERIOR_TIPO_D:
                case TAB_NORMT.BANZO_INFERIOR_TIPO_D:
                case TAB_NORMT.PERFIL_DOBRADO:
                case TAB_NORMT.BANZO_INFERIOR_MEDABAR:
                case TAB_NORMT.BANZO_SUPERIOR_MEDABAR:
                case TAB_NORMT.DIAGONAL_MEDABAR:
                case TAB_NORMT.PERFIL_C_PADRAO_165:
                case TAB_NORMT.PERFIL_Z_PADRAO_165:
                case TAB_NORMT.PERFIL_C_PADRAO_216:
                case TAB_NORMT.PERFIL_Z_PADRAO_216:
                case TAB_NORMT.PERFIL_C_PADRAO_292:
                case TAB_NORMT.PERFIL_Z_PADRAO_292:
                case TAB_NORMT.PERFIL_Z_ENRIJECIDO_185:
                case TAB_NORMT.PERFIL_Z_ENRIJECIDO_360:
                case TAB_NORMT.PERFIL_Z_ENRIJECIDO_245:
                case TAB_NORMT.PERFIL_Z_ENRIJECIDO_300:
                case TAB_NORMT.PERFIL_C_PADRAO_125:
                case TAB_NORMT.PERFIL_C_PADRAO_185:
                    return CAM_FAMILIA.Dobrado;



                case TAB_NORMT.SSR1:
                case TAB_NORMT.SSR2:
                case TAB_NORMT.SSR1M:
                case TAB_NORMT.SSR1BM:
                case TAB_NORMT.SSR1F:
                case TAB_NORMT.SSR1BF:
                case TAB_NORMT.STEEL_DECK:
                case TAB_NORMT.PANEL_RIB_II:
                case TAB_NORMT.PANEL_RIB_III:
                case TAB_NORMT.TELHA_ONDULADA:
                case TAB_NORMT.TELHA_FORRO:
                case TAB_NORMT.TRINS:
                    return CAM_FAMILIA.Chapa;

                case TAB_NORMT.BARRA_CHATA:
                case TAB_NORMT.CHAPA_EXPANDIDA:
                case TAB_NORMT.GRADE:
                case TAB_NORMT.TELA:
                case TAB_NORMT.CHAPA_XADREZ:
                    return CAM_FAMILIA.Chapa;


                case TAB_NORMT.CHAPA:
                case TAB_NORMT.VIGA_ALMA:
                case TAB_NORMT.VIGA_MESA:
                    return CAM_FAMILIA.Chapa;

                case TAB_NORMT.PERFIL_T_LAMINADO:
                case TAB_NORMT.PERFIL_I_LAMINADO:
                case TAB_NORMT.TUBO_RETANGULAR:
                case TAB_NORMT.FERRO_REDONDO:
                case TAB_NORMT.TUBO_QUADRADO:
                case TAB_NORMT.CANTONEIRA_LAMINADA:
                case TAB_NORMT.PERFIL_C_LAMINADO:
                case TAB_NORMT.TUBO_REDONDO:
                    return CAM_FAMILIA.Laminado;

                case TAB_NORMT.PERFIL_I_SOLDADO:
                    return CAM_FAMILIA.Soldado;


                case TAB_NORMT._INVALIDO:
                case TAB_NORMT._VAZIO:
                case TAB_NORMT.MARCA_KIT_PIE:
                case TAB_NORMT.ALMOX_FABRICACAO:
                case TAB_NORMT.KIT_PIE:
                case TAB_NORMT._DESCONTINUADO_1:
                case TAB_NORMT._DESCONTINUADO_2:
                    return CAM_FAMILIA._Erro;
            }

            return CAM_FAMILIA._Desconhecido;
        }
        public static CAM_PERFIL_TIPO GetTipo(this TAB_NORMT NORMT)
        {
            switch (NORMT)
            {
                case TAB_NORMT._INVALIDO:
                case TAB_NORMT._VAZIO:
                case TAB_NORMT._DESCONTINUADO_1:
                case TAB_NORMT._DESCONTINUADO_2:
                    break;


                case TAB_NORMT.CHAPA:
                case TAB_NORMT.PERFIL_DOBRADO:
                case TAB_NORMT.BANZO_INFERIOR_TIPO_D:
                case TAB_NORMT.BANZO_SUPERIOR_TIPO_D:
                case TAB_NORMT.BANZO_INFERIOR_MEDABAR:
                case TAB_NORMT.BANZO_SUPERIOR_MEDABAR:
                case TAB_NORMT.CHAPA_XADREZ:
                case TAB_NORMT.VIGA_ALMA:
                case TAB_NORMT.VIGA_MESA:
                case TAB_NORMT.CHAPA_EXPANDIDA:
                case TAB_NORMT.SSR1:
                case TAB_NORMT.SSR2:
                case TAB_NORMT.SSR1M:
                case TAB_NORMT.SSR1BM:
                case TAB_NORMT.SSR1F:
                case TAB_NORMT.SSR1BF:
                case TAB_NORMT.STEEL_DECK:
                case TAB_NORMT.PANEL_RIB_II:
                case TAB_NORMT.PANEL_RIB_III:
                case TAB_NORMT.TELHA_ONDULADA:
                case TAB_NORMT.GRADE:
                case TAB_NORMT.TELA:
                case TAB_NORMT.TELHA_FORRO:
                case TAB_NORMT.TRINS:
                    return CAM_PERFIL_TIPO.Chapa;


                case TAB_NORMT.CANTONEIRA_LAMINADA:
                    return CAM_PERFIL_TIPO.L_Laminado;



                case TAB_NORMT.FERRO_REDONDO:
                    return CAM_PERFIL_TIPO.Barra_Redonda;


                case TAB_NORMT.BARRA_CHATA:
                    return CAM_PERFIL_TIPO.Barra_Chata;


                case TAB_NORMT.PERFIL_I_LAMINADO:
                case TAB_NORMT.PERFIL_C_LAMINADO:
                case TAB_NORMT.PERFIL_T_LAMINADO:
                    return CAM_PERFIL_TIPO.WLam;



                case TAB_NORMT.PERFIL_I_SOLDADO:
                    return CAM_PERFIL_TIPO.I_Soldado;



                case TAB_NORMT.PERFIL_C_PADRAO_125:
                case TAB_NORMT.PERFIL_C_PADRAO_165:
                case TAB_NORMT.PERFIL_C_PADRAO_216:
                case TAB_NORMT.PERFIL_C_PADRAO_292:
                case TAB_NORMT.PERFIL_C_PADRAO_185:
                    return CAM_PERFIL_TIPO.C_Enrigecido;



                case TAB_NORMT.PERFIL_Z_PADRAO_165:
                case TAB_NORMT.PERFIL_Z_PADRAO_216:
                case TAB_NORMT.PERFIL_Z_PADRAO_292:
                case TAB_NORMT.PERFIL_Z_ENRIJECIDO_185:
                case TAB_NORMT.PERFIL_Z_ENRIJECIDO_360:
                case TAB_NORMT.PERFIL_Z_ENRIJECIDO_245:
                case TAB_NORMT.PERFIL_Z_ENRIJECIDO_300:
                    return CAM_PERFIL_TIPO.Z_Purlin;



                case TAB_NORMT.TUBO_REDONDO:
                    return CAM_PERFIL_TIPO.Tubo_Redondo;


                case TAB_NORMT.DIAGONAL_MEDABAR:
                case TAB_NORMT.DIAGONAL_MEDAJOIST:
                    return CAM_PERFIL_TIPO.U_Dobrado;

                case TAB_NORMT.TUBO_QUADRADO:
                case TAB_NORMT.TUBO_RETANGULAR:
                    return CAM_PERFIL_TIPO.Tubo_Quadrado;


                case TAB_NORMT.MARCA_KIT_PIE:
                case TAB_NORMT.ALMOX_FABRICACAO:
                case TAB_NORMT.KIT_PIE:
                    return CAM_PERFIL_TIPO.Elemento_Unitario;
            }


            return CAM_PERFIL_TIPO._Desconhecido;
        }
        public static CAM_TIPO_BASE GetTipoBase(this TAB_NORMT NORMT)
        {
            if (NORMT == 0)
            {
                return CAM_TIPO_BASE.Marca;
            }
            else if (
                  NORMT == TAB_NORMT.CHAPA |
                  NORMT == TAB_NORMT.VIGA_MESA |
                  NORMT == TAB_NORMT.VIGA_ALMA
                )
            {
                return CAM_TIPO_BASE.Chapa;
            }
            else if (
                  NORMT == TAB_NORMT.PERFIL_DOBRADO |
                  NORMT == TAB_NORMT.BANZO_INFERIOR_MEDABAR |
                  NORMT == TAB_NORMT.BANZO_INFERIOR_TIPO_D |
                  NORMT == TAB_NORMT.BANZO_SUPERIOR_MEDABAR |
                  NORMT == TAB_NORMT.BANZO_SUPERIOR_TIPO_D |
                  NORMT == TAB_NORMT.DIAGONAL_MEDABAR |
                  NORMT == TAB_NORMT.DIAGONAL_MEDAJOIST
                  )
            {
                return CAM_TIPO_BASE.Perfil_Dobrado;
            }
            else if (
                  NORMT == TAB_NORMT.PERFIL_C_PADRAO_125
                | NORMT == TAB_NORMT.PERFIL_C_PADRAO_165
                | NORMT == TAB_NORMT.PERFIL_C_PADRAO_185
                | NORMT == TAB_NORMT.PERFIL_C_PADRAO_216
                | NORMT == TAB_NORMT.PERFIL_C_PADRAO_292
                )
            {
                return CAM_TIPO_BASE.Purlin_C;
            }
            else if (
                 NORMT == TAB_NORMT.PERFIL_Z_ENRIJECIDO_185
               | NORMT == TAB_NORMT.PERFIL_Z_ENRIJECIDO_360
               | NORMT == TAB_NORMT.PERFIL_Z_ENRIJECIDO_245
               | NORMT == TAB_NORMT.PERFIL_Z_ENRIJECIDO_300
               | NORMT == TAB_NORMT.PERFIL_Z_PADRAO_165
               | NORMT == TAB_NORMT.PERFIL_Z_PADRAO_216
               | NORMT == TAB_NORMT.PERFIL_Z_PADRAO_292
               )
            {
                return CAM_TIPO_BASE.Purlin_Z;
            }
            else if (
                NORMT == TAB_NORMT.PANEL_RIB_II |
                NORMT == TAB_NORMT.PANEL_RIB_III |
                NORMT == TAB_NORMT.SSR1 |
                NORMT == TAB_NORMT.SSR1BF |
                NORMT == TAB_NORMT.SSR1BM |
                NORMT == TAB_NORMT.SSR1F |
                NORMT == TAB_NORMT.SSR1M |
                NORMT == TAB_NORMT.SSR2 |
                NORMT == TAB_NORMT.STEEL_DECK |
                NORMT == TAB_NORMT.TELHA_FORRO |
                NORMT == TAB_NORMT.TELHA_ONDULADA)
            {
                return CAM_TIPO_BASE.Telha;
            }
            else if (
                NORMT == TAB_NORMT.CANTONEIRA_LAMINADA |
                NORMT == TAB_NORMT.CHAPA_XADREZ |
                NORMT == TAB_NORMT.CHAPA_EXPANDIDA |
                NORMT == TAB_NORMT.FERRO_REDONDO |
                NORMT == TAB_NORMT.TUBO_QUADRADO |
                NORMT == TAB_NORMT.TUBO_REDONDO |
                NORMT == TAB_NORMT.TUBO_RETANGULAR |
                NORMT == TAB_NORMT.PERFIL_C_LAMINADO |
                NORMT == TAB_NORMT.PERFIL_T_LAMINADO |
                NORMT == TAB_NORMT.GRADE |
                NORMT == TAB_NORMT.TELA
                )
            {
                return CAM_TIPO_BASE.Laminado;
            }
            else if (NORMT == TAB_NORMT.PERFIL_I_LAMINADO)
            {
                return CAM_TIPO_BASE.W_Lam;
            }
            else if (NORMT == TAB_NORMT.PERFIL_I_SOLDADO)
            {
                return CAM_TIPO_BASE.I_Soldado;
            }
            else if (NORMT == TAB_NORMT.TRINS)
            {
                return CAM_TIPO_BASE.Trins;
            }
            else if (
                  NORMT == TAB_NORMT.KIT_PIE
                | NORMT == TAB_NORMT.MARCA_KIT_PIE
                | NORMT == TAB_NORMT.ALMOX_FABRICACAO
                )
            {
                return CAM_TIPO_BASE.Almox;
            }
            else
            {
                return CAM_TIPO_BASE._Desconhecido;
            }
        }
        public static TAB_NORMT NORMT(this ReadCAM cam, string MAKTX = "")
        {
            return NORMT(cam.Perfil.Tipo, MAKTX, cam.Nome, cam.Comprimento, cam.Formato.GetDobras(), cam.Perfil.Descricao);
        }
        public static TAB_NORMT NORMT(this DLM.cam.NC1 cam, string maktx = "")
        {
            var perfil = cam.GetPerfil();
            return NORMT(perfil.Tipo, maktx, cam.Nome, cam.Comprimento, perfil.Dobras, perfil.Descricao);
        }
        public static TAB_NORMT NORMT(this CAM_PERFIL_TIPO Tipo, string MAKTX, string Nome, double Comprimento, int Dobras, string Perfil = null)
        {
            switch (Tipo)
            {
                case CAM_PERFIL_TIPO.Caixao:
                case CAM_PERFIL_TIPO.I_Soldado:
                    return TAB_NORMT.PERFIL_I_SOLDADO;

                case CAM_PERFIL_TIPO.T_Aresta_Redondo:
                case CAM_PERFIL_TIPO.T_Soldado:
                    return TAB_NORMT.PERFIL_T_LAMINADO;

                case CAM_PERFIL_TIPO.Z_Dobrado:
                case CAM_PERFIL_TIPO.U_Dobrado:
                case CAM_PERFIL_TIPO.L_Dobrado:
                case CAM_PERFIL_TIPO.Cartola:
                case CAM_PERFIL_TIPO.Especial_1:
                case CAM_PERFIL_TIPO.Especial_2:
                case CAM_PERFIL_TIPO.Especial_3:
                    return TAB_NORMT.PERFIL_DOBRADO;


                case CAM_PERFIL_TIPO.L_Laminado:
                    return TAB_NORMT.CANTONEIRA_LAMINADA;

                case CAM_PERFIL_TIPO.Tubo_Quadrado:
                    return TAB_NORMT.TUBO_RETANGULAR;

                case CAM_PERFIL_TIPO.Barra_Redonda:
                    return TAB_NORMT.FERRO_REDONDO;

                case CAM_PERFIL_TIPO.Tubo_Redondo:
                    return TAB_NORMT.TUBO_REDONDO;

                case CAM_PERFIL_TIPO.Barra_Chata:
                    return TAB_NORMT.BARRA_CHATA;


                case CAM_PERFIL_TIPO.Z_Purlin:
                case CAM_PERFIL_TIPO.C_Enrigecido:
                    var purlin = DBases.GetMBS_Perfil_Purlin(Perfil);
                    if(purlin!=null)
                    {
                        if (purlin.Enrigecida)
                        {
                            if (Comprimento < Cfg.Init.TestList_Purlin_Enrigecida_Comp_Min)
                            {
                                return TAB_NORMT.PERFIL_DOBRADO;
                            }
                        }
                        else
                        {
                            if (Comprimento < Cfg.Init.TestList_Purlin_Normal_Comp_Min)
                            {
                                return TAB_NORMT.PERFIL_DOBRADO;
                            }
                        }
                        return purlin.NORMT;
                    }

                    return TAB_NORMT.PERFIL_DOBRADO;

                case CAM_PERFIL_TIPO.Chapa_Xadrez:
                    if (Dobras > 0)
                    {
                        return TAB_NORMT.PERFIL_DOBRADO;
                    }
                    return TAB_NORMT.CHAPA_XADREZ;


                case CAM_PERFIL_TIPO.Chapa:
                    var tipo = Nome.GetTipoDesmembrado();
                    if (tipo == CAM_TIPO_DESMEMBRADO.Mesa_I | tipo == CAM_TIPO_DESMEMBRADO.Mesa_S)
                    {
                        return TAB_NORMT.VIGA_MESA;
                    }
                    else if (tipo == CAM_TIPO_DESMEMBRADO.Alma)
                    {
                        return TAB_NORMT.VIGA_ALMA;
                    }
                    else if (Dobras > 0)
                    {
                        return TAB_NORMT.PERFIL_DOBRADO;
                    }
                    else if (MAKTX.Replace(" ", "") == "BANZOSUPERIORMEDABAR")
                    {
                        return TAB_NORMT.BANZO_SUPERIOR_MEDABAR;
                    }
                    else if (MAKTX.Replace(" ", "") == "BANZOINFERIORMEDABAR")
                    {
                        return TAB_NORMT.BANZO_INFERIOR_MEDABAR;
                    }

                    return TAB_NORMT.CHAPA;


                case CAM_PERFIL_TIPO.INP:
                case CAM_PERFIL_TIPO.WLam:
                    return TAB_NORMT.PERFIL_I_LAMINADO;



                case CAM_PERFIL_TIPO.UNP:
                case CAM_PERFIL_TIPO.UAP:
                    return TAB_NORMT.PERFIL_C_LAMINADO;

                case CAM_PERFIL_TIPO.Elemento_Unitario:
                case CAM_PERFIL_TIPO.Generico:
                    return TAB_NORMT.KIT_PIE;

                case CAM_PERFIL_TIPO.Z_Laminado:
                case CAM_PERFIL_TIPO._Desconhecido:
                case CAM_PERFIL_TIPO._Erro:
                    return TAB_NORMT._VAZIO;
                default:
                    break;
            }

            if (Nome.EndsWith("_A"))
            {
                return TAB_NORMT.ALMOX_FABRICACAO;
            }

            return TAB_NORMT._INVALIDO;
        }
        public static TAB_NORMT NORMT(this string valor)
        {

            if (valor == null)
            {
                return TAB_NORMT._VAZIO;
            }

            if (valor.ESoNumero())
            {
                var str = valor.Int();

                if (str == 0)
                {
                    return TAB_NORMT._VAZIO;
                }
                return (TAB_NORMT)str;

            }

            var vlr = valor.GetEnum<TAB_NORMT>();
            return vlr;

            //return TAB_NORMT._INVALIDO;
        }
    }
}
