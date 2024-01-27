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

using System.ComponentModel;
using System.Runtime.Serialization;

namespace Splunk.Client;

public partial class ApplicationCollection
{
    /// <summary>
    /// Provides selection criteria for retrieving a slice of an
    /// <see cref= "ApplicationCollection"/>.
    /// </summary>
    /// <remarks>
    /// <para><b>References:</b></para>
    /// <list type="number">
    /// <item><description>
    ///   <a href="http://goo.gl/pqZJco">REST API: GET apps/local</a>
    /// </description></item>
    /// </list>
    /// </remarks>
    /// <seealso cref="T:Splunk.Client.Args{Splunk.Client.ApplicationCollection.Filter}"/>
    public sealed class Filter : Args<Filter>
    {
        /// <summary>
        /// Gets or sets a value specifying the maximum number of
        /// <see cref= "Application"/> entries to return.
        /// </summary>
        /// <remarks>
        /// If the value of <c>Count</c> is set to zero, then all
        /// <see cref= "Application"/> entries are returned. The default value is
        /// <c>30</c>.
        /// </remarks>
        /// <value>
        /// A value specifying the maximum number of <see cref="Application"/>
        /// entries to return.
        /// </value>
        [DataMember(Name = "count", EmitDefaultValue = false)]
        [DefaultValue(30)]
        public int Count
        { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the first result (inclusive)
        /// from which to begin returning entries.
        /// </summary>
        /// <remarks>
        /// The <c>Offset</c> property is zero-based and cannot be negative. The
        /// default value is zero.
        /// </remarks>
        /// <value>
        /// A value specifying the first result (inclusive) from which to begin
        /// returning entries.
        /// </value>
        [DataMember(Name = "offset", EmitDefaultValue = false)]
        [DefaultValue(0)]
        public int Offset
        { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to scan for new
        /// <see cref="Application"/> instances and reload any objects those new
        /// instances contain.
        /// </summary>
        /// <remarks>
        /// The default is <c>false</c>.
        /// </remarks>
        /// <value>
        /// <c>true</c>, if a scan and reload should be done; <c>false</c>
        /// otherwise.
        /// </value>
        [DataMember(Name = "refresh", EmitDefaultValue = false)]
        [DefaultValue(false)]
        public bool Refresh
        { get; set; }

        /// <summary>
        /// Gets or sets a search expression to filter <see cref= "Application"/>
        /// entries.
        /// </summary>
        /// <remarks>
        /// Use this expression to filter the entries returned based on
        /// <see cref="Application"/> properties.
        /// </remarks>
        /// <value>
        /// A search expression to filter <see cref="Application"/> entries.
        /// </value>
        [DataMember(Name = "search", EmitDefaultValue = false)]
        [DefaultValue(null)]
        public string? Search
        { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the sort direction for
        /// <see cref="Application"/> entries.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="SortDirection"/>.Ascending.
        /// </remarks>
        /// <value>
        /// The sort direction for <see cref="Application"/> entries.
        /// </value>
        [DataMember(Name = "sort_dir", EmitDefaultValue = false)]
        [DefaultValue(SortDirection.Ascending)]
        public SortDirection SortDirection
        { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the sort mode for
        /// <see cref= "Application"/> entries.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="SortMode"/>.Automatic.
        /// </remarks>
        /// <value>
        /// The sort mode for <see cref="Application"/> entries.
        /// </value>
        [DataMember(Name = "sort_mode", EmitDefaultValue = false)]
        [DefaultValue(SortMode.Automatic)]
        public SortMode SortMode
        { get; set; }
    }
}
