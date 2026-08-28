using Conexoes.Janelas;
using DLM;
using DLM.db;
using DLM.vars;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Markup;

namespace Conexoes
{
    public static class Extensoes_Enum
    {
        public static string GetDisplayName(this Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            if (member == null)
                return value.ToString();
            var attribute = member.GetCustomAttribute<DisplayAttribute>();
            return attribute?.Name ?? value.ToString();
        }
        public static List<T> ToList<T>() where T : struct, Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }
    }
    public static class Extensoes_HTML
    {
        public static string ToStringNull(this object item)
        {
            if (item.IsNullOrEmpty())
            {
                return null;
            }
            else if (item is Celula)
            {
                var valor = ((Celula)item).Valor;

                return valor.NotNullOrEmpty() ? valor : null;
            }
            return item.ToString();
        }
        public static string RemoverAtributoHTML(this string html, string atributo, string substituto = "")
        {
            // Regex para encontrar o atributo dentro de qualquer tag
            string pattern = $@"\s{atributo}\s*=\s*""[^""]*""";

            // Remove todas as ocorrências do atributo
            string resultado = Regex.Replace(html, pattern, substituto, RegexOptions.IgnoreCase);

            return resultado;
        }
        public static string RemoverTagHtml(this string html, string tag, string substituto = "")
        {
            // Regex para remover a tag de abertura e fechamento
            string pattern = $@"</?{tag}\b[^>]*>";
            string resultado = Regex.Replace(html, pattern, substituto, RegexOptions.IgnoreCase);

            return resultado;
        }
    }
    public static class Extensoes_String
    {
        public static (string Valor, Celula_Tipo_Valor Tipo) GetValorETipo(this object valorOrig, Celula_Tipo_Valor tipoSugerido = Celula_Tipo_Valor.Desconhecido)
        {
            if(valorOrig is RSStr)
            {
                valorOrig = ((RSStr)valorOrig).Valor;
                tipoSugerido = Celula_Tipo_Valor.Moeda;
            }
            else if (valorOrig is PesoStrKg)
            {
                valorOrig = ((PesoStrKg)valorOrig).Valor;
                tipoSugerido = Celula_Tipo_Valor.Decimal;
            }
            else if (valorOrig is PesoStrTon)
            {
                valorOrig = ((PesoStrTon)valorOrig).Valor;
                tipoSugerido = Celula_Tipo_Valor.Decimal;
            }

            // 1. Se o tipo já foi especificado explicitamente, respeita e apenas converte para string
            if (tipoSugerido != Celula_Tipo_Valor.Desconhecido)
            {
                return (valorOrig?.ToString() ?? string.Empty, tipoSugerido);
            }

            // 2. Tratamento de nulos
            if (valorOrig == null || valorOrig is DBNull)
            {
                return (string.Empty, Celula_Tipo_Valor.NULL);
            }

            // 3. Pattern Matching para inferência de tipo e formatação invariável
            return valorOrig switch
            {
                string s => (s, Celula_Tipo_Valor.Texto),

                sbyte or byte or short or ushort or int or uint or long or ulong
                    => (Convert.ToString(valorOrig, CultureInfo.InvariantCulture), Celula_Tipo_Valor.Inteiro),

                float or double or decimal
                    => (Convert.ToString(valorOrig, CultureInfo.InvariantCulture), Celula_Tipo_Valor.Decimal),

                bool b => (b ? "True" : "False", Celula_Tipo_Valor.Booleano),

                DateTime dt => (dt.TimeOfDay == TimeSpan.Zero ? dt.ToString("yyyy-MM-dd") : dt.ToString("yyyy-MM-dd HH:mm:ss"), Celula_Tipo_Valor.Data),

                TimeSpan ts => (ts.ToString(), Celula_Tipo_Valor.Hora),

                byte[] bytes => (Convert.ToBase64String(bytes), Celula_Tipo_Valor.Binario),

                _ => (valorOrig.ToString(), Celula_Tipo_Valor.Texto)
            };
        }


        // Dicionário com os mapeamentos dos ícones padrão SAP para Emojis
        private static readonly Dictionary<string, string> IconMap = new(StringComparer.OrdinalIgnoreCase)
        {
            // Ícones existentes...
            { "@A0@", "🚚" }, // ICON_DELIVERY / ICON_TRUCK (Caminhão / Entrega)
            { "@B0@", "🚛" }, // ICON_TRANSPORT / ICON_FREIGHT (Transporte pesado / Frete)
            { "@6C@", "📦" }, // ICON_PACKAGE / ICON_BOX (Pacote / Caixa)
            { "@A1@", "🏬" }, // ICON_WAREHOUSE / ICON_PLANT (Centro de Distribuição / Depósito)
            { "@AC@", "🏢" }, // ICON_STORE_LOCATION (Local de Armazenamento / Depósito)
            { "@4B@", "✈️" }, // ICON_AIRPLANE (Transporte Aéreo)
            { "@4C@", "🚢" }, // ICON_SHIP (Transporte Marítimo)
            { "@4D@", "🚂" }, // ICON_TRAIN (Transporte Ferroviário)
            { "@6S@", "📑" }, // ICON_DELIVERY_NOTE (Nota de Entrega / Romaneio)
            { "@8M@", "🌐" }, // ICON_FOREIGN_TRADE (Comércio Exterior / Importação-Exportação)
            { "@C0@", "🏭" }, // ICON_STORE (Depósito / Estoque)
            { "@01@", "🆗" },  // ICON_OKAY
            { "@02@", "❌" },  // ICON_CANCEL
            { "@03@", "⬅️" },   // ICON_BACK
            { "@04@", "🚪" },  // ICON_EXIT
            { "@05@", "🖨️" },  // ICON_PRINT
            { "@06@", "🔍" },  // ICON_SEARCH
            { "@07@", "🔄" },  // ICON_REFRESH
            { "@08@", "✅" },  // ICON_CHECK
            { "@09@", "✏️" },   // ICON_CHANGE
            { "@0A@", "🔴" },  // ICON_RED_LIGHT
            { "@0B@", "🟡" },  // ICON_YELLOW_LIGHT
            { "@0C@", "🟢" },  // ICON_GREEN_LIGHT
            { "@0D@", "📄" },  // ICON_CREATE
            { "@0E@", "🗑️" },  // ICON_DELETE
            { "@0F@", "👁️" },  // ICON_DISPLAY
            { "@11@", "💾" },  // ICON_SAVE
            { "@12@", "📂" },  // ICON_OPEN
            { "@14@", "📋" },  // ICON_PASTE
            { "@15@", "✂️" },   // ICON_CUT
            { "@1A@", "⭐" },  // ICON_FAVORITES
            { "@2L@", "📧" },  // ICON_MAIL
            { "@3B@", "📊" },  // ICON_GRAPHICS
            { "@3V@", "⚙️" },  // ICON_SETTINGS
            { "@3W@", "🔧" },  // ICON_TOOLS
            { "@5B@", "📌" },  // ICON_PIN
        
            // Ícone que faltava:
            { "@5D@", "👤" }, // ICON_EMPLOYEE (Homem / Empregado) - ou pode usar 👨‍💼
        
            // Outros ícones úteis da família de usuários/pessoas:
            { "@5C@", "👥" }, // ICON_CUSTOMER (Cliente / Grupo)
            { "@AD@", "👨‍💻" }, // ICON_USER (Usuário)
            { "@7W@", "🔒" }, // ICON_LOCKED
            { "@7X@", "🔓" },  // ICON_UNLOCKED
            { "@2N@", "ℹ️" },
            { "@5Y@", "✅" },  // ICON_GREEN_LIGHT
        };

        // Regex para capturar o padrão do código de ícone SAP (ex: @01@, @0A@, @3V@)
        private static readonly Regex SapIconRegex = new(@"@[A-Za-z0-9]{2}@", RegexOptions.Compiled);

        /// <summary>
        /// Procura e substitui todos os códigos de ícones SAP presentes no texto pelos seus emojis correspondentes.
        /// </summary>
        public static string ReplaceSapIcons(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // O Regex.Replace encontra os códigos no texto e busca a correspondência no dicionário
            return SapIconRegex.Replace(text, match =>
            {
                return IconMap.TryGetValue(match.Value, out var emoji) ? emoji : match.Value;
            });
        }
        /// <summary>
        /// valida se o pep segue o padrão
        ///00-000000.P00.000.XXY
        ///20-123456.P00.001.30A
        ///
        ///0 = numeros de 0 a 9
        ///X = letra ou numero
        ///Y = se é A, ou B ou C
        /// </summary>
        /// <param name="texto"></param>
        /// <returns></returns>
        public static string ValidarPEP(this string texto)
        {
            if (texto.IsNullOrEmpty() || texto.Length < 21)
            {
                return "Erro: O PEP deve ter pelo menos 21 caracteres.";
            }
            if (texto == Cfg.Init.DefaultPEP)
            {
                return "Defina um PEP válido.";
            }

            // Extrai apenas os primeiros 21 caracteres para validação do padrão
            string primeiros21 = texto.Substring(0, 21);

            // 2. Construção da Expressão Regular (Regex)
            // ^[0-9]{2}      -> Dois números (00)
            // -              -> Um hífen (-)
            // [0-9]{6}      -> Seis números (000000)
            // \.P            -> Um ponto seguido da letra P (.P)
            // [0-9]{2}      -> Dois números (00)
            // \.             -> Um ponto (.)
            // [0-9]{3}      -> Três números (000)
            // \.             -> Um ponto (.)
            // [A-Za-z0-9]{2} -> Dois caracteres alfanuméricos (XX - letra ou número)
            // [ABCabc]       -> Uma letra que deve ser A, B ou C (Y)
            string padraoRegex = @"^[0-9]{2}-[0-9]{6}\.P[0-9]{2}\.[0-9]{3}\.[A-Za-z0-9]{2}[ABCabc]$";

            // 3. Validação do padrão
            if (!Regex.IsMatch(primeiros21, padraoRegex))
            {
                // Se falhar, vamos identificar o que falhou para dar um retorno mais detalhado
                return DetalharErros(primeiros21);
            }

            return "OK";
        }

        private static string DetalharErros(string trecho)
        {
            StringBuilder erros = new StringBuilder("Erro: O padrão dos primeiros 21 caracteres está incorreto. Detalhes:");
            var pep_padrao = "00-000000.P00.000.00X";
            // Validações individuais para ajudar o usuário a corrigir o texto
            if (!Regex.IsMatch(trecho.Substring(0, 2), @"^[0-9]{2}$"))
                erros.Append("\n- Os 2 primeiros caracteres devem ser números.");

            if (trecho[2] != '-')
                erros.Append($"\n O 3º caractere deve ser um hífen (-).");

            if (!Regex.IsMatch(trecho.Substring(3, 6), @"^[0-9]{6}$"))
                erros.Append($"\n- Do 4º ao 9º caractere devem ser 6 números.");

            if (trecho.Substring(9, 2) != ".P")
                erros.Append($"\n- Do 10º ao 11º caractere deve ser '.P'.");

            if (!Regex.IsMatch(trecho.Substring(11, 2), @"^[0-9]{2}$"))
                erros.Append("\n- Do 12º ao 13º caractere devem ser números.");

            if (trecho[13] != '.')
                erros.Append("\n- O 14º caractere deve ser um ponto (.).");

            if (!Regex.IsMatch(trecho.Substring(14, 3), @"^[0-9]{3}$") && trecho.Substring(14, 3) != "ITC")
                erros.Append("\n- Do 15º ao 17º caractere devem ser 3 números.");

            if (trecho[17] != '.')
                erros.Append("\n- O 18º caractere deve ser um ponto (.).");

            if (!Regex.IsMatch(trecho.Substring(18, 2), @"^[A-Za-z0-9]{2}$"))
                erros.Append("\n- O 19º e 20º caractere (XX) devem ser letras ou números.");

            if (!Regex.IsMatch(trecho.Substring(20, 1), @"^[ABCabc]$"))
            {
                var retr = trecho.Substring(19, 1);

                var subs = trecho.Substring(19, 2);

                if (retr == "R")
                {
                    var lista = new string[] { "RC", "RE", "RF", "RM", "RP", "RS", "RV" };

                    if (!subs.EqualsOne(lista))
                    {
                        erros.Append($"\nEtapa de retrabalho inválida. Possíveis combinações:{string.Join(",", lista)}");
                    }
                    else
                    {
                        return "OK";
                    }
                }
                else if (trecho.Substring(14, 3) == "ITC")
                {
                    if (!trecho.Substring(18, 3).ESoNumero() && !trecho.Substring(18, 3).Contains("."))
                    {
                        erros.Append($"\nEtapa ITC inválida. Deve terminar com um número de 000 a 999");
                    }
                    else
                    {
                        return "OK";
                    }
                }
                else if(trecho.Substring(18,1) == "L")
                {
                    if (!trecho.Substring(19, 2).ESoNumero())
                    {
                        erros.Append($"\nEtapa L inválida. Deve terminar com um número de 01 a 99");
                    }
                    else
                    {
                        return "OK";
                    }
                }
                else
                {
                    erros.Append("\n- O 21º caractere (Y) deve ser estritamente A, B ou C.");
                }
            }

            if (erros.Length > 0)
            {
                erros = new StringBuilder($"{pep_padrao}\n{trecho}\n{erros}");
            }

            return erros.ToString();
        }

        public static string AjustarPascalCase(this string codigoGerado)
        {
            if (string.IsNullOrEmpty(codigoGerado)) return "";

            // 1. Ajusta o nome da classe (captura o que está após 'class ')
            codigoGerado = Regex.Replace(codigoGerado, @"(?<=class\s+)([a-zA-Z0-9_]+)", m =>
            {
                return CapitalizarTexto(m.Value);
            });

            // 2. Ajusta as propriedades (captura o nome da propriedade antes de ' {get;set;}')
            codigoGerado = Regex.Replace(codigoGerado, @"([a-zA-Z0-9_]+)(?=\s*\{get;set;\})", m =>
            {
                return CapitalizarTexto(m.Value);
            });

            return codigoGerado;
        }

        // Função interna que transforma "eventos_email_regras" em "Eventos_Email_Regras"
        // ou "prioridade" em "Prioridade"
        private static string CapitalizarTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto;

            // Se tiver underline, capitaliza cada parte mantendo o underline
            if (texto.Contains("_"))
            {
                var partes = texto.Split('_');
                for (int i = 0; i < partes.Length; i++)
                {
                    if (partes[i].Length > 0)
                    {
                        partes[i] = char.ToUpper(partes[i][0]) + partes[i].Substring(1);
                    }
                }
                return string.Join("_", partes);
            }

            // Se for uma palavra única, apenas bota a primeira letra em maiúscula
            return char.ToUpper(texto[0]) + texto.Substring(1);
        }
        public static string Sanitize(this string str)
        {
            if (str != null)
            {
                return str.TrimStart("_").TrimEnd("_").TrimEnd().TrimStart().Upper().RemoverCaracteresEspeciais().Replace(" ", "_");
            }
            return str;
        }
        public static List<string> SplitTabulation(this string str)
        {
            string[] partes = str.Split(new[] { "\r\n", "\n", "\r", "\t", "\v", "\f", "\u2028", "\u2029" }, StringSplitOptions.RemoveEmptyEntries);
            return partes.ToList();
        }
        public static string Upper(this string txt)
        {
            return txt?.ToUpper();
        }
        public static bool Contem(this object item, string valor, double porcentagem = 70)
        {
            if (item == null) { return false; }
            if (valor.IsNullOrEmpty()) { return true; }
            else
            {



                valor = valor.Upper().TrimStart().TrimEnd();
                var descricao_item = item.ToString().Upper();

                if (item is Celula)
                {
                    var cel = item as Celula;
                    descricao_item = $"{cel.ColunaUpper}={cel.ToString()}";
                }

                if (valor == descricao_item)
                {
                    return true;
                }
                else if (descricao_item.Contem(valor))
                {
                    return true;
                }
                else
                {
                    var chaves = valor.Replace("  ", " ").Split(' ').ToList().FindAll(y => y.Replace(" ", "").Count() > 2);

                    int cc = 0;
                    foreach (string chave in chaves)
                    {
                        if (descricao_item.Contem(chave))
                        {
                            cc++;
                        }
                    }

                    if (cc > 0)
                    {
                        double x = 100.0 * cc / chaves.Count().Double();
                        return (x >= porcentagem);
                    }
                    else
                    {
                        return false;
                    }

                }
            }
        }
        /// <summary>
        /// Ajusta o valor para o padrão do PEP, substituindo espaços por "-" e "." e formatando conforme a estrutura do PEP.
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        public static string ToPEP(this string valor)
        {
            var retorno = "";
            var pep = valor.Substituir("", "-", ".");
            //10-123456.P00.001.30A.F2
            //setor atividade
            if (pep.LenghtStr() > 1)
            {
                retorno += $"{pep.Substring(0, 2)}";
            }
            //contrato
            if (pep.LenghtStr() > 7)
            {
                retorno += $"-{pep.Substring(2, 6)}";
            }
            //pedido
            if (pep.LenghtStr() > 10)
            {
                retorno += $".{pep.Substring(8, 3)}";
            }
            //etapa
            if (pep.LenghtStr() > 13)
            {
                retorno += $".{pep.Substring(11, 3)}";
            }
            //sub-etapa
            if (pep.LenghtStr() > 16)
            {
                retorno += $".{pep.Substring(14, 3)}";
            }
            //pep
            if (pep.LenghtStr() > 18)
            {
                retorno += $".{pep.Substring(17, 2)}";
            }

            return retorno;
        }
        public static int LenghtStr(this object valor)
        {
            if (valor != null)
            {
                if (valor is string)
                {
                    return ((string)valor).Length;
                }
                return valor.ToString().Length;
            }

            return 0;
        }
        public static bool NotNullOrEmpty(this object valor, bool decimais = true)
        {
            return !valor.IsNullOrEmpty(decimais);
        }
        public static bool IsNullOrEmpty(this object valor, bool decimais = true)
        {
            if (valor == null) { return true; }

            if (valor is Tabelas)
            {
                var tb = (Tabelas)valor;
                if (tb == null) { return true; }
                else if (tb.Count == 0) { return true; }
            }
            else if (valor is Tabela)
            {
                var tb = (Tabela)valor;
                if (tb == null) { return true; }
                else if (tb.Count == 0) { return true; }
            }
            else if (valor is Linha)
            {
                var tb = (Linha)valor;
                if (tb == null) { return true; }
                else if (tb.Count == 0) { return true; }
            }

            if (valor is string)
            {
                var vlr = (string)valor;
                if (vlr.Length == 0)
                {
                    return true;
                }
            }

            if (valor is long?)
            {
                var vlr = (long?)valor;
                return vlr == 0;
            }
            else if (valor is long)
            {
                var vlr = (long)valor;
                return vlr == 0;
            }
            else if (valor is int?)
            {
                var vlr = (int?)valor;
                return vlr == 0;
            }
            else if (valor is double?)
            {
                var vlr = (double?)valor;
                return vlr == 0;
            }
            else if (valor is double)
            {
                var vlr = (double)valor;
                return vlr == 0;
            }
            else if (valor is decimal?)
            {
                var vlr = (decimal?)valor;
                return vlr == 0;
            }
            else if (valor is decimal)
            {
                var vlr = (decimal)valor;
                return vlr == 0;
            }

            var str = valor.ToString();
            if (valor is Celula)
            {
                str = ((Celula)valor).Valor;
            }

            if (str.LenghtStr() == 0)
            {
                return true;
            }

            if (decimais)
            {
                if (str.LenghtStr() == 1)
                {
                    if (str == "") { return true; }
                    if (str == " ") { return true; }
                    if (str == "0") { return true; }

                    if (str == ".") { return true; }
                    if (str == ",") { return true; }
                    if (str == "'") { return true; }
                }

                else
                {
                    if (str == "0.0") { return true; }
                    if (str == "0,0") { return true; }
                    if (str.Replace("0000-00-00", "").LenghtStr() == 0) { return true; }
                    if (str.Replace("0", "").Replace(",", "").Replace(".", "").LenghtStr() == 0) { return true; }
                    if (str == "0.0d") { return true; }
                }
            }
            else if (str.LenghtStr() == 2)
            {
                if (str == "[]") { return true; }
                else if (str == "{}") { return true; }
            }
            return false;
        }
        public static string GetKey(this string txt)
        {
            if (!txt.IsNullOrEmpty(false))
            {
                return txt.Upper().Replace(" ", "").Replace(".", "");
            }
            return txt;
        }
        public static string FirstCharToUpper(this string text)
        {

            if (text == null)
            {
                return null;
            }
            else if (text.LenghtStr() == 0)
            {
                return "";
            }
            else if (text.LenghtStr() == 1)
            {
                return text.Upper();
            }
            else
            {
                var str_join = "";
                var strs = text.Split(' ').ToList();
                for (int i = 0; i < strs.Count; i++)
                {
                    var st = strs[i];
                    if (i > 0)
                    {
                        str_join += " ";
                    }
                    if (st.LenghtStr() > 0)
                    {
                        if (st.LenghtStr() == 1)
                        {
                            str_join += st.Upper();
                        }
                        else
                        {
                            str_join += char.ToUpper(st[0]) + st.Substring(1).ToLower();
                        }
                    }
                    else
                    {
                        str_join += " ";
                    }
                }
                return str_join;
            }
        }
        public static bool StartsW(this string text, params string[] values)
        {
            if (text == null) { return false; }
            if (text.IsNullOrEmpty(false))
            {
                return false;
            }
            foreach (var value in values)
            {
                if (!value.IsNullOrEmpty(false))
                {
                    if (text.TrimStart().StartsWith(value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool EndsW(this string text, params string[] values)
        {
            if (text.IsNullOrEmpty(false))
            {
                return false;
            }
            foreach (var value in values)
            {
                if (!value.IsNullOrEmpty(false))
                {
                    if (text.TrimStart().EndsWith(value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool EqualsOne(this string text, params string[] values)
        {
            if (text.IsNullOrEmpty(false))
            {
                return false;
            }
            foreach (var value in values)
            {
                if (value == text)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool NotEquals(this string text, params string[] values)
        {
            if (text.IsNullOrEmpty(false))
            {
                return false;
            }
            foreach (var value in values)
            {
                if (value == text)
                {
                    return false;
                }
            }
            return true;
        }
        public static bool Contem(this string text, params string[] values)
        {
            if (text.IsNullOrEmpty())
            {
                return false;
            }
            foreach (var value in values)
            {
                if (value.LenghtStr() > 0)
                {
                    if (text.TrimStart().Contains(value))
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        public static string Remover(this string text, params string[] values)
        {
            if (text.IsNullOrEmpty(false))
            {
                return text;
            }
            foreach (var value in values)
            {
                if (!value.IsNullOrEmpty(false))
                {
                    text = text.Replace(value, "");
                }
            }

            return text;
        }
        public static string Substituir(this string text, string new_value, params string[] old_values)
        {
            if (text.IsNullOrEmpty(false))
            {
                return text;
            }
            foreach (var value in old_values)
            {
                if (!value.IsNullOrEmpty(false))
                {
                    text = text.Replace(value, new_value);
                }
            }

            return text;
        }
        public static bool ContemTudo(this string text, params string[] values)
        {
            if (text.IsNullOrEmpty(false))
            {
                return false;
            }
            foreach (var value in values)
            {
                if (!value.IsNullOrEmpty(false))
                {
                    if (!text.TrimStart().Contains(value))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public static string getLetra(this int indice)
        {
            return getLetra((long)indice);
        }
        public static string getLetra(this long indice)
        {
            string retorno = "";
            do
            {
                long resto = indice % 26;
                retorno = (char)('A' + resto) + retorno;
                indice = (indice / 26) - 1;
            }
            while (indice >= 0);

            return retorno;
        }
        public static bool IsLower(this string valor)
        {
            return valor.Any(char.IsLower);
        }
        public static bool ESoNumero(this string str)
        {
            if (str == null)
            {
                return false;
            }
            if (str.LenghtStr() == 0)
            {
                return false;
            }
            foreach (char c in str.Upper().Remover(",", ".", " ", "-", "+", "E"))
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
        public static bool CaracteresEspeciais(this string valor)
        {
            var regex2 = new Regex("^[a-zA-Z0-9]*$");
            return !regex2.IsMatch(valor.Replace("-", "").Replace("_", "").Replace(" ", ""));
        }
        public static string TrimStart(this string target, string trimString)
        {
            string result = target;
            while (result.StartsW(trimString))
            {
                result = result.Substring(trimString.LenghtStr());
            }

            return result;
        }
        public static string TrimEnd(this string target, string trimString)
        {
            string result = target;
            while (result.EndsW(trimString))
            {
                result = result.Substring(0, result.LenghtStr() - trimString.LenghtStr());
            }

            return result;
        }
        public static string RemoveAspas(this string txt)
        {
            return txt.Replace(@"""", "");
        }

        /// <summary>
        /// Remove caracteres duplicados que estão lado a lado
        /// </summary>
        /// <param name="texto"></param>
        /// <param name="remover"></param>
        /// <returns></returns>
        public static string RemoverDuplicatas(this string texto, string remover)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            string t = regex.Replace(texto, remover);
            return t;
        }
        public static string Esquerda(this string stxt, int MaxComp, bool pontilhado = false)
        {
            string txt = stxt;
            if (txt.LenghtStr() > MaxComp)
            {
                txt = txt.Substring(0, MaxComp) + (pontilhado ? "..." : "");
            }
            return txt;
        }

        public static string CortarStringDireita(this string txt, int comp)
        {
            if (txt.Length > comp)
            {
                return txt.Substring(txt.Length - comp);
            }
            else
            {
                return "";
            }
        }

        public static string Direita(this string stxt, int comp)
        {
            string txt = stxt;
            if (comp < txt.LenghtStr())
            {
                return txt.Substring(txt.LenghtStr() - comp, comp);
            }

            return txt;
        }


        public static string RemoverNumeros(this string txt)
        {
            return Regex.Replace(txt, @"[\d-]", string.Empty);
        }
        public static string RemoverTextos(this string txt, bool manter_sinais = false)
        {
            if (manter_sinais)
            {
                return Regex.Replace(txt, "[^0-9.+-]", "");
            }
            else
            {
                return Regex.Replace(txt, "[^0-9.]", "");
            }
        }
        /// <summary>
        /// Remove os espaços iniciais e finais
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string TrimTxt(this string txt)
        {
            if (txt.NotNullOrEmpty())
            {
                return txt.TrimStart().TrimEnd();
            }
            return txt;
        }
        public static string NormalizarTexto(this string txt)
        {
            if (txt.IsNullOrEmpty())
                return txt;

            txt = txt.Replace("°", "o");
            txt = Regex.Replace(txt, "[\u2010-\u2015\u2212\u00AD]", "-");
            // Normaliza para decompor caracteres acentuados (ex: "é" -> "e" + acento)
            string sem_acento = txt.Normalize(NormalizationForm.FormD);

            // Remove marcas de acento (diacríticos)
            var sem_acento_diacritico = new StringBuilder();
            foreach (var c in sem_acento)
            {
                var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
                if (categoria != UnicodeCategory.NonSpacingMark)
                    sem_acento_diacritico.Append(c);
            }

            // Normaliza de volta para FormC
            string retorno = sem_acento_diacritico.ToString().Normalize(NormalizationForm.FormC);

            // Remove caracteres especiais, deixando apenas letras, números e espaço
            //semAcento = Regex.Replace(semAcento, @"[^a-zA-Z0-9\s]", "");
            retorno = Regex.Replace(retorno, @"[^a-zA-Z0-9\s/\\,.!?%*+-:@]", "");

            // Substitui múltiplos espaços por apenas um
            retorno = Regex.Replace(retorno, @"\s+", " ");

            // Remove espaços iniciais
            return retorno.TrimStart().TrimEnd();
        }
        public static string RemoverAcentos(this string txt)
        {
            if (txt == null) { return null; }

            txt = txt.TrimStart().TrimEnd();
            if (txt.LenghtStr() == 0)
            {
                return "";
            }

            // O FormD separa a letra base do seu acento (ex: 'É' vira 'E' + '´')
            var normalizar = txt.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizar.LenghtStr());

            for (int i = 0; i < normalizar.LenghtStr(); i++)
            {
                char c = normalizar[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

                // Se NÃO for um acento (NonSpacingMark), nós mantemos o caractere base
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Retorna para o FormC (junta novamente caracteres que não deviam ter sido separados)
            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }
        public static string RemoverCaracteresEspeciais(this string txt)
        {
            if (txt == null) { return null; }
            txt = txt.TrimStart().TrimEnd();
            if (txt.LenghtStr() == 0)
            {
                return "";
            }
            var normalizar = txt.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizar.LenghtStr());

            for (int i = 0; i < normalizar.LenghtStr(); i++)
            {
                char c = normalizar[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            var ret = stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);

            var retorno = Regex.Replace(ret, @"[^0-9a-zA-Z-]+", "_");

            retorno = retorno.Substituir(" ", "\n", "\t", "\v", "\r", "\f", @"\N", @"\F", @"\V", @"\R", @"\F");
            retorno = retorno.TrimStart("_").TrimEnd("_");

            return retorno;
        }


        private static readonly object _randomLock = new object();
        private static readonly Random _random = new Random();

        private const string CharsDefault = "abcdefghijklmnopqrstuvwxyz0123456789";

        public static string RandomString(this int length, string chars = CharsDefault)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException("length", "O comprimento deve ser maior que zero.");
            if (string.IsNullOrEmpty(chars))
                throw new ArgumentException("O conjunto de caracteres nao pode ser vazio.", "chars");

            char[] buffer = new char[length];
            lock (_randomLock)
            {
                for (int i = 0; i < length; i++)
                    buffer[i] = chars[_random.Next(chars.Length)];
            }
            return new string(buffer);
        }
    }
}
