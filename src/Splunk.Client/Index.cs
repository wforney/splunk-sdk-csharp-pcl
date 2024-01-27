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

namespace Splunk.Client
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Splunk.Client.Converters;
    using Splunk.Client.Entities;
    using Splunk.Client.Syndication;

    /// <summary>
    /// Provides a class that represents a Splunk data index.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>References:</b>
    /// <list type="number">
    /// <item><description>
    ///   <a href="http://goo.gl/UscaUQ">REST API Reference: Indexes</a>.
    /// </description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="T:Splunk.Client.Entity{Splunk.Client.Resource}"/>
    /// <seealso cref="T:Splunk.Client.IIndex"/>
    public class Index : Entity<Resource>, IIndex
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Index"/> class.
        /// </summary>
        /// <param name="service">
        /// An object representing a root Splunk service endpoint.
        /// </param>
        /// <param name="name">
        /// An object identifying a Splunk resource within
        /// <paramref name= "service"/>.<see cref="Namespace"/>.
        /// </param>
        ///
        /// ### <exception cref="ArgumentNullException">
        /// <paramref name="service"/> or <paramref name="name"/> are <c>null</c>.
        /// </exception>
        protected internal Index(Service service, string name)
            : this(service.Context, service.Namespace, name)
        {
            Contract.Requires<ArgumentNullException>(service != null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class.
        /// </summary>
        /// <param name="context">
        /// An object representing a Splunk server session.
        /// </param>
        /// <param name="feed">
        /// A Splunk response atom feed.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/> or <paramref name="feed"/> are <c>null</c>.
        /// </exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// <paramref name="feed"/> is in an invalid format.
        /// </exception>
        protected internal Index(Context context, AtomFeed feed)
        {
            this.Initialize(context, feed);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Index"/> class.
        /// </summary>
        /// <param name="context">
        /// An object representing a Splunk server session.
        /// </param>
        /// <param name="ns">
        /// An object identifying a Splunk services namespace.
        /// </param>
        /// <param name="name">
        /// Name of the index to be represented by the current instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/>, <paramref name="ns"/>, or <paramref
        /// name="name"/> are <c>null</c>.
        /// </exception>
        internal Index(Context context, Namespace ns, string name)
            : base(context, ns, ClassResourceName, name)
        { }

        /// <summary>
        /// Infrastructure. Initializes a new instance of the <see cref= "Index"/>
        /// class.
        /// </summary>
        /// <remarks>
        /// This API supports the Splunk client infrastructure and is not intended to
        /// be used directly from your code.
        /// </remarks>
        public Index()
        { }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public bool AssureUTF8 => this.Content.GetValue("AssureUTF8", BooleanConverter.Instance);

        /// <inheritdoc/>
        public int BloomFilterTotalSizeKB => this.Content.GetValue("BloomfilterTotalSizeKB", Int32Converter.Instance);

        /// <inheritdoc/>
        public string BucketRebuildMemoryHint => this.Content.GetValue("BucketRebuildMemoryHint", StringConverter.Instance);

        /// <inheritdoc/>
        public string ColdPath => this.Content.GetValue("ColdPath", StringConverter.Instance);

        /// <inheritdoc/>
        public string ColdPathExpanded => this.Content.GetValue("ColdPathExpanded", StringConverter.Instance);

        /// <inheritdoc/>
        public long ColdPathMaxDataSizeMB => this.Content.GetValue("ColdPathMaxDataSizeMB", Int64Converter.Instance);

        /// <inheritdoc/>
        public string ColdToFrozenDir => this.Content.GetValue("ColdToFrozenDir", StringConverter.Instance);

        /// <inheritdoc/>
        public string ColdToFrozenScript => this.Content.GetValue("ColdToFrozenScript", StringConverter.Instance);

        /// <inheritdoc/>
        public bool CompressRawData => this.Content.GetValue("CompressRawdata", BooleanConverter.Instance);

        /// <inheritdoc/>
        public long CurrentDBSizeMB => this.Content.GetValue("CurrentDBSizeMB", Int32Converter.Instance);

        /// <inheritdoc/>
        public string DefaultDatabase => this.Content.GetValue("DefaultDatabase", StringConverter.Instance);

        /// <inheritdoc/>
        public bool Disabled => this.Content.GetValue("Disabled", BooleanConverter.Instance);

        /// <inheritdoc/>
        public Eai Eai => this.Content.GetValue("Eai", Eai.Converter.Instance);

        /// <inheritdoc/>
        public bool EnableOnlineBucketRepair => this.Content.GetValue("EnableOnlineBucketRepair", BooleanConverter.Instance);

        /// <inheritdoc/>
        public bool EnableRealTimeSearch => this.Content.GetValue("EnableRealtimeSearch", BooleanConverter.Instance);

        /// <inheritdoc/>
        public int FrozenTimePeriodInSecs => this.Content.GetValue("FrozenTimePeriodInSecs", Int32Converter.Instance);

        /// <inheritdoc/>
        public string HomePath => this.Content.GetValue("HomePath", StringConverter.Instance);

        /// <inheritdoc/>
        public string HomePathExpanded => this.Content.GetValue("HomePathExpanded", StringConverter.Instance);

        /// <inheritdoc/>
        public long HomePathMaxDataSizeMB => this.Content.GetValue("HomePathMaxDataSizeMB", Int64Converter.Instance);

        /// <inheritdoc/>
        public string IndexThreads => this.Content.GetValue("IndexThreads", StringConverter.Instance);

        /// <inheritdoc/>
        public bool IsInternal => this.Content.GetValue("IsInternal", BooleanConverter.Instance);

        /// <inheritdoc/>
        public bool IsReady => this.Content.GetValue("IsReady", BooleanConverter.Instance);

        /// <inheritdoc/>
        public bool IsVirtual => this.Content.GetValue("IsVirtual", BooleanConverter.Instance);

        /// <inheritdoc/>
        public long LastInitSequenceNumber => this.Content.GetValue("LastInitSequenceNumber", Int64Converter.Instance);

        /// <inheritdoc/>
        public long LastInitTime => this.Content.GetValue("LastInitTime", Int64Converter.Instance);

        /// <inheritdoc/>
        public string MaxBloomBackfillBucketAge => this.Content.GetValue("MaxBloomBackfillBucketAge", StringConverter.Instance);

        /// <inheritdoc/>
        public int MaxBucketSizeCacheEntries => this.Content.GetValue("MaxBucketSizeCacheEntries", Int32Converter.Instance);

        /// <inheritdoc/>
        public int MaxConcurrentOptimizes => this.Content.GetValue("MaxConcurrentOptimizes", Int32Converter.Instance);

        /// <inheritdoc/>
        public string MaxDataSize => this.Content.GetValue("MaxDataSize", StringConverter.Instance);

        /// <inheritdoc/>
        public int MaxHotBuckets => this.Content.GetValue("MaxHotBuckets", Int32Converter.Instance);

        /// <inheritdoc/>
        public int MaxHotIdleSecs => this.Content.GetValue("MaxHotIdleSecs", Int32Converter.Instance);

        /// <inheritdoc/>
        public int MaxHotSpanSecs => this.Content.GetValue("MaxHotSpanSecs", Int32Converter.Instance);

        /// <inheritdoc/>
        public int MaxMemMB => this.Content.GetValue("MaxMemMB", Int32Converter.Instance);

        /// <inheritdoc/>
        public int MaxMetaEntries => this.Content.GetValue("MaxMetaEntries", Int32Converter.Instance);

        /// <inheritdoc/>
        public int MaxRunningProcessGroups => this.Content.GetValue("MaxRunningProcessGroups", Int32Converter.Instance);

        /// <inheritdoc/>
        public int MaxRunningProcessGroupsLowPriority => this.Content.GetValue("MaxRunningProcessGroupsLowPriority", Int32Converter.Instance);

        /// <inheritdoc/>
        public DateTime MaxTime => this.Content.GetValue("MaxTime", DateTimeConverter.Instance);

        /// <inheritdoc/>
        public int MaxTimeUnreplicatedNoAcks => this.Content.GetValue("MaxTimeUnreplicatedNoAcks", Int32Converter.Instance);

        /// <inheritdoc/>
        public int MaxTimeUnreplicatedWithAcks => this.Content.GetValue("MaxTimeUnreplicatedWithAcks", Int32Converter.Instance);

        /// <inheritdoc/>
        public int MaxTotalDataSizeMB => this.Content.GetValue("MaxTotalDataSizeMB", Int32Converter.Instance);

        /// <inheritdoc/>
        public int MaxWarmDBCount => this.Content.GetValue("MaxWarmDBCount", Int32Converter.Instance);

        /// <inheritdoc/>
        public string MemPoolMB => this.Content.GetValue("MemPoolMB", StringConverter.Instance);

        /// <inheritdoc/>
        public string MinRawFileSyncSecs => this.Content.GetValue("MinRawFileSyncSecs", StringConverter.Instance);

        /// <inheritdoc/>
        public DateTime MinTime => this.Content.GetValue("MinTime", DateTimeConverter.Instance);

        /// <inheritdoc/>
        public int NumBloomFilters => this.Content.GetValue("NumBloomfilters", Int32Converter.Instance);

        /// <inheritdoc/>
        public int NumHotBuckets => this.Content.GetValue("NumHotBuckets", Int32Converter.Instance);

        /// <inheritdoc/>
        public int NumWarmBuckets => this.Content.GetValue("NumWarmBuckets", Int32Converter.Instance);

        /// <inheritdoc/>
        public int PartialServiceMetaPeriod => this.Content.GetValue("PartialServiceMetaPeriod", Int32Converter.Instance);

        /// <inheritdoc/>
        public int ProcessTrackerServiceInterval => this.Content.GetValue("ProcessTrackerServiceInterval", Int32Converter.Instance);

        /// <inheritdoc/>
        public int QuarantineFutureSecs => this.Content.GetValue("QuarantineFutureSecs", Int32Converter.Instance);

        /// <inheritdoc/>
        public int QuarantinePastSecs => this.Content.GetValue("QuarantinePastSecs", Int32Converter.Instance);

        /// <inheritdoc/>
        public int RawChunkSizeBytes => this.Content.GetValue("RawChunkSizeBytes", Int32Converter.Instance);

        /// <inheritdoc/>
        public int RepFactor => this.Content.GetValue("RepFactor", Int32Converter.Instance);

        /// <inheritdoc/>
        public int RotatePeriodInSecs => this.Content.GetValue("RotatePeriodInSecs", Int32Converter.Instance);

        /// <inheritdoc/>
        public int ServiceMetaPeriod => this.Content.GetValue("ServiceMetaPeriod", Int32Converter.Instance);

        /// <inheritdoc/>
        public bool ServiceOnlyAsNeeded => this.Content.GetValue("ServiceOnlyAsNeeded", BooleanConverter.Instance);

        /// <inheritdoc/>
        public int ServiceSubtaskTimingPeriod => this.Content.GetValue("ServiceSubtaskTimingPeriod", Int32Converter.Instance);

        /// <inheritdoc/>
        public string SummaryHomePathExpanded => this.Content.GetValue("SummaryHomePathExpanded", StringConverter.Instance);

        /// <inheritdoc/>
        public string SuppressBannerList => this.Content.GetValue("SuppressBannerList", StringConverter.Instance);

        /// <inheritdoc/>
        public bool Sync => this.Content.GetValue("Sync", BooleanConverter.Instance);

        /// <inheritdoc/>
        public bool SyncMeta => this.Content.GetValue("SyncMeta", BooleanConverter.Instance);

        /// <inheritdoc/>
        public string ThawedPath => this.Content.GetValue("ThawedPath", StringConverter.Instance);

        /// <inheritdoc/>
        public string ThawedPathExpanded => this.Content.GetValue("ThawedPathExpanded", StringConverter.Instance);

        /// <inheritdoc/>
        public int ThrottleCheckPeriod => this.Content.GetValue("ThrottleCheckPeriod", Int32Converter.Instance);

        /// <inheritdoc/>
        public long TotalEventCount => this.Content.GetValue("TotalEventCount", Int64Converter.Instance);

        /// <inheritdoc/>
        public string TStatsHomePath => this.Content.GetValue("TstatsHomePath", StringConverter.Instance);

        /// <inheritdoc/>
        public string TStatsHomePathExpanded => this.Content.GetValue("TstatsHomePathExpanded", StringConverter.Instance);

        #endregion

        #region Methods

        /// <inheritdoc/>
        public async Task DisableAsync()
        {
            var resourceName = new ResourceName(this.ResourceName, "disable");

            using var response = await this.Context.PostAsync(this.Namespace, resourceName).ConfigureAwait(false);
            await response.EnsureStatusCodeAsync(HttpStatusCode.OK).ConfigureAwait(false);
            await this.ReconstructSnapshotAsync(response).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task EnableAsync()
        {
            var resourceName = new ResourceName(this.ResourceName, "enable");

            using var response = await this.Context.PostAsync(this.Namespace, resourceName).ConfigureAwait(false);
            await response.EnsureStatusCodeAsync(HttpStatusCode.OK).ConfigureAwait(false);
            await this.ReconstructSnapshotAsync(response).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(IndexAttributes attributes)
        {
            return await this.UpdateAsync(attributes.AsEnumerable()).ConfigureAwait(false);
        }

        #endregion

        #region Privates/internals

        /// <summary>
        /// Name of the class resource.
        /// </summary>
        internal static readonly ResourceName ClassResourceName = new ResourceName("data", "indexes");

        #endregion

    }
}
