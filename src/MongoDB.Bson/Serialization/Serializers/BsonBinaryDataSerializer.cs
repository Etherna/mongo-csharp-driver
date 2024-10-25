/* Copyright 2010-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace Etherna.MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for BsonBinaryDatas.
    /// </summary>
    public sealed class BsonBinaryDataSerializer : BsonValueSerializerBase<BsonBinaryData>
    {
        // private static fields
        private static BsonBinaryDataSerializer __instance = new BsonBinaryDataSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonBinaryDataSerializer class.
        /// </summary>
        public BsonBinaryDataSerializer()
            : base(BsonType.Binary)
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the BsonBinaryDataSerializer class.
        /// </summary>
        public static BsonBinaryDataSerializer Instance
        {
            get { return __instance; }
        }

        // protected methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected override BsonBinaryData DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;
            return bsonReader.ReadBinaryData();
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, BsonBinaryData value)
        {
            var bsonWriter = context.Writer;
            bsonWriter.WriteBinaryData(value);
        }
    }
}
