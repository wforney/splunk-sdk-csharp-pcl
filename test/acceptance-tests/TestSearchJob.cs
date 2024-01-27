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

namespace Splunk.Client.AcceptanceTests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualBasic.FileIO;
using Splunk.Client;
using Splunk.Client.Exceptions;
using Splunk.Client.Helper;
using Xunit;

/// <summary>
/// This is the search test class
/// </summary>
public class SearchTest
{
    /// <summary>
    /// Search query which will give 'sg' tags
    /// in output when "segmentation == raw".
    /// </summary>
    private const string Search = "search index=_internal GET | head 3";

    /// <summary>
    /// Tests the result from a bad search argument.
    /// </summary>
    [Trait("acceptance-test", "Splunk.Client.Job")]
    [MockContext]
    [Fact]
    public async Task BadOutputMode()
    {
        using var service = await SdkHelper.CreateService();
        var search = "invalidpart" + Search;

        try
        {
            _ = await service.Jobs.CreateAsync(search);
        }
        catch (BadRequestException)
        {
            return;
        }
        catch (Exception e)
        {
            Assert.Fail(string.Format("Unexpected exception: {0}\n{1}", e.Message, e.StackTrace));
        }

        Assert.Fail(string.Format("Expected BadRequestException but no exception was thrown."));
    }

    /// <summary>
    /// Tests the result from a search argument.
    /// </summary>
    [Trait("acceptance-test", "Splunk.Client.Job")]
    [MockContext]
    [Fact]
    public async Task JobSearchMode()
    {
        using var service = await SdkHelper.CreateService();
        var jobArgs = new JobArgs
        {
            SearchMode = SearchMode.Normal
        };
        var job = await service.Jobs.CreateAsync(Search, args: jobArgs);
        Assert.NotNull(job);

        jobArgs.SearchMode = SearchMode.RealTime;
        var updatedSnapshot = await job.UpdateAsync(jobArgs);
        Assert.True(updatedSnapshot);

        await job.CancelAsync();
    }

    /// <summary>
    /// Tests the result from a search argument.
    /// </summary>
    [Trait("acceptance-test", "Splunk.Client.Job")]
    [MockContext]
    [Fact]
    public async Task JobExecutionMode()
    {
        using var service = await SdkHelper.CreateService();
        Job job;

        job = await service.Jobs.CreateAsync(Search, mode: ExecutionMode.Normal);
        Assert.NotNull(job);

        job = await service.Jobs.CreateAsync(Search, mode: ExecutionMode.OneShot);
        Assert.NotNull(job);

        job = await service.Jobs.CreateAsync(Search, mode: ExecutionMode.Blocking);
        Assert.NotNull(job);

        await job.CancelAsync();
    }

    /// <summary>
    /// Tests all output modes for Job.Events
    /// </summary>
    [Trait("acceptance-test", "Splunk.Client.Job")]
    [MockContext]
    [Fact]
    public async Task JobEventsTruncationModeArgument()
    {
        using var service = await SdkHelper.CreateService();
        var jobArgs = new JobArgs();

        await ForEachEnum(typeof(TruncationMode), async enumValue =>
            {
                var job = await service.Jobs.CreateAsync(Search, args: jobArgs);

                var args = new SearchEventArgs
                {
                    TruncationMode = (TruncationMode)Enum.Parse(typeof(TruncationMode), enumValue)
                };

                using (var stream = await job.GetSearchEventsAsync(args))
                { }

                await job.CancelAsync();
            });
    }

    /// <summary>
    /// Test that Job.SearchEventsAsync() defaults to getting all search events
    /// </summary>
    [Trait("acceptance_test", "Splunk.Client.Job")]
    [MockContext]
    [Fact]
    public async Task JobEventsDefaultsToAll()
    {
        using var service = await SdkHelper.CreateService();
        var jobArgs = new JobArgs();

        var job = await service.Jobs.CreateAsync("search index=_* | head 101", args: jobArgs);

        using (var stream = await job.GetSearchEventsAsync())
        {
            // Is the event count greater than the default of 100?
            Assert.Equal(101, job.EventCount);
        }

        await job.CancelAsync();
    }

