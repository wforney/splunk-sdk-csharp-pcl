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

using System.Collections.ObjectModel;
using System.Net;
using Splunk.Client.Syndication;

namespace Splunk.Client;

/// <summary>
/// Provides an object representation of a collection of Splunk applications.
/// </summary>
/// <seealso cref="EntityCollection{Application,Resource}"/>
/// <seealso cref="IApplicationCollection{Application}"/>
public partial class ApplicationCollection : EntityCollection<Application, Resource>, IApplicationCollection<Application>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationCollection"/>
    /// class.
    /// </summary>
    /// <param name="service">
    /// An object representing a root Splunk service endpoint.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="service"/> is <c>null</c>.
    /// </exception>
    protected internal ApplicationCollection(Service service)
        : base(service, ClassResourceName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationCollection"/>
    /// class.
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
    /// <exception cref="InvalidDataException">
    /// <paramref name="feed"/> is in an invalid format.
    /// </exception>
    protected internal ApplicationCollection(Context context, AtomFeed feed) => this.Initialize(context, feed);

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationCollection"/>
    /// class.
    /// </summary>
    /// <param name="context">
    /// An object representing a Splunk server session.
    /// </param>
    /// <param name="ns">
    /// An object identifying a Splunk services namespace.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> or <paramref name="ns"/> are <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="ns"/> is not specific.
    /// </exception>
    protected internal ApplicationCollection(Context context, Namespace ns)
        : base(context, ns, ClassResourceName)
    {
    }

    /// <summary>
    /// Infrastructure. Initializes a new instance of the
    /// <see cref= "ApplicationCollection"/> class.
    /// </summary>
    /// <remarks>
    /// This API supports the Splunk client infrastructure and is not intended to
    /// be used directly from your code.
    /// </remarks>
    public ApplicationCollection()
    {
    }

    /// <inheritdoc/>
    public virtual ReadOnlyCollection<Message> Messages => this.Snapshot.GetValue("Messages") ?? NoMessages;

    /// <inheritdoc/>
    public virtual Pagination Pagination => this.Snapshot.GetValue("Pagination") ?? Pagination.None;

    /// <inheritdoc/>
    public async Task<Application> CreateAsync(string name, string template, ApplicationAttributes? attributes = null)
    {
        var arguments = new CreationArgs
        {
            ExplicitApplicationName = name,
            Filename = false,
            Name = name,
            Template = template
        }
        .AsEnumerable();

        if (attributes != null)
        {
            arguments = arguments.Concat(attributes);
        }

        var resourceEndpoint = await this.CreateAsync(arguments).ConfigureAwait(false);
        return resourceEndpoint;
    }

    /// <inheritdoc/>
    public virtual async Task GetSliceAsync(Filter criteria) => await this.GetSliceAsync(criteria.AsEnumerable()).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<Application> InstallAsync(string path, string? name = null, bool update = false)
    {
        var resourceName = ClassResourceName;

        var args = new CreationArgs()
        {
            ExplicitApplicationName = name ?? "",
            Filename = true,
            Name = path,
            Update = update
        };

        using var response = await this.Context.PostAsync(this.Namespace, resourceName, args).ConfigureAwait(false);
        await response.EnsureStatusCodeAsync(HttpStatusCode.Created).ConfigureAwait(false);

        var resourceEndpoint = await Entity<Resource>.CreateAsync<Application>(this.Context, response).ConfigureAwait(false);
        return resourceEndpoint;
    }

    /// <summary>
    /// Name of the class resource.
    /// </summary>
    internal static readonly ResourceName ClassResourceName = new("apps", "local");
}
