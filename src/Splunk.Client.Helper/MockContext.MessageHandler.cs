// <copyright file="MockContext.MessageHandler.cs" company="Splunk, Inc.">
//     Copyright 2014 Splunk, Inc.
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

using System.Security.Cryptography;
using System.Text;

namespace Splunk.Client.Helper;

/// <summary>
/// Class MockContext. Implements the <see cref="Context" />
/// </summary>
/// <seealso cref="Context" />
public partial class MockContext
{
    /// <summary>
    /// Class MessageHandler. Implements the <see cref="System.Net.Http.HttpMessageHandler" />
    /// </summary>
    /// <seealso cref="System.Net.Http.HttpMessageHandler" />
    private abstract class MessageHandler : HttpMessageHandler
    {
        private static readonly byte[] crlf = new byte[] { 0x0D, 0x0A };

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>The client.</value>
        public HttpClient Client { get; } = new HttpClient(new HttpClientHandler { UseCookies = false });

        /// <summary>
        /// Computes the checksum.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="stream">The stream.</param>
        /// <returns>Tuple&lt;System.String, System.Int64&gt;.</returns>
        protected static async Task<Tuple<string, long>> ComputeChecksum(HttpRequestMessage request, MemoryStream stream)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(stream);

            string text;
            byte[] bytes;

            text = string.Format("{0} {1} HTTP/{2}", request.Method, request.RequestUri, request.Version);
            bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
            stream.Write(crlf, 0, crlf.Length);

            foreach (var header in request.Headers)
            {
                text = string.Format("{0}: {1}", header.Key, string.Join(", ", header.Value));
                bytes = Encoding.UTF8.GetBytes(text);
                stream.Write(bytes, 0, bytes.Length);
                stream.Write(crlf, 0, crlf.Length);
            }

            if (request.Content != null)
            {
                foreach (var header in request.Content.Headers)
                {
                    text = string.Format("{0}: {1}", header.Key, string.Join(", ", header.Value));
                    bytes = Encoding.UTF8.GetBytes(text);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Write(crlf, 0, crlf.Length);
                }
            }

            stream.Write(crlf, 0, crlf.Length);
            var offset = stream.Position;

            if (request.Content != null)
            {
                await request.Content.CopyToAsync(stream);
            }

            var hashAlgorithm = SHA512.Create();
            _ = stream.Seek(0, SeekOrigin.Begin);

            var hash = hashAlgorithm.ComputeHash(stream);
            var checksum = Convert.ToBase64String(hash);

            return new Tuple<string, long>(checksum, offset);
        }

        /// <summary>
        /// Duplicates the and compute checksum.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Tuple&lt;System.String, HttpRequestMessage&gt;.</returns>
        protected static async Task<Tuple<string, HttpRequestMessage>> DuplicateAndComputeChecksum(HttpRequestMessage request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var stream = new MemoryStream();

            try
            {
                var result = await ComputeChecksum(request, stream);
                var checksum = result.Item1;
                var offset = result.Item2;

                StreamContent? content = null;

                if (request.Content is null)
                {
                    stream.Close();
                }
                else
                {
                    content = new StreamContent(stream);

                    foreach (var header in request.Content.Headers)
                    {
                        content.Headers.Add(header.Key, header.Value);
                    }

                    content.Headers.ContentLength = request.Content.Headers.ContentLength;
                    _ = stream.Seek(offset, SeekOrigin.Begin);
                }

                var duplicateRequest = new HttpRequestMessage(request.Method, request.RequestUri)
                {
                    Content = content,
                    Version = request.Version
                };

                foreach (var header in request.Headers)
                {
                    duplicateRequest.Headers.Add(header.Key, header.Value);
                }

                return new Tuple<string, HttpRequestMessage>(checksum, duplicateRequest);
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Client.Dispose();
                base.Dispose(true);
            }
        }
    }
}
