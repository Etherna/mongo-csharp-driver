/* Copyright 2015-present MongoDB Inc.
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
* skipations under the License.
*/

using System;
using System.Linq.Expressions;
using Etherna.MongoDB.Driver.Core.Misc;

namespace Etherna.MongoDB.Driver.Linq.Linq2Implementation.Expressions
{
    internal sealed class SkipExpression : ExtensionExpression, ISourcedExpression
    {
        private readonly Expression _count;
        private readonly Expression _source;

        public SkipExpression(Expression source, Expression count)
        {
            _source = Ensure.IsNotNull(source, nameof(source));
            _count = Ensure.IsNotNull(count, nameof(count));
        }

        public override ExtensionExpressionType ExtensionType
        {
            get { return ExtensionExpressionType.Skip; }
        }

        public Expression Count
        {
            get { return _count; }
        }

        public Expression Source
        {
            get { return _source; }
        }

        public override Type Type
        {
            get { return _source.Type; }
        }

        public override string ToString()
        {
            return string.Format("{0}.Skip({1})", _source.ToString(), _count.ToString());
        }

        public SkipExpression Update(Expression source, Expression count)
        {
            if (source != _source ||
                count != _count)
            {
                return new SkipExpression(source, count);
            }

            return this;
        }

        protected internal override Expression Accept(ExtensionExpressionVisitor visitor)
        {
            return visitor.VisitSkip(this);
        }
    }
}
