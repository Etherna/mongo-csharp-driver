﻿/* Copyright 2010-present MongoDB Inc.
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

using System;
using System.Collections.Generic;
using System.Dynamic;
using Etherna.MongoDB.Bson.Serialization;
namespace Etherna.MongoDB.Bson
{
    /// <summary>
    /// A static helper class containing BSON defaults.
    /// </summary>
    public static class BsonDefaults
    {
        // private static fields
        private static bool __dynamicArraySerializerWasSet;
        private static IBsonSerializer __dynamicArraySerializer;
        private static bool __dynamicDocumentSerializerWasSet;
        private static IBsonSerializer __dynamicDocumentSerializer;
        private static GuidRepresentation __guidRepresentation = GuidRepresentation.CSharpLegacy;
        private static GuidRepresentationMode __guidRepresentationMode = GuidRepresentationMode.V2;
        private static int __maxDocumentSize = int.MaxValue;
        private static int __maxSerializationDepth = 100;

        // static constructor
        static BsonDefaults()
        {
            var testWithDefaultGuidRepresentation = Environment.GetEnvironmentVariable("TEST_WITH_DEFAULT_GUID_REPRESENTATION");
            if (testWithDefaultGuidRepresentation != null)
            {
                var _ = Enum.TryParse(testWithDefaultGuidRepresentation, out __guidRepresentation); // ignore errors
            }

            var testWithDefaultGuidRepresentationMode = Environment.GetEnvironmentVariable("TEST_WITH_DEFAULT_GUID_REPRESENTATION_MODE");
            if (testWithDefaultGuidRepresentationMode != null)
            {
                var _ = Enum.TryParse(testWithDefaultGuidRepresentationMode, out __guidRepresentationMode); // ignore errors
            }
        }

        // public static properties
        /// <summary>
        /// Gets or sets the default representation to be used in serialization of
        /// Guids to the database.
        /// <seealso cref="MongoDB.Bson.GuidRepresentation"/>
        /// </summary>
        [Obsolete("Configure serializers instead.")]
        public static GuidRepresentation GuidRepresentation
        {
            get
            {
                if (BsonDefaults.GuidRepresentationMode != GuidRepresentationMode.V2)
                {
                    throw new InvalidOperationException("BsonDefaults.GuidRepresentation can only be used when BsonDefaults.GuidRepresentationMode is V2.");
                }
                return __guidRepresentation;
            }
            set
            {
                if (BsonDefaults.GuidRepresentationMode != GuidRepresentationMode.V2)
                {
                    throw new InvalidOperationException("BsonDefaults.GuidRepresentation can only be used when BsonDefaults.GuidRepresentationMode is V2.");
                }
                __guidRepresentation = value;
            }
        }

        /// <summary>
        /// Gets or sets the default representation to be used in serialization of
        /// Guids to the database.
        /// <seealso cref="MongoDB.Bson.GuidRepresentation"/>
        /// </summary>
        [Obsolete("This property will be removed in a later release.")]
        public static GuidRepresentationMode GuidRepresentationMode
        {
            get { return __guidRepresentationMode; }
            set
            {
                __guidRepresentationMode = value;
                if (value == GuidRepresentationMode.V3)
                {
                    __guidRepresentation = GuidRepresentation.Unspecified;
                }
            }
        }

        /// <summary>
        /// Gets or sets the default max document size. The default is 4MiB.
        /// </summary>
        public static int MaxDocumentSize
        {
            get { return __maxDocumentSize; }
            set { __maxDocumentSize = value; }
        }

        /// <summary>
        /// Gets or sets the default max serialization depth (used to detect circular references during serialization). The default is 100.
        /// </summary>
        public static int MaxSerializationDepth
        {
            get { return __maxSerializationDepth; }
            set { __maxSerializationDepth = value; }
        }

        //public static methods
        /// <summary>
        /// Gets the dynamic array serializer.
        /// </summary>
        public static IBsonSerializer GetDynamicArraySerializer(
            bool forceStaticSerializerRegistry = false)
        {
            if (!__dynamicArraySerializerWasSet)
            {
                __dynamicArraySerializer = BsonSerializer.LookupSerializer<List<object>>(forceStaticSerializerRegistry);
            }

            return __dynamicArraySerializer;
        }

        /// <summary>
        /// Gets or sets the dynamic document serializer.
        /// </summary>
        public static IBsonSerializer GetDynamicDocumentSerializer(
            bool forceStaticSerializerRegistry = false)
        {
            if (!__dynamicDocumentSerializerWasSet)
            {
                __dynamicDocumentSerializer = BsonSerializer.LookupSerializer<ExpandoObject>(forceStaticSerializerRegistry);
            }

            return __dynamicDocumentSerializer;
        }

        /// <summary>
        /// Sets the dynamic array serializer.
        /// </summary>
        public static void SetDynamicArraySerializer(IBsonSerializer value)
        {
            __dynamicArraySerializerWasSet = true;
            __dynamicArraySerializer = value;
        }

        /// <summary>
        /// Gets or sets the dynamic document serializer.
        /// </summary>
        public static void SetDynamicDocumentSerializer(IBsonSerializer value)
        {
            __dynamicDocumentSerializerWasSet = true;
            __dynamicDocumentSerializer = value;
        }
    }
}
