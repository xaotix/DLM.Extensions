using Conexoes;
using DLM.cam;
using DLM.desenho;
using DLM.vars;
using netDxf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Conexoes
{

    public static class ExtensoesCanvas
    {
        public static List<P3d> GetContorno(this System.Windows.Media.Media3D.Rect3D rec, P3d origem = null, bool fechar = true)
        {
            if (origem == null)
            {
                origem = new P3d();
            }
            var retorno = new List<P3d>();

            var cor = Colors.Blue;

            retorno.Add(new P3d(rec.X + origem.X, rec.Y + origem.Y));
            retorno.Add(new P3d(rec.X + origem.X + rec.SizeX, rec.Y + origem.Y));
            retorno.Add(new P3d(rec.X + origem.X + rec.SizeX, rec.Y + origem.Y + rec.SizeY));
            retorno.Add(new P3d(rec.X + origem.X, rec.Y + origem.Y + rec.SizeY));

            if (fechar)
            {
                retorno.Add(new P3d(rec.X + origem.X, rec.Y + origem.Y));
            }


            return retorno;
        }

        public static System.Windows.Media.Media3D.Rect3D Offset(this System.Windows.Media.Media3D.Rect3D contorno, double offset, P3d origem = null)
        {
            if (origem == null)
            {
                origem = new P3d();
            }
            var offset_contorno = contorno.GetContorno(origem).ToLiv().Offset(offset).ToP3d();
            var min = offset_contorno.Min();
            var max = offset_contorno.Max();

            var nRec = new System.Windows.Media.Media3D.Rect3D(min.X, min.Y, min.Z, min.DistanciaX(max).Abs(), min.DistanciaY(max).Abs(), min.DistanciaZ(max).Abs());

            return nRec;
        }


        public static P3d Medir(this FrameworkElement uIElement)
        {
            if (uIElement == null) { return new P3d(); }
            uIElement.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            return new P3d(uIElement.ActualWidth, uIElement.ActualHeight);
        }
        public static P3d Medir(this Canvas uIElement)
        {
            if (uIElement == null) { return new P3d(); }
            uIElement.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            return new P3d(uIElement.ActualWidth, uIElement.ActualHeight);
        }
        public static void SetCache(this Canvas Desenho_Corte, double RenderScale)
        {
            RenderOptions.SetBitmapScalingMode(Desenho_Corte, BitmapScalingMode.HighQuality);
            var bm = new BitmapCache(RenderScale);
            bm.EnableClearType = false;
            Desenho_Corte.CacheMode = bm;
        }
        public static SolidColorBrush GetCor(this AciColor Cor)
        {
            var c = Cor.ToColor();
            return new SolidColorBrush(System.Windows.Media.Color.FromRgb(c.R, c.G, c.B));
        }
        public static System.Windows.Point GetPosicao(this UIElement elemento)
        {
            var s = VisualTreeHelper.GetOffset(elemento);
            return new System.Windows.Point(s.X, s.Y);
        }
        public static List<UIElement> GetRange(this List<UIElement> lista, P3d min, P3d max)
        {
            var retorno = new List<UIElement>();
            foreach (var ui in lista)
            {
                var pos = ui.GetPosicao();
                if (pos.X >= min.X && pos.X <= max.X && pos.Y >= min.Y && pos.Y <= max.Y)
                {
                    retorno.Add(ui);
                }
            }
            return retorno;
        }
        public static void GetBordas(this Border LayoutRoot, out P3d topoesquerdo, out P3d baixodireito, Canvas parent)
        {
            var topLeft = LayoutRoot.TransformToAncestor(parent).Transform(new System.Windows.Point(0, 0));
            var rightBottom = new P3d(topLeft.X + LayoutRoot.ActualWidth, topLeft.Y + LayoutRoot.ActualHeight);

            baixodireito = rightBottom;
            topoesquerdo = topLeft.P3d();
        }
        public static void TrazerPraFrente(this UIElement element, Canvas pParent = null)
        {
            try
            {
                if (pParent == null)
                {
                    if (element is FrameworkElement)
                    {
                        var obj = element as FrameworkElement;
                        pParent = obj.Parent as Canvas;
                    }

                }


                var maxZ = pParent.Children.OfType<UIElement>()
        .Where(x => x != element)
        .Select(x => Panel.GetZIndex(x))
        .Max();
                Panel.SetZIndex(element, maxZ + 1);
            }
            catch (Exception)
            {
            }
        }
        public static Ellipse Circulo(this P3d p1, double raio, double espessura_linha, SolidColorBrush cor, bool background = true)
        {
            Ellipse nObj = new Ellipse();
            nObj.Width = raio * 2;
            nObj.Height = raio * 2;
            nObj.StrokeThickness = espessura_linha;
            Canvas.SetLeft(nObj, p1.X - raio);
            Canvas.SetTop(nObj, p1.Y - raio);
            nObj.Stroke = cor;

            if (background)
            {
                nObj.Fill = System.Windows.Media.Brushes.Black;
            }
            nObj.UseLayoutRounding = true;
            return nObj;
        }
        public static Border Texto(this string texto, System.Windows.Point p1, SolidColorBrush cor, double tamanho, double angulo = 0, double contorno = 1, double cornerRadius = 0)
        {
            var nObj = new Border();
            //b.VerticalAlignment = VerticalAlignment.Center;
            //b.HorizontalAlignment = HorizontalAlignment.Center;




            System.Windows.Controls.Label textBlock = new System.Windows.Controls.Label();

            textBlock.FontFamily = new System.Windows.Media.FontFamily("RomanS");
            textBlock.Content = texto;
            textBlock.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            textBlock.Foreground = cor;
            textBlock.Background = System.Windows.Media.Brushes.Black;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            textBlock.UseLayoutRounding = true;

            //switch (s.Alignment)


            textBlock.FontSize = tamanho;
            textBlock.Margin = new Thickness(1);

            nObj.Child = textBlock;


            //textBlock.RenderTransform = group;
            nObj.RenderTransform = GetTransformTexto(angulo);
            nObj.BorderBrush = cor;
            nObj.Background = System.Windows.Media.Brushes.Black;

            nObj.BorderThickness = new Thickness(contorno);
            nObj.CornerRadius = new CornerRadius(cornerRadius);


            nObj.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));

            var norigem = new P3d(p1.X, p1.Y).Mover(angulo, -(nObj.DesiredSize.Width / 2)); ;
            norigem = norigem.Mover(angulo - 90, -(nObj.DesiredSize.Height / 2));
            Canvas.SetLeft(nObj, norigem.X);
            Canvas.SetTop(nObj, norigem.Y);


            nObj.UseLayoutRounding = true;
            return nObj;
        }
        public static Button Botao(this string texto, System.Windows.Point p1, SolidColorBrush cor, double tamanho, double angulo = 0, double contorno = 0.5, SolidColorBrush background = null)
        {
            var b = new Button();
            b.Content = texto;
            b.Margin = new Thickness(contorno);
            b.Background = System.Windows.Media.Brushes.Black;
            b.Foreground = cor;
            b.FontFamily = new System.Windows.Media.FontFamily("RomanS");
            b.RenderTransform = GetTransformTexto(angulo);
            b.BorderThickness = new Thickness(contorno);
            b.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));


            if (background != null)
            {
                b.Background = background;
            }


            var norigem = new P3d(p1.X, p1.Y).Mover(angulo, -(b.DesiredSize.Width / 2)); ;
            norigem = norigem.Mover(angulo - 90, -(b.DesiredSize.Height / 2));
            Canvas.SetLeft(b, norigem.X);
            Canvas.SetTop(b, norigem.Y);
            return b;
        }
        private static TransformGroup GetTransformTexto(this double angulo)
        {
            TransformGroup group = new TransformGroup();
            ScaleTransform flipTrans = new ScaleTransform();
            flipTrans.ScaleY = -1;

            if (angulo != 0)
            {
                group.Children.Add(new RotateTransform(-angulo));
            }
            group.Children.Add(flipTrans);

            return group;
        }
        public static void SetCor(this UIElement obj, SolidColorBrush cor, SolidColorBrush background = null)
        {
            if (background == null)
            {
                background = System.Windows.Media.Brushes.Black;
            }
            if (obj is null)
            {
                return;
            }
            if (obj is Border)
            {
                var border = obj as Border;
                border.BorderBrush = cor;
                border.Background = background;

                var s = border.Child;

                if (s is Label)
                {
                    var b = s as Label;
                    b.Foreground = cor;
                    b.Background = background;
                }
                else if (s is TextBlock)
                {
                    var b = s as TextBlock;
                    b.Foreground = cor;
                    b.Background = background;
                }
            }
            else if (obj is Label)
            {
                var b = obj as Label;
                b.Background = background;
                b.Foreground = cor;
            }
            else if (obj is Button)
            {
                var b = obj as Button;
                b.Background = background;
                b.Foreground = cor;
            }
            else if (obj is TextBlock)
            {
                var b = obj as TextBlock;
                b.Background = background;
                b.Foreground = cor;
            }
            else if (obj is Line)
            {
                var b = obj as Line;
                b.Stroke = cor;
            }

        }
        public static Label Label(this string Texto, P3d p1 = null, SolidColorBrush cor = null, double Tamanho = 10, double angulo = 0)
        {
            if (p1 == null)
            {
                p1 = new P3d();
            }

            if (cor == null)
            {
                cor = System.Windows.Media.Brushes.Black;
            }

            var nObj = new System.Windows.Controls.Label();
            nObj.FontSize = Tamanho;

            nObj.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            nObj.Content = Texto;
            nObj.Foreground = cor;
            nObj.Background = System.Windows.Media.Brushes.Black;

            nObj.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));

            var norigem = new P3d(p1.X, p1.Y).Mover(angulo, -(nObj.DesiredSize.Width / 2)); ;
            norigem = norigem.Mover(angulo - 90, -(nObj.DesiredSize.Height / 2));
            Canvas.SetLeft(nObj, norigem.X);
            Canvas.SetTop(nObj, norigem.Y);



            nObj.RenderTransform = GetTransformTexto(angulo);
            nObj.UseLayoutRounding = true;

            return nObj;
        }
        public static Line Linha(this P3d p1, P3d p2, SolidColorBrush cor = null, double espessura_linha = 1, TipoLinhaCanvas tipo = TipoLinhaCanvas.Continua)
        {
            if (cor == null)
            {
                cor = System.Windows.Media.Brushes.Black;
            }
            if (espessura_linha <= 0)
            {
                espessura_linha = 1;
            }
            var nObj = new Line();
            nObj.Stroke = cor;
            nObj.X1 = p1.X;
            nObj.Y1 = p1.Y;
            nObj.X2 = p2.X;
            nObj.Y2 = p2.Y;



            if (tipo == TipoLinhaCanvas.Tracado)
            {
                nObj.StrokeDashArray = new DoubleCollection() { 10 * espessura_linha, 10 * espessura_linha };
            }
            if (tipo == TipoLinhaCanvas.Traco_Ponto)
            {
                nObj.StrokeDashArray = new DoubleCollection() { 10 * espessura_linha, 2 * espessura_linha, 2 * espessura_linha, 2 * espessura_linha };
            }


            nObj.StrokeThickness = espessura_linha;

            nObj.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            nObj.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            nObj.StrokeThickness = espessura_linha;
            nObj.UseLayoutRounding = true;
            return nObj;
        }
    }
}
