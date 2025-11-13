using DLM.vars;
using netDxf;
using netDxf.Entities;
using System.Collections.Generic;

namespace DLM.desenho
{
    public static class ExtensoesNetDxfEM1
    {
        public static List<EntityObject> AddPerfil_ULateral(this DxfDocument dxf, netDxf.Tables.Layer l, Conexoes.Macros.Escada.Escada_Parte parte, double offset_x)
        {
            var retorno = new List<netDxf.Entities.EntityObject>();

            retorno.Add(dxf.AddRec(l, offset_x, parte.Origem_Final.Acumulada_Y, DLM.vars.Global.EM1.Perfil_U_Secao, parte.Comprimento_Final));
            double y1 = parte.Origem_Final.Acumulada_Y;
            double y2 = parte.Origem_Final.Acumulada_Y + parte.Comprimento_Final;
            retorno.Add(dxf.AddLine(l, offset_x + DLM.vars.Global.EM1.Perfil_U_Espessura, y1, offset_x + DLM.vars.Global.EM1.Perfil_U_Espessura, y2));
            retorno.Add(dxf.AddLine(l, offset_x + DLM.vars.Global.EM1.Perfil_U_Secao - DLM.vars.Global.EM1.Perfil_U_Espessura, y1, offset_x + +Global.EM1.Perfil_U_Secao - DLM.vars.Global.EM1.Perfil_U_Espessura, y2));

            foreach (var furo in parte.Furos_DIR)
            {
                if (furo.Face == Furo_Face.Alma)
                {
                    retorno.AddRange(dxf.AddFuro(l, offset_x + furo.Origem.Y, parte.Origem_Final.Acumulada_Y + furo.Origem.X, furo.Diametro, Desenho_Furo.Vista));
                }
                else if (furo.Face == Furo_Face.Aba1)
                {
                    retorno.AddRange(dxf.AddFuro(l, offset_x, parte.Origem_Final.Acumulada_Y + furo.Origem.X, furo.Diametro, Desenho_Furo.Corte));

                }
                else if (furo.Face == Furo_Face.Aba2)
                {
                    retorno.AddRange(dxf.AddFuro(l, offset_x + DLM.vars.Global.EM1.Perfil_U_Secao, parte.Origem_Final.Acumulada_Y + furo.Origem.X, furo.Diametro, Desenho_Furo.Corte));

                }
            }

            foreach (var furo in parte.Furos_ESQ)
            {
                if (furo.Face == Furo_Face.Alma)
                {
                    retorno.AddRange(dxf.AddFuro(l, offset_x + furo.Origem.Y, parte.Origem_Final.Acumulada_Y + furo.Origem.X, furo.Diametro, Desenho_Furo.Vista));
                }
                else if (furo.Face == Furo_Face.Aba2)
                {
                    retorno.AddRange(dxf.AddFuro(l, offset_x, parte.Origem_Final.Acumulada_Y + furo.Origem.X, furo.Diametro, Desenho_Furo.Corte));

                }
                else if (furo.Face == Furo_Face.Aba1)
                {
                    retorno.AddRange(dxf.AddFuro(l, offset_x + DLM.vars.Global.EM1.Perfil_U_Secao, parte.Origem_Final.Acumulada_Y + furo.Origem.X, furo.Diametro, Desenho_Furo.Corte));
                }
            }

            return retorno;
        }
        public static void AddPerfil_Frontal(this DxfDocument dxf, netDxf.Tables.Layer l, netDxf.Tables.Layer lC, Conexoes.Macros.Escada.Escada_Parte parte)
        {
            //desenho do perfil
            dxf.AddRec(l, parte.Origem_Final.Acumulada_X, parte.Origem_Final.Acumulada_Y, parte.Largura, parte.Comprimento_Final);
            dxf.AddRec(l, parte.Origem_Final.Acumulada_X + DLM.vars.Global.EM1.Degrau_Distancia_Horizontal + DLM.vars.Global.EM1.Perfil_U_Largura, parte.Origem_Final.Acumulada_Y, parte.Largura, parte.Comprimento_Final);
            //linhas pontilhadas espessura perfil
            dxf.AddLine(l, parte.Origem_Final.Acumulada_X + DLM.vars.Global.EM1.Perfil_U_Largura - DLM.vars.Global.EM1.Perfil_U_Espessura, parte.Origem_Final.Acumulada_Y, parte.Origem_Final.Acumulada_X + DLM.vars.Global.EM1.Perfil_U_Largura - DLM.vars.Global.EM1.Perfil_U_Espessura, parte.Origem_Final.Acumulada_Y + parte.Comprimento_Final, netDxf.Tables.Linetype.Dashed);
            dxf.AddLine(l, parte.Origem_Final.Acumulada_X + DLM.vars.Global.EM1.Degrau_Distancia_Horizontal + DLM.vars.Global.EM1.Perfil_U_Largura + DLM.vars.Global.EM1.Perfil_U_Espessura, parte.Origem_Final.Acumulada_Y, parte.Origem_Final.Acumulada_X + DLM.vars.Global.EM1.Degrau_Distancia_Horizontal + DLM.vars.Global.EM1.Perfil_U_Largura + DLM.vars.Global.EM1.Perfil_U_Espessura, parte.Origem_Final.Acumulada_Y + parte.Comprimento_Final, netDxf.Tables.Linetype.Dashed);
            double y = parte.Origem_Final.Acumulada_Y + (DLM.vars.Global.EM1.Cantoneira_Largura / 2);
            double y0 = parte.Origem_Final.Acumulada_Y;
            double x1 = parte.Origem_Final.Acumulada_X + DLM.vars.Global.EM1.Perfil_U_Largura;
            double x2 = parte.Origem_Final.Acumulada_X + DLM.vars.Global.EM1.Degrau_Distancia_Horizontal + DLM.vars.Global.EM1.Perfil_U_Largura;

            //cantoneira da base
            if (parte.Tipo == Tipo.Inicial | parte.Tipo == Tipo.Simples)
            {

                //cantoneiras
                //esquerdo
                dxf.AddPerfilL(lC, x1 - DLM.vars.Global.EM1.Perfil_U_Espessura, y, Orientacao.SEI);
                //direito
                dxf.AddPerfilL(lC, x2 + DLM.vars.Global.EM1.Perfil_U_Espessura, y, Orientacao.SDS);

                //furos base
                dxf.AddFuro(lC, x1 - DLM.vars.Global.EM1.Cantoneira_Offset_Furo - DLM.vars.Global.EM1.Perfil_U_Espessura, y0, DLM.vars.Global.EM1.Cantoneira_Furo_Diametro, Desenho_Furo.Corte, true, Sentido.Vertical);
                dxf.AddFuro(lC, x2 + DLM.vars.Global.EM1.Cantoneira_Offset_Furo + DLM.vars.Global.EM1.Perfil_U_Espessura, y0, DLM.vars.Global.EM1.Cantoneira_Furo_Diametro, Desenho_Furo.Corte, true, Sentido.Vertical);

            }

            foreach (var t in parte.Furos_ESQ)
            {
                if (t.Face == Furo_Face.Aba1 | t.Face == Furo_Face.Aba2)
                {
                    dxf.AddFuro(l, x1 - (DLM.vars.Global.EM1.Perfil_U_Largura / 2), parte.Origem_Final.Origem_Y + t.Origem.X, DLM.vars.Global.EM1.Cantoneira_Furo_Diametro, Desenho_Furo.Vista, true, Sentido.Horizontal);
                }
                else
                {
                    dxf.AddFuro(l, x1, parte.Origem_Final.Origem_Y + t.Origem.X, DLM.vars.Global.EM1.Cantoneira_Furo_Diametro, Desenho_Furo.Corte, true, Sentido.Horizontal);

                }
            }
            foreach (var t in parte.Furos_DIR)
            {
                if (t.Face == Furo_Face.Aba1 | t.Face == Furo_Face.Aba2)
                {
                    dxf.AddFuro(l, x2 + (DLM.vars.Global.EM1.Perfil_U_Largura / 2), parte.Origem_Final.Origem_Y + t.Origem.X, DLM.vars.Global.EM1.Cantoneira_Furo_Diametro, Desenho_Furo.Vista, true, Sentido.Horizontal);
                }
                else
                {
                    dxf.AddFuro(l, x2, parte.Origem_Final.Origem_Y + t.Origem.X, DLM.vars.Global.EM1.Cantoneira_Furo_Diametro, Desenho_Furo.Corte, true, Sentido.Horizontal);
                }

            }
        }
    }
}
