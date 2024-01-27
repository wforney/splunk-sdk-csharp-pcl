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

using Splunk.Client.Arguments;
using Xunit;

namespace Splunk.Client.UnitTests;

public class TestSavedSearchCollection
{
    [Trait("unit-test", "SavedSearchCollection.Filter")]
    [Fact]
    private void CanSpecifyFilter()
    {
        var expectedString = new string[] {
            "count=30; earliest_time=null; latest_time=null; listDefaultActionArgs=f; offset=0; search=null; sort_dir=asc; sort_key=name; sort_mode=auto",
            "count=30; earliest_time=null; latest_time=null; listDefaultActionArgs=f; offset=0; search=some_unchecked_string; sort_dir=asc; sort_key=name; sort_mode=auto",
        };
        var expectedArguments = new List<Argument>[]
        {
            new()
            {
            },
            new()
            {
                new Argument("search", "some_unchecked_string")
            }
        };

        SavedSearchCollection.Filter args;

        args = new SavedSearchCollection.Filter();
        Assert.Equal(expectedString[0], args.ToString());
        Assert.Equal(expectedArguments[0], args);
    }

    [Trait("class", "Splunk.Client.Args")]
    [Fact]
    private void CanSetEveryValue()
    {
        var args = new SavedSearchCollection.Filter()
        {
            Count = 100,
            EarliestTime = "some_unchecked_string",
            LatestTime = "some_unchecked_string",
            ListDefaultActions = true,
            Offset = 100,
            Search = "some_unchecked_string",
            SortDirection = SortDirection.Descending,
            SortKey = "some_unchecked_string",
            SortMode = SortMode.Alphabetic
        };

        Assert.Equal("count=100; earliest_time=some_unchecked_string; latest_time=some_unchecked_string; listDefaultActionArgs=t; offset=100; search=some_unchecked_string; sort_dir=desc; sort_key=some_unchecked_string; sort_mode=alpha", args.ToString());

        var list = new List<Argument>()
        {
            new("count", "100"),
            new("earliest_time", "some_unchecked_string"),
            new("latest_time", "some_unchecked_string"),
            new("listDefaultActionArgs", "t"),
            new("offset", "100"),
            new("search", "some_unchecked_string"),
            new("sort_dir", "desc"),
            new("sort_key", "some_unchecked_string"),
            new("sort_mode", "alpha")
        };

        Assert.Equal(list, args);
    }
}
