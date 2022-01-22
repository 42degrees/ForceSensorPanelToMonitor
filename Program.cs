using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ForceSensorPanelToMonitor
{
    internal static class Program
    {
        public static readonly int WM_SHOWFIRSTINSTANCE = WinApi.RegisterWindowMessage($"WM_SHOWFIRSTINSTANCE|{WinApi.AssemblyGuid}");

        /// <summary>
        /// Tell our hiding sibling to wake up.
        /// </summary>
        static public void TickleSiblingProcess()
        {
            // We don't bother trying to find our hiding sibling, we just
            // send our custom message to all windows and let our sibling
            // catch it.
            WinApi.SendNotifyMessage((IntPtr)WinApi.HWND_BROADCAST,
                                     WM_SHOWFIRSTINSTANCE,
                                     IntPtr.Zero,
                                     IntPtr.Zero);
        }

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
                    var ptr = WinApi.GetForegroundWindow();
                    WinApi.GetWindowThreadProcessId(ptr, out int u);
                    var process = Process.GetProcessById(u);
                    WinApi.AttachConsole(process.Id);

                    Console.WriteLine(@"

Usage: ForceSensorPanelToMonitor [/StartToTray]

/StartToTray - Starts the application minimized to the tray.  Clicking on the icon in the tray brings up the settings dialog.
/? - This text.

");

                    return;
                }
            }

            // We allow one copy per logged-in session, hence the Local\ prefix and the userId as part of the Mutex.
            string userId = System.Security.Principal.WindowsIdentity.GetCurrent().User.Value;

            // Make sure there is no other copy running yet.  
            // Keep the mutex from being garbage collected until main exits
            using var mutex = new Mutex(true, $"Local\\ForceSensorPanelToMonitor-{userId}-{WinApi.AssemblyGuid}", out bool createdNew);

            if (!createdNew)
            {
                // If they've asked us to start minimized, then clearly they don't
                // want the window popping up.  In that case just silently exit.
                if (!startMinimized)
                    TickleSiblingProcess();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = new SettingsDialog(startMinimized);

            Application.Run(mainForm);
        }
    }
}
