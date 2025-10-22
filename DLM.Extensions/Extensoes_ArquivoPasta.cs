using Conexoes.Macros.Escada;
using DLM;
using DLM.ini;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Conexoes.Utilz;

namespace Conexoes
{
    public static class Extensoes_ArquivoPasta
    {
        [DllImport("User32.dll")]
        private static extern int SetForegroundWindow(IntPtr point);
        public static List<string> ToString(this List<Arquivo> arquivos)
        {
            return arquivos.Select(x => x.Endereco.ToUpper()).Distinct().ToList();
        }
        public static List<string> ToString(this List<Pasta> pastas)
        {
            return pastas.Select(x => x.Endereco).Distinct().ToList();
        }


        public static List<string> GetPastas(this string raiz, string busca = "*", SearchOption opcao = SearchOption.TopDirectoryOnly)
        {
            if (!Directory.Exists(raiz))
            {
                return new List<string>();
            }
            var lista = Directory.GetDirectories(raiz, busca, opcao).ToList();

            var retorno = new List<string>();
            retorno.AddRange(lista);

            return retorno;
        }

        public static void Add(this List<ArquivoCopiar> lista, string destino, string origem, string marca, string posicao)
        {
            lista.Add(new ArquivoCopiar(destino, origem, marca, posicao));
        }

        public static List<Arquivo> GetArquivos(this List<Pasta> Pastas, string Filtro = "*", SearchOption SubPastas = SearchOption.TopDirectoryOnly)
        {
            var Retorno = new List<Arquivo>();
            Pastas = Pastas.GroupBy(x => x.Endereco).Select(x => x.First()).ToList();
            foreach (var pasta in Pastas)
            {
                Retorno.AddRange(GetArquivos(pasta, Filtro, SubPastas));
            }
            Retorno = Retorno.GroupBy(x => x).Select(x => x.First()).ToList();
            return Retorno;
        }

