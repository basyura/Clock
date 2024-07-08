using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Clock.WinApi
{
    public class KeyboardHook
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        /// <summary></summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        /// <summary></summary>
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        /// <summary></summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        /// <summary></summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary> </summary>
        private const int WH_KEYBOARD_LL = 13;
        /// <summary></summary>
        private IntPtr _hookID = IntPtr.Zero;
        /// <summary></summary>
        private readonly LowLevelKeyboardProc _proc;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="proc"></param>
        public KeyboardHook(LowLevelKeyboardProc proc)
        {
            _proc = proc;
        }
        /// <summary>
        /// 
        /// </summary>
        public void SetHook()
        {
            _hookID = SetHook(_proc);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Unhook()
        {
            UnhookWindowsHookEx(_hookID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="proc"></param>
        /// <returns></returns>
        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
    }
}