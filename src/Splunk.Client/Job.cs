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
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using Splunk.Client.Arguments;
    using Splunk.Client.Converters;
    using Splunk.Client.Entities;
    using Splunk.Client.Syndication;
    using BooleanConverter = Converters.BooleanConverter;
    using DateTimeConverter = Converters.DateTimeConverter;
    using Int32Converter = Converters.Int32Converter;
    using Int64Converter = Converters.Int64Converter;

    /// <summary>
    /// Provides an object representation of a Splunk search job.
    /// </summary>
    /// <seealso cref="T:Splunk.Client.Entity{Splunk.Client.Resource}"/>
    /// <seealso cref="T:Splunk.Client.IJob"/>
    public class Job : Entity<Resource>, IJob
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class.
        /// </summary>
        /// <param name="service">
        /// An object representing a root Splunk service endpoint.
        /// </param>
        /// <param name="name">
        /// An object identifying a Splunk resource within
        /// <paramref name= "service"/>.<see cref="Namespace"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="service"/> or <paramref name="name"/> are <c>null</c>.
        /// </exception>
        protected internal Job(Service service, string name)
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
        /// <param name="entry">
        /// A Splunk response atom feed.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/> or <paramref name="entry"/> are <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidDataException">
        /// <paramref name="entry"/> is in an invalid format.
        /// </exception>
        protected internal Job(Context context, AtomEntry entry)
        {
            this.Initialize(context, entry, new Version(0, 0));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Job"/> class.
        /// </summary>
        /// <param name="context">
        /// An object representing a Splunk server session.
        /// </param>
        /// <param name="ns">
        /// An object identifying a Splunk services namespace.
        /// </param>
        /// <param name="name">
        /// Name of the search <see cref="Job"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="name"/> is <c>null</c> or empty.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/> or <paramref name="ns"/> are <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="ns"/> is not specific.
        /// </exception>
        internal Job(Context context, Namespace ns, string name)
            : base(context, ns, JobCollection.ClassResourceName, name)
        { }

        /// <summary>
        /// Infrastructure. Initializes a new instance of the <see cref= "Job"/>
        /// class.
        /// </summary>
        /// <remarks>
        /// This API supports the Splunk client infrastructure and is not intended to
        /// be used directly from your code.
        /// </remarks>
        public Job()
        { }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public virtual bool CanSummarize => this.Content.GetValue("CanSummarize", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual DateTime CursorTime => this.Content.GetValue("CursorTime", DateTimeConverter.Instance);

        /// <inheritdoc/>
        public virtual int DefaultSaveTtl => this.Content.GetValue("DefaultSaveTTL", Int32Converter.Instance);

        /// <inheritdoc/>
        public virtual int DefaultTtl => this.Content.GetValue("DefaultTTL", Int32Converter.Instance);

        /// <inheritdoc/>
        public virtual long DiskUsage => this.Content.GetValue("DiskUsage", Int64Converter.Instance);

        /// <inheritdoc/>
        public virtual DispatchState DispatchState => this.Content.GetValue("DispatchState", EnumConverter<DispatchState>.Instance);

        /// <inheritdoc/>
        public virtual double DoneProgress => this.Content.GetValue("DoneProgress", DoubleConverter.Instance);

        /// <inheritdoc/>
        public virtual long DropCount => this.Content.GetValue("DropCount", Int64Converter.Instance);

        /// <inheritdoc/>
        public virtual Eai Eai => this.Content.GetValue("Eai", Eai.Converter.Instance);

        /// <inheritdoc/>
        public virtual DateTime EarliestTime => this.Content.GetValue("EarliestTime", DateTimeConverter.Instance);

        /// <inheritdoc/>
        public virtual long EventAvailableCount => this.Content.GetValue("EventAvailableCount", Int64Converter.Instance);

        /// <inheritdoc/>
        public virtual long EventCount => this.Content.GetValue("EventCount", Int64Converter.Instance);

        /// <inheritdoc/>
        public virtual int EventFieldCount => this.Content.GetValue("EventFieldCount", Int32Converter.Instance);

        /// <inheritdoc/>
        public virtual bool EventIsStreaming => this.Content.GetValue("EventIsStreaming", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual bool EventIsTruncated => this.Content.GetValue("EventIsTruncated", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual string EventSearch => this.Content.GetValue("EventSearch", StringConverter.Instance);

        /// <inheritdoc/>
        public virtual SortDirection EventSorting => this.Content.GetValue("EventSorting", EnumConverter<SortDirection>.Instance);

        /// <inheritdoc/>
        public virtual long IndexEarliestTime => this.Content.GetValue("IndexEarliestTime", Int64Converter.Instance);

        /// <inheritdoc/>
        public virtual long IndexLatestTime => this.Content.GetValue("IndexLatestTime", Int64Converter.Instance);

        /// <inheritdoc/>
        public virtual bool IsBatchModeSearch => this.Content.GetValue("IsBatchModeSearch", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual bool IsDone => this.Content.GetValue("IsDone", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual bool IsFailed => this.Content.GetValue("IsFailed", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual bool IsFinalized => this.Content.GetValue("IsFinalized", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual bool IsPaused => this.Content.GetValue("IsPaused", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual bool IsPreviewEnabled => this.Content.GetValue("IsPreviewEnabled", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual bool IsRealTimeSearch => this.Content.GetValue("IsRealTimeSearch", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual bool IsRemoteTimeline => this.Content.GetValue("IsRemoteTimeline", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual bool IsSaved => this.Content.GetValue("IsSaved", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual bool IsSavedSearch => this.Content.GetValue("IsSavedSearch", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual bool IsZombie => this.Content.GetValue("IsZombie", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual string Keywords => this.Content.GetValue("Keywords", StringConverter.Instance);

        /// <inheritdoc/>
        public virtual DateTime LatestTime => this.Content.GetValue("LatestTime", DateTimeConverter.Instance);

        //// TODO: Messages	{System.Dynamic.ExpandoObject}	System.Dynamic.ExpandoObject

        /// <inheritdoc/>
        public virtual string NormalizedSearch => this.Content.GetValue("NormalizedSearch", StringConverter.Instance);

        /// <inheritdoc/>
        public virtual int NumPreviews => this.Content.GetValue("NumPreviews", Int32Converter.Instance);

        /// <inheritdoc/>
        public virtual dynamic Performance => this.Content.GetValue("Performance", ExpandoAdapter.Converter.Instance);

        /// <inheritdoc/>
        public virtual int Pid => this.Content.GetValue("Pid", Int32Converter.Instance);

        /// <inheritdoc/>
        public virtual int Priority => this.Content.GetValue("Priority", Int32Converter.Instance);

        /// <inheritdoc/>
        public virtual string RemoteSearch => this.Content.GetValue("RemoteSearch", StringConverter.Instance);

        /// <inheritdoc/>
        public virtual string ReportSearch => this.Content.GetValue("ReportSearch", StringConverter.Instance);

        /// <inheritdoc/>
        public virtual dynamic Request => this.Content.GetValue("Request");

        /// <inheritdoc/>
        public virtual long ResultCount => this.Content.GetValue("ResultCount", Int64Converter.Instance);

        /// <inheritdoc/>
        public virtual bool ResultIsStreaming => this.Content.GetValue("ResultIsStreaming", BooleanConverter.Instance);

        /// <inheritdoc/>
        public virtual long ResultPreviewCount => this.Content.GetValue("ResultPreviewCount", Int64Converter.Instance);

        /// <inheritdoc/>
        public virtual double RunDuration => this.Content.GetValue("RunDuration", DoubleConverter.Instance);

        /// <inheritdoc/>
        public virtual RuntimeAdapter Runtime => this.Content.GetValue("Runtime", RuntimeAdapter.Converter.Instance);

        /// <inheritdoc/>
        public virtual long ScanCount => this.Content.GetValue("ScanCount", Int64Converter.Instance);

        /// <inheritdoc/>
        public virtual string Search => this.Snapshot.Title;

        /// <inheritdoc/>
        public virtual DateTime SearchEarliestTime => this.Content.GetValue("SearchEarliestTime", UnixDateTimeConverter.Instance);

        /// <inheritdoc/>
        public virtual DateTime SearchLatestTime => this.Content.GetValue("SearchLatestTime", UnixDateTimeConverter.Instance);

        /// <inheritdoc/>
        public virtual ReadOnlyCollection<string> SearchProviders => this.Content.GetValue(
                    "SearchProviders", ReadOnlyCollectionConverter<List<string>, StringConverter, string>.Instance);

        /// <inheritdoc/>
        public virtual string Sid => this.Name;

        /// <inheritdoc/>
        public virtual int StatusBuckets => this.Content.GetValue("StatusBuckets", Int32Converter.Instance);

        /// <inheritdoc/>
        public virtual long Ttl => this.Content.GetValue("Ttl", Int64Converter.Instance);

        #endregion

        #region Methods

        #region Getting, removing, and updating the current Job

        /// <inheritdoc/>
        public override async Task GetAsync()
        {
            await GetAsync(DispatchState.Running).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task GetAsync(DispatchState dispatchState, int delay = 30000, int retryInterval = 250)
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            // Timeout if Job is never at least the requiredState
            cancellationTokenSource.CancelAfter(delay);
            var token = cancellationTokenSource.Token;

            for (int i = 0; ; i++)
            {
                using (var response = await this.Context.GetAsync(this.Namespace, this.ResourceName, token).ConfigureAwait(false))
                {
                    if (response.Message.StatusCode != HttpStatusCode.NoContent)
                    {
                        await response.EnsureStatusCodeAsync(HttpStatusCode.OK).ConfigureAwait(false);
                        await this.ReconstructSnapshotAsync(response).ConfigureAwait(false);

                        // None or Parsing are the only states to continue checking status for
                        if (this.DispatchState >= dispatchState || this.DispatchState == DispatchState.Queued)
                        {
                            break;
                        }
                    }
                }

                await Task.Delay(retryInterval).ConfigureAwait(false);
                retryInterval += retryInterval / 2;
            }
        }

        /// <inheritdoc/>
        public virtual async Task TransitionAsync(DispatchState dispatchState, int delay = 30000, int retryInterval = 250)
        {
            if (this.DispatchState >= dispatchState)
            {
                return;
            }

            await this.GetAsync(dispatchState, delay, retryInterval).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> UpdateAsync(CustomJobArgs arguments)
        {
            return await this.UpdateAsync(arguments.AsEnumerable()).ConfigureAwait(false);
        }

        #endregion

        #region Retrieving search results

        /// <inheritdoc/>
        public virtual async Task<SearchResultStream> GetSearchEventsAsync(SearchEventArgs args)
        {
            var searchResults = await this.GetSearchResultsAsync(DispatchState.Done, "events", args).ConfigureAwait(false);
            return searchResults;
        }

        /// <inheritdoc/>
        public virtual async Task<SearchResultStream> GetSearchEventsAsync(int count = 0)
        {
            var args = new SearchEventArgs { Count = count };
            var searchResults = await this.GetSearchEventsAsync(args);
            return searchResults;
        }

        /// <inheritdoc/>
        public virtual async Task<SearchResultStream> GetSearchPreviewAsync(SearchResultArgs args)
        {
            var searchResults = await this.GetSearchResultsAsync(DispatchState.Running, "results_preview", args).ConfigureAwait(false);
            return searchResults;
        }

        /// <inheritdoc/>
        public virtual async Task<SearchResultStream> GetSearchPreviewAsync(int count = 0)
        {
            var args = new SearchResultArgs { Count = count };
            var searchResults = await this.GetSearchPreviewAsync(args);
            return searchResults;
        }

        /// <inheritdoc/>
        public virtual async Task<HttpResponseMessage> GetSearchResponseMessageAsync(SearchResultArgs args, OutputMode outputMode = OutputMode.Xml)
        {
            await this.TransitionAsync(DispatchState.Done).ConfigureAwait(false);

            var resourceName = new ResourceName(this.ResourceName, "results");
            var requestArgs = args.AsEnumerable().Concat(new SearchResponseMessageArgs { OutputMode = outputMode });
            var message = await this.Context.GetHttpResponseMessageAsync(this.Namespace, resourceName, requestArgs).ConfigureAwait(false);

            try
            {
                message.EnsureSuccessStatusCode();
            }
            catch
            {
                message.Dispose();
                throw;
            }

            return message;
        }

        /// <inheritdoc/>
        public virtual async Task<HttpResponseMessage> GetSearchResponseMessageAsync(int count = 0, OutputMode outputMode = OutputMode.Xml)
        {
            var args = new SearchResultArgs { Count = count };
            return await this.GetSearchResponseMessageAsync(args, outputMode);
        }

        /// <inheritdoc/>
        public virtual async Task<SearchResultStream> GetSearchResultsAsync(SearchResultArgs args)
        {
            var searchResults = await this.GetSearchResultsAsync(DispatchState.Done, "results", args).ConfigureAwait(false);
            return searchResults;
        }

        /// <inheritdoc/>
        public virtual async Task<SearchResultStream> GetSearchResultsAsync(int count = 0)
        {
            var args = new SearchResultArgs { Count = count };
            var searchResults = await this.GetSearchResultsAsync(args);
            return searchResults;
        }

        #endregion

        #region Job control

        /// <inheritdoc/>
        public virtual async Task CancelAsync()
        {
            await this.TransitionAsync(DispatchState.Running).ConfigureAwait(false);
            await this.PostControlCommandAsync(Cancel).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task DisablePreviewAsync()
        {
            await this.TransitionAsync(DispatchState.Running).ConfigureAwait(false);
            await this.PostControlCommandAsync(DisablePreview).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task EnablePreviewAsync()
        {
            await this.TransitionAsync(DispatchState.Running).ConfigureAwait(false);
            await this.PostControlCommandAsync(EnablePreview).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task FinalizeAsync()
        {
            await this.TransitionAsync(DispatchState.Running).ConfigureAwait(false);
            await this.PostControlCommandAsync(Finalize).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task PauseAsync()
        {
            await this.TransitionAsync(DispatchState.Running).ConfigureAwait(false);
            await this.PostControlCommandAsync(Pause).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task SaveAsync()
        {
            await this.TransitionAsync(DispatchState.Running).ConfigureAwait(false);
            await this.PostControlCommandAsync(Save).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task SetPriorityAsync(int priority)
        {
            await this.TransitionAsync(DispatchState.Running).ConfigureAwait(false);

            await this.PostControlCommandAsync(new Argument[] 
            { 
                new Argument("action", "priority"),
                new Argument("priority", priority.ToString())
            }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task SetTtlAsync(int ttl)
        {
            await this.TransitionAsync(DispatchState.Running).ConfigureAwait(false);

            await this.PostControlCommandAsync(new Argument[]
            { 
                new Argument("action", "setttl"),
                new Argument("ttl", ttl.ToString())
            }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task TouchAsync(int ttl)
        {
            await this.TransitionAsync(DispatchState.Running).ConfigureAwait(false);

            await this.PostControlCommandAsync(new Argument[]
            { 
                new Argument("action", "touch"),
                new Argument("ttl", ttl.ToString())
            }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task UnpauseAsync()
        {
            await this.TransitionAsync(DispatchState.Running).ConfigureAwait(false);
            await this.PostControlCommandAsync(Unpause).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task UnsaveAsync()
        {
            await this.TransitionAsync(DispatchState.Running).ConfigureAwait(false);
            await this.PostControlCommandAsync(Unsave).ConfigureAwait(false);
        }

        #endregion

        #region Infrastructure

        /// <inheritdoc/>
        protected internal override async Task<bool> ReconstructSnapshotAsync(Response response)
        {
            var entry = new AtomEntry();

            await entry.ReadXmlAsync(response.XmlReader).ConfigureAwait(false);
            this.CreateSnapshot(entry, new Version(0, 0));

            return true;
        }

        #endregion

        #endregion

        #region Privates/internals

        static readonly Argument[] Cancel = new Argument[] 
        { 
            new Argument("action", "cancel")
        };

        static readonly Argument[] DisablePreview = new Argument[] 
        { 
            new Argument("action", "disablepreview") 
        };

        static readonly Argument[] EnablePreview = new Argument[] 
        { 
            new Argument("action", "enablepreview") 
        };

        static readonly Argument[] Finalize = new Argument[] 
        { 
            new Argument("action", "finalize") 
        };

        static readonly Argument[] Pause = new Argument[]
        { 
            new Argument("action", "pause") 
        };

        static readonly Argument[] Save = new Argument[]
        {
            new Argument("action", "save") 
        };

        static readonly Argument[] Unpause = new Argument[] 
        { 
            new Argument("action", "unpause") 
        };

        static readonly Argument[] Unsave = new Argument[] 
        { 
            new Argument("action", "unsave") 
        };

        async Task<SearchResultStream> GetSearchResultsAsync(DispatchState dispatchState, string endpoint, IEnumerable<Argument> args)
        {
            await this.TransitionAsync(dispatchState).ConfigureAwait(false);

            var resourceName = new ResourceName(this.ResourceName, endpoint);
            var response = await this.Context.GetAsync(this.Namespace, resourceName, args).ConfigureAwait(false);

            try
            {
                var searchResults = await SearchResultStream.CreateAsync(response).ConfigureAwait(false);
                return searchResults;
            }
            catch 
            {
                response.Dispose();
                throw;
            }
        }

        async Task PostControlCommandAsync(IEnumerable<Argument> args)
        {
            var resourceName = new ResourceName(this.ResourceName, "control");

            using var response = await this.Context.PostAsync(this.Namespace, resourceName, args).ConfigureAwait(false);
            await response.EnsureStatusCodeAsync(HttpStatusCode.OK).ConfigureAwait(false);
        }

        #endregion

        #region Types

        /// <summary>
        /// Arguments for update.
        /// </summary>
        class SearchResponseMessageArgs : Args<SearchResponseMessageArgs>
        {
            /// <summary>
            /// Gets or sets a value that indicates whether Splunk should check
            /// Splunkbase for updates to an <see cref="Application"/>.
            /// </summary>
            /// <value>
            /// <c>true</c> if check for updates, <c>false</c> if not.
            /// </value>
            [DataMember(Name = "output_mode", EmitDefaultValue = false)]
            [DefaultValue(OutputMode.Default)]
            public OutputMode OutputMode
            { get; set; }
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This is by design.")]
        public class RuntimeAdapter : ExpandoAdapter<RuntimeAdapter>
        {
            /// <inheritdoc/>
            public bool AutoCancel => this.GetValue("AutoCancel", BooleanConverter.Instance);

            /// <inheritdoc/>
            public bool AutoPause => this.GetValue("AutoPause", BooleanConverter.Instance);
        }
     
        #endregion
    }
}
