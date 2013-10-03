//  -------------------------------------------------------------------------
//   <copyright file="IArduinoDeviceAdapter.cs" company="ApacheTech Consultancy">
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
using System.ComponentModel;
using System.IO.Ports;
using System.Threading.Tasks;
using ArduinoDeviceLibrary.Exceptions;
using ArduinoDeviceLibrary.IO;

namespace ArduinoDeviceLibrary.Interfaces
{
    /// <summary>
    ///     Defines an adapter for use with Arduino serial devices.
    /// </summary>
    public interface IArduinoDeviceAdapter : IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        ///     Gets the name of the Arduino device.
        /// </summary>
        /// <value>The name of the Arduino device.</value>
        string DeviceName { get; }

        /// <summary>
        ///     Occurs when the Arduino device connection is opened.
        /// </summary>
        event EventHandler Connected;

        /// <summary>
        ///     Occurs when data is received from the Arduino device.
        /// </summary>
        event ArduinoDataReceivedEventHandler DataReceived;

        /// <summary>
        ///     Occurs when data is sent to the Arduino device.
        /// </summary>
        event EventHandler DataSent;

        /// <summary>
        ///     Occurs when the Arduino device connection is closed.
        /// </summary>
        event EventHandler Disconnected;

        /// <summary>
        ///     Occurs when an error message is received from the Arduino device.
        /// </summary>
        event SerialErrorReceivedEventHandler ErrorReceived;

        /// <summary>
        ///     Opens the connection to the Arduino device.
        /// </summary>
        /// <exception cref="ArduinoException">Error occurred while attempting to open the connection to the Arduino device. See inner exception for details.</exception>
        void Connect();

        /// <summary>
        ///     Closes the connection to the Arduino device.
        /// </summary>
        /// <exception cref="ArduinoException">Error occurred while attempting to close the connection to the Arduino device. See inner exception for details.</exception>
        void Disconnect();

        /// <summary>
        ///     Automatically discovers the arduino device, by searching through the WMI records for up to date information.
        /// </summary>
        /// <exception cref="ArduinoException">Multiple Arduino devices found, or corrupted drivers are causing a false positive.</exception>
        void DiscoverArduinoDevice();

        /// <summary>
        ///     Reads a line of characters asynchronously from the current stream and returns the data as a string.
        /// </summary>
        /// <returns>A task that represents the asynchronous read operation. The value of the TResult parameter contains the next line from the stream, or is null if all the characters have been read.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to read data.</exception>
        Task<string> ReadLineAsync();

        /// <summary>
        ///     Reads all characters from the current position to the end of the stream asynchronously and returns them as one string.
        /// </summary>
        /// <returns>A task that represents the asynchronous read operation. The value of the TResult parameter contains a string with the characters from the current position to the end of the stream.</returns>
        Task<string> ReadToEndAsync();

        /// <summary>
        ///     Writes a character to the stream asynchronously.
        /// </summary>
        /// <param name="value">The character to write to the stream. If value is null, nothing is written.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        Task WriteAsync(char value);

        /// <summary>
        ///     Writes an array of characters to the stream asynchronously.
        /// </summary>
        /// <param name="buffer">The character array to write data from.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        Task WriteAsync(char[] buffer);

        /// <summary>
        ///     Writes a subarray of characters to the stream asynchronously.
        /// </summary>
        /// <param name="buffer">The character array to write data from.</param>
        /// <param name="index">The character position in the buffer at which to start reading data.</param>
        /// <param name="count">The maximum number of characters to write.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        Task WriteAsync(char[] buffer, int index, int count);

        /// <summary>
        ///     Writes a string to the stream asynchronously.
        /// </summary>
        /// <param name="value">The string to write to the stream. If value is null, nothing is written.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        Task WriteAsync(string value);

        /// <summary>
        ///     Writes a character followed by a line terminator asynchronously to the stream.
        /// </summary>
        /// <param name="value">The character to write to the stream.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        Task WriteLineAsync(char value);

        /// <summary>
        ///     Writes an array of characters followed by a line terminator asynchronously to the stream.
        /// </summary>
        /// <param name="buffer">The character array to write data from.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        Task WriteLineAsync(char[] buffer);

        /// <summary>
        ///     Writes a subarray of characters followed by a line terminator asynchronously to the stream.
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified character array with the values between index and (index + count - 1) replaced by the characters read from the current source.</param>
        /// <param name="index">The position in buffer at which to begin writing.</param>
        /// <param name="count">The maximum number of characters to read. If the end of the stream is reached before the specified number of characters is written into the buffer, the current method returns.</param>
        /// <returns>A task that represents the asynchronous read operation. The value of the TResult parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        Task WriteLineAsync(char[] buffer, int index, int count);

        /// <summary>
        ///     Writes a string followed by a line terminator asynchronously to the stream.
        /// </summary>
        /// <param name="value">The string to write. If the value is null, only a line terminator is written.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        Task WriteLineAsync(string value);

        /// <summary>
        ///     Writes a line terminator asynchronously to the stream.
        /// </summary>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="ArduinoException">Arduino port not open when attempting to write data.</exception>
        Task WriteLineAsync();

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        new void Dispose();

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        new event PropertyChangedEventHandler PropertyChanged;
    }
}