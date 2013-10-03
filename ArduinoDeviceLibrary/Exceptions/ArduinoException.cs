//  -------------------------------------------------------------------------
//   <copyright file="ArduinoException.cs" company="ApacheTech Consultancy">
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
using System.Runtime.Serialization;

namespace ArduinoDeviceLibrary.Exceptions
{
    /// <summary>
    ///     Represents errors that occur with Arduino devices.
    /// </summary>
    /// <remarks>
    ///     Although this class is simply a mask for a standard exception, the separation achieved by 
    ///     this mask allows for unique exceptions to be sent out from and handled by the Arduino device.
    /// 
    ///     //!++ REVISION NOTES:
    /// 
    ///      * In future versions, this class will be used as a base Exception type for all derived exceptions 
    ///     with the Arduino devices. One main aim of this library is to create as automated process as possible.
    ///     As such, defensive programming is essential, with well documented and tested exception handling
    ///     at all stages.
    /// </remarks>
    [Serializable]
    public class ArduinoException : Exception
    {
        /// <summary>
        ///     Initialises a new instance of the <see cref="ArduinoException"/> class.
        /// </summary>
        public ArduinoException() { }

        /// <summary>
        ///     Initialises a new instance of the <see cref="T:System.Exception" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ArduinoException(string message) : base(message) { }

        /// <summary>
        ///     Initialises a new instance of the <see cref="ArduinoException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public ArduinoException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        ///     Initialises a new instance of the <see cref="T:System.Exception" /> class with serialised data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialised object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected ArduinoException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}