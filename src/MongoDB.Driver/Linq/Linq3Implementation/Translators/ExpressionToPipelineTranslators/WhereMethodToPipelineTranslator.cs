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
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Ast;
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Ast.Stages;
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Misc;
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Reflection;
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToFilterTranslators;

namespace Etherna.MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToPipelineTranslators
{
    internal static class WhereMethodToPipelineTranslator
    {
        // public static methods
        public static TranslatedPipeline Translate(TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.Is(QueryableMethod.Where))
            {
                var sourceExpression = arguments[0];
                var pipeline = ExpressionToPipelineTranslator.Translate(context, sourceExpression);
                ClientSideProjectionHelper.ThrowIfClientSideProjection(expression, pipeline, method);

                var predicateLambda = ExpressionHelper.UnquoteLambda(arguments[1]);
                var predicateFilter = ExpressionToFilterTranslator.TranslateLambda(context, predicateLambda, parameterSerializer: pipeline.OutputSerializer, asRoot: true);

                pipeline = pipeline.AddStage(
                    AstStage.Match(predicateFilter),
                    pipeline.OutputSerializer);

                return pipeline;
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}
