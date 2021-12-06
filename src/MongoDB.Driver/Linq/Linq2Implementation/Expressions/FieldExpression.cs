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
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Driver.Core.Misc;

namespace Etherna.MongoDB.Driver.Linq.Linq2Implementation.Expressions
{
    internal sealed class FieldExpression : SerializationExpression, IFieldExpression, IHasOutOfCurrentScopePrefix
    {
        private readonly Expression _document;
        private readonly string _fieldName;
        private readonly Expression _original;
        private readonly IBsonSerializer _serializer;
        private string _outOfCurrentScopePrefix;

        public FieldExpression(string fieldName, IBsonSerializer serializer)
            : this(null, fieldName, serializer, null)
        {
        }

        public FieldExpression(Expression document, string fieldName, IBsonSerializer serializer)
            : this(document, fieldName, serializer, null)
        {
        }

        public FieldExpression(string fieldName, IBsonSerializer serializer, Expression original)
            : this(null, fieldName, serializer, original)
        {
        }

        public FieldExpression(Expression document, string fieldName, IBsonSerializer serializer, Expression original)
        {
            _document = document;
            _fieldName = Ensure.IsNotNull(fieldName, nameof(fieldName));
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
            _original = original;
        }

        public Expression Document
        {
            get { return _document; }
        }

        public string FieldName
        {
            get { return _fieldName; }
        }

        public override ExtensionExpressionType ExtensionType
        {
            get { return ExtensionExpressionType.Field; }
        }

        public Expression Original
        {
            get { return _original; }
        }

        public string OutOfCurrentScopePrefix
        {
            get { return _outOfCurrentScopePrefix; }
            set { _outOfCurrentScopePrefix = value; }
        }

        public override IBsonSerializer Serializer
        {
            get { return _serializer; }
        }

        public override Type Type
        {
            get { return _serializer.ValueType; }
        }

        public override string ToString()
        {
            return _document?.ToString() + "{" + _fieldName + "}";
        }

        public FieldExpression Update(Expression document, Expression original)
        {
            if (document != _document || original != _original)
            {
                return new FieldExpression(document, _fieldName, _serializer, original);
            }

            return this;
        }

        public FieldExpression WithSerializer(IBsonSerializer serializer)
        {
            Ensure.IsNotNull(serializer, nameof(serializer));
            return new FieldExpression(
                _document,
                _fieldName,
                serializer,
                _original);
        }

        protected internal override Expression Accept(ExtensionExpressionVisitor visitor)
        {
            return visitor.VisitField(this);
        }
    }
}
