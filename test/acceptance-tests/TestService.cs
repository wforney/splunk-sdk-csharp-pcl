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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Splunk.Client;
using Splunk.Client.Arguments;
using Splunk.Client.Exceptions;
using Splunk.Client.Helper;
using Splunk.Client.Settings;
using Xunit;

/// <summary>
/// The test service class.
/// </summary>
public class TestService
{
    // TODO: Move to unit-tests project
    [Trait("acceptance-test", "Splunk.Client.Service")]
    [Fact]
    public void CanConstructService()
    {
        foreach (var ns in TestNamespaces)
        {
            using var service = new Service(SdkHelper.Splunk.Scheme, SdkHelper.Splunk.Host, SdkHelper.Splunk.Port, ns);
            Assert.Equal(string.Format("{0}://{1}:{2}/{3}",
                SdkHelper.Splunk.Scheme.ToString().ToLower(),
                SdkHelper.Splunk.Host,
                SdkHelper.Splunk.Port,
                ns),
                service.ToString());

            _ = Assert.IsType<ApplicationCollection>(service.Applications);
            Assert.NotNull(service.Applications);

            _ = Assert.IsType<ConfigurationCollection>(service.Configurations);
            Assert.NotNull(service.Configurations);

            _ = Assert.IsType<IndexCollection>(service.Indexes);
            Assert.NotNull(service.Indexes);

            _ = Assert.IsType<JobCollection>(service.Jobs);
            Assert.NotNull(service.Jobs);

            _ = Assert.IsType<SavedSearchCollection>(service.SavedSearches);
            Assert.NotNull(service.SavedSearches);

            _ = Assert.IsType<Server>(service.Server);
            Assert.NotNull(service.Server);

            _ = Assert.IsType<StoragePasswordCollection>(service.StoragePasswords);
            Assert.NotNull(service.StoragePasswords);

            _ = Assert.IsType<Transmitter>(service.Transmitter);
            Assert.NotNull(service.Transmitter);
        }
    }

    #region Access Control

    [Trait("acceptance-test", "Splunk.Client.StoragePassword")]
    [MockContext]
    [Fact]
    public async Task CanCrudStoragePassword()
    {
        foreach (var ns in TestNamespaces)
        {
            using var service = await SdkHelper.CreateService(ns);
            var sps = service.StoragePasswords;
            await sps.GetAllAsync();

            foreach (var sp in sps)
            {
                if (sp.Username.Contains("delete-me-"))
                {
                    await sp.RemoveAsync(); // TODO: FAILS BECAUSE OF MONO URI IMPLEMENTATION!
                }
            }

            //// Create and change the password for 50 StoragePassword instances

            var surname = MockContext.GetOrElse(string.Format("delete-me-{0}-", Guid.NewGuid().ToString("N")));
            var realms = new string?[] { null, "splunk.com" };

            for (var i = 0; i < realms.Length; i++)
            {
                var password = "foobar-foobar";
                var username = surname + i;
                var realm = realms[i];

                //// Create

                var sp = await sps.CreateAsync(password, username, realm);

                Assert.Equal(password, sp.ClearPassword);
                Assert.Equal(username, sp.Username);
                Assert.Equal(realm, sp.Realm);

                //// Read

                await sp.GetAsync();

                Assert.Equal(password, sp.ClearPassword);
                Assert.Equal(username, sp.Username);
                Assert.Equal(realm, sp.Realm);

                sp = await sps.GetAsync(username, realm);

                Assert.Equal(password, sp.ClearPassword);
                Assert.Equal(username, sp.Username);
                Assert.Equal(realm, sp.Realm);

                //// Update
                var generator = new PasswordGenerator();
                password = MockContext.GetOrElse(generator.Generate());
                await sp.UpdateAsync(password);

                Assert.Equal(password, sp.ClearPassword);
                Assert.Equal(username, sp.Username);
                Assert.Equal(realm, sp.Realm);

                //// Remove

                await sp.RemoveAsync();

                try
                {
                    await sp.GetAsync();
                    Assert.True(false);
                }
                catch (ResourceNotFoundException)
                { }

                try
                {
                    _ = await sps.GetAsync(username, realm);
                    Assert.True(false);
                }
                catch (ResourceNotFoundException)
                { }

                sp = await sps.GetOrNullAsync(username, realm);
                Assert.Null(sp);
            }
        }
    }

    [Trait("acceptance-test", "Splunk.Client.Service")]
    [MockContext]
    [Fact]
    public async Task CanGetCapabilities()
    {
        using var service = await SdkHelper.CreateService();
        var capabilities = await service.GetCapabilitiesAsync();
        var serverInfo = await service.Server.GetInfoAsync();

        if (serverInfo.OSName == "Windows")
        {
            foreach (var s in new List<string>
                    {
                        "accelerate_datamodel",         // 0
                        "admin_all_objects",            // 1
                        "change_authentication",        // 2
                        "change_own_password",          // 3
                        "delete_by_keyword",            // 4
                        "edit_deployment_client",       // 5
                        "edit_deployment_server",       // 6
                        "edit_dist_peer",               // 7
                        "edit_forwarders",              // 8
                        "edit_httpauths",               // 9
                        "edit_input_defaults",          // 10
                        "edit_monitor",                 // 11
                        "edit_roles",                   // 12
                        "edit_scripted",                // 13
                        "edit_search_server",           // 14
                        "edit_server",                  // 15
                        "edit_splunktcp",               // 16
                        "edit_splunktcp_ssl",           // 17
                        "edit_tcp",                     // 18
                        "edit_udp",                     // 19
                        "edit_user",                    // 20
                        "edit_view_html",               // 21
                        "edit_web_settings",            // 22
                        //"edit_win_admon",               // 23
                        "edit_win_eventlogs",           // 24
                        //"edit_win_perfmon",             // 25
                        "edit_win_regmon",              // 26
                        "edit_win_wmiconf",             // 27
                        "get_diag",                     // 28
                        "get_metadata",                 // 29
                        "get_typeahead",                // 30
                        "indexes_edit",                 // 31
                        "input_file",                   // 32
                        "license_edit",                 // 33
                        "license_tab",                  // 34
                        "list_deployment_client",       // 35
                        "list_deployment_server",       // 36
                        "list_forwarders",              // 37
                        "list_httpauths",               // 38
                        "list_inputs",                  // 39
                        "list_pdfserver",               // 40
                        "list_win_localavailablelogs",  // 41
                        "output_file",                  // 42
                        "request_remote_tok",           // 43
                        "rest_apps_management",         // 44
                        "rest_apps_view",               // 45
                        "rest_properties_get",          // 46
                        "rest_properties_set",          // 47
                        "restart_splunkd",              // 48
                        "rtsearch",                     // 49
                        "run_debug_commands",           // 50
                        "schedule_rtsearch",            // 51
                        "schedule_search",              // 52
                        "search",                       // 53
                        "use_file_operator",            // 54
                        "write_pdfserver"               // 55
                    })
            {
                Assert.True(capabilities.Contains(s), string.Format("{0} is not in capabilities", s));
            }
        }
        else
        {
            Assert.Equal(new ReadOnlyCollection<string>(new List<string>
                    {
                        "accelerate_datamodel",
                        "admin_all_objects",
                        "change_authentication",
                        "change_own_password",
                        "delete_by_keyword",
                        "edit_deployment_client",
                        "edit_deployment_server",
                        "edit_dist_peer",
                        "edit_forwarders",
                        "edit_httpauths",
                        "edit_input_defaults",
                        "edit_monitor",
                        "edit_roles",
                        "edit_scripted",
                        "edit_search_server",
                        "edit_server",
                        "edit_splunktcp",
                        "edit_splunktcp_ssl",
                        "edit_tcp",
                        "edit_udp",
                        "edit_user",
                        "edit_view_html",
                        "edit_web_settings",
                        "get_diag",
                        "get_metadata",
                        "get_typeahead",
                        "indexes_edit",
                        "input_file",
                        "license_edit",
                        "license_tab",
                        "list_deployment_client",
                        "list_deployment_server",
                        "list_forwarders",
                        "list_httpauths",
                        "list_inputs",
                        "output_file",
                        "request_remote_tok",
                        "rest_apps_management",
                        "rest_apps_view",
                        "rest_properties_get",
                        "rest_properties_set",
                        "restart_splunkd",
                        "rtsearch",
                        "run_debug_commands",
                        "schedule_rtsearch",
                        "schedule_search",
                        "search",
                        "use_file_operator"
                    }),
                capabilities);
        }
    }

