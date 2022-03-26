using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForceSensorPanelToMonitor
{
    public class Monitor
    {
        /// <summary>
        /// To which video adapter is this monitor connected
        /// </summary>
        public WinApi.LUID AdapterId { get; set; }

        /// <summary>
        /// This is the ID of this monitor on this video adapter (this number is only unique for the target adapter).
        /// </summary>
        public uint TargetMonitorId { get; set; }

        /// <summary>
        /// This is the ID of this monitor in the source list (this number is only unique for the source adapter).
        /// </summary>
        public uint SourceMonitorId { get; internal set; }

        /// <summary>
        /// The display name of the monitor.  This is not guaranteed to be unique either.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets the device name associated with a display.
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets a value indicating whether this display is the primary display device.
        /// </summary>
        public bool IsPrimary { get; set;  }

        /// <summary>
        ///    Gets the working area of the display. The working area is the desktop area of
        //     the display, excluding taskbars, docked windows, and docked tool bars.
        /// </summary>
        public Rectangle WorkingArea { get; set; }

        /// <summary>
        /// Gets the bounds of the display.
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// The active size of the display.
        /// </summary>
        public WinApi.DISPLAYCONFIG_2DREGION ActiveSize { get; set; }

        /// <summary>
        /// The number of pixels this monitor currently supports.  Used for sorting
        /// by resolution.
        /// </summary>
        public int PixelCount { get; internal set; }
    }
}
