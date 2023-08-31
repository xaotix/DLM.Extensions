using Conexoes;
using netDxf;
using netDxf.Entities;
using System.Collections.Generic;
using System.Linq;

namespace DLM.desenho
{
    public static class Extensoes_NetDXFMBS
    {
        public static List<EntityObject> Converter(this netDxfMBS.DxfDocument Origem)
        {
            var Destino = new List<EntityObject>();

            foreach (var t in Origem.Arcs)
            {
                var m = t.Get();
                Destino.Add(m);
            }
            foreach (var t in Origem.Circles)
            {
                var m = t.Get();
                Destino.Add(m);
            }
            foreach (var t in Origem.Ellipses)
            {
                var m = t.Get();
                Destino.Add(m);
            }
            foreach (var t in Origem.Lines)
            {
                var m = t.Get();
                Destino.Add(m);
            }
            foreach (var t in Origem.Points)
            {
                var m = t.Get();
            }
            foreach (var t in Origem.Texts)
            {
                var m = t.Get();
                Destino.Add(m);
            }
            foreach (var t in Origem.Polylines)
            {
                /*VER*/
                //var m = new Polyline(t)
                if (t is netDxfMBS.Entities.Polyline)
                {
                    var s = t as netDxfMBS.Entities.Polyline;
                    var m = s.Get();
                    Destino.Add(m);
                }

            }
            foreach (var a in Origem.Inserts)
            {
                Destino.AddRange(Converter(a));
            }


            return Destino;
        }

        public static List<EntityObject> Converter(netDxfMBS.Entities.Insert a)
        {
            var Destino = new List<EntityObject>();
            foreach (var s in a.Block.Entities)
            {
                if (s is netDxfMBS.Entities.Arc)
                {
                    var t = (s as netDxfMBS.Entities.Arc).Get(new Vector3(a.InsertionPoint.X, a.InsertionPoint.Y, a.InsertionPoint.Z));
                    Destino.Add(t);
                }
                else if (s is netDxfMBS.Entities.Circle)
                {
                    var t = (s as netDxfMBS.Entities.Circle).Get(new Vector3(a.InsertionPoint.X, a.InsertionPoint.Y, a.InsertionPoint.Z));
                    Destino.Add(t);
                }
                else if (s is netDxfMBS.Entities.Ellipse)
                {
                    var t = (s as netDxfMBS.Entities.Ellipse).Get(new Vector3(a.InsertionPoint.X, a.InsertionPoint.Y, a.InsertionPoint.Z));
                    Destino.Add(t);
                }
                else if (s is netDxfMBS.Entities.Line)
                {
                    var t = (s as netDxfMBS.Entities.Line).Get(new Vector3(a.InsertionPoint.X, a.InsertionPoint.Y, a.InsertionPoint.Z));
                    Destino.Add(t);
                }
                else if (s is netDxfMBS.Entities.Point)
                {
                    var t = (s as netDxfMBS.Entities.Point).Get(new Vector3(a.InsertionPoint.X, a.InsertionPoint.Y, a.InsertionPoint.Z));
                    Destino.Add(t);
                }
                else if (s is netDxfMBS.Entities.Polyline)
                {
                    var t = (s as netDxfMBS.Entities.Polyline).Get(new Vector3(a.InsertionPoint.X, a.InsertionPoint.Y, a.InsertionPoint.Z));
                    Destino.Add(t);
                }
                else if (s is netDxfMBS.Entities.Text)
                {
                    var t = (s as netDxfMBS.Entities.Text).Get();
                    Destino.Add(t);
                }
                else if (s is netDxfMBS.Entities.Insert)
                {
                    var t = (s as netDxfMBS.Entities.Insert);
                    Destino.AddRange(Converter(t));
                }
                else
                {

                }
            }
            return Destino;
        }

        public static Text Get(this netDxfMBS.Entities.Text t)
        {
            var m = new Text(t.Value, new Vector3(t.BasePoint.X, t.BasePoint.Y, t.BasePoint.Z), t.Height);
            m.Color = Get(t.Color);

            m.Alignment = Get(t.Alignment);
            return m;
        }

