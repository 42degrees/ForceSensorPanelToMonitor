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
            var targetDeviceName = new WinApi.DISPLAYCONFIG_TARGET_DEVICE_NAME
            {
                header =
                {
                    size = (uint)Marshal.SizeOf(typeof (WinApi.DISPLAYCONFIG_TARGET_DEVICE_NAME)),
                    adapterId = adapterId,
                    id = targetId,
                    type = WinApi.DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME
                }
            };
            var error = WinApi.DisplayConfigGetDeviceInfo(ref targetDeviceName);
            if (error != ERROR_SUCCESS)
                throw new Win32Exception(error);
            return targetDeviceName.monitorFriendlyDeviceName;
        }

        private static string MonitorSource(WinApi.LUID adapterId, uint targetId)
        {
            var sourceDeviceName = new WinApi.DISPLAYCONFIG_SOURCE_DEVICE_NAME
            {
                header =
                {
                    size = (uint)Marshal.SizeOf(typeof (WinApi.DISPLAYCONFIG_SOURCE_DEVICE_NAME)),
                    adapterId = adapterId,
                    id = targetId,
                    type = WinApi.DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME
                }
            };
            var error = WinApi.DisplayConfigGetDeviceInfo(ref sourceDeviceName);
            if (error != ERROR_SUCCESS)
                throw new Win32Exception(error);
            return sourceDeviceName.viewGdiDeviceName;
        }

        /// <summary>
        /// Check whether current session is locked. 
        /// Note: If UAC popups a secure desktop, this method may also fail. 
        ///       There is no API for differentiating between Locked Desktop
        ///       and UAC Secure Desktop.
        ///       
        /// Based on code from OneCodeTeam in GitHub: 
        /// https://github.com/microsoftarchive/msdn-code-gallery-microsoft
        /// </summary>
        public static bool IsLocked()
        {
            var hDesktop = IntPtr.Zero;
            try
            {

                // Opens the desktop that receives user input.
                hDesktop = WinApi.OpenInputDesktop(dwFlags: 0,
                                                   fInherit: false,
                                                   dwDesiredAccess: WinApi.ACCESS_MASK.DESKTOP_READOBJECTS 
                                                                  | WinApi.ACCESS_MASK.DESKTOP_WRITEOBJECTS);
            }
            finally
            {
                // Close an open handle to a desktop object.
                if (IntPtr.Zero != hDesktop)
                {
                    WinApi.CloseDesktop(hDesktop);
                }
            }

            // If hDesktop is IntPtr.Zero, then the session is locked.
            return IntPtr.Zero == hDesktop;
        }

        public static IEnumerable<Monitor> GetAllMonitors()
        {
            if (IsLocked())
            {
                // The current session is locked, don't bother checking.
                return new List<Monitor>();
            }

            var error = WinApi.GetDisplayConfigBufferSizes(WinApi.QUERY_DEVICE_CONFIG_FLAGS.QDC_DATABASE_CURRENT, 
                                                           out var pathCount, 
                                                           out var modeCount);
            if (error != ERROR_SUCCESS)
                throw new Win32Exception(error);

            var displayPaths = new WinApi.DISPLAYCONFIG_PATH_INFO[pathCount];
            var displayModes = new WinApi.DISPLAYCONFIG_MODE_INFO[modeCount];
            uint currentTopologyId = 0;

            error = WinApi.QueryDisplayConfig(WinApi.QUERY_DEVICE_CONFIG_FLAGS.QDC_DATABASE_CURRENT,
                                              ref pathCount, 
                                              displayPaths, 
                                              ref modeCount, 
                                              displayModes,
                                              out currentTopologyId);

            if (error != ERROR_SUCCESS)
                throw new Win32Exception(error);

            if (!Enum.IsDefined(typeof(WinApi.DisplayConfigTopology), currentTopologyId))
            {
                throw new InvalidTopologyException("The current topology is unknown");
            }

            var currentTopology = (WinApi.DisplayConfigTopology)currentTopologyId;

            if (currentTopology != WinApi.DisplayConfigTopology.DISPLAYCONFIG_TOPOLOGY_EXTEND)
            {
                throw new InvalidTopologyException("The current topology is {currentTopology}, but this only works if the topology is DISPLAYCONFIG_TOPOLOGY_EXTEND.");
            }

            var screens = Screen.AllScreens;

            var monitors = new List<Monitor>();
            foreach (var displayPath in displayPaths)
            {
                WinApi.DISPLAYCONFIG_MODE_INFO currentDisplayTarget, currentDisplaySource;
                Screen currentScreen;
                try
                {
                    currentDisplayTarget = displayModes.Single(mode => mode.infoType == WinApi.DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET
                                                                    && mode.adapterId.Equals(displayPath.targetInfo.adapterId)
                                                                    && mode.id == displayPath.targetInfo.id);

                    currentDisplaySource = displayModes.Single(mode => mode.infoType == WinApi.DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE
                                                                    && mode.adapterId.Equals(displayPath.sourceInfo.adapterId)
                                                                    && mode.id == displayPath.sourceInfo.id);

                    currentScreen = screens.Single(screen => screen.Bounds.Left == currentDisplaySource.modeInfo.sourceMode.position.x
                                                          && screen.Bounds.Top == currentDisplaySource.modeInfo.sourceMode.position.y);
                }
                catch (InvalidOperationException)
                {
                    // This can happen if an event fires while the console is locked or we fire while the UAC
                    // secure desktop is active.  We should now be detecting this above in the "IsLocked()" method
                    // but I'm putting this here as well, just in case there are other scenarios we aren't catching.
                    continue;
                }

                var sourceAdapterId = currentDisplaySource.adapterId;
                var targetAdapterId = currentDisplayTarget.adapterId;

                var monitor = new Monitor()
                {
                    TargetMonitorId = currentDisplayTarget.id,
                    SourceMonitorId = currentDisplaySource.id,
                    AdapterId = currentDisplayTarget.adapterId,
                    Bounds = currentScreen.Bounds,
                    DeviceName = currentScreen.DeviceName,
                    IsPrimary = currentScreen.Primary,
                    WorkingArea = currentScreen.WorkingArea,
                    ActiveSize = currentDisplayTarget.modeInfo.targetMode.targetVideoSignalInfo.activeSize,
                    PixelCount = currentScreen.Bounds.Width * currentScreen.Bounds.Height,
                };

                monitor.FriendlyName = MonitorFriendlyName(monitor.AdapterId, monitor.TargetMonitorId);

                monitors.Add(monitor);
            }

            // Combine any monitors that are using the same source, and remove the duplicates.
            foreach (var deviceName in monitors.Select(monitor => monitor.DeviceName).Distinct().ToList())
            {
                var monitorsAttachedToDevice = monitors.Where(monitor => monitor.DeviceName == deviceName).ToList();

                if (monitorsAttachedToDevice.Count() > 1)
                {
                    var monitorToKeep = monitorsAttachedToDevice.First();

                    foreach (var monitor in monitorsAttachedToDevice.Skip(1))
                    {
                        monitorToKeep.FriendlyName = monitorToKeep.FriendlyName + "/" + monitor.FriendlyName;
                        monitors.Remove(monitor);
                    }
                }
            }

            return monitors;
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
