// <copyright file="Context.cs" company="Splunk, Inc.">
// Copyright © 2014 Splunk, Inc.
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

using System.Globalization;
using System.Text;

namespace Splunk.Client
{
    /// <summary>
    /// Provides a class for sending HTTP requests and receiving HTTP responses from a Splunk server.
    /// </summary>
    /// <seealso cref="T:System.IDisposable" />
    public class Context : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class with a protocol, host,
        /// and port number.
        /// </summary>
        /// <param name="scheme">The <see cref="Scheme" /> used to communicate with <see cref="Host" /></param>
        /// <param name="host">The DNS name of a Splunk server instance.</param>
        /// <param name="port">The port number used to communicate with <see cref="Host" />.</param>
        /// <param name="timeout">The timeout.</param>
        public Context(Scheme scheme, string host, int port, TimeSpan timeout = default)
            : this(scheme, host, port, timeout, null)
        {
            // NOTE: This constructor obviates the need for callers to include a using for System.Net.Http.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class with a protocol, host,
        /// port number, and optional message handler.
        /// </summary>
        /// <param name="scheme">The <see cref="Scheme" /> used to communicate with <see cref="Host" />.</param>
        /// <param name="host">The DNS name of a Splunk server instance.</param>
        /// <param name="port">The port number used to communicate with <see cref="Host" />.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="handler">The <see cref="HttpMessageHandler" /> responsible for processing the HTTP response messages.</param>
        /// <param name="disposeHandler"><c>true</c> if the inner handler should be disposed of by Dispose, <c>false</c> if you
        /// intend to reuse the inner handler.</param>
        public Context(Scheme scheme, string host, int port, TimeSpan timeout, HttpMessageHandler? handler, bool disposeHandler = true)
        {
            Contract.Requires<ArgumentException>(scheme is Scheme.Http or Scheme.Https);
            Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(host));
            Contract.Requires<ArgumentException>(port is >= 0 and <= 65535);

            this.Scheme = scheme;
            this.Host = host;
            this.Port = port;
            this.httpClient = handler is null ? new HttpClient(new HttpClientHandler { UseCookies = false }) : new HttpClient(handler, disposeHandler);
            this.httpClient.DefaultRequestHeaders.Add("User-Agent", "splunk-sdk-csharp/2.2.9");
            this.CookieJar = new CookieStore();

            if (timeout != default)
            {
                this.httpClient.Timeout = timeout;
            }
        }

        /// <summary>
        /// CookieStore to store authentication cookies.
        /// </summary>
        public CookieStore CookieJar;

        /// <summary>
        /// Gets the Splunk host name associated with the current <see cref="Context" />.
        /// </summary>
        /// <value>A Splunk host name.</value>
        public string Host { get; private set; }

        /// <summary>
        /// Gets the management port number used to communicate with <see cref="Host" />
        /// </summary>
        /// <value>A Splunk management port number.</value>
        public int Port { get; private set; }

        /// <summary>
        /// Gets the scheme used to communicate with <see cref="Host" /> on <see cref="Port" />.
        /// </summary>
        /// <value>The scheme used to communicate with <see cref="Host" /> on <see cref="Port" />.</value>
        public Scheme Scheme { get; private set; }

        /// <summary>
        /// Gets or sets the session key used by the current <see cref="Context" />.
        /// </summary>
        /// <value>The session key used by the current <see cref="Context" /> or <c> null</c>.</value>
        /// <remarks>This value is <c>null</c> until it is set.</remarks>
        public string? SessionKey { get; set; }

        /// <summary>
        /// Sends a DELETE request as an asynchronous operation.
        /// </summary>
        /// <param name="ns">An object identifying a Splunk services namespace.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="argumentSets">The argument sets.</param>
        /// <returns>The response to the DELETE request.</returns>
        public virtual async Task<Response> DeleteAsync(Namespace ns, ResourceName resource,
            params IEnumerable<Argument>[] argumentSets)
        {
            var token = CancellationToken.None;
            var response = await this.GetResponseAsync(HttpMethod.Delete, ns, resource, null, argumentSets, token).ConfigureAwait(false);
            return response;
        }

