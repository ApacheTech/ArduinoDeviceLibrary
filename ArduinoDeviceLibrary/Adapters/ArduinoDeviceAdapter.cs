//  -------------------------------------------------------------------------
//   <copyright file="ArduinoDeviceAdapter.cs" company="ApacheTech Consultancy">
//       Copyright (c) ApacheTech Consultancy. 2013. All rights reserved.
//   </copyright>
//   <license type="GNU General Public License" version="3">
//       This program is free software: you can redistribute it and/or modify
//       it under the terms of the GNU General Public License as published by
//       the Free Software Foundation, either version 3 of the License, or
//       (at your option) any later version.
//  
//       This program is distributed in the hope that it will be useful,
//       but WITHOUT ANY WARRANTY; without even the implied warranty of
//       MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//       GNU General Public License for more details.
//  
//       You should have received a copy of the GNU General Public License
//       along with this program. If not, see http://www.gnu.org/licenses
//   <license>
// -------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using ArduinoDeviceLibrary.Exceptions;
using ArduinoDeviceLibrary.IO;
using ArduinoDeviceLibrary.Interfaces;
using ArduinoDeviceLibrary.Properties;

namespace ArduinoDeviceLibrary.Adapters
{
    /// <summary>
    ///     Provides automated detection and initiation of Arduino devices. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    ///     An adapter for Arduino (VID 2431) devices, including XBee USB Serial Ports from Digi International 
    ///     (VID 40d0) and Future Technologies (VID 0403), as standard.
    /// 
    ///     The ArduinoDeviceAdapter class, which inherits from IDisposable and INotifyPropertyChanged, allows 
    ///     automatic detection of devices based on their VendorID when plugged in and instantiates a 
    ///     System.IO.Ports.SerialPort based on the detected COM Port and a user designated Baud Rate (9600 baud 
    ///     as default.); as well as asynchronous IO to and from the device using the 'async' and 'await' pattern 
    ///     with the underlying BaseStream.
    /// 
    ///     Events fire whenever a device is connected or disconnected, when data is received of sent and when 
    ///     errors occur, with custom ArduinoExceptions for defensive programming, an IArduinoDeviceAdapter interface 
    ///     for SoC/IoC/DI and a custom ArduinoDataReceiverEventHandler which gives the data as a property within
    ///     ArduinoDataReceivedEventArgs.
    /// </remarks>
    [UsedImplicitly]
    public sealed class ArduinoDeviceAdapter : IArduinoDeviceAdapter
    {
        #region Private Fields

        /// <summary>
        ///     The baud rate the Arduino device will transmit data at.
        /// </summary>
        private readonly int _baudRate;

        /// <summary>
        ///     A System Watcher to hook device connect events from the WMI tree.
        /// </summary>
        private readonly ManagementEventWatcher _deviceConnectionWatcher;

        /// <summary>
        ///     A list of VendorIDs to search for within the PnPEntity WMI class.
        /// </summary>
        private readonly List<string> _vendors;

        /// <summary>
        ///     The connection to the Arduino device.
        /// </summary>
        private SerialPort _connection;

        /// <summary>
        ///     The name of the Arduino device.
        /// </summary>
        private string _deviceName = String.Empty;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialises a new instance of the <see cref="ArduinoDeviceAdapter" /> class.
        /// </summary>
        /// <param name="vendors">The list of VendorIDs to search for within the PnPEntity WMI class. If null, a default list of VendorIDs will be used.</param>
        /// <param name="baudRate">The baud rate the Arduino device will transmit data at. If no value is given, the default baud rate of 9600 will be used. Leave blank if using X-Bee modules or if the baud rate is unknown.</param>
        public ArduinoDeviceAdapter(List<string> vendors, int baudRate = 9600)
        {
            // Set the baud rate the Arduino device will transmit data at.
            _baudRate = baudRate;

            // Initialise the list of VendorIDs.
            _vendors = vendors ?? new List<string> {"2431", "0403", "40d0"};

            // Ensure that the VendorID list has the bare minimum elements needed.
            foreach (var s in new[] {"2431", "0403", "40d0"}.Where(s => !_vendors.Contains(s))) _vendors.Add(s);

            // Initialise the device connection watcher.
            // This will be raised whenever a Plug and Play device is plugged in or unplugged.
            _deviceConnectionWatcher = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 OR EventType = 3"));

            // Attach event listeners to the device watchers.
            _deviceConnectionWatcher.EventArrived += (sender, e) => DiscoverArduinoDevice();

            // Add event listeners to the DataReceived and ErrorOccured events on the underlying serial port.
            Connection.DataReceived += Connection_DataReceived;
            Connection.ErrorReceived += Connection_ErrorReceived;

            // Start monitoring the WMI tree for changes in SerialPort devices.
            _deviceConnectionWatcher.Start();
        }

