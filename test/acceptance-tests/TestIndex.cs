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

namespace Splunk.Client.AcceptanceTests;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Splunk.Client;
using Splunk.Client.Helper;
using Xunit;

/// <summary>
/// Tests the Index class
/// </summary>
public class IndexTest
{
    /// <summary>
    /// Tests the basic getters and setters of index
    /// </summary>
    [Trait("acceptance-test", "Splunk.Client.Index")]
    [MockContext]
    [Fact]
    public async Task IndexCollection()
    {
        var indexName = string.Format("delete-me-{0}", Guid.NewGuid());

        using var service = await SdkHelper.CreateService();
        var indexes = service.Indexes;

        var testIndex = await indexes.CreateAsync(indexName);
        //// TODO: Verify testIndex

        await testIndex.GetAsync();
        //// TODO: Reverify testIndex

        await indexes.GetAllAsync();
        Assert.NotNull(indexes.SingleOrDefault(x => x.Title == testIndex.Title));

        foreach (var index in indexes)
        {
            int dummyInt;
            string dummyString;
            bool dummyBool;
            DateTime dummyTime;
            dummyBool = index.AssureUTF8;
            dummyInt = index.BloomFilterTotalSizeKB;
            dummyString = index.ColdPath;
            dummyString = index.ColdPathExpanded;
            dummyString = index.ColdToFrozenDir;
            dummyString = index.ColdToFrozenScript;
            dummyBool = index.CompressRawData;
            var size = index.CurrentDBSizeMB;
            dummyString = index.DefaultDatabase;
            dummyBool = index.EnableRealTimeSearch;
            dummyInt = index.FrozenTimePeriodInSecs;
            dummyString = index.HomePath;
            dummyString = index.HomePathExpanded;
            dummyString = index.IndexThreads;
            var time = index.LastInitTime;
            dummyString = index.MaxBloomBackfillBucketAge;
            dummyInt = index.MaxConcurrentOptimizes;
            dummyString = index.MaxDataSize;
            dummyInt = index.MaxHotBuckets;
            dummyInt = index.MaxHotIdleSecs;
            dummyInt = index.MaxHotSpanSecs;
            dummyInt = index.MaxMemMB;
            dummyInt = index.MaxMetaEntries;
            dummyInt = index.MaxRunningProcessGroups;
            dummyTime = index.MaxTime;
            dummyInt = index.MaxTotalDataSizeMB;
            dummyInt = index.MaxWarmDBCount;
            dummyString = index.MemPoolMB;
            dummyString = index.MinRawFileSyncSecs;
            dummyTime = index.MinTime;
            dummyInt = index.NumBloomFilters;
            dummyInt = index.NumHotBuckets;
            dummyInt = index.NumWarmBuckets;
            dummyInt = index.PartialServiceMetaPeriod;
            dummyInt = index.QuarantineFutureSecs;
            dummyInt = index.QuarantinePastSecs;
            dummyInt = index.RawChunkSizeBytes;
            dummyInt = index.RotatePeriodInSecs;
            dummyInt = index.ServiceMetaPeriod;
            dummyString = index.SuppressBannerList;
            var sync = index.Sync;
            dummyBool = index.SyncMeta;
            dummyString = index.ThawedPath;
            dummyString = index.ThawedPathExpanded;
            dummyInt = index.ThrottleCheckPeriod;
            var eventCount = index.TotalEventCount;
            dummyBool = index.Disabled;
            dummyBool = index.IsInternal;
        }

        for (var i = 0; i < indexes.Count; i++)
        {
            var index = indexes[i];

            int dummyInt;
            string dummyString;
            bool dummyBool;
            DateTime dummyTime;
            dummyBool = index.AssureUTF8;
            dummyInt = index.BloomFilterTotalSizeKB;
            dummyString = index.ColdPath;
            dummyString = index.ColdPathExpanded;
            dummyString = index.ColdToFrozenDir;
            dummyString = index.ColdToFrozenScript;
            dummyBool = index.CompressRawData;
            var size = index.CurrentDBSizeMB;
            dummyString = index.DefaultDatabase;
            dummyBool = index.EnableRealTimeSearch;
            dummyInt = index.FrozenTimePeriodInSecs;
            dummyString = index.HomePath;
            dummyString = index.HomePathExpanded;
            dummyString = index.IndexThreads;
            var time = index.LastInitTime;
            dummyString = index.MaxBloomBackfillBucketAge;
            dummyInt = index.MaxConcurrentOptimizes;
            dummyString = index.MaxDataSize;
            dummyInt = index.MaxHotBuckets;
            dummyInt = index.MaxHotIdleSecs;
            dummyInt = index.MaxHotSpanSecs;
            dummyInt = index.MaxMemMB;
            dummyInt = index.MaxMetaEntries;
            dummyInt = index.MaxRunningProcessGroups;
            dummyTime = index.MaxTime;
            dummyInt = index.MaxTotalDataSizeMB;
            dummyInt = index.MaxWarmDBCount;
            dummyString = index.MemPoolMB;
            dummyString = index.MinRawFileSyncSecs;
            dummyTime = index.MinTime;
            dummyInt = index.NumBloomFilters;
            dummyInt = index.NumHotBuckets;
            dummyInt = index.NumWarmBuckets;
            dummyInt = index.PartialServiceMetaPeriod;
            dummyInt = index.QuarantineFutureSecs;
            dummyInt = index.QuarantinePastSecs;
            dummyInt = index.RawChunkSizeBytes;
            dummyInt = index.RotatePeriodInSecs;
            dummyInt = index.ServiceMetaPeriod;
            dummyString = index.SuppressBannerList;
            var sync = index.Sync;
            dummyBool = index.SyncMeta;
            dummyString = index.ThawedPath;
            dummyString = index.ThawedPathExpanded;
            dummyInt = index.ThrottleCheckPeriod;
            var eventCount = index.TotalEventCount;
            dummyBool = index.Disabled;
            dummyBool = index.IsInternal;
        }

        var attributes = GetIndexAttributes(testIndex);

        attributes.EnableOnlineBucketRepair = !testIndex.EnableOnlineBucketRepair;
        attributes.MaxBloomBackfillBucketAge = "20d";
        attributes.FrozenTimePeriodInSecs = testIndex.FrozenTimePeriodInSecs + 1;
        attributes.MaxConcurrentOptimizes = testIndex.MaxConcurrentOptimizes + 1;
        attributes.MaxDataSize = "auto";
        attributes.MaxHotBuckets = testIndex.MaxHotBuckets + 1;
        attributes.MaxHotIdleSecs = testIndex.MaxHotIdleSecs + 1;
        attributes.MaxMemMB = testIndex.MaxMemMB + 1;
        attributes.MaxMetaEntries = testIndex.MaxMetaEntries + 1;
        attributes.MaxTotalDataSizeMB = testIndex.MaxTotalDataSizeMB + 1;
        attributes.MaxWarmDBCount = testIndex.MaxWarmDBCount + 1;
        attributes.MinRawFileSyncSecs = "disable";
        attributes.PartialServiceMetaPeriod = testIndex.PartialServiceMetaPeriod + 1;
        attributes.QuarantineFutureSecs = testIndex.QuarantineFutureSecs + 1;
        attributes.QuarantinePastSecs = testIndex.QuarantinePastSecs + 1;
        attributes.RawChunkSizeBytes = testIndex.RawChunkSizeBytes + 1;
        attributes.RotatePeriodInSecs = testIndex.RotatePeriodInSecs + 1;
        attributes.ServiceMetaPeriod = testIndex.ServiceMetaPeriod + 1;
        attributes.SyncMeta = !testIndex.SyncMeta;
        attributes.ThrottleCheckPeriod = testIndex.ThrottleCheckPeriod + 1;

        var updatedSnapshot = await testIndex.UpdateAsync(attributes);
        Assert.True(updatedSnapshot);

        await testIndex.DisableAsync();
        Assert.True(testIndex.Disabled); // Because the disable endpoint returns an updated snapshot

        await service.Server.RestartAsync(2 * 60 * 1000); // Because you can't re-enable an index without a restart
        await service.LogOnAsync();

        testIndex = await service.Indexes.GetAsync(indexName);
        await testIndex.EnableAsync(); // Because the enable endpoint returns an updated snapshot
        Assert.False(testIndex.Disabled);

        await service.Server.RestartAsync(2 * 60 * 1000);
        await service.LogOnAsync();

        await testIndex.RemoveAsync();

        await Task.Delay(5000);

        await SdkHelper.ThrowsAsync<ResourceNotFoundException>(async () => await testIndex.GetAsync());

        testIndex = await indexes.GetOrNullAsync(indexName);
        Assert.Null(testIndex);
    }

