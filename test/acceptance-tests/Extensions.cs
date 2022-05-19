// <copyright file="Extensions.cs" company="Splunk Inc.">
//     Copyright © 2014
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

namespace Splunk.Client.AcceptanceTests;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Splunk.Client.Helper;
using Xunit;

/// <summary>
/// Class Extensions.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Log on as an asynchronous operation.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public static async Task LogOnAsync(this Service service) => await service.LogOnAsync(SdkHelper.Splunk.Username, SdkHelper.Splunk.Password);

    /// <summary>
    /// Waits for updated event count.
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="expectedEventCount">Expected event count.</param>
    /// <param name="seconds">Seconds.</param>
    /// <returns>The for updated event count.</returns>
    public static async Task PollForUpdatedEventCount(this Client.Index index, long expectedEventCount, int seconds = 60)
    {
        var watch = Stopwatch.StartNew();

        while (watch.Elapsed < new TimeSpan(0, 0, seconds) && index.TotalEventCount != expectedEventCount)
        {
            await Task.Delay(1000);
            await index.GetAsync();
        }

        Assert.Equal(expectedEventCount, index.TotalEventCount);
    }

    /// <summary>
    /// Create a fresh test app with the given name, delete the existing test app and reboot Splunk.
    /// </summary>
    /// <param name="applications">The applications.</param>
    /// <param name="name">The app name</param>
    /// <returns>A Task&lt;Application&gt; representing the asynchronous operation.</returns>
    public static async Task<Application?> RecreateAsync(this ApplicationCollection applications, string name)
    {
        var app = await applications.GetOrNullAsync(name);

        if (app is not null)
        {
            await app.RemoveAsync();
        }

        _ = await applications.CreateAsync(name, "sample_app");
        app = await applications.GetOrNullAsync(name);
        Assert.NotNull(app);

        return app;
    }

    /// <summary>
    /// Recreate as an asynchronous operation.
    /// </summary>
    /// <param name="indexes">The indexes.</param>
    /// <param name="name">The name.</param>
    /// <returns>A Task&lt;Index&gt; representing the asynchronous operation.</returns>
    public static async Task<Client.Index?> RecreateAsync(this IndexCollection indexes, string name)
    {
        var index = await indexes.GetOrNullAsync(name);

        if (index is not null)
        {
            await index.RemoveAsync();
        }

        _ = await indexes.CreateAsync(name);

        index = await indexes.GetOrNullAsync(name);
        Assert.NotNull(index);

        return index;
    }

    /// <summary>
    /// Asynchronously removes an application by name, if it exists.
    /// </summary>
    /// <param name="applications">Applications.</param>
    /// <param name="name">Name.</param>
    /// <returns>
    /// <c>true</c> if the application existed and was removed; otherwise, if the application
    /// did not exist, <c>false</c>.
    /// </returns>
    public static async Task<bool> RemoveAsync(this ApplicationCollection applications, string name)
    {
        var app = await applications.GetOrNullAsync(name);

        if (app is null)
        {
            return false;
        }

        await app.RemoveAsync();
        return true;
    }

    /// <summary>
    /// Asynchronously removes an index by name, if it exists.
    /// </summary>
    /// <param name="indexes">The indexes.</param>
    /// <param name="name">Name.</param>
    /// <returns>
    /// <c>true</c> if the index existed and was removed; otherwise, if the application did not
    /// exist, <c>false</c>.
    /// </returns>
    public static async Task<bool> RemoveAsync(this IndexCollection indexes, string name)
    {
        var index = await indexes.GetOrNullAsync(name);

        if (index is null)
        {
            return false;
        }

        await index.RemoveAsync();
        return true;
    }
}
