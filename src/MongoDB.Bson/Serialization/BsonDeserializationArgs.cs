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
using Etherna.MongoDB.Bson.IO;

namespace Etherna.MongoDB.Bson.Serialization
{
    /// <summary>
    /// Represents args common to all serializers.
    /// </summary>
    public struct BsonDeserializationArgs
    {
        // private fields
        private bool _forceStaticSerializerRegistry;
        private Type _nominalType;

        // constructors
        private BsonDeserializationArgs(
            bool forceStaticSerializerRegistry,
            Type nominalType)
        {
            _forceStaticSerializerRegistry = forceStaticSerializerRegistry;
            _nominalType = nominalType;
        }

        // public properties
        /// <summary>
        /// Gets or sets when to force using of static serialization registry
        /// </summary>
        public bool ForceStaticSerializerRegistry
        {
            get { return _forceStaticSerializerRegistry; }
            set { _forceStaticSerializerRegistry = value; }
        }

        /// <summary>
        /// Gets or sets the nominal type.
        /// </summary>
        /// <value>
        /// The nominal type.
        /// </value>
        public Type NominalType
        {
            get { return _nominalType; }
            set { _nominalType = value; }
        }
    }
}
