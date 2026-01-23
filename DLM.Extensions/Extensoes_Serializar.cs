using Conexoes;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

// ======================================================================
//  BASE64 AUTOMÁTICO PARA STRINGS COM CARACTERES INVÁLIDOS
// ======================================================================
public static class XmlSafe
{
    private const string Prefix = "__B64__";

    public static bool ContemInvalido(string s)
    {
        if (string.IsNullOrEmpty(s))
            return false;

        foreach (char c in s)
            if (!XmlConvert.IsXmlChar(c))
                return true;

        return false;
    }

    public static bool EstaMarcado(string s)
    {
        return s != null && s.StartsWith(Prefix);
    }

    public static string Encode(string s)
    {
        if (s == null) return null;
        return Prefix + Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
    }

    public static string Decode(string s)
    {
        if (s == null) return null;

        if (!s.StartsWith(Prefix))
            return s;

        string b64 = s.Substring(Prefix.Length);
        return Encoding.UTF8.GetString(Convert.FromBase64String(b64));
    }
}

// ======================================================================
//  LIMPEZA RECURSIVA DO OBJETO (ANTES E DEPOIS DA SERIALIZAÇÃO)
// ======================================================================
public static class XmlObjectCleaner
{
    public static T PrepararParaSerializar<T>(T obj)
    {
        TratarObjeto(obj, encode: true);
        return obj;
    }

    public static T RestaurarDepoisDeDeserializar<T>(T obj)
    {
        TratarObjeto(obj, encode: false);
        return obj;
    }

    private static void TratarObjeto(object obj, bool encode)
    {
        if (obj == null)
            return;

        var tipo = obj.GetType();

        if (tipo.IsPrimitive || tipo.IsEnum || tipo == typeof(decimal) ||
            tipo == typeof(DateTime) || tipo == typeof(Guid))
            return;

        if (tipo == typeof(string))
            return;

        if (typeof(IEnumerable).IsAssignableFrom(tipo))
        {
            foreach (var item in (IEnumerable)obj)
                TratarObjeto(item, encode);
            return;
        }

        foreach (var prop in tipo.GetProperties())
        {
            if (!prop.CanRead || !prop.CanWrite)
                continue;

            var pt = prop.PropertyType;
            var valor = prop.GetValue(obj);

            if (valor == null)
                continue;

            if (pt == typeof(string))
            {
                string s = (string)valor;

                if (encode)
                {
                    if (XmlSafe.ContemInvalido(s) && !XmlSafe.EstaMarcado(s))
                        prop.SetValue(obj, XmlSafe.Encode(s));
                }
                else
                {
                    if (XmlSafe.EstaMarcado(s))
                        prop.SetValue(obj, XmlSafe.Decode(s));
                }
            }
            else if (!pt.IsPrimitive && pt != typeof(decimal) &&
                     pt != typeof(DateTime) && pt != typeof(Guid))
            {
                TratarObjeto(valor, encode);
            }
        }
    }
}

// ======================================================================
//  WRITER UTF‑8
// ======================================================================
public class Utf8StringWriter : StringWriter
{
    public override Encoding Encoding => Encoding.UTF8;
}

// ======================================================================
//  FUNÇÕES PRINCIPAIS RENOMEADAS
// ======================================================================
public static class XmlSeguro
{
    // -------------------------------------------------------------
    //  SALVAR (SERIALIZAR) — UTF‑8 + BASE64 AUTOMÁTICO RECURSIVO
    // -------------------------------------------------------------
    public static string SerializarEspecial<T>(this T objeto, string arquivo = null)
    {
        string texto = "";

        try
        {
            objeto = XmlObjectCleaner.PrepararParaSerializar(objeto);

            var serializer = new XmlSerializer(typeof(T));

            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                NewLineHandling = NewLineHandling.Entitize
            };

            using (var sw = new Utf8StringWriter())
            using (var xw = XmlWriter.Create(sw, settings))
            {
                serializer.Serialize(xw, objeto);
                texto = sw.ToString();
            }

            if (!string.IsNullOrEmpty(arquivo))
                File.WriteAllText(arquivo, texto, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            ex.Alerta();
            texto = "";
        }

        return texto;
    }

    // -------------------------------------------------------------
    //  CARREGAR (DESSERIALIZAR) — RESTAURA BASE64 EM TODO O GRAFO
    // -------------------------------------------------------------
    public static T DesSerializarEspecial<T>(this string arquivoOuXml)
    {
        string xml = null;
        var serializer = new XmlSerializer(typeof(T));

        try
        {
            if (string.IsNullOrWhiteSpace(arquivoOuXml))
                return default(T);

            if (File.Exists(arquivoOuXml))
            {
                using (var fs = new FileStream(arquivoOuXml, FileMode.Open, FileAccess.Read))
                using (var sr = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                {
                    xml = sr.ReadToEnd();
                }
            }
            else if (arquivoOuXml.Contains("<?xml"))
            {
                xml = arquivoOuXml;
            }
            else
            {
                return default(T);
            }

            using (var reader = new StringReader(xml))
            {
                var obj = (T)serializer.Deserialize(reader);
                obj = XmlObjectCleaner.RestaurarDepoisDeDeserializar(obj);
                return obj;
            }
        }
        catch (Exception ex)
        {
            ex.Alerta();
            return default(T);
        }
    }
}
