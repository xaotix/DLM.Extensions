using Conexoes;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Conexoes.Macros
{
    public static class Extensoes_Furo_Purlin
    {
        [Browsable(false)]
        public static string GetStr(this Tipo_Posicao_Purlin Posicao)
        {
            switch (Posicao)
            {
                case Tipo_Posicao_Purlin.FB:
                    return "FLB";
                case Tipo_Posicao_Purlin.FB_Rebatido:
                    return "FBR";
                case Tipo_Posicao_Purlin.Eixo_Vao:
                    return "EIX";
                case Tipo_Posicao_Purlin.Centro_Vao:
                    return "CEV";
                case Tipo_Posicao_Purlin.Corrente_Esquerda:
                    return "CRE";
                case Tipo_Posicao_Purlin.Corrente_Direita:
                    return "CRD";
                case Tipo_Posicao_Purlin.Corrente:
                    return "CRO";
                case Tipo_Posicao_Purlin.Manual:
                    return "MAN";
                case Tipo_Posicao_Purlin.Borda:
                    return "BOR";
                case Tipo_Posicao_Purlin.Transpasse:
                    return "TRA";
                case Tipo_Posicao_Purlin.Rebatido:
                    return "REB";
                case Tipo_Posicao_Purlin.Array:
                    return "ARR";
                case Tipo_Posicao_Purlin.SBR:
                    return "SBR";
                case Tipo_Posicao_Purlin.Centro_Peca:
                    return "CPC";
            }
            return "";
        }
        public static List<Furo_Purlin> Aninhar(this List<Furo_Purlin> furos, double Comprimento)
        {
            furos = furos.OrderBy(x => x.X).ToList();
            for (int i = 0; i < furos.Count; i++)
            {
                if (i == 0)
                {
                    furos[i].Anterior = new Furo_Purlin();
                    if (furos.Count > 1)
                    {
                        furos[i].Proximo = furos[i + 1];
                    }
                }
                else if (i < furos.Count - 1 && furos.Count > 1)
                {
                    furos[i].Proximo = furos[i + 1];
                    furos[i].Anterior = furos[i - 1];
                }
                else if (furos.Count > 1)
                {
                    furos[i].Proximo = new Furo_Purlin(Comprimento, 0);
                    furos[i].Anterior = furos[i - 1];
                }
            }
            furos = furos.OrderBy(x => x.X).ToList();
            return furos;
        }
        public static  List<Furo_Purlin> Rebater(this List<Furo_Purlin> furos, double Comprimento, bool Rebater_Furos_Ignorar = true, double Dist_Min = 25)
        {
            var retorno = new List<Furo_Purlin>();
            foreach (var s in furos)
            {
                var fn = s.MoverX(-s.X);
                fn = fn.MoverX(Comprimento - s.X);
                fn.Pai = s;
                fn.Posicao = Tipo_Posicao_Purlin.Rebatido;
                fn.Texto.Value = s.Posicao.GetStr() + " - " + s.ToString();
                //só rebate se não encontra nenhum furo
                if (furos.Find(x => Math.Round(x.X) == Math.Round(fn.X) | Math.Round(x.X) == Math.Round(fn.X - 1) | Math.Round(x.X) == Math.Round(fn.X + 1)) == null)
                {
                    //só rebate se não encontra nenhum furo próximo
                    if (Rebater_Furos_Ignorar)
                    {
                        if (furos.FindAll(x => x.GetDistancia(fn) < Dist_Min).Count == 0)
                        {
                            retorno.Add(fn);
                        }
                    }
                    else
                    {
                        retorno.Add(fn);
                    }
                }
            }
            return retorno;
        }
        public static List<Furo_Purlin> GetFurosBatendo(this List<Furo_Purlin> furos, double Dist_Min = 25)
        {
            if (furos.Count == 1)
            {
                return new List<Furo_Purlin>();
            }
            else
            {
                return furos.FindAll(x => x.Anterior.DistanciaX(x) < Dist_Min | x.Proximo.DistanciaX(x) < Dist_Min).OrderBy(x => x.X).ToList();
            }
        }
    }
}
