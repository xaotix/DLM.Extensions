using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;


namespace Conexoes
{
    public static class ExtensoesClasse
    {
        public static void SalvarIni<T>(this T objeto, string arquivo)
        {
            var props = objeto.GetPropriedades().Filter().OrderBy(x => x.Name);
            var ini = new List<string>();
            ini.Add("[VALORES]");
            ini.AddRange(props.Select(p => $"{p.Name}={p.GetValue(objeto)}").ToList());
            ini.SalvarLista(arquivo);
        }
        public static T CarregarIni<T>(this string arquivo)
        {
            var objeto = Novo<T>();

            var props = objeto.GetPropriedades().Filter();
            var ini = DLM.ini.INI.Get(arquivo);
            foreach (var p in props)
            {
                if (p.CanWrite)
                {
                    var valor = ini.Get("VALORES", p.Name);
                    if (valor.Value == "") { continue; }

                    SetValor(objeto, p, valor.Value);

                }
            }
            return objeto;
        }



        /// <summary>
        /// Copia propriedades se forem básicas
        /// Ignora propriedade com atributo XmlIgnore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Para"></param>
        /// <param name="De"></param>
        public static T CopiarVars<T>(this T Para, T De, bool somente_preenchido = false)
        {
            var props = De.GetPropriedades().Filter();
            var props_para = Para.GetPropriedades().Filter();
            foreach (var prop in props)
            {
                if (prop.CanWrite)
                {
                    var valor = prop.GetValue(De);
                    var igual = props_para.Find(x => x.Name == prop.Name);
                    if (igual != null)
                    {
                        if (igual.CanWrite)
                        {

                            try
                            {
                                if ((valor.ToString().Length > 0 && somente_preenchido) | !somente_preenchido)
                                    igual.SetValue(Para, valor);

                            }
                            catch (Exception ex)
                            {
                                Conexoes.Utilz.Alerta(ex);
                            }
                        }

                    }
                }
            }

            return Para;
        }
        public static T CopiarVars<T>(this T Para, DLM.ini.INISec De, int sufix = 0)
        {
            var props_para = Para.GetPropriedades().Filter();
            foreach (var prop in props_para)
            {
                var valor = sufix < 1 ? De.Values.Find((Predicate<DLM.ini.INIField>)(x => x.Key == prop.Name)) : De.Values.Find((Predicate<DLM.ini.INIField>)(x => x.Key == $"{prop.Name}{sufix}"));
                if (valor != null)
                {
                    SetValor(Para, prop, valor.Value);
                }
            }
            return Para;
        }
        public static void CopiarVars<T>(this T Para, DLM.db.Linha De, string prefix = "")
        {
            var props_para = Para.GetPropriedades().Filter();
            foreach (var prop_para in props_para)
            {
                var valor = De[$"{prefix}{prop_para.Name}"];

                if (valor != null)
                {
                    SetValor(Para, prop_para, valor.Valor);
                }
            }
        }

        public static void SetValor<T>(this T objeto, string Propriedade, string valor)
        {
            var props = objeto.GetPropriedades().Filter().Find(x => x.Name == Propriedade);
            if (props != null)
            {
                if (props.CanWrite)
                {
                    SetValor(objeto, props, valor);
                }
            }
        }
        public static string GetValor<T>(this T objeto, string Propriedade)
        {
            var props = objeto.GetPropriedades().Filter().Find(x => x.Name == Propriedade);
            if (props != null)
            {
                return props.GetValue(objeto).ToString(); ;
            }
            return null;
        }

        private static void SetValor<T>(T Objeto, PropertyInfo Propriedade, string Valor)
        {
            if (Valor == null) { return; }
            var prop = Propriedade.PropertyType.Name.ToLower();

            if (Propriedade.CanWrite && Propriedade.CanRead && 
                (
                Propriedade.PropertyType.IsEnum | 
                Propriedade.PropertyType.IsPrimitive | 
                prop == "string" | 
                prop == "datetime" |
                prop == "nullable`1"
                )
                )
            {
                if (prop == "string")
                {
                    Propriedade.SetValue(Objeto, Valor);
                }
                else if (prop == "double")
                {
                    Propriedade.SetValue(Objeto, Valor.Double(20));
                }
                else if (prop == "boolean")
                {
                    Propriedade.SetValue(Objeto, Valor.Boolean());
                }
                else if (prop.StartsWith("int"))
                {
                    Propriedade.SetValue(Objeto, Valor.Int());
                }
                else if (prop.StartsWith("long"))
                {
                    Propriedade.SetValue(Objeto, Valor.Long());
                }
                else if (prop == "datetime" | (prop == "nullable`1" && Propriedade.PropertyType.FullName.Contains("System.DateTime")))
                {
                    Propriedade.SetValue(Objeto, Valor.DataNull());
                }
                else if (Propriedade.PropertyType.IsEnum)
                {
                    if (Valor.ESoNumero())
                    {
                        var et = Enum.ToObject(Propriedade.PropertyType, Valor.Int());
                        Propriedade.SetValue(Objeto, et);
                    }
                    else if (!Valor.Contains(" ") && Valor.Length > 0)
                    {
                        var et = Enum.Parse(Propriedade.PropertyType, Valor);
                        Propriedade.SetValue(Objeto, et);
                    }
                }
                else
                {

                }
            }
           

        }


