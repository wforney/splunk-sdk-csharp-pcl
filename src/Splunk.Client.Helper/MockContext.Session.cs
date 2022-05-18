// <copyright file="MockContext.Session.cs" company="Splunk, Inc.">
//     Copyright 2014 Splunk, Inc.
// </copyright>
/*
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

using System.Runtime.Serialization;

namespace Splunk.Client.Helper;

/// <summary>
/// Class MockContext. Implements the <see cref="Context" />
/// </summary>
/// <seealso cref="Context" />
public partial class MockContext
{
    /// <summary>
    /// Class Session.
    /// </summary>
    [DataContract]
    private class Session
    {
        /// <summary>
        /// The data
        /// </summary>
        [DataMember(Order = 2)]
        public Queue<object> Data;

        /// <summary>
        /// The recordings
        /// </summary>
        [DataMember(Order = 3)]
        public Queue<Recording> Recordings;

        /// <summary>
        /// Initializes a new instance of the <see cref="Session" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Session(string name)
        {
            this.Name = name;
            this.Data = new Queue<object>();
            this.Recordings = new Queue<Recording>();
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember(Order = 1)]
        public string Name { get; private set; }
    }
}
