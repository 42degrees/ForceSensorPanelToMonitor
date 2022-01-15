using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ForceSensorPanelToMonitor
{
    public partial class formMain1 : Form
    {
        private Timer _timer = null;
        private Timer _savedTimer = null;
        private bool _startMinimal = false;

        public formMain1(bool startMinimal = false)
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

            // Run once when we first start
            SyncSensorPanel();
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
                trayIcon.ShowBalloonTip(1000);
            }
        }

        private void restoreWindow()
        {
            ShowInTaskbar = true;
            trayIcon.Visible = false;
            WindowState = FormWindowState.Normal;
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

        private void chkRunAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (chkRunAtStartup.Checked)
                {
                    // Register ourselves to run minimized with Windows
                    key.SetValue("ForceSensorPanelToMonitor", "\"" + Application.ExecutablePath + "\" /StartToTray");
                }
                else
                {
                    // Remove any registration
                    key.DeleteValue("ForceSensorPanelToMonitor", false);
                }
            }
        }
    }

}
