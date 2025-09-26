using DLM.cam;
using DLM.encoder;
using DLM.vars;
using System.Collections.Generic;
using System.Linq;

namespace Conexoes
{
    public static class Extensoes_NC1
    {
        public static TAB_CAM_TIPO_SOLDA GetInverso(this TAB_CAM_TIPO_SOLDA tipo)
        {
            switch (tipo)
            {
                case TAB_CAM_TIPO_SOLDA.W001:
                    return TAB_CAM_TIPO_SOLDA.W015;
                case TAB_CAM_TIPO_SOLDA.W002:
                    return TAB_CAM_TIPO_SOLDA.W016;
                case TAB_CAM_TIPO_SOLDA.W003:
                    return TAB_CAM_TIPO_SOLDA.W017;
                case TAB_CAM_TIPO_SOLDA.W004:
                    return TAB_CAM_TIPO_SOLDA.W018;
                case TAB_CAM_TIPO_SOLDA.W005:
                    return TAB_CAM_TIPO_SOLDA.W005;
                case TAB_CAM_TIPO_SOLDA.W006:
                    return TAB_CAM_TIPO_SOLDA.W006;
                case TAB_CAM_TIPO_SOLDA.W007:
                    return TAB_CAM_TIPO_SOLDA.W026;
                case TAB_CAM_TIPO_SOLDA.W008:
                    return TAB_CAM_TIPO_SOLDA.W027;
                case TAB_CAM_TIPO_SOLDA.W013:
                    return TAB_CAM_TIPO_SOLDA.W021;
                case TAB_CAM_TIPO_SOLDA.W014:
                    return TAB_CAM_TIPO_SOLDA.W022;
                case TAB_CAM_TIPO_SOLDA.W015:
                    return TAB_CAM_TIPO_SOLDA.W001;
                case TAB_CAM_TIPO_SOLDA.W016:
                    return TAB_CAM_TIPO_SOLDA.W002;
                case TAB_CAM_TIPO_SOLDA.W017:
                    return TAB_CAM_TIPO_SOLDA.W003;
                case TAB_CAM_TIPO_SOLDA.W018:
                    return TAB_CAM_TIPO_SOLDA.W004;
                case TAB_CAM_TIPO_SOLDA.W021:
                    return TAB_CAM_TIPO_SOLDA.W013;
                case TAB_CAM_TIPO_SOLDA.W022:
                    return TAB_CAM_TIPO_SOLDA.W014;
                case TAB_CAM_TIPO_SOLDA.W026:
                    return TAB_CAM_TIPO_SOLDA.W007;
                case TAB_CAM_TIPO_SOLDA.W027:
                    return TAB_CAM_TIPO_SOLDA.W008;
                case TAB_CAM_TIPO_SOLDA.W028:
                    return TAB_CAM_TIPO_SOLDA.W029;
                case TAB_CAM_TIPO_SOLDA.W029:
                    return TAB_CAM_TIPO_SOLDA.W028;




                case TAB_CAM_TIPO_SOLDA.W009:
                case TAB_CAM_TIPO_SOLDA.W010:
                case TAB_CAM_TIPO_SOLDA.W011:
                case TAB_CAM_TIPO_SOLDA.W012:
                case TAB_CAM_TIPO_SOLDA.W019:
                case TAB_CAM_TIPO_SOLDA.W020:
                case TAB_CAM_TIPO_SOLDA.W023:
                case TAB_CAM_TIPO_SOLDA.W024:
                case TAB_CAM_TIPO_SOLDA.W025:
                case TAB_CAM_TIPO_SOLDA.W100:
                case TAB_CAM_TIPO_SOLDA.W101:
                case TAB_CAM_TIPO_SOLDA._:
                default:
                    return TAB_CAM_TIPO_SOLDA._;
            }

        }
        public static List<Furo> AsFuro(this List<NC1_Obj> lista, bool inverter_y = true, double offset_y = 0)
        {
            return lista.Select(x => new Furo(x.X, (x.Y * (inverter_y ? -1 : 1)) - offset_y, x.Diam, x.Slot, x.Slot_Angle)).ToList();
        }
        public static TAB_NC1_CODE_FAMILY GetFamilia(this TAB_NC1_CODE code)
        {
            switch (code)
            {
                case TAB_NC1_CODE.E0:
                case TAB_NC1_CODE.E1:
                case TAB_NC1_CODE.E2:
                case TAB_NC1_CODE.E3:
                case TAB_NC1_CODE.E4:
                case TAB_NC1_CODE.E5:
                case TAB_NC1_CODE.E6:
                case TAB_NC1_CODE.E7:
                case TAB_NC1_CODE.E8:
                case TAB_NC1_CODE.E9:
                case TAB_NC1_CODE.ST:
                case TAB_NC1_CODE.EN:
                    return TAB_NC1_CODE_FAMILY.Ini_Fim;

                case TAB_NC1_CODE.S0:
                case TAB_NC1_CODE.S1:
                case TAB_NC1_CODE.S2:
                case TAB_NC1_CODE.S3:
                case TAB_NC1_CODE.S4:
                case TAB_NC1_CODE.S5:
                case TAB_NC1_CODE.S6:
                case TAB_NC1_CODE.S7:
                case TAB_NC1_CODE.S8:
                case TAB_NC1_CODE.S9:
                case TAB_NC1_CODE.SI:
                    return TAB_NC1_CODE_FAMILY.Indicacao;

                case TAB_NC1_CODE.B0:
                case TAB_NC1_CODE.B1:
                case TAB_NC1_CODE.B2:
                case TAB_NC1_CODE.B3:
                case TAB_NC1_CODE.B4:
                case TAB_NC1_CODE.B5:
                case TAB_NC1_CODE.B6:
                case TAB_NC1_CODE.B7:
                case TAB_NC1_CODE.B8:
                case TAB_NC1_CODE.B9:
                case TAB_NC1_CODE.BO:
                    return TAB_NC1_CODE_FAMILY.Furo;

                case TAB_NC1_CODE.A0:
                case TAB_NC1_CODE.A1:
                case TAB_NC1_CODE.A2:
                case TAB_NC1_CODE.A3:
                case TAB_NC1_CODE.A4:
                case TAB_NC1_CODE.A5:
                case TAB_NC1_CODE.A6:
                case TAB_NC1_CODE.A7:
                case TAB_NC1_CODE.A8:
                case TAB_NC1_CODE.A9:
                case TAB_NC1_CODE.AK:
                    return TAB_NC1_CODE_FAMILY.Contorno_EXT;

                case TAB_NC1_CODE.I0:
                case TAB_NC1_CODE.I1:
                case TAB_NC1_CODE.I2:
                case TAB_NC1_CODE.I3:
                case TAB_NC1_CODE.I4:
                case TAB_NC1_CODE.I5:
                case TAB_NC1_CODE.I6:
                case TAB_NC1_CODE.I7:
                case TAB_NC1_CODE.I8:
                case TAB_NC1_CODE.I9:
                case TAB_NC1_CODE.IK:
                    return TAB_NC1_CODE_FAMILY.Contorno_INT;

                case TAB_NC1_CODE.PU:
                case TAB_NC1_CODE.KO:
                case TAB_NC1_CODE.SC:
                case TAB_NC1_CODE.TO:
                case TAB_NC1_CODE.UE:
                case TAB_NC1_CODE.PR:
                case TAB_NC1_CODE.KA:
                case TAB_NC1_CODE.P0:
                case TAB_NC1_CODE.P1:
                case TAB_NC1_CODE.P2:
                case TAB_NC1_CODE.P3:
                case TAB_NC1_CODE.P4:
                case TAB_NC1_CODE.P5:
                case TAB_NC1_CODE.P6:
                case TAB_NC1_CODE.P7:
                case TAB_NC1_CODE.P8:
                case TAB_NC1_CODE.P9:
                case TAB_NC1_CODE.K0:
                case TAB_NC1_CODE.K1:
                case TAB_NC1_CODE.K2:
                case TAB_NC1_CODE.K3:
                case TAB_NC1_CODE.K4:
                case TAB_NC1_CODE.K5:
                case TAB_NC1_CODE.K6:
                case TAB_NC1_CODE.K7:
                case TAB_NC1_CODE.K8:
                case TAB_NC1_CODE.K9:
                case TAB_NC1_CODE._:
                    return TAB_NC1_CODE_FAMILY.Outro;
            }

            return TAB_NC1_CODE_FAMILY._;

        }
    }

