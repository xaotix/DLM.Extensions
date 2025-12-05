using Conexoes;
using DLM.ini;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Conexoes
{
    public static class Extensoes_Abrir
    {
        [DllImport("User32.dll")]
        private static extern int SetForegroundWindow(IntPtr point);


        public static bool Abrir(this List<Conexoes.Arquivo> arquivos, string argumentos = "", bool wait = false)
        {
            arquivos = arquivos.FindAll(x => x != null);

            var cams = new List<Arquivo>();
            var dwgs = new List<Arquivo>();
            var dxfs = new List<Arquivo>();
            var outros = new List<Arquivo>();

            foreach (var arq in arquivos)
            {
                if (arq.Extensao == Cfg.Init.EXT_DWG)
                {
                    dwgs.Add(arq);
                }
                else if (arq.Extensao == Cfg.Init.EXT_CAM)
                {
                    cams.Add(arq);
                }
                else if (arq.Extensao == Cfg.Init.EXT_DXF)
                {
                    dxfs.Add(arq);
                }
                else
                {
                    outros.Add(arq);
                }
            }


            if (cams.Count > 0)
            {
                var exe = Cfg_User.Init.EXE_TecnoPlot;
                if (!exe.Exists())
                {
                    exe = Cfg_User.Init.EXE_TecnoPlot2;
                }
                if (!exe.Exists())
                {
                    "arquivo_ou_pasta executável do TecnoPlot não encontrado.".Alerta();
                    return false;
                }
                DLM.vars.TecnoMetalVars.SalvarTecnoPlot(cams);


                var process = Process.Start(exe);

                process.WaitForInputIdle();
                if (process != null)
                {
                    System.Threading.Thread.Sleep(500);

                    IntPtr h = process.MainWindowHandle;
                    SetForegroundWindow(h);
                    System.Windows.Forms.SendKeys.SendWait("{PGUP}");
                    System.Windows.Forms.SendKeys.SendWait("{+}");
                    System.Windows.Forms.SendKeys.SendWait("{+}");
                }
            }
            if (dwgs.Count > 0 && Buff.Pedido != null && Buff.Obra != null)
            {
                string cfg = $"5|0|{Cfg.Init.DIR_RAIZ_OBRAS.Replace(@"\", "")}|1|{Conexoes.Buff.Obra.Nome}|2|{Conexoes.Buff.Pedido.Nome}|5|{dwgs[0]}.DWG|";
                INI.Set(Cfg_User.Init.TecIniWindows, "Tecno2005", "WorksManagerCurrentStatus", cfg);
            }


            foreach (var arq in dwgs)
            {
                var script = TecnoMetalVars.GetScriptTecnoMetal();
                TecnoMetalVars.MatarExecutavelChato();
                if (arq.Endereco.StartsW(Cfg.Init.DIR_RAIZ_OBRAS))
                {
                    Open(Cfg_User.Init.AcadApp, $"{script} {Utilz._Aspas}{arq.Endereco}{Utilz._Aspas}", wait);
                }
                else
                {
                    Open(Cfg_User.Init.AcadApp, $"{Utilz._Aspas}{arq.Endereco}{Utilz._Aspas}", wait);
                }
                TecnoMetalVars.MatarExecutavelChato();
            }
            foreach (var arq in dxfs)
            {
                var script = TecnoMetalVars.GetScriptTecnoMetal();
                TecnoMetalVars.MatarExecutavelChato();
                Open(Cfg_User.Init.AcadApp, $"{Utilz._Aspas}{arq.Endereco}{Utilz._Aspas}", wait);
                TecnoMetalVars.MatarExecutavelChato();
            }

            foreach (var arq in outros)
            {
                Open(arq.Endereco, "", wait);
            }

            return true;
        }
        public static bool Abrir(this Conexoes.Arquivo arquivo, string argumentos = "", bool wait = false)
        {
            return new List<Arquivo> { arquivo }.Abrir(argumentos, wait);
        }

        public static bool Abrir(this List<string> arquivos_ou_pastas, string argumentos = "", bool wait = false)
        {
            foreach (var item in arquivos_ou_pastas)
            {
                var st = item.Abrir(argumentos, wait);
                if (!st)
                {
                    return false;
                }
            }

            return true;
        }
        public static bool Abrir(this string arquivo_ou_pasta, string argumentos = "", bool wait = false)
        {
            if (arquivo_ou_pasta.E_Diretorio())
            {
                return Open(arquivo_ou_pasta, argumentos, wait);
            }
            else
            {
                return arquivo_ou_pasta.AsArquivo().Abrir(argumentos, wait);
            }
        }

        private static bool Open(string arquivo_ou_pasta, string argumentos = "", bool wait = false)
        {
            if (!arquivo_ou_pasta.Exists())
            {
                return false;
            }

            if (arquivo_ou_pasta.Contem("%"))
            {
                arquivo_ou_pasta = Environment.ExpandEnvironmentVariables(arquivo_ou_pasta);
            }

            try
            {
                var process = Process.Start(arquivo_ou_pasta, argumentos);
                if (wait)
                {
                    process.WaitForExit();
                }
                return true;
            }
            catch (Exception ex)
            {
                ex.Alerta(arquivo_ou_pasta, $"Open -> Argumentos: {argumentos}");
                return false;
            }
        }
    }
}
