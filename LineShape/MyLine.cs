
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using IShapes;

namespace LineShape
{
    public class MyLine : IShape
    {
        private Point start;
        private Point end;
        public string Name => "Line";
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
            return new System.Windows.Shapes.Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y,
                StrokeThickness = 1,
                Stroke = new SolidColorBrush(Colors.Red),
            };
        }
    }
}
