using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ForceSensorPanelToMonitor
{
    internal static class Program
    {
        [DllImport("kernel32", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var args = Environment.GetCommandLineArgs();

            var startMinimized = false;
            if (null != args && args.Length > 0)
            {
                startMinimized = args.Any(a => a.ToLower() == "/starttotray");
                var wantHelp = args.Any(a => a == "/?" || a.ToLower() == "--help");

                if (wantHelp)
                {
                    // To display the usage, we need to attach to the existing console, if any
                    var ptr = GetForegroundWindow();
                    GetWindowThreadProcessId(ptr, out int u);
                    var process = Process.GetProcessById(u);
                    AttachConsole(process.Id);

                    Console.WriteLine(@"

Usage: ForceSensorPanelToMonitor [/StartToTray]

/StartToTray - Starts the application minimized to the tray.  Clicking on the icon in the tray brings up the settings dialog.
/? - This text.

");

                    return;
                }
            }


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = new formMain1(startMinimized);

            Application.Run(mainForm);
        }
    }
}
