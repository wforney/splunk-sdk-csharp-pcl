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

using System;
using System.Net;
using System.Threading.Tasks;
using Splunk.Client;
using Splunk.Client.Helper;
using Index = Splunk.Client.Index;

namespace Splunk.Examples.Submit;

/// <summary>
/// An example program to submit events into Splunk.
/// </summary>
public class Program
{
    static Program()
    {
        //// TODO: Use WebRequestHandler.ServerCertificateValidationCallback instead
        //// 1. Instantiate a WebRequestHandler
        //// 2. Set its ServerCertificateValidationCallback
        //// 3. Instantiate a Splunk.Client.Context with the WebRequestHandler
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
        {
            return true;
        };
    }

    static void Main(string[] args)
    {
        using (var service = new Service(SdkHelper.Splunk.Scheme, SdkHelper.Splunk.Host, SdkHelper.Splunk.Port, new Namespace(user: "nobody", app: "search")))
        {
            Run(service).Wait();
        }

        Console.Write("Press return to exit: ");
        Console.ReadLine();
    }

    /// <summary>
    /// The main program
    /// </summary>
    /// <param name="argv">The command line arguments</param>
    static async Task Run(Service service)
    {
        Console.WriteLine("Login as " + SdkHelper.Splunk.Username);

        await service.LogOnAsync(SdkHelper.Splunk.Username, SdkHelper.Splunk.Password);

        Console.WriteLine("Create an index");

        string indexName = "user-index";
        Index index = await service.Indexes.GetOrNullAsync(indexName);

        if (index != null)
        {
            await index.RemoveAsync();
        }

        index = await service.Indexes.CreateAsync(indexName);
        Exception exception = null;

        try
        {
            await index.EnableAsync();

            ITransmitter transmitter = service.Transmitter;
            SearchResult result;

            result = await transmitter.SendAsync("Hello World.", indexName);
            result = await transmitter.SendAsync("Goodbye world.", indexName);

            using var results = await service.SearchOneShotAsync(string.Format("search index={0}", indexName));
            foreach (SearchResult task in results)
            {
                Console.WriteLine(task);
            }
        }
        catch (Exception e)
        {
            exception = e;
        }

        await index.RemoveAsync();

        if (exception != null)
        {
            throw exception;
        }
    }
}
