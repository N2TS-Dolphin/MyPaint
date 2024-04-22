
using System.Windows;
using System.Windows.Media;

namespace IShapes
{
    public interface IShape : ICloneable
    {
        void AddFirst(Point point);
        void AddLast(Point point);
        UIElement Convert();
        string Name { get; }
        string Image { get; }
    }
}
