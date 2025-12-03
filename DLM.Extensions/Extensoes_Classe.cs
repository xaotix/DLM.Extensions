using DLM;
using DLM.db;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
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
                    var igual_para = props_para.Find(x => x.Name == prop.Name);
                    if (igual_para != null)
                    {
                        if (igual_para.CanWrite)
                        {
                            try
                            {
                                if (valor != null && somente_preenchido)
                                {
                                    if ((valor.ToString().LenghtStr() > 0 && somente_preenchido))
                                        igual_para.SetValue(Para, valor);
                                }
                                else if (!somente_preenchido)
                                {
                                    igual_para.SetValue(Para, valor);
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.Alerta();
                            }
                        }

                    }
                }
            }

            return Para;
        }
        public static T CopiarVars<T>(this T Para, DLM.ini.INISec De, int sufix = 0)
        {
            var props_para = Para.GetPropriedades().Filter().FindAll(x => x.CanWrite);
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
        public static void CopiarVars<T>(this T Para, DLM.db.Linha De, string prefix = "", bool ignorar_vazias = false)
        {
            var props_para = Para.GetPropriedades().Filter().FindAll(x => x.CanWrite);
            foreach (var prop_para in props_para)
            {
                var valor = De[$"{prefix}{prop_para.Name}", true];

                if (valor != null)
                {
                    if (ignorar_vazias && valor.IsNullOrEmpty())
                    {
                        continue;
                    }
                    else
                    {
                        SetValor(Para, prop_para, valor.Valor);
                    }
                }
                else
                {

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

            if (Propriedade.CanWrite && Propriedade.CanRead
                //&&
                //(
                //Propriedade.PropertyType.IsEnum |
                //Propriedade.PropertyType.IsPrimitive |
                //prop == "string" |
                //prop == nameof(PesoStr).ToLower() |
                //prop == nameof(RSStr).ToLower() |
                //prop == "datetime" |
                //prop == "decimal" |
                //prop == "nullable`1"
                //)
                )
            {
                if (prop == "string")
                {
                    Propriedade.SetValue(Objeto, Valor);
                }
                else if (prop == nameof(PesoStr).ToLower())
                {
                    Propriedade.SetValue(Objeto, Valor.PesoStr(20));
                }
                else if (prop == nameof(RSStr).ToLower())
                {
                    Propriedade.SetValue(Objeto, Valor.RSStr(20));
                }
                else if ((prop == "nullable`1" && Propriedade.PropertyType.FullName.Contem("System.Double")))
                {
                    Propriedade.SetValue(Objeto, Valor.DoubleNull(20));
                }
                else if (prop == "double")
                {
                    Propriedade.SetValue(Objeto, Valor.Double(20));
                }
                else if ((prop == "nullable`1" && Propriedade.PropertyType.FullName.Contem("System.Decimal")))
                {
                    Propriedade.SetValue(Objeto, Valor.DecimalNull(20));
                }
                else if (prop == "decimal")
                {
                    Propriedade.SetValue(Objeto, Valor.Decimal(20));
                }
                else if (prop == "nullable`1" && Propriedade.PropertyType.FullName.Contem("System.Boolean"))
                {
                    Propriedade.SetValue(Objeto, Valor.BooleanNull());
                }
                else if (prop == "boolean")
                {
                    Propriedade.SetValue(Objeto, Valor.Boolean());
                }
                else if (prop == "nullable`1" && Propriedade.PropertyType.FullName.Contem("System.Int64"))
                {
                    Propriedade.SetValue(Objeto, Valor.LongNull());
                }
                else if (prop.StartsW("long") || prop.StartsW("int64"))
                {
                    Propriedade.SetValue(Objeto, Valor.Long());
                }
                else if ((prop == "nullable`1" && Propriedade.PropertyType.FullName.Contem("System.Int32")))
                {
                    Propriedade.SetValue(Objeto, Valor.IntNull());
                }
                else if (prop.StartsW("int"))
                {
                    Propriedade.SetValue(Objeto, Valor.Int());
                }
                else if (prop == "datetime" | (prop == "nullable`1" && Propriedade.PropertyType.FullName.Contem("System.DateTime")))
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
                    else if (!Valor.Contem(" ") && Valor.LenghtStr() > 0)
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
            if (arquivo.LenghtStr() > 0)
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
                else if (arquivo.Contem("?xml", "xmlns="))
                {
                    xml = arquivo;
                }



                if (xml.LenghtStr() > 0)
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
                            (!arquivo.Contem("?xml") ? $"\n[Arquivo ={arquivo}]" : "") +
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
                ex.Alerta();
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
                ex.Alerta();
            }

            return (T)Convert.ChangeType(null, typeof(T));
        }

        public static DLM.db.Linha GetLinha<T>(this T obj, bool only_can_write = true, bool only_browsable = true, bool only_simple_properties = true, params string[] remover)
        {
            var tbl = GetTabela<T>(new List<T> { obj }, only_can_write, only_browsable, only_simple_properties);
            if (tbl.Count == 0)
            {
                return new DLM.db.Linha();
            }
            var linha = tbl[0];
            foreach (var r in remover)
            {
                var cel = linha[r];
                linha.Remove(cel);
            }
            return linha;
        }

        /// <summary>
        /// Extrai uma tabela com todas as propriedades da lista
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lista"></param>
        /// <returns></returns>
        public static DLM.db.Tabela GetTabela<T>(this List<T> lista, bool only_can_write, bool only_browsable = true, bool simple_properties = true, string nome = "")
        {

            var retorno = new DLM.db.Tabela(nome);
            if (lista.Count > 0)
            {
                var listagem = new List<PropertyInfo>();
                listagem.AddRange(lista[0].GetPropriedades());
                var colunas_props = new List<Celula>();
                foreach (var item in listagem)
                {
                    var ncel = new Celula(item.Name);
                    ncel.DisplayName = item.GetDisplayName();
                    ncel.StringFormat = item.GetDataFormatString();
                    colunas_props.Add(ncel);
                }

                if (simple_properties)
                {
                    listagem = listagem.Filter();
                }
                if (only_browsable)
                {
                    listagem = listagem.FindAll(x => x.Browsable());
                }
                if (only_can_write)
                {
                    listagem = listagem.FindAll(x => x.CanWrite);
                }
                var colunas = listagem.Select(x => x.Name).ToList();
                var display = listagem.Select(x => x.GetDisplayName()).ToList();
                foreach (var item in lista)
                {
                    if (item == null)
                    {
                        continue;
                    }
                    //pula itens que são diferentes do primeiro item da lista
                    if (item.GetType() != lista[0].GetType())
                    {
                        continue;
                    }
                    var celulas = new List<Celula>();
                    var props = item.GetPropriedades().Filter();

                    for (int c = 0; c < colunas.Count; c++)
                    {
                        var igual = props.Find(x => x.Name == colunas[c]);
                        var cel = new DLM.db.Celula(colunas[c]);
                        var ccel = colunas_props.Find(x => x.Coluna == colunas[c]);

                        if (igual != null)
                        {

                            try
                            {
                                var vlr = igual.GetValue(item);
                                cel.Set(vlr);
                            }
                            catch (Exception)
                            {

                            }
                        }


                        if (ccel != null)
                        {
                            cel.DisplayName = ccel.DisplayName;
                            cel.StringFormat = ccel.StringFormat;
                        }

                        celulas.Add(cel);
                    }
                    retorno.Add(new Linha(celulas));
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
        /// Filtra, mantendo somente objetos simples
        /// </summary>
        /// <param name="lista"></param>
        /// <returns></returns>
        public static List<PropertyInfo> Filter(this List<PropertyInfo> lista)
        {
            var retorno = new List<PropertyInfo>();

            foreach (var l in lista)
            {
                var prop = l.PropertyType.Name.ToLower();
                if (l.PropertyType.IsPrimitive | l.PropertyType.IsEnum | prop == "string" | prop == "datetime" | prop == "decimal" | prop == "pesostr" | prop == "rsstr")
                {
                    retorno.Add(l);
                }
                else if (prop == "nullable`1")
                {
                    var full = l.PropertyType.FullName;
                    if (full.Contem(
                        "System.DateTime",
                        "System.Double",
                        "System.Decimal",
                        "System.Int16",
                        "System.Int32",
                        "System.Int64"
                        ))
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

            if (prop != null)
            {
                return prop.GetValue(obj).ToString();
            }


            return valor;
        }





        private static string GetDisplayName(this PropertyInfo property)
        {
            if (property == null)
            {
                return "";
            }
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
        private static string GetDataFormatString(this PropertyInfo property)
        {
            try
            {
                var displayFormatAttr = property.GetCustomAttribute<DisplayFormatAttribute>();
                if (displayFormatAttr != null)
                {
                    return displayFormatAttr.DataFormatString;
                }

                // Verifica se há MetadataTypeAttribute
                var metadataAttrs = property.DeclaringType.GetCustomAttributes(typeof(MetadataTypeAttribute), true);
                if (metadataAttrs.Length > 0)
                {
                    var metaAttr = metadataAttrs[0] as MetadataTypeAttribute;
                    var metaProperty = metaAttr.MetadataClassType.GetProperty(property.Name);
                    if (metaProperty != null)
                    {
                        return metaProperty.GetDataFormatString();
                    }
                }

                // Verifica se há DisplayFormatAttribute diretamente
                var formatAttr = property.GetCustomAttributes(typeof(DisplayFormatAttribute), true)
                                         .FirstOrDefault() as DisplayFormatAttribute;

                return formatAttr?.DataFormatString ?? "";
            }
            catch (Exception)
            {

            }

            return "";
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