    [Trait("acceptance-test", "Splunk.Client.StoragePasswordCollection")]
    [MockContext]
    [Fact]
    public async Task CanGetStoragePasswords()
    {
        foreach (var ns in TestNamespaces)
        {
            using var service = await SdkHelper.CreateService(ns);
            var sps = service.StoragePasswords;
            await sps.GetAllAsync();

            if (sps.Count < 50)
            {
                //// Ensure we've got 50 passwords to enumerate

                var surname = MockContext.GetOrElse(string.Format("delete-me-{0}-", Guid.NewGuid().ToString("N")));
                var realms = new string?[] { null, "splunk.com" };

                for (var i = 0; i < 50 - sps.Count; i++)
                {
                    var username = surname + i;
                    var realm = realms[i % realms.Length];
                    var generator = new PasswordGenerator();
                    var password = MockContext.GetOrElse(generator.Generate());

                    var sp = await service.StoragePasswords.CreateAsync(password, username, realm);

                    Assert.Equal(password, sp.ClearPassword);
                    Assert.Equal(username, sp.Username);
                    Assert.Equal(realm, sp.Realm);
                }

                await service.StoragePasswords.GetAllAsync();
            }

            var count = 0;

            foreach (var sp in sps)
            {
                count++;
            }

            Assert.Equal(sps.Count, count);

            var spl = new List<StoragePassword>(sps.Count);

            for (var i = 0; i < count; i++)
            {
                spl.Add(sps[i]);
            }

            Assert.True(sps.SequenceEqual(spl));
        }
    }

    [Trait("acceptance-test", "Splunk.Client.Service")]
    [MockContext]
    [Fact]
    public async Task CanLoginAndLogoff()
    {
        using var service = await SdkHelper.CreateService(Namespace.Default);
        try
        {
            await service.Applications.GetAllAsync();
        }
        catch (Exception e)
        {
            Assert.Fail(string.Format("Expected: No exception, Actual: {0}", e.GetType().FullName));
        }

        await service.LogOffAsync();

        Assert.Null(service.SessionKey);
        Assert.True(service.Context.CookieJar.IsEmpty());

        try
        {
            await service.Applications.GetAllAsync();
            Assert.Fail("Expected AuthenticationFailureException");
        }
        catch (AuthenticationFailureException e)
        {
            Assert.Equal(HttpStatusCode.Unauthorized, e.StatusCode);
        }

        try
        {
            await service.LogOnAsync("admin", "bad-password");
            Assert.Fail(string.Format("Expected: {0}, Actual: {1}", typeof(AuthenticationFailureException).FullName, "no exception"));
        }
        catch (AuthenticationFailureException e)
        {
            Assert.Equal(HttpStatusCode.Unauthorized, e.StatusCode);
            _ = Assert.Single(e.Details);
            Assert.Equal(e.Details[0], new Message(MessageType.Warning, "Login failed"));
        }
        catch (Exception e)
        {
            Assert.Fail(string.Format("Expected: {0}, Actual: {1}", typeof(AuthenticationFailureException).FullName, e.GetType().FullName));
        }
    }

    #endregion

    #region Cookie Tests

    [Trait("acceptance-test", "Splunk.Client.Service")]
    [MockContext]
    [Fact]
    public async Task CanGetCookieOnLogin()
    {
        if (await this.AreCookiesSupported())
        {
            using var service = await SdkHelper.CreateService(Namespace.Default);
            Assert.False(service.Context.CookieJar.IsEmpty());
        }
    }

    [Trait("acceptance-test", "Splunk.Client.Service")]
    [MockContext]
    [Fact]
    public async Task CanMakeAuthRequestWithCookie()
    {
        if (await this.AreCookiesSupported())
        {
            using var service = await SdkHelper.CreateService(Namespace.Default);
            Assert.False(service.Context.CookieJar.IsEmpty());
            try
            {
                await service.Applications.GetAllAsync();
            }
            catch (Exception e)
            {
                Assert.Fail(string.Format("Expected: No exception, Actual: {0}", e.GetType().FullName));
            }
        }
    }

    [Trait("acceptance-test", "Splunk.Client.Service")]
    [MockContext]
    [Fact]
    public async Task CanMakeRequestWithOnlyCookie()
    {
        if (await this.AreCookiesSupported())
        {
            // Create a service
            var service = await SdkHelper.CreateService(Namespace.Default);
            // Get the cookie out of that valid service
            var cookie = service.Context.CookieJar.GetCookieHeader();

            // SdkHelper with login = false
            var service2 = await SdkHelper.CreateService(Namespace.Default, false);
            service2.Context.CookieJar.AddCookie(cookie);

            // Check that cookie is the same
            var cookie2 = service2.Context.CookieJar.GetCookieHeader();
            Assert.Equal(cookie, cookie2);

            try
            {
                await service2.Applications.GetAllAsync();
            }
            catch (Exception e)
            {
                Assert.Fail(string.Format("Expected: No exception, Actual: {0}", e.GetType().FullName));
            }
        }
    }

    [Trait("accpetance-test", "Splunk.Clinet.Service")]
    [MockContext]
    [Fact]
    public async Task CanNotMakeRequestWithOnlyBadCookie()
    {
        if (await this.AreCookiesSupported())
        {
            // SdkHelper with login = false
            var service = await SdkHelper.CreateService(Namespace.Default, false);
            // Put a bad cookie into the cookie jar
            service.Context.CookieJar.AddCookie("bad=cookie");
            try
            {
                await service.Applications.GetAllAsync();
                Assert.Fail("Expected AuthenticationFailureException");
            }
            catch (AuthenticationFailureException e)
            {
                Assert.Equal(HttpStatusCode.Unauthorized, e.StatusCode);
            }
        }
    }

    [Trait("acceptance-test", "Splunk.Client.Service")]
    [MockContext]
    [Fact]
    public async Task CanMakeRequestWithGoodCookieAndBadCookie()
    {
        if (await this.AreCookiesSupported())
        {
            // Create a service
            var service = await SdkHelper.CreateService(Namespace.Default);
            // Get the cookie out of that valid service
            var cookie = service.Context.CookieJar.GetCookieHeader();

            // SdkHelper with login = false
            var service2 = await SdkHelper.CreateService(Namespace.Default, false);
            service2.Context.CookieJar.AddCookie(cookie);

            // Check that cookie is the same
            var cookie2 = service2.Context.CookieJar.GetCookieHeader();
            Assert.Equal(cookie, cookie2);

            // Add an additional bad cookie
            service2.Context.CookieJar.AddCookie("bad=cookie");

            try
            {
                await service2.Applications.GetAllAsync();
            }
            catch (Exception e)
            {
                Assert.Fail(string.Format("Expected: No exception, Actual: {0}", e.GetType().FullName));
            }
        }
    }

    [Trait("acceptance-test", "Splunk.Client.Service")]
    [MockContext]
    [Fact]
    public async Task CanMakeRequestWithBadCookieAndGoodCookie()
    {
        if (await this.AreCookiesSupported())
        {
            // Create a service
            var service = await SdkHelper.CreateService(Namespace.Default);
            // Get the cookie out of that valid service
            var goodCookie = service.Context.CookieJar.GetCookieHeader();

            // SdkHelper with login = false
            var service2 = await SdkHelper.CreateService(Namespace.Default, false);

            // Add a bad cookie
            service2.Context.CookieJar.AddCookie("bad=cookie");

            // Add the good cookie
            service2.Context.CookieJar.AddCookie(goodCookie);

            try
            {
                await service2.Applications.GetAllAsync();
            }
            catch (Exception e)
            {
                Assert.Fail(string.Format("Expected: No exception, Actual: {0}", e.GetType().FullName));
            }
        }
    }

    public async Task<bool> AreCookiesSupported()
    {
        using var service = await SdkHelper.CreateService();
        var info = await service.Server.GetInfoAsync();
        var v = info.Version;

        return (v.Major == 6 && v.Minor >= 2) || v.Major > 6;
    }
    #endregion

    #region Applications

    [Trait("acceptance-test", "Splunk.Client.Application")]
    [MockContext]
    [Fact]
    public async Task CanCrudApplication()
    {
        using var service = await SdkHelper.CreateService();
        //// Install, update, and remove the Splunk App for Twitter Data, version 2.3.1

        var twitterApp = await service.Applications.GetOrNullAsync("twitter2");

        if (twitterApp != null)
        {
            await twitterApp.RemoveAsync();

            await SdkHelper.ThrowsAsync<ResourceNotFoundException>(async () => await twitterApp.GetAsync());

            twitterApp = await service.Applications.GetOrNullAsync("twitter2");
            Assert.Null(twitterApp);
        }

        var splunkHostEntry = await Dns.GetHostEntryAsync(service.Context.Host);
        var localHostEntry = await Dns.GetHostEntryAsync("localhost");

        if (splunkHostEntry.HostName == localHostEntry.HostName)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Data", "app-for-twitter-data_230.spl");
            Assert.True(File.Exists(path));

            twitterApp = await service.Applications.InstallAsync(path, update: true);

            //// Other asserts on the contents of the update

            Assert.Equal("Splunk", twitterApp.ApplicationAuthor);
            Assert.Equal(true, twitterApp.CheckForUpdates);
            Assert.Equal(false, twitterApp.Configured);
            Assert.Equal("This application indexes Twitter's sample stream.", twitterApp.Description);
            Assert.Equal("Splunk-Twitter Connector", twitterApp.Label);
            Assert.Equal(false, twitterApp.Refresh);
            Assert.Equal(false, twitterApp.StateChangeRequiresRestart);
            Assert.Equal("2.3.0", twitterApp.Version);
            Assert.Equal(true, twitterApp.Visible);

            //// TODO: Check ApplicationSetupInfo and ApplicationUpdateInfo
            //// We might check that there is no update info for 2.3.1:
            ////    Assert.Null(twitterApplicationUpdateInfo.Update);
            //// Then change the version number to 2.3.0:
            ////    await twitterApplication.UpdateAsync(new ApplicationAttributes() { Version = "2.3.0" });
            //// Finally:
            //// ApplicationUpdateInfo twitterApplicationUpdateInfo = await twitterApplication.GetUpdateInfoAsync();
            //// Assert.NotNull(twitterApplicationUpdateInfo.Update);
            //// Assert.True(string.Compare(twitterApplicationUpdateInfo.Update.Version, "2.3.0") == 1, "expect the newer twitter app info");
            //// Assert.Equal("41ceb202053794cfec54b8d28f78d83c", twitterApplicationUpdateInfo.Update.Checksum);

            var setupInfo = await twitterApp.GetSetupInfoAsync();
            var updateInfo = await twitterApp.GetUpdateInfoAsync();

            await twitterApp.RemoveAsync();

            try
            {
                await twitterApp.GetAsync();
                Assert.Fail("Expected ResourceNotFoundException");
            }
            catch (ResourceNotFoundException)
            { }

            twitterApp = await service.Applications.GetOrNullAsync("twitter2");
            Assert.Null(twitterApp);
        }