    /// <summary>
    /// Test that Job.GetSearchPreviewAsync() defaults to getting all search events
    /// </summary>
    [Trait("acceptance_test", "Splunk.Client.Job")]
    [MockContext]
    [Fact]
    public async Task JobPreviewDefaultsToAll()
    {
        using var service = await SdkHelper.CreateService();
        var jobArgs = new JobArgs();

        var job = await service.Jobs.CreateAsync("search index=_* | head 101", args: jobArgs);

        for (var delay = 1000; delay < 5000; delay += 1000)
        {
            try
            {
                await job.TransitionAsync(DispatchState.Done, delay);
                break;
            }
            catch (TaskCanceledException)
            { }
        }

        using (var stream = await job.GetSearchPreviewAsync())
        {
            // Is the result preview count greater than the default of 100?
            Assert.Equal(101, job.ResultPreviewCount);
        }

        await job.CancelAsync();
    }

    /// <summary>
    /// Test that Job.GetSearchResponseMessageAsync() defaults to getting all search events
    /// </summary>
    [Trait("acceptance_test", "Splunk.Client.Job")]
    [MockContext]
    [Fact]
    public async Task JobResponseMessageDefaultsToAll()
    {
        using var service = await SdkHelper.CreateService();
        foreach (OutputMode outputMode in Enum.GetValues(typeof(OutputMode)))
        {
            foreach (ExecutionMode executionMode in Enum.GetValues(typeof(ExecutionMode)))
            {
                var job = await service.Jobs.CreateAsync("search index=_* | head 101", mode: executionMode);

                using (var message = await job.GetSearchResponseMessageAsync(outputMode: outputMode))
                {
                    // Is the result preview count greater than the default of 100?
                    Assert.Equal(101, job.EventAvailableCount);
                    Assert.Equal(101, job.EventCount);
                    Assert.Equal(101, job.ResultPreviewCount);
                    Assert.Equal(101, job.ResultCount);

                    var streamReader = new StreamReader(await message.Content.ReadAsStreamAsync());

                    switch (outputMode)
                    {
                        case OutputMode.Default:
                            Assert.Equal("?count=0", message.RequestMessage?.RequestUri?.Query);
                            {
                                var result = XDocument.Load(streamReader);
                            }

                            break;
                        case OutputMode.Atom:
                            Assert.Equal("?count=0&output_mode=atom", message.RequestMessage?.RequestUri?.Query);
                            {
                                var result = XDocument.Load(streamReader);
                            }

                            break;
                        case OutputMode.Csv:
                            Assert.Equal("?count=0&output_mode=csv", message.RequestMessage?.RequestUri?.Query);
                            using (var parser = new TextFieldParser(streamReader))
                            {
                                parser.Delimiters = new string[] { "," };
                                parser.HasFieldsEnclosedInQuotes = true;
                                var fields = parser.ReadFields();
                                var values = new List<string[]>();
                                while (!parser.EndOfData)
                                {
                                    var readFields = parser.ReadFields();
                                    if (readFields is not null)
                                    {
                                        values.Add(fields!);
                                    }
                                }

                                Assert.Equal(101, values.Count);
                            }

                            break;
                        case OutputMode.Json:
                            Assert.Equal("?count=0&output_mode=json", message.RequestMessage?.RequestUri?.Query);
                            var content = await streamReader.ReadToEndAsync();
                            _ = JsonSerializer.Deserialize<object?>(content);

                            break;
                        case OutputMode.JsonColumns:
                            Assert.Equal("?count=0&output_mode=json_cols", message.RequestMessage?.RequestUri?.Query);
                            var jsonColumns = await streamReader.ReadToEndAsync();
                            _ = JsonSerializer.Deserialize<object?>(jsonColumns);

                            break;
                        case OutputMode.JsonRows:
                            Assert.Equal("?count=0&output_mode=json_rows", message.RequestMessage?.RequestUri?.Query);
                            var jsonRows = await streamReader.ReadToEndAsync();
                            _ = JsonSerializer.Deserialize<object?>(jsonRows);

                            break;
                        case OutputMode.Raw:
                            Assert.Equal("?count=0&output_mode=raw", message.RequestMessage?.RequestUri?.Query);
                            {
                                var rawResult = await streamReader.ReadToEndAsync();
                                _ = rawResult;
                            }

                            break;
                        case OutputMode.Xml:
                            Assert.Equal("?count=0&output_mode=xml", message.RequestMessage?.RequestUri?.Query);
                            {
                                _ = XDocument.Load(streamReader);
                            }

                            break;
                    }
                }

                await job.CancelAsync();
            }
        }
    }

