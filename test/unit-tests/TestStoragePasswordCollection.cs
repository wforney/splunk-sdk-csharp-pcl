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

using Xunit;

namespace Splunk.Client.UnitTests;

public class TestStoragePasswordCollection
{
    [Trait("unit-test", "Splunk.Client.StoragePassword")]
    [Fact]
    public async Task CanConstructStoragePassword()
    {
        var feed = await TestAtomFeed.ReadFeed(Path.Combine(TestAtomFeed.Directory, "StoragePassword.GetAsync.xml"));

        using var context = new Context(Scheme.Https, "localhost", 8089);
        var password = new StoragePassword(context, feed);
        CheckStoragePassword(feed.Entries[0], password);
    }

    [Trait("unit-test", "Splunk.Client.StoragePasswordCollection")]
    [Fact]
    public async Task CanConstructStoragePasswordCollection()
    {
        var feed = await TestAtomFeed.ReadFeed(Path.Combine(TestAtomFeed.Directory, "StoragePasswordCollection.GetAsync.xml"));

        using var context = new Context(Scheme.Https, "localhost", 8089);
        var expectedNames = new string[]
        {
                ":foobar:",
                "splunk.com:foobar:",
                "splunk\\:com:foobar:"
        };

        var passwords = new StoragePasswordCollection(context, feed);

        Assert.Equal(expectedNames, from password in passwords select password.Title);
        Assert.Equal(expectedNames.Length, passwords.Count);
        CheckCommonProperties("passwords", passwords);

        for (int i = 0; i < passwords.Count; i++)
        {
            var entry = feed.Entries[i];
            var password = passwords[i];
            CheckStoragePassword(entry, password);
        }
    }

    public void CheckCommonProperties<TResource>(string expectedName, BaseEntity<TResource> entity) where TResource : BaseResource, new()
    {
        Assert.Equal(expectedName, entity.Title);

        //// Properties common to all resources

        Version value = entity.GeneratorVersion;
        Assert.NotNull(value);

        Uri value2 = entity.Id;
        Assert.NotNull(value2);

        string value3 = entity.Title;
        Assert.NotNull(value3);

        DateTime value4 = entity.Updated;
        Assert.NotEqual(DateTime.MinValue, value4);
    }

    public void CheckEai(StoragePassword password)
    {
            bool canList = password.Eai.Acl.CanList;
            string app = password.Eai.Acl.App;
            dynamic eai = password.Eai;
            Assert.Equal(app, eai.Acl.App);
            Assert.Equal(canList, eai.Acl.CanList);
    }

    public void CheckStoragePassword(AtomEntry entry, StoragePassword password)
    {
        CheckCommonProperties(entry.Title, password);
        CheckEai(password);

        Assert.Equal(entry.Content["ClearPassword"], password.ClearPassword);
        Assert.Equal(entry.Content["EncrPassword"], password.EncryptedPassword);
        Assert.Equal(entry.Content["Password"], password.Password);
        Assert.Equal(entry.Content["Username"], password.Username);
        var dictionary = (IDictionary<string, object>)entry.Content;
        Assert.Equal(dictionary["Realm"], password.Realm);
    }
}
