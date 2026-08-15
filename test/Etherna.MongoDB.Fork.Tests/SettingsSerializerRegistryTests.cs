/* Copyright 2020-present Etherna SA
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using Etherna.MongoDB.Bson.Serialization;

using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Fork.Tests.Support;
using Xunit;

namespace Etherna.MongoDB.Fork.Tests
{
    /// <summary>
    /// Covers the fork making <see cref="MongoDatabaseSettings.SerializerRegistry"/> and
    /// <see cref="MongoCollectionSettings.SerializerRegistry"/> settable. Upstream v3.10.0 declares
    /// both get-only, returning the process-wide static registry, so none of the assignments below
    /// would even compile against the official driver.
    ///
    /// No server is involved: building a client and asking it for a database and a collection is a
    /// pure object-graph operation. The client is disposed so its background SDAM monitors don't
    /// outlive the test.
    /// </summary>
    public class SettingsSerializerRegistryTests
    {
        private const string NoServerConnectionString = "mongodb://localhost:27017";

        public class DatabaseDoc { public int Value { get; set; } }
        public class InheritedDoc { public int Value { get; set; } }
        public class OverriddenDoc { public int Value { get; set; } }

        [Fact]
        public void ADatabaseKeepsTheSerializerRegistryItWasGiven()
        {
            var registry = new ContextualSerializerRegistry();

            using var client = new MongoClient(NoServerConnectionString);
            var database = client.GetDatabase("db", new MongoDatabaseSettings { SerializerRegistry = registry });

            Assert.Same(registry, database.Settings.SerializerRegistry);
        }

        [Fact]
        public void ACollectionResolvesItsDocumentSerializer_FromTheDatabaseRegistry()
        {
            // The assertion that matters: the registry is consulted, not merely stored.
            var marker = new MarkerSerializer<DatabaseDoc>();
            var registry = new ContextualSerializerRegistry().Override<DatabaseDoc>(marker);

            using var client = new MongoClient(NoServerConnectionString);
            var database = client.GetDatabase("db", new MongoDatabaseSettings { SerializerRegistry = registry });
            var collection = database.GetCollection<DatabaseDoc>("coll");

            Assert.Same(marker, collection.DocumentSerializer);
        }

        [Fact]
        public void CollectionSettingsInheritTheDatabaseRegistry_ViaApplyDefaultValues()
        {
            var marker = new MarkerSerializer<InheritedDoc>();
            var registry = new ContextualSerializerRegistry().Override<InheritedDoc>(marker);

            using var client = new MongoClient(NoServerConnectionString);
            var database = client.GetDatabase("db", new MongoDatabaseSettings { SerializerRegistry = registry });

            // Settings supplied explicitly, but with no registry of their own.
            var collection = database.GetCollection<InheritedDoc>("coll", new MongoCollectionSettings());

            Assert.Same(registry, collection.Settings.SerializerRegistry);
            Assert.Same(marker, collection.DocumentSerializer);
        }

        [Fact]
        public void AnExplicitCollectionRegistry_WinsOverTheDatabaseOne()
        {
            var databaseMarker = new MarkerSerializer<OverriddenDoc>();
            var databaseRegistry = new ContextualSerializerRegistry().Override<OverriddenDoc>(databaseMarker);

            var collectionMarker = new MarkerSerializer<OverriddenDoc>();
            var collectionRegistry = new ContextualSerializerRegistry().Override<OverriddenDoc>(collectionMarker);

            using var client = new MongoClient(NoServerConnectionString);
            var database = client.GetDatabase("db", new MongoDatabaseSettings { SerializerRegistry = databaseRegistry });
            var collection = database.GetCollection<OverriddenDoc>(
                "coll",
                new MongoCollectionSettings { SerializerRegistry = collectionRegistry });

            Assert.Same(collectionRegistry, collection.Settings.SerializerRegistry);
            Assert.Same(collectionMarker, collection.DocumentSerializer);
        }

        [Fact]
        public void WithoutAnExplicitRegistry_ADatabaseFallsBackToTheAmbientOne()
        {
            using var client = new MongoClient(NoServerConnectionString);

            Assert.Same(
                BsonSerializer.GetSerializerRegistry(forceStaticSerializerRegistry: true),
                client.GetDatabase("db").Settings.SerializerRegistry);
        }

        [Fact]
        public void TheRegistryCannotBeReassigned_OnceTheSettingsAreFrozen()
        {
            using var client = new MongoClient(NoServerConnectionString);
            var database = client.GetDatabase("db");

            Assert.True(database.Settings.IsFrozen);
            Assert.Throws<InvalidOperationException>(
                () => database.Settings.SerializerRegistry = new ContextualSerializerRegistry());
        }

        [Fact]
        public void FreshCollectionSettingsHaveNoRegistryUntilDefaultsAreApplied()
        {
            // Fork-specific: upstream's get-only property always answered with the static registry,
            // so it could never be null. Here it stays unset until ApplyDefaultValues runs.
            Assert.Null(new MongoCollectionSettings().SerializerRegistry);
        }
    }
}
