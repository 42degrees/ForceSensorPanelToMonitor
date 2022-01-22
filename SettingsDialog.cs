using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32.TaskScheduler;
using System.Windows.Forms;
using System.IO;
using System.Security.Principal;

namespace ForceSensorPanelToMonitor
{
    public partial class SettingsDialog : Form
    {
        private readonly static string ForceSensorPanelToMonitorTaskName = "ForceSensorPanelToMonitor";

        private Timer _timer = null;
        private Timer _savedTimer = null;
        private bool _startMinimal = false;

        public SettingsDialog(bool startMinimal = false)
        {
            _startMinimal = startMinimal;
            InitializeComponent();
            SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);
        }

        private void formMain1_Load(object sender, EventArgs e)
        {
            var savedMonitorName = Properties.Settings.Default.MonitorName;
            var savedWindowClass = Properties.Settings.Default.WindowClass;
            var savedRefreshRate = Properties.Settings.Default.RefreshRate;

            // Add all the monitors to the drop-down list

            foreach (var screen in DisplayTools.getAllMonitorInfo())
            {
                var index = cbMonitorList.Items.Add(screen.DeviceFriendlyName());
                if (savedMonitorName == cbMonitorList.Items[index].ToString())
                {
                    cbMonitorList.SelectedIndex = index;
                }
            }

            var intervalValues = new List<IntervalComboBoxItem>();
            intervalValues.Add(new IntervalComboBoxItem { itemText = "Manual Only" });
            intervalValues.Add(new IntervalComboBoxItem { itemText = "10 seconds", seconds = 10 });
            intervalValues.Add(new IntervalComboBoxItem { itemText = "30 seconds", seconds = 30 });
            intervalValues.Add(new IntervalComboBoxItem { itemText = "1 minute",   seconds = 1 * 60 });
            intervalValues.Add(new IntervalComboBoxItem { itemText = "10 minutes", seconds = 10 * 60 });
            intervalValues.Add(new IntervalComboBoxItem { itemText = "20 minutes", seconds = 20 * 60 });
            intervalValues.Add(new IntervalComboBoxItem { itemText = "60 minutes", seconds = 60 * 60 });
            intervalValues.Add(new IntervalComboBoxItem { itemText = "2 hours",    seconds = 120 * 60 });
            intervalValues.Add(new IntervalComboBoxItem { itemText = "4 hours",    seconds = 240 * 60 });
            intervalValues.Add(new IntervalComboBoxItem { itemText = "8 hours",    seconds = 480 * 60 });
            intervalValues.Add(new IntervalComboBoxItem { itemText = "1 day",      seconds = 1440 * 60 });

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
            if (scheduledTaskExists())
            {
                chkRunAtStartup.Checked = true;
            }

            // Run once when we first start
            SyncSensorPanel();
        }

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == Program.WM_SHOWFIRSTINSTANCE)
            {
                restoreWindow();
            }
            base.WndProc(ref message);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // Run the algorithm to sync the window with the monitor
            SyncSensorPanel();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Ask the user if they really intended to close the app.  If they did, then close it down. Otherwise just return false;
        /// </summary>
        /// <returns>true if the user really wanted to close the application.</returns>
        private void formMain1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var dr = MessageBox.Show("If you exit the app, the sensor panel may move.  Minimize to hide the app in the tray.  Are you sure you want to quit?", "Leaving App", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }
        }

        public void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
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

        private void formMain1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
                trayIcon.Visible = true;
                //ShowInTaskbar = true;
                //trayIcon.Visible = false;
                trayIcon.ShowBalloonTip(1000);
            }
        }

        private void restoreWindow()
        {
            ShowInTaskbar = true;
            trayIcon.Visible = false;
            WindowState = FormWindowState.Normal;

            // We do this in case we were tickled by a new execution of ourselves,
            // since we likely weren't in the front of the other windows.
            WinApi.ShowWindow(this.Handle, WinApi.SW_SHOWNORMAL);
            WinApi.SetForegroundWindow(this.Handle);
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            restoreWindow();
        }

        private void trayIcon_Click(object sender, EventArgs e)
        {
            restoreWindow();
        }

        private void SyncSensorPanel()
        {
            var chosenScreen = DisplayTools.getAllMonitorInfo().FirstOrDefault(screen => screen.DeviceFriendlyName() == (string)cbMonitorList.SelectedItem);

            if (null != chosenScreen)
            {
                // Get the window with the selected class name
                var hWnd = DisplayTools.FindWindowByClassName(tbWindowClass.Text);

                if (null != hWnd)
                {
                    DisplayTools.MoveWindow(hWnd, chosenScreen.Bounds.Location, chosenScreen.Bounds.Size);

                    // If we got here then everything worked and the panel was moved.  Save the current settings for later.
                    SaveSettings();
                }
            }
        }

        private void SaveSettings()
        {
            if (null != cbMonitorList.SelectedItem)
            {
                Properties.Settings.Default["MonitorName"] = (string)cbMonitorList.SelectedItem;
            }

            if (null == tbWindowClass.Text || tbWindowClass.Text.Length == 0)
            {
                tbWindowClass.Text = "TForm_HWMonitoringSensorPanel";
            }

            Properties.Settings.Default["WindowClass"] = tbWindowClass.Text;

            var currentSelection = cbRefreshInterval.SelectedItem as IntervalComboBoxItem;

            if (null != currentSelection && currentSelection.seconds > 0)
            {
                Properties.Settings.Default["RefreshRate"] = currentSelection.seconds;
            }
            else
            {
                Properties.Settings.Default["RefreshRate"] = 0;
            }

            Properties.Settings.Default.Save();

            lblMessage.Text = "Configuration Saved";

            if (null != _savedTimer)
            {
                _savedTimer.Stop();
                _savedTimer = null;
            }

            _savedTimer = new Timer();
            _savedTimer.Interval = (5 * 1000); // convert seconds to milliseconds
            _savedTimer.Tick += new EventHandler(SaveTimer_Tick);
            _savedTimer.Start();
        }

        private void cbRefreshInterval_SelectedIndexChanged(object sender, EventArgs e)
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

        /// <returns>true if the login trigger already exists in the scheduled tasks.</returns>
        private bool scheduledTaskExists()
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

        private void createScheduledTask()
        {
            if (scheduledTaskExists())
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
        private void removeScheduledTask()
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

        private void chkRunAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (chkRunAtStartup.Checked)
                {
                    // Register ourselves to run minimized when the user logs in.
                    createScheduledTask();
                }
                else
                {
                    // Remove any registration
                    removeScheduledTask();
                }
            }
        }
    }

}
