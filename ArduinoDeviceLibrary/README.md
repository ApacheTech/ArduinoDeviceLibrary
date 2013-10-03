﻿Arduino Device Library (ADLib)
==============================

__Written By:__ Pete 'Apache' Matthews

An adapter for Arduino (VID 2431) devices, including XBee USB Serial Ports from Digi International 
(VID 40d0) and Future Technologies (VID 0403), as standard.
 
The ArduinoDeviceAdapter class, which inherits from IDisposable and INotifyPropertyChanged, allows 
automatic detection of devices based on their VendorID when plugged in and instantiates a 
System.IO.Ports.SerialPort based on the detected COM Port and a user designated Baud Rate (9600 baud 
as default.); as well as asynchronous IO to and from the device using the 'async' and 'await' pattern 
with the underlying BaseStream.

Events fire whenever a device is connected or disconnected, when data is received of sent and when 
errors occur, with custom ArduinoExceptions for defensive programming, an IArduinoDeviceAdapter interface 
for SoC/IoC/DI and a custom ArduinoDataReceiverEventHandler which gives the data as a property within
ArduinoDataReceivedEventArgs.