        public static T DeSerializar<T>(this string arquivo)
        {
            string xml = "";
            var serializer = new XmlSerializer(typeof(T));
            if (arquivo.Length > 0)
            {
                if (File.Exists(arquivo))
                {

                    try
                    {



                        using (Stream writer = File.Open(arquivo, FileMode.Open))
                        {
                            var retorno = (T)serializer.Deserialize(writer);
                            return retorno;
                        }



                    }
                    catch (Exception)
                    {
                        xml = string.Join("\n", Utilz.Arquivo.Ler(arquivo));
                        //Conexoes.Utilz.Alerta(ex);
                    }

                }
                else if (arquivo.Contains("?xml"))
                {
                    xml = arquivo;
                }



                if (xml.Length > 0)
                {
                    try
                    {
                        using (TextReader reader = new StringReader(xml))
                        {
                            var retorno = (T)serializer.Deserialize(reader);
                            return retorno;
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.Alerta($"Erro ao tentar ler o XML" +
                            (!arquivo.Contains("?xml") ? $"\n[Arquivo ={arquivo}]" : "") +
                            $"\n[XML]" +
                            $"\n{xml}", "DeSerializar");
                    }

                }

            }
            try
            {
                return Novo<T>();
            }
            catch (Exception)
            {
            }

            return RetornaNull<T>();
        }
        public static List<T> Mirror<T>(this List<T> Origem)
        {
            List<T> retorno = new List<T>();
            for (int i = Origem.Count - 1; i >= 0; i--)
            {
                retorno.Add(Origem[i]);
            }

            return retorno;
        }

        private static T Novo<T>()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }

        private static T RetornaNull<T>()
        {
            return (T)Convert.ChangeType(null, typeof(T));
        }
        public static T GetSelecao<T>(this object sender)
        {
            if (sender is FrameworkElement)
            {
                var sel = (T)((FrameworkElement)sender).DataContext;
                return sel;

            }
            return RetornaNull<T>();
        }

