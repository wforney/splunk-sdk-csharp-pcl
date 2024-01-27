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

using System.Runtime.Serialization;

namespace Splunk.Client;

public partial class ApplicationCollection
{
    /// <summary>
    /// Arguments for creation.
    /// </summary>
    /// <seealso cref="Args{CreationArgs}"/>
    private class CreationArgs : Args<CreationArgs>
    {
        [DataMember(Name = "explicit_appname", IsRequired = true)]
        public string ExplicitApplicationName { get; set; } = string.Empty;

        [DataMember(Name = "filename", IsRequired = true)]
        public bool? Filename { get; set; }

        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; } = string.Empty;

        [DataMember(Name = "template", EmitDefaultValue = true)]
        public string Template { get; set; } = default!;

        [DataMember(Name = "update", EmitDefaultValue = false)]
        public bool? Update { get; set; }
    }
}
