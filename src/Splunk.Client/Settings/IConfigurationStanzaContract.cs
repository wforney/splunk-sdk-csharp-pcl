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

using System.Collections;
using System.Diagnostics.Contracts;
using Splunk.Client.Arguments;

//// TODO:
//// [O] Contracts
//// [O] Documentation

namespace Splunk.Client.Settings
{
    /// <summary>
    /// A configuration stanza contract.
    /// </summary>
    /// <seealso cref="T:Splunk.Client.IConfigurationStanza"/>
    [ContractClassFor(typeof(IConfigurationStanza))]
    internal abstract class IConfigurationStanzaContract : IConfigurationStanza
    {
        public abstract ConfigurationSetting this[int index] { get; }
        public abstract int Count { get; }
        public abstract string Author { get; }

        public abstract Task GetAsync();

        public abstract Task<bool> UpdateAsync(params Argument[] arguments);

        public Task<string> GetAsync(string keyName)
        {
            ArgumentNullException.ThrowIfNull(keyName);
            return default!;
        }

        public abstract IEnumerator<ConfigurationSetting> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => default!;

        public abstract Task RemoveAsync();

        public abstract Task<bool> SendAsync(HttpMethod method, string action, params Argument[] arguments);

        public abstract Task<bool> UpdateAsync(IEnumerable<Argument> arguments);

        public Task UpdateAsync(string keyName, object value)
        {
            ArgumentNullException.ThrowIfNull(keyName);
            ArgumentNullException.ThrowIfNull(value);
            return default!;
        }

        public Task UpdateAsync(string keyName, string value)
        {
            ArgumentNullException.ThrowIfNull(keyName);
            ArgumentNullException.ThrowIfNull(value);
            return default!;
        }
    }
}
