﻿/* Copyright 2015-present MongoDB Inc.
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
using System.Linq.Expressions;
using Etherna.MongoDB.Driver.Linq.Linq2Implementation.Expressions;

namespace Etherna.MongoDB.Driver.Linq.Linq2Implementation.Processors
{
    internal sealed class CorrelatedAccumulatorRemover : ExtensionExpressionVisitor
    {
        public static Expression Remove(Expression node, Guid correlationId)
        {
            var remover = new CorrelatedAccumulatorRemover(correlationId);
            return remover.Visit(node);
        }

        private readonly Guid _correlationId;

        private CorrelatedAccumulatorRemover(Guid correlationId)
        {
            _correlationId = correlationId;
        }

        protected internal override Expression VisitCorrelated(CorrelatedExpression node)
        {
            if (node.CorrelationId == _correlationId && node.Expression is AccumulatorExpression)
            {
                return Visit(node.Expression);
            }

            return base.VisitCorrelated(node);
        }
    }
}
