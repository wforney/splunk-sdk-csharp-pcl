// <copyright file="SdkHelper.cs" company="Splunk, Inc.">
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
using Xunit;

namespace Splunk.Client.Helper
{
    /// <summary>
    /// Class SdkHelper.
    /// </summary>
    public static class SdkHelper
    {
        /// <summary>
        /// Initializes the <see cref="SdkHelper" /> class.
        /// </summary>
        static SdkHelper()
        {
            //// TODO: Use WebRequestHandler.ServerCertificateValidationCallback instead
            //// 1. Instantiate a WebRequestHandler
            //// 2. Set its ServerCertificateValidationCallback
            //// 3. Instantiate a Splunk.Client.Context with the WebRequestHandler

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            Splunk = new SplunkRC(Path.Combine(home, ".splunkrc"));
        }

        /// <summary>
        /// Gets the user configure.
        /// </summary>
        /// <value>The user configure.</value>
        public static SplunkRC Splunk { get; private set; }

        /// <summary>
        /// Create a Splunk <see typeref="Service" /> and login using the settings provided in .splunkrc.
        /// </summary>
        /// <param name="ns">The ns.</param>
        /// <param name="login">if set to <c>true</c> [login].</param>
        /// <returns>The service created.</returns>
        public static async Task<Service> CreateService(Namespace? ns = null, bool login = true)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            var context = new MockContext(Splunk.Scheme, Splunk.Host, Splunk.Port);
            var service = new Service(context, ns);
            if (login)
            {
                await service.LogOnAsync(Splunk.Username, Splunk.Password);
            }

            return service;
        }

        /// <summary>
        /// Throws as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="testCode">The test code.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public static async Task ThrowsAsync<T>(Func<Task> testCode)
        where T : Exception
        {
            Assert.NotNull(testCode);

            try
            {
                await testCode();
                return;
            }
            catch (T)
            {
                return;
            }
            catch (Exception unexpectedException)
            {
                Assert.Fail(string.Format("Expected {0}; found exception {1}: {2}", typeof(T).FullName, unexpectedException.GetType().FullName, unexpectedException.Message));
            }

            Assert.Fail(string.Format("Expected exception {0}, but not exception raised.", typeof(T)));
        }

        /// <summary>
        /// Class SplunkRC. This class cannot be inherited.
        /// </summary>
        public sealed class SplunkRC
        {
            /// <summary>
            /// The host
            /// </summary>
            public string Host = "localhost";

            /// <summary>
            /// The password
            /// </summary>
            public string Password = "changeme";

            /// <summary>
            /// The port
            /// </summary>
            public int Port = 8089;

            /// <summary>
            /// The scheme
            /// </summary>
            public Scheme Scheme = Scheme.Https;

            /// <summary>
            /// The username
            /// </summary>
            public string Username = "admin";

            /// <summary>
            /// Initializes a new instance of the <see cref="SplunkRC" /> class.
            /// </summary>
            /// <param name="path">The location of a .splunkrc file.</param>
            internal SplunkRC(string path)
            {
                StreamReader? reader;
                try
                {
                    reader = new StreamReader(path);
                }
                catch (FileNotFoundException)
                {
                    return;
                }

                var argList = new List<string>(4);
                string line;
                while ((line = reader.ReadLine()) is not null)
                {
                    line = line.Trim();

                    if (line.StartsWith("#", StringComparison.InvariantCulture))
                    {
                        continue;
                    }

                    if (line.Length == 0)
                    {
                        continue;
                    }

                    argList.Add(line);
                }

                foreach (var arg in argList)
                {
                    var pair = arg.Split('=');

                    switch (pair[0].ToLower().Trim())
                    {
                        case "scheme":
                            this.Scheme = pair[1].Trim() == "https" ? Scheme.Https : Scheme.Http;
                            break;

                        case "host":
                            this.Host = pair[1].Trim();
                            break;

                        case "port":
                            this.Port = int.Parse(pair[1].Trim());
                            break;

                        case "username":
                            this.Username = pair[1].Trim();
                            break;

                        case "password":
                            this.Password = pair[1].Trim();
                            break;
                    }
                }
            }
        }
    }
}
