
using System.Windows;

namespace IShapes
{
    public interface IShape : ICloneable
    {
        void AddFirst(Point point);
        void AddLast(Point point);
        UIElement Convert();
        string Name { get; }
    }
}
