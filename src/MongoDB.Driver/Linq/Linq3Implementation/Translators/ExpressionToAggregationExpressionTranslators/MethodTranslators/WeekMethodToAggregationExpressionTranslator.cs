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

using System.Linq.Expressions;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Misc;
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Reflection;

namespace Etherna.MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToAggregationExpressionTranslators.MethodTranslators
{
    internal static class WeekMethodToAggregationExpressionTranslator
    {
        public static TranslatedExpression Translate(TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.IsOneOf(DateTimeMethod.Week, DateTimeMethod.WeekWithTimezone))
            {
                var dateExpression = arguments[0];
                var dateTranslation = ExpressionToAggregationExpressionTranslator.Translate(context, dateExpression);

                TranslatedExpression timezoneTranslation = null;
                if (method.Is(DateTimeMethod.WeekWithTimezone))
                {
                    var timezoneExpression = arguments[1];
                    timezoneTranslation = ExpressionToAggregationExpressionTranslator.Translate(context, timezoneExpression);
                }

                var ast = AstExpression.DatePart(AstDatePart.Week, dateTranslation.Ast, timezoneTranslation?.Ast);
                return new TranslatedExpression(expression, ast, Int32Serializer.Instance);
            }
            throw new ExpressionNotSupportedException(expression);
        }
    }
}
