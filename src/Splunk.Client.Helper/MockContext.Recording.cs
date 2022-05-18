// <copyright file="MockContext.Recording.cs" company="Splunk, Inc.">
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

using System.Net;
using System.Runtime.Serialization;

namespace Splunk.Client.Helper;

/// <summary>
/// Class MockContext. Implements the <see cref="Context" />
/// </summary>
/// <seealso cref="Context" />
public partial class MockContext
{
    /// <summary>
    /// Class Recording.
    /// </summary>
    [DataContract]
    private class Recording
    {
        [DataMember(Name = "Checksum", IsRequired = true)]
        private readonly string checksum;

        [DataMember(Name = "Response.Content", IsRequired = true)]
        private readonly string content;

        [DataMember(Name = "Response.Content.Headers", IsRequired = true)]
        private readonly List<KeyValuePair<string, IEnumerable<string>>> contentHeaders;

        [DataMember(Name = "Response.Headers", IsRequired = true)]
        private readonly List<KeyValuePair<string, IEnumerable<string>>> headers;

        [DataMember(Name = "Response.ReasonPhrase", IsRequired = true)]
        private readonly string reasonPhrase;

        [DataMember(Name = "Response.StatusCode", IsRequired = true)]
        private readonly HttpStatusCode statusCode;

        [DataMember(Name = "Response.Version", IsRequired = true)]
        private readonly Version version;

        [DataMember]
        private object gate = new();

        private HttpResponseMessage? response;

        private Recording()
        {
            this.checksum = string.Empty;
            this.content = string.Empty;
            this.contentHeaders = new List<KeyValuePair<string, IEnumerable<string>>>();
            this.headers = new List<KeyValuePair<string, IEnumerable<string>>>();
            this.reasonPhrase = string.Empty;
            this.version = new Version(1, 0);
        }

        private Recording(HttpResponseMessage response, byte[] content, string checksum)
        {
            ArgumentNullException.ThrowIfNull(response);
            ArgumentNullException.ThrowIfNull(content);

            this.checksum = checksum ?? throw new ArgumentNullException(nameof(checksum));
            this.content = Convert.ToBase64String(content);
            this.contentHeaders = response.Content.Headers.ToList();
            this.headers = response.Headers.ToList();
            this.response = response;
            this.reasonPhrase = response.ReasonPhrase ?? string.Empty;
            this.statusCode = response.StatusCode;
            this.version = response.Version;
        }

        /// <summary>
        /// Gets the checksum.
        /// </summary>
        /// <value>The checksum.</value>
        public string Checksum => this.checksum;

        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <value>The response.</value>
        public HttpResponseMessage Response
        {
            get
            {
                if (this.response == null)
                {
                    lock (this.gate)
                    {
                        if (this.response == null)
                        {
                            var buffer = Convert.FromBase64String(this.content);

                            var response = new HttpResponseMessage(this.statusCode)
                            {
                                Content = new StreamContent(new MemoryStream(buffer)),
                                ReasonPhrase = this.reasonPhrase,
                                Version = this.version
                            };

                            foreach (var header in this.headers)
                            {
                                response.Headers.Add(header.Key, header.Value);
                            }

                            foreach (var header in this.contentHeaders)
                            {
                                response.Content.Headers.Add(header.Key, header.Value);
                            }

                            this.response = response;
                        }
                    }
                }

                return this.response;
            }
        }

        /// <summary>
        /// Creates the recording.
        /// </summary>
        /// <param name="checksum">The checksum.</param>
        /// <param name="response">The response.</param>
        /// <returns>Recording.</returns>
        public static async Task<Recording> CreateRecording(string checksum, HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsByteArrayAsync();
            var recording = new Recording(response, content, checksum);

            return recording;
        }
    }
}
