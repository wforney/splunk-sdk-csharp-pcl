﻿/*
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

using Splunk.Client.Settings;
using Xunit;

namespace Splunk.Client.UnitTests;

public class TestServerMessageCollection
{
    [Trait("unit-test", "Splunk.Client.ServerMessage")]
    [Fact]
    public async Task CanConstructServerMessage()
    {
        var feed = await TestAtomFeed.ReadFeed(Path.Combine(TestAtomFeed.Directory, "ServerMessageCollection.CreateAsync.xml"));

        using var context = new Context(Scheme.Https, "localhost", 8089);
        var message = new ServerMessage(context, feed);

        CheckCommonProperties("some_message_name", message);

        Assert.Equal("some_message_text", message.Text);
        Assert.Equal(ServerMessageSeverity.Information, message.Severity);
        Assert.Equal("2014-05-27 15:28:02Z", message.TimeCreated.ToString("u"));

        bool canList = message.Eai.Acl.CanList;
        string app = message.Eai.Acl.App;
        dynamic eai = message.Eai;
        Assert.Equal(app, eai.Acl.App);
        Assert.Equal(canList, eai.Acl.CanList);
    }

    [Trait("unit-test", "Splunk.Client.ServerMessageCollection")]
    [Fact]
    public async Task CanConstructServerMessageCollection()
    {
        var feed = await TestAtomFeed.ReadFeed(Path.Combine(TestAtomFeed.Directory, "ServerMessageCollection.GetAsync.xml"));

        using var context = new Context(Scheme.Https, "localhost", 8089);
        var expectedNames = new string[]
        {
                "some_message_name",
                "some_other_message_name",
        };

        var messages = new ConfigurationCollection(context, feed);

        Assert.Equal(expectedNames, from message in messages select message.Title);
        Assert.Equal(expectedNames.Length, messages.Count);
        CheckCommonProperties("messages", messages);

        for (int i = 0; i < messages.Count; i++)
        {
            CheckCommonProperties(expectedNames[i], messages[i]);
        }
    }

    public void CheckCommonProperties<TResource>(string expectedName, BaseEntity<TResource> entity) where TResource : BaseResource, new()
    {
        Assert.Equal(expectedName, entity.Title);

        //// Properties common to all resources

        Version value = entity.GeneratorVersion;
        Assert.NotNull(value);

        Uri value2 = entity.Id;
        Assert.NotNull(value2);

        string value3 = entity.Title;
        Assert.NotNull(value3);

        DateTime value4 = entity.Updated;
        Assert.NotEqual(DateTime.MinValue, value4);
    }
}
