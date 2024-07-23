using System.Linq;
using System.Windows;

namespace Clock.Extensions
{
    public static class WidowExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static bool IsWindowWithinAnyScreen(this Window window)
        {
            // ウィンドウの位置とサイズを取得
            var windowRect = new System.Drawing.Rectangle(
                (int)window.Left,
                (int)window.Top,
                (int)window.Width,
                (int)window.Height);

            // すべてのモニタの表示範囲を取得
            var screens = System.Windows.Forms.Screen.AllScreens;

            // ウィンドウがいずれかのモニタ内に収まっているかをチェック
            return screens.Any(screen => screen.WorkingArea.IntersectsWith(windowRect));
        }
    }
}
