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

using System.Diagnostics.Contracts;
using Splunk.Client.Entities;

namespace Splunk.Client.Settings;

/// <summary>
/// Interface for configuration stanza.
/// </summary>
/// <seealso cref="T:IEntity"/>
/// <seealso cref="T:IReadOnlyList{ConfigurationSetting}"/>
[ContractClass(typeof(IConfigurationStanzaContract))]
public interface IConfigurationStanza : IEntity, IReadOnlyList<ConfigurationSetting>
{
    /// <summary>
    /// Gets the author of the current <see cref="ConfigurationStanza"/>.
    /// </summary>
    /// <value>
    /// The author.
    /// </value>
    string Author { get; }

    /// <summary>
    /// Asynchronously retrieves a configuration setting value from the current
    /// <see cref="ConfigurationStanza"/>
    /// </summary>
    /// <remarks>
    /// This method uses the <a href="http://goo.gl/cqT50u">GET
    /// properties/{file_name}/{stanza_name}/{key_Name}</a> endpoint to construct
    /// the <see cref="ConfigurationSetting"/> identified by
    /// <paramref name="keyName"/>.
    /// </remarks>
    /// <param name="keyName">
    /// The name of a configuration setting.
    /// </param>
    /// <returns>
    /// The string value of <paramref name="keyName"/>.
    /// </returns>
    Task<string> GetAsync(string keyName);

    /// <summary>
    /// Asynchronously updates the value of an existing setting in the current
    /// <see cref="ConfigurationStanza"/>.
    /// </summary>
    /// <remarks>
    /// This method uses the <a href="http://goo.gl/sSzcMy">POST
    /// properties/{file_name}/{stanza_name}/{key_Name}</a> endpoint to update
    /// the <see cref="ConfigurationSetting"/> identified by
    /// <paramref name="keyName"/>.
    /// </remarks>
    /// <param name="keyName">
    /// The name of a configuration setting in the current
    /// <see cref= "ConfigurationStanza"/>.
    /// </param>
    /// <param name="value">
    /// A new value for the setting identified by <paramref name="keyName"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the operation.
    /// </returns>
    Task UpdateAsync(string keyName, object value);

    /// <inheritdoc/>
    Task UpdateAsync(string keyName, string value);
}
