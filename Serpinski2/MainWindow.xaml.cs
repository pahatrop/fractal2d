using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Serpinski2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int Level = 4;
        private bool wndReady = false;
        //Высота и ширина для отрисовки
        private int _width
        {
            get
            {
                int s = 500;
                if (!wndReady) return s;
                Dispatcher.Invoke(() =>
                {
                    if (!int.TryParse(sizeW.Text, out s))
                        s = 500;
                    sizeW.Text = s.ToString();
                });
                return s;
            }
        }
        private int _height
        {
            get
            {
                int s = 500;
                if (!wndReady) return s;
                Dispatcher.Invoke(() =>
                {
                    if (!int.TryParse(sizeH.Text, out s) || !wndReady)
                        s = 500;
                    sizeH.Text = s.ToString();
                });
                return s;
            }
        }
        // Bitmap для фрактала
        private Bitmap _fractal;
        // используем для отрисовки на PictureBox
        private Graphics _graph;
        private bool _work = false;
        private System.Windows.Point _oldMousePosition;
        private Action<object, RoutedEventArgs> _currentDelegat;
        private Timer _timer;
        public MainWindow()
        {
            InitializeComponent();
            _timer = new Timer(1000);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (_work)
                    status.Text = "Working";
                else
                    status.Text = "Ready";
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            drawingArea.Width = _width;
            drawingArea.Height = _height;
            wndReady = true;
        }
        
        private void TriangleButton_Click(object sender, RoutedEventArgs e)
        {
            if (_work) return;
            _work = true;
            drawingArea.Width = _width;
            drawingArea.Height = _height;
            Task.Run(() => {
                _currentDelegat = TriangleButton_Click;
                //создаем Bitmap для треугольника
                _fractal = new Bitmap(_width, _height);
                // cоздаем новый объект Graphics из указанного Bitmap
                _graph = Graphics.FromImage(_fractal);
                //вершины треугольника
                PointF topPoint = new PointF(_width / 2f, 0);
                PointF leftPoint = new PointF(0, _height);
                PointF rightPoint = new PointF(_width, _height);
                //вызываем функцию отрисовки
                DrawTriangle(Level, topPoint, leftPoint, rightPoint);
                //отображаем получившийся фрактал
                Dispatcher.Invoke(() => drawingArea.Source = BitmapToImageSource(_fractal));
                _work = false;
            });
        }

        private void PascalTriangleButton_Click(object sender, RoutedEventArgs e)
        {
            if (_work) return;
            _work = true;
            drawingArea.Width = _width;
            drawingArea.Height = _height;
            Task.Run(() => {
                _currentDelegat = PascalTriangleButton_Click;
                //создаем Bitmap для треугольника
                _fractal = new Bitmap(_width, _height);
                // cоздаем новый объект Graphics из указанного Bitmap
                _graph = Graphics.FromImage(_fractal);
                //вершины треугольника
                //вызываем функцию отрисовки
                DrawPascalTriangle();
                //отображаем получившийся фрактал
                Dispatcher.Invoke(() => drawingArea.Source = BitmapToImageSource(_fractal));
                _work = false;
            });
        }

        private void CarpetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_work) return;
            _work = true;
            drawingArea.Width = _width;
            drawingArea.Height = _height;
            Task.Run(() => {
                _currentDelegat = CarpetButton_Click;
                //создаем Bitmap для прямоугольника
                _fractal = new Bitmap(_width, _height);
                // cоздаем новый объект Graphics из указанного Bitmap
                _graph = Graphics.FromImage(_fractal);
                //создаем прямоугольник и вызываем функцию отрисовки ковра
                RectangleF carpet = new RectangleF(0, 0, _width, _height);
                DrawCarpet(Level, carpet);
                //отображаем результат
                Dispatcher.Invoke(() => drawingArea.Source = BitmapToImageSource(_fractal));
                _work = false;
            });
        }

        private System.Windows.Media.Imaging.BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                System.Windows.Media.Imaging.BitmapImage bitmapimage = new System.Windows.Media.Imaging.BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }
        
        private void DrawTriangle(int level, PointF top, PointF left, PointF right)
        {
            //проверяем, закончили ли мы построение
            if (level == 0)
            {
                PointF[] points = new PointF[3]
                {
                    top, right, left
                };
                //рисуем фиолетовый треугольник
                _graph.FillPolygon(Brushes.BlueViolet, points);
            }
            else
            {
                //вычисляем среднюю точку
                var leftMid = MidPoint(top, left); //левая сторона
                var rightMid = MidPoint(top, right); //правая сторона
                var topMid = MidPoint(left, right); // основание
                //рекурсивно вызываем функцию для каждого и 3 треугольников
                DrawTriangle(level - 1, top, leftMid, rightMid);
                DrawTriangle(level - 1, leftMid, left, topMid);
                DrawTriangle(level - 1, rightMid, topMid, right);
            }
        }

        private void DrawPascalTriangle()
        {
            int n = 512, m = 3;
            int[,] T = new int[n,n];
            for (int i = 0; i < n; ++i)
            {
                T[i, 0] = 1;
                for (int j = 1; j <= i; ++j)
                {
                    T[i, j] = (T[i - 1, j - 1] + T[i - 1, j]) % m;
                    if(T[i,j] == 1)
                         _graph.FillRectangle(Brushes.Orange, i, j, 1, 1);
                }
                for (int j = i + 1; j < n; ++j)
                {
                    T[i, j] = 0;
                }
            }

        }

        //функция вычисления координат средней точки
        private PointF MidPoint(PointF p1, PointF p2)
        {
            return new PointF((p1.X + p2.X) / 2f, (p1.Y + p2.Y) / 2f);
        }

        private void DrawCarpet(int level, RectangleF carpet)
        {
            //проверяем, закончили ли мы построение
            if (level == 0)
            {
                //Рисуем прямоугольник
                _graph.FillRectangle(Brushes.OrangeRed, carpet);
            }
            else
            {
                // делим прямоугольник на 9 частей
                var width = carpet.Width / 3f;
                var height = carpet.Height / 3f;
                // (x1, y1) - координаты левой верхней вершины прямоугольника
                // от нее будем отсчитывать остальные вершины маленьких прямоугольников
                var x1 = carpet.Left;
                var x2 = x1 + width;
                var x3 = x1 + 2f * width;

                var y1 = carpet.Top;
                var y2 = y1 + height;
                var y3 = y1 + 2f * height;

                DrawCarpet(level - 1, new RectangleF(x1, y1, width, height)); // левый 1(верхний)
                DrawCarpet(level - 1, new RectangleF(x2, y1, width, height)); // средний 1
                DrawCarpet(level - 1, new RectangleF(x3, y1, width, height)); // правый 1
                DrawCarpet(level - 1, new RectangleF(x1, y2, width, height)); // левый 2
                DrawCarpet(level - 1, new RectangleF(x3, y2, width, height)); // правый 2
                DrawCarpet(level - 1, new RectangleF(x1, y3, width, height)); // левый 3
                DrawCarpet(level - 1, new RectangleF(x2, y3, width, height)); // средний 3
                DrawCarpet(level - 1, new RectangleF(x3, y3, width, height)); // правый 3
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            drawingArea.Width = _width * e.NewValue;
            drawingArea.Height = _height * e.NewValue;
            var newValue = 3 + (int)e.NewValue;
            if (newValue != Level)
            {
                Level = newValue;
                _currentDelegat?.Invoke(null, null);
                Console.WriteLine(Level);
            }
        }

        private void drawingArea_MouseMove(object sender, MouseEventArgs e)
        {
            var newMousePosition = Mouse.GetPosition((System.Windows.Controls.Image)sender);

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (newMousePosition.Y < _oldMousePosition.Y)
                    sv.ScrollToVerticalOffset(sv.VerticalOffset + 1);
                if (newMousePosition.Y > _oldMousePosition.Y)
                    sv.ScrollToVerticalOffset(sv.VerticalOffset - 1);

                if (newMousePosition.X < _oldMousePosition.X)
                    sv.ScrollToHorizontalOffset(sv.HorizontalOffset + 1);
                if (newMousePosition.X > _oldMousePosition.X)
                    sv.ScrollToHorizontalOffset(sv.HorizontalOffset - 1);
            }
            else
            {
                _oldMousePosition = newMousePosition;
            }
        }

        private void Cantor_Click(object sender, RoutedEventArgs e)
        {
            if (_work) return;
            _work = true;
            Task.Run(() => {
                _currentDelegat = Cantor_Click;
                

                int lenght = 0;
                Dispatcher.Invoke(() =>
                {
                    if (!int.TryParse(CantorLenght.Text, out lenght)) lenght = 100;
                    CantorLenght.Text = lenght.ToString();
                    if(lenght > _width)
                        drawingArea.Width = lenght;
                    else
                        drawingArea.Width = _width;
                    //создаем Bitmap для треугольника
                    _fractal = new Bitmap((int)drawingArea.Width, _height);
                    // cоздаем новый объект Graphics из указанного Bitmap
                    _graph = Graphics.FromImage(_fractal);
                    //вызываем функцию отрисовки
                });
                var result = DrawCantor(10, 10, lenght);
                //отображаем получившийся фрактал
                Pen myPen = new Pen(Color.Red, 1);
                foreach (var item in result)
                {
                    _graph.DrawRectangle(myPen, item[0], item[1], item[2], 5);
                }
                Dispatcher.Invoke(() => drawingArea.Source = BitmapToImageSource(_fractal));
                _work = false;
            });
        }
        private List<int[]> DrawCantor(int x, int y, int width)
        {
            var retList = new List<int[]>();
            if (width >= 3)
            {
                retList.Add(new int[] { x, y, width });
                //Сдвигаемся вниз
                y = y + 20;
                //Вызываем функцию для двух полученных отрезков
                retList = retList.Concat(DrawCantor(x + width * 2 / 3, y, width / 3)).ToList();
                retList = retList.Concat(DrawCantor(x, y, width / 3)).ToList();
            }
            return retList;
        }

        private void SaveToFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog();
                dialog.Filter = "Image files (*.png) | *.png";
                if (dialog.ShowDialog() == true)
                {
                    _fractal.Save(dialog.FileName, ImageFormat.Png);
                    MessageBox.Show("Success", "File saved!");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Something wrong", ex.Message);
            }
        }
    }
}
