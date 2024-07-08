using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FormKeys = System.Windows.Forms.Keys;

namespace Clock.WinApi
{
    public class WinDHook
    {
        /// <summary></summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        /// <summary></summary>
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        /// <summary></summary>
        private KeyboardHook _keyboardHook;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="proc"></param>
        private Window _window;
        public WinDHook(Window window)
        {
            _window = window;
            _keyboardHook = new KeyboardHook(Hook);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Toggle(bool isOn)
        {
            if (isOn)
            {
                _keyboardHook.SetHook();
            }
            else
            {
                _keyboardHook.Unhook();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private IntPtr Hook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (_window.Topmost)
            {
                return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            }

            // WM_KEYDOWN
            if (nCode >= 0 && wParam == (IntPtr)0x0100)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if ((GetAsyncKeyState((int)FormKeys.LWin) & 0x8000) != 0 && vkCode == (int)FormKeys.D)
                {
                    _window.Topmost = true;
                    Task.Run(() =>
                    {
                        Thread.Sleep(100);
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _window.Topmost = false;
                        }));
                    });
                }
            }

            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }
    }
}
