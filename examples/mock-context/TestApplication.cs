using System;
using System.Net;
using System.Threading.Tasks;
using Splunk.Client.Helper;
using Xunit;

namespace Splunk.Client.UnitTests;
public class ApplicationTest
{
    [Trait("acceptance-test", "Splunk.Client.Application")]
    [MockContext]
    [Fact]
    public async Task TestApplications()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        using var service = await SdkHelper.CreateService();
        ApplicationCollection apps = service.Applications;
        await apps.GetAllAsync();

        foreach (Application app in apps)
        {
            await CheckApplication(app);
        }

        for (int i = 0; i < apps.Count; i++)
        {
            await CheckApplication(apps[i]);
        }
    }

    async Task CheckApplication(Application app)
    {
        ApplicationSetupInfo setupInfo = null;

        try 
        {
            setupInfo = await app.GetSetupInfoAsync();

            //// TODO: Install an app which hits this code before this test runs

            Assert.NotNull(setupInfo.Eai);
            bool p0 = setupInfo.Refresh;
        } 
        catch (InternalServerErrorException e) 
        {
            Assert.Contains("Internal Server Error", e.Message);
        }

        ApplicationArchiveInfo archiveInfo = await app.PackageAsync();

        string p = app.Author; 
        Assert.NotNull(p);

        string p2 = app.ApplicationAuthor;
        bool p3 = app.CheckForUpdates;
        string p4 = app.Description;
        string p5 = app.Label;
        bool p6 = app.Refresh;
        string p7 = app.Version;
        bool p8 = app.Configured;
        bool p9 = app.StateChangeRequiresRestart;
        bool p10 = app.Visible;

        ApplicationUpdateInfo updateInfo = await app.GetUpdateInfoAsync();
        Assert.NotNull(updateInfo.Eai);

        if (updateInfo.Update != null)
        {
            var update = updateInfo.Update;

                string p11 = updateInfo.Update.ApplicationName;
                Uri p12 = updateInfo.Update.ApplicationUri;
                string p13 = updateInfo.Update.ApplicationName;
                string p14 = updateInfo.Update.ChecksumType;
                string p15 = updateInfo.Update.Homepage;
                bool p16 = updateInfo.Update.ImplicitIdRequired;
                long p17 = updateInfo.Update.Size;
                string p18 = updateInfo.Update.Version;
        }

        DateTime p19 = updateInfo.Updated;
    }
}
