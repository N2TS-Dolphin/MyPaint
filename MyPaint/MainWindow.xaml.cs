using IShapes;
using LineShape;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MyPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        bool _isDrawing = false;
        bool _shiftMode = false;
        Point _start; Point _end;
        List<IShape> _painters = new List<IShape>();
        IShape _painter = new MyLine();
        List<IShape> _prototype = new List<IShape>();

        private SolidColorBrush _currentColor = new SolidColorBrush(Colors.Black);
        private int _currentThickness = 1;
        private DoubleCollection _currentDashStyle = null;

        ColorDialog _myColorDialog = new ColorDialog();
        Stack<IShape> _redoStack = new Stack<IShape>();

        public MainWindow()
        {
            InitializeComponent();
            PreviewKeyDown += MainWindow_PreviewKeyDown; 
            KeyDown += new System.Windows.Input.KeyEventHandler(OnButtonKeyDown);
            KeyUp += new System.Windows.Input.KeyEventHandler(OnButtonKeyUp);
        }
        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            shapeIconListView.ItemsSource = _prototype;

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
            _painter = _prototype[0]; // Set initial selected shape
            shapeIconListView.SelectedIndex = 0;
        }

        private void OnButtonKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                _shiftMode = true;
            }
        }

        private void OnButtonKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                _shiftMode = false;
            }
        }

        private void shapeIconListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (shapeIconListView.SelectedItem != null)
            {
                _painter = (IShape)shapeIconListView.SelectedItem;
            }
        }
        private void mouseCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = true;
            _start = e.GetPosition(mouseCanvas);
        }

        private void mouseCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isDrawing)
            {
                RedrawCanvas();

                _end = e.GetPosition(mouseCanvas);

              

                _painter.AddFirst(_start);
                _painter.AddLast(_end);

                if (_shiftMode)
                {
                    _painter.ShiftPressMode();
                }
                drawingArea.Children.Add(_painter.Convert(_currentColor, _currentThickness, _currentDashStyle));
            }
        }

        private void mouseCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;

            var temp = (IShape)_painter.Clone();
            _painters.Add(temp);
          
            //clear redo stack
            _redoStack.Clear();

            temp.Color= _currentColor;
            temp.Thickness = _currentThickness;
            temp.DashStyle= _currentDashStyle;
        }
        private void pasteBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void copyBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void undo()
        {
            if (_painters.Count > 0)
            {
                var lastShape = _painters[_painters.Count - 1];
                _painters.RemoveAt(_painters.Count - 1);

                _redoStack.Push(lastShape);

                RedrawCanvas();
            }
        }

        private void redo()
        {
            if (_redoStack.Count > 0)
            {
                // Pop the last shape from redoStack
                var redoShape = _redoStack.Pop();

                // Add the redoShape to _painters
                _painters.Add(redoShape);

                // Redraw canvas
                RedrawCanvas();
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.Z:
                        undo();
                        break;
                    case Key.Y:
                        redo();
                        break;
                    default:
                        break;
                }
            }
        }

        private void undoBtn_Click(object sender, RoutedEventArgs e)
        {
            undo();
        }

        private void redoBtn_Click(object sender, RoutedEventArgs e)
        {
            redo();
        }

        private void RedrawCanvas()
        {
            drawingArea.Children.Clear();
            foreach (var item in _painters)
            {
                drawingArea.Children.Add(item.Convert(item.Color, item.Thickness, item.DashStyle));
            }
        }

        private void editColorBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_myColorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _currentColor = new SolidColorBrush(Color.FromRgb(_myColorDialog.Color.R, _myColorDialog.Color.G, _myColorDialog.Color.B));
            }
        }

        private void thicknessComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = thicknessComboBox.SelectedIndex;

            switch (index)
            {
                case 0:
                    _currentThickness = 1;
                    break;
                case 1:
                    _currentThickness = 2;
                    break;
                case 2:
                    _currentThickness = 3;
                    break;
                case 3:
                    _currentThickness = 4;
                    break;
                case 4:
                    _currentThickness = 5;
                    break;
                default:
                    break;
            }

        }

        private void styleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            int index = styleComboBox.SelectedIndex;

            switch (index)
            {
                case 0:
                    _currentDashStyle = null;
                    break;
                case 1:
                    _currentDashStyle = new DoubleCollection() { 6, 1 };
                    break;
                case 2:
                    _currentDashStyle = new DoubleCollection() { 1, 1 };
                    break;
                case 3:
                    _currentDashStyle = new DoubleCollection() { 4, 1, 1, 1};
                    break;
               
                default:
                    break;
            }

        }

        private void newFileBtn_Click(object sender, RoutedEventArgs e)
        {
            clearMyCanvas();
        }

        private void clearMyCanvas()
        {
            _painters.Clear();
            drawingArea.Children.Clear();
            drawingArea.Background = Brushes.Transparent;
        }

        private void openFileBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "PNG (*.png)|*.png";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = dialog.FileName;

                BitmapImage bitmapImage = new BitmapImage(new Uri(path, UriKind.Absolute));

                //Make image into Background
                drawingArea.Background = new ImageBrush(bitmapImage);
            }
        }
   
        private void saveFileBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "PNG (*.png)|*.png";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filename = dialog.FileName;
                SaveCanvasImage(drawingArea, filename);
                System.Windows.MessageBox.Show("Your canvas has been saved", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SaveCanvasImage(Canvas canvas, string filename)
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
            canvas.Measure(new Size((int)canvas.ActualWidth, (int)canvas.ActualHeight));
            canvas.Arrange(new Rect(new Size((int)canvas.ActualWidth, (int)canvas.ActualHeight)));

            renderBitmap.Render(canvas);

            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (FileStream file = File.Create(filename))
            {
                pngEncoder.Save(file);
            }
                  
        }

        private void blackColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Black);
        }

        private void grayColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Gray);
        }

        private void redColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Red);
        }

        private void orangeColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Orange);
        }

        private void yellowColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Yellow);
        }

        private void greenColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Green);
        }

        private void blueColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Blue);
        }

        private void purpleColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Purple);
        }

        private void brownColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Brown);
        }

        private void pinkColorButton_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Pink);
        }

        float factor = 1;
        float maxZoom = 10f; // Maximum zoom factor
        float minZoom = 1f; // Minimum zoom factor
        private void drawingArea_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0 && factor > minZoom) // Zoom out
            {
                factor -= 0.1f;
            }
            else if (e.Delta > 0 && factor < maxZoom) // Zoom in
            {
                factor += 0.1f;
            }
            Point pt = Mouse.GetPosition(drawingArea);
            st.CenterX = pt.X;
            st.CenterY = pt.Y;
            st.ScaleY = st.ScaleX = factor;
        }
    }
}