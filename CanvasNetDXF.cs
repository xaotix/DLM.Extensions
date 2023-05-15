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
        public static List<UIElement> Render(this DxfDocument dxf, Viewbox view, CanvasDXFType type, bool blocks)
        {
            var drawing = new List<UIElement>();
            try
            {
                view.Child = null;

                double maxy = 0;
                double maxx = 0;

                if (dxf == null) { return new List<UIElement>(); }

                view.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                view.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

                var comp = (dxf.MaxX() - dxf.MinX()).Abs();
                var width = (dxf.MaxY() - dxf.MinY()).Abs();

                var size_dxf = comp > width ? comp : width;
                
                if (view.ActualWidth == 0 | view.ActualHeight == 0)
                {
                    var w = 0.0;
                    var h = 0.0;
                    var parent = view.Parent;
                    if (parent != null)
                    {
                        var fme = (FrameworkElement)view.Parent;
                        w = fme.ActualWidth.Round(0) - view.Margin.Left - view.Margin.Right;
                        h = fme.ActualHeight.Round(0) - view.Margin.Top - view.Margin.Bottom;
                    }

                    if (w <= 0)
                    {
                        w = 350;
                    }
                    if (h <= 0)
                    {
                        h = 350;
                    }

                    view.Width = w;
                    view.Height = h;

                }

                var size_frame = view.ActualWidth>view.ActualHeight?view.ActualWidth:view.ActualHeight;
                var margin = (size_dxf * .025).Round(2);
                var thick = ((size_dxf + margin * 2) / view.ActualWidth).Round(4);
                var canvas_scale = (size_frame / size_dxf).Round(4);

                var v_width = view.ActualWidth;
                drawing.AddRange(dxf.GetCanvas(out maxx, out maxy, 0,0,blocks, type));

                var canvas = new Canvas();
                canvas.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                canvas.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                canvas.Width = comp;
                canvas.Height = width;
                canvas.Margin = new Thickness(margin);
                var scale = new ScaleTransform();
                scale.ScaleY = -1;
                canvas.LayoutTransform = scale;
                canvas.UpdateLayout();

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
                view.Child = canvas;
            }
            catch (Exception ex)
            {
                Utilz.Alerta(ex);
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
                foreach (var s in dxf.Texts)
                {
                    if ((type == CanvasDXFType.Medajoist) && (
                        s.Value.Contains("PARAFUSO")
                     || s.Value.ToUpper().Contains("VERSION")
                     || s.Value.ToUpper().Contains("NSD")
                     || s.Value.ToUpper().Contains("TIP")
                     )
                     )
                    {
                        continue;
                    }
                    xs.Add(s.Position.X);
                    ys.Add(s.Position.Y + (s.Height / 2));
                    ys.Add(s.Position.Y - (s.Height / 2));
                }
                foreach (var s in dxf.Polylines)
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
                foreach (var s in dxf.Circles)
                {
                    xs.Add(s.Center.X);
                    ys.Add(s.Center.Y);

                    xs.Add(s.Center.X + s.Radius);
                    xs.Add(s.Center.X - s.Radius);
                    ys.Add(s.Center.Y + s.Radius);
                    ys.Add(s.Center.Y - s.Radius);
                }
                foreach (var s in dxf.Arcs)
                {
                    xs.Add(s.Center.X);
                    ys.Add(s.Center.Y);
                }
                foreach (var s in dxf.Lines)
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

            foreach (var pol in dxf.Polylines)
            {
                var Linhas = pol.GetCanvas(X0, Y0, thickness, bylayer);
                foreach (Line linha in Linhas)
                {
                    xs.Add(linha.X1);
                    ys.Add(linha.Y1);

                    xs.Add(linha.X2);
                    ys.Add(linha.Y2);
                    objects.Add(linha);
                }
            }
            foreach (var arc in dxf.Arcs)
            {
                var arc_path = arc.GetCanvas(X0, Y0, thickness, bylayer);

                objects.Add(arc_path);
            }
            foreach (var circulo in dxf.Circles)
            {
                if (type == CanvasDXFType.Medajoist)
                {
                    continue;
                }
                var tt = circulo.GetCanvas(X0, Y0, thickness, bylayer);

                xs.Add(circulo.Center.X + X0);
                ys.Add(circulo.Center.Y + Y0);

                xs.Add(circulo.Center.X + circulo.Radius + X0);
                xs.Add(circulo.Center.X - circulo.Radius + X0);
                ys.Add(circulo.Center.Y + circulo.Radius + Y0);
                ys.Add(circulo.Center.Y - circulo.Radius + Y0);

                objects.Add(tt);

            }
            foreach (var line in dxf.Lines)
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

                var linha = line.GetCanvas(X0, Y0, thickness, bylayer);

                xs.Add(linha.X1);
                ys.Add(linha.Y1);

                xs.Add(linha.X2);
                ys.Add(linha.Y2);

                objects.Add(linha);
            }
            foreach (var text in dxf.Texts)
            {
                if ((type == CanvasDXFType.Medajoist) && (
                    text.Value.Contains("PARAFUSO")
                 || text.Value.ToUpper().Contains("VERSION")
                 || text.Value.ToUpper().Contains("NSD")
                 || text.Value.ToUpper().Contains("TIP")
                 )
                 )
                {
                    continue;
                }
                var textBlock = text.GetCanvas(X0, Y0);

                xs.Add(text.Position.X + X0);
                ys.Add(text.Position.Y + Y0);
                objects.Add(textBlock);
            }
            foreach (var ellipse in dxf.Ellipses)
            {
                if (type == CanvasDXFType.Medajoist)
                {
                    continue;
                }
                var tt = ellipse.GetCanvas(X0, Y0, thickness, bylayer);

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
                foreach (var bloco in dxf.Inserts)
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

                var tam = (2.5/scale).Round(2);
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



                if (tam > 0)
                {
                    s.FontSize = Math.Round(tam, 2);
                }
                s.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Ideal);
                s.UseLayoutRounding = true;
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
                    var n = (p as netDxf.Entities.Arc).GetCanvas(X0, Y0, thick, p.Color);
                    objects.Add(n);
                }
                else if (p is netDxf.Entities.Circle)
                {
                    var n = (p as netDxf.Entities.Circle).GetCanvas(X0, Y0, thick, p.Color);
                    objects.Add(n);
                }
                else if (p is netDxf.Entities.Line)
                {
                    var n = (p as netDxf.Entities.Line).GetCanvas(X0, Y0, thick, p.Color);
                    objects.Add(n);
                }
                else if (p is netDxf.Entities.Polyline)
                {
                    var n = (p as netDxf.Entities.Polyline).GetCanvas(X0, Y0, thick, p.Color);
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
        public static List<Line> GetCanvas(this netDxf.Entities.Polyline pol, double X0 = 0, double Y0 = 0, double thick = 1, netDxf.AciColor bylayer = null)
        {
            if (bylayer == null)
            {
                bylayer = netDxf.AciColor.ByLayer;
            }
            var cor = (pol.Color.ToString() == bylayer.ToString() ? pol.Layer.Color : pol.Color).GetCor();
            var lines = new List<Line>();
            for (int i = 0; i < pol.Vertexes.Count; i++)
            {
                if (i < pol.Vertexes.Count - 1)
                {
                    var p1 = new P3d(pol.Vertexes[i].Position.X + X0, pol.Vertexes[i].Position.Y + Y0);
                    var p2 = new P3d(pol.Vertexes[i + 1].Position.X + X0, pol.Vertexes[i + 1].Position.Y + Y0);
                    var linha = p1.Linha(p2, cor, thick);
                    pol.Canvas = linha;
                    linha.Tag = pol;
                    lines.Add(linha);
                }
            }
            return lines;
        }
        public static Line GetCanvas(this netDxf.Entities.Line line, double X0, double Y0, double thick, netDxf.AciColor bylayer = null)
        {
            if (bylayer == null)
            {
                bylayer = netDxf.AciColor.ByLayer;
            }
            var nLine = new Line();
            nLine.Stroke = (line.Color.ToString() == bylayer.ToString() ? line.Layer.Color : line.Color).GetCor();
            nLine.X1 = line.StartPoint.X + X0;
            nLine.Y1 = line.StartPoint.Y + Y0;
            nLine.X2 = line.EndPoint.X + X0;
            nLine.Y2 = line.EndPoint.Y + Y0;

            var lin = line.Linetype;
            if (line.Linetype.Name.ToUpper() == netDxf.Tables.Linetype.ByLayer.Name.ToUpper())
            {
                lin = line.Layer.Linetype;
            }

            if (lin.Name.ToUpper() == netDxf.Tables.Linetype.Dashed.Name.ToUpper())
            {
                nLine.StrokeDashArray = new DoubleCollection() { 10, 10 };
            }
            if (lin.Name.ToUpper() == netDxf.Tables.Linetype.DashDot.Name.ToUpper())
            {
                nLine.StrokeDashArray = new DoubleCollection() { 10, 2, 2, 2 };
            }

            if (line.Linetype.ToString() == netDxf.Tables.Linetype.Dashed.ToString())
            {
                nLine.StrokeDashArray = new DoubleCollection() { 10, 3 };
            }
            else if (line.Linetype.ToString() == netDxf.Tables.Linetype.DashDot.ToString())
            {
                nLine.StrokeDashArray = new DoubleCollection() { 10, 3, 3 };
            }

            nLine.StrokeThickness = thick;

            nLine.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            nLine.VerticalAlignment = VerticalAlignment.Center;
            nLine.StrokeThickness = thick;
            //linha.ToolTip = s;
            line.Canvas = nLine;
            nLine.Tag = line;
            return nLine;
        }
        public static Path GetCanvas(this netDxf.Entities.Arc arc, double X0 = 0, double Y0 = 0, double thick = 1, netDxf.AciColor bylayer = null)
        {
            if (bylayer == null)
            {
                bylayer = netDxf.AciColor.ByLayer;
            }
            var arc_path = new Path();

            Canvas.SetLeft(arc_path, 0);
            Canvas.SetTop(arc_path, 0);

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
            arc_path.Data = pathGeometry;
            arc_path.StrokeThickness = thick;
            arc_path.Stroke = (arc.Color.ToString() == bylayer.ToString() ? arc.Layer.Color : arc.Color).GetCor();

            arc_path.ToolTip = arc;
            arc.Canvas = arc_path;
            arc_path.Tag = arc;
            return arc_path;
        }
        public static Ellipse GetCanvas(this netDxf.Entities.Ellipse ell, double X0 = 0, double Y0 = 0, double thick = 1, netDxf.AciColor bylayer = null)
        {
            if (bylayer == null)
            {
                bylayer = netDxf.AciColor.ByLayer;
            }
            var tt = new Ellipse();
            tt.Width = ell.MajorAxis;
            tt.Height = ell.MinorAxis;
            tt.StrokeThickness = thick;
            Canvas.SetLeft(tt, ell.Center.X + X0 - (ell.MajorAxis / 2));
            Canvas.SetTop(tt, ell.Center.Y + Y0 - (ell.MinorAxis / 2));
            tt.Stroke = (ell.Color.ToString() == bylayer.ToString() ? ell.Layer.Color : ell.Color).GetCor();
            //tt.ToolTip = s;
            tt.Tag = ell;
            ell.Canvas = tt;
            return tt;
        }
        public static Ellipse GetCanvas(this netDxf.Entities.Circle circle, double X0 = 0, double Y0 = 0, double thick = 1, netDxf.AciColor bylayer = null)
        {
            if (bylayer == null)
            {
                bylayer = netDxf.AciColor.ByLayer;
            }
            var tt = new Ellipse();
            tt.Width = circle.Radius * 2;
            tt.Height = circle.Radius * 2;
            tt.StrokeThickness = thick;
            Canvas.SetLeft(tt, circle.Center.X + X0 - circle.Radius);
            Canvas.SetTop(tt, circle.Center.Y + Y0 - circle.Radius);
            tt.Stroke = (circle.Color.ToString() == bylayer.ToString() ? circle.Layer.Color : circle.Color).GetCor();
            //tt.ToolTip = s;
            circle.Canvas = tt;
            tt.Tag = circle;
            return tt;
        }
        private static Border GetText(netDxf.Entities.EntityObject entity,double X0 =0, double Y0 = 0)
        {
            if(entity is netDxf.Entities.Text | entity is netDxf.Entities.MText)
            {
                string Value = "";
                double Height = 11.0;

                var TextAlignment = netDxf.Entities.TextAlignment.MiddleCenter;
                var Position = new Vector3();
                var Rotation = 0.0;

                if (entity is netDxf.Entities.Text)
                {
                    var txt = entity as netDxf.Entities.Text;
                    TextAlignment = txt.Alignment;
                    Position = txt.Position;
                    Rotation = txt.Rotation;
                    Value = txt.Value;
                    Height = txt.Height;
                }
                else if (entity is netDxf.Entities.MText)
                {
                    var txt = entity as netDxf.Entities.MText;
                    //TextAlignment = txt.Alignment;
                    Position = txt.Position;
                    Rotation = txt.Rotation;
                    Value = txt.PlainText();
                    Height = txt.Height;
                    TextAlignment = txt.AttachmentPoint.Get();
                }



                var border = new Border();
                border.VerticalAlignment = VerticalAlignment.Center;
                border.HorizontalAlignment = HorizontalAlignment.Center;



                switch (TextAlignment)
                {
                    case netDxf.Entities.TextAlignment.TopLeft:
                        border.VerticalAlignment = VerticalAlignment.Top;
                        border.HorizontalAlignment = HorizontalAlignment.Left;
                        break;
                    case netDxf.Entities.TextAlignment.TopCenter:
                        border.VerticalAlignment = VerticalAlignment.Top;
                        border.HorizontalAlignment = HorizontalAlignment.Center;
                        break;
                    case netDxf.Entities.TextAlignment.TopRight:
                        border.VerticalAlignment = VerticalAlignment.Top;
                        border.HorizontalAlignment = HorizontalAlignment.Right;
                        break;
                    case netDxf.Entities.TextAlignment.MiddleLeft:
                        border.VerticalAlignment = VerticalAlignment.Center;
                        border.HorizontalAlignment = HorizontalAlignment.Left;
                        break;
                    case netDxf.Entities.TextAlignment.MiddleCenter:
                        border.VerticalAlignment = VerticalAlignment.Center;
                        border.HorizontalAlignment = HorizontalAlignment.Center;
                        break;
                    case netDxf.Entities.TextAlignment.MiddleRight:
                        border.VerticalAlignment = VerticalAlignment.Center;
                        border.HorizontalAlignment = HorizontalAlignment.Right;
                        break;
                    case netDxf.Entities.TextAlignment.BottomLeft:
                        border.VerticalAlignment = VerticalAlignment.Bottom;
                        border.HorizontalAlignment = HorizontalAlignment.Left;
                        break;
                    case netDxf.Entities.TextAlignment.BottomCenter:
                        border.VerticalAlignment = VerticalAlignment.Bottom;
                        border.HorizontalAlignment = HorizontalAlignment.Center;
                        break;
                    case netDxf.Entities.TextAlignment.BottomRight:
                        border.VerticalAlignment = VerticalAlignment.Bottom;
                        border.HorizontalAlignment = HorizontalAlignment.Right;
                        break;
                    case netDxf.Entities.TextAlignment.BaselineLeft:
                        border.VerticalAlignment = VerticalAlignment.Center;
                        border.HorizontalAlignment = HorizontalAlignment.Left;
                        break;
                    case netDxf.Entities.TextAlignment.BaselineCenter:
                        border.VerticalAlignment = VerticalAlignment.Center;
                        border.HorizontalAlignment = HorizontalAlignment.Center;
                        break;
                    case netDxf.Entities.TextAlignment.BaselineRight:
                        border.VerticalAlignment = VerticalAlignment.Center;
                        border.HorizontalAlignment = HorizontalAlignment.Right;
                        break;
                    case netDxf.Entities.TextAlignment.Aligned:
                        border.VerticalAlignment = VerticalAlignment.Center;
                        border.HorizontalAlignment = HorizontalAlignment.Center;
                        break;
                    case netDxf.Entities.TextAlignment.Middle:
                        border.VerticalAlignment = VerticalAlignment.Center;
                        border.HorizontalAlignment = HorizontalAlignment.Center;
                        break;
                    case netDxf.Entities.TextAlignment.Fit:
                        border.VerticalAlignment = VerticalAlignment.Center;
                        border.HorizontalAlignment = HorizontalAlignment.Center;
                        break;
                }

                var cor = (entity.Color.ToString() == entity.Layer.Color.ToString() ? entity.Layer.Color : entity.Color).GetCor();
                var textBlock = new System.Windows.Controls.Label();


                textBlock.FontSize = Height;
                textBlock.FontFamily = new System.Windows.Media.FontFamily("RomanS");
                textBlock.Content = Value;
                textBlock.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
                textBlock.Foreground = cor;
                textBlock.Background = System.Windows.Media.Brushes.Transparent;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                //textBlock.Margin = new Thickness(2, 2, 2, 2);

                border.Child = textBlock;

                var group = new TransformGroup();

                var flipTrans = new ScaleTransform();
                flipTrans.ScaleY = -1;

                group.Children.Add(new RotateTransform(-Rotation));
                group.Children.Add(flipTrans);

                border.RenderTransform = group;
                border.BorderBrush = System.Windows.Media.Brushes.White;
                border.Background = System.Windows.Media.Brushes.Black;

                border.BorderThickness = new Thickness(2, 2, 2, 2);
                border.CornerRadius = new CornerRadius(5);

                border.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));

                var norigem = new P3d();
                if (border.HorizontalAlignment == HorizontalAlignment.Center)
                {
                    norigem = new P3d((Position.X + X0), (Position.Y + Y0)).Mover(Rotation, -(textBlock.DesiredSize.Width / 2));
                }
                else if (border.HorizontalAlignment == HorizontalAlignment.Left)
                {
                    norigem = new P3d((Position.X + X0), (Position.Y + Y0)).Mover(Rotation, 0);
                }
                else if (border.HorizontalAlignment == HorizontalAlignment.Right)
                {
                    norigem = new P3d((Position.X + X0), (Position.Y + Y0)).Mover(Rotation, (-textBlock.DesiredSize.Width));
                }
                norigem = norigem.Mover(Rotation - 90, -(textBlock.DesiredSize.Height / 2));
                Canvas.SetLeft(border, norigem.X);
                Canvas.SetTop(border, norigem.Y);

                entity.Canvas = border;
                border.Tag = entity;
                textBlock.Tag = entity;


                return border;
            }

            return null;
            
        }
        public static Border GetCanvas(this netDxf.Entities.MText text, double X0 = 0, double Y0 = 0)
        {
            return GetText(text as netDxf.Entities.EntityObject, X0, Y0);
        }
        public static Border GetCanvas(this netDxf.Entities.Text text, double X0 = 0, double Y0 = 0)
        {
            return GetText(text as netDxf.Entities.EntityObject, X0, Y0);

        }
    }
}
