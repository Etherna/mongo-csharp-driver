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

using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Bson.Serialization.Serializers;

namespace Etherna.MongoDB.Fork.Tests.Support
{
    /// <summary>
    /// A serializer that exists only to be recognised by reference. Used where a test just needs to
    /// know <em>which</em> registry answered a lookup.
    /// </summary>
    public sealed class MarkerSerializer<TValue> : SerializerBase<TValue>
    {
    }

    /// <summary>
    /// Writes strings with a prefix, so BSON produced through a contextual registry is
    /// distinguishable from BSON produced through the static one.
    /// </summary>
    /// <remarks>
    /// Targets <see cref="BsonString"/> rather than <see cref="string"/> because the element values
    /// of a <see cref="BsonDocument"/> are looked up by their runtime <see cref="BsonValue"/> type.
    /// </remarks>
    public sealed class TaggingBsonStringSerializer : SerializerBase<BsonString>
    {
        public const string Tag = "ctx:";

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, BsonString value) =>
            context.Writer.WriteString(Tag + value.Value);
    }

    /// <summary>Writes 64-bit integers offset by <see cref="Offset"/>.</summary>
    public sealed class OffsetBsonInt64Serializer : SerializerBase<BsonInt64>
    {
        public const long Offset = 1000;

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, BsonInt64 value) =>
            context.Writer.WriteInt64(value.Value + Offset);
    }

    /// <summary>Writes booleans negated.</summary>
    public sealed class NegatingBsonBooleanSerializer : SerializerBase<BsonBoolean>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, BsonBoolean value) =>
            context.Writer.WriteBoolean(!value.Value);
    }
}
