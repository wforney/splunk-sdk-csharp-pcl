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

using System.Collections.ObjectModel;
using Xunit;

namespace Splunk.Client.UnitTests
{
    public class TestResource
    {
        [Trait("unit-test", "Splunk.Client.Resource")]
        [Fact]
        public async Task CanConstructResource()
        {
            var feed = await TestAtomFeed.ReadFeed(Path.Combine(TestAtomFeed.Directory, "JobCollection.GetAsync.xml"));

            using var context = new Context(Scheme.Https, "localhost", 8089);
            dynamic collection = new ResourceCollection(feed);
            CheckCommonStaticPropertiesOfResource(collection);

            //// Static property checks

            Assert.Equal("jobs", collection.Title);
            Assert.Empty(collection.Links);
            Assert.Empty(collection.Messages);

            Pagination p = collection.Pagination;
            Assert.Equal(14, p.TotalResults);
            Assert.Equal(0, p.StartIndex);
            Assert.Equal(0, p.ItemsPerPage);

            var p2 = collection.Resources;
            Assert.Equal(14, p2.Count);

            foreach (var resource in collection.Resources)
            {
                CheckCommonStaticPropertiesOfResource(resource);
                CheckExistenceOfJobProperties(resource);
                Assert.IsType<Resource>(resource);
            }

            {
                dynamic resource = collection.Resources[0];

                CheckCommonStaticPropertiesOfResource(resource);

                Assert.IsType<Uri>(resource.Id);
                Assert.Equal("https://localhost:8089/services/search/jobs/scheduler__admin__search__RMD50aa4c13eb03d1730_at_1401390000_866", resource.Id.ToString());

                Assert.IsType<string>(resource.Content["Sid"]);
                Assert.Equal("scheduler__admin__search__RMD50aa4c13eb03d1730_at_1401390000_866", resource.Content["Sid"]);

                Assert.IsType<string>(resource.Title);
                Assert.Equal("search search index=_internal | head 1000", resource.Title);

                Assert.NotNull(resource.Links);
                Assert.IsType<ReadOnlyDictionary<string, Uri>>(resource.Links);
                Assert.Equal(new string[] { "alternate", "search.log", "events", "results", "results_preview", "timeline", "summary", "control" }, resource.Links.Keys);

                CheckExistenceOfJobProperties(resource);
            }
        }

        #region Privates/internals

        static void CheckCommonStaticPropertiesOfResource(BaseResource resource)
        {
                dynamic o = resource;
                Assert.True(o.Id.Equals(resource.Id));

                o = resource;
                Assert.True(o.Title.Equals(resource.Title));
        }

        internal static void CheckExistenceOfApplicationProperties(dynamic entity)
        {
            var p = entity.Content["CheckForUpdates"];
            var p2 = entity.Content["Configured"];
            var p3 = entity.Content["Disabled"];
            var p4 = entity.Content["Eai"];
            var p5 = entity.Content["Label"];
            var p6 = entity.Content["StateChangeRequiresRestart"];
            var p7 = entity.Content["Visible"];
        }

        internal static void CheckExistenceOfJobProperties(dynamic job)
        {
            var p = job.Published;
            var p2 = job.Content["CanSummarize"];
            var p3 = job.Content["CursorTime"];
            var p4 = job.Content["DefaultSaveTTL"];
            var p5 = job.Content["DefaultTTL"];
            var p6 = job.Content["DiskUsage"];
            var p7 = job.Content["DispatchState"];
            var p8 = job.Content["DoneProgress"];
            var p9 = job.Content["DropCount"];
            var p10 = job.Content["Eai"]["Acl"];
            var p11 = job.Content["Eai"]["Acl"]["App"];
            var p12 = job.Content["Eai"]["Acl"]["CanWrite"];
            var p13 = job.Content["Eai"]["Acl"]["Modifiable"];
            var p14 = job.Content["Eai"]["Acl"]["Owner"];
            var p15 = job.Content["Eai"]["Acl"]["Perms"];
            var p16 = job.Content["Eai"]["Acl"]["Perms"]["Read"];
            var p17 = job.Content["Eai"]["Acl"]["Perms"]["Write"];
            var p18 = job.Content["Eai"]["Acl"]["Sharing"];
            var p19 = job.Content["Eai"]["Acl"]["Ttl"];
            var p20 = job.Content["EarliestTime"];
            var p21 = job.Content["EventAvailableCount"];
            var p22 = job.Content["EventCount"];

            //// More...

            var p23 = job.Content["Messages"];

            //// More...
        }
        #endregion
    }
}
