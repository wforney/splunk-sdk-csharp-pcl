/*
 * Copyright 2014 Splunk, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"): you may
 * not use this file except in compliance with the License. You may obtain
 * a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 */

//// TODO:
//// [O] Contracts
//// [O] Documentation

namespace Splunk.Client.Converters
{
    using System;
    using System.IO;
    using Splunk.Client;

    /// <summary>
    /// Provides a converter to convert strings to <see cref="bool"/> values.
    /// </summary>
    /// <seealso cref="T:Splunk.Client.ValueConverter{System.Boolean}"/>
    sealed class BooleanConverter : ValueConverter<bool>
    {
        #region Fields

        /// <summary>
        /// The default <see cref="BooleanConverter"/> instance.
        /// </summary>
        public static readonly BooleanConverter Instance = new BooleanConverter();

        #endregion

        #region Methods

        /// <summary>
        /// Converts the string representation of an object to a
        /// <see cref= "bool"/> value.
        /// </summary>
        /// <param name="input">
        /// The object to convert.
        /// </param>
        /// <returns>
        /// Result of the conversion.
        /// </returns>
        /// <exception cref="InvalidDataException">
        /// The <paramref name="input"/> does not represent a <see cref="bool"/>
        /// value.
        /// </exception>
        public override bool Convert(object input)
        {
            var x = input as bool?;

            if (x != null)
            {
                return x.Value;
            }

            var value = input.ToString();

            switch (value)
            {
                case "t": return true;
                case "f": return false;
                case "true": return true;
                case "false": return false;
            }

            int result;

            if (int.TryParse(input.ToString(), result: out result))
            {
                return result != 0;
            }

            throw NewInvalidDataException(input);
        }

        #endregion
    }
}
