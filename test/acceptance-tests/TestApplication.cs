/*
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

namespace Splunk.Client.AcceptanceTests;

using System;
using System.Threading.Tasks;
using Splunk.Client;
using Splunk.Client.Helper;
using Xunit;

/// <summary>
/// Application tests
/// </summary>
public class ApplicationTest
{
    /// <summary>
    /// The app tests
    /// </summary>
    [Trait("acceptance-test", "Splunk.Client.Application")]
    [MockContext]
    [Fact]
    public async Task Application()
    {
        using var service = await SdkHelper.CreateService();
        var apps = service.Applications;
        await apps.GetAllAsync();

        foreach (var app in apps)
        {
            await CheckApplication(app);
        }

        for (var i = 0; i < apps.Count; i++)
        {
            await CheckApplication(apps[i]);
        }

        if (await service.Applications.RemoveAsync("sdk-tests"))
        {
            await service.Server.RestartAsync(2 * 60 * 1000);
            await service.LogOnAsync();
        }

        var attributes = new ApplicationAttributes
        {
            ApplicationAuthor = "me",
            Description = "this is a description",
            Label = "SDKTEST",
            Visible = false
        };

        var testApp = await service.Applications.CreateAsync("sdk-tests", "barebones", attributes);
        testApp = await service.Applications.GetAsync("sdk-tests");

        Assert.Equal("SDKTEST", testApp.Label);
        Assert.Equal("me", testApp.ApplicationAuthor);
        Assert.Equal("nobody", testApp.Author);
        Assert.False(testApp.Configured);
        Assert.False(testApp.Visible);

        var exception = await Record.ExceptionAsync(async () =>
        {
            var p = testApp.CheckForUpdates;
            await Task.CompletedTask;
        });
        Assert.Null(exception);

        attributes = new ApplicationAttributes
        {
            ApplicationAuthor = "not me",
            Description = "new description",
            Label = "new label",
            Visible = false,
            Version = "1.5"
        };

        //// Update the application

        _ = await testApp.UpdateAsync(attributes, true);
        await testApp.GetAsync();

        Assert.Equal("not me", testApp.ApplicationAuthor);
        Assert.Equal("nobody", testApp.Author);
        Assert.Equal("new description", testApp.Description);
        Assert.Equal("new label", testApp.Label);
        Assert.Equal("1.5", testApp.Version);
        Assert.False(testApp.Visible);

        var updateInfo = await testApp.GetUpdateInfoAsync();
        Assert.NotNull(updateInfo.Eai.Acl);

        //// Package the application

        var archiveInfo = await testApp.PackageAsync();

        Assert.Equal("Package", archiveInfo.Title);
        Assert.NotEqual(DateTime.MinValue, archiveInfo.Updated);

        exception = await Record.ExceptionAsync(async () =>
        {
            var p = archiveInfo.ApplicationName;
            await Task.CompletedTask;
        });
        Assert.Null(exception);

        Assert.True(archiveInfo.ApplicationName.Length > 0);

        exception = await Record.ExceptionAsync(async () =>
        {
            var p = archiveInfo.Eai;
            await Task.CompletedTask;
        });
        Assert.Null(exception);

        Assert.NotNull(archiveInfo.Eai);
        Assert.NotNull(archiveInfo.Eai.Acl);

        exception = await Record.ExceptionAsync(async () =>
        {
            var p = archiveInfo.Path;
            await Task.CompletedTask;
        });
        Assert.Null(exception);

        Assert.True(archiveInfo.Path.Length > 0);

        exception = await Record.ExceptionAsync(async () =>
        {
            var p = archiveInfo.Refresh;
            await Task.CompletedTask;
        });
        Assert.Null(exception);

        exception = await Record.ExceptionAsync(async () =>
        {
            var p = archiveInfo.Uri;
            await Task.CompletedTask;
        });
        Assert.Null(exception);

        Assert.True(archiveInfo.Uri.AbsolutePath.Length > 0);

        Assert.True(await service.Applications.RemoveAsync("sdk-tests"));
        await service.Server.RestartAsync(2 * 60 * 1000);
    }

    internal static async Task CheckApplication(Application app)
    {
        ApplicationSetupInfo? setupInfo = null;

        Exception exception;
        try
        {
            setupInfo = await app.GetSetupInfoAsync();

            //// TODO: Install an app which hits this code before this test runs

            Assert.NotNull(setupInfo.Eai);
            exception = await Record.ExceptionAsync(
                async () =>
                {
                    var p = setupInfo.Refresh;
                    await Task.CompletedTask;
                });
            Assert.Null(exception);
        }
        catch (InternalServerErrorException e)
        {
            Assert.Contains("Internal Server Error", e.Message);
        }

        var archiveInfo = await app.PackageAsync();

        exception = await Record.ExceptionAsync(
            async () =>
            {
                var p = app.Author;
                Assert.NotNull(p);
                await Task.CompletedTask;
            });
        Assert.Null(exception);

        exception = await Record.ExceptionAsync(async () => { var p = app.ApplicationAuthor; await Task.CompletedTask; });
        Assert.Null(exception);
        exception = await Record.ExceptionAsync(async () => { var p = app.CheckForUpdates; await Task.CompletedTask; });
        Assert.Null(exception);
        exception = await Record.ExceptionAsync(async () => { var p = app.Description; await Task.CompletedTask; });
        Assert.Null(exception);
        exception = await Record.ExceptionAsync(async () => { var p = app.Label; await Task.CompletedTask; });
        Assert.Null(exception);
        exception = await Record.ExceptionAsync(async () => { var p = app.Refresh; await Task.CompletedTask; });
        Assert.Null(exception);
        exception = await Record.ExceptionAsync(async () => { var p = app.Version; await Task.CompletedTask; });
        Assert.Null(exception);
        exception = await Record.ExceptionAsync(async () => { var p = app.Configured; await Task.CompletedTask; });
        Assert.Null(exception);
        exception = await Record.ExceptionAsync(async () => { var p = app.StateChangeRequiresRestart; await Task.CompletedTask; });
        Assert.Null(exception);
        exception = await Record.ExceptionAsync(async () => { var p = app.Visible; await Task.CompletedTask; });
        Assert.Null(exception);

        var updateInfo = await app.GetUpdateInfoAsync();
        Assert.NotNull(updateInfo.Eai);

        if (updateInfo.Update is not null)
        {
            var update = updateInfo.Update;

            exception = await Record.ExceptionAsync(async () =>
            {
                var p = updateInfo.Update.ApplicationName;
                await Task.CompletedTask;
            });
            Assert.Null(exception);
            exception = await Record.ExceptionAsync(async () =>
            {
                var p = updateInfo.Update.ApplicationUri;
                await Task.CompletedTask;
            });
            Assert.Null(exception);
            exception = await Record.ExceptionAsync(async () =>
            {
                var p = updateInfo.Update.ApplicationName;
                await Task.CompletedTask;
            });
            Assert.Null(exception);
            exception = await Record.ExceptionAsync(async () =>
            {
                var p = updateInfo.Update.ChecksumType;
                await Task.CompletedTask;
            });
            Assert.Null(exception);
            exception = await Record.ExceptionAsync(async () =>
            {
                var p = updateInfo.Update.Homepage;
                await Task.CompletedTask;
            });
            Assert.Null(exception);
            exception = await Record.ExceptionAsync(async () =>
            {
                var p = updateInfo.Update.ImplicitIdRequired;
                await Task.CompletedTask;
            });
            Assert.Null(exception);
            exception = await Record.ExceptionAsync(async () =>
            {
                var p = updateInfo.Update.Size;
                await Task.CompletedTask;
            });
            Assert.Null(exception);
            exception = await Record.ExceptionAsync(async () =>
            {
                var p = updateInfo.Update.Version;
                await Task.CompletedTask;
            });
            Assert.Null(exception);
        }

        exception = await Record.ExceptionAsync(async () => { var p = updateInfo.Updated; await Task.CompletedTask; });
        Assert.Null(exception);
    }
}
