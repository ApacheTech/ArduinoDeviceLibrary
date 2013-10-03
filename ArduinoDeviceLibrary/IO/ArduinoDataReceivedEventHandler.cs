//  -------------------------------------------------------------------------
//   <copyright file="ArduinoDataReceivedEventHandler.cs" company="ApacheTech Consultancy">
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

namespace ArduinoDeviceLibrary.IO
{
    /// <summary>
    ///     Represents the method that will handle the ArduinoDeviceLibrary.ArduinoDeviceAdapter.DataReceived event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ArduinoDataReceivedEventArgs"/> instance containing the event data.</param>
    public delegate void ArduinoDataReceivedEventHandler(object sender, ArduinoDataReceivedEventArgs e);
}