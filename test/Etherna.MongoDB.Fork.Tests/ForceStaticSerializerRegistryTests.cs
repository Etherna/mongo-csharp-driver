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

using System.Collections.Generic;
using System.Dynamic;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.IO;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongoDB.Fork.Tests.Support;
using Xunit;

namespace Etherna.MongoDB.Fork.Tests
{
    /// <summary>
    /// Covers the <c>forceStaticSerializerRegistry</c> flag where the fork actually threads it
    /// through to a serializer lookup. None of these entry points exist upstream: upstream has no
    /// contextual registry to bypass.
    ///
    /// Deliberately not covered: the parameters the fork accepts but never reads —
    /// <c>BsonDocument.GetValue/TryGetValue</c> as implemented on <c>BsonDocument</c> itself
    /// (<see cref="RawBsonDocument"/>'s overrides do read them, and are covered here), every
    /// <c>WriteEndDocument</c> implementation, and <c>WrappingBsonWriter.WriteEndDocument</c>.
    /// Asserting on those would pin behavior the fork does not actually implement.
    /// </summary>
    public class ForceStaticSerializerRegistryTests
    {
        public class Payload { public string Text { get; set; } }

        [Fact]
        public void BsonDocumentToString_UsesTheContextualRegistry_ButHonoursTheForceFlag()
        {
            var document = new BsonDocument("text", "hello");
            var contextual = new ContextualSerializerRegistry().Override(new TaggingBsonStringSerializer());

            using (ContextualRegistryScope.Enter(contextual))
            {
                Assert.Contains(TaggingBsonStringSerializer.Tag, document.ToString());
                Assert.DoesNotContain(TaggingBsonStringSerializer.Tag, document.ToString(forceStaticSerializerRegistry: true));
            }
        }

        [Fact]
        public void ToJson_HonoursForceStaticSerializerRegistry_FromTheSerializationArgs()
        {
            var value = new BsonDocument("text", "hello");
            var contextual = new ContextualSerializerRegistry().Override(new TaggingBsonStringSerializer());

            using (ContextualRegistryScope.Enter(contextual))
            {
                Assert.Contains(TaggingBsonStringSerializer.Tag, value.ToJson());
                Assert.DoesNotContain(
                    TaggingBsonStringSerializer.Tag,
                    value.ToJson(args: new BsonSerializationArgs { ForceStaticSerializerRegistry = true }));
            }
        }

        [Fact]
        public void ToBson_HonoursForceStaticSerializerRegistry_FromTheSerializationArgs()
        {
            var value = new BsonDocument("text", "hello");
            var contextual = new ContextualSerializerRegistry().Override(new TaggingBsonStringSerializer());

            using (ContextualRegistryScope.Enter(contextual))
            {
                var contextualBytes = value.ToBson();
                var forcedBytes = value.ToBson(args: new BsonSerializationArgs { ForceStaticSerializerRegistry = true });

                Assert.NotEqual(contextualBytes, forcedBytes);
                Assert.Equal(
                    value.ToBson(args: new BsonSerializationArgs { ForceStaticSerializerRegistry = true }),
                    forcedBytes);
            }
        }

        [Fact]
        public void SerializerExtension_Serialize_PropagatesTheForceFlagIntoTheArgs()
        {
            var value = new BsonDocument("text", "hello");
            var contextual = new ContextualSerializerRegistry().Override(new TaggingBsonStringSerializer());

            using (ContextualRegistryScope.Enter(contextual))
            {
                Assert.DoesNotContain(TaggingBsonStringSerializer.Tag, SerializeVia(value, forceStaticSerializerRegistry: true));
                Assert.Contains(TaggingBsonStringSerializer.Tag, SerializeVia(value, forceStaticSerializerRegistry: false));
            }

            static string SerializeVia(BsonDocument value, bool forceStaticSerializerRegistry)
            {
                var written = new BsonDocument();
                using (var writer = new BsonDocumentWriter(written))
                {
                    var context = BsonSerializationContext.CreateRoot(writer);
                    BsonDocumentSerializer.Instance.Serialize(context, value, forceStaticSerializerRegistry);
                }

                return written.ToString(forceStaticSerializerRegistry: true);
            }
        }

