using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ForceSensorPanelToMonitor
{
    public class ApplicationWindow
    {
        public string    ClassName     { get; }
        public IntPtr    hWnd          { get; }
        public string    WindowTitle   { get; }
        public Rectangle WindowLocation { get; }

        public ApplicationWindow(IntPtr hWnd, string className, string windowTitle, Rectangle windowLocation)
        {
            this.hWnd = hWnd;
            this.ClassName = className;
            this.WindowTitle = windowTitle;
            this.WindowLocation = windowLocation;
        }
    }

    public static class DisplayTools
    {
        public const int ERROR_SUCCESS = 0;

        private static string MonitorFriendlyName(WinApi.LUID adapterId, uint targetId)
        {
            var deviceName = new WinApi.DISPLAYCONFIG_TARGET_DEVICE_NAME
            {
                header =
                {
                    size = (uint)Marshal.SizeOf(typeof (WinApi.DISPLAYCONFIG_TARGET_DEVICE_NAME)),
                    adapterId = adapterId,
                    id = targetId,
                    type = WinApi.DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME
                }
            };
            var error = WinApi.DisplayConfigGetDeviceInfo(ref deviceName);
            if (error != ERROR_SUCCESS)
                throw new Win32Exception(error);
            return deviceName.monitorFriendlyDeviceName;
        }

        private static IEnumerable<string> GetAllMonitorsFriendlyNames()
        {
            var error = WinApi.GetDisplayConfigBufferSizes(WinApi.QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS, 
                                                               out var pathCount, 
                                                               out var modeCount);
            if (error != ERROR_SUCCESS)
                throw new Win32Exception(error);

            var displayPaths = new WinApi.DISPLAYCONFIG_PATH_INFO[pathCount];
            var displayModes = new WinApi.DISPLAYCONFIG_MODE_INFO[modeCount];
            error = WinApi.QueryDisplayConfig(WinApi.QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS,
                ref pathCount, displayPaths, ref modeCount, displayModes, IntPtr.Zero);
            if (error != ERROR_SUCCESS)
                throw new Win32Exception(error);

            for (var i = 0; i < modeCount; i++)
                if (displayModes[i].infoType == WinApi.DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
                    yield return MonitorFriendlyName(displayModes[i].adapterId, displayModes[i].id);
        }

        public static List<string> GetFriendlyNames(this Screen screen)
        {
            var allFriendlyNames = GetAllMonitorsFriendlyNames();
            return allFriendlyNames.ToList<string>();
        }

        public static List<Screen> GetAllMonitorInfo()
        {
            return Screen.AllScreens.ToList<Screen>();
        }

        public static string DeviceFriendlyName(this Screen screen)
        {
            var allFriendlyNames = GetAllMonitorsFriendlyNames();
            for (var index = 0; index < Screen.AllScreens.Length; index++)
                if (Equals(screen, Screen.AllScreens[index]))
                    return allFriendlyNames.ToArray()[index];
            return null;
        }

        public static IntPtr FindWindowByClassName(string className)
        {
            return WinApi.FindWindow(className, null);
        }

        public static void MoveWindow(IntPtr hWnd, System.Drawing.Point location, System.Drawing.Size size)
        {
            WinApi.MoveWindow(hWnd, location.X, location.Y, size.Width, size.Height, true);
        }

        public static string GetClassName(IntPtr hWnd)
        {
            // Pre-allocate 256 characters, since this is the maximum class name length.
            var className = new StringBuilder(256);

            if (WinApi.GetClassName(hWnd, className, className.Capacity) != 0)
            {
                return className.ToString();
            }

            return "";
        }

        /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
        /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
        public static IDictionary<IntPtr, ApplicationWindow> GetOpenWindows()
        {
            var shellWindow = WinApi.GetShellWindow();
            var windows = new Dictionary<IntPtr, ApplicationWindow>();

            WinApi.EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!WinApi.IsWindowVisible(hWnd)) return true;

                var length = WinApi.GetWindowTextLength(hWnd);
                if (length == 0) return true;

                var builder = new StringBuilder(length);
                WinApi.GetWindowText(hWnd, builder, length + 1);

                if (WinApi.HRESULT.S_OK == WinApi.DwmGetWindowAttribute(hWnd, 
                                                                        WinApi.DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS,
                                                                        out WinApi.RECT rect,
                                                                        Marshal.SizeOf(typeof(WinApi.RECT))))

//                if (WinApi.GetWindowRect(hWnd, out WinApi.RECT rect))
                {
                    var windowLocation = new Rectangle(rect.Left, rect.Top, 
                                                       rect.Right- rect.Left, rect.Bottom- rect.Top);
                    windows[hWnd] = new ApplicationWindow(hWnd, 
                                                          GetClassName(hWnd), 
                                                          builder.ToString(), 
                                                          windowLocation);
                }

                return true;

            }, 0);

            return windows;
        }
    }
}