        #endregion

        #region Destructor

        /// <summary>
        ///     Finalises an instance of the <see cref="ArduinoDeviceAdapter"/> class.
        /// </summary>
        ~ArduinoDeviceAdapter()
        {
            // Safe release all resources upon unexpected disposal.
            Dispose();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the name of the Arduino device.
        /// </summary>
        /// <value>The name of the Arduino device.</value>
        public string DeviceName
        {
            get { return _deviceName; }
            private set
            {
                _deviceName = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Private Properties

        /// <summary>
        ///     Gets the SerialPort connection to the Arduino device.
        /// </summary>
        /// <value>The SerialPort connection to the Arduino device.</value>
        private SerialPort Connection
        {
            get { return _connection; }
            set
            {
                _connection = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     Occurs when the Arduino device connection is opened.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        ///     Occurs when data is received from the Arduino device.
        /// </summary>
        public event ArduinoDataReceivedEventHandler DataReceived;

        /// <summary>
        ///     Occurs when data is sent to the Arduino device.
        /// </summary>
        public event EventHandler DataSent;

        /// <summary>
        ///     Occurs when the Arduino device connection is closed.
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        ///     Occurs when an error message is received from the Arduino device.
        /// </summary>
        public event SerialErrorReceivedEventHandler ErrorReceived;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Opens the connection to the Arduino device.
        /// </summary>
        /// <exception cref="ArduinoException">Error occurred while attempting to open the connection to the Arduino device. See inner exception for details.</exception>
        public void Connect()
        {
            try
            {
                // Open the connection, if it is not already open.
                if (Connection.IsOpen) return;
                Connection.Open();
                OnConnected();
            }
            catch (Exception ex)
            {
                throw new ArduinoException(
                    "Error occurred while attempting to open the connection to the Arduino device. See inner exception for details.",
                    ex);
            }
        }

        /// <summary>
        ///     Closes the connection to the Arduino device.
        /// </summary>
        /// <exception cref="ArduinoException">Error occurred while attempting to close the connection to the Arduino device. See inner exception for details.</exception>
        public void Disconnect()
        {
            try
            {
                // Close the connection, if it is open.
                if (!Connection.IsOpen) return;
                Connection.Close();
                OnDisconnected();
            }
            catch (Exception ex)
            {
                throw new ArduinoException(
                    "Error occurred while attempting to close the connection to the Arduino device. See inner exception for details.",
                    ex);
            }
        }

        /// <summary>
        ///     Automatically discovers the arduino device, by searching through the WMI records for up to date information.
        /// </summary>
        /// <exception cref="ArduinoException">Multiple Arduino devices found, or corrupted drivers are causing a false positive.</exception>
        public void DiscoverArduinoDevice()
        {
            // Create a query string to filter out all PnPEntities apart from the whitelisted VendorIDs.
            var vendors = String.Join(" OR ",
                _vendors.Select(s => s.Replace(s, String.Format("PNPDeviceID LIKE \"%VID[_]{0}%\"", s))));

            // Use WQL to query WMI for all Serial Ports currently in use and filter to only see relevant Serial devices.
            using (var searcher = new ManagementObjectSearcher("root\\CIMV2",
                                                               "SELECT * FROM Win32_PnPEntity " +
                                                               "WHERE ConfigManagerErrorCode = 0 " +
                                                               "AND Caption LIKE \"%(COM%\" " +
                                                               "AND (" + vendors + ")"))
            {
                // Collate results into a list.
                var query = searcher.Get().Cast<ManagementObject>().ToList();

                // Nullify output and return if no results are found.
                if (!query.Any())
                {
                    DeviceName = null;
                    Connection = null;
                    return;
                }

                // Throw an exception if more than one device was detected.
                if (query.Count > 1)
                    throw new ArduinoException(
                        "Multiple Arduino devices found, or corrupted drivers are causing a false positive.");

                // Set the DeviceName to be the WMI friendly name.
                DeviceName = query[0]["Caption"].ToString();

                // Strip the Port name from the device caption.
                var portName = DeviceName.Substring(
                    DeviceName.LastIndexOf("COM", StringComparison.Ordinal)).Replace(")", "");

                // Set the SerialPort to an instance of the discovered device.
                Connection = new SerialPort
                {
                    PortName = portName,    // Set the COM port to connect through.
                    BaudRate = _baudRate,   // Set the baud rate to connect at.
                    RtsEnable = true,       // Enable "Request To Send" mode.
                    DtrEnable = true,       // Enable "Data Terminal Ready" mode.
                };
            }
        }

        #endregion
        
        #region Public Async IO Methods

        /// <summary>
        ///     Reads a line of characters asynchronously from the current stream and returns the data as a string.
        /// </summary>
        /// <returns>A task that represents the asynchronous read operation. The value of the TResult parameter contains the next line from the stream, or is null if all the characters have been read.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to read data.</exception>
        public async Task<string> ReadLineAsync()
        {
            if (!Connection.IsOpen)
                throw new ArduinoException("Arduino port not open when attempting to read data.");
            var data = await (new StreamReader(Connection.BaseStream)).ReadLineAsync();
            OnDataReceived(data);
            return data;
        }

        /// <summary>
        ///     Reads all characters from the current position to the end of the stream asynchronously and returns them as one string.
        /// </summary>
        /// <returns>A task that represents the asynchronous read operation. The value of the TResult parameter contains a string with the characters from the current position to the end of the stream.</returns>
        public async Task<string> ReadToEndAsync()
        {
            if (!Connection.IsOpen)
                throw new ArduinoException("Arduino port not open when attempting to read data.");
            var data = await (new StreamReader(Connection.BaseStream)).ReadToEndAsync();
            OnDataReceived(data);
            return data;
        }

        /// <summary>
        ///     Writes a character to the stream asynchronously.
        /// </summary>
        /// <param name="value">The character to write to the stream. If value is null, nothing is written.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        public Task WriteAsync(char value)
        {
            if (!Connection.IsOpen)
                throw new ArduinoException("Arduino port not open when attempting to write data.");
            OnDataSent();
            return (new StreamWriter(Connection.BaseStream)).WriteAsync(value);
        }

        /// <summary>
        ///     Writes an array of characters to the stream asynchronously.
        /// </summary>
        /// <param name="buffer">The character array to write data from.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        public Task WriteAsync(char[] buffer)
        {
            if (!Connection.IsOpen)
                throw new ArduinoException("Arduino port not open when attempting to write data.");
            OnDataSent();
            return (new StreamWriter(Connection.BaseStream)).WriteAsync(buffer);
        }

        /// <summary>
        ///     Writes a subarray of characters to the stream asynchronously.
        /// </summary>
        /// <param name="buffer">The character array to write data from.</param>
        /// <param name="index">The character position in the buffer at which to start reading data.</param>
        /// <param name="count">The maximum number of characters to write.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        public Task WriteAsync(char[] buffer, int index, int count)
        {
            if (!Connection.IsOpen)
                throw new ArduinoException("Arduino port not open when attempting to write data.");
            OnDataSent();
            return (new StreamWriter(Connection.BaseStream)).WriteAsync(buffer, index, count);
        }

        /// <summary>
        ///     Writes a string to the stream asynchronously.
        /// </summary>
        /// <param name="value">The string to write to the stream. If value is null, nothing is written.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        public Task WriteAsync(string value)
        {
            if (!Connection.IsOpen)
                throw new ArduinoException("Arduino port not open when attempting to write data.");
            OnDataSent();
            return (new StreamWriter(Connection.BaseStream)).WriteAsync(value);
        }

        /// <summary>
        ///     Writes a character followed by a line terminator asynchronously to the stream.
        /// </summary>
        /// <param name="value">The character to write to the stream.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        public Task WriteLineAsync(char value)
        {
            if (!Connection.IsOpen)
                throw new ArduinoException("Arduino port not open when attempting to write data.");
            OnDataSent();
            return (new StreamWriter(Connection.BaseStream)).WriteLineAsync(value);
        }

        /// <summary>
        ///     Writes an array of characters followed by a line terminator asynchronously to the stream.
        /// </summary>
        /// <param name="buffer">The character array to write data from.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        public Task WriteLineAsync(char[] buffer)
        {
            if (!Connection.IsOpen)
                throw new ArduinoException("Arduino port not open when attempting to write data.");
            OnDataSent();
            return (new StreamWriter(Connection.BaseStream)).WriteLineAsync(buffer);
        }

        /// <summary>
        ///     Writes a subarray of characters followed by a line terminator asynchronously to the stream.
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified character array with the values between index and (index + count - 1) replaced by the characters read from the current source.</param>
        /// <param name="index">The position in buffer at which to begin writing.</param>
        /// <param name="count">The maximum number of characters to read. If the end of the stream is reached before the specified number of characters is written into the buffer, the current method returns.</param>
        /// <returns>A task that represents the asynchronous read operation. The value of the TResult parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        public Task WriteLineAsync(char[] buffer, int index, int count)
        {
            if (!Connection.IsOpen)
                throw new ArduinoException("Arduino port not open when attempting to write data.");
            OnDataSent();
            return (new StreamWriter(Connection.BaseStream)).WriteLineAsync(buffer, index, count);
        }

        /// <summary>
        ///     Writes a string followed by a line terminator asynchronously to the stream.
        /// </summary>
        /// <param name="value">The string to write. If the value is null, only a line terminator is written.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        public Task WriteLineAsync(string value)
        {
            if (!Connection.IsOpen)
                throw new ArduinoException("Arduino port not open when attempting to write data.");
            OnDataSent();
            return (new StreamWriter(Connection.BaseStream)).WriteLineAsync(value);
        }

        /// <summary>
        ///     Writes a line terminator asynchronously to the stream.
        /// </summary>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        public Task WriteLineAsync()
        {
            if (!Connection.IsOpen)
                throw new ArduinoException("Arduino port not open when attempting to write data.");
            OnDataSent();
            return (new StreamWriter(Connection.BaseStream)).WriteLineAsync();
        }

        #endregion

        #region Public Overrided Methods

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Connection != null
                       ? String.Format("{0} connected at {1} baud.", DeviceName, _baudRate)
                       : "No Arduino device found.";
        }

        #endregion

        #region Private Event Invocation Methods

        /// <summary>
        ///     Called when the Arduino device connection is opened.
        /// </summary>
        private void OnConnected()
        {
            var handler = Connected;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Called when data is received from the Arduino device.
        /// </summary>
        /// <param name="data">The data received from the Arduino device.</param>
        private void OnDataReceived(string data)
        {
            var handler = DataReceived;
            if (handler != null) handler(this, new ArduinoDataReceivedEventArgs(data));
        }

        /// <summary>
        ///     Called when data is sent to the Arduino device.
        /// </summary>
        private void OnDataSent()
        {
            var handler = DataSent;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Called when the Arduino device connection is closed.
        /// </summary>
        private void OnDisconnected()
        {
            var handler = Disconnected;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Called when an error message is received from the Arduino device.
        /// </summary>
        /// <param name="e">The <see cref="SerialErrorReceivedEventArgs"/> instance containing the event data.</param>
        private void OnErrorReceived(SerialErrorReceivedEventArgs e)
        {
            var handler = ErrorReceived;
            if (handler != null) handler(this, e);
        }

        #endregion
        
        #region Private Event Handling Methods

        /// <summary>
        ///     Handles the DataReceived event of the Connection control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SerialDataReceivedEventArgs" /> instance containing the event data.</param>
        /// <exception cref="ArduinoException">Invalid data sent from Arduino device.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">e;Invalid data type sent from Arduino device.</exception>
        private async void Connection_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            switch (e.EventType)
            {
                case SerialData.Chars:
                    await ReadToEndAsync();
                    break;
                case SerialData.Eof:
                    throw new ArduinoException("Invalid data sent from Arduino device.");
                default:
                    throw new ArgumentOutOfRangeException("e", e.EventType,
                                                          "Invalid data type sent from Arduino device.");
            }
        }

        /// <summary>
        ///     Handles the ErrorReceived event of the Connection control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SerialErrorReceivedEventArgs"/> instance containing the event data.</param>
        private void Connection_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            OnErrorReceived(e);
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Close the SerialPort link when this instance is disposed.
            if (Connection != null && Connection.IsOpen) Connection.Close();

            // Stop the WMI monitors when this instance is disposed.
            _deviceConnectionWatcher.Stop();
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Called when a property is set.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}