        public static TextAlignment Get(netDxfMBS.TextAlignment from)
        {
            TextAlignment to = TextAlignment.Aligned;
            switch (from)
            {
                case netDxfMBS.TextAlignment.TopLeft:
                    to = TextAlignment.TopLeft;
                    break;
                case netDxfMBS.TextAlignment.TopCenter:
                    to = TextAlignment.TopCenter;

                    break;
                case netDxfMBS.TextAlignment.TopRight:
                    to = TextAlignment.TopRight;

                    break;
                case netDxfMBS.TextAlignment.MiddleLeft:
                    to = TextAlignment.MiddleLeft;

                    break;
                case netDxfMBS.TextAlignment.MiddleCenter:
                    to = TextAlignment.MiddleCenter;

                    break;
                case netDxfMBS.TextAlignment.MiddleRight:
                    to = TextAlignment.TopLeft;

                    break;
                case netDxfMBS.TextAlignment.BottomLeft:
                    to = TextAlignment.BottomLeft;

                    break;
                case netDxfMBS.TextAlignment.BottomCenter:
                    to = TextAlignment.BottomCenter;

                    break;
                case netDxfMBS.TextAlignment.BottomRight:
                    to = TextAlignment.BottomRight;

                    break;
                case netDxfMBS.TextAlignment.BaselineLeft:
                    to = TextAlignment.BaselineLeft;

                    break;
                case netDxfMBS.TextAlignment.BaselineCenter:
                    to = TextAlignment.BaselineCenter;

                    break;
                case netDxfMBS.TextAlignment.BaselineRight:
                    to = TextAlignment.BaselineRight;

                    break;
            }

            return to;
        }

        public static Polyline3D Get(this netDxfMBS.Entities.Polyline pol, Vector3 Origem = new Vector3())
        {
            var m = new Polyline3D(pol.Vertexes.Select(x => new Vector3(x.Location.X + Origem.X, x.Location.Y + Origem.Y, 0 + Origem.Z)).ToList());
            m.Color = Get(pol.Color);
            return m;
        }

        public static Point Get(this netDxfMBS.Entities.Point point, Vector3 Origem = new Vector3())
        {
            var m = new Point(point.Location.X + Origem.X, point.Location.Y + Origem.Y, point.Location.Z + Origem.Z);
            m.Thickness = point.Thickness;
            m.Color = Get(point.Color);
            return m;
        }

        public static Line Get(this netDxfMBS.Entities.Line line, Vector3 Origem = new Vector3())
        {
            var m = new Line(new Vector3(line.StartPoint.X + Origem.X, line.StartPoint.Y + Origem.Y, line.StartPoint.Z + Origem.Z),
                new Vector3(line.EndPoint.X + Origem.X, line.EndPoint.Y + Origem.Y, line.EndPoint.Z + Origem.Z)
                );
            m.Color = Get(line.Color);
            return m;
        }

        public static Arc Get(this netDxfMBS.Entities.Arc t, Vector3 Origem = new Vector3())
        {
            var m = new Arc(new Vector3(t.Center.X + Origem.X, t.Center.Y + Origem.Y, t.Center.Z + Origem.Z), t.Radius, t.StartAngle, t.EndAngle);
            m.Color = Get(t.Color);
            return m;
        }

        public static Circle Get(this netDxfMBS.Entities.Circle t, Vector3 Origem = new Vector3())
        {
            var m = new Circle(new Vector3(t.Center.X + Origem.X, t.Center.Y + Origem.Y, t.Center.Z + Origem.Z), t.Radius);
            m.Color = Get(t.Color);
            return m;
        }

        public static netDxf.AciColor Get(this netDxfMBS.AciColor cor)
        {
            AciColor m = new AciColor();
            m = AciColor.FromCadIndex(cor.Index);
            if (cor.Index == netDxfMBS.AciColor.ByLayer.Index)
            {
                m.FromColor(cor.ToColor());
            }
            return m;
        }

        public static Ellipse Get(this netDxfMBS.Entities.Ellipse ellipe, Vector3 Origem = new Vector3())
        {
            var m = new Ellipse(new Vector3(ellipe.Center.X + Origem.X, ellipe.Center.Y + Origem.Y, ellipe.Center.Z + Origem.Z), ellipe.MajorAxis, ellipe.MinorAxis);
            m.Color = ellipe.Color.Get();
            return m;
        }

    }
}