    /// <summary>
    /// Test that Job.SearchResultsAsync() defaults to getting all search events
    /// </summary>
    [Trait("acceptance_test", "Splunk.Client.Job")]
    [MockContext]
    [Fact]
    public async Task JobResultsDefaultsToAll()
    {
        using var service = await SdkHelper.CreateService();
        var jobArgs = new JobArgs();

        var job = await service.Jobs.CreateAsync("search index=_* | head 101", args: jobArgs);

        using (var stream = await job.GetSearchResultsAsync())
        {
            // Is the result count greater than the default of 100?
            Assert.Equal(101, job.ResultCount);
        }

        await job.CancelAsync();
    }

    /// <summary>
    /// Tests all search modes
    /// </summary>
    [Trait("acceptance-test", "Splunk.Client.Job")]
    [MockContext]
    [Fact]
    public async Task JobSearchModeArgument()
    {
        var type = typeof(SearchMode);

        await RunJobForEachEnum(type, mode =>
            new JobArgs()
            {
                SearchMode = (SearchMode)Enum.Parse(type, mode)
            });
    }

    [Trait("acceptance-test", "Splunk.Client.Job")]
    [MockContext]
    [Fact]
    public async Task JobTransitionDelay()
    {
        using var service = await SdkHelper.CreateService();
        //// Reference: [Algorithms for calculating variance](https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance#Online_algorithm)

        var args = new JobArgs { SearchMode = SearchMode.RealTime };

        var min = double.PositiveInfinity;
        var max = double.NegativeInfinity;

        var n = 0;
        var mean = 0.0;
        var variance = 0.0;
        var sampleSize = 1; // increase to compute statistics used to derive Assert.InRange values

        for (n = 1; n <= sampleSize; ++n)
        {
            var job = await service.Jobs.CreateAsync("search index=_internal", args: args);
            var start = DateTime.Now;
            var totalDelay = 0;

            for (var delay = 1000; delay < 5000; delay += 1000)
            {
                try
                {
                    await job.TransitionAsync(DispatchState.Done, delay);
                    break;
                }
                catch (TaskCanceledException)
                { }

                totalDelay += delay;
            }

            var duration = DateTime.Now - start;

            try
            {
                await job.CancelAsync();
            }
            catch (TaskCanceledException)
            { }

            var x = duration.TotalMilliseconds - totalDelay;

            if (x < min)
            {
                min = x;
            }

            if (x > max)
            {
                max = x;
            }

            var delta = x - mean;
            mean += delta / n;
            variance += delta * (x - mean);

            // Statistically derived by repeated tests with sampleSize = 100; no failures in a test with sampleSize = 10,000
            // This range is outside three standard deviations. Adjust as required to support your test environment.
            Assert.InRange(x, 1000, 5000);
        }

        double sd;

        if (--n < 2)
        {
            sd = variance = double.NaN;
        }
        else
        {
            variance /= n - 1;
            sd = Math.Sqrt(variance);
        }

        Console.WriteLine("\n  Mean: {0}\n  SD: {1}\n  Range: [{3}, {4}]\n  N: {2}", mean, sd, min, max, n); // swallowed by Xunit version [1.0, 2.0)
    }