    /// <summary>
    /// Tests submitting and streaming events to an index given the indexAttributes argument
    /// and also removing all events from the index
    /// </summary>
    [Trait("acceptance-test", "Splunk.Client.Transmitter")]
    [MockContext]
    [Fact]
    public async Task Transmitter1()
    {
        var indexName = string.Format("delete-me-{0}", Guid.NewGuid());

        using var service = await SdkHelper.CreateService();
        var index = await service.Indexes.RecreateAsync(indexName);

        Assert.NotNull(index);
        Assert.False(index!.Disabled);

        await Task.Delay(2000);

        // Submit event using TransmitterArgs

        const string Source = "splunk-sdk-tests";
        const string SourceType = "splunk-sdk-test-event";
        const string Host = "test-host";

        var transmitterArgs = new TransmitterArgs
        {
            Host = Host,
            Source = Source,
            SourceType = SourceType,
        };

        var transmitter = service.Transmitter;
        SearchResult result;

        //// TODO: Check contentss
        var eventText = MockContext.GetOrElse(string.Format("1, {0}, {1}, simple event", DateTime.Now, indexName));
        Assert.NotNull(eventText);
        result = await transmitter.SendAsync(eventText!, indexName, transmitterArgs);
        Assert.NotNull(result);

        eventText = MockContext.GetOrElse(string.Format("2, {0}, {1}, simple event", DateTime.Now, indexName));
        Assert.NotNull(eventText);
        result = await transmitter.SendAsync(eventText!, indexName, transmitterArgs);
        Assert.NotNull(result);

        using (var stream = new MemoryStream())
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 4096, leaveOpen: true))
            {
                writer.WriteLine(
                    MockContext.GetOrElse(string.Format("1, {0}, {1}, stream event", DateTime.Now, indexName)));
                writer.WriteLine(
                    MockContext.GetOrElse(string.Format("2, {0}, {1}, stream event", DateTime.Now, indexName)));
            }

            _ = stream.Seek(0, SeekOrigin.Begin);

            await transmitter.SendAsync(stream, indexName, transmitterArgs);
        }

        await index.PollForUpdatedEventCount(4);

        var search = string.Format(
            "search index={0} host={1} source={2} sourcetype={3}",
            indexName,
            Host,
            Source,
            SourceType);

        using (var stream = await service.SearchOneShotAsync(search))
        {
            Assert.Empty(stream.FieldNames);
            Assert.False(stream.IsFinal);
            Assert.Equal(0, stream.ReadCount);

            foreach (var record in stream)
            {
                var fieldNames = stream.FieldNames;

                Assert.Equal(14, fieldNames.Count);
                Assert.Equal(14, record.FieldNames.Count);
                Assert.Equal(fieldNames.AsEnumerable(), record.FieldNames.AsEnumerable());

                var memberNames = record.GetDynamicMemberNames();
                var intersection = fieldNames.Intersect(memberNames);

                Assert.Equal(memberNames, intersection);
            }

            Assert.Equal(14, stream.FieldNames.Count);
            Assert.Equal(4, stream.ReadCount);
        }
    }

    /// <summary>
    /// Test submitting and streaming to a default index given the indexAttributes argument
    /// and also removing all events from the index
    /// </summary>
    [Trait("acceptance-test", "Splunk.Client.Transmitter")]
    [MockContext]
    [Fact]
    public async Task Transmitter2()
    {
        var indexName = "main";

        using var service = await SdkHelper.CreateService();
        var index = await service.Indexes.GetAsync(indexName);
        var currentEventCount = index.TotalEventCount;
        Assert.NotNull(index);

        var transmitter = (Transmitter)service.Transmitter;
        var indexAttributes = GetIndexAttributes(index);
        Assert.NotNull(indexAttributes);

        // Submit event to default index using variable arguments
        var eventText = MockContext.GetOrElse(string.Format("{0}, DefaultIndexArgs string event Hello World 1", DateTime.Now));
        Assert.NotNull(eventText);
        _ = await transmitter.SendAsync(eventText!, indexName);
        eventText = MockContext.GetOrElse(string.Format("{0}, DefaultIndexArgs string event Hello World 2", DateTime.Now));
        Assert.NotNull(eventText);
        _ = await transmitter.SendAsync(eventText!, indexName);

        await index.PollForUpdatedEventCount(currentEventCount + 2);
        currentEventCount += 2;

        using (var stream = new MemoryStream())
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 4096, leaveOpen: true))
            {
                writer.WriteLine(
                    MockContext.GetOrElse(string.Format("{0}, DefaultIndexArgs stream events 1", DateTime.Now)));
                writer.WriteLine(
                    MockContext.GetOrElse(string.Format("{0}, DefaultIndexArgs stream events 2", DateTime.Now)));
            }

            _ = stream.Seek(0, SeekOrigin.Begin);
            await transmitter.SendAsync(stream, indexName);
        }

        await index.PollForUpdatedEventCount(currentEventCount + 2);
        currentEventCount += 2;
    }

    /// <summary>
    /// Gets old values from given index, skip saving paths and things we cannot write
    /// </summary>
    /// <param name="index">The Index</param>
    /// <returns>The argument getIndexProperties</returns>
    private static IndexAttributes GetIndexAttributes(Client.Index index)
    {
        var indexAttributes = new IndexAttributes
        {
            FrozenTimePeriodInSecs = index.FrozenTimePeriodInSecs,
            MaxConcurrentOptimizes = index.MaxConcurrentOptimizes,
            MaxDataSize = index.MaxDataSize,
            MaxHotBuckets = index.MaxHotBuckets,
            MaxHotIdleSecs = index.MaxHotIdleSecs,
            MaxHotSpanSecs = index.MaxHotSpanSecs,
            MaxMemMB = index.MaxMemMB,
            MaxMetaEntries = index.MaxMetaEntries,
            MaxTotalDataSizeMB = index.MaxTotalDataSizeMB,
            MaxWarmDBCount = index.MaxWarmDBCount,
            MinRawFileSyncSecs = index.MinRawFileSyncSecs,
            PartialServiceMetaPeriod = index.PartialServiceMetaPeriod,
            QuarantineFutureSecs = index.QuarantineFutureSecs,
            QuarantinePastSecs = index.QuarantinePastSecs,
            RawChunkSizeBytes = index.RawChunkSizeBytes,
            RotatePeriodInSecs = index.RotatePeriodInSecs,
            ServiceMetaPeriod = index.ServiceMetaPeriod,
            SyncMeta = index.SyncMeta,
            ThrottleCheckPeriod = index.ThrottleCheckPeriod
        };

        return indexAttributes;
    }
}
