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

using System.Linq;
using System.Linq.Expressions;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Bson.Serialization.Conventions;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Ast.Filters;
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToFilterTranslators.ToFilterFieldTranslators;

namespace Etherna.MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToFilterTranslators.ExpressionTranslators
{
    internal static class TypeIsExpressionToFilterTranslator
    {
        public static AstFilter Translate(TranslationContext context, TypeBinaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.TypeIs)
            {
                var fieldExpression = expression.Expression;
                var fieldTranslation = ExpressionToFilterFieldTranslator.Translate(context, fieldExpression);
                var nominalType = fieldExpression.Type;
                var actualType = expression.TypeOperand;

                if (nominalType == actualType)
                {
                    return AstFilter.MatchesEverything();
                }
                else
                {
                    var discriminatorConvention = fieldTranslation.Serializer.GetDiscriminatorConvention();
                    var discriminatorField = fieldTranslation.Ast.SubField(discriminatorConvention.ElementName);

                    return discriminatorConvention switch
                    {
                        IHierarchicalDiscriminatorConvention hierarchicalDiscriminatorConvention => DiscriminatorAstFilter.TypeIs(discriminatorField, hierarchicalDiscriminatorConvention, nominalType, actualType),
                        IScalarDiscriminatorConvention scalarDiscriminatorConvention => DiscriminatorAstFilter.TypeIs(discriminatorField, scalarDiscriminatorConvention, nominalType, actualType),
                        _ => throw new ExpressionNotSupportedException(expression, because: "is operator is not supported with the configured discriminator convention")
                    };
                }
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}
