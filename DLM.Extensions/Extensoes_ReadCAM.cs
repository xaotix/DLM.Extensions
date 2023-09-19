using DLM.cam;
using DLM.encoder;
using DLM.vars;
using System.Collections.Generic;
using System.Linq;

namespace Conexoes
{
    public static class Extensoes_ReadCAM
    {
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
