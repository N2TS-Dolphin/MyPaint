using IShapes;
using LineShape;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        bool _isDrawing = false; int _shapeType = 1;
        Point _start; Point _end;
        List<IShape> _painters = new List<IShape>();
        IShape _painter = new MyLine();
        List<IShape> _prototype = new List<IShape>();
        private ObservableCollection<Button> _buttonControls = new ObservableCollection<Button>();
        public MainWindow()
        {
            InitializeComponent();
        }
        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            shapeIconListView.ItemsSource = _buttonControls;

            string folder = AppDomain.CurrentDomain.BaseDirectory;
            var file = new DirectoryInfo(folder).GetFiles("*.dll");

            foreach (var f in file)
            {
                var assembly = Assembly.LoadFrom(f.FullName);
                var types = assembly.GetTypes();
                foreach (var t in types)
                {
                    if ((t.IsClass) && (typeof(IShape).IsAssignableFrom(t)))
                    {
                        _prototype.Add((IShape)Activator.CreateInstance(t)!);
                    }
                }
            }

            foreach (var item in _prototype)
            {
                var control = new Button
                {
                    Width = 70,
                    Height = 30,
                    Content = item.Name,
                    Tag = item
                };
                control.Click += Control_Click;
                _buttonControls.Add(control);
            }
            _painter = _prototype[0];
        }

        private void Control_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var item = button.Tag as IShape;
                _painter = item;
            }
        }

        private void mouseCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = true;
            _start = e.GetPosition(mouseCanvas);
        }

        private void mouseCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                drawingArea.Children.Clear();
                foreach (var item in _painters)
                {
                    drawingArea.Children.Add(item.Convert());
                }

                _end = e.GetPosition(mouseCanvas);

                _painter.AddFirst(_start);
                _painter.AddLast(_end);
                drawingArea.Children.Add(_painter.Convert());
            }
        }

        private void mouseCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            _painters.Add((IShape)_painter.Clone());
        }
        private void pasteBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void copyBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void undoBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void redoBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void shapeIconListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void editColorBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void thicknessComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void styleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void newFileBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void openFileBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void saveFileBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void blackColorBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void grayColorBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void darkredColorBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void redColorBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void orangeColorBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void yellowColorBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void greenColorBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void blueColorBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void purpleColorBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void brownColorBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void pinkColorButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Mouse press");
        }
    }
}