        //// Create an app from one of the built-in templates

        var name = MockContext.GetOrElse(string.Format("delete-me-{0}", Guid.NewGuid()));

        var creationAttributes = new ApplicationAttributes()
        {
            ApplicationAuthor = "Splunk",
            Configured = true,
            Description = "This app confirms that an app can be created from a template",
            Label = name,
            Version = "2.0.0",
            Visible = true
        };

        var templatedApp = await service.Applications.CreateAsync(name, "barebones", creationAttributes);

        Assert.Equal(creationAttributes.ApplicationAuthor, templatedApp.ApplicationAuthor);
        Assert.Equal(true, templatedApp.CheckForUpdates);
        Assert.Equal(creationAttributes.Configured, templatedApp.Configured);
        Assert.Equal(creationAttributes.Description, templatedApp.Description);
        Assert.Equal(creationAttributes.Label, templatedApp.Label);
        Assert.Equal(false, templatedApp.Refresh);
        Assert.Equal(false, templatedApp.StateChangeRequiresRestart);
        Assert.Equal(creationAttributes.Version, templatedApp.Version);
        Assert.Equal(creationAttributes.Visible, templatedApp.Visible);

        var updateAttributes = new ApplicationAttributes()
        {
            ApplicationAuthor = "Splunk, Inc.",
            Configured = true,
            Description = "This app update confirms that an app can be updated from a template",
            Label = name,
            Version = "2.0.1",
            Visible = true
        };

        var updatedSnapshot = await templatedApp.UpdateAsync(updateAttributes, checkForUpdates: true);
        Assert.True(updatedSnapshot);
        await templatedApp.GetAsync(); // Because UpdateAsync doesn't produce an updated snapshot

        Assert.Equal(updateAttributes.ApplicationAuthor, templatedApp.ApplicationAuthor);
        Assert.Equal(true, templatedApp.CheckForUpdates);
        Assert.Equal(updateAttributes.Configured, templatedApp.Configured);
        Assert.Equal(updateAttributes.Description, templatedApp.Description);
        Assert.Equal(updateAttributes.Label, templatedApp.Label);
        Assert.Equal(false, templatedApp.Refresh);
        Assert.Equal(false, templatedApp.StateChangeRequiresRestart);
        Assert.Equal(updateAttributes.Version, templatedApp.Version);
        Assert.Equal(updateAttributes.Visible, templatedApp.Visible);

        Assert.False(templatedApp.Disabled);

        await templatedApp.DisableAsync();
        await templatedApp.GetAsync(); // Because POST apps/local/{name} does not return an updated snapshot
        Assert.True(templatedApp.Disabled);

        await templatedApp.EnableAsync();
        await templatedApp.GetAsync(); // Because POST apps/local/{name} does not return an updated snapshot
        Assert.False(templatedApp.Disabled);

        var archiveInfo = await templatedApp.PackageAsync();

        await templatedApp.RemoveAsync();

        try
        {
            await templatedApp.GetAsync();
            Assert.Fail("Expected ResourceNotFoundException");
        }
        catch (ResourceNotFoundException)
        { }

        // On Windows, this fails with a permissions error. At some point investigate and reinstate.
        /*
        if (splunkHostEntry.HostName == localHostEntry.HostName)
        {
            File.Delete(archiveInfo.Path);
        }
        */

