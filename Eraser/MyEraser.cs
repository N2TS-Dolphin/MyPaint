
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using IShape;

namespace ToolEraser
{
    public class MyEraser : IShapes
    {
        private List<Point> _points = new List<Point>();
        public string Name => "Eraser";
        public string Image => "pack://application:,,,/ToolEraser;component/Resources/eraser.png";

        public void AddFirst(Point point)
        {
            _points.Add(point);
        }

        public void AddLast(Point point)
        {
            _points.Add(point);
        }

        public UIElement Convert(SolidColorBrush color, int thickness, DoubleCollection dashStyle)
        {
            var polyline = new Polyline
            {
                Stroke = Brushes.White,
                StrokeThickness = thickness * 2,
                StrokeDashArray = dashStyle
            };

            polyline.Points = new PointCollection(_points);

            return polyline;
        }

        public void ShiftPressMode()
        {
            // Not needed for eraser tool
        }

        public SolidColorBrush Color { get; set; }
        public int Thickness { get; set; }
        public DoubleCollection DashStyle { get; set; }

        public object Clone()
        {
            return new MyEraser
            {
                _points = new List<Point>(_points),
                Color = this.Color,
                Thickness = this.Thickness,
                DashStyle = this.DashStyle
            };
        }
    }
}
