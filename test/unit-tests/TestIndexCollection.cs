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

namespace Splunk.Client.UnitTests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Splunk.Client;
using Splunk.Client.Arguments;
using Splunk.Client.Settings;
using Xunit;

public class TestIndexCollection
{
    [Trait("unit-test", "Splunk.Client.Index")]
    [Fact]
    private async Task CanConstructIndex()
    {
        var feed = await TestAtomFeed.ReadFeed(Path.Combine(TestAtomFeed.Directory, "Index.GetAsync.xml"));

        using var context = new Context(Scheme.Https, "localhost", 8089);
        var index = new Client.Index(context, feed);
        CheckCommonProperties("_audit", index);

        var canList = index.Eai.Acl.CanList;
        var app = index.Eai.Acl.App;
        dynamic eai = index.Eai;
        Assert.Equal(app, eai.Acl.App);
        Assert.Equal(canList, eai.Acl.CanList);
    }

    [Trait("unit-test", "Splunk.Client.IndexCollection")]
    [Fact]
    private async Task CanConstructIndexCollection()
    {
        var feed = await TestAtomFeed.ReadFeed(Path.Combine(TestAtomFeed.Directory, "IndexCollection.GetAsync.xml"));

        using var context = new Context(Scheme.Https, "localhost", 8089);
        var expectedNames = new string[]
        {
                "_audit",
                "_blocksignature",
                "_internal",
                "_thefishbucket",
                "history",
                "main",
                "splunklogger",
                "summary"
        };

        var indexes = new ConfigurationCollection(context, feed);

        Assert.Equal(expectedNames, from index in indexes select index.Title);
        Assert.Equal(expectedNames.Length, indexes.Count);
        CheckCommonProperties("indexes", indexes);

        for (var i = 0; i < indexes.Count; i++)
        {
            CheckCommonProperties(expectedNames[i], indexes[i]);
        }
    }

    [Trait("unit-test", "Splunk.Client.IndexCollection.Filter")]
    [Fact]
    private void CanSpecifyFilter()
    {
        // Checked against http://docs.splunk.com/Documentation/Splunk/latest/RESTAPI/RESTindex#GET_data.2Findexes

        var criteria = new IndexCollection.Filter();
        Assert.Equal("count=30; offset=0; search=null; sort_dir=asc; sort_key=name; sort_mode=auto; summarize=0", criteria.ToString());
        Assert.Empty(criteria);

        criteria = new IndexCollection.Filter()
        {
            Count = 100,
            Offset = 100,
            Search = "some_unchecked_string",
            SortDirection = SortDirection.Descending,
            SortKey = "some_unchecked_string",
            SortMode = SortMode.Alphabetic,
            Summarize = true
        };

        Assert.Equal("count=100; offset=100; search=some_unchecked_string; sort_dir=desc; sort_key=some_unchecked_string; sort_mode=alpha; summarize=1", criteria.ToString());

        Assert.Equal(new List<Argument>()
            {
                new Argument("count", 100),
                new Argument("offset", 100),
                new Argument("search", "some_unchecked_string"),
                new Argument("sort_dir", "desc"),
                new Argument("sort_key", "some_unchecked_string"),
                new Argument("sort_mode", "alpha"),
                new Argument("summarize", 1)
            },
            criteria.AsEnumerable());
    }

    private static void CheckCommonProperties<TResource>(string expectedName, BaseEntity<TResource> entity) where TResource : BaseResource, new()
    {
        Assert.Equal(expectedName, entity.Title);

        //// Properties common to all resources

        var value = entity.GeneratorVersion;
        Assert.NotNull(value);

        var value2 = entity.Id;
        Assert.NotNull(value2);

        var value3 = entity.Title;
        Assert.NotNull(value3);

        var value4 = entity.Updated;
        Assert.NotEqual(DateTime.MinValue, value4);
    }
}
