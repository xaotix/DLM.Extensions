using Conexoes;
using DLM.desenho;
using DLM.vars;
using netDxf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Conexoes
{
    public static class Extensoes_CanvasNetDXF
    {
        public static netDxf.Entities.TextAlignment Get(this netDxf.Entities.MTextAttachmentPoint value)
        {
            switch (value)
            {
                case netDxf.Entities.MTextAttachmentPoint.TopLeft:
                    return netDxf.Entities.TextAlignment.TopLeft;
                case netDxf.Entities.MTextAttachmentPoint.TopCenter:
                    return netDxf.Entities.TextAlignment.TopCenter;
                case netDxf.Entities.MTextAttachmentPoint.TopRight:
                    return netDxf.Entities.TextAlignment.TopRight;
                case netDxf.Entities.MTextAttachmentPoint.MiddleLeft:
                    return netDxf.Entities.TextAlignment.MiddleLeft;
                case netDxf.Entities.MTextAttachmentPoint.MiddleCenter:
                    return netDxf.Entities.TextAlignment.MiddleCenter;
                case netDxf.Entities.MTextAttachmentPoint.MiddleRight:
                    return netDxf.Entities.TextAlignment.MiddleRight;
                case netDxf.Entities.MTextAttachmentPoint.BottomLeft:
                    return netDxf.Entities.TextAlignment.BottomLeft;
                case netDxf.Entities.MTextAttachmentPoint.BottomCenter:
                    return netDxf.Entities.TextAlignment.BottomCenter;
                case netDxf.Entities.MTextAttachmentPoint.BottomRight:
                    return netDxf.Entities.TextAlignment.BottomRight;
            }
            return netDxf.Entities.TextAlignment.MiddleCenter;
        }
        public static List<UIElement> Render(this DxfDocument dxf, Viewbox viewBox, CanvasDXFType type, bool blocks)
        {
            var drawing = new List<UIElement>();
            try
            {
                viewBox.Child = null;

                double maxy = 0;
                double maxx = 0;

                if (dxf == null) { return new List<UIElement>(); }


                var comp = (dxf.MaxX() - dxf.MinX()).Abs().Round(0);
                var width = (dxf.MaxY() - dxf.MinY()).Abs().Round(0);

                var size_dxf = comp > width ? comp : width;


                var pai = viewBox.Parent as StackPanel;
                pai.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));



                var size_frame = pai.ActualWidth > pai.ActualHeight ? pai.ActualWidth : pai.ActualHeight;

                if (size_frame == 0)
                {
                    size_frame = 350;
                }
                var margin = (size_dxf * .025).Round(2);
                var thick = ((size_dxf + margin * 2) / viewBox.ActualWidth).Round(4);
                var canvas_scale = (size_frame / size_dxf).Round(4);

                var v_width = viewBox.ActualWidth;
                drawing.AddRange(dxf.GetCanvas(out maxx, out maxy, 0, 0, blocks, type));

                var canvas = new Canvas();
                canvas.Width = comp;
                canvas.Height = width;
                canvas.Background = System.Windows.Media.Brushes.Black;

                var scale = new ScaleTransform();
                scale.ScaleY = -1;
                canvas.LayoutTransform = scale;

                foreach (var obj in drawing)
                {
                    try
                    {
                        Adjust(thick, obj, canvas_scale);
                        canvas.Children.Add(obj);
                    }
                    catch (Exception)
                    {
                    }
                }
                viewBox.Child = canvas;
                canvas.SetCache(canvas_scale * 2);

            }
            catch (Exception ex)
            {
                ex.Alerta();
            }

            return drawing;
        }
        public static List<UIElement> GetCanvas(this DxfDocument dxf, double y0 = 0, double x0 = 0)
        {
            double height, width;
            return GetCanvas(dxf, out width, out height, x0, y0, true);
        }
        public static List<UIElement> GetCanvas(this DxfDocument dxf, out double width, out double height, double X0 = 0, double Y0 = 0, bool blocks = false, CanvasDXFType type = CanvasDXFType.Generic)
        {
            if (dxf == null)
            {
                width = 0;
                height = 0;
                ;
                return new List<UIElement> { "Invalid Documment".Label() };
            }
            double thickness = 1;
            var bylayer = AciColor.ByLayer;

            var objects = new List<UIElement>();
            var xs = new List<double>();
            var ys = new List<double>();
            if (type == CanvasDXFType.Medajoist)
            {
                foreach (var txt in dxf.Entities.Texts)
                {
                    var vlr = txt.Value.ToUpper();
                    if ((type == CanvasDXFType.Medajoist) && txt.Value.Contem("PARAFUSO", "VERSION", "NSD", "TIP"))
                    {
                        continue;
                    }
                    xs.Add(txt.Position.X);
                    ys.Add(txt.Position.Y + (txt.Height / 2));
                    ys.Add(txt.Position.Y - (txt.Height / 2));
                }
                foreach (var s in dxf.Entities.Polylines3D)
                {
                    for (int i = 0; i < s.Vertexes.Count; i++)
                    {
                        if (i < s.Vertexes.Count - 1)
                        {
                            var linha = new Line();
                            linha.Stroke = (s.Color.ToString() == bylayer.ToString() ? s.Layer.Color : s.Color).GetCor();
                            linha.X1 = s.Vertexes[i].X;
                            linha.Y1 = s.Vertexes[i].Y;
                            linha.X2 = s.Vertexes[i + 1].X;
                            linha.Y2 = s.Vertexes[i + 1].Y;

                            xs.Add(linha.X1);
                            ys.Add(linha.Y1);

                            xs.Add(linha.X2);
                            ys.Add(linha.Y2);
                        }
                    }
                }
                foreach (var s in dxf.Entities.Polylines2D)
                {
                    for (int i = 0; i < s.Vertexes.Count; i++)
                    {
                        if (i < s.Vertexes.Count - 1)
                        {
                            var linha = new Line();
                            linha.Stroke = (s.Color.ToString() == bylayer.ToString() ? s.Layer.Color : s.Color).GetCor();
                            linha.X1 = s.Vertexes[i].Position.X;
                            linha.Y1 = s.Vertexes[i].Position.Y;
                            linha.X2 = s.Vertexes[i + 1].Position.X;
                            linha.Y2 = s.Vertexes[i + 1].Position.Y;

                            xs.Add(linha.X1);
                            ys.Add(linha.Y1);

                            xs.Add(linha.X2);
                            ys.Add(linha.Y2);
                        }
                    }
                }
                foreach (var s in dxf.Entities.Circles)
                {
                    xs.Add(s.Center.X);
                    ys.Add(s.Center.Y);

                    xs.Add(s.Center.X + s.Radius);
                    xs.Add(s.Center.X - s.Radius);
                    ys.Add(s.Center.Y + s.Radius);
                    ys.Add(s.Center.Y - s.Radius);
                }
                foreach (var s in dxf.Entities.Arcs)
                {
                    xs.Add(s.Center.X);
                    ys.Add(s.Center.Y);
                }
                foreach (var s in dxf.Entities.Lines)
                {
                    var linha = new Line();
                    linha.Stroke = (s.Color.ToString() == bylayer.ToString() ? s.Layer.Color : s.Color).GetCor();
                    linha.X1 = s.StartPoint.X;
                    linha.Y1 = s.StartPoint.Y;
                    linha.X2 = s.EndPoint.X;
                    linha.Y2 = s.EndPoint.Y;
                    xs.Add(linha.X1);
                    ys.Add(linha.Y1);

                    xs.Add(linha.X2);
                    ys.Add(linha.Y2);

                }

                xs = xs.OrderBy(x => x).ToList();
                ys = ys.OrderBy(y => y).ToList();

                if (xs.Count > 0 && ys.Count > 0)
                {
                    X0 = X0 + Math.Abs(xs.First());
                    Y0 = Y0 + Math.Abs(ys.First());
                }


            }
            else
            {
                X0 = X0 + dxf.MinX();
                Y0 = Y0 + dxf.MinY();
            }

            X0 = -X0;
            Y0 = -Y0;


            xs.Clear();
            ys.Clear();

            foreach (var pol in dxf.Entities.Polylines2D)
            {
                var Linhas = pol.GetCanvas(X0, Y0, thickness);
                foreach (Line linha in Linhas)
                {
                    xs.Add(linha.X1);
                    ys.Add(linha.Y1);

                    xs.Add(linha.X2);
                    ys.Add(linha.Y2);
                    objects.Add(linha);
                }
            }
            foreach (var pol in dxf.Entities.Polylines3D)
            {
                var Linhas = pol.GetCanvas(X0, Y0, thickness);
                foreach (Line linha in Linhas)
                {
                    xs.Add(linha.X1);
                    ys.Add(linha.Y1);

                    xs.Add(linha.X2);
                    ys.Add(linha.Y2);
                    objects.Add(linha);
                }
            }
            foreach (var arc in dxf.Entities.Arcs)
            {
                var arc_path = arc.GetCanvas(X0, Y0, thickness);

                objects.Add(arc_path);
            }
            foreach (var circulo in dxf.Entities.Circles)
            {
                if (type == CanvasDXFType.Medajoist)
                {
                    continue;
                }
                var tt = circulo.GetCanvas(X0, Y0, thickness);

                xs.Add(circulo.Center.X + X0);
                ys.Add(circulo.Center.Y + Y0);

                xs.Add(circulo.Center.X + circulo.Radius + X0);
                xs.Add(circulo.Center.X - circulo.Radius + X0);
                ys.Add(circulo.Center.Y + circulo.Radius + Y0);
                ys.Add(circulo.Center.Y - circulo.Radius + Y0);

                objects.Add(tt);

            }
            foreach (var line in dxf.Entities.Lines)
            {
                if (type == CanvasDXFType.Medajoist)
                {
                    if (line.Layer.Name == "CONTORNO3" && line.Color != AciColor.Green)
                    {
                        var angulo = line.Angulo();
                        if (angulo != 0 && angulo != 180 && angulo != 90 && angulo != -90)
                        {
                            continue;
                        }
                    }
                    else if (line.Layer.Name == "0")
                    {
                        continue;
                    }
                }

                var linha = line.GetCanvas(X0, Y0, thickness);

                xs.Add(linha.X1);
                ys.Add(linha.Y1);

                xs.Add(linha.X2);
                ys.Add(linha.Y2);

                objects.Add(linha);
            }
            foreach (var text in dxf.Entities.Texts)
            {
                var vlr = text.Value.ToUpper();
                if ((type == CanvasDXFType.Medajoist) && vlr.Contem("PARAFUSO", "VERSION", "NSD", "TIP"))
                {
                    continue;
                }
                var textBlock = text.GetCanvas(X0, Y0);

                xs.Add(text.Position.X + X0);
                ys.Add(text.Position.Y + Y0);
                objects.Add(textBlock);
            }
            foreach (var ellipse in dxf.Entities.Ellipses)
            {
                if (type == CanvasDXFType.Medajoist)
                {
                    continue;
                }
                var tt = ellipse.GetCanvas(X0, Y0, thickness);

                xs.Add(ellipse.Center.X + X0);
                ys.Add(ellipse.Center.Y + Y0);

                xs.Add(ellipse.Center.X + ellipse.MajorAxis + X0);
                xs.Add(ellipse.Center.X - ellipse.MajorAxis + X0);
                ys.Add(ellipse.Center.Y + ellipse.MajorAxis + Y0);
                ys.Add(ellipse.Center.Y - ellipse.MajorAxis + Y0);

                xs.Add(ellipse.Center.X + ellipse.MinorAxis + X0);
                xs.Add(ellipse.Center.X - ellipse.MinorAxis + X0);
                ys.Add(ellipse.Center.Y + ellipse.MinorAxis + Y0);
                ys.Add(ellipse.Center.Y - ellipse.MinorAxis + Y0);

                objects.Add(tt);

            }


            if (blocks)
            {
                foreach (var bloco in dxf.Entities.Inserts)
                {
                    objects.AddRange(bloco.GetCanvas(X0, Y0, thickness));
                }
            }


            if (xs.Count > 0 && ys.Count > 0)
            {
                if (type == CanvasDXFType.Medajoist)
                {
                    width = xs.Distinct().ToList().OrderBy(x => x).Last();
                    height = ys.Distinct().ToList().OrderBy(x => x).Last();

                }
                else
                {
                    width = xs.Distinct().ToList().OrderBy(x => x).Last();
                    height = ys.Distinct().ToList().OrderBy(x => x).Last();
                }
            }
            else
            {
                width = 0;
                height = 0;
            }



            return objects;
        }
        public static void Adjust(double thick, object t, double scale)
        {
            //Line, Ellipse, Path...
            if (t is Shape)
            {
                var s = (t as Shape);

                s.StrokeThickness = Math.Round(thick, 2);
                s.UseLayoutRounding = true;
            }
            else if (t is System.Windows.Controls.Label)
            {
                var s = (t as System.Windows.Controls.Label);

                var tam = (2.5 / scale).Round(2);
                //if(s.Tag is netDxf.Entities.Text)
                //{
                //   var txt = s.Tag as netDxf.Entities.Text;
                //    tam = txt.Height * scale*2;
                //}
                //else if (s.Tag is netDxf.Entities.MText)
                //{
                //    var txt = s.Tag as netDxf.Entities.MText;
                //    tam = txt.Height *scale*2;
                //}



                //if (tam > 0)
                //{
                //    s.FontSize = Math.Round(tam, 2);
                //}
                //s.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Ideal);
                //s.UseLayoutRounding = true;
            }
            else if (t is Border)
            {
                var s = (t as Border);
                Adjust(thick, s.Child, scale);
            }
            //Grid, Stack Panel, Canvas
            else if (t is Panel)
            {
                var s = (t as Panel);
                foreach (var f in s.Children)
                {
                    Adjust(thick, f, scale);
                }
            }
        }
        private static List<UIElement> GetCanvas(this netDxf.Entities.Insert insert, double X0, double Y0, double thick = 1)
        {
            var objects = new List<UIElement>();
            foreach (var p in insert.Explode())
            {
                if (p is netDxf.Entities.Arc)
                {
                    var n = (p as netDxf.Entities.Arc).GetCanvas(X0, Y0, thick);
                    objects.Add(n);
                }
                else if (p is netDxf.Entities.Circle)
                {
                    var n = (p as netDxf.Entities.Circle).GetCanvas(X0, Y0, thick);
                    objects.Add(n);
                }
                else if (p is netDxf.Entities.Line)
                {
                    var n = (p as netDxf.Entities.Line).GetCanvas(X0, Y0, thick);
                    objects.Add(n);
                }
                else if (p is netDxf.Entities.Polyline3D)
                {
                    var n = (p as netDxf.Entities.Polyline3D).GetCanvas(X0, Y0, thick);
                    objects.AddRange(n);
                }
                else if (p is netDxf.Entities.Polyline2D)
                {
                    var n = (p as netDxf.Entities.Polyline2D).GetCanvas(X0, Y0, thick);
                    objects.AddRange(n);
                }
                else if (p is netDxf.Entities.Text)
                {
                    var n = (p as netDxf.Entities.Text).GetCanvas(X0, Y0);
                    objects.Add(n);
                }
                else if (p is netDxf.Entities.MText)
                {
                    var n = (p as netDxf.Entities.MText).GetCanvas(X0, Y0);
                    objects.Add(n);
                }
                else if (p is netDxf.Entities.Insert)
                {
                    objects.AddRange((p as netDxf.Entities.Insert).GetCanvas(X0, Y0, thick));
                }
            }
            return objects;
        }
        public static List<Line> GetCanvas(this netDxf.Entities.Polyline3D pol, double X0 = 0, double Y0 = 0, double thick = 1)
        {

            var cor = pol.GetCor();
            var lines = new List<Line>();
            for (int i = 0; i < pol.Vertexes.Count; i++)
            {
                if (i < pol.Vertexes.Count - 1)
                {
                    var p1 = new P3d(pol.Vertexes[i].X + X0, pol.Vertexes[i].Y + Y0);
                    var p2 = new P3d(pol.Vertexes[i + 1].X + X0, pol.Vertexes[i + 1].Y + Y0);
                    var linha = p1.Linha(p2, cor, thick);
                    pol.Canvas = linha;
                    linha.Tag = pol;
                    lines.Add(linha);
                }
            }
            return lines;
        }
        public static List<Line> GetCanvas(this netDxf.Entities.Polyline2D obj, double X0 = 0, double Y0 = 0, double thick = 1)
        {

            var cor = obj.GetCor();
            var nObjs = new List<Line>();
            for (int i = 0; i < obj.Vertexes.Count; i++)
            {
                if (i < obj.Vertexes.Count - 1)
                {
                    var p1 = new P3d(obj.Vertexes[i].Position.X + X0, obj.Vertexes[i].Position.Y + Y0);
                    var p2 = new P3d(obj.Vertexes[i + 1].Position.X + X0, obj.Vertexes[i + 1].Position.Y + Y0);
                    var nObj = p1.Linha(p2, cor, thick);
                    obj.Canvas = nObj;
                    nObj.Tag = obj;
                    nObjs.Add(nObj);
                }
            }
            return nObjs;
        }
        public static TipoLinhaCanvas GetTipoLinha(this netDxf.Entities.Line line)
        {
            var lin = line.Linetype;
            if (line.Linetype.Name.ToUpper() == netDxf.Tables.Linetype.ByLayer.Name.ToUpper())
            {
                lin = line.Layer.Linetype;
            }

            if (lin.Name.ToUpper() == netDxf.Tables.Linetype.Dashed.Name.ToUpper())
            {
                return TipoLinhaCanvas.Tracado;
            }
            if (lin.Name.ToUpper() == netDxf.Tables.Linetype.DashDot.Name.ToUpper())
            {
                return TipoLinhaCanvas.Traco_Ponto;
            }

            return TipoLinhaCanvas.Continua;
        }
        public static Line GetCanvas(this netDxf.Entities.Line line, double X0, double Y0, double thick = 1)
        {


            var cor = line.GetCor();
            var tipo = line.GetTipoLinha();

            var p1 = line.StartPoint.ToP3d().MoverX(X0).MoverY(Y0);
            var p2 = line.EndPoint.ToP3d().MoverX(X0).MoverY(Y0);
            var nObj = p1.Linha(p2, cor, thick, tipo);


            //var nObj = new Line();
            //nObj.Stroke = cor;
            //nObj.X1 = line.StartPoint.X + X0;
            //nObj.Y1 = line.StartPoint.Y + Y0;
            //nObj.X2 = line.EndPoint.X + X0;
            //nObj.Y2 = line.EndPoint.Y + Y0;

            //var lin = line.Linetype;
            //if (line.Linetype.Name.ToUpper() == netDxf.Tables.Linetype.ByLayer.Name.ToUpper())
            //{
            //    lin = line.Layer.Linetype;
            //}

            //if (lin.Name.ToUpper() == netDxf.Tables.Linetype.Dashed.Name.ToUpper())
            //{
            //    nObj.StrokeDashArray = new DoubleCollection() { 10, 10 };
            //}
            //if (lin.Name.ToUpper() == netDxf.Tables.Linetype.DashDot.Name.ToUpper())
            //{
            //    nObj.StrokeDashArray = new DoubleCollection() { 10, 2, 2, 2 };
            //}

            //nObj.StrokeThickness = thick;

            //nObj.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            //nObj.VerticalAlignment = VerticalAlignment.Center;
            //nObj.StrokeThickness = thick;

            line.Canvas = nObj;
            nObj.Tag = line;
            nObj.UseLayoutRounding = true;
            return nObj;
        }
        public static Path GetCanvas(this netDxf.Entities.Arc arc, double X0 = 0, double Y0 = 0, double thick = 1)
        {
            var nObj = new Path();


            Canvas.SetLeft(nObj, 0);
            Canvas.SetTop(nObj, 0);

            double start_angle = (((arc.StartAngle.GrausParaRadianos()) % (Math.PI * 2)) + Math.PI * 2) % (Math.PI * 2);
            double end_angle = (((arc.EndAngle.GrausParaRadianos()) % (Math.PI * 2)) + Math.PI * 2) % (Math.PI * 2);


            double angle_diff = end_angle - start_angle;
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure();
            var arcSegment = new ArcSegment();
            arcSegment.IsLargeArc = angle_diff >= Math.PI;
            //Set start of arc
            pathFigure.StartPoint = new System.Windows.Point(arc.Center.X + X0 + arc.Radius * Math.Cos(start_angle), arc.Center.Y + Y0 + arc.Radius * Math.Sin(start_angle));
            //set end point of arc.
            arcSegment.Point = new System.Windows.Point(arc.Center.X + X0 + arc.Radius * Math.Cos(end_angle), arc.Center.Y + Y0 + arc.Radius * Math.Sin(end_angle));
            arcSegment.Size = new System.Windows.Size(arc.Radius, arc.Radius);
            arcSegment.SweepDirection = SweepDirection.Clockwise;

            pathFigure.Segments.Add(arcSegment);
            pathGeometry.Figures.Add(pathFigure);
            nObj.Data = pathGeometry;
            nObj.StrokeThickness = thick;
            nObj.Stroke = arc.GetCor();

            nObj.ToolTip = arc;
            arc.Canvas = nObj;
            nObj.Tag = arc;
            nObj.UseLayoutRounding = true;
            return nObj;
        }
        public static Ellipse GetCanvas(this netDxf.Entities.Ellipse ell, double X0 = 0, double Y0 = 0, double thick = 1)
        {
            var nObj = new Ellipse();
            nObj.Width = ell.MajorAxis;
            nObj.Height = ell.MinorAxis;
            nObj.StrokeThickness = thick;
            Canvas.SetLeft(nObj, ell.Center.X + X0 - (ell.MajorAxis / 2));
            Canvas.SetTop(nObj, ell.Center.Y + Y0 - (ell.MinorAxis / 2));
            nObj.Stroke = ell.GetCor();
            //tt.ToolTip = s;
            nObj.Tag = ell;
            ell.Canvas = nObj;
            nObj.UseLayoutRounding = true;
            return nObj;
        }
        public static Ellipse GetCanvas(this netDxf.Entities.Circle circle, double X0 = 0, double Y0 = 0, double thick = 1)
        {
            var nObj = new Ellipse();
            nObj.Width = circle.Radius * 2;
            nObj.Height = circle.Radius * 2;
            nObj.StrokeThickness = thick;
            Canvas.SetLeft(nObj, circle.Center.X + X0 - circle.Radius);
            Canvas.SetTop(nObj, circle.Center.Y + Y0 - circle.Radius);
            nObj.Stroke = circle.GetCor();
            //tt.ToolTip = s;
            circle.Canvas = nObj;
            nObj.Tag = circle;
            nObj.UseLayoutRounding = true;
            return nObj;
        }
        public static Border GetCanvas(this netDxf.Entities.MText text, double X0 = 0, double Y0 = 0)
        {
            return GetText(text as netDxf.Entities.EntityObject, X0, Y0);
        }
        public static Border GetCanvas(this netDxf.Entities.Text text, double X0 = 0, double Y0 = 0)
        {
            return GetText(text as netDxf.Entities.EntityObject, X0, Y0);

        }
        private static Border GetText(netDxf.Entities.EntityObject entity, double X0 = 0, double Y0 = 0)
        {
            if (entity is netDxf.Entities.Text | entity is netDxf.Entities.MText)
            {
                var Cor = entity.GetCor();
                //SolidColorBrush Cor = Brushes.Cyan;

                var label = new System.Windows.Controls.Label();

                label.FontFamily = new FontFamily("Monospace");
                label.Foreground = Cor;
                label.VerticalAlignment = VerticalAlignment.Center;
                label.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                label.SetValue(Label.FontWeightProperty, FontWeights.Bold);
                label.UseLayoutRounding = true;



                var TextAlignment = netDxf.Entities.TextAlignment.MiddleCenter;
                var Position = new Vector3();
                var Rotation = 0.0;

                if (entity is netDxf.Entities.Text)
                {
                    var txt = entity as netDxf.Entities.Text;
                    Position = txt.Position;
                    Rotation = txt.Rotation;
                    label.Content = txt.Value;
                    label.FontSize = txt.Height;
                    TextAlignment = txt.Alignment;
                }
                else if (entity is netDxf.Entities.MText)
                {
                    var txt = entity as netDxf.Entities.MText;
                    Position = txt.Position;
                    Rotation = txt.Rotation;
                    label.Content = txt.PlainText();
                    label.FontSize = txt.Height;
                    TextAlignment = txt.AttachmentPoint.Get();
                }







                var nObj = new Border();
                var vert = VerticalAlignment.Center;
                var horiz = HorizontalAlignment.Center;
                GetAlignment(TextAlignment, out horiz, out vert);
                nObj.VerticalAlignment = vert;
                nObj.HorizontalAlignment = horiz;
                nObj.UseLayoutRounding = true;

                nObj.Child = label;

                var group = new TransformGroup();

                var flipTrans = new ScaleTransform();
                flipTrans.ScaleY = -1;

                group.Children.Add(new RotateTransform(-Rotation));
                group.Children.Add(flipTrans);

                nObj.RenderTransform = group;
                nObj.Background = System.Windows.Media.Brushes.Black;

                var medidas = nObj.Medir();


                var norigem = new P3d((Position.X + X0), (Position.Y + Y0));

                if (nObj.HorizontalAlignment == HorizontalAlignment.Center)
                {
                    norigem = norigem.Mover(Rotation, -(nObj.DesiredSize.Width / 2));
                }
                else if (nObj.HorizontalAlignment == HorizontalAlignment.Left)
                {
                    norigem = norigem.Mover(Rotation, 0);
                }
                else if (nObj.HorizontalAlignment == HorizontalAlignment.Right)
                {
                    norigem = norigem.Mover(Rotation, (-nObj.DesiredSize.Width));
                }

                if (nObj.VerticalAlignment == VerticalAlignment.Center)
                {
                    norigem = norigem.Mover(Rotation - 90, -(nObj.DesiredSize.Height / 2));
                }

                Canvas.SetLeft(nObj, norigem.X);
                Canvas.SetTop(nObj, norigem.Y);




                entity.Canvas = nObj;
                nObj.Tag = entity;
                label.Tag = entity;


                return nObj;
            }

            return null;

        }

        public static void GetAlignment(this netDxf.Entities.TextAlignment TextAlignment, out HorizontalAlignment HorizontalAlignment, out VerticalAlignment VerticalAlignment)
        {
            VerticalAlignment = VerticalAlignment.Center;
            HorizontalAlignment = HorizontalAlignment.Center;
            switch (TextAlignment)
            {
                case netDxf.Entities.TextAlignment.TopLeft:
                    VerticalAlignment = VerticalAlignment.Top;
                    HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case netDxf.Entities.TextAlignment.TopCenter:
                    VerticalAlignment = VerticalAlignment.Top;
                    HorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case netDxf.Entities.TextAlignment.TopRight:
                    VerticalAlignment = VerticalAlignment.Top;
                    HorizontalAlignment = HorizontalAlignment.Right;
                    break;
                case netDxf.Entities.TextAlignment.MiddleLeft:
                    VerticalAlignment = VerticalAlignment.Center;
                    HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case netDxf.Entities.TextAlignment.MiddleCenter:
                    VerticalAlignment = VerticalAlignment.Center;
                    HorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case netDxf.Entities.TextAlignment.MiddleRight:
                    VerticalAlignment = VerticalAlignment.Center;
                    HorizontalAlignment = HorizontalAlignment.Right;
                    break;
                case netDxf.Entities.TextAlignment.BottomLeft:
                    VerticalAlignment = VerticalAlignment.Bottom;
                    HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case netDxf.Entities.TextAlignment.BottomCenter:
                    VerticalAlignment = VerticalAlignment.Bottom;
                    HorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case netDxf.Entities.TextAlignment.BottomRight:
                    VerticalAlignment = VerticalAlignment.Bottom;
                    HorizontalAlignment = HorizontalAlignment.Right;
                    break;
                case netDxf.Entities.TextAlignment.BaselineLeft:
                    VerticalAlignment = VerticalAlignment.Center;
                    HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case netDxf.Entities.TextAlignment.BaselineCenter:
                    VerticalAlignment = VerticalAlignment.Center;
                    HorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case netDxf.Entities.TextAlignment.BaselineRight:
                    VerticalAlignment = VerticalAlignment.Center;
                    HorizontalAlignment = HorizontalAlignment.Right;
                    break;
                case netDxf.Entities.TextAlignment.Aligned:
                    VerticalAlignment = VerticalAlignment.Center;
                    HorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case netDxf.Entities.TextAlignment.Middle:
                    VerticalAlignment = VerticalAlignment.Center;
                    HorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case netDxf.Entities.TextAlignment.Fit:
                    VerticalAlignment = VerticalAlignment.Center;
                    HorizontalAlignment = HorizontalAlignment.Center;
                    break;
            }
        }

        public static SolidColorBrush GetCor(this netDxf.Entities.EntityObject entity)
        {
            var Cor = Brushes.White;

            if (entity.Color.IsByLayer)
            {
                Cor = (entity.Layer.Color).GetCor();
            }
            else if (entity.Color.IsByBlock)
            {

            }
            else
            {
                Cor = (entity.Color).GetCor();
            }

            var cl = Cor.Color;
            if (cl.R == 0 && cl.G == 0 && cl.B == 0)
            {
                Cor = Brushes.White;
            }

            return Cor;
        }
    }
}
