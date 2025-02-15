﻿/*
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

namespace Splunk.Client.Settings
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for configuration setting.
    /// </summary>
    /// <seealso cref="T:IBaseResource"/>
    public interface IConfigurationSetting : IBaseResource
    {
        /// <summary>
        /// Gets the links.
        /// </summary>
        /// <value>
        /// The links.
        /// </value>
        IReadOnlyDictionary<string, Uri> Links { get; }

        /// <summary>
        /// Gets the cached value of the current <see cref="ConfigurationSetting"/>.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        string Value { get; }
    }
}
