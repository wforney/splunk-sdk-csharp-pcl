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

using System.Net;
using Xunit;

namespace Splunk.Client.UnitTests;

public class TestSearchPreviewStream
{
    [Trait("unit-test", "Splunk.Client.SearchPreviewStream")]
    [Fact]
    public async Task CanHandleInFlightErrorsReportedBySplunk()
    {
        var path = Path.Combine(TestAtomFeed.Directory, "Service.ExportSearchPreviews-failure.xml");
        var message = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new FileStream(path, FileMode.Open, FileAccess.Read))
        };
        SearchPreviewStream? stream = null;

        try
        {
            stream = await SearchPreviewStream.CreateAsync(message);
            int count = 0;

            foreach (var preview in stream)
            {
                ++count;
            }

            Assert.Fail("Expected RequestException");
        }
        catch (RequestException e)
        {
            Assert.Equal("Fatal: JournalSliceDirectory: Cannot seek to 0", e.Message);
        }
        finally
        {
            stream?.Dispose();
        }
    }

    [Trait("unit-test", "Splunk.Client.SearchPreviewStream")]
    [Fact]
    public async Task CanSkipEmptyPreviews()
    {
        var baseFileName = Path.Combine(TestAtomFeed.Directory, "DVPL-5873");
        var message = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new FileStream($"{baseFileName}.xml", FileMode.Open, FileAccess.Read))
        };

        using var stream = await SearchPreviewStream.CreateAsync(message);
        int count = 0;

        foreach (var observedResult in stream)
        {
            ++count;
        }

        Assert.Equal(count, stream.ReadCount);
    }
}
