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

using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Driver.Core.Misc;
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Ast.Visitors;

namespace Etherna.MongoDB.Driver.Linq.Linq3Implementation.Ast.Filters
{
    internal sealed class AstGeoWithinCenterSphereFilterOperation : AstFilterOperation
    {
        private readonly BsonValue _radius;
        private readonly BsonValue _x;
        private readonly BsonValue _y;

        public AstGeoWithinCenterSphereFilterOperation(BsonValue x, BsonValue y, BsonValue radius)
        {
            _x = Ensure.IsNotNull(x, nameof(x));
            _y = Ensure.IsNotNull(y, nameof(y));
            _radius = Ensure.IsNotNull(radius, nameof(radius));
        }

        public override AstNodeType NodeType => AstNodeType.GeoWithinCenterSphereFilterOperation;
        public BsonValue Radius => _radius;
        public BsonValue X => _x;
        public BsonValue Y => _y;

        public override AstNode Accept(AstNodeVisitor visitor)
        {
            return visitor.VisitGeoWithinCenterSphereFilterOperation(this);
        }

        public override BsonValue Render()
        {
            return new BsonDocument("$geoWithin", new BsonDocument("$centerSphere", new BsonArray { new BsonArray { _x, _y }, _radius }));
        }
    }
}
