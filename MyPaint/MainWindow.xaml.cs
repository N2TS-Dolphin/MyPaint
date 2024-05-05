using ToolEraser;
using IShape;
using LineShape;
using ToolPencil;

using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        List<IShapes> _painters = new List<IShapes>();
        List<IShapes> _prototypeShape = new List<IShapes>();
        List<IShapes> _prototypeTool = new List<IShapes>();
        IShapes _painter = new MyLine();

        private SolidColorBrush _currentColor = new SolidColorBrush(Colors.Black);
        private int _currentThickness = 1;
        private DoubleCollection _currentDashStyle = null;

        ColorDialog _myColorDialog = new ColorDialog();
        Stack<IShapes> _redoStack = new Stack<IShapes>();

        public float ZoomFactor { get; set; } //Zoom scaling factor
        private float maxFactor = 10f; // Maximum zoom factor
        private float minFactor = 1f; // Minimum zoom factor
        public MainWindow()
        {
            InitializeComponent();
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            KeyDown += new System.Windows.Input.KeyEventHandler(OnButtonKeyDown);
            KeyUp += new System.Windows.Input.KeyEventHandler(OnButtonKeyUp);
        }
        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ZoomFactor = 1;

            string folder = AppDomain.CurrentDomain.BaseDirectory;
            var file = new DirectoryInfo(folder).GetFiles("*.dll");

            foreach (var f in file)
            {
                string fileName = Path.GetFileNameWithoutExtension(f.Name);

                // Nếu tên file bắt đầu bằng "Tool", thêm vào danh sách công cụ (_prototypeTool)
                if (fileName.StartsWith("Tool"))
                {
                    var assembly = Assembly.LoadFrom(f.FullName);
                    var types = assembly.GetTypes();
                    foreach (var t in types)
                    {
                        if ((t.IsClass) && (typeof(IShapes).IsAssignableFrom(t)))
                        {
                            _prototypeTool.Add((IShapes)Activator.CreateInstance(t)!);
                        }
                    }
                }
                // Nếu tên file không bắt đầu bằng "Tool", thêm hình dạng vào danh sách _prototypeShape
                else
                {
                    var assembly = Assembly.LoadFrom(f.FullName);
                    var types = assembly.GetTypes();
                    foreach (var t in types)
                    {
                        if ((t.IsClass) && (typeof(IShapes).IsAssignableFrom(t)))
                        {
                            _prototypeShape.Add((IShapes)Activator.CreateInstance(t)!);
                        }
                    }
                }
            }
            shapeIconListView.ItemsSource = _prototypeShape;
            toolIconListView.ItemsSource = _prototypeTool;

            _painter = _prototypeShape[0]; // Set initial selected shape
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

        private void shapeIconListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (shapeIconListView.SelectedItem != null)
            {
                _painter = (IShapes)shapeIconListView.SelectedItem;
                toolIconListView.SelectedItem = null;
            }
        }

        private void toolIconListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (toolIconListView.SelectedItem != null)
            {
                _painter = (IShapes)toolIconListView.SelectedItem;
                shapeIconListView.SelectedItem = null;
                if (_painter is MyPencil)
                {
                    _painter = new MyPencil();
                }
                else if (_painter is MyEraser)
                {
                    _painter = new MyEraser();
                }
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
                    _currentDashStyle = new DoubleCollection() { 4, 1, 1, 1 };
                    break;

                default:
                    break;
            }

        }

        private void mouseCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = true;
            _start = e.GetPosition(mouseCanvas);
            _painter.AddFirst(_start);
        }

        private void mouseCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isDrawing)
            {
                Point point = e.GetPosition(mouseCanvas);
                _painter.AddLast(point);
                RedrawCanvas();

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

            var temp = (IShapes)_painter.Clone();
            _painters.Add(temp);

            //clear redo stack
            _redoStack.Clear();

            temp.Color = _currentColor;
            temp.Thickness = _currentThickness;
            temp.DashStyle = _currentDashStyle;

            // Clear the list of points in the _painter object
            if (_painter is MyPencil)
                _painter = new MyPencil();
        }

        // Xử lý chức năng
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

        private void RedrawCanvas()
        {
            drawingArea.Children.Clear();
            foreach (var item in _painters)
            {
                drawingArea.Children.Add(item.Convert(item.Color, item.Thickness, item.DashStyle));
            }
        }

        private void clearMyCanvas()
        {
            _painters.Clear();
            drawingArea.Children.Clear();
            drawingArea.Background = Brushes.Transparent;
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


        // Xử lý các sự kiện click

        /// <summary>
        /// Xử lý thao tác paste
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pasteBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Xử lý thao tác copy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copyBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Xử lý thao tác hoàn tác
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void undoBtn_Click(object sender, RoutedEventArgs e)
        {
            undo();
        }

        /// <summary>
        /// Xử lý thao tác lặp lại
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void redoBtn_Click(object sender, RoutedEventArgs e)
        {
            redo();
        }

        /// <summary>
        /// Xử lý thao tác sửa màu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editColorBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_myColorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _currentColor = new SolidColorBrush(Color.FromRgb(_myColorDialog.Color.R, _myColorDialog.Color.G, _myColorDialog.Color.B));
            }
        }

        /// <summary>
        /// Xử lý thao tác tạo file mới
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newFileBtn_Click(object sender, RoutedEventArgs e)
        {
            clearMyCanvas();
        }

        /// <summary>
        /// Xử lý thao tác mở file trong máy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Xử lý thao tác lưu file hiện tại
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Xử lý chọn màu đen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void blackColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Black);
        }

        /// <summary>
        /// Xử lý chọn màu xám
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grayColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Gray);
        }

        /// <summary>
        /// Xử lý chọn màu đỏ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void redColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Red);
        }

        /// <summary>
        /// Xử lý chọn màu cam
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void orangeColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Orange);
        }

        /// <summary>
        /// Xử lý chọn màu vàng
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void yellowColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Yellow);
        }

        /// <summary>
        /// Xử lý chọn màu xanh lá
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void greenColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Green);
        }

        /// <summary>
        /// Xử lý chọn màu xanh biển
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void blueColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Blue);
        }

        /// <summary>
        /// Xử lý chọn màu tím
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void purpleColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Purple);
        }

        /// <summary>
        /// Xử lý chọn màu nâu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void brownColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Brown);
        }

        /// <summary>
        /// Xử lý chọn màu hổng
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pinkColorButton_Click(object sender, RoutedEventArgs e)
        {
            _currentColor = new SolidColorBrush(Colors.Pink);
        }

        

        /// <summary>
        /// Zoom Vùng vẽ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawingArea_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0 && ZoomFactor > minFactor) // Zoom out
            {
                ZoomFactor -= 0.5f;
            }
            else if (e.Delta > 0 && ZoomFactor < maxFactor) // Zoom in
            {
                ZoomFactor += 0.5f;
            }
            ChangeCanvasScale();
        }

        private void ChangeCanvasScale()
        {
            Point pt = Mouse.GetPosition(drawingArea);
            da.CenterX = pt.X;
            da.CenterY = pt.Y;
            da.ScaleY = da.ScaleX = ZoomFactor;

            mc.CenterX = pt.X;
            mc.CenterY = pt.Y;
            mc.ScaleY = mc.ScaleX = ZoomFactor;
        }

        private void zoomInBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ZoomFactor < maxFactor) // Zoom in
            {
                ZoomFactor += 0.5f;
                ChangeCanvasScale();
            }
        }

        private void zoomOutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ZoomFactor > minFactor) // Zoom out
            {
                ZoomFactor -= 0.5f;
                ChangeCanvasScale();
            }
        }
    }
}