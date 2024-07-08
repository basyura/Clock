using System.Windows.Controls;

namespace Clock.Extensions
{
    public static class MenuItemExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsLocked(this MenuItem self)
        {
            if (self.Icon == null)
            {
                return false;
            }

            return self.Icon.ToString() == "✔";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static void ToggleLockState(this MenuItem self)
        {
            self.Icon = self.IsLocked() ? "" : "✔";
        }
    }
}
