using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        private WinDHook _winDHook;
        /// <summary> </summary>
        private ClockShadow _shadow;
        /// <summary>
        /// 
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            _shadow = new ClockShadow(this);

            _win32Api = new Win32Api(this);
            _winDHook = new WinDHook(this);

            // 再描画タイマー
            _redrawTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1),
            };
            _redrawTimer.Tick += RedrawTimer_Tick;

            // 背景透明化用タイマー
            _leaveTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(0.5),
            };
            _leaveTimer.Tick += LeaveTimer_Tick;


            Loaded += (s, e) => _redrawTimer.Start();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RedrawTimer_Tick(object sender, EventArgs e)
        {
            // モニタの表示範囲内にいない場合
            if (!this.IsWindowWithinAnyScreen())
            {
                // ロックされていたら解除する
                if (LockMenu.IsLocked())
                {
                    ToggleLockState(LockMenu);
                }
                // 左上に移動する
                _shadow.Move(0, 0);
            }

            _shadow.Update();
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

            _shadow.Update();
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
                _shadow.HideDateTime();
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
                _shadow.Update();
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
            ToggleLockState(item);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        private void ToggleLockState(MenuItem item)
        {
            // on/off 切り替え
            item.ToggleLockState();

            // 状態更新
            _shadow.UpdateLockedPosition(item.IsLocked() ? (Point?)new Point(Left, Top) : null);

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
        private void TimeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            item.ToggleLockState();
            // 状態更新
            _shadow.UpdateTimeVisibility(item.IsLocked());
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