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

namespace Splunk.Client
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Splunk.Client.Converters;
    using Splunk.Client.Syndication;

    /// <summary>
    /// Provides information about a Splunk server instance.
    /// </summary>
    /// <seealso cref="T:Splunk.Client.Resource"/>
    /// <seealso cref="T:Splunk.Client.IServerInfo"/>
    public class ServerInfo : Resource, IServerInfo
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerInfo"/> class.
        /// </summary>
        /// <param name="entry">
        /// An object representing a Splunk atom entry response.
        /// </param>
        /// <param name="generatorVersion">
        /// The version of the generator producing the <see cref="AtomFeed"/>
        /// feed containing <paramref name="entry"/>.
        /// </param>
        protected internal ServerInfo(AtomEntry entry, Version generatorVersion)
            : base(entry, generatorVersion)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerInfo"/> class.
        /// </summary>
        /// <param name="feed">
        /// An object representing a Splunk atom feed response.
        /// </param>
        protected internal ServerInfo(AtomFeed feed)
        {
            this.Initialize(feed);
        }

        /// <summary>
        /// Infrastructure. Initializes a new instance of the
        /// <see cref= "BaseResource"/> class.
        /// </summary>
        /// <remarks>
        /// This API supports the Splunk client infrastructure and is not intended to
        /// be used directly from your code.
        /// </remarks>
        public ServerInfo()
        { }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public virtual string ActiveLicenseGroup => this.Content.GetValue("ActiveLicenseGroup", StringConverter.Instance);

        /// <inheritdoc/>
        public virtual dynamic AddOns => this.Content.GetValue("AddOns", ExpandoAdapter.Converter.Instance);

        /// <inheritdoc/>
        public virtual string Build => this.Content.GetValue("Build", StringConverter.Instance);

        /// <inheritdoc/>
        public virtual string CpuArchitecture => this.Content.GetValue("CpuArch", StringConverter.Instance);

        /// <inheritdoc/>
        public virtual Eai Eai => this.Content.GetValue("Eai", Eai.Converter.Instance);

        /// <inheritdoc/>
        public virtual Guid Guid => this.Content.GetValue("Guid", GuidConverter.Instance);

        /// <inheritdoc/>
        public virtual bool IsFree => this.Content.GetValue("IsFree", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual bool IsRealTimeSearchEnabled => this.Content.GetValue("RtsearchEnabled", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual bool IsTrial => this.Content.GetValue("IsTrial", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual ReadOnlyCollection<string> LicenseKeys => this.Content.GetValue(
                    "LicenseKeys", ReadOnlyCollectionConverter<List<string>, StringConverter, string>.Instance);

        /// <inheritdoc/>
        public virtual ReadOnlyCollection<string> LicenseLabels => this.Content.GetValue(
                    "LicenseLabels", ReadOnlyCollectionConverter<List<string>, StringConverter, string>.Instance);

        /// <inheritdoc/>
        public virtual string LicenseSignature => this.Content.GetValue("LicenseSignature", StringConverter.Instance);

        /// <inheritdoc/>
        public virtual LicenseState LicenseState => this.Content.GetValue("LicenseState", EnumConverter<LicenseState>.Instance);

        /// <inheritdoc/>
        public virtual Guid MasterGuid => this.Content.GetValue("MasterGuid", GuidConverter.Instance);

        /// <inheritdoc/>
        public virtual ServerMode Mode => this.Content.GetValue("Mode", EnumConverter<ServerMode>.Instance);

        /// <inheritdoc/>
        public virtual int NumberOfCores => this.Content.GetValue("NumberOfCores", Int32Converter.Instance);

        /// <inheritdoc/>
        public virtual string OSBuild => this.Content.GetValue("OsBuild", StringConverter.Instance);

        /// <inheritdoc/>
        public virtual string OSName => this.Content.GetValue("OsName", StringConverter.Instance);

        /// <inheritdoc/>
        public virtual string OSVersion => this.Content.GetValue("OsVersion", StringConverter.Instance);

        /// <inheritdoc/>
        public virtual long PhysicalMemoryMB => this.Content.GetValue("PhysicalMemoryMB", Int64Converter.Instance);

        /// <inheritdoc/>
        public virtual bool RealTimeSearchEnabled => this.Content.GetValue("RtsearchEnabled", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual string ServerName => this.Content.GetValue("ServerName", StringConverter.Instance);

        /// <inheritdoc/>
        public virtual DateTime StartupTime => this.Content.GetValue("StartupTime", UnixDateTimeConverter.Instance);

        /// <inheritdoc/>
        public virtual Version Version => this.Content.GetValue("Version", VersionConverter.Instance);

        #endregion
    }
}
