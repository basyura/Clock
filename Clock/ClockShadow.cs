using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Clock
{
    public class ClockShadow
    {
        /// <summary></summary>
        private MainWindow _w;
        /// <summary></summary>
        private Condition _condition = new Condition();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        public ClockShadow(MainWindow window)
        {
            _w = window; 
        }
        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            _w.ClockCanvas.Children.Clear();

            DateTime now = DateTime.Now;

            DrawClockFace(now);
            DrawHands(now);
            DrawDate(now);
            UpdatePosition();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        public void UpdateLockedPosition(Point? point)
        {
            _condition.LockPosition = point;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="flg"></param>
        public void UpdateTimeVisibility(bool flg)
        {
            _condition.IsShowTime = flg;
        }
        /// <summary>
        /// 
        /// </summary>
        public void HideDateTime()
        {
            _w.DateTextBlock.Visibility = Visibility.Collapsed;
            _w.TimeTextBlock.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 
        /// </summary>
        private void DrawClockFace(DateTime now)
        {
            double radius = Math.Min(_w.ClockCanvas.ActualWidth, _w.ClockCanvas.ActualHeight) / 2 - 10;

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

            Canvas.SetLeft(face, _w.ClockCanvas.ActualWidth / 2 - radius);
            Canvas.SetTop(face, _w.ClockCanvas.ActualHeight / 2 - radius);
            _w.ClockCanvas.Children.Add(face);

            // 数字を描画
            DrawNumbers(radius);
            // 目盛線を描画
            DrawTicks(radius, now);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        private void DrawNumbers(double radius)
        {
            double centerX = _w.ClockCanvas.ActualWidth / 2;
            double centerY = _w.ClockCanvas.ActualHeight / 2;

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
                _w.ClockCanvas.Children.Add(number);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        private void DrawTicks(double radius, DateTime now)
        {
            Point center = new Point(_w.ClockCanvas.ActualWidth / 2, _w.ClockCanvas.ActualHeight / 2);
            // 現在時刻に一致する場合に色をつける
            int m = -1;
            if (now.Minute % 5 == 0)
            {
                m = now.Minute / 5;
            }
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

                // 現在時刻と一致した
                if (i == m)
                {
                    hourTick.Stroke = Brushes.Red;
                    hourTick.Y1 -= 5;
                }


                _w.ClockCanvas.Children.Add(hourTick);
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
                    // 現在時刻と一致した場合に色をつける
                    if (now.Minute == i)
                    {
                        minuteTick.Stroke = Brushes.Red;
                        //minuteTick.Y2 += 2;
                        minuteTick.Y1 -= 5;
                    }
                    _w.ClockCanvas.Children.Add(minuteTick);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void DrawHands(DateTime now)
        {
            double radius = Math.Min(_w.ClockCanvas.ActualWidth, _w.ClockCanvas.ActualHeight) / 2 - 10;
            Point center = new Point(_w.ClockCanvas.ActualWidth / 2, _w.ClockCanvas.ActualHeight / 2);

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

            _w.ClockCanvas.Children.Add(hand);
        }
        /// <summary>
        /// 
        /// </summary>
        private void DrawDate(DateTime now)
        {
            string dayOfWeek = now.ToString("ddd");
            _w.DateTextBlock.Text = $"{now.Month}/{now.Day} ({dayOfWeek})";
            _w.DateTextBlock.Visibility = Visibility.Visible;


            if (_condition.IsShowTime)
            {
                _w.TimeTextBlock.Text = now.ToString("HH:mm");
                _w.TimeTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                _w.TimeTextBlock.Visibility = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void UpdatePosition()
        {
            if (_condition.LockPosition== null)
            {
                return;
            }

            _w.Left = _condition.LockPosition.Value.X;
            _w.Top = _condition.LockPosition.Value.Y;
        }
    }
}
