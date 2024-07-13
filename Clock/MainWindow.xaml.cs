using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Clock.Extensions;
using Clock.WinApi;

namespace Clock
{
    public partial class MainWindow : Window
    {
        /// <summary> </summary>
        private readonly Win32Api _win32Api;

        /// <summary> </summary>
        private readonly DispatcherTimer _redrawTimer;
        /// <summary> </summary>
        private readonly DispatcherTimer _leaveTimer;

        /// <summary> </summary>
        private bool _isResizing = false;
        /// <summary> </summary>
        private Point _lastMousePosition;
        /// <summary> </summary>
        private Point? _lockPosition;
        /// <summary> </summary>
        private WinDHook _winDHook;
        /// <summary>
        /// 
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            _win32Api = new Win32Api(this);

            _redrawTimer = new DispatcherTimer();
            _redrawTimer.Interval = TimeSpan.FromSeconds(1);
            _redrawTimer.Tick += RedrawTimer_Tick;

            _leaveTimer = new DispatcherTimer();
            _leaveTimer.Interval = TimeSpan.FromSeconds(0.5);
            _leaveTimer.Tick += LeaveTimer_Tick;

            _winDHook = new WinDHook(this);

            Loaded += (s, e) => _redrawTimer.Start();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RedrawTimer_Tick(object sender, EventArgs e)
        {
            ClockCanvas.Children.Clear();
            DrawClockFace();
            DrawHands();
            UpdateDate();
            UpdatePosition();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeaveTimer_Tick(object sender, EventArgs e)
        {
            _leaveTimer.Stop();
            MainGrid.Background = Brushes.Transparent;
            WindowBorder.BorderBrush = Brushes.Transparent;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_isResizing)
            {
                return;
            }

            ClockCanvas.Children.Clear();
            DrawClockFace();
            DrawHands();
            UpdateDate();
        }
        /// <summary>
        /// 
        /// </summary>
        private void DrawClockFace()
        {
            double radius = Math.Min(ClockCanvas.ActualWidth, ClockCanvas.ActualHeight) / 2 - 10;

            // キャンバスのサイズが有効か確認します
            if (radius <= 0)
            {
                return;
            }

            Ellipse face = new Ellipse
            {
                Width = 2 * radius,
                Height = 2 * radius,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Fill = Brushes.Black // 時計の背景を黒に設定
            };
            Canvas.SetLeft(face, ClockCanvas.ActualWidth / 2 - radius);
            Canvas.SetTop(face, ClockCanvas.ActualHeight / 2 - radius);
            ClockCanvas.Children.Add(face);

            // 数字を描画
            DrawNumbers(radius);
            // 目盛線を描画
            DrawTicks(radius);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        private void DrawNumbers(double radius)
        {
            double centerX = ClockCanvas.ActualWidth / 2;
            double centerY = ClockCanvas.ActualHeight / 2;

            for (int i = 0; i < 12; i++)
            {
                double angle = i * 30 * Math.PI / 180;
                double x = centerX + (radius - 22) * Math.Cos(angle - Math.PI / 2);
                double y = centerY + (radius - 22) * Math.Sin(angle - Math.PI / 2);

                TextBlock number = new TextBlock
                {
                    Text = (i == 0 ? 12 : i).ToString(), // 0を12に変換
                    FontSize = 20,
                    Foreground = Brushes.White
                };

                number.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Size textSize = number.DesiredSize;

                Canvas.SetLeft(number, x - textSize.Width / 2);
                Canvas.SetTop(number, y - textSize.Height / 2);
                ClockCanvas.Children.Add(number);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        private void DrawTicks(double radius)
        {
            Point center = new Point(ClockCanvas.ActualWidth / 2, ClockCanvas.ActualHeight / 2);
            // Create the hour ticks
            for (int i = 0; i < 12; i++)
            {
                Line hourTick = new Line
                {
                    X1 = center.X,
                    Y1 = center.Y - radius,
                    X2 = center.X,
                    Y2 = center.Y - radius + 10,
                    Stroke = Brushes.White,
                    StrokeThickness = 2,
                    RenderTransform = new RotateTransform(i * 30, center.X, center.Y)
                };
                ClockCanvas.Children.Add(hourTick);
            }

            // Create the minute ticks
            for (int i = 0; i < 60; i++)
            {
                if (i % 5 != 0) // Skip the hour ticks
                {
                    Line minuteTick = new Line
                    {
                        X1 = center.X,
                        Y1 = center.Y - radius,
                        X2 = center.X,
                        Y2 = center.Y - radius + 5,
                        Stroke = Brushes.White,
                        StrokeThickness = 2,
                        RenderTransform = new RotateTransform(i * 6, center.X, center.Y)
                    };
                    ClockCanvas.Children.Add(minuteTick);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void DrawHands()
        {
            DateTime now = DateTime.Now;
            double radius = Math.Min(ClockCanvas.ActualWidth, ClockCanvas.ActualHeight) / 2 - 10;
            Point center = new Point(ClockCanvas.ActualWidth / 2, ClockCanvas.ActualHeight / 2);

            // キャンバスのサイズが有効か確認します
            if (radius <= 0)
            {
                return;
            }

            // Hour hand
            DrawHand(center, radius * 0.5, (now.Hour % 12 + now.Minute / 60.0) * 30, Brushes.White, 6);

            // Minute hand
            DrawHand(center, radius * 0.7, (now.Minute + now.Second / 60.0) * 6, Brushes.White, 4);

            // Second hand
            DrawHand(center, radius * 0.9, now.Second * 6, Brushes.Red, 2);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="length"></param>
        /// <param name="angle"></param>
        /// <param name="brush"></param>
        /// <param name="thickness"></param>
        private void DrawHand(Point center, double length, double angle, Brush brush, double thickness)
        {
            double radian = angle * Math.PI / 180;
            Point endPoint = new Point(
                center.X + length * Math.Cos(radian - Math.PI / 2),
                center.Y + length * Math.Sin(radian - Math.PI / 2));

            Line hand = new Line
            {
                X1 = center.X,
                Y1 = center.Y,
                X2 = endPoint.X,
                Y2 = endPoint.Y,
                Stroke = brush,
                StrokeThickness = thickness
            };

            ClockCanvas.Children.Add(hand);
        }
        /// <summary>
        /// 
        /// </summary>
        private void UpdateDate()
        {
            DateTime now = DateTime.Now;
            string dayOfWeek = now.ToString("ddd");
            DateTextBlock.Text = $"{now.Month}/{now.Day} ({dayOfWeek})";
            DateTextBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            DateTextBlock.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 
        /// </summary>
        private void UpdatePosition()
        {
            if (_lockPosition == null)
            {
                return;
            }

            Left = _lockPosition.Value.X;
            Top = _lockPosition.Value.Y;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(this);
            if (Cursor == Cursors.SizeNWSE || Cursor == Cursors.SizeNESW)
            {
                _isResizing = true;
                _redrawTimer.Stop();
                _lastMousePosition = pos;
                DateTextBlock.Visibility = Visibility.Collapsed;
                Mouse.Capture((UIElement)sender);
            }
            else
            {
                if (e.ClickCount == 2)
                {
                    // ダブルクリックで最大化・元に戻す
                    this.WindowState = this.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
                }
                else if (e.ButtonState == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            Point pos = e.GetPosition(this);
            if (_isResizing)
            {
                double deltaX = pos.X - _lastMousePosition.X;
                double deltaY = pos.Y - _lastMousePosition.Y;

                if (this.Cursor == Cursors.SizeNWSE)
                {
                    double delta = Math.Max(deltaX, deltaY);
                    this.Width = Math.Max(this.MinWidth, this.Width + delta);
                    this.Height = this.Width;
                }
                else if (this.Cursor == Cursors.SizeNESW)
                {
                    double delta = Math.Max(-deltaX, deltaY);
                    this.Width = Math.Max(this.MinWidth, this.Width + delta);
                    this.Height = this.Width;
                }

                _lastMousePosition = pos;
                return;
            }

            int _resizeMargin = 10;
            if (pos.X >= this.ActualWidth - _resizeMargin && pos.Y >= this.ActualHeight - _resizeMargin)
            {
                this.Cursor = Cursors.SizeNWSE;
            }
            else if (pos.X <= _resizeMargin && pos.Y >= this.ActualHeight - _resizeMargin)
            {
                this.Cursor = Cursors.SizeNESW;
            }
            else
            {
                this.Cursor = Cursors.Arrow;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isResizing)
            {
                _isResizing = false;
                _redrawTimer.Start();
                this.Cursor = Cursors.Arrow;
                ClockCanvas.Children.Clear();
                DrawClockFace();
                DrawHands();
                UpdateDate();
                Mouse.Capture(null);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            _leaveTimer.Stop();
            MainGrid.Background = Brushes.Black;
            WindowBorder.BorderBrush = Brushes.DarkSlateGray;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            _leaveTimer.Start();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopMostMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            this.Topmost = !item.IsLocked();
            item.ToggleLockState();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LockMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            _lockPosition = item.IsLocked() ? null : (Point?)new Point(Left, Top);
            item.ToggleLockState();

            // alt + tab の表示・非表示を切り替える
            _win32Api.ToggleAltTabVisibility(item.IsLocked() ? Visibility.Collapsed : Visibility.Visible);
            // windows + D のキーをフック
            _winDHook.Toggle(item.IsLocked());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}