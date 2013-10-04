//  -------------------------------------------------------------------------
//   <copyright file="ArduinoDataReceivedEventArgs.cs" company="ApacheTech Consultancy">
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
using ArduinoDeviceLibrary.Properties;

namespace ArduinoDeviceLibrary.IO
{
    /// <summary>
    ///     Provides data for the ArduinoDeviceLibrary.ArduinoDeviceAdapter.DataReceived event.
    /// </summary>
    public class ArduinoDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initialises a new instance of the <see cref="ArduinoDataReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="data">The data, sent from the Arduino device.</param>
        public ArduinoDataReceivedEventArgs(string data) { Data = data; }

        /// <summary>
        ///     Gets the data, sent from the Arduino device.
        /// </summary>
        /// <value>The data, sent from the Arduino device.</value>
        [UsedImplicitly]
        public string Data { get; private set; }
    }
}