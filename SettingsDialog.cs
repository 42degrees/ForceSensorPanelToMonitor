using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Win32.TaskScheduler;
using System.Windows.Forms;
using System.IO;
using System.Security.Policy;
using System.Security.Principal;
using ForceSensorPanelToMonitor.Properties;

namespace ForceSensorPanelToMonitor
{
    public partial class SettingsDialog : Form
    {
        private static readonly string ForceSensorPanelToMonitorTaskName = "ForceSensorPanelToMonitor";

        private static SessionSwitchEventHandler _sessionSwitchEventHandler;

        private readonly bool _startMinimal = false;

        private Timer _timer = null;
        private Timer _savedTimer = null;

        public SettingsDialog(bool startMinimal = false)
        {
            _startMinimal = startMinimal;
            InitializeComponent();
            SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);

            _sessionSwitchEventHandler = new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            SystemEvents.SessionSwitch += _sessionSwitchEventHandler;
        }

        ~SettingsDialog()
        {
            // Remove the static handler so that we don't leak
            SystemEvents.SessionSwitch -= _sessionSwitchEventHandler;
        }

        #region Overrides

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == Program.WM_SHOWFIRSTINSTANCE)
            {
                RestoreWindow();
            }
            base.WndProc(ref message);
        }

        #endregion

        #region Event Handlers

        private void FillMonitorList()
        {
            var savedMonitorName = Settings.Default.MonitorName;

            // Note that this list can contain monitors that mirror another.
            var allMonitors = DisplayTools.GetAllMonitors();

            cbMonitorList.Items.Clear();
            foreach (var monitor in allMonitors.OrderBy(monitor => monitor.FriendlyName))
            {
                var index = cbMonitorList.Items.Add(monitor.FriendlyName);
                if (savedMonitorName == cbMonitorList.Items[index].ToString())
                {
                    cbMonitorList.SelectedIndex = index;
                }
            }

            // If no monitors were selected, let's see if maybe there was
            // a mirrored monitor selected previously, but the monitors have
            // been split back up.  Pick the lowest resolution monitor from the
            // group that was split.
            if (cbMonitorList.SelectedIndex == -1)
            {
                var monitorNameForTest = "/" + savedMonitorName + "/";
                var selectedMonitors = allMonitors.Where(monitor => monitorNameForTest.Contains("/" + monitor.FriendlyName + "/"))
                                                  .OrderBy(monitor => monitor.PixelCount);
                if (selectedMonitors.Count() > 0)
                {
                    cbMonitorList.SelectedIndex = cbMonitorList.FindStringExact(selectedMonitors.First().FriendlyName);
                }
            }
        }

        private void FormMain1_Load(object sender, EventArgs e)
        {
            var savedMonitorName = Settings.Default.MonitorName;
            var savedWindowClass = Settings.Default.WindowClass;
            var savedRefreshRate = Settings.Default.RefreshRate;
            var savedPreventOtherApplications = Settings.Default.PreventOtherApplications;

            // Add all the monitors to the drop-down list
            FillMonitorList();

            var intervalValues = new List<IntervalComboBoxItem>
            {
                new IntervalComboBoxItem { itemText = "Manual Only"                     },
                new IntervalComboBoxItem { itemText = "10 seconds", seconds = 10        },
                new IntervalComboBoxItem { itemText = "30 seconds", seconds = 30        },
                new IntervalComboBoxItem { itemText = "1 minute",   seconds = 1 * 60    },
                new IntervalComboBoxItem { itemText = "10 minutes", seconds = 10 * 60   },
                new IntervalComboBoxItem { itemText = "20 minutes", seconds = 20 * 60   },
                new IntervalComboBoxItem { itemText = "60 minutes", seconds = 60 * 60   },
                new IntervalComboBoxItem { itemText = "2 hours",    seconds = 120 * 60  },
                new IntervalComboBoxItem { itemText = "4 hours",    seconds = 240 * 60  },
                new IntervalComboBoxItem { itemText = "8 hours",    seconds = 480 * 60  },
                new IntervalComboBoxItem { itemText = "1 day",      seconds = 1440 * 60 },
            };

            cbRefreshInterval.DataSource = intervalValues;

            var previousIndex = intervalValues.FindIndex(interval => interval.seconds == savedRefreshRate);

            cbRefreshInterval.SelectedIndex = 0;
            if (previousIndex > 0)
            {
                cbRefreshInterval.SelectedIndex = previousIndex;
            }

            tbWindowClass.Text = savedWindowClass;

            if (_startMinimal)
            {
                WindowState = FormWindowState.Minimized;
            }

            // Set the start at login checkbox if we found a scheduled task with our name on it
            if (ScheduledTaskExists())
            {
                chkRunAtStartup.Checked = true;
            }

            chkPreventOtherApplications.Checked = savedPreventOtherApplications;

            // Run once when we first start
            SyncSensorPanel();
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            // Run the algorithm to sync the window with the monitor
            SyncSensorPanel();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Ask the user if they really intended to close the app.  If they did, then close it down. Otherwise just return false;
        /// </summary>
        /// <returns>true if the user really wanted to close the application.</returns>
        private void FormMain1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.WindowsShutDown)
            {
                var dr = MessageBox.Show(
                    "If you exit the app, the sensor panel may move.  "
                    + "Minimize if you want to hide the app in the tray.  "
                    + "Are you sure you want to quit?",
                    "Leaving App",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dr == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        public void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            // Update the monitors in the drop-down list, as monitors may have been added
            FillMonitorList();

            SyncSensorPanel();
        }

        /// <summary>
        /// This timer is the main timer that is used to move the panel back to where it needs to be.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyTimer_Tick(object sender, EventArgs e)
        {
            SyncSensorPanel();
            _timer.Start();
        }

        /// <summary>
        /// This method turns off the Message label after the set timer expires.  It does not restart the timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveTimer_Tick(object sender, EventArgs e)
        {
            lblMessage.Text = "";
        }

        private void FormMain1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
                trayIcon.Visible = true;
                trayIcon.ShowBalloonTip(1000);
            }
        }

        /// <summary>
        /// Someone double-clicked on the icon in the tray, bring it to the front.
        /// </summary>
        private void TrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            RestoreWindow();
        }

        /// <summary>
        /// Someone clicked on the icon in the tray, bring it to the front.
        /// </summary>
        private void TrayIcon_Click(object sender, EventArgs e)
        {
            RestoreWindow();
        }

        private void CbRefreshInterval_SelectedIndexChanged(object sender, EventArgs e)
        {
            // If they have selected any value except for the first item (manual only) then set a timer.
            var currentSelection = cbRefreshInterval.SelectedItem as IntervalComboBoxItem;

            if (null != currentSelection && currentSelection.seconds > 0)
            {
                if (null != _timer)
                {
                    _timer.Stop();
                    _timer = null;
                }

                _timer = new Timer();
                _timer.Interval = (currentSelection.seconds * 1000); // convert seconds to milliseconds
                _timer.Tick += new EventHandler(MyTimer_Tick);
                _timer.Start();
            }
        }

        private void ChkRunAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (chkRunAtStartup.Checked)
            {
                // Register ourselves to run minimized when the user logs in.
                CreateScheduledTask();
            }
            else
            {
                // Remove any registration
                RemoveScheduledTask();
            }
        }

        /// <summary>
        /// Called when the session switch event is triggered
        /// </summary>
        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLogon
                || e.Reason == SessionSwitchReason.SessionUnlock
                || e.Reason == SessionSwitchReason.ConsoleConnect)
            {
                SyncSensorPanel();
            }
        }

        #endregion

        /// <summary>
        /// Bring the settings dialog out of the tray and into focus.  Note that this could happen
        /// for a number of different scenarios, so we can't make too many assumptions about our
        /// state.
        /// </summary>
        private void RestoreWindow()
        {
            ShowInTaskbar = true;
            trayIcon.Visible = false;
            WindowState = FormWindowState.Normal;

            // We do this in case we were tickled by a new execution of ourselves,
            // since we likely weren't in the front of the other windows.
            WinApi.ShowWindow(this.Handle, WinApi.SW_SHOWNORMAL);
            WinApi.SetForegroundWindow(this.Handle);
        }

        /// <summary>
        /// Searches through all of the attached monitors and returns the one they have selected in the UI.
        /// </summary>
        /// <returns>The chosen monitor or null if the monitor could not be found.</returns>
        private Monitor GetChosenMonitor()
        {
            var allMonitors = DisplayTools.GetAllMonitors();
            var currentMonitor = allMonitors.SingleOrDefault(monitor => monitor.FriendlyName == (string)cbMonitorList.SelectedItem);
            return currentMonitor;
        }

        /// <summary>
        /// This is the workhorse that actually forces the sensor panel window to be the same size
        /// as the chosen monitor.  It also triggers moving other erroneous windows off the sensor
        /// panel if that feature has been enabled.
        /// </summary>
        private void SyncSensorPanel()
        {
            var chosenScreen = GetChosenMonitor();
            if (null != chosenScreen)
            {
                // Get the window with the selected class name
                var hWnd = DisplayTools.FindWindowByClassName(tbWindowClass.Text);

                if (IntPtr.Zero != hWnd)
                {
                    DisplayTools.MoveWindow(hWnd, chosenScreen.Bounds.Location, chosenScreen.Bounds.Size);

                    // Now that we know we have a monitor and an active application, let's see if anybody has encroached.
                    // If they want us to...
                    if (chkPreventOtherApplications.Checked)
                    {
                        MoveErroneousWindows();
                    }

                    // If we got here then everything worked and the panel was moved.  Save the current settings for later.
                    SaveSettings();
                }
            }
        }

        /// <summary>
        /// Walk through all of the top-level windows and if they are on the sensor panel monitor
        /// move them off to the nearest non-sensor panel monitor.
        /// </summary>
        private void MoveErroneousWindows()
        {
            var chosenMonitor = GetChosenMonitor();
            if (null != chosenMonitor)
            {
                var nearestMonitor = GetNearestMonitor(chosenMonitor: chosenMonitor);

                if (null != nearestMonitor)
                {
                    // Now we walk through all of the top-level windows.  We need to skip Explorer, there might be others
                    // we need to skip.  We should check the window to make sure it's movable (has a rind) before we move it
                    // as there's no point in trying to relocate a splash screen, etc.
                    var openWindows = DisplayTools.GetOpenWindows();

                    // Look at all windows except the one we are protecting
                    foreach (var window in openWindows.Values)
                    {
                        if (window.ClassName == tbWindowClass.Text)
                        {
                            continue;
                        }

                        // Check if the window is on the sensor panel monitor
                        var isOnChosenScreen = chosenMonitor.Bounds.Contains(window.WindowLocation.Location);
                        if (!isOnChosenScreen)
                        {
                            continue;
                        }

                        // Found one, move it to the nearest monitor
                        var distanceFromOriginX = window.WindowLocation.X - chosenMonitor.Bounds.X;
                        var distanceFromOriginY = window.WindowLocation.Y - chosenMonitor.Bounds.Y;

                        // Move it to the same offset on the nearest monitor
                        // We need to make sure we're on the monitor though
                        var newX = nearestMonitor.Bounds.X + distanceFromOriginX;
                        var newY = nearestMonitor.Bounds.Y + distanceFromOriginY;
                        var newPoint = new Point(newX, newY);
                        if (!nearestMonitor.Bounds.Contains(newPoint))
                        {
                            // Move it somewhere safe
                            newX = nearestMonitor.Bounds.X + 20;
                            newY = nearestMonitor.Bounds.Y + 20;
                        }

                        // Move the window
                        DisplayTools.MoveWindow(window.hWnd, new Point(newX, newY), window.WindowLocation.Size);
                    }
                }
            }
        }

        /// <summary>
        /// First, let's identify the monitor that is nearest to the sensor panel (chosenScreen).
        /// To do this we calculate the distance from the center of chosenScreen
        /// to the center of every other and choose the one that is the closest.
        /// </summary>
        /// <param name="chosenMonitor">The screen from which to measure.</param>
        /// <returns>The closest screen (if there were no other screens, returns null).</returns>
        /// <exception cref="NotImplementedException"></exception>
        private Monitor GetNearestMonitor(Monitor chosenMonitor)
        {
            if (null == chosenMonitor)
            {
                throw new ArgumentException("chosenScreen was null");
            }

            Monitor closestMonitor = null;

            var sensorPanelCenter = new Point(chosenMonitor.Bounds.X + chosenMonitor.Bounds.Width/2,
                                              chosenMonitor.Bounds.Y + chosenMonitor.Bounds.Height/2);
            var closestDistance = double.MaxValue;
            foreach (var monitor in DisplayTools.GetAllMonitors())
            {
                var screenCenter = new Point(monitor.Bounds.X + monitor.Bounds.Width / 2,
                                             monitor.Bounds.Y + monitor.Bounds.Height / 2);

                var distance = Math.Sqrt(  Math.Pow(sensorPanelCenter.X - screenCenter.X, 2) 
                                               + Math.Pow(sensorPanelCenter.Y - screenCenter.Y, 2));

                // If screen is not the sensor panel itself, but we found one closer than the previous
                // remember it.
                if (distance > 1 && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestMonitor = monitor;
                }
            }

            // We have our candidate
            return closestMonitor;
        }

        private void SaveSettings()
        {
            if (null != cbMonitorList.SelectedItem)
            {
                Settings.Default["MonitorName"] = (string)cbMonitorList.SelectedItem;
            }

            if (string.IsNullOrEmpty(tbWindowClass.Text))
            {
                tbWindowClass.Text = "TForm_HWMonitoringSensorPanel";
            }

            Settings.Default["WindowClass"] = tbWindowClass.Text;
            Settings.Default["PreventOtherApplications"] = chkPreventOtherApplications.Checked;


            if (cbRefreshInterval.SelectedItem is IntervalComboBoxItem currentSelection && currentSelection.seconds > 0)
            {
                Settings.Default["RefreshRate"] = currentSelection.seconds;
            }
            else
            {
                Settings.Default["RefreshRate"] = 0;
            }

            Settings.Default.Save();

            lblMessage.Text = "Configuration Saved";

            if (null != _savedTimer)
            {
                _savedTimer.Stop();
                _savedTimer = null;
            }

            _savedTimer = new Timer
            {
                Interval = (5 * 1000) // convert seconds to milliseconds
            };
            _savedTimer.Tick += new EventHandler(SaveTimer_Tick);
            _savedTimer.Start();
        }

        /// <returns>true if the login trigger already exists in the scheduled tasks.</returns>
        private bool ScheduledTaskExists()
        {
            try
            {
                using (var ts = new TaskService())
                {
                    var task = ts.FindTask(ForceSensorPanelToMonitorTaskName);
                    return null != task;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred trying to identify the startup task: " + e.Message);
            }
            return false;
        }

        private void CreateScheduledTask()
        {
            if (ScheduledTaskExists())
            {
                // Nothing to do.
                return;
            }

            try
            {
                using (var ts = new TaskService())
                {
                    // Create a new task definition and assign properties
                    var task = ts.NewTask();

                    task.RegistrationInfo.Description = "Start ForceSensorPanelToMonitor when the current user logs in";
                    task.Principal.RunLevel = TaskRunLevel.Highest;

                    // Add a trigger that, starting tomorrow, will fire every other week on Monday
                    // and Saturday and repeat every 10 minutes for the following 11 hours
                    var currentIdentity = WindowsIdentity.GetCurrent();
                    var isAdministrator = (new WindowsPrincipal(currentIdentity).IsInRole(WindowsBuiltInRole.Administrator));

                    if (!isAdministrator)
                    {
                        MessageBox.Show("The current user is not an administrator, cannot configure to run at startup.  Please manually create a scheduled task for this user to run as an administrator.");
                        return;
                    }

                    var logonTrigger = new LogonTrigger() { UserId = currentIdentity.Name };

                    // Give Aida64 time to launch.  They normally set a 10 second delay after login, so we'll do 30 seconds to make sure they came up.
                    logonTrigger.Delay = TimeSpan.FromSeconds(30);
                
                    task.Triggers.Add(logonTrigger);

                    // Create an action that will launch Notepad whenever the trigger fires
                    task.Actions.Add(new ExecAction("ForceSensorPanelToMonitor.exe", "/StartToTray", Path.GetDirectoryName(Application.ExecutablePath)));

                    // Register the task in the root folder
                    ts.RootFolder.RegisterTaskDefinition(ForceSensorPanelToMonitorTaskName, task);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred trying to create the startup task: " + e.Message);
            }
        }

        /// <summary>
        /// Remove the task created with createScheduledTask() above.  Note that the task must be in the root folder in the task scheduler.
        /// </summary>
        private void RemoveScheduledTask()
        {
            try
            {
                using (var ts = new TaskService())
                {
                    var task = ts.GetTask(ForceSensorPanelToMonitorTaskName);
                    if (null != task)
                    {
                        ts.RootFolder.DeleteTask(ForceSensorPanelToMonitorTaskName);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred trying to remove the startup task: " + e.Message);
            }
        }

        private void SettingsDialog_Activated(object sender, EventArgs e)
        {
            // Every time we get focus, refresh the monitor list
            FillMonitorList();
        }
    }

}