        [Fact]
        public void TheForceFlag_ReachesEveryRecursiveStepOfTheBsonValueFamily()
        {
            // One string value per recursive step the fork threads the flag through: the id-first
            // path of BsonDocumentSerializer (via SerializeIdFirst), the Document, Array and
            // JavaScriptWithScope cases of the BsonValueSerializer dispatcher (plus the per-element
            // forwarding of BsonDocumentSerializer/BsonArraySerializer), and the scope of
            // BsonJavaScriptWithScopeSerializer. An upstream merge that reverts any one of those
            // one-line forwards would resurface the contextual tag in the forced output.
            var document = new BsonDocument
            {
                { "_id", "root-id" },
                { "nested", new BsonDocument("inner", "nested-value") },
                { "array", new BsonArray { "array-value" } },
                { "code", new BsonJavaScriptWithScope("f()", new BsonDocument("scoped", "scope-value")) },
            };
            var contextual = new ContextualSerializerRegistry().Override(new TaggingBsonStringSerializer());

            using (ContextualRegistryScope.Enter(contextual))
            {
                // Control: all four strings really are intercepted by the contextual registry, so
                // the forced assertion below cannot pass vacuously.
                Assert.Equal(4, CountTags(document.ToJson(args: new BsonSerializationArgs
                {
                    SerializeIdFirst = true
                })));

                Assert.Equal(0, CountTags(document.ToJson(args: new BsonSerializationArgs
                {
                    SerializeIdFirst = true,
                    ForceStaticSerializerRegistry = true
                })));
            }

            static int CountTags(string json) =>
                (json.Length - json.Replace(TaggingBsonStringSerializer.Tag, "").Length)
                    / TaggingBsonStringSerializer.Tag.Length;
        }

        [Fact]
        public void BsonDefaults_ResolvesTheDynamicSerializers_ThroughTheSelectedRegistry()
        {
            // BsonDefaults' upstream DynamicArraySerializer/DynamicDocumentSerializer properties were
            // replaced by these registry-aware methods. The setters are not exercised anywhere in this
            // suite: they latch a private flag that no API can clear, which would permanently pin the
            // dynamic serializers for every later test in the process.
            var arrayMarker = new MarkerSerializer<List<object>>();
            var documentMarker = new MarkerSerializer<ExpandoObject>();
            var contextual = new ContextualSerializerRegistry()
                .Override<List<object>>(arrayMarker)
                .Override<ExpandoObject>(documentMarker);

            using (ContextualRegistryScope.Enter(contextual))
            {
                Assert.Same(arrayMarker, BsonDefaults.GetDynamicArraySerializer());
                Assert.Same(documentMarker, BsonDefaults.GetDynamicDocumentSerializer());

                Assert.NotSame(arrayMarker, BsonDefaults.GetDynamicArraySerializer(forceStaticSerializerRegistry: true));
                Assert.NotSame(documentMarker, BsonDefaults.GetDynamicDocumentSerializer(forceStaticSerializerRegistry: true));
            }
        }

        [Fact]
        public void DeserializationContextCreateRoot_SelectsItsDynamicSerializers_WithTheForceFlag()
        {
            var arrayMarker = new MarkerSerializer<List<object>>();
            var contextual = new ContextualSerializerRegistry().Override<List<object>>(arrayMarker);

            using (ContextualRegistryScope.Enter(contextual))
            using (var reader = new BsonDocumentReader(new BsonDocument()))
            {
                Assert.Same(
                    arrayMarker,
                    BsonDeserializationContext.CreateRoot(reader).DynamicArraySerializer);

                Assert.NotSame(
                    arrayMarker,
                    BsonDeserializationContext.CreateRoot(reader, configurator: null, forceStaticSerializerRegistry: true)
                        .DynamicArraySerializer);
            }
        }

        [Fact]
        public void RawBsonDocumentTryGetValue_NeverConsultsTheAccessor_WhenForced()
        {
            // TryGetValue(name, out value, forceStaticSerializerRegistry: true) is the exact call
            // CommandUsingQueryMessageWireProtocol makes to pull $clusterTime/operationTime out of a
            // server reply on a driver-owned thread. The wire protocol itself is internal and out of
            // reach, so the rule is pinned here on the public overload it relies on. The observable
            // effect is the dynamic-serializer lookup performed when the deserialization context is
            // created: forced, it must resolve against the static registry without ever asking the
            // accessor.
            var bytes = new BsonDocument("x", 1).ToBson();

            using (var rawDocument = new RawBsonDocument(bytes))
            using (ContextualRegistryScope.Enter(new ContextualSerializerRegistry()))
            {
                AsyncLocalSerializationContextAccessor.ResetInvocationCount();

                Assert.True(rawDocument.TryGetValue("x", out _, forceStaticSerializerRegistry: true));
                // GetValue funnels into TryGetValue, forwarding the flag.
                Assert.Equal(1, rawDocument.GetValue("x", defaultValue: null, forceStaticSerializerRegistry: true).AsInt32);
                Assert.Equal(0, AsyncLocalSerializationContextAccessor.InvocationCount);

                // Control: the unforced overload does consult the accessor.
                rawDocument.TryGetValue("x", out _);
                Assert.True(AsyncLocalSerializationContextAccessor.InvocationCount > 0);
            }
        }