    public static class Extensoes_ReadCAM
    {
        public static void Abrir(this Cam cam)
        {
            new List<Arquivo> { new Arquivo(cam.Arquivo) }.Abrir();
        }
        public static void Abrir(this List<DLM.cam.ReadCAM> Arquivos)
        {
            Arquivos.Select(x => new Conexoes.Arquivo(x.Arquivo)).ToList().Abrir();
        }
        public static List<Report> VerificarFuros(this ReadCAM readCAM)
        {
            var Reports = new List<Report>();
            Reports.AddRange(readCAM.Formato.LIV1.Furacoes.Verificar().Select(x => new Report("Furação", $"[LIV1] => {x.Descricao}", TipoReport.Alerta)));
            Reports.AddRange(readCAM.Formato.LIV2.Furacoes.Verificar().Select(x => new Report("Furação", $"[LIV2] => {x.Descricao}", TipoReport.Alerta)));
            Reports.AddRange(readCAM.Formato.LIV3.Furacoes.Verificar().Select(x => new Report("Furação", $"[LIV3] => {x.Descricao}", TipoReport.Alerta)));
            Reports.AddRange(readCAM.Formato.LIV4.Furacoes.Verificar().Select(x => new Report("Furação", $"[LIV4] => {x.Descricao}", TipoReport.Alerta)));


            if (Reports.Count > 0)
            {
                return new List<Report> { new Report("Furação", $"[{readCAM.Nome}] -> Furos batendo, sobrepostos ou com pouca borda:\n{string.Join("\n", Reports.Select(x => x.Descricao))}\n\n\n", TipoReport.Alerta) };
            }

            return Reports;
        }
    }

}