        public static bool LimparArquivosPasta(this Pasta Pasta, string filtro = "*", bool backup = false, string arquivo_backup = null)
        {
            var arquivos = Pasta.GetArquivos(filtro);

            if (backup)
            {
                if (arquivo_backup == null)
                {
                    arquivo_backup = $"{Pasta.Endereco.GetSubPasta(Cfg.Init.PASTA_BACKUPS)}R00.ZIP";
                }
                Utilz.FazerBackup(arquivo_backup, arquivos);
            }

            foreach (var p in arquivos)
            {
                if (!p.Endereco.Delete())
                {
                    return false;
                }
            }
            return true;
        }
        public static string getUpdir(this string Dir)
        {
            if (Dir == null) { return ""; }
            if (Dir.Length == 0) { return ""; }
            if (!Dir.EndsWith(@"\")) { Dir = Dir + @"\"; }
            return System.IO.Path.GetFullPath(System.IO.Path.Combine(Dir, @"..\")).ToUpper();
        }




        public static bool Delete(this string Arquivo_Ou_Pasta, bool msg = true, bool log = false)
        {
            if (!Arquivo_Ou_Pasta.Exists())
            {
                return true;
            }
            if (Arquivo_Ou_Pasta == null)
            {
                return true;
            }

        retentar:
            if (Arquivo_Ou_Pasta.E_Diretorio())
            {
                try
                {
                    Directory.Delete(Arquivo_Ou_Pasta, true);
                }
                catch (Exception ex)
                {
                    var erro = ex.Message + "\n\n\n" + ex.StackTrace;
                    if (msg)
                    {
                        if (Conexoes.Utilz.Pergunta($"Não foi possível excluir o arquivo. \nTentar Novamente?\n{ex.Message}"))
                        {
                            goto retentar;
                        }
                    }
                    if (log)
                    {
                        DLM.log.Log(ex);
                    }
                }
            }

            if (Arquivo_Ou_Pasta.Exists())
            {
                try
                {
                    File.Delete(Arquivo_Ou_Pasta);
                }
                catch (Exception ex)
                {
                    if (msg)
                    {
                        if (Conexoes.Utilz.Pergunta($"Não foi possível excluir o arquivo. \nTentar Novamente?\n{ex.Message}"))
                        {
                            goto retentar;
                        }
                    }
                    if (log)
                    {
                        DLM.log.Log(ex);
                    }

                    return false;
                }
            }
            return !Arquivo_Ou_Pasta.Exists();
        }

        public static List<Arquivo> GetArquivos(this Pasta Raiz, string chave = "*", SearchOption SubPastas = SearchOption.TopDirectoryOnly)
        {
            if (!Raiz.Exists())
            {
                return new List<Arquivo>();
            }
            var retorno = new List<Arquivo>();
            foreach (var arquivo in Directory.GetFiles(Raiz.Endereco, chave, SubPastas))
            {
                retorno.Add(new Arquivo(arquivo, Raiz));
            }
            return retorno;
        }
        public static List<Arquivo> GetArquivos(this Pasta Raiz, bool sub_pastas = false, params string[] filtros)
        {
            return Raiz.Endereco.GetArquivos(sub_pastas, filtros);
        }
        public static List<Arquivo> GetArquivos(this string Raiz, bool sub_pastas = false, params string[] filtros)
        {
            if (!Raiz.Exists())
            {
                return new List<Arquivo>();
            }
            if (filtros == null)
            {
                filtros = new[] { "*" };
            }
            var arquivos = new List<Arquivo>();
            var pasta = Raiz.AsPasta();
            foreach (var filtro in filtros)
            {
                arquivos.AddRange(GetArquivos(pasta, filtro, sub_pastas ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
            }
            arquivos = arquivos.Distinct().ToList();

            return arquivos;
        }

        public static List<Arquivo> GetArquivos(this string Raiz, string filtro = "*", bool sub_pastas = false)
        {
            if (!Raiz.Exists())
            {
                return new List<Arquivo>();
            }
            var pasta = Raiz.AsPasta();
            return GetArquivos(pasta, filtro, sub_pastas ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public static Arquivo AsArquivo(this string Arquivo, Pasta pai = null)
        {
            var nPasta = new Arquivo(Arquivo, pai);
            return nPasta;
        }
        public static Pasta AsPasta(this string Diretorio, Pasta pai = null)
        {
            Diretorio = Diretorio.ToUpper();
            if (Diretorio.EndsWith($@"{Cfg.Init.EXT_Obra}\"))
            {
                var n_Pasta = new ObraTecnoMetal(Diretorio, pai);
                return n_Pasta;
            }
            else if (Diretorio.EndsWith($@"{Cfg.Init.EXT_Pedido}\"))
            {
                var n_Pasta = new PedidoTecnoMetal(Diretorio, (ObraTecnoMetal)pai);
                return n_Pasta;
            }
            else if (Diretorio.EndsWith($@"{Cfg.Init.EXT_Pedido}\"))
            {
                var n_Pasta = new SubEtapaTecnoMetal(Diretorio, (PedidoTecnoMetal)pai);
                return n_Pasta;
            }
            else
            {
                var n_Pasta = new Pasta(Diretorio, pai);
                return n_Pasta;
            }
        }
        public static void Copiar(this List<Conexoes.Arquivo> arquivos, string destino, bool mensagem = false, bool log = false)
        {
            foreach (var arquivo in arquivos)
            {
                arquivo.Endereco.Copiar(destino, mensagem);
            }
        }
        public static bool Mover(this List<Conexoes.Arquivo> arquivos, string destino, bool mensagem = false, bool log = false)
        {
            return arquivos.Select(x => x.Endereco).ToList().Mover(destino, mensagem, log);
        }
        public static bool Mover(this List<string> arquivos, string destino, bool mensagem = false, bool log = false)
        {
            var tudo_ok = true;
            foreach (var arq in arquivos)
            {
                var mov = arq.Mover(destino);
                if (!mov)
                {
                    tudo_ok = false;
                }
            }
            return tudo_ok;
        }
        public static bool Mover(this string arquivo, string destino, bool mensagem = false, bool log = false)
        {
            if (arquivo.Copiar(destino, mensagem, log))
            {
                if (!arquivo.Delete(mensagem))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public static string CreateDirectory(this string pasta)
        {
            if (pasta.Contains(@"\"))
            {
                if (!pasta.EndsWith(@"\"))
                {
                    pasta += @"\";
                }
            }
            else if (pasta.Contains(@"/"))
            {
                if (!pasta.EndsWith(@"/"))
                {
                    pasta += @"/";
                }
            }

            if (Directory.Exists(pasta))
            {
                return pasta;
            }
            try
            {
                if (pasta.Contains(@"\") | pasta.Contains(@"/"))
                {
                    Directory.CreateDirectory(pasta);
                }
            }
            catch (Exception ex)
            {
                ex.Alerta($"Erro ao tentar criar a pasta \n\n{pasta}");
                return "";
            }

            return pasta;
        }

        public static string GetSubPasta(this string Raiz, string Pasta, bool criar = true)
        {
            var novo_dir = Raiz.ToUpper();
            if (novo_dir.Contains($@"\") && !novo_dir.EndsWith($@"\"))
            {
                novo_dir += $@"\";
            }
            else if (novo_dir.Contains($@"/") && !novo_dir.EndsWith($@"/"))
            {
                novo_dir += $@"/";
            }

            novo_dir += Pasta;



            if (novo_dir.Contains($@"\") && !novo_dir.EndsWith($@"\"))
            {
                novo_dir += $@"\";
            }
            else if (novo_dir.Contains($@"/") && !novo_dir.EndsWith($@"/"))
            {
                novo_dir += $@"/";
            }


            if (criar)
            {
                if (!Directory.Exists(novo_dir))
                {
                    try
                    {
                        Directory.CreateDirectory(novo_dir);
                    }
                    catch (Exception ex)
                    {
                        Conexoes.Utilz.Alerta(ex);
                        return "";
                    }
                }
            }


            return novo_dir;
        }

        public static void CriarPastas(this string Raiz, params string[] Pastas)
        {
            foreach (string Pasta in Pastas)
            {
                Raiz.GetSubPasta(Pasta);
            }
        }


        public static bool Exists(this string arquivo)
        {
            if (arquivo == null) { return false; }
            if (arquivo.Length == 0) { return false; }

            if (arquivo.E_Diretorio())
            {
                try
                {
                    return Directory.Exists(arquivo);
                }
                catch (Exception)
                {

                }
            }


            try
            {
                return File.Exists(arquivo);
            }
            catch (Exception)
            {

            }
            return false;
        }
        public static bool E_Diretorio(this string dir)
        {
            if (dir.EndsWith(@"\") | dir.EndsWith(@"/"))
            {
                return true;
            }
            else if (File.Exists(dir))
            {
                return false;
            }
            else if (!Directory.Exists(dir))
            {
                return false;
            }
            else if ((File.GetAttributes(dir) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Copiar(this string arquivo_origem, string Destino_Pasta_Ou_Arquivo, bool mensagem = true, bool log = false)
        {
            if (!arquivo_origem.Exists())
            {
                if(mensagem)
                {
                    $"Arquivo de origem não econtrado. {arquivo_origem}".Alerta();
                }

                return false;
            }
            if (Destino_Pasta_Ou_Arquivo == "")
            {
                if(mensagem)
                {
                    $"Destino em branco.".Alerta();
                }
                return false;
            }

            string arquivo_destino = Destino_Pasta_Ou_Arquivo;
            if (E_Diretorio(arquivo_destino))
            {
                if (arquivo_destino.Contains(@"\") && !arquivo_destino.EndsWith(@"\"))
                {
                    arquivo_destino = arquivo_destino + @"\";
                }
                else if (arquivo_destino.Contains(@"/") && !arquivo_destino.EndsWith(@"/"))
                {
                    arquivo_destino = arquivo_destino + @"/";
                }

                arquivo_destino = $"{arquivo_destino}{arquivo_origem.getNome()}.{arquivo_origem.getExtensao()}";
            }

            string diretorio = arquivo_destino.getPasta();
        inicio:
            if (!diretorio.Exists())
            {
                try
                {
                    Directory.CreateDirectory(diretorio);
                }
                catch (Exception ex)
                {
                    var erro = $"Não foi possível criar o diretório {Destino_Pasta_Ou_Arquivo}.\n" + ex.Message;
                    if (mensagem)
                    {

                        if (Utilz.Pergunta("Tentar novamente?\n\n\n" + erro))
                        {
                            goto inicio;
                        }
                    }

                    if (log)
                    {
                        DLM.log.Log(ex);
                    }

                    return false;
                }
            }







        retentar:

            if (File.Exists(arquivo_destino))
            {
                if (arquivo_origem.ToUpper() == arquivo_destino.ToUpper())
                {
                    return true;
                }
                try
                {
                    File.Delete(arquivo_destino);
                }
                catch (Exception ex)
                {
                    if (mensagem)
                    {
                        if (Utilz.Pergunta($"Falha ao tentar apagar o arquivo existente para substituir pelo novo: {arquivo_destino}\n\nTentar novamente ?\n{ex.Message}\n\n\n\n{ex.StackTrace}"))
                        {
                            goto retentar;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }


            try
            {
                File.Copy(arquivo_origem, arquivo_destino);
                return true;
            }
            catch (Exception ex)
            {
                if (mensagem)
                {
                    if (Utilz.Pergunta("Falha ao tentar copiar o arquivo " + arquivo_origem + " para o destino: " + Destino_Pasta_Ou_Arquivo + "\n\nTentar novamente ?\n\n\n\n\n\n" + ex.Message + "\n\n\n\n" + ex.StackTrace))
                    {
                        goto retentar;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }


        public static bool Abrir(this Conexoes.Arquivo Arquivo, string argumentos = "", bool wait = false)
        {
            if (Arquivo == null) { return false; }
            return new List<Arquivo> { Arquivo }.Abrir(argumentos, wait);
        }

        public static void Abrir(this List<string> Arquivos)
        {
            Arquivos.Select(x => new Conexoes.Arquivo(x)).ToList().Abrir();
        }
        public static bool Abrir(this List<Conexoes.Arquivo> Arquivos, string argumentos = "", bool wait = false)
        {

            var cams = new List<Arquivo>();
            var dwgs = new List<Arquivo>();
            var dxfs = new List<Arquivo>();
            var outros = new List<Arquivo>();

            foreach (var arq in Arquivos)
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
                    Conexoes.Utilz.Alerta("Arquivo executável do TecnoPlot não encontrado.");
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

        public static bool Abrir(this string arquivo_ou_pasta, string argumentos = "", bool wait = false)
        {
            if (arquivo_ou_pasta.Contains("%"))
            {
                arquivo_ou_pasta = Environment.ExpandEnvironmentVariables(arquivo_ou_pasta);
            }
            if (E_Diretorio(arquivo_ou_pasta))
            {
                if (Directory.Exists(arquivo_ou_pasta))
                {
                    Process.Start(arquivo_ou_pasta, argumentos);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return arquivo_ou_pasta.AsArquivo().Abrir(argumentos, wait);

            }
        }

        public static void AbrirAsAdmin(this string appToRun, string arguments = "")
        {
            string domain = "medabil.com.br";
            string username = "med.admin";
            string usernameFull = $@"{domain}\{username}";
            //string password = "K@$p3rsk1@2023!@";
            string password = "B1td3f3nd3r@2025@";
            var workingDirectory = Environment.ExpandEnvironmentVariables(@"%windir%\system32");

            string filePath = Environment.ExpandEnvironmentVariables(@"%windir%\system32\cmd.exe");

            if (appToRun.Contains("%"))
            {
                appToRun = Environment.ExpandEnvironmentVariables(appToRun);
            }

            string fullCommand = $"/C start \"\" \"{appToRun}\" {arguments}";
            if (arguments == "" | arguments ==null)
            {
                fullCommand = $"/C start \"\" \"{appToRun}\"";
            }
            try
            {


                var startInfo = new ProcessStartInfo
                {
                    UserName = username,
                    Password = ConvertToSecureString(password),
                    Domain = domain,
                    WorkingDirectory = workingDirectory,
                    FileName = filePath,
                    Arguments = fullCommand,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    LoadUserProfile = true,
                    CreateNoWindow = false
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                }
            }
            catch (Exception ex1)
            {
                ex1.Log();
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        Password = ConvertToSecureString("M&nfds14"),
                        UserName = "administrador",
                        WorkingDirectory = workingDirectory,
                        FileName = filePath,
                        Arguments = fullCommand,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        LoadUserProfile = true,
                        CreateNoWindow = false
                    };

                    using (var process = new Process { StartInfo = startInfo })
                    {
                        process.Start();
                    }
                }
                catch (Exception ex2)
                {
                    ex2.Log();
                }
            }
        }
        static System.Security.SecureString ConvertToSecureString(string password)
        {
            var securePassword = new System.Security.SecureString();
            foreach (char c in password)
            {
                securePassword.AppendChar(c);
            }
            securePassword.MakeReadOnly();
            return securePassword;
        }

        private static bool Open(string arquivo, string argumentos = "", bool wait = false)
        {
            if (!arquivo.Exists())
            {
                return false;
            }
            if (arquivo.E_Diretorio())
            {
                try
                {
                    if (Directory.Exists(arquivo))
                    {
                        Process.Start(arquivo, argumentos);
                        return true;
                    }

                }
                catch (Exception ex2)
                {

                    Utilz.Alerta(arquivo + "\n\n" + ex2.Message);
                    return false;
                }

            }
            else
            {
                try
                {
                    if (File.Exists(arquivo))
                    {
                        var process = Process.Start(arquivo, argumentos);
                        if (wait)
                        {
                            process.WaitForExit();
                        }
                        return true;
                    }

                }
                catch (Exception ex2)
                {

                    Utilz.Alerta(arquivo + "\n\n" + ex2.Message);
                    return false;
                }
            }


            return false;
        }


        public static bool EMaisRecente(this string arquivo_atual, string arquivo_a_comparar)
        {
            if (!arquivo_atual.Exists()) { return false; }
            if (!arquivo_a_comparar.Exists()) { return false; }

            FileInfo f1 = new FileInfo(arquivo_atual);
            FileInfo f2 = new FileInfo(arquivo_atual);


            return f1.LastWriteTime > f2.LastWriteTime;
        }


    }
}
