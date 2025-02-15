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

namespace Splunk.Client.UnitTests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Splunk.Client;
using Splunk.Client.Entities;
using Splunk.Client.Syndication;
using Xunit;

public class TestBaseEntity
{
    [Trait("unit-test", "Splunk.Client.Entity")]
    [Fact]
    public async Task CanConstructEntity()
    {
        var feed = await TestAtomFeed.ReadFeed(Path.Combine(TestAtomFeed.Directory, "Application.GetAsync.xml"));

        using var context = new Context(Scheme.Https, "localhost", 8089);
        var entity = new Entity<Resource>(context, feed);
        CheckApplication(entity, feed.Entries[0], feed.GeneratorVersion);
    }

    [Trait("unit-test", "Splunk.Client.EntityCollection<Entity>")]
    [Fact]
    public async Task CanConstructEntityCollectionOfEntity()
    {
        var feed = await TestAtomFeed.ReadFeed(Path.Combine(TestAtomFeed.Directory, "ApplicationCollection.GetAsync.xml"));

        using var context = new Context(Scheme.Https, "localhost", 8089);
        //// EntityCollection<TEntity> checks

        var collection = new EntityCollection<Entity<Resource>, Resource>(context, feed);
        CheckCommonStaticPropertiesEntity(collection);

        Assert.Equal(feed.Id, collection.Id);
        Assert.Equal(feed.GeneratorVersion, collection.GeneratorVersion);
        Assert.Equal(feed.Title, collection.Title);
        Assert.Equal(feed.Entries.Count, collection.Count);

        for (var i = 0; i < collection.Count; i++)
        {
            var entry = feed.Entries[i];
            var entity = collection[i];
            CheckApplication(entity, entry, feed.GeneratorVersion);
        }
    }

    [Trait("unit-test", "Splunk.Client.EntityCollection<EntityCollection<Entity>")]
    [Fact]
    public async Task CanConstructEntityCollectionOfEntityCollection()
    {
        var feed = await TestAtomFeed.ReadFeed(Path.Combine(TestAtomFeed.Directory, "ConfigurationCollection.GetAsync.xml"));

        using var context = new Context(Scheme.Https, "localhost", 8089);
        var collection = new EntityCollection<EntityCollection<Entity<Resource>, Resource>, ResourceCollection>(context, feed);

        CheckCommonStaticPropertiesEntity(collection);
        Assert.Equal("properties", collection.Name);
        Assert.Equal(83, collection.Count);

        foreach (var entity in collection)
        {
            Assert.Empty(entity);
        }
    }

    #region Privates/internals

    private static void CheckCommonStaticPropertiesEntity<TResource>(BaseEntity<TResource> entity) where TResource : BaseResource, new()
    {
        dynamic o = entity;
        Assert.True(o.Context.Equals(entity.Context));

        o = entity;
        Assert.True(o.Namespace.Equals(entity.Namespace));

        o = entity;
        Assert.True(o.ResourceName.Equals(entity.ResourceName));

        o = entity;
        Assert.True(o.GeneratorVersion.Equals(entity.GeneratorVersion));

        o = entity;
        Assert.True(o.Id.Equals(entity.Id));

        o = entity;
        Assert.True(o.Title.Equals(entity.Title));

        o = entity;
        Assert.True(o.Updated.Equals(entity.Updated));
    }

    private static void CheckApplication(Entity<Resource> application, AtomEntry entry, Version generatorVersion)
    {
        CheckCommonStaticPropertiesEntity(application);

        Assert.Equal(entry.Author, application.Dynamic.Author);
        Assert.Equal(entry.Title, application.Name);
        Assert.Equal(entry.Title, application.Title);
        Assert.Equal(entry.Updated, application.Updated);

        Assert.NotNull(application.Dynamic.Links);

        IReadOnlyDictionary<string, Uri> links = application.Dynamic.Links;
        Assert.NotEqual(0, links.Count);

        Assert.Equal(generatorVersion, application.GeneratorVersion);

        TestResource.CheckExistenceOfApplicationProperties(application.Dynamic);
    }

    #endregion
}
