// <copyright file="MockContext.Player.cs" company="Splunk, Inc.">
// Copyright 2014 Splunk, Inc.
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

namespace Splunk.Client.Helper;

/// <summary>
/// Class MockContext.
/// Implements the <see cref="Context" />
/// </summary>
/// <seealso cref="Context" />
public partial class MockContext
{
    /// <summary>
    /// Class Player.
    /// Implements the <see cref="Splunk.Client.Helper.MockContext.MessageHandler" />
    /// </summary>
    /// <seealso cref="Splunk.Client.Helper.MockContext.MessageHandler" />
    private class Player : MessageHandler
    {
        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var recording = session.Recordings.Dequeue();

            using var stream = new MemoryStream();
            var result = await ComputeChecksum(request, stream);
            var checksum = result.Item1;

            if (checksum != recording.Checksum)
            {
                var text = string.Format(
                    "The recording in {0} is out of sync with {1}.",
                    RecordingDirectoryName,
                    CallerId);
                throw new InvalidOperationException(text);
            }

            var response = recording.Response;
            response.RequestMessage = request;

            return response;
        }
    }
}
