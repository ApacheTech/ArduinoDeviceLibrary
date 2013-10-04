Arduino.NET Device Library (ADLib)
==================================
__Written By:__ Peter Matthews

### TL;DR:

ADLib is an adapter for Arduino Devices, written in C# for .NET Framework 4.5 which automates and simplifies a number of complex tasks.

### Description:

An adapter for Arduino (VID 2431) devices, including XBee USB Serial Ports from Digi International (VID 40d0) and Future Technologies (VID 0403), as standard.

The ArduinoDeviceAdapter class, which inherits from IDisposable and INotifyPropertyChanged, allows automatic detection of devices based on their VendorID when plugged in and instantiates a System.IO.Ports.SerialPort based on the detected COM Port and a user designated Baud Rate (9600 baud as default.); as well as asynchronous IO to and from the device using the 'async' and 'await' pattern with the underlying BaseStream.

Events fire whenever a device is connected or disconnected, when data is received of sent and when errors occur, with custom ArduinoExceptions for defensive programming, an IArduinoDeviceAdapter interface for SoC/IoC/DI and a custom ArduinoDataReceivedEventHandler which gives the data as a property within ArduinoDataReceivedEventArgs.

## Functionality and Features:

  *  Automatic detection of the COM Port used by Arduino based Serial or USB Serial devices.

  *  Custom Exception classes.

  *  IPropertyNotify inheritence for easy binding and WPF Interop.

  *  IDisposable inheritence for safe disposal and garbage collection.

  *  Event driven design for maximum asynchrony and automation.

  *  Extracted interface for scalability and separation of concerns.

## Download:

###  Pre-Release (v0.1-alpha)

__Download:__ https://github.com/ApacheTech/ArduinoDeviceLibrary/releases/tag/v0.1-alpha

__Source:__ https://github.com/ApacheTech/ArduinoDeviceLibrary

While this version is stable and has been tested via USB and XBee, a full break-test has not yet been done.

Please give feedback for testing purposes and log any issues you find. Also, please give any suggestions of features that would be good to include. I'm looking to expand this library as time goes on, with an aim to creating a full Arduino.NET API. Think about what processes and commands are most used and how they could be automated or simplified and integrated into this project.

## Setup & Usage

Once downloaded, include the .dll file as a reference in your .NET project. Alternatively, download the source code and add a reference to the library source within your application.

```C#
var arduino = new ArduinoDeviceLibrary.ArduinoDeviceAdapter(null, 115200);
```

Once initiated, event listeners should be added to the relevent events and the devices can then be discovered.
    
```C#
arduino.DiscoverArduinoDevice();
```
## Development

  *  Currently, the ArduinoDeviceAdapter class only works with a single device. This will be changed in future versions to collate a list of available devices and delegate between them.
  
  *  Once the project is set up to work with multiple Serial devices, a number of channels will be added for multicasting and interoperation purposes; including ArduinoProxy, ArduinoBridge, ArduinoFunnel and ArduinoBroadcast channels.

  *  Eventually, we are hoping to move the project towards a full features Arduino.NET Framework, powered by MEF, to allow extensible features and addons. 

## Known Issues

  *  The constructor could do with somme overloads to cut down on ambiguous parameters.
