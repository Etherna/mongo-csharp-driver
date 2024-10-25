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
        private static int __maxDocumentSize = int.MaxValue;
        private static int __maxSerializationDepth = 100;

        // public static properties
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
