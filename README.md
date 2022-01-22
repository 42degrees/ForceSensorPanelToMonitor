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

## License

Apache License, Version 2.0

http://opensource.org/licenses/Apache-2.0
