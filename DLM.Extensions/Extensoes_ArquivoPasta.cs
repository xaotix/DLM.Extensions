using DLM;
using DLM.ini;
using DLM.sap.Avanco;
using DLM.vars;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Conexoes
{

    public static class Extensoes_ArquivoPasta
    {


        public static List<string> ToString(this List<Arquivo> arquivos)
        {
            return arquivos.Select(x => x.Endereco.ToUpper()).Distinct().ToList();
        }
        public static List<string> ToString(this List<Pasta> pastas)
        {
            return pastas.Select(x => x.Endereco).Distinct().ToList();
        }

        public static bool CopiarPara(this Arquivo arquivo, string pasta_ou_arquivo, bool msg = false, bool log = true)
        {
            return arquivo.Endereco.Copiar(pasta_ou_arquivo, msg, log);
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

        public static List<Arquivo> GetArquivos(this List<Pasta> pastas, string Filtro = "*", SearchOption subpastas = SearchOption.TopDirectoryOnly)
        {
            var retorno = new List<Arquivo>();
            pastas = pastas.GroupBy(x => x.Endereco).Select(x => x.First()).ToList();
            foreach (var pasta in pastas)
            {
                retorno.AddRange(GetArquivos(pasta, Filtro, subpastas));
            }
            retorno = retorno.GroupBy(x => x).Select(x => x.First()).ToList();
            return retorno;
        }

        public static bool LimparArquivosPasta(this Pasta Pasta, string filtro = "*", bool backup = false, string arquivo_backup = null)
        {
            var arquivos = Pasta.GetArquivos(filtro);

            if (backup)
            {
                if (arquivo_backup == null)
                {
                    arquivo_backup = $@"{Pasta.Endereco.GetSubPasta(Cfg.Init.PASTA_BACKUPS)}\{Pasta.Nome}_R00.ZIP";
                }
                Utilz.FazerBackup(arquivo_backup, arquivos);
            }

            foreach (var arq in arquivos)
            {
                if (!arq.Endereco.Delete())
                {
                    return false;
                }
            }
            return true;
        }


        public static bool Delete(this string arq_ou_pasta, bool msg = true, bool log = false)
        {
            if (!arq_ou_pasta.Exists())
            {
                return true;
            }
            if (arq_ou_pasta == null)
            {
                return true;
            }

        retentar:
            if (arq_ou_pasta.E_Diretorio())
            {
                try
                {
                    Directory.Delete(arq_ou_pasta, true);
                }
                catch (Exception ex)
                {
                    if (msg)
                    {
                        if ($"Não foi possível excluir o arquivo {arq_ou_pasta}. \nTentar Novamente?\n\n{ex.Message}".Pergunta())
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

            if (arq_ou_pasta.Exists())
            {
                try
                {
                    File.Delete(arq_ou_pasta);
                }
                catch (Exception ex)
                {
                    if (msg)
                    {
                        if ($"Não foi possível excluir o arquivo. \nTentar Novamente?\n{ex.Message}".Pergunta())
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
            return !arq_ou_pasta.Exists();
        }

        public static List<Arquivo> GetArquivos(this Pasta raiz, string chave = "*", SearchOption sub_folders = SearchOption.TopDirectoryOnly)
        {
            if (!raiz.Exists())
            {
                return new List<Arquivo>();
            }
            var retorno = new List<Arquivo>();
            var arquivos = Directory.GetFiles(raiz.Endereco, chave, sub_folders);

            var Tarefas = new List<Task>();
            var arquivos_map = new ConcurrentBag<Arquivo>();
            foreach (var arq in arquivos)
            {
                Tarefas.Add(Task.Factory.StartNew(() =>
                {
                    if (arq.Exists())
                    {
                        var nArq = new Arquivo(arq,sub_folders == SearchOption.TopDirectoryOnly? raiz:null);
                        arquivos_map.Add(nArq);
                    }
                }));
            }

            Task.WaitAll(Tarefas.ToArray());
            retorno.AddRange(arquivos_map);
            retorno = retorno.OrderBy(x => x.Nome).ToList();
            return retorno;
        }
        public static List<Arquivo> GetArquivos(this Pasta raiz, bool sub_pastas = false, params string[] filtros)
        {

            if (!raiz.Exists())
            {
                return new List<Arquivo>();
            }
            if (filtros == null)
            {
                filtros = new[] { "*" };
            }
            var arquivos = new List<Arquivo>();
            foreach (var filtro in filtros)
            {
                arquivos.AddRange(GetArquivos(raiz, filtro, sub_pastas ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
            }
            arquivos = arquivos.Distinct().ToList();

            return arquivos;
        }


        public static List<Arquivo> GetArquivos(this string raiz, bool sub_pastas = false, params string[] filtros)
        {
            return raiz.AsPasta().GetArquivos(sub_pastas, filtros);
        }
        public static List<Arquivo> GetArquivos(this string raiz, string filtro = "*", bool sub_pastas = false)
        {
            return raiz.AsPasta().GetArquivos(filtro, sub_pastas ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public static Arquivo AsArquivo(this string arquivo, Pasta pai = null)
        {
            var nPasta = new Arquivo(arquivo, pai);
            return nPasta;
        }
        public static Pasta AsPasta(this string dir, Pasta pai = null)
        {
            dir = dir.ToUpper();
            if (dir.Replace(@"\", "").Replace(@"/", "").EndsW($@".{Cfg.Init.EXT_Obra}"))
            {
                return new ObraTecnoMetal(dir, pai);
            }
            else if (dir.Replace(@"\", "").Replace(@"/", "").EndsW($@".{Cfg.Init.EXT_Pedido}"))
            {
                return new PedidoTecnoMetal(dir, (ObraTecnoMetal)pai);
            }
            else if (dir.Replace(@"\", "").Replace(@"/", "").EndsW($@".{Cfg.Init.EXT_Etapa}"))
            {
                if (pai is not PedidoTecnoMetal && pai != null)
                {
                    $"Pasta inválida: Pasta de etapa dentro de outra pasta de etapa:\n{dir}".Alerta();
                    return new Pasta(dir, pai);
                }
                else if (pai != null)
                {
                    return new SubEtapaTecnoMetal(dir, (PedidoTecnoMetal)pai);
                }
                else
                {
                    return new Pasta(dir);
                }
            }
            else
            {
                return new Pasta(dir, pai);
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
            if (pasta.Contem(@"\"))
            {
                if (!pasta.EndsW(@"\"))
                {
                    pasta += @"\";
                }
            }
            else if (pasta.Contem(@"/"))
            {
                if (!pasta.EndsW(@"/"))
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
                if (pasta.Contem(@"\", @"/"))
                {
                    Directory.CreateDirectory(pasta);
                }
            }
            catch (Exception ex)
            {
                ex.Alerta($"Erro ao tentar criar a pasta_ou_arquivo \n\n{pasta}");
                return "";
            }

            return pasta;
        }

        public static Pasta GetSubPasta(this Pasta root, string folder, bool create = true)
        {
            return new Pasta(root.Endereco.GetSubPasta(folder, create), root);
        }
        public static string GetSubPasta(this string root, string folder, bool create = true)
        {
            var novo_dir = root.ToUpper();
            if (novo_dir.Contem($@"\") && !novo_dir.EndsW($@"\"))
            {
                novo_dir += $@"\";
            }
            else if (novo_dir.Contem($@"/") && !novo_dir.EndsW($@"/"))
            {
                novo_dir += $@"/";
            }

            novo_dir += folder;



            if (novo_dir.Contem($@"\") && !novo_dir.EndsW($@"\"))
            {
                novo_dir += $@"\";
            }
            else if (novo_dir.Contem($@"/") && !novo_dir.EndsW($@"/"))
            {
                novo_dir += $@"/";
            }


            if (create)
            {
                if (!Directory.Exists(novo_dir))
                {
                    try
                    {
                        Directory.CreateDirectory(novo_dir);
                    }
                    catch (Exception ex)
                    {
                        ex.Alerta(novo_dir);
                        return "";
                    }
                }
            }


            return novo_dir;
        }

        public static void CriarPastas(this string root, params string[] pastas)
        {
            foreach (string pasta in pastas)
            {
                root.GetSubPasta(pasta);
            }
        }


        public static bool Exists(this string file)
        {
            if (file == null) { return false; }
            if (file.LenghtStr() == 0) { return false; }

            if (file.E_Diretorio())
            {
                try
                {
                    return Directory.Exists(file);
                }
                catch (Exception)
                {

                }
            }


            try
            {
                return File.Exists(file);
            }
            catch (Exception)
            {

            }
            return false;
        }
        public static bool E_Diretorio(this string dir)
        {
            if (dir.EndsW(@"\") | dir.EndsW(@"/"))
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
                if (mensagem)
                {
                    $"arquivo de origem não econtrado. {arquivo_origem}".Alerta();
                }

                return false;
            }
            if (Destino_Pasta_Ou_Arquivo == "")
            {
                if (mensagem)
                {
                    $"Destino em branco.".Alerta();
                }
                return false;
            }

            string arquivo_destino = Destino_Pasta_Ou_Arquivo;
            if (arquivo_destino.E_Diretorio())
            {
                if (arquivo_destino.Contem(@"\") && !arquivo_destino.EndsW(@"\"))
                {
                    arquivo_destino = arquivo_destino + @"\";
                }
                else if (arquivo_destino.Contem(@"/") && !arquivo_destino.EndsW(@"/"))
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

                        if ($"Tentar novamente?\n\n\n{ex.GetTexto($"Não foi possível criar o diretório {Destino_Pasta_Ou_Arquivo}.\n\n")}".Pergunta())
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
                        if ($"Falha ao tentar apagar o arquivo existente para substituir pelo novo: {arquivo_destino}\n\nTentar novamente ?\n{ex.Message}\n\n\n\n{ex.StackTrace}".Pergunta())
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
                    if ($"Falha ao tentar copiar o arquivo {arquivo_origem} para o destino: {Destino_Pasta_Ou_Arquivo}\n\nTentar novamente?\n\n{ex.Message}\n\n\n\n{ex.StackTrace}".Pergunta())
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









        public static bool EMaisRecente(this string arquivo_atual, string arquivo_a_comparar)
        {
            if (!arquivo_atual.Exists()) { return false; }
            if (!arquivo_a_comparar.Exists()) { return false; }

            FileInfo f1 = new FileInfo(arquivo_atual);
            FileInfo f2 = new FileInfo(arquivo_atual);


            return f1.LastWriteTime > f2.LastWriteTime;
        }

        public static string getPasta(this string arq)
        {
            if (arq == null) { return ""; }
            if (arq.Length == 0) { return ""; }
            try
            {
                var retorno = System.IO.Path.GetDirectoryName(arq);

                if (!retorno.EndsW(@"\") && retorno.Contem(@"\"))
                {
                    retorno += @"\";
                }
                else if (!retorno.EndsW(@"/") && retorno.Contem(@"/"))
                {
                    retorno += @"/";
                }
                return retorno;
            }
            catch (Exception)
            {
            }
            return "";
        }
        public static string getNomePasta(this string arq)
        {
            var pasta = arq.getPasta();
            if (pasta.Length > 0)
            {
                var items = pasta.Replace(@"\", "|").Replace(@"/", "|").Replace("||", "|").Split('|').ToList().FindAll(x => x != "");
                if (items.Count > 0)
                {
                    return items.Last();
                }
            }

            return "";
        }
        public static string getEdicaoComHoras(this string arq)
        {
            var dt = Cfg.Init.DataDummy;
            if (arq == null) { return ""; }
            if (arq.Length == 0) { return ""; }
            if (File.Exists(arq))
            {
                dt = File.GetLastWriteTime(arq);
            }
            return dt.String("dd/MM/yyyy HH:mm:ss");
        }
        public static string getEdicao(this string arq)
        {
            if (arq == null) { return ""; }
            if (arq.Length == 0) { return ""; }

            if (arq.Exists())
            {
                return System.IO.File.GetLastWriteTime(arq).String(Cfg.Init.DATE_FORMAT);
            }
            return "";
        }
        public static string getExtensao(this string arq, bool upper = true)
        {
            if (arq == null) { return ""; }
            if (arq.Length == 0) { return ""; }
            var ext = System.IO.Path.GetExtension(arq).TrimStart('.');

            return upper ? ext.ToUpper() : ext;
        }
        public static string getNome(this string arq, bool extensao = false)
        {
            if (arq == null) { return ""; }
            if (arq.Length == 0) { return ""; }
            try
            {
                if (arq.E_Diretorio())
                {
                    var ret = arq;
                    ret = ret.TrimEnd(@"/".ToCharArray());
                    ret = ret.TrimEnd(@"\".ToCharArray());
                    ret = ret.Replace(@"\", "|").Replace(@"/", "|");
                    ret = ret.Split('|').ToList().Last();
                    return ret;
                }
                else
                {
                    var nome_arq = System.IO.Path.GetFileNameWithoutExtension(arq);
                    if (nome_arq == "")
                    {
                        return System.IO.Path.GetDirectoryName(arq);
                    }
                    if (extensao)
                    {
                        var ext = arq.getExtensao(false);
                        return $"{nome_arq}.{ext}";
                    }

                    return nome_arq;
                }

            }
            catch (Exception)
            {

            }
            return arq;
        }
        public static string getUpdir(this string dir)
        {
            if (dir.LenghtStr() == 0) { return ""; }
            if (!dir.EndsW(@"\")) { dir = dir + @"\"; }
            return System.IO.Path.GetFullPath(System.IO.Path.Combine(dir, @"..\")).ToUpper();
        }

    }
}
