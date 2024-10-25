/* Copyright 2010-present MongoDB Inc.
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
using System.Linq;
using System.Linq.Expressions;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Bson.Serialization.Conventions;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Ast.Filters;
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Misc;
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Reflection;
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Serializers;
using Etherna.MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToFilterTranslators.ToFilterFieldTranslators;

namespace Etherna.MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToFilterTranslators.MethodTranslators
{
    internal static class AllOrAnyMethodToFilterTranslator
    {
        public static AstFilter Translate(TranslationContext context, MethodCallExpression expression)
        {
            if (AllWithContainsInPredicateMethodToFilterTranslator.CanTranslate(expression, out var arrayFieldExpression, out var arrayConstantExpression))
            {
                return AllWithContainsInPredicateMethodToFilterTranslator.Translate(context, arrayFieldExpression, arrayConstantExpression);
            }

            if (AnyWithContainsInPredicateMethodToFilterTranslator.CanTranslate(expression, out arrayFieldExpression, out arrayConstantExpression))
            {
                return AnyWithContainsInPredicateMethodToFilterTranslator.Translate(context, arrayFieldExpression, arrayConstantExpression);
            }

            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.IsOneOf(EnumerableMethod.All, EnumerableMethod.Any, EnumerableMethod.AnyWithPredicate, ArrayMethod.Exists) || ListMethod.IsExistsMethod(method))
            {
                var sourceExpression = method.IsStatic ? arguments[0] : expression.Object;
                var (field, filter) = FilteredEnumerableFilterFieldTranslator.Translate(context, sourceExpression);

                if (method.IsOneOf(EnumerableMethod.All, EnumerableMethod.AnyWithPredicate, ArrayMethod.Exists) || ListMethod.IsExistsMethod(method))
                {
                    var predicateLambda = (LambdaExpression)(method.IsStatic ? arguments[1] : arguments[0]);
                    var parameterExpression = predicateLambda.Parameters.Single();
                    var elementSerializer = ArraySerializerHelper.GetItemSerializer(field.Serializer);
                    var parameterSymbol = context.CreateSymbol(parameterExpression, "@<elem>", elementSerializer); // @<elem> represents the implied element
                    var predicateContext = context.WithSingleSymbol(parameterSymbol); // @<elem> is the only symbol visible inside an $elemMatch
                    var predicateFilter = ExpressionToFilterTranslator.Translate(predicateContext, predicateLambda.Body, exprOk: false);

                    filter = AstFilter.Combine(filter, predicateFilter);
                }

                if (method.Is(EnumerableMethod.All))
                {
                    return AstFilter.Not(AstFilter.ElemMatch(field, AstFilter.Not(filter)));
                }
                else
                {
                    if (filter == null)
                    {
                        return AstFilter.And(AstFilter.Ne(field, BsonNull.Value), AstFilter.Not(AstFilter.Size(field, 0)));
                    }
                    else
                    {
                        // { $elemMatch : { $or : [{ $eq : x }, { $eq : y }, ... ] } } => { $in : [x, y, ...] }
                        if (filter is AstOrFilter orFilter &&
                            orFilter.Filters.All(IsImpliedElementEqualityComparison))
                        {
                            var values = orFilter.Filters
                                .Select(filter => ((AstFieldOperationFilter)filter).Operation)
                                .Select(operation => ((AstComparisonFilterOperation)operation).Value);

                            return AstFilter.In(field, values);
                        }

                        return AstFilter.ElemMatch(field, filter);
                    }
                }
            }

            throw new ExpressionNotSupportedException(expression);

            static bool IsImpliedElementEqualityComparison(AstFilter filter)
                =>
                    filter is AstFieldOperationFilter fieldOperationFilter &&
                    fieldOperationFilter.Field.Path == "@<elem>" &&
                    fieldOperationFilter.Operation is AstComparisonFilterOperation comparisonFilterOperation &&
                    comparisonFilterOperation.Operator == AstComparisonFilterOperator.Eq;
        }
    }

    internal static class FilteredEnumerableFilterFieldTranslator
    {
        public static (AstFilterField, AstFilter) Translate(TranslationContext context, Expression sourceExpression)
        {
            if (sourceExpression is MethodCallExpression sourceMethodCallExpression)
            {
                var method = sourceMethodCallExpression.Method;
                var arguments = sourceMethodCallExpression.Arguments;

                if (method.Is(EnumerableMethod.OfType))
                {
                    var ofTypeSourceExpression = arguments[0];
                    var (sourceField, sourceFilter) = Translate(context, ofTypeSourceExpression);

                    var nominalType = ArraySerializerHelper.GetItemSerializer(sourceField.Serializer).ValueType;
                    var actualType = method.GetGenericArguments()[0];
                    var sourceSerializer = sourceField.Serializer;
                    var itemSerializer = ArraySerializerHelper.GetItemSerializer(sourceSerializer);

                    var discriminatorConvention = itemSerializer.GetDiscriminatorConvention();
                    var discriminatorField = AstFilter.Field(discriminatorConvention.ElementName, BsonValueSerializer.Instance);

                    var ofTypeFilter = discriminatorConvention switch
                    {
                        IHierarchicalDiscriminatorConvention hierarchicalDiscriminatorConvention => DiscriminatorAstFilter.TypeIs(discriminatorField, hierarchicalDiscriminatorConvention, nominalType, actualType),
                        IScalarDiscriminatorConvention scalarDiscriminatorConvention => DiscriminatorAstFilter.TypeIs(discriminatorField, scalarDiscriminatorConvention, nominalType, actualType),
                        _ => throw new ExpressionNotSupportedException(sourceExpression, because: "OfType method is not supported with the configured discriminator convention")
                    };

                    var actualTypeSerializer = context.KnownSerializersRegistry.GetSerializer(sourceExpression);
                    var enumerableActualTypeSerializer = IEnumerableSerializer.Create(actualTypeSerializer);
                    var actualTypeSourceField = AstFilter.Field(sourceField.Path, enumerableActualTypeSerializer);
                    var combinedFilter = AstFilter.Combine(sourceFilter, ofTypeFilter);

                    return (actualTypeSourceField, combinedFilter);
                }

                if (method.Is(EnumerableMethod.Where))
                {
                    var whereSourceExpression = arguments[0];
                    var (sourceField, sourceFilter) = Translate(context, whereSourceExpression);

                    var predicateLambda = (LambdaExpression)arguments[1];
                    var parameterExpression = predicateLambda.Parameters.Single();
                    var itemSerializer = ArraySerializerHelper.GetItemSerializer(sourceField.Serializer);
                    var parameterSymbol = context.CreateSymbol(parameterExpression, "@<elem>", itemSerializer); // @<elem> represents the implied element
                    var predicateContext = context.WithSingleSymbol(parameterSymbol); // @<elem> is the only symbol visible inside an $elemMatch
                    var whereFilter = ExpressionToFilterTranslator.Translate(predicateContext, predicateLambda.Body, exprOk: false);
                    var combinedFilter = AstFilter.Combine(sourceFilter, whereFilter);

                    return (sourceField, combinedFilter);
                }
            }

            var field = ExpressionToFilterFieldTranslator.Translate(context, sourceExpression);
            return (field, null);
        }
    }
}
