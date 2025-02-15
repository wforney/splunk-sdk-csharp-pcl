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

namespace Splunk.Examples.Submit;

using Splunk.Client;
using Splunk.Client.Helper;
using Splunk.Client.Helpers;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

/// <summary>
/// An example program to list apps installed on the server.
/// </summary>
public class Program
{
    static Program()
    {
        // TODO: Use WebRequestHandler.ServerCertificateValidationCallback instead
        // 1. Instantiate a WebRequestHandler
        // 2. Set its ServerCertificateValidationCallback
        // 3. Instantiate a Splunk.Client.Context with the WebRequestHandler
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
    public async static Task Run(Service service)
    {
        await service.LogOnAsync(SdkHelper.Splunk.Username, SdkHelper.Splunk.Password);

        // Load connection info for Splunk server in .splunkrc file.
        Console.WriteLine("List of Apps:");
        await service.Applications.GetAllAsync();

        foreach (var app in service.Applications)
        {
            Console.WriteLine(app.Name);
            // Write a seperator between the name and the description of an app.
            Console.WriteLine(Enumerable.Repeat<char>('-', app.Name.Length).ToArray());
            Console.WriteLine(app.Description);
        }
    }
}
