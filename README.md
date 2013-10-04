Arduino Device Library (ADLib)
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

### Pre-Release (v0.1-alpha)

While this version is stable and has been tested via USB and XBee, a full break-test has not yet been done.

Please give feedback for testing purposes and log any issues you find. Also, please give any suggestions 
of features that would be good to include. I'm looking to expand this library as time goes on, with an 
aim to creating a full Arduino.NET API. Think about what processes and commands are most used and how they
could be automated or simplified and integrated into this project.