        templatedApp = await service.Applications.GetOrNullAsync(templatedApp.Name);
        Assert.Null(templatedApp);
    }

    [Trait("acceptance-test", "Splunk.Client.ApplicationCollection")]
    [MockContext]
    [Fact]
    public async Task CanGetApplications()
    {
        foreach (var ns in TestNamespaces)
        {
            using var service = await SdkHelper.CreateService(ns);
            var args = new ApplicationCollection.Filter()
            {
                Offset = 0,
                Count = 10
            };

            do
            {
                await service.Applications.GetSliceAsync(args);
                await service.Applications.ReloadAsync();

                foreach (var application in service.Applications)
                {
                    string? value = null;

                    value = string.Format("ApplicationAuthor = {0}", application.ApplicationAuthor);
                    value = string.Format("Author = {0}", application.Author);
                    value = string.Format("CheckForUpdates = {0}", application.CheckForUpdates);
                    value = string.Format("Configured = {0}", application.Configured);
                    value = string.Format("Description = {0}", application.Description);
                    value = string.Format("Disabled = {0}", application.Disabled);
                    value = string.Format("Eai = {0}", application.Eai);
                    value = string.Format("Id = {0}", application.Id);
                    value = string.Format("Label = {0}", application.Label);
                    value = string.Format("Links = {0}", application.Links);
                    value = string.Format("Name = {0}", application.Name);
                    value = string.Format("Namespace = {0}", application.Namespace);
                    //value = string.Format("Published = {0}", application.Published);
                    value = string.Format("ResourceName = {0}", application.ResourceName);
                    value = string.Format("StateChangeRequiresRestart = {0}", application.StateChangeRequiresRestart);
                    value = string.Format("Updated = {0}", application.Updated);
                    value = string.Format("Version = {0}", application.Version);
                    value = string.Format("Visible = {0}", application.Visible);
                }

                args.Offset += service.Applications.Pagination.ItemsPerPage;
            }
            while (args.Offset < service.Applications.Pagination.TotalResults);
        }
    }

    #endregion

    #region Configuration

    [Trait("acceptance-test", "Splunk.Client.Configuration")]
    [MockContext]
    [Fact]
    public async Task CanCrudConfiguration() // no delete operation is available
    {
        const string testApplicationName = "acceptance-test_Splunk.Client.Configuration";

        using (var service = await SdkHelper.CreateService())
        {
            var application = await service.Applications.GetOrNullAsync(testApplicationName);

            if (application is not null)
            {
                await application.RemoveAsync();

                application = await service.Applications.GetOrNullAsync(testApplicationName);
                Assert.Null(application);
            }

            _ = await service.Applications.CreateAsync(testApplicationName, "barebones");
            application = await service.Applications.GetOrNullAsync(testApplicationName);
            Assert.NotNull(application);

            await service.Server.RestartAsync(2 * 60 * 1000);
        }

        using (var service = await SdkHelper.CreateService(new Namespace("nobody", testApplicationName)))
        {
            var fileName = MockContext.GetOrElse(string.Format("delete-me-{0}", Guid.NewGuid()));

            //// Create

            var configuration = await service.Configurations.CreateAsync(fileName);

            //// Read

            configuration = await service.Configurations.GetAsync(fileName);

            //// Update the default stanza through a ConfigurationStanza object

            var defaultStanza = await configuration.UpdateAsync("default", new Argument("foo", 1), new Argument("bar", 2));
            _ = await defaultStanza.UpdateAsync(new Argument("bar", 3), new Argument("foobar", 4));
            await defaultStanza.UpdateAsync("non_existent_setting", "some_value");

            await defaultStanza.GetAsync(); // because the rest api does not return settings unless you ask for them
            Assert.Equal(4, defaultStanza.Count);

            ConfigurationSetting setting;

            setting = defaultStanza.SingleOrDefault(s => s.Title == "foo");
            Assert.NotNull(setting);
            Assert.Equal("1", setting.Value);

            setting = defaultStanza.SingleOrDefault(s => s.Title == "bar");
            Assert.NotNull(setting);
            Assert.Equal("3", setting.Value);

            setting = defaultStanza.SingleOrDefault(s => s.Title == "foobar");
            Assert.NotNull(setting);
            Assert.Equal("4", setting.Value);

            setting = defaultStanza.SingleOrDefault(s => s.Title == "non_existent_setting");
            Assert.NotNull(setting);
            Assert.Equal("some_value", setting.Value);

            //// Create, read, update, and delete a stanza through the Service object

            var configurationStanza = await configuration.CreateAsync("stanza");
            Assert.Empty(configurationStanza);

            var isUpdatedSnapshot = await configurationStanza.UpdateAsync(new Argument("foo", 5), new Argument("bar", 6));
            Assert.False(isUpdatedSnapshot);
            Assert.Empty(configurationStanza);

            await configurationStanza.GetAsync();
            Assert.Equal(4, configurationStanza.Count); // because all stanzas inherit from the default stanza

            setting = configurationStanza.SingleOrDefault(s => s.Title == "foo");
            Assert.NotNull(setting);
            Assert.Equal("5", setting.Value);

            setting = configurationStanza.SingleOrDefault(s => s.Title == "bar");
            Assert.NotNull(setting);
            Assert.Equal("6", setting.Value);

            await configurationStanza.RemoveAsync();

            try
            {
                await configurationStanza.GetAsync();
                Assert.True(false);
            }
            catch (ResourceNotFoundException)
            { }

            configurationStanza = await configuration.GetOrNullAsync("stanza");
            Assert.Null(configurationStanza);
        }

        using (var service = await SdkHelper.CreateService())
        {
            var application = await service.Applications.GetAsync(testApplicationName);
            await application.RemoveAsync();
            await service.Server.RestartAsync(2 * 60 * 1000);
        }
    }

    [Trait("acceptance-test", "Splunk.Client.ConfigurationCollection")]
    [MockContext]
    [Fact]
    public async Task CanGetConfigurations()
    {
        var watch1 = new Stopwatch();
        watch1.Start();
        var stopwatch = new Stopwatch();
        var fail = false;
        foreach (var ns in TestNamespaces)
        {
            Console.WriteLine("namespace=" + ns);
            using var service = await SdkHelper.CreateService();
            #region MyRegion stanzaOfInputConfigurations

            var inputsConfiguration = await service.Configurations.GetAsync("inputs");

            stopwatch.Start();
            // check stanza
            foreach (var stanza in inputsConfiguration) // TODO: FAILS BECAUSE OF MONO URI IMPLEMENTATION!
            {
                try
                {
                    Console.WriteLine(stanza.Name);
                    await stanza.GetAsync();
                    Console.WriteLine("        stanza={0}", stanza.Name);
                    Console.WriteLine("        currentStopwatch={0}.", stopwatch.Elapsed.TotalMinutes);
                    if (stopwatch.Elapsed.TotalMinutes > 5)
                    {
                        fail = true;
                        Console.WriteLine(
                            "!!!execute more than 10 minutes, fail on stanz in inputConfigurations!!!!");
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("=====1======= exception on inputsConfiguration stanza: {0}: {1}===",
                        stanza.Name, e);
                }
            }

            Console.WriteLine("    take {0}m to enumerate inputsConfiguration.",
                stopwatch.Elapsed.TotalMinutes);

            #endregion

            #region MyRegion serviceConfigurations

            await service.Configurations.GetAllAsync();

            Console.WriteLine("    # of service.Configurations={0}.", service.Configurations.Count);

            Console.WriteLine("=======================2. all config=============================");
            foreach (var configuration in service.Configurations)
            {
                try
                {
                    await configuration.GetAllAsync();
                    Console.WriteLine("       2.configuration={0}", configuration.Name);
                    foreach (var stanza in configuration)
                    {
                        try
                        {

                            await stanza.GetAsync();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("======2.1====== exception on stanza={0}: {1}=========",
                                stanza.Name, e);
                        }
                    }

                    Console.WriteLine("        currentStopwatch={0}.", stopwatch.Elapsed.TotalMinutes);
                }
                catch (Exception e)
                {
                    Console.WriteLine("======2====== exception on config- {0}: {1}=========",
                        configuration.Name, e);
                }

                if (stopwatch.Elapsed.TotalMinutes > 5)
                {
                    fail = true;
                    Console.WriteLine(
                        "!!!execute more than 10 minutes, fail on enumerate serviceConfigurations !!!!");
                    break;
                }
            }

            Console.WriteLine("    take {0}m to enumerate service.Configurations.",
                stopwatch.Elapsed.TotalMinutes);
            Console.WriteLine("total take {0} to here2", watch1.Elapsed.TotalMinutes);

            #endregion

            #region MyRegion serviceConfigAndStanza

            Console.WriteLine("================ 3. serviceConfigAndStanza=====================");

            var configurationList = new List<Configuration>(service.Configurations.Count);

            var myconut = service.Configurations.Count;
            for (var i = 0; i < service.Configurations.Count; i++)
            {
                var configuration = service.Configurations[i];
                Console.WriteLine("3.configuration={0}", configuration.Name);

                configurationList.Add(configuration);

                await configuration.GetAllAsync();
                Console.WriteLine("        3.configuration={0},count={1}", configuration.Name,
                    configuration.Count);
                var stanzaList = new List<ConfigurationStanza>(configuration.Count);

                for (var j = 0; j < configuration.Count; j++)
                {
                    var stanza = configuration[j];

                    try
                    {
                        stanzaList.Add(stanza);
                        await stanza.GetAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("======3====== exception on add {0}: {1}=========", stanza.Name, e);
                    }
                }

                if (!string.Join(",", configuration.ToList()).Equals(string.Join(",", stanzaList)))
                {
                    fail = true;
                    Console.WriteLine(" configurationToList={0} ", string.Join(",", configuration.ToList()));
                    Console.WriteLine(" stanzaList={0} ", string.Join(",", stanzaList));
                }

                if (stopwatch.Elapsed.TotalMinutes > 5)
                {
                    fail = true;
                    Console.WriteLine("!!!execute more than 10 minutes, fail on compare {0}!!!!",
                        configuration.Name);
                    break;
                }
            }

            stopwatch.Stop();
            Console.WriteLine("take {0}m to add/compare all configurations.", stopwatch.Elapsed.TotalMinutes);

            #endregion

            Console.WriteLine("total take {0} to here", watch1.Elapsed.TotalMinutes);
            Console.WriteLine("total take {0} to here2", watch1.Elapsed.TotalMinutes);
            Assert.False(fail);
        }

        Console.WriteLine("Finally, total take {0} to here2", watch1.Elapsed.TotalMinutes);
    }

    #endregion

    #region Indexes

    [Trait("acceptance-test", "Splunk.Client.Index")]
    [Fact]
    public async Task CanCreateIndex()
    {
        using var service = await SdkHelper.CreateService(new Namespace("nobody", "search"));
        var indexName = string.Format("delete-me-{0}", Guid.NewGuid());

        var index = await service.Indexes.CreateAsync(indexName);
        var foundIndex = await service.Indexes.GetAsync(indexName);
        Assert.Equal(indexName, foundIndex.Title);
    }

    [Trait("acceptance-test", "Splunk.Client.Index")]
    [MockContext]
    [Fact]
    public async Task CanCrudIndex()
    {
        var ns = new Namespace("nobody", "search");

        using var service = await SdkHelper.CreateService(ns);
        var indexName = MockContext.GetOrElse(string.Format("delete-me-{0}", Guid.NewGuid()));
        Client.Index index;

        //// Create
        index = await service.Indexes.CreateAsync(indexName);
        Assert.Equal(true, index.EnableOnlineBucketRepair);

        //// Read
        index = await service.Indexes.GetAsync(indexName);

        //// Update
        var attributes = new IndexAttributes()
        {
            EnableOnlineBucketRepair = false
        };

        _ = await index.UpdateAsync(attributes);
        Assert.Equal(attributes.EnableOnlineBucketRepair, index.EnableOnlineBucketRepair);

        await index.DisableAsync();
        Assert.Equal(true, index.Disabled);

        await service.Server.RestartAsync(2 * 60 * 1000);
        await service.LogOnAsync();

        await index.EnableAsync();
        Assert.Equal(false, index.Disabled);

        await service.Server.RestartAsync(2 * 60 * 1000);
        await service.LogOnAsync();

        //// Delete
        await index.RemoveAsync();
        await SdkHelper.ThrowsAsync<ResourceNotFoundException>(async () => await index.GetAsync());
    }

    [Trait("acceptance-test", "Splunk.Client.IndexCollection")]
    [MockContext]
    [Fact]
    public async Task CanGetIndexes()
    {
        using var service = await SdkHelper.CreateService(new Namespace(user: "nobody", app: "search"));
        await service.Indexes.GetAllAsync();

        foreach (var entity in service.Indexes)
        {
            await entity.GetAsync();

            Assert.Equal(entity.ToString(), entity.Id.ToString());
            _ = entity.AssureUTF8;
            _ = entity.BloomFilterTotalSizeKB;
            _ = entity.BucketRebuildMemoryHint;
            _ = entity.ColdPath;
            _ = entity.ColdPathExpanded;
            _ = entity.ColdToFrozenDir;
            _ = entity.ColdToFrozenScript;
            _ = entity.CurrentDBSizeMB;
            _ = entity.DefaultDatabase;
            _ = entity.Disabled;
            _ = entity.Eai;
            _ = entity.EnableOnlineBucketRepair;
            _ = entity.EnableRealTimeSearch;
            _ = entity.FrozenTimePeriodInSecs;
            _ = entity.HomePath;
            _ = entity.HomePathExpanded;
            _ = entity.IndexThreads;
            _ = entity.IsInternal;
            _ = entity.IsReady;
            _ = entity.IsVirtual;
            _ = entity.LastInitSequenceNumber;
            _ = entity.LastInitTime;
            _ = entity.MaxBloomBackfillBucketAge;
            _ = entity.MaxBucketSizeCacheEntries;
            _ = entity.MaxConcurrentOptimizes;
            _ = entity.MaxDataSize;
            _ = entity.MaxHotBuckets;
            _ = entity.MaxHotIdleSecs;
            _ = entity.MaxHotSpanSecs;
            _ = entity.MaxMemMB;
            _ = entity.MaxMetaEntries;
            _ = entity.MaxRunningProcessGroups;
            _ = entity.MaxRunningProcessGroupsLowPriority;
            _ = entity.MaxTime;
            _ = entity.MaxTimeUnreplicatedNoAcks;
            _ = entity.MaxTimeUnreplicatedWithAcks;
            _ = entity.MaxTotalDataSizeMB;
            _ = entity.MaxWarmDBCount;
            _ = entity.MemPoolMB;
            _ = entity.MinRawFileSyncSecs;
            _ = entity.MinTime;
            _ = entity.PartialServiceMetaPeriod;
            _ = entity.ProcessTrackerServiceInterval;
            _ = entity.QuarantineFutureSecs;
            _ = entity.QuarantinePastSecs;
            _ = entity.RawChunkSizeBytes;
            _ = entity.RepFactor;
            _ = entity.RotatePeriodInSecs;
            _ = entity.ServiceMetaPeriod;
            _ = entity.ServiceOnlyAsNeeded;
            _ = entity.ServiceSubtaskTimingPeriod;
            _ = entity.SummaryHomePathExpanded;
            _ = entity.Sync;
            _ = entity.SyncMeta;
            _ = entity.ThawedPath;
            _ = entity.ThawedPathExpanded;
            _ = entity.ThrottleCheckPeriod;
            _ = entity.TotalEventCount;
            _ = entity.TStatsHomePath;
            _ = entity.TStatsHomePathExpanded;

            var sameEntity = await service.Indexes.GetAsync(entity.ResourceName.Title);

            Assert.Equal(entity.ResourceName, sameEntity.ResourceName);

            Assert.Equal(entity.AssureUTF8, sameEntity.AssureUTF8);
            Assert.Equal(entity.BloomFilterTotalSizeKB, sameEntity.BloomFilterTotalSizeKB);
            Assert.Equal(entity.BucketRebuildMemoryHint, sameEntity.BucketRebuildMemoryHint);
            Assert.Equal(entity.ColdPath, sameEntity.ColdPath);
            Assert.Equal(entity.ColdPathExpanded, sameEntity.ColdPathExpanded);
            Assert.Equal(entity.ColdToFrozenDir, sameEntity.ColdToFrozenDir);
            Assert.Equal(entity.ColdToFrozenScript, sameEntity.ColdToFrozenScript);
            Assert.Equal(entity.CurrentDBSizeMB, sameEntity.CurrentDBSizeMB);
            Assert.Equal(entity.DefaultDatabase, sameEntity.DefaultDatabase);
            Assert.Equal(entity.Disabled, sameEntity.Disabled);
            // Assert.Equal(entity.Eai, sameEntity.Eai); // TODO: verify this property setting (?)
            Assert.Equal(entity.EnableOnlineBucketRepair, sameEntity.EnableOnlineBucketRepair);
            Assert.Equal(entity.EnableRealTimeSearch, sameEntity.EnableRealTimeSearch);
            Assert.Equal(entity.FrozenTimePeriodInSecs, sameEntity.FrozenTimePeriodInSecs);
            Assert.Equal(entity.HomePath, sameEntity.HomePath);
            Assert.Equal(entity.HomePathExpanded, sameEntity.HomePathExpanded);
            Assert.Equal(entity.IndexThreads, sameEntity.IndexThreads);
            Assert.Equal(entity.IsInternal, sameEntity.IsInternal);
            Assert.Equal(entity.IsReady, sameEntity.IsReady);
            Assert.Equal(entity.IsVirtual, sameEntity.IsVirtual);
            Assert.Equal(entity.LastInitSequenceNumber, sameEntity.LastInitSequenceNumber);
            Assert.Equal(entity.LastInitTime, sameEntity.LastInitTime);
            Assert.Equal(entity.MaxBloomBackfillBucketAge, sameEntity.MaxBloomBackfillBucketAge);
            Assert.Equal(entity.MaxBucketSizeCacheEntries, sameEntity.MaxBucketSizeCacheEntries);
            Assert.Equal(entity.MaxConcurrentOptimizes, sameEntity.MaxConcurrentOptimizes);
            Assert.Equal(entity.MaxDataSize, sameEntity.MaxDataSize);
            Assert.Equal(entity.MaxHotBuckets, sameEntity.MaxHotBuckets);
            Assert.Equal(entity.MaxHotIdleSecs, sameEntity.MaxHotIdleSecs);
            Assert.Equal(entity.MaxHotSpanSecs, sameEntity.MaxHotSpanSecs);
            Assert.Equal(entity.MaxMemMB, sameEntity.MaxMemMB);
            Assert.Equal(entity.MaxMetaEntries, sameEntity.MaxMetaEntries);
            Assert.Equal(entity.MaxRunningProcessGroups, sameEntity.MaxRunningProcessGroups);
            Assert.Equal(entity.MaxRunningProcessGroupsLowPriority, sameEntity.MaxRunningProcessGroupsLowPriority);
            Assert.True((sameEntity.MaxTime - entity.MaxTime) <= new TimeSpan(0, 0, 20));//expect the test finish run within 20s which means new events comes only within 20s
            Assert.Equal(entity.MaxTimeUnreplicatedNoAcks, sameEntity.MaxTimeUnreplicatedNoAcks);
            Assert.Equal(entity.MaxTimeUnreplicatedWithAcks, sameEntity.MaxTimeUnreplicatedWithAcks);
            Assert.Equal(entity.MaxTotalDataSizeMB, sameEntity.MaxTotalDataSizeMB);
            Assert.Equal(entity.MaxWarmDBCount, sameEntity.MaxWarmDBCount);
            Assert.Equal(entity.MemPoolMB, sameEntity.MemPoolMB);
            Assert.Equal(entity.MinRawFileSyncSecs, sameEntity.MinRawFileSyncSecs);
            Assert.Equal(entity.MinTime, sameEntity.MinTime);
            Assert.Equal(entity.PartialServiceMetaPeriod, sameEntity.PartialServiceMetaPeriod);
            Assert.Equal(entity.ProcessTrackerServiceInterval, sameEntity.ProcessTrackerServiceInterval);
            Assert.Equal(entity.QuarantineFutureSecs, sameEntity.QuarantineFutureSecs);
            Assert.Equal(entity.QuarantinePastSecs, sameEntity.QuarantinePastSecs);
            Assert.Equal(entity.RawChunkSizeBytes, sameEntity.RawChunkSizeBytes);
            Assert.Equal(entity.RepFactor, sameEntity.RepFactor);
            Assert.Equal(entity.RotatePeriodInSecs, sameEntity.RotatePeriodInSecs);
            Assert.Equal(entity.ServiceMetaPeriod, sameEntity.ServiceMetaPeriod);
            Assert.Equal(entity.ServiceOnlyAsNeeded, sameEntity.ServiceOnlyAsNeeded);
            Assert.Equal(entity.ServiceSubtaskTimingPeriod, sameEntity.ServiceSubtaskTimingPeriod);
            Assert.Equal(entity.SummaryHomePathExpanded, sameEntity.SummaryHomePathExpanded);
            Assert.Equal(entity.Sync, sameEntity.Sync);
            Assert.Equal(entity.SyncMeta, sameEntity.SyncMeta);
            Assert.Equal(entity.ThawedPath, sameEntity.ThawedPath);
            Assert.Equal(entity.ThawedPathExpanded, sameEntity.ThawedPathExpanded);
            Assert.Equal(entity.ThrottleCheckPeriod, sameEntity.ThrottleCheckPeriod);
            Assert.Equal(entity.TStatsHomePath, sameEntity.TStatsHomePath);
            Assert.Equal(entity.TStatsHomePathExpanded, sameEntity.TStatsHomePathExpanded);
        }
    }

    #endregion

    #region Inputs

    [Trait("acceptance-test", "Splunk.Client.Transmitter")]
    [MockContext]
    [Fact]
    public async Task CanSendEvents()
    {
        using var service = await SdkHelper.CreateService();
        // default index

        var index = await service.Indexes.GetAsync("main");
        Assert.NotNull(index);
        Assert.False(index.Disabled);

        var receiver = service.Transmitter;

        var currentEventCount = index.TotalEventCount;
        var sendEventCount = 10;

        for (var i = 0; i < sendEventCount; i++)
        {
            var result = await receiver.SendAsync(
                MockContext.GetOrElse(string.Format("{0:D6} {1} Simple event", i, DateTime.Now)));
            Assert.NotNull(result);
        }

        var watch = Stopwatch.StartNew();

        while (watch.Elapsed < new TimeSpan(0, 0, 120) && index.TotalEventCount != currentEventCount + sendEventCount)
        {
            await Task.Delay(1000);
            await index.GetAsync();
        }

        Console.WriteLine("After send {0} string events, Current Index TotalEventCount = {1} ", sendEventCount, index.TotalEventCount);
        Console.WriteLine("Sleep {0}s to wait index.TotalEventCount got updated", watch.Elapsed);
        Assert.True(index.TotalEventCount == currentEventCount + sendEventCount);

        // Test stream events

        currentEventCount += sendEventCount;

        using (var eventStream = new MemoryStream())
        {
            using (var writer = new StreamWriter(eventStream, Encoding.UTF8, 4096, leaveOpen: true))
            {
                for (var i = 0; i < sendEventCount; i++)
                {
                    writer.Write(
                        MockContext.GetOrElse(string.Format("{0:D6}, {1}, Stream event\r\n", i, DateTime.Now)));
                }
            }

            _ = eventStream.Seek(0, SeekOrigin.Begin);
            await receiver.SendAsync(eventStream);
        }

        watch.Restart();

        while (watch.Elapsed < new TimeSpan(0, 0, 120) && index.TotalEventCount != currentEventCount + sendEventCount)
        {
            await Task.Delay(1000);
            await index.GetAsync();
        }

        Console.WriteLine("After send {0} stream events, Current Index TotalEventCount = {1} ", sendEventCount, index.TotalEventCount);
        Console.WriteLine("Sleep {0}s to wait index.TotalEventCount got updated", watch.Elapsed);
        Assert.True(index.TotalEventCount == currentEventCount + sendEventCount);
    }

    #endregion

    #region Search

    [Trait("acceptance-test", "Splunk.Client.Job")]
    [MockContext]
    [Fact]
    public async Task CanCrudJob()
    {
        using var service = await SdkHelper.CreateService();
        Job job1 = null, job2 = null;

        //// Create

        job1 = await service.Jobs.CreateAsync("search index=_internal | head 100");

        using (var stream = await job1.GetSearchEventsAsync())
        {
        }

        using (var stream = await job1.GetSearchPreviewAsync())
        {
        }

        using (var stream = await job1.GetSearchResultsAsync())
        {
        }

        //// Read

        job2 = await service.Jobs.GetAsync(job1.ResourceName.Title);

        Assert.Equal(job1.ResourceName.Title, job2.ResourceName.Title);
        Assert.Equal(job1.Name, job1.ResourceName.Title);
        Assert.Equal(job1.Name, job2.Name);
        Assert.Equal(job1.Sid, job1.Name);
        Assert.Equal(job1.Sid, job2.Sid);
        Assert.Equal(job1.Id, job2.Id);
        //Assert.Equal(new SortedDictionary<string, Uri>().Concat(job1.Links), new SortedDictionary<string, Uri>().Concat(job2.Links));

        //// Update

        var updatedSnapshot = await job1.UpdateAsync(new CustomJobArgs
                {
                    new Argument("foo", 1),
                    new Argument("bar", 2)
                });

        Assert.True(updatedSnapshot);

        //// Delete

        await job1.RemoveAsync();

        try
        {
            await job1.GetAsync();
            Assert.True(false);
        }
        catch (ResourceNotFoundException)
        { }

        try
        {
            _ = await service.Jobs.GetAsync(job1.Name);
            Assert.True(false);
        }
        catch (ResourceNotFoundException)
        { }

        job2 = await service.Jobs.GetOrNullAsync(job1.Name);
        Assert.Null(job2);
    }

    [Trait("acceptance-test", "Splunk.Client.SavedSearch")]
    [MockContext]
    [Fact]
    public async Task CanCrudSavedSearch()
    {
        using var service = await SdkHelper.CreateService();
        //// Create

        var name = MockContext.GetOrElse(string.Format("delete-me-{0}", Guid.NewGuid()));
        var search = "search index=_internal | head 1000";

        var originalAttributes = new SavedSearchAttributes
        {
            CronSchedule = "00 * * * *", // on the hour
            IsScheduled = true,
            IsVisible = false
        };

        var savedSearch = await service.SavedSearches.CreateAsync(name, search, originalAttributes);

        Assert.Equal(search, savedSearch.Search);
        Assert.Equal(originalAttributes.CronSchedule, savedSearch.CronSchedule);
        Assert.Equal(originalAttributes.IsScheduled, savedSearch.IsScheduled);
        Assert.Equal(originalAttributes.IsVisible, savedSearch.IsVisible);

        //// Read

        savedSearch = await service.SavedSearches.GetAsync(name);
        Assert.Equal(false, savedSearch.IsVisible);

        //// Read history

        var jobHistory = await savedSearch.GetHistoryAsync();
        Assert.Empty(jobHistory);

        var job1 = await savedSearch.DispatchAsync();

        jobHistory = await savedSearch.GetHistoryAsync();
        Assert.Equal(1, jobHistory.Count);
        Assert.Equal(job1, jobHistory[0]);
        Assert.Equal(job1.Name, jobHistory[0].Name);
        Assert.Equal(job1.ResourceName, jobHistory[0].ResourceName);
        Assert.Equal(job1.Sid, jobHistory[0].Sid);

        var job2 = await savedSearch.DispatchAsync();

        jobHistory = await savedSearch.GetHistoryAsync();
        Assert.Equal(2, jobHistory.Count);
        Assert.Equal(1, jobHistory.Select(job => job).Where(job => job.Equals(job1)).Count());
        Assert.Equal(1, jobHistory.Select(job => job).Where(job => job.Equals(job2)).Count());

        await job1.CancelAsync();

        jobHistory = await savedSearch.GetHistoryAsync();
        Assert.Equal(1, jobHistory.Count);
        Assert.Equal(job2, jobHistory[0]);
        Assert.Equal(job2.Name, jobHistory[0].Name);
        Assert.Equal(job2.ResourceName, jobHistory[0].ResourceName);
        Assert.Equal(job2.Sid, jobHistory[0].Sid);

        await job2.CancelAsync();

        jobHistory = await savedSearch.GetHistoryAsync();
        Assert.Empty(jobHistory);

        //// Read schedule

        var dateTime = MockContext.GetOrElse(DateTime.Now);

        var schedule = await savedSearch.GetScheduledTimesAsync("0", "+2d");
        Assert.Equal(48, schedule.Count);

        var expected = dateTime.AddMinutes(60);
        expected = expected.Date.AddHours(expected.Hour);
        Assert.Equal(expected, schedule[0]);

        //// Update

        search = "search index=_internal * earliest=-1m";

        var updatedAttributes = new SavedSearchAttributes
        {
            ActionEmailBcc = "user1@splunk.com",
            ActionEmailCC = "user2@splunk.com",
            ActionEmailFrom = "user3@splunk.com",
            ActionEmailTo = "user4@splunk.com, user5@splunk.com",
            IsVisible = true
        };

        _ = await savedSearch.UpdateAsync(search, updatedAttributes);

        Assert.Equal(search, savedSearch.Search);
        Assert.Equal(updatedAttributes.ActionEmailBcc, savedSearch.Actions.Email.Bcc);
        Assert.Equal(updatedAttributes.ActionEmailCC, savedSearch.Actions.Email.CC);
        Assert.Equal(updatedAttributes.ActionEmailFrom, savedSearch.Actions.Email.From);
        Assert.Equal(updatedAttributes.ActionEmailTo, savedSearch.Actions.Email.To);
        Assert.Equal(updatedAttributes.IsVisible, savedSearch.IsVisible);

        //// Update schedule

        dateTime = MockContext.GetOrElse(DateTime.Now);

        //// TODO:
        //// Figure out why POST saved/searches/{name}/reschedule ignores schedule_time and runs the
        //// saved searches right away. Are we using the right time format?

        //// TODO:
        //// Figure out how to parse or--more likely--complain that savedSearch.NextScheduledTime uses
        //// timezone names like "Pacific Daylight Time".

        await savedSearch.ScheduleAsync(dateTime.AddMinutes(15)); // Does not return anything but status
        _ = await savedSearch.GetScheduledTimesAsync("0", "+2d");

        //// Delete

        await savedSearch.RemoveAsync();
    }

    [Trait("acceptance-test", "Splunk.Client.Service")]
    [MockContext]
    [Fact]
    public async Task CanDispatchSavedSearch()
    {
        using var service = await SdkHelper.CreateService();
        var job = await service.DispatchSavedSearchAsync("Splunk errors last 24 hours");

        using var stream = await job.GetSearchResultsAsync();
        var results = new List<SearchResult>();

        foreach (var result in stream)
        {
            results.Add(result);
        }

        Assert.NotEmpty(results);
    }

    [Trait("acceptance-test", "Splunk.Client.JobCollection")]
    [MockContext]
    [Fact]
    public async Task CanGetJobs()
    {
        using var service = await SdkHelper.CreateService();
        var jobs = new Job[]
        {
                await service.Jobs.CreateAsync("search index=_internal | head 10"),
                await service.Jobs.CreateAsync("search index=_internal | head 10"),
                await service.Jobs.CreateAsync("search index=_internal | head 10"),
                await service.Jobs.CreateAsync("search index=_internal | head 10"),
                await service.Jobs.CreateAsync("search index=_internal | head 10"),
        };

        await service.Jobs.GetAllAsync();
        Assert.True(service.Jobs.Count >= jobs.Length);

        foreach (var job in jobs)
        {
            Assert.Contains(job, service.Jobs);
        }

        var sequence = new List<Job>(service.Jobs.Count);

        for (var i = 0; i < service.Jobs.Count; i++)
        {
            sequence.Add(service.Jobs[i]);
        }

        Assert.Equal(service.Jobs.ToList(), sequence);
    }

    [Trait("acceptance-test", "Splunk.Client.SavedSearchCollection")]
    [MockContext]
    [Fact]
    public async Task CanGetSavedSearches()
    {
        using var service = await SdkHelper.CreateService();
        var savedSearches = service.SavedSearches;
        await savedSearches.GetAllAsync();

        foreach (var savedSearch in savedSearches)
        {
        }

        for (var i = 0; i < savedSearches.Count; i++)
        {
            var savedSearch = savedSearches[i];
        }
    }

    [Trait("acceptance-test", "Splunk.Client.Service")]
    [MockContext]
    [Fact]
    public async Task CanCancelExportSearchPreviews()
    {
        using var service = await SdkHelper.CreateService();
        const string search = "search index=_internal | tail 1000 | stats count by method";
        var args = new SearchExportArgs { Count = 0, EarliestTime = "-24h" };

        using (var stream = await service.ExportSearchPreviewsAsync(search, args))
        { }

        using (var stream = await service.ExportSearchPreviewsAsync(search, args))
        {
            for (var i = 0; stream.ReadCount <= 0; i++)
            {
                await Task.Delay(10);
            }
        }

        await service.LogOffAsync();
    }

    [Trait("acceptance-test", "Splunk.Client.Service")]
    [MockContext]
    [Fact]
    public async Task CanCancelExportSearchResults()
    {
        using var service = await SdkHelper.CreateService();
        const string search = "search index=_internal | tail 100";
        var args = new SearchExportArgs { Count = 0 };

        using (var stream = await service.ExportSearchResultsAsync(search, args))
        { }

        using (var stream = await service.ExportSearchResultsAsync(search, args))
        {
            for (var i = 0; stream.ReadCount <= 0; i++)
            {
                await Task.Delay(10);
            }
        }

        await service.LogOffAsync();
    }

    [Trait("acceptance-test", "Splunk.Client.Service")]
    [MockContext]
    [Fact]
    public async Task CanExportSearchPreviewsToEnumerable()
    {
        using var service = await SdkHelper.CreateService();
        const string search = "search index=_internal | stats count by method";
        var args = new SearchExportArgs() { Count = 0, EarliestTime = "-1d", MaxCount = 5000 };

        var watch = new Stopwatch();
        watch.Start();

        using (var stream = await service.ExportSearchPreviewsAsync(search, args))
        {
            var results = new List<SearchResult>();

            foreach (var preview in stream)
            {
                Assert.Equal<IEnumerable<string>>(new ReadOnlyCollection<string>(new string[]
                    {
                            "method",
                            "count",
                    }),
                    preview.FieldNames);

                if (preview.IsFinal)
                {
                    results.AddRange(preview.Results);
                }
            }

            watch.Stop();

            Console.WriteLine("spent {0} to read all stream", watch.Elapsed.TotalSeconds);
            Console.WriteLine("stream.ReadCount={0}", stream.ReadCount);

            Assert.True(stream.ReadCount >= 1);
            Assert.NotEmpty(results);
        }

        await service.LogOffAsync();
    }

    [Trait("acceptance-test", "Splunk.Client.Service")]
    [MockContext]
    [Fact]
    public async Task CanExportSearchPreviewsToObservable()
    {
        using var service = await SdkHelper.CreateService();
        const string search = "search index=_internal | stats count by method";
        var args = new SearchExportArgs() { Count = 0, EarliestTime = "-24h", MaxCount = 5000 };

        using (var stream = await service.ExportSearchPreviewsAsync(search, args))
        {
            var manualResetEvent = new ManualResetEvent(true);
            var results = new List<SearchResult>();
            var exception = (Exception)null;

            _ = stream.Subscribe(new Observer<SearchPreview>(
                onNext: (preview) =>
                {
                    Assert.Equal<IEnumerable<string>>(new ReadOnlyCollection<string>(new string[]
                        {
                                "method",
                                "count",
                        }),
                        preview.FieldNames);

                    if (preview.IsFinal)
                    {
                        results.AddRange(preview.Results);
                    }
                },
                onCompleted: () => manualResetEvent.Set(),
                onError: (e) =>
                {
                    exception = new ApplicationException("SearchPreviewStream error: " + e.Message, e);
                    _ = manualResetEvent.Set();
                }));

            _ = manualResetEvent.Reset();
            _ = manualResetEvent.WaitOne();

            Assert.Null(exception);
            Assert.NotEmpty(results);
            Assert.True(stream.ReadCount >= 1);
        }

        await service.LogOffAsync();
    }

    [Trait("acceptance-test", "Splunk.Client.Service")]
    [MockContext]
    [Fact]
    public async Task CanExportSearchResultsToEnumerable()
    {
        //random failed in CI, so try several time to see if it improves

        using var service = await SdkHelper.CreateService();
        const string search = "search index=_internal| head 3";
        // do some trivial search to make sure _internal have some records
        _ = await service.SearchOneShotAsync(search);
        _ = await service.SearchOneShotAsync(search);
        _ = await service.SearchOneShotAsync(search);
        await Task.Delay(3000);

        using (var stream = await service.SearchOneShotAsync(search))
        {
            foreach (var result in stream)
            {
                Console.WriteLine(result);
            }

            Assert.Equal(3, stream.ReadCount);
        }

        var args = new SearchExportArgs { Count = 0 };

        using (var stream = await service.ExportSearchResultsAsync(search, args))
        {
            await Task.Delay(3000);

            var results = new List<SearchResult>();

            //wait till results are ready
            foreach (var result in stream)
            {
                results.Add(result);
            }

            Assert.True(results.Count == 3, string.Format("get result count ={0}", results.Count));

        }

        await service.LogOffAsync();

    }

    [Trait("acceptance-test", "Splunk.Client.Service")]
    [MockContext]
    [Fact]
    public async Task CanExportSearchResultsToObservable()
    {

        using var service = await SdkHelper.CreateService();
        var args = new SearchExportArgs();
        const string search = "search index=_internal| head 3";
        // do some trivial search to make sure _internal have some records
        _ = await service.SearchOneShotAsync(search);
        _ = await service.SearchOneShotAsync(search);
        _ = await service.SearchOneShotAsync(search);
        await Task.Delay(2000);

        using (var stream = await service.SearchOneShotAsync(search))
        {
            foreach (var result in stream)
            {
                Console.WriteLine(result);
            }

            Assert.Equal(3, stream.ReadCount);
        }

        var task = service.ExportSearchResultsAsync(search, args);

        //set a timeout task to prevent the test running too long
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(60));
        var completionSource = new TaskCompletionSource<object>();
        _ = source.Token.Register(() => completionSource.TrySetCanceled());
        _ = await Task.WhenAny(task, completionSource.Task);

        if (!task.IsCompleted)
        {
            Console.WriteLine("timeout !!!!{0}", task.Status);
            Assert.Fail("test failed due to timeout");
        }

        await Task.Delay(2000);
        using (var stream = task.Result)
        {
            var manualResetEvent = new ManualResetEvent(true);
            var results = new List<SearchResult>();
            var exception = (Exception)null;
            var readCount = 0;

            _ = stream.Subscribe(new Observer<SearchResult>(
                onNext: (result) =>
                {
                    var memberNames = result.GetDynamicMemberNames();
                    var count = stream.FieldNames.Intersect(memberNames).Count();
                    Assert.Equal(count, memberNames.Count());

                    if (stream.IsFinal)
                    {
                        results.Add(result);
                    }

                    readCount++;
                },
                onCompleted: () => manualResetEvent.Set(),
                onError: (e) =>
                {
                    exception = new ApplicationException("SearchPreviewStream error: " + e.Message, e);
                    _ = manualResetEvent.Set();
                }));

            _ = manualResetEvent.Reset();
            //manualResetEvent.WaitOne();

            Assert.Null(exception);
            Assert.True(stream.IsFinal);
            Assert.Equal(3, results.Count);
            Assert.Equal(stream.ReadCount, readCount);
        }

        await service.LogOffAsync();
    }

    [Trait("acceptance-test", "Splunk.Client.Service")]
    [MockContext]
    [Fact]
    public async Task CanSearchOneshot()
    {
        using var service = await SdkHelper.CreateService();
        var indexName = MockContext.GetOrElse(string.Format("delete-me-{0}", Guid.NewGuid().ToString("N")));

        var searches = new[]
        {
                new
                {
                    Command = string.Format("search index={0} * | delete", indexName),
                    ResultCount = 0
                },
                new
                {
                    Command = "search index=_internal | head 1000",
                    ResultCount = 1000
                }
            };

        _ = await service.Indexes.CreateAsync(indexName);

        foreach (var search in searches)
        {
            using var stream = await service.SearchOneShotAsync(search.Command, count: 0);
            var list = new List<SearchResult>();

            foreach (var result in stream)
            {
                list.Add(result);
            }

            Assert.Equal(search.ResultCount, stream.ReadCount);
            Assert.Equal(search.ResultCount, list.Count);
        }
    }

    #endregion

    #region System

    [Trait("acceptance-test", "Splunk.Client.ServerMessage")]
    [MockContext]
    [Fact]
    public async Task CanCrudServerMessage()
    {
        using var service = await SdkHelper.CreateService();
        //// Create

        var name = MockContext.GetOrElse(string.Format("delete-me-{0}", Guid.NewGuid()));
        var messages = service.Server.Messages;

        var messageList = new ServerMessage[]
        {
                await messages.CreateAsync(string.Format("{0}-{1}", name, ServerMessageSeverity.Information), ServerMessageSeverity.Information, "some message text"),
                await messages.CreateAsync(string.Format("{0}-{1}", name, ServerMessageSeverity.Warning), ServerMessageSeverity.Warning, "some message text"),
                await messages.CreateAsync(string.Format("{0}-{1}", name, ServerMessageSeverity.Error), ServerMessageSeverity.Error, "some message text")
        };

        //// Read

        await messages.GetAllAsync();

        foreach (var message in messageList)
        {
            Assert.NotNull(messages.SingleOrDefault(m => m.Name == message.Name));
            var messageCopy = await messages.GetAsync(message.Name);
            Assert.Equal(message, messageCopy);
            await message.GetAsync();
        }

        //// Delete (there is no update)

        foreach (var message in messages)
        {
            if (message.Name.StartsWith("delete-me-"))
            {
                await message.RemoveAsync();
            }
        }

        //// Verify delete

        await messages.GetAllAsync();

        foreach (var message in messages)
        {
            Assert.False(message.Name.StartsWith("delete-me-"));
        }
    }

    [Trait("acceptance-test", "Splunk.Client.ServerSettings")]
    [MockContext]
    [Fact]
    public async Task CanCrudServerSettings()
    {
        using var service = await SdkHelper.CreateService();
        //// Get

        var originalSettings = await service.Server.GetSettingsAsync();

        var originalValues = new ServerSettingValues
        {
            EnableSplunkWebSsl = originalSettings.EnableSplunkWebSsl,
            Host = originalSettings.Host,
            HttpPort = originalSettings.HttpPort,
            ManagementHostPort = originalSettings.ManagementHostPort,
            MinFreeSpace = originalSettings.MinFreeSpace,
            Pass4SymmetricKey = originalSettings.Pass4SymmetricKey,
            ServerName = originalSettings.ServerName,
            SessionTimeout = originalSettings.SessionTimeout,
            SplunkDB = originalSettings.SplunkDB,
            StartWebServer = originalSettings.StartWebServer,
            TrustedIP = originalSettings.TrustedIP
        };

        //// Update

        try
        {
            var updatedValues = new ServerSettingValues
            {
                EnableSplunkWebSsl = !originalSettings.EnableSplunkWebSsl,
                Host = originalSettings.Host,
                HttpPort = originalSettings.HttpPort + 1,
                ManagementHostPort = originalSettings.ManagementHostPort,
                MinFreeSpace = originalSettings.MinFreeSpace - 1,
                Pass4SymmetricKey = originalSettings.Pass4SymmetricKey + "-update",
                ServerName = originalSettings.ServerName,
                SessionTimeout = "2h",
                SplunkDB = originalSettings.SplunkDB,
                StartWebServer = !originalSettings.StartWebServer,
                TrustedIP = originalSettings.TrustedIP
            };

            var updatedSettings = await service.Server.UpdateSettingsAsync(updatedValues);

            Assert.Equal(updatedValues.EnableSplunkWebSsl, updatedSettings.EnableSplunkWebSsl);
            Assert.Equal(updatedValues.Host, updatedSettings.Host);
            Assert.Equal(updatedValues.HttpPort, updatedSettings.HttpPort);
            Assert.Equal(updatedValues.ManagementHostPort, updatedSettings.ManagementHostPort);
            Assert.Equal(updatedValues.MinFreeSpace, updatedSettings.MinFreeSpace);
            Assert.Equal(updatedValues.Pass4SymmetricKey, updatedSettings.Pass4SymmetricKey);
            Assert.Equal(updatedValues.ServerName, updatedSettings.ServerName);
            Assert.Equal(updatedValues.SessionTimeout, updatedSettings.SessionTimeout);
            Assert.Equal(updatedValues.SplunkDB, updatedSettings.SplunkDB);
            Assert.Equal(updatedValues.StartWebServer, updatedSettings.StartWebServer);
            Assert.Equal(updatedValues.TrustedIP, updatedSettings.TrustedIP);

            //// Restart the server because it's required following a server settings update

            await service.Server.RestartAsync(2 * 60 * 1000);
            await service.LogOnAsync();

        }
        catch (Exception e1)
        {
            try
            {
                service.Server.UpdateSettingsAsync(originalValues).Wait(); // because you can't await in catch block
            }
            catch (Exception e2)
            {
                throw new AggregateException(e1, e2);
            }

            throw;
        }

        //// Restore

        originalSettings = await service.Server.UpdateSettingsAsync(originalValues);

        Assert.Equal(originalValues.EnableSplunkWebSsl, originalSettings.EnableSplunkWebSsl);
        Assert.Equal(originalValues.Host, originalSettings.Host);
        Assert.Equal(originalValues.HttpPort, originalSettings.HttpPort);
        Assert.Equal(originalValues.ManagementHostPort, originalSettings.ManagementHostPort);
        Assert.Equal(originalValues.MinFreeSpace, originalSettings.MinFreeSpace);
        Assert.Equal(originalValues.Pass4SymmetricKey, originalSettings.Pass4SymmetricKey);
        Assert.Equal(originalValues.ServerName, originalSettings.ServerName);
        Assert.Equal(originalValues.SessionTimeout, originalSettings.SessionTimeout);
        Assert.Equal(originalValues.SplunkDB, originalSettings.SplunkDB);
        Assert.Equal(originalValues.StartWebServer, originalSettings.StartWebServer);
        Assert.Equal(originalValues.TrustedIP, originalSettings.TrustedIP);

        //// Restart the server because it's required following a settings update
        await service.Server.RestartAsync(2 * 60 * 1000);
    }

    [Trait("acceptance-test", "Splunk.Client.Server")]
    [MockContext]
    [Fact]
    public async Task CanGetServerInfo()
    {
        using var service = await SdkHelper.CreateService();
        var info = await service.Server.GetInfoAsync();

        var acl = info.Eai.Acl;
        var permissions = acl.Permissions;
        var build = info.Build;
        var cpuArchitecture = info.CpuArchitecture;
        var guid = info.Guid;
        var isFree = info.IsFree;
        var isRealtimeSearchEnabled = info.IsRealTimeSearchEnabled;
        var isTrial = info.IsTrial;
        var licenseKeys = info.LicenseKeys;
        var licenseLabels = info.LicenseLabels;
        var licenseSignature = info.LicenseSignature;
        var licenseState = info.LicenseState;
        var masterGuid = info.MasterGuid;
        var mode = info.Mode;
        var osBuild = info.OSBuild;
        var osName = info.OSName;
        var osVersion = info.OSVersion;
        var serverName = info.ServerName;
        var version = info.Version;
    }

    [Trait("acceptance-test", "Splunk.Client.Server")]
    [MockContext]
    [Fact]
    public async Task CanRestartServer()
    {
        var watch = Stopwatch.StartNew();

        using var service = await SdkHelper.CreateService();
        try
        {
            await service.Server.RestartAsync(2 * 60 * 1000);
        }
        catch (OperationCanceledException e)
        {
            Console.WriteLine("----------------------------------------------------------------------------------------");
            Console.WriteLine("{1}, spend {0}s to restart server failed:", watch.Elapsed.TotalSeconds, DateTime.Now);
            Console.WriteLine(e);
            Console.WriteLine("----------------------------------------------------------------------------------------");
        }

        Assert.Null(service.SessionKey);
        await service.LogOnAsync();
    }

    #endregion

    #region Privates/internals

    private static readonly ReadOnlyCollection<Namespace> TestNamespaces = new(new Namespace[]
    {
        Namespace.Default,
        new Namespace("admin", "search"),
        new Namespace("nobody", "search")
    });

    #endregion
}
