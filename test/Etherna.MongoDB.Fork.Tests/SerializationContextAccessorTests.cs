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

using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongoDB.Fork.Tests.Support;
using Xunit;

namespace Etherna.MongoDB.Fork.Tests
{
    /// <summary>
    /// Covers the fork's core addition: <see cref="ISerializationContextAccessor"/> and the
    /// <c>forceStaticSerializerRegistry</c> escape hatch on
    /// <see cref="BsonSerializer.GetSerializerRegistry(bool)"/> /
    /// <see cref="BsonSerializer.LookupSerializer(System.Type, bool)"/>.
    ///
    /// Upstream has neither: it exposes a read-only <c>BsonSerializer.SerializerRegistry</c>
    /// property that always yields the one process-wide registry.
    /// </summary>
    public class SerializationContextAccessorTests
    {
        public class Widget { public int Value { get; set; } }

        private sealed class WidgetMarkerSerializer : SerializerBase<Widget> { }

        [Fact]
        public void GetSerializerRegistry_ResolvesTheContextualRegistry_WhileAScopeIsActive()
        {
            var contextual = new ContextualSerializerRegistry();

            using (ContextualRegistryScope.Enter(contextual))
            {
                Assert.Same(contextual, BsonSerializer.GetSerializerRegistry());
            }
        }

        [Fact]
        public void GetSerializerRegistry_FallsBackToTheStaticRegistry_OutsideAnyScope()
        {
            AsyncLocalSerializationContextAccessor.EnsureInstalled();

            // The accessor is installed but yields null, which must degrade to the static registry.
            Assert.Same(
                BsonSerializer.GetSerializerRegistry(forceStaticSerializerRegistry: true),
                BsonSerializer.GetSerializerRegistry());
        }

        [Fact]
        public void GetSerializerRegistry_IgnoresTheContextualRegistry_WhenForced()
        {
            var contextual = new ContextualSerializerRegistry();

            using (ContextualRegistryScope.Enter(contextual))
            {
                var forced = BsonSerializer.GetSerializerRegistry(forceStaticSerializerRegistry: true);

                Assert.NotSame(contextual, forced);
            }
        }

        [Fact]
        public void GetSerializerRegistry_DoesNotEvenConsultTheAccessor_WhenForced()
        {
            var contextual = new ContextualSerializerRegistry();

            using (ContextualRegistryScope.Enter(contextual))
            {
                AsyncLocalSerializationContextAccessor.ResetInvocationCount();

                BsonSerializer.GetSerializerRegistry(forceStaticSerializerRegistry: true);

                Assert.Equal(0, AsyncLocalSerializationContextAccessor.InvocationCount);
            }
        }

        [Fact]
        public void GetSerializerRegistry_ConsultsTheAccessorOnEveryCall_WithoutMemoizing()
        {
            var contextual = new ContextualSerializerRegistry();

            using (ContextualRegistryScope.Enter(contextual))
            {
                AsyncLocalSerializationContextAccessor.ResetInvocationCount();

                BsonSerializer.GetSerializerRegistry();
                BsonSerializer.GetSerializerRegistry();
                BsonSerializer.GetSerializerRegistry();

                Assert.Equal(3, AsyncLocalSerializationContextAccessor.InvocationCount);
            }
        }

        [Fact]
        public void ScopesNest_AndTheInnermostOneWins()
        {
            var outer = new ContextualSerializerRegistry();
            var inner = new ContextualSerializerRegistry();

            using (ContextualRegistryScope.Enter(outer))
            {
                Assert.Same(outer, BsonSerializer.GetSerializerRegistry());

                using (ContextualRegistryScope.Enter(inner))
                {
                    Assert.Same(inner, BsonSerializer.GetSerializerRegistry());
                }

                Assert.Same(outer, BsonSerializer.GetSerializerRegistry());
            }
        }

        [Fact]
        public void LookupSerializer_ResolvesThroughTheContextualRegistry_ButHonoursTheForceFlag()
        {
            var marker = new WidgetMarkerSerializer();
            var contextual = new ContextualSerializerRegistry().Override(marker);

            using (ContextualRegistryScope.Enter(contextual))
            {
                Assert.Same(marker, BsonSerializer.LookupSerializer(typeof(Widget)));
                Assert.Same(marker, BsonSerializer.LookupSerializer<Widget>());

                Assert.NotSame(marker, BsonSerializer.LookupSerializer(typeof(Widget), forceStaticSerializerRegistry: true));
                Assert.NotSame(marker, BsonSerializer.LookupSerializer<Widget>(forceStaticSerializerRegistry: true));
            }
        }
    }
}
