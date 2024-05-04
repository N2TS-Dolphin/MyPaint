
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

        public SolidColorBrush Color { get; set; }
        public int Thickness { get; set; }
        public DoubleCollection DashStyle { get; set; }

        public string Name => "Line";
        public string Image => "pack://application:,,,/LineShape;component/Resources/Line-icon.png";
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
        public UIElement Convert(SolidColorBrush color, int thickness, DoubleCollection dashStyle) {
            return new System.Windows.Shapes.Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y,
                StrokeThickness = thickness,
                Stroke = color,
                StrokeDashArray= dashStyle
            };
        }

        public void ShiftPressMode()
        {
            double diff = Math.Abs(end.X - start.X) - Math.Abs(end.Y - start.Y);
            if (diff > 0)
            {
                end.Y = start.Y;
            }
            else
            {
                end.X = start.X;
            }
        }

    }
}
