using AstroClient.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AstroClient.Client
{
    internal class WindowManager
    {
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
        private const int CTRL_CLOSE_EVENT = 2;
        private const int CTRL_C_EVENT = 0;

        public static void Start()
        {
            try
            {
                Console.Title = $"Astro Client {UpdateManager.Version}";
                LogSystem.Log("Console title set.");

                Console.CursorVisible = false;
                LogSystem.Log("Console cursor visibility set to false.");

                Task.Run(ConsoleSystem.KeepWindowSize);
                LogSystem.Log("Console window size set.");

                IntPtr consoleWindow = GetConsoleWindow();
                if (consoleWindow == IntPtr.Zero)
                {
                    LogSystem.Log("Failed to get console window handle.");
                }
                else
                {
                    LogSystem.Log("Console window handle obtained.");

                    int windowStyle = GetWindowLong(consoleWindow, GWL_STYLE);
                    LogSystem.Log("Original window style retrieved.");

                    windowStyle &= ~WS_MAXIMIZEBOX;
                    windowStyle &= ~WS_SIZEBOX;
                    LogSystem.Log("Maximize box and sizing border removed from window style.");

                    SetWindowLong(consoleWindow, GWL_STYLE, windowStyle);
                    LogSystem.Log("New window style set.");

                    ShowScrollBar(consoleWindow, SB_BOTH, false);
                    LogSystem.Log("Scrollbars hidden.");
                }
                LogSystem.Log("Console control handler set.");

            }
            catch (Exception ex)
            {
                ConsoleSystem.SetColor(System.Drawing.Color.DarkRed);
                ConsoleSystem.AnimatedText($"Error in WindowManager. Check logs for details.");
                LogSystem.ReportError($"Error in Enable method: {ex}");
            }
        }
    }
}