    /// <summary>
    /// Tests all search modes for export
    /// </summary>
    [Trait("acceptance-test", "Splunk.Client.Job")]
    [MockContext]
    [Fact]
    public async Task ExportSearchModeArgument()
    {
        var type = typeof(SearchMode);

        await RunExportForEachEnum(Search, type, (mode) =>
            new SearchExportArgs()
            {
                SearchMode = (SearchMode)Enum.Parse(type, mode)
            });
    }

    /// <summary>
    /// Tests all search modes for export
    /// </summary>
    [Trait("acceptance-test", "Splunk.Client.Job")]
    [MockContext]
    [Fact]
    public async Task ExportTruncationModeArgument()
    {
        var type = typeof(TruncationMode);

        await RunExportForEachEnum(Search, type, (mode) =>
            new SearchExportArgs()
            {
                TruncationMode = (TruncationMode)Enum.Parse(type, mode)
            });
    }

    [Trait("acceptance-test", "Splunk.Client.Job")]
    [MockContext]
    [Fact]
    public async Task CanRefreshJob()
    {
        const string search = "search index=_internal * | head 1 | debug cmd=sleep param1=5";

        using var service = await SdkHelper.CreateService();
        var job = await service.Jobs.CreateAsync(search);

        await CheckJobAsync(job, service);
        Assert.True(job.DispatchState < DispatchState.Done);

        await job.TransitionAsync(DispatchState.Done, 10 * 1000);

        await CheckJobAsync(job, service);
        Assert.True(job.DispatchState == DispatchState.Done);

        await job.CancelAsync();
    }

    [Trait("acceptance-test", "Splunk.Client.QueuedSearchCreate")]
    [MockContext]
    [Fact]
    public async Task QueuedSearchCreate()
    {
        const string searchPrefix = "search index=_internal ";
        var i = 0;

        using (var service = await SdkHelper.CreateService())
        {
            var jobs = new List<Job>();
            Job? job = null;
            do
            {
                var jobArgs = new JobArgs
                {
                    SearchMode = SearchMode.RealTime
                };

                try
                {
                    // Jobs should eventually be queued w/o waiting for them to get to running state
                    job = await service.Jobs.Create(searchPrefix + i.ToString(), args: jobArgs);
                    Assert.Equal(DispatchState.None, job.DispatchState);
                    await job.GetAsync();
                    jobs.Add(job);
                    i++;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.GetBaseException().ToString());
                    break;
                }
            } while (job.DispatchState != DispatchState.Queued);

            Assert.NotNull(job);
            Assert.Equal(DispatchState.Queued, job!.DispatchState);

            // Cleanup
            foreach (var j in jobs)
            {
                await j.CancelAsync();
            }
        }

