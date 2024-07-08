using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Clock
{
    /// <summary>
    /// 
    /// </summary>
    public class Win32Api
    {
        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum ExtendedWindowStyles
        {
            WS_EX_TOOLWINDOW = 0x00000080
        }
        /// <summary>
        /// 
        /// </summary>
        public enum GetWindowLongFields
        {
            GWL_EXSTYLE = -20
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nIndex"></param>
        /// <param name="dwNewLong"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);



        /// <summary> </summary>
        private Window _window;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        public Win32Api(Window window)
        {
            _window = window;
        }
        /// <summary>
        /// alt + tab の表示・非表示を切り替える
        /// </summary>
        /// <param name="flg"></param>
        public  void ToggleAltTabVisibility(Visibility visibility)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(_window);
            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);
            if (visibility == Visibility.Visible)
            {
                exStyle &= ~(int)ExtendedWindowStyles.WS_EX_TOOLWINDOW; // WS_EX_TOOLWINDOW を削除
            }
            else
            {
                exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            }
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }
    }
}
