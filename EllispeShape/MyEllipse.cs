
using IShapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace EllipseShape
{
    public class MyEllipse : IShape
    {
        private Point start;
        private Point end;

        public SolidColorBrush Color { get; set; }
        public int Thickness { get; set; }
        public DoubleCollection DashStyle { get; set; }

        public string Name => "Ellipse";
        public string Image => "pack://application:,,,/EllipseShape;component/Resources/Ellipse-icon.png";
        public void AddFirst(Point point)
        {
            start = point;
        }
        public void AddLast(Point point)
        {
            end = point;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
        public UIElement Convert(SolidColorBrush color, int thickness, DoubleCollection dashStyle)
        {
            var width = end.X - start.X;
            var height = end.Y - start.Y;
            var result = new Ellipse
            {
                Width = (width < 0) ? -width : width,
                Height = (height < 0) ? -height : height,
                StrokeThickness = thickness,
                Stroke = color,
                StrokeDashArray = dashStyle
            };
            var left = (width < 0) ? end.X : start.X;
            var top = (height < 0) ? end.Y : start.Y;
            Canvas.SetLeft(result, left);
            Canvas.SetTop(result, top);
            return result;
        }
    }
}
