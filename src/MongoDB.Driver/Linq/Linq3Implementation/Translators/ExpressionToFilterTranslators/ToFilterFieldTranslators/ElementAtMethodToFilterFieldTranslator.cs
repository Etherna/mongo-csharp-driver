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
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Ast.Filters;
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Misc;
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Reflection;

namespace Etherna.MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToFilterTranslators.ToFilterFieldTranslators
{
    internal static class ElementAtMethodToFilterFieldTranslator
    {
        public static TranslatedFilterField Translate(TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.Is(EnumerableMethod.ElementAt))
            {
                var sourceExpression = arguments[0];
                var indexExpression = arguments[1];

                return ArrayIndexExpressionToFilterFieldTranslator.Translate(context, expression, fieldExpression: sourceExpression, indexExpression);
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}
