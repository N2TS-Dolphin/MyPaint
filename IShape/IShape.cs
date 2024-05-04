
using System.Windows;
using System.Windows.Media;

namespace IShapes
{
    public interface IShape : ICloneable
    {
        void AddFirst(Point point);
        void AddLast(Point point);
        UIElement Convert(SolidColorBrush color, int thickness, DoubleCollection dashStyle);
        void ShiftPressMode();

        string Name { get; }
        string Image { get; }

        SolidColorBrush Color { get; set; }
        int Thickness { get; set; }
        DoubleCollection DashStyle { get; set; }

    }
}
