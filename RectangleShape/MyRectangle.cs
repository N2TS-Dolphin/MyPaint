using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using IShapes;

namespace RectangleShape
{
    public class MyRectangle : IShape
    {
        private Point start;
        private Point end;
        public string Name => "Rectangle";
        public string Image => "pack://application:,,,/RectangleShape;component/Resources/Rectangle-icon.png";
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
        public UIElement Convert()
        {
            var width = end.X - start.X;
            var height = end.Y - start.Y;
            var result = new Rectangle
            {
                Width = (width < 0) ? -width : width,
                Height = (height < 0) ? -height : height,
                StrokeThickness = 1,
                Stroke = new SolidColorBrush(Colors.Green),
            };
            var left = (width < 0) ? end.X : start.X;
            var top = (height < 0) ? end.Y : start.Y;
            Canvas.SetLeft(result, left);
            Canvas.SetTop(result, top);
            return result;
        }
    }
}
