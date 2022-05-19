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

namespace Splunk.Client.UnitTests;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Splunk.Client;
using Xunit;

public class TestApplicationCollection
{
    [Trait("unit-test", "Splunk.Client.Application")]
    [Fact]
    private async Task CanConstructApplication()
    {
        var feed = await TestAtomFeed.ReadFeed(Path.Combine(TestAtomFeed.Directory, "Application.GetAsync.xml"));

        using var context = new Context(Scheme.Https, "localhost", 8089);
        var exception = Record.Exception(() => new Application(context, feed));
        Assert.Null(exception);
    }

    [Trait("unit-test", "Splunk.Client.ApplicationCollection")]
    [Fact]
    private async Task CanConstructApplicationCollection()
    {
        var feed = await TestAtomFeed.ReadFeed(Path.Combine(TestAtomFeed.Directory, "ApplicationCollection.GetAsync.xml"));

        using var context = new Context(Scheme.Https, "localhost", 8089);
        var expectedApplicationNames = new string[]
        {
                "delete-me-cc314e4eda254dec885f9465e55729e5",
                "delete-me-db1bc2678174495a9f91d0c2360f53bc",
                "delete-me-decd5a97569440768914cd0629c5501d",
                "delete-me-e341f711346a4898b15accd7f77c5c92",
                "framework",
                "gettingstarted",
                "introspection_generator_addon",
                "launcher",
                "learned",
                "legacy",
                "sample_app",
                "search",
                "splunk_datapreview",
                "SplunkForwarder",
                "SplunkLightForwarder"
        };

        var applications = new ApplicationCollection(context, feed);

        Assert.Equal(expectedApplicationNames, from application in applications select application.Title);
        Assert.Equal(expectedApplicationNames.Length, applications.Count);

        for (var i = 0; i < applications.Count; i++)
        {
            Assert.Equal(expectedApplicationNames[i], applications[i].Title);

            var value = applications[i].GeneratorVersion;
            Assert.NotNull(value);

            var value2 = applications[i].Id;
            Assert.NotNull(value2);

            var value3 = applications[i].Title;
            Assert.NotNull(value3);

            var value4 = applications[i].Updated;
            Assert.NotEqual(DateTime.MinValue, value4);
        }
    }
}
