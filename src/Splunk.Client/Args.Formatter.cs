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

//// TODO: Ensure this code is solid
//// [ ] Support more than one level of inheritance => move away from generic implementation.
//// [O] Contracts
//// [O] Documentation

namespace Splunk.Client
{
    public abstract partial class Args<TArgs> where TArgs : Args<TArgs>
    {
        /// <summary>
        /// A formatter.
        /// </summary>
        /// <seealso cref="IEnumerable{Argument}" />
        private record struct Formatter
        {
            /// <summary>
            /// Describes the format to use.
            /// </summary>
            public Func<object, string> Format;

            /// <summary>
            /// <c>true</c> if this object is collection.
            /// </summary>
            public bool IsCollection;
        }
    }
}