        Assert.True(i > 0);
    }

    [Trait("acceptance-test", "Splunk.Client.QueuedSearchCreateAsync")]
    [MockContext]
    [Fact]
    public async Task QueuedSearchCreateAsync()
    {
        const string searchPrefix = "search index=_internal ";
        var i = 0;

        using (var service = await SdkHelper.CreateService())
        {
            var jobs = new List<Job>();
            Job? job = null;
            do
            {
                var jobArgs = new JobArgs
                {
                    SearchMode = SearchMode.RealTime
                };

                try
                {
                    // Jobs should eventually be queued w/o waiting for them to get to running state
                    job = await service.Jobs.CreateAsync(searchPrefix + i.ToString(), args: jobArgs);
                    jobs.Add(job);
                    i++;
                }
                catch (Exception)
                {
                    break;
                }
            } while (job.DispatchState != DispatchState.Queued);

            Assert.NotNull(job);
            Assert.Equal(DispatchState.Queued, job!.DispatchState);

            // Cleanup
            foreach (var j in jobs)
            {
                await j.CancelAsync();
            }
        }

        Assert.True(i > 0);
    }

    #region Helpers

    /// <summary>
    /// Touches the job after it is queryable.
    /// </summary>
    /// <param name="job">The job</param>
    private static async Task CheckJobAsync(Job job, Service service)
    {
        //string[] dummyList;

        _ = job.CursorTime;
        //dummyString = job.Delegate;
        _ = job.DiskUsage;
        _ = job.DispatchState;
        _ = job.DoneProgress;
        _ = job.DropCount;
        _ = job.EarliestTime;
        _ = job.EventAvailableCount;
        _ = job.EventCount;
        _ = job.EventFieldCount;
        _ = job.EventIsStreaming;
        _ = job.EventIsTruncated;
        _ = job.EventSearch;
        _ = job.EventSorting;
        _ = job.IndexEarliestTime;
        _ = job.IndexLatestTime;
        _ = job.Keywords;
        //dummyString = job.Label;

        var serverInfo = await service.Server.GetInfoAsync();

        if (serverInfo.Version.CompareTo(new Version(6, 0)) < 0)
        {
            _ = job.LatestTime;
        }

        _ = job.NumPreviews;
        _ = job.Priority;
        _ = job.RemoteSearch;
        //dummyString = job.ReportSearch;
        _ = job.ResultCount;
        _ = job.ResultIsStreaming;
        _ = job.ResultPreviewCount;
        _ = job.RunDuration;
        _ = job.ScanCount;
        _ = job.EventSearch;// Search;
        _ = job.EarliestTime;//SearchEarliestTime;
        _ = job.LatestTime;
        _ = job.SearchProviders;
        _ = job.Sid;
        _ = job.StatusBuckets;
        _ = job.Ttl;
        _ = job.IsDone;
        _ = job.IsFailed;
        _ = job.IsFinalized;
        _ = job.IsPaused;
        _ = job.IsPreviewEnabled;
        _ = job.IsRealTimeSearch;
        _ = job.IsRemoteTimeline;
        _ = job.IsSaved;
        _ = job.IsSavedSearch;
        _ = job.IsZombie;
        Assert.Equal(job.Name, job.Sid);
    }

    /// <summary>
    /// Run export for each enum value in an enum type.
    /// </summary>
    /// <param name="enumType">The enum type</param>
    /// <param name="getJobExportArgs">
    /// The funtion to get arguments to run a job.
    /// </param>
    private static async Task RunExportForEachEnum(string search, Type enumType, Func<string, SearchExportArgs> getJobExportArgs)
    {
        using var service = await SdkHelper.CreateService();
        await ForEachEnum(enumType, async @enum =>
            {
                using var stream = await service.ExportSearchPreviewsAsync(search, getJobExportArgs(@enum));
            });
    }

    /// <summary>
    /// Run a job for each enum value in an enum type.
    /// </summary>
    /// <param name="enumType">The enum type</param>
    /// <param name="getJobArgs">
    /// The funtion to get arguments to run a job.
    /// </param>
    private static async Task RunJobForEachEnum(Type enumType, Func<string, JobArgs> getJobArgs)
    {
        using var service = await SdkHelper.CreateService();
        await ForEachEnum(enumType, async @enum =>
        {
            var job = await service.Jobs.CreateAsync(Search, args: getJobArgs(@enum));
            await job.CancelAsync();
        });
    }

    /// <summary>
    /// Perform an action for each enum value in an enum type.
    /// </summary>
    /// <param name="enumType">The enum type</param>
    /// <param name="action">
    /// The action to perform on an enum value
    /// </param>
    private static async Task ForEachEnum(Type enumType, Func<string, Task> action)
    {
        var enums = Enum.GetNames(enumType);

        foreach (var @enum in enums)
        {
            await action(@enum);
        }
    }

    #endregion
}
