# ForceSensorPanelToMonitor

A small project dedicated to forcing the Aida64 sensor panel to fit to a chosen monitor even if the monitor profiles change over time.

## Install

I haven't written an installer yet.  At this point you'll have to pull the repo and build it yourself, then put the contents of the output directory somewhere and run it.
If you run it once, then check the "Start application with windows" it will register itself and you can ignore it from then on.

## Documentation

http://42degreesdesign.com/ForceSensorPanelToMonitor/

Yeah, right, like there's any documentation besides this.

This program has only one dialog, and that's the settings dialog where you tell it what window to watch, 
what monitor to use, and whether you want it to do things on a timed basis.
It doesn't know anything specific about Aida64 except the default window class (TForm_HWMonitoringSensorPanel).
It also doesn't know anything about any specific monitor.
So you could use this program to force any window with a unique window class
to stay mapped to any specific monitor.

Note that if you leave the Refresh rate at Manual, that means that the program will only respond to changes in the display profile (like removing a monitor, changing monitor settings or size, etc.) or if you manually press the Refresh Now button.
Also, it will always move the window when it is first run.  Note that if the sensor panel is not running when we first run the program does not currently notice when you do open the sensor panel, so you'll have to hit Refresh Now.  

To make sure we run after the sensor panel when it registers itself to run on login, it sets a time delay to make sure the sensor panel is up first.
It currently throws no errors if the window isn't up, it just silently fails.  If this is an issue, let me know and I can add code to monitor for the window to be created.

The program will also optionally monitor the panel that is being monitored to see if any applications have accidentally (or intentionally) put windows on there.  If it finds any it will attempt to move the window to the nearest other monitor (doing its best to keep the window the same size and relation to the top and left of the monitor).  It will only do this when it checks the sensor panel, so it is recommended if you want this feature that you enable the timed polling feature as well, or it may only move rogue windows very rarely.  I have mine set to checking every 10 seconds.  The actual function is very lightweight, so I don't expect it to be noticable to your system.

## Beta testers

Until I get the installer created, if you want to try it but don't have the ability to compile it, let me know and I'll send you a zip file to try.
Send requests to [ForceSensorPanelToMonitor@pingbot.com](mailto:ForceSensorPanelToMonitor@pingbot.com).

## License

Apache License, Version 2.0

http://opensource.org/licenses/Apache-2.0