        public static string Serializar<T>(this T toSerialize, string arquivo = null)
        {
            string texto = "";

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());
                using (StringWriter textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, toSerialize);
                    texto = textWriter.ToString();
                }
                if (arquivo != null)
                {
                    if (arquivo.Delete())
                    {
                        Utilz.Arquivo.Gravar(arquivo, new List<string> { texto });
                    }
                }
            }
            catch (Exception ex)
            {
                Utilz.Alerta(ex);
                texto = "";
            }


            return texto;

        }

        public static T Clonar<T>(this T toSerialize)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());
                using (StringWriter textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, toSerialize);
                    return DeSerializar<T>(textWriter.ToString());
                }
            }
            catch (Exception ex)
            {
                Conexoes.Utilz.Alerta(ex);
            }

            return (T)Convert.ChangeType(null, typeof(T));
        }

        public static DLM.db.Linha GetLinha<T>(this T obj, bool only_can_write = false, params string[] remover)
        {
            var tbl = GetTabela<T>(new List<T> { obj }, only_can_write);
            var linha = tbl.Linhas[0];
            foreach(var r in remover)
            {
                var cel = linha[r];
                linha.Celulas.Remove(cel);
            }
            return linha;
        }
        public static DLM.db.Linha GetLinha<T>(this T obj)
        {
            var tbl = GetTabela<T>(new List<T> { obj });
            return tbl.Linhas[0];
        }
        /// <summary>
        /// Extrai uma tabela com todas as propriedades da lista
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lista"></param>
        /// <returns></returns>
        public static DLM.db.Tabela GetTabela<T>(this List<T> lista, bool only_can_write = false)
        {

            var retorno = new DLM.db.Tabela();
            if (lista.Count > 0)
            {
                List<PropertyInfo> todos = lista[0].GetPropriedades().Filter();
                List<PropertyInfo> listagem = todos.FindAll(x => x.Browsable());
                if (only_can_write)
                {
                    listagem = listagem.FindAll(x => x.CanWrite);
                }
                var colunas = listagem.Select(x => x.Name).ToList();
                var display = listagem.Select(x => x.GetDisplayName()).ToList();
                foreach (var L in lista)
                {
                    //pula itens que são diferentes do primeiro item da lista
                    if (L.GetType() != lista[0].GetType())
                    {
                        continue;
                    }
                    var linha = new DLM.db.Linha();
                    var props = L.GetPropriedades().Filter();

                    for (int c = 0; c < colunas.Count; c++)
                    {
                        var igual = props.Find(x => x.Name == colunas[c]);

                        if (igual != null)
                        {
                            var valor = igual.GetValue(L);
                            linha.Add(colunas[c], valor);
                        }
                        else
                        {
                            linha.Add(colunas[c], null);
                        }
                    }
                    retorno.Add(linha);
                }

            }
            return retorno;
        }

        public static DLM.db.Tabela GetTabela(this List<List<object>> lista, bool header = true)
        {
            var retorno = new DLM.db.Tabela();
            var l_header = new DLM.db.Linha();
            var c_max = lista.Max(x => x.Count);

            for (int c = 0; c < c_max; c++)
            {
                if (header && c < lista[0].Count)
                {
                    l_header.Add(lista[0][c].ToString(), "");
                }
                else
                {
                    l_header.Add(c.getLetra(), "");
                }
            }


            for (int l = 0; l < lista.Count; l++)
            {
                if (l == 0 && header)
                {
                    continue;
                }
                var lc = new DLM.db.Linha();
                for (int c = 0; c < lista[l].Count; c++)
                {
    
                    lc.Add(l_header[c].Coluna, lista[l][c]);
                }
                retorno.Add(lc);
            }

            return retorno;
        }




        /// <summary>
        /// Filtra, removendo XmlIgnore & !CanWrite
        /// </summary>
        /// <param name="lista"></param>
        /// <returns></returns>
        public static List<PropertyInfo> Filter(this List<PropertyInfo> lista)
        {
            var retorno = new List<PropertyInfo>();

            foreach (var l in lista)
            {
                var prop = l.PropertyType.Name.ToLower();
                if (l.PropertyType.IsPrimitive | l.PropertyType.IsEnum | prop == "string" | prop == "datetime")
                {
                    retorno.Add(l);
                }
                else if(prop == "nullable`1")
                {
                    if(l.PropertyType.FullName.Contains("System.DateTime"))
                    {
                        retorno.Add(l);
                    }
                }
            }
            return retorno;
        }

        public static List<PropertyInfo> GetPropriedades<T>(this T objeto)
        {
            if (objeto == null) { return new List<PropertyInfo>(); }
            var props = objeto.GetType().GetProperties().ToList().FindAll(x => x.CanRead);
            return props;
        }

        public static string GetPropriedade<T>(this T obj, string propriedade)
        {
            string valor = "";
            var propriedaes = obj.GetPropriedades();
            var prop = propriedaes.Find(x => x.Name.ToUpper() == propriedade.ToUpper());

            if(prop!=null)
            {
                return prop.GetValue(obj).ToString();
            }


            return valor;
        }





        private static string GetDisplayName(this PropertyInfo property)
        {
            var atts = property.DeclaringType.GetCustomAttributes(typeof(MetadataTypeAttribute), true);
            if (atts.Length > 0)
            {
                var metaAttr = atts[0] as MetadataTypeAttribute;
                var metaProperty = metaAttr.MetadataClassType.GetProperty(property.Name);
                return metaProperty.GetDisplayName();
            }




            var atts2 = property.GetCustomAttributes(typeof(DisplayNameAttribute), true);
            if (atts2.Length > 0)
            {
                var att = (atts2[0] as DisplayNameAttribute);
                return att.DisplayName;
            }

            return property.Name;
        }



        /// <summary>
        /// Verifica se uma propriedade de classe é visivel
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool Browsable(this PropertyInfo c)
        {
            if (c.CanRead)
            {
                //procura o atributo [Browsable(valor)]
                var att = c.CustomAttributes.ToList().Find(x => x.AttributeType.Name == "BrowsableAttribute");

                var get = c.GetMethod.Attributes;


                var tem_indices = c.GetIndexParameters();
                if (tem_indices.Count() > 0)
                {
                    return false;
                }

                if (att != null)
                {
                    //procura o argumento Boolean do atributo e verifica se é trus [Browsable(true)]
                    var opt = att.ConstructorArguments.ToList().Find(x => x.ArgumentType.Name == "Boolean");
                    if (opt != null)
                    {
                        if ((bool)opt.Value)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
            return false;
        }
        public static bool XmlIgnore(this PropertyInfo c)
        {
            if (c.CanRead)
            {
                //procura o atributo [Browsable(valor)]
                var att = c.CustomAttributes.ToList().Find(x => x.AttributeType.Name == "XmlIgnoreAttribute");
                return att != null;
            }

            return false;
        }

    }
}