        /// <summary>
        /// Releases all disposable resources used by the current <see cref="Context" />.
        /// </summary>
        /// <seealso cref="M:System.IDisposable.Dispose()" />
        /// <remarks>Do not override this method. Override <see cref="Dispose(bool)" /> instead.</remarks>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this); // Enables derivatives that introduce finalizers from needing to reimplement
        }

        /// <summary>
        /// Sends a GET request as an asynchronous operation.
        /// </summary>
        /// <param name="ns">An object identifying a Splunk services namespace.</param>
        /// <param name="resource">An object identifiying a Splunk resource in the context of <paramref name="ns" />.</param>
        /// <param name="argumentSets">The argument sets.</param>
        /// <returns>The response to the GET request.</returns>
        public virtual async Task<Response> GetAsync(Namespace ns, ResourceName resource,
            params IEnumerable<Argument>[] argumentSets)
        {
            var token = CancellationToken.None;
            var response = await this.GetResponseAsync(HttpMethod.Get, ns, resource, null, argumentSets, token).ConfigureAwait(false);
            return response;
        }

        /// <summary>
        /// Sends a GET request as an asynchronous operation.
        /// </summary>
        /// <param name="ns">An object identifying a Splunk services namespace.</param>
        /// <param name="resourceName">An object identifiying a Splunk resource in the context of <paramref name="ns" />.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <param name="argumentSets">The argument sets.</param>
        /// <returns>The response to the GET request.</returns>
        public virtual async Task<Response> GetAsync(Namespace ns, ResourceName resourceName, CancellationToken token,
            params IEnumerable<Argument>[] argumentSets)
        {
            var response = await this.GetResponseAsync(HttpMethod.Get, ns, resourceName, null, argumentSets, token).ConfigureAwait(false);
            return response;
        }

        /// <summary>
        /// Sends a GET request as an asynchronous operation.
        /// </summary>
        /// <param name="ns">An object identifying a Splunk services namespace.</param>
        /// <param name="resource">An object identifiying a Splunk resource in the context of <paramref name="ns" />.</param>
        /// <param name="argumentSets">The argument sets.</param>
        /// <returns>The response to the GET request.</returns>
        public virtual async Task<HttpResponseMessage> GetHttpResponseMessageAsync(Namespace ns, ResourceName resource,
            params IEnumerable<Argument>[] argumentSets)
        {
            var token = CancellationToken.None;
            var message = await this.SendAsync(HttpMethod.Get, ns, resource, null, argumentSets, token).ConfigureAwait(false);
            return message;
        }

        /// <summary>
        /// Sends a GET request as an asynchronous operation.
        /// </summary>
        /// <param name="ns">An object identifying a Splunk services namespace.</param>
        /// <param name="resource">An object identifiying a Splunk resource in the context of <paramref name="ns" />.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <param name="argumentSets">The argument sets.</param>
        /// <returns>The response to the GET request.</returns>
        public virtual async Task<HttpResponseMessage> GetHttpResponseMessageAsync(Namespace ns, ResourceName resource,
            CancellationToken token, params IEnumerable<Argument>[] argumentSets)
        {
            var message = await this.SendAsync(HttpMethod.Get, ns, resource, null, argumentSets, token).ConfigureAwait(false);
            return message;
        }

        /// <summary>
        /// Sends a POST request as an asynchronous operation.
        /// </summary>
        /// <param name="ns">An object identifying a Splunk services namespace.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="argumentSets">The argument sets.</param>
        /// <returns>The response to the GET request.</returns>
        public virtual async Task<Response> PostAsync(Namespace ns, ResourceName resource,
            params IEnumerable<Argument>[] argumentSets)
        {
            var content = CreateStringContent(argumentSets);
            return await this.PostAsync(ns, resource, content, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a POST request as an asynchronous operation.
        /// </summary>
        /// <param name="ns">An object identifying a Splunk services namespace.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="content">The content.</param>
        /// <param name="argumentSets">The argument sets.</param>
        /// <returns>The response to the GET request.</returns>
        public virtual async Task<Response> PostAsync(Namespace ns, ResourceName resource, HttpContent content, params IEnumerable<Argument>[]? argumentSets)
        {
            var token = CancellationToken.None;
            var response = await this.GetResponseAsync(HttpMethod.Post, ns, resource, content, argumentSets, token).ConfigureAwait(false);
            return response;
        }

        /// <summary>
        /// Sends a GET, POST, or DELETE request as an asynchronous operation.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="ns">An object identifying a Splunk services namespace.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="argumentSets">The argument sets.</param>
        /// <returns>The response to the GET, POST, or DELETE request.</returns>
        public virtual async Task<Response> SendAsync(HttpMethod method, Namespace ns, ResourceName resource, params IEnumerable<Argument>[]? argumentSets)
        {
            ArgumentNullException.ThrowIfNull(method);
            Contract.Requires<ArgumentException>(method == HttpMethod.Delete || method == HttpMethod.Get || method == HttpMethod.Post);
            var token = CancellationToken.None;
            HttpContent? content = null;

            if (method == HttpMethod.Post)
            {
                content = CreateStringContent(argumentSets);
                argumentSets = null;
            }

            var response = await this.GetResponseAsync(method, ns, resource, content, argumentSets, token).ConfigureAwait(false);
            return response;
        }

        /// <summary>
        /// Converts the current <see cref="Context" /> to its string representation.
        /// </summary>
        /// <returns>A string representation of the current <see cref="Context" />.</returns>
        /// <seealso cref="M:System.Object.ToString()" />
        public override string? ToString() => (string?)string.Concat(CultureInfo.InvariantCulture, SchemeStrings[(int)this.Scheme], "://", this.Host, ":", this.Port.ToString(CultureInfo.InvariantCulture.NumberFormat));

        /// <summary>
        /// Releases all resources used by the <see cref="Context" />.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release
        /// only unmanaged resources.</param>
        /// <remarks>Subclasses should implement the disposable pattern as follows:
        /// <list type="bullet"><item><description>Override this method and call it from the override.</description></item><item><description>Provide a finalizer, if needed, and call this method from it.</description></item><item><description>
        /// To help ensure that resources are always cleaned up appropriately, ensure that the
        /// override is callable multiple times without throwing an exception.
        /// </description></item></list>
        /// There is no performance benefit in overriding this method on types that use only managed
        /// resources (such as arrays) because they are automatically reclaimed by the garbage
        /// collector. See <a href="http://goo.gl/VPIovn">Implementing a Dispose Method</a>.</remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this.httpClient is not null)
            {
                this.httpClient.Dispose();
                this.httpClient = null;
            }
        }

        /// <summary>
        /// The scheme strings
        /// </summary>
        private static readonly string[] SchemeStrings = { "http", "https" };
        /// <summary>
        /// The HTTP client
        /// </summary>
        private HttpClient? httpClient;

        /// <summary>
        /// Sets the HTTP client.
        /// </summary>
        /// <value>The HTTP client.</value>
        private HttpClient HttpClient => this.httpClient ?? throw new ObjectDisposedException(this.ToString());

        /// <summary>
        /// Creates the content of the string.
        /// </summary>
        /// <param name="argumentSets">The argument sets.</param>
        /// <returns>System.Net.Http.StringContent.</returns>
        private static StringContent CreateStringContent(params IEnumerable<Argument>[]? argumentSets)
        {
            if (argumentSets is null)
            {
                return new StringContent(string.Empty);
            }

            var body = string.Join("&",
                from args in argumentSets
                where args is not null
                from arg in args
                select string.Join("=", Uri.EscapeDataString(arg.Name), Uri.EscapeDataString(arg.Value.ToString())));

            var stringContent = new StringContent(body);
            return stringContent;
        }

        /// <summary>
        /// Creates the service URI.
        /// </summary>
        /// <param name="ns">The ns.</param>
        /// <param name="name">The name.</param>
        /// <param name="argumentSets">The argument sets.</param>
        /// <returns>System.Uri.</returns>
        private Uri CreateServiceUri(Namespace ns, ResourceName name, params IEnumerable<Argument>[]? argumentSets)
        {
            var builder = new StringBuilder(this.ToString())
                .Append('/')
                .Append(ns.ToUriString())
                .Append('/')
                .Append(name.ToUriString());

            if (argumentSets is not null)
            {
                var query = string.Join(
                    "&",
                    from args in argumentSets
                    where args is not null
                    from arg in args
                    select string.Join("=", Uri.EscapeDataString(arg.Name), Uri.EscapeDataString(arg.Value.ToString())));

                if (query.Length > 0)
                {
                    _ = builder.Append('?').Append(query);
                }
            }

            var uri = UriConverter.Instance.Convert(builder.ToString());
            return uri;
        }

        /// <summary>
        /// Get response as an asynchronous operation.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="ns">The ns.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="content">The content.</param>
        /// <param name="argumentSets">The argument sets.</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A Task&lt;Splunk.Client.Response&gt; representing the asynchronous operation.</returns>
        private async Task<Response> GetResponseAsync(HttpMethod method, Namespace ns, ResourceName resource, HttpContent? content, IEnumerable<Argument>[]? argumentSets, CancellationToken cancellationToken)
        {
            var message = await this.SendAsync(method, ns, resource, content, argumentSets, cancellationToken);
            var response = await Response.CreateAsync(message).ConfigureAwait(false);

            // If a Set-Cookie Header is received, parse it and add/update the cookie store
            if (response.Message.Headers.Contains("Set-Cookie"))
            {
                foreach (var setCookieString in response.Message.Headers.GetValues("Set-Cookie"))
                {
                    this.CookieJar.AddCookie(setCookieString);
                }
            }

            return response;
        }

        /// <summary>
        /// Send as an asynchronous operation.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="ns">The ns.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="content">The content.</param>
        /// <param name="argumentSets">The argument sets.</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A Task&lt;System.Net.Http.HttpResponseMessage&gt; representing the asynchronous operation.</returns>
        private async Task<HttpResponseMessage> SendAsync(HttpMethod method, Namespace ns, ResourceName resource, HttpContent? content, IEnumerable<Argument>[]? argumentSets, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ns);
            ArgumentNullException.ThrowIfNull(resource);

            var serviceUri = this.CreateServiceUri(ns, resource, argumentSets);

            using var request = new HttpRequestMessage(method, serviceUri) { Content = content };

            // If the CookieStore has cookies, include them in the Cookie header Otherwise if there
            // is a SessionKey, include it in the Authorization header
            if (this.CookieJar.IsEmpty())
            {
                if (this.SessionKey is not null)
                {
                    request.Headers.Add("Authorization", string.Concat("Splunk ", this.SessionKey));
                }
            }
            else
            {
                request.Headers.Add("Cookie", this.CookieJar.GetCookieHeader());
            }

            var message = await this.HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            return message;
        }
    }
}
