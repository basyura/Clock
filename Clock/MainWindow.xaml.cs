using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Clock
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private DispatcherTimer leaveTimer;
        private const int ResizeMargin = 10;
        private bool resizing = false;
        private Point lastMousePosition;

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            leaveTimer = new DispatcherTimer();
            leaveTimer.Interval = TimeSpan.FromSeconds(0.5);
            leaveTimer.Tick += LeaveTimer_Tick;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Start();
            DrawClockFace();
            UpdateDate();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ClockCanvas.Children.Clear();
            DrawClockFace();
            DrawHands();
            UpdateDate();
        }

        private void LeaveTimer_Tick(object sender, EventArgs e)
        {
            leaveTimer.Stop();
            MainGrid.Background = Brushes.Transparent;
            WindowBorder.BorderBrush = Brushes.Transparent;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!resizing)
            {
                ClockCanvas.Children.Clear();
                DrawClockFace();
                DrawHands();
                UpdateDate();
            }
        }

        private void DrawClockFace()
        {
            double radius = Math.Min(ClockCanvas.ActualWidth, ClockCanvas.ActualHeight) / 2 - 10;

            // キャンバスのサイズが有効か確認します
            if (radius <= 0) return;

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
        }

        private void DrawNumbers(double radius)
        {
            double centerX = ClockCanvas.ActualWidth / 2;
            double centerY = ClockCanvas.ActualHeight / 2;

            for (int i = 0; i < 12; i++)
            {
                double angle = i * 30 * Math.PI / 180;
                double x = centerX + (radius - 20) * Math.Cos(angle - Math.PI / 2);
                double y = centerY + (radius - 20) * Math.Sin(angle - Math.PI / 2);

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

        private void DrawHands()
        {
            DateTime now = DateTime.Now;
            double radius = Math.Min(ClockCanvas.ActualWidth, ClockCanvas.ActualHeight) / 2 - 10;
            Point center = new Point(ClockCanvas.ActualWidth / 2, ClockCanvas.ActualHeight / 2);

            // キャンバスのサイズが有効か確認します
            if (radius <= 0) return;

            // Hour hand
            DrawHand(center, radius * 0.5, (now.Hour % 12 + now.Minute / 60.0) * 30, Brushes.White, 6);

            // Minute hand
            DrawHand(center, radius * 0.7, (now.Minute + now.Second / 60.0) * 6, Brushes.White, 4);

            // Second hand
            DrawHand(center, radius * 0.9, now.Second * 6, Brushes.Red, 2);
        }

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

        private void UpdateDate()
        {
            DateTime now = DateTime.Now;
            string dayOfWeek = now.ToString("ddd");
            DateTextBlock.Text = $"{now.Month}/{now.Day} ({dayOfWeek})";
            DateTextBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);
            if (this.Cursor == Cursors.SizeNWSE || this.Cursor == Cursors.SizeNESW)
            {
                resizing = true;
                timer.Stop();
                lastMousePosition = pos;
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

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            if (resizing)
            {
                double deltaX = pos.X - lastMousePosition.X;
                double deltaY = pos.Y - lastMousePosition.Y;

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

                lastMousePosition = pos;
                return;
            }

            if (pos.X >= this.ActualWidth - ResizeMargin && pos.Y >= this.ActualHeight - ResizeMargin)
            {
                this.Cursor = Cursors.SizeNWSE;
            }
            else if (pos.X <= ResizeMargin && pos.Y >= this.ActualHeight - ResizeMargin)
            {
                this.Cursor = Cursors.SizeNESW;
            }
            else
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (resizing)
            {
                resizing = false;
                this.Cursor = Cursors.Arrow;
                timer.Start();
                ClockCanvas.Children.Clear();
                DrawClockFace();
                DrawHands();
                UpdateDate();
                Mouse.Capture(null);
            }
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            leaveTimer.Stop();
            MainGrid.Background = Brushes.Black;
            WindowBorder.BorderBrush = Brushes.DarkSlateGray;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            leaveTimer.Start();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}