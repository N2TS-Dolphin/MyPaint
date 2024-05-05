
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using IShapes;

namespace Pencil
{
    public class MyPencil : IShape
    {
        private List<Point> _points = new List<Point>();
        private Point _lastPoint;

        public void AddFirst(Point point)
        {
            _points.Add(point);
            _lastPoint = point;
        }

        public void AddLast(Point point)
        {
            _points.Add(point);
            _lastPoint = point;
        }

        public UIElement Convert(SolidColorBrush color, int thickness, DoubleCollection dashStyle)
        {
            var polyline = new Polyline
            {
                Stroke = color,
                StrokeThickness = thickness,
                StrokeDashArray = dashStyle
            };

            polyline.Points = new PointCollection(_points);

            return polyline;
        }

        public void ShiftPressMode()
        {
            // Not needed for pencil tool
        }

        public string Name { get { return "Pencil"; } }
        public string Image { get { return "/Resources/pencil.png"; } }

        public SolidColorBrush Color { get; set; }
        public int Thickness { get; set; }
        public DoubleCollection DashStyle { get; set; }

        public object Clone()
        {
            return new MyPencil
            {
                _points = new List<Point>(_points),
                Color = this.Color,
                Thickness = this.Thickness,
                DashStyle = this.DashStyle
            };
        }
    }
}
