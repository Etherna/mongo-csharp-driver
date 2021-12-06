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
using System.Collections.Generic;
using System.Reflection;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Driver.Linq.Linq2Implementation.Expressions;
using Etherna.MongoDB.Driver.Linq.Linq2Implementation.Expressions.ResultOperators;

namespace Etherna.MongoDB.Driver.Linq.Linq2Implementation.Processors.Pipeline.MethodCallBinders
{
    internal sealed class SumBinder : SelectingResultOperatorBinderBase
    {
        public static IEnumerable<MethodInfo> GetSupportedMethods()
        {
            return MethodHelper.GetEnumerableAndQueryableMethodDefinitions("Sum");
        }

        protected override ResultOperator CreateResultOperator(Type resultType, IBsonSerializer serializer)
        {
            return new SumResultOperator(resultType, serializer);
        }

        protected override AccumulatorType GetAccumulatorType()
        {
            return AccumulatorType.Sum;
        }
    }
}
