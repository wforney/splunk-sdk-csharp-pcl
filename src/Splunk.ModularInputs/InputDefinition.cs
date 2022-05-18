// <copyright file="InputDefinition.cs" company="Splunk, Inc.">
// Copyright � 2014 Splunk, Inc.
// </copyright>
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

using Splunk.Client;

namespace Splunk.ModularInputs
{
    /// <summary>
    /// Represents the specification of a modular input instance.
    /// </summary>
    public class InputDefinition : IDisposable
    {
        private readonly object gate = new();

        private Service? service;

        /// <summary>
        /// A directory to write state that needs to be shared between executions of this program.
        /// </summary>
        /// <value>The checkpoint directory.</value>
        public string? CheckpointDirectory { get; set; }

        /// <summary>
        /// The name of this instance.
        /// </summary>
        /// <value>The name.</value>
        public string? Name { get; set; }

        /// <summary>
        /// A dictionary of all the parameters and their values for this instance.
        /// </summary>
        /// <value>The parameters.</value>
        public IDictionary<string, Parameter>? Parameters { get; set; }

        /// <summary>
        /// The hostname of the splunkd server that invoked this program.
        /// </summary>
        /// <value>The server host.</value>
        public string? ServerHost { get; set; }

        /// <summary>
        /// The URI to reach the REST API of the splunkd instance that invoked this program.
        /// </summary>
        /// <value>The server URI.</value>
        public string? ServerUri { get; set; }

        /// <summary>
        /// A Service instance connected to the Splunk instance that invoked this program.
        /// </summary>
        /// <value>The service.</value>
        /// <exception cref="InvalidOperationException">Cannot get a Service object without a ServerUri</exception>
        /// <exception cref="FormatException">Invalid server URI</exception>
        /// <exception cref="FormatException"></exception>
        public Service Service
        {
            get
            {
                if (this.ServerUri is null)
                {
                    throw new InvalidOperationException("Cannot get a Service object without a ServerUri");
                }

                if (this.service is null)
                {
                    lock (this.gate)
                    {
                        if (this.service is null)
                        {
                            if (!Uri.TryCreate(this.ServerUri, UriKind.Absolute, out var uri))
                            {
                                throw new FormatException("Invalid server URI");
                            }

                            Client.Scheme scheme;

                            if (uri.Scheme.Equals("https"))
                            {
                                scheme = Splunk.Client.Scheme.Https;
                            }
                            else if (uri.Scheme.Equals("http"))
                            {
                                scheme = Splunk.Client.Scheme.Http;
                            }
                            else
                            {
                                var text = "Invalid URI scheme: " + uri.Scheme + "; expected http or https";
                                throw new FormatException(text);
                            }

                            var service = new Service(scheme, uri.Host, uri.Port)
                            {
                                SessionKey = this.SessionKey
                            };
                            this.service = service;
                        }
                    }
                }

                return this.service;
            }
        }

        /// <summary>
        /// A REST API session key allowing the instance to make REST calls.
        /// </summary>
        /// <value>The session key.</value>
        public string? SessionKey { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this.service != null)
            {
                this.service.Dispose();
            }
        }
    }
}
