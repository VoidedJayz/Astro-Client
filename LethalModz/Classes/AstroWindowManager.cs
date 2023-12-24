using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Astro.Classes
{
    internal class AstroWindowManager
    {
        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;
        private const int WS_SIZEBOX = 0x40000;
        private const int SB_BOTH = 3;

        public static async Task Enable()
        {
            Console.Title = $"Astro Boyz {AstroUtils.currentVersion}";
            Console.CursorVisible = false;
            Console.SetWindowSize(100, 30);
            IntPtr consoleWindow = GetConsoleWindow();

            // Remove maximize button and resizing
            int windowStyle = GetWindowLong(consoleWindow, GWL_STYLE);

            // Remove maximize box and sizing border
            windowStyle &= ~WS_MAXIMIZEBOX;
            windowStyle &= ~WS_SIZEBOX;

            // Set the new style
            SetWindowLong(consoleWindow, GWL_STYLE, windowStyle);

            // Hide scrollbars
            ShowScrollBar(consoleWindow, SB_BOTH, false);

        }
    }
}
