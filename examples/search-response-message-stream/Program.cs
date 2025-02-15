﻿/*
 * Copyright 2013 Splunk, Inc.
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

namespace Splunk.Examples.Authenticate;

using System;
using System.Net;
using System.Threading.Tasks;
using Splunk.Client;
using Splunk.Client.Helper;
using Splunk.Client.Helpers;

/// <summary>
/// An example program that gets an HttpResponseMessage for any ExecutionMode
/// </summary>
public class Program
{
    static Program()
    {
        // In production we recommend using WebRequestHandler.ServerCertificateValidationCallback instead. To do this:
        // 1. Instantiate a WebRequestHandler
        // 2. Set its ServerCertificateValidationCallback
        // 3. Instantiate a Splunk.Client.Context with the WebRequestHandler
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
        {
            return true;
        };
    }

    /// <summary>
    /// Main function
    /// </summary>
    /// <param name="args">The arguments to main.</param>
    static void Main(string[] args)
    {
        using (var service = new Service(SdkHelper.Splunk.Scheme, SdkHelper.Splunk.Host, SdkHelper.Splunk.Port))
        {
            Console.WriteLine("Connected to {0}:{1} ", service.Context.Host, service.Context.Port);
            Run(service).Wait();
        }

        Console.Write("Press return to exit: ");
        Console.ReadLine();
    }

    /// <summary>
    /// Runs the specified service.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <returns>a task</returns>
    static async Task Run(Service service)
    {
        await service.LogOnAsync(SdkHelper.Splunk.Username, SdkHelper.Splunk.Password);

        try {
            //// This code shows how to execute a long-running search job.
            //// We query for the first 100,000 records from Splunk's _internal index and choose a 3 second
            //// delay to improve the chances our retry loop runs more than once.

            int delay = 3000;
            var job = await service.Jobs.CreateAsync("search index=_internal | head 100000", mode: ExecutionMode.Normal);

            for (int count = 1; ; ++count)
            {
                try
                {
                    await job.TransitionAsync(DispatchState.Done, delay);
                    break;
                }
                catch (TaskCanceledException)
                {
                    // Consider logging the fact that the operation is taking a long time, around count * (delay / 1000) seconds so far
                    // Also consider stopping the query, if it runs too long
                }
                // Consider increasing the delay on each iteration
            }

            //// Now that the search job is done we can print the results.
            //// This example shows how to fetch raw search results in a specific format: JSON. Select an alternative format by
            //// by selecting the OutputMode you like.

            using var message = await job.GetSearchResponseMessageAsync(outputMode: OutputMode.Json);
            Console.Error.WriteLine("Search results (Press Control-C to cancel:");
            var content = await message.Content.ReadAsStringAsync();
            Console.WriteLine(content);
        }
        finally
        {
            service.LogOffAsync().Wait();
        }
    }
}
