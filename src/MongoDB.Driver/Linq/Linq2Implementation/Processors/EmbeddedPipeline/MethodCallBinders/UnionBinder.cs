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

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Etherna.MongoDB.Driver.Linq.Linq2Implementation.Expressions;

namespace Etherna.MongoDB.Driver.Linq.Linq2Implementation.Processors.EmbeddedPipeline.MethodCallBinders
{
    internal sealed class UnionBinder : IMethodCallBinder<EmbeddedPipelineBindingContext>
    {
        public static IEnumerable<MethodInfo> GetSupportedMethods()
        {
            yield return MethodHelper.GetMethodDefinition(() => Enumerable.Union<object>(null, null));
            yield return MethodHelper.GetMethodDefinition(() => Queryable.Union<object>(null, null));
        }

        public Expression Bind(PipelineExpression pipeline, EmbeddedPipelineBindingContext bindingContext, MethodCallExpression node, IEnumerable<Expression> arguments)
        {
            var value = bindingContext.Bind(arguments.Single());

            return new PipelineExpression(
                new UnionExpression(
                    pipeline.Source,
                    value),
                pipeline.Projector);
        }
    }
}