        [Fact]
        public void RawBsonDocumentMaterialize_NeverConsultsTheAccessor_WhenForced()
        {
            // Same rule as above, for the overload CommandUsingQueryMessageWireProtocol uses to
            // materialize $clusterTime and server error documents.
            var bytes = new BsonDocument("x", 1).ToBson();

            using (var rawDocument = new RawBsonDocument(bytes))
            using (ContextualRegistryScope.Enter(new ContextualSerializerRegistry()))
            {
                AsyncLocalSerializationContextAccessor.ResetInvocationCount();

                var materialized = rawDocument.Materialize(BsonBinaryReaderSettings.Defaults, forceStaticSerializerRegistry: true);

                Assert.Equal(1, materialized["x"].AsInt32);
                Assert.Equal(0, AsyncLocalSerializationContextAccessor.InvocationCount);

                rawDocument.Materialize(BsonBinaryReaderSettings.Defaults);
                Assert.True(AsyncLocalSerializationContextAccessor.InvocationCount > 0);
            }
        }

        public class GenericFlagProbe { }
        public class NonGenericFlagProbe { }
        public class ByteArrayFlagProbe { }

        /// <summary>Records the args it is handed, so the flag's propagation can be asserted.</summary>
        private sealed class FlagRecordingSerializer<TValue> : SerializerBase<TValue> where TValue : new()
        {
            public bool? LastForceStaticSerializerRegistry { get; private set; }

            public override TValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                LastForceStaticSerializerRegistry = args.ForceStaticSerializerRegistry;
                context.Reader.ReadStartDocument();
                context.Reader.ReadEndDocument();
                return new TValue();
            }
        }

        [Fact]
        public void Deserialize_PropagatesTheForceFlag_IntoTheDeserializationArgs()
        {
            // Regression test: the flag used to stop at the root serializer lookup and the context's
            // dynamic serializers, leaving args.ForceStaticSerializerRegistry false — so nested
            // lookups fell back to the contextual registry even on a forced deserialization. That is
            // the path AWSLastSaslStep uses to parse a server response on a driver-owned thread.
            var recorder = new FlagRecordingSerializer<GenericFlagProbe>();
            BsonSerializer.RegisterSerializer(typeof(GenericFlagProbe), recorder);

            BsonSerializer.Deserialize<GenericFlagProbe>("{ }", forceStaticSerializerRegistry: true);
            Assert.True(recorder.LastForceStaticSerializerRegistry);

            BsonSerializer.Deserialize<GenericFlagProbe>("{ }");
            Assert.False(recorder.LastForceStaticSerializerRegistry);
        }

        [Fact]
        public void Deserialize_PropagatesTheForceFlag_IntoTheArgsOfTheNonGenericOverloadToo()
        {
            var recorder = new FlagRecordingSerializer<NonGenericFlagProbe>();
            BsonSerializer.RegisterSerializer(typeof(NonGenericFlagProbe), recorder);

            BsonSerializer.Deserialize("{ }", typeof(NonGenericFlagProbe), forceStaticSerializerRegistry: true);
            Assert.True(recorder.LastForceStaticSerializerRegistry);

            BsonSerializer.Deserialize("{ }", typeof(NonGenericFlagProbe));
            Assert.False(recorder.LastForceStaticSerializerRegistry);
        }

        [Fact]
        public void Deserialize_PropagatesTheForceFlag_FromTheByteArrayOverload()
        {
            // The byte[] overload is the shape AWSLastSaslStep actually uses to parse the server's
            // SASL payload; it funnels through the Stream and IBsonReader overloads, so this also
            // pins that the flag survives that whole chain.
            var recorder = new FlagRecordingSerializer<ByteArrayFlagProbe>();
            BsonSerializer.RegisterSerializer(typeof(ByteArrayFlagProbe), recorder);
            var bytes = new BsonDocument().ToBson();

            BsonSerializer.Deserialize<ByteArrayFlagProbe>(bytes, forceStaticSerializerRegistry: true);
            Assert.True(recorder.LastForceStaticSerializerRegistry);

            BsonSerializer.Deserialize<ByteArrayFlagProbe>(bytes);
            Assert.False(recorder.LastForceStaticSerializerRegistry);
        }

        [Fact]
        public void Deserialize_LooksUpTheRootSerializer_ThroughTheSelectedRegistry()
        {
            var marker = new MarkerSerializer<Payload>();
            var contextual = new ContextualSerializerRegistry().Override<Payload>(marker);

            using (ContextualRegistryScope.Enter(contextual))
            {
                // The marker serializer refuses to deserialize, which is exactly how we detect that it
                // was the one selected; forcing the static registry gets the real class map serializer.
                Assert.ThrowsAny<System.NotSupportedException>(
                    () => BsonSerializer.Deserialize<Payload>("{ 'Text' : 'hello' }"));

                var deserialized = BsonSerializer.Deserialize<Payload>(
                    "{ 'Text' : 'hello' }",
                    forceStaticSerializerRegistry: true);

                Assert.Equal("hello", deserialized.Text);
            }
        }
    }
}
