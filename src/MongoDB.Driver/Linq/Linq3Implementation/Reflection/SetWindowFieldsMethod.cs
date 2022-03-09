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

using System;
using System.Reflection;

namespace Etherna.MongoDB.Driver.Linq.Linq3Implementation.Reflection
{
    internal static class SetWindowFieldsMethod
    {
        // private static fields
        private static readonly MethodInfo __addToSet;
        private static readonly MethodInfo __averageWithDecimal;
        private static readonly MethodInfo __averageWithDouble;
        private static readonly MethodInfo __averageWithInt32;
        private static readonly MethodInfo __averageWithInt64;
        private static readonly MethodInfo __averageWithNullableDecimal;
        private static readonly MethodInfo __averageWithNullableDouble;
        private static readonly MethodInfo __averageWithNullableInt32;
        private static readonly MethodInfo __averageWithNullableInt64;
        private static readonly MethodInfo __averageWithNullableSingle;
        private static readonly MethodInfo __averageWithSingle;
        private static readonly MethodInfo __count;
        private static readonly MethodInfo __covariancePopulationWithDecimals;
        private static readonly MethodInfo __covariancePopulationWithDoubles;
        private static readonly MethodInfo __covariancePopulationWithInt32s;
        private static readonly MethodInfo __covariancePopulationWithInt64s;
        private static readonly MethodInfo __covariancePopulationWithNullableDecimals;
        private static readonly MethodInfo __covariancePopulationWithNullableDoubles;
        private static readonly MethodInfo __covariancePopulationWithNullableInt32s;
        private static readonly MethodInfo __covariancePopulationWithNullableInt64s;
        private static readonly MethodInfo __covariancePopulationWithNullableSingles;
        private static readonly MethodInfo __covariancePopulationWithSingles;
        private static readonly MethodInfo __covarianceSampleWithDecimals;
        private static readonly MethodInfo __covarianceSampleWithDoubles;
        private static readonly MethodInfo __covarianceSampleWithInt32s;
        private static readonly MethodInfo __covarianceSampleWithInt64s;
        private static readonly MethodInfo __covarianceSampleWithNullableDecimals;
        private static readonly MethodInfo __covarianceSampleWithNullableDoubles;
        private static readonly MethodInfo __covarianceSampleWithNullableInt32s;
        private static readonly MethodInfo __covarianceSampleWithNullableInt64s;
        private static readonly MethodInfo __covarianceSampleWithNullableSingles;
        private static readonly MethodInfo __covarianceSampleWithSingles;
        private static readonly MethodInfo __denseRank;
        private static readonly MethodInfo __derivativeWithDecimal;
        private static readonly MethodInfo __derivativeWithDecimalAndUnit;
        private static readonly MethodInfo __derivativeWithDouble;
        private static readonly MethodInfo __derivativeWithDoubleAndUnit;
        private static readonly MethodInfo __derivativeWithInt32;
        private static readonly MethodInfo __derivativeWithInt32AndUnit;
        private static readonly MethodInfo __derivativeWithInt64;
        private static readonly MethodInfo __derivativeWithInt64AndUnit;
        private static readonly MethodInfo __derivativeWithSingle;
        private static readonly MethodInfo __derivativeWithSingleAndUnit;
        private static readonly MethodInfo __documentNumber;
        private static readonly MethodInfo __exponentialMovingAverageWithDecimal;
        private static readonly MethodInfo __exponentialMovingAverageWithDouble;
        private static readonly MethodInfo __exponentialMovingAverageWithInt32;
        private static readonly MethodInfo __exponentialMovingAverageWithInt64;
        private static readonly MethodInfo __exponentialMovingAverageWithSingle;
        private static readonly MethodInfo __first;
        private static readonly MethodInfo __integralWithDecimal;
        private static readonly MethodInfo __integralWithDecimalAndUnit;
        private static readonly MethodInfo __integralWithDouble;
        private static readonly MethodInfo __integralWithDoubleAndUnit;
        private static readonly MethodInfo __integralWithInt32;
        private static readonly MethodInfo __integralWithInt32AndUnit;
        private static readonly MethodInfo __integralWithInt64;
        private static readonly MethodInfo __integralWithInt64AndUnit;
        private static readonly MethodInfo __integralWithSingle;
        private static readonly MethodInfo __integralWithSingleAndUnit;
        private static readonly MethodInfo __last;
        private static readonly MethodInfo __max;
        private static readonly MethodInfo __min;
        private static readonly MethodInfo __push;
        private static readonly MethodInfo __rank;
        private static readonly MethodInfo __shift;
        private static readonly MethodInfo __shiftWithDefaultValue;
        private static readonly MethodInfo __standardDeviationPopulationWithDecimal;
        private static readonly MethodInfo __standardDeviationPopulationWithDouble;
        private static readonly MethodInfo __standardDeviationPopulationWithInt32;
        private static readonly MethodInfo __standardDeviationPopulationWithInt64;
        private static readonly MethodInfo __standardDeviationPopulationWithNullableDecimal;
        private static readonly MethodInfo __standardDeviationPopulationWithNullableDouble;
        private static readonly MethodInfo __standardDeviationPopulationWithNullableInt32;
        private static readonly MethodInfo __standardDeviationPopulationWithNullableInt64;
        private static readonly MethodInfo __standardDeviationPopulationWithNullableSingle;
        private static readonly MethodInfo __standardDeviationPopulationWithSingle;
        private static readonly MethodInfo __standardDeviationSampleWithDecimal;
        private static readonly MethodInfo __standardDeviationSampleWithDouble;
        private static readonly MethodInfo __standardDeviationSampleWithInt32;
        private static readonly MethodInfo __standardDeviationSampleWithInt64;
        private static readonly MethodInfo __standardDeviationSampleWithNullableDecimal;
        private static readonly MethodInfo __standardDeviationSampleWithNullableDouble;
        private static readonly MethodInfo __standardDeviationSampleWithNullableInt32;
        private static readonly MethodInfo __standardDeviationSampleWithNullableInt64;
        private static readonly MethodInfo __standardDeviationSampleWithNullableSingle;
        private static readonly MethodInfo __standardDeviationSampleWithSingle;
        private static readonly MethodInfo __sumWithDecimal;
        private static readonly MethodInfo __sumWithDouble;
        private static readonly MethodInfo __sumWithInt32;
        private static readonly MethodInfo __sumWithInt64;
        private static readonly MethodInfo __sumWithNullableDecimal;
        private static readonly MethodInfo __sumWithNullableDouble;
        private static readonly MethodInfo __sumWithNullableInt32;
        private static readonly MethodInfo __sumWithNullableInt64;
        private static readonly MethodInfo __sumWithNullableSingle;
        private static readonly MethodInfo __sumWithSingle;

        // static constructor
        static SetWindowFieldsMethod()
        {
            __addToSet = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, SetWindowFieldsWindow window) => partition.AddToSet(selector, window));
            __averageWithDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector, SetWindowFieldsWindow window) => partition.Average(selector, window));
            __averageWithDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector, SetWindowFieldsWindow window) => partition.Average(selector, window));
            __averageWithInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector, SetWindowFieldsWindow window) => partition.Average(selector, window));
            __averageWithInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector, SetWindowFieldsWindow window) => partition.Average(selector, window));
            __averageWithNullableDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal?> selector, SetWindowFieldsWindow window) => partition.Average(selector, window));
            __averageWithNullableDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double?> selector, SetWindowFieldsWindow window) => partition.Average(selector, window));
            __averageWithNullableInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int?> selector, SetWindowFieldsWindow window) => partition.Average(selector, window));
            __averageWithNullableInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long?> selector, SetWindowFieldsWindow window) => partition.Average(selector, window));
            __averageWithNullableSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float?> selector, SetWindowFieldsWindow window) => partition.Average(selector, window));
            __averageWithSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector, SetWindowFieldsWindow window) => partition.Average(selector, window));
            __count = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, SetWindowFieldsWindow window) => partition.Count(window));
            __covariancePopulationWithDecimals = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector1, Func<object, decimal> selector2, SetWindowFieldsWindow window) => partition.CovariancePopulation(selector1, selector2, window));
            __covariancePopulationWithDoubles = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector1, Func<object, double> selector2, SetWindowFieldsWindow window) => partition.CovariancePopulation(selector1, selector2, window));
            __covariancePopulationWithInt32s = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector1, Func<object, int> selector2, SetWindowFieldsWindow window) => partition.CovariancePopulation(selector1, selector2, window));
            __covariancePopulationWithInt64s = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector1, Func<object, long> selector2, SetWindowFieldsWindow window) => partition.CovariancePopulation(selector1, selector2, window));
            __covariancePopulationWithNullableDecimals = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal?> selector1, Func<object, decimal?> selector2, SetWindowFieldsWindow window) => partition.CovariancePopulation(selector1, selector2, window));
            __covariancePopulationWithNullableDoubles = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double?> selector1, Func<object, double?> selector2, SetWindowFieldsWindow window) => partition.CovariancePopulation(selector1, selector2, window));
            __covariancePopulationWithNullableInt32s = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int?> selector1, Func<object, int?> selector2, SetWindowFieldsWindow window) => partition.CovariancePopulation(selector1, selector2, window));
            __covariancePopulationWithNullableInt64s = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long?> selector1, Func<object, long?> selector2, SetWindowFieldsWindow window) => partition.CovariancePopulation(selector1, selector2, window));
            __covariancePopulationWithNullableSingles = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float?> selector1, Func<object, float?> selector2, SetWindowFieldsWindow window) => partition.CovariancePopulation(selector1, selector2, window));
            __covariancePopulationWithSingles = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector1, Func<object, float> selector2, SetWindowFieldsWindow window) => partition.CovariancePopulation(selector1, selector2, window));
            __covarianceSampleWithDecimals = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector1, Func<object, decimal> selector2, SetWindowFieldsWindow window) => partition.CovarianceSample(selector1, selector2, window));
            __covarianceSampleWithDoubles = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector1, Func<object, double> selector2, SetWindowFieldsWindow window) => partition.CovarianceSample(selector1, selector2, window));
            __covarianceSampleWithInt32s = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector1, Func<object, int> selector2, SetWindowFieldsWindow window) => partition.CovarianceSample(selector1, selector2, window));
            __covarianceSampleWithInt64s = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector1, Func<object, long> selector2, SetWindowFieldsWindow window) => partition.CovarianceSample(selector1, selector2, window));
            __covarianceSampleWithNullableDecimals = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal?> selector1, Func<object, decimal?> selector2, SetWindowFieldsWindow window) => partition.CovarianceSample(selector1, selector2, window));
            __covarianceSampleWithNullableDoubles = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double?> selector1, Func<object, double?> selector2, SetWindowFieldsWindow window) => partition.CovarianceSample(selector1, selector2, window));
            __covarianceSampleWithNullableInt32s = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int?> selector1, Func<object, int?> selector2, SetWindowFieldsWindow window) => partition.CovarianceSample(selector1, selector2, window));
            __covarianceSampleWithNullableInt64s = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long?> selector1, Func<object, long?> selector2, SetWindowFieldsWindow window) => partition.CovarianceSample(selector1, selector2, window));
            __covarianceSampleWithNullableSingles = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float?> selector1, Func<object, float?> selector2, SetWindowFieldsWindow window) => partition.CovarianceSample(selector1, selector2, window));
            __covarianceSampleWithSingles = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector1, Func<object, float> selector2, SetWindowFieldsWindow window) => partition.CovarianceSample(selector1, selector2, window));
            __denseRank = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition) => partition.DenseRank());
            __derivativeWithDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector, SetWindowFieldsWindow window) => partition.Derivative(selector, window));
            __derivativeWithDecimalAndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector, WindowTimeUnit unit, SetWindowFieldsWindow window) => partition.Derivative(selector, unit, window));
            __derivativeWithDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector, SetWindowFieldsWindow window) => partition.Derivative(selector, window));
            __derivativeWithDoubleAndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector, WindowTimeUnit unit, SetWindowFieldsWindow window) => partition.Derivative(selector, unit, window));
            __derivativeWithInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector, SetWindowFieldsWindow window) => partition.Derivative(selector, window));
            __derivativeWithInt32AndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector, WindowTimeUnit unit, SetWindowFieldsWindow window) => partition.Derivative(selector, unit, window));
            __derivativeWithInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector, SetWindowFieldsWindow window) => partition.Derivative(selector, window));
            __derivativeWithInt64AndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector, WindowTimeUnit unit, SetWindowFieldsWindow window) => partition.Derivative(selector, unit, window));
            __derivativeWithSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector, SetWindowFieldsWindow window) => partition.Derivative(selector, window));
            __derivativeWithSingleAndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector, WindowTimeUnit unit, SetWindowFieldsWindow window) => partition.Derivative(selector, unit, window));;
            __documentNumber = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition) => partition.DocumentNumber());
            __exponentialMovingAverageWithDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector, ExponentialMovingAverageWeighting weighting, SetWindowFieldsWindow window) => partition.ExponentialMovingAverage(selector, weighting, window));
            __exponentialMovingAverageWithDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector, ExponentialMovingAverageWeighting weighting, SetWindowFieldsWindow window) => partition.ExponentialMovingAverage(selector, weighting, window));
            __exponentialMovingAverageWithInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector, ExponentialMovingAverageWeighting weighting, SetWindowFieldsWindow window) => partition.ExponentialMovingAverage(selector, weighting, window));
            __exponentialMovingAverageWithInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector, ExponentialMovingAverageWeighting weighting, SetWindowFieldsWindow window) => partition.ExponentialMovingAverage(selector, weighting, window));
            __exponentialMovingAverageWithSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector, ExponentialMovingAverageWeighting weighting, SetWindowFieldsWindow window) => partition.ExponentialMovingAverage(selector, weighting, window));
            __first = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, SetWindowFieldsWindow window) => partition.First(selector, window));
            __integralWithDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector, SetWindowFieldsWindow window) => partition.Integral(selector, window));
            __integralWithDecimalAndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector, WindowTimeUnit unit, SetWindowFieldsWindow window) => partition.Integral(selector, unit, window));
            __integralWithDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector, SetWindowFieldsWindow window) => partition.Integral(selector, window));
            __integralWithDoubleAndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector, WindowTimeUnit unit, SetWindowFieldsWindow window) => partition.Integral(selector, unit, window));
            __integralWithInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector, SetWindowFieldsWindow window) => partition.Integral(selector, window));
            __integralWithInt32AndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector, WindowTimeUnit unit, SetWindowFieldsWindow window) => partition.Integral(selector, unit, window));
            __integralWithInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector, SetWindowFieldsWindow window) => partition.Integral(selector, window));
            __integralWithInt64AndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector, WindowTimeUnit unit, SetWindowFieldsWindow window) => partition.Integral(selector, unit, window));
            __integralWithSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector, SetWindowFieldsWindow window) => partition.Integral(selector, window));
            __integralWithSingleAndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector, WindowTimeUnit unit, SetWindowFieldsWindow window) => partition.Integral(selector, unit, window)); ;
            __last = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, SetWindowFieldsWindow window) => partition.Last(selector, window));
            __max = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, SetWindowFieldsWindow window) => partition.Max(selector, window));
            __min = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, SetWindowFieldsWindow window) => partition.Min(selector, window));
            __push = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, SetWindowFieldsWindow window) => partition.Push(selector, window));
            __rank = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition) => partition.Rank());
            __shift = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, int by) => partition.Shift(selector, by));
            __shiftWithDefaultValue = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, int by, object defaultValue) => partition.Shift(selector, by, defaultValue));
            __standardDeviationPopulationWithDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector, SetWindowFieldsWindow window) => partition.StandardDeviationPopulation(selector, window));
            __standardDeviationPopulationWithDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector, SetWindowFieldsWindow window) => partition.StandardDeviationPopulation(selector, window));
            __standardDeviationPopulationWithInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector, SetWindowFieldsWindow window) => partition.StandardDeviationPopulation(selector, window));
            __standardDeviationPopulationWithInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector, SetWindowFieldsWindow window) => partition.StandardDeviationPopulation(selector, window));
            __standardDeviationPopulationWithNullableDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal?> selector, SetWindowFieldsWindow window) => partition.StandardDeviationPopulation(selector, window));
            __standardDeviationPopulationWithNullableDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double?> selector, SetWindowFieldsWindow window) => partition.StandardDeviationPopulation(selector, window));
            __standardDeviationPopulationWithNullableInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int?> selector, SetWindowFieldsWindow window) => partition.StandardDeviationPopulation(selector, window));
            __standardDeviationPopulationWithNullableInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long?> selector, SetWindowFieldsWindow window) => partition.StandardDeviationPopulation(selector, window));
            __standardDeviationPopulationWithNullableSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float?> selector, SetWindowFieldsWindow window) => partition.StandardDeviationPopulation(selector, window));
            __standardDeviationPopulationWithSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector, SetWindowFieldsWindow window) => partition.StandardDeviationPopulation(selector, window));
            __standardDeviationSampleWithDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector, SetWindowFieldsWindow window) => partition.StandardDeviationSample(selector, window));
            __standardDeviationSampleWithDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector, SetWindowFieldsWindow window) => partition.StandardDeviationSample(selector, window));
            __standardDeviationSampleWithInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector, SetWindowFieldsWindow window) => partition.StandardDeviationSample(selector, window));
            __standardDeviationSampleWithInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector, SetWindowFieldsWindow window) => partition.StandardDeviationSample(selector, window));
            __standardDeviationSampleWithNullableDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal?> selector, SetWindowFieldsWindow window) => partition.StandardDeviationSample(selector, window));
            __standardDeviationSampleWithNullableDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double?> selector, SetWindowFieldsWindow window) => partition.StandardDeviationSample(selector, window));
            __standardDeviationSampleWithNullableInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int?> selector, SetWindowFieldsWindow window) => partition.StandardDeviationSample(selector, window));
            __standardDeviationSampleWithNullableInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long?> selector, SetWindowFieldsWindow window) => partition.StandardDeviationSample(selector, window));
            __standardDeviationSampleWithNullableSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float?> selector, SetWindowFieldsWindow window) => partition.StandardDeviationSample(selector, window));
            __standardDeviationSampleWithSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector, SetWindowFieldsWindow window) => partition.StandardDeviationSample(selector, window));
            __sumWithDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector, SetWindowFieldsWindow window) => partition.Sum(selector, window));
            __sumWithDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector, SetWindowFieldsWindow window) => partition.Sum(selector, window));
            __sumWithInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector, SetWindowFieldsWindow window) => partition.Sum(selector, window));
            __sumWithInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector, SetWindowFieldsWindow window) => partition.Sum(selector, window));
            __sumWithNullableDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal?> selector, SetWindowFieldsWindow window) => partition.Sum(selector, window));
            __sumWithNullableDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double?> selector, SetWindowFieldsWindow window) => partition.Sum(selector, window));
            __sumWithNullableInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int?> selector, SetWindowFieldsWindow window) => partition.Sum(selector, window));
            __sumWithNullableInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long?> selector, SetWindowFieldsWindow window) => partition.Sum(selector, window));
            __sumWithNullableSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float?> selector, SetWindowFieldsWindow window) => partition.Sum(selector, window));
            __sumWithSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector, SetWindowFieldsWindow window) => partition.Sum(selector, window));
        }

        // public properties
        public static MethodInfo AddToSet => __addToSet;
        public static MethodInfo AverageWithDecimal => __averageWithDecimal;
        public static MethodInfo AverageWithDouble => __averageWithDouble;
        public static MethodInfo AverageWithInt32 => __averageWithInt32;
        public static MethodInfo AverageWithInt64 => __averageWithInt64;
        public static MethodInfo AverageWithNullableDecimal => __averageWithNullableDecimal;
        public static MethodInfo AverageWithNullableDouble => __averageWithNullableDouble;
        public static MethodInfo AverageWithNullableInt32 => __averageWithNullableInt32;
        public static MethodInfo AverageWithNullableInt64 => __averageWithNullableInt64;
        public static MethodInfo AverageWithNullableSingle => __averageWithNullableSingle;
        public static MethodInfo AverageWithSingle => __averageWithSingle;
        public static MethodInfo Count => __count;
        public static MethodInfo CovariancePopulationWithDecimals => __covariancePopulationWithDecimals;
        public static MethodInfo CovariancePopulationWithDoubles => __covariancePopulationWithDoubles;
        public static MethodInfo CovariancePopulationWithInt32s => __covariancePopulationWithInt32s;
        public static MethodInfo CovariancePopulationWithInt64s => __covariancePopulationWithInt64s;
        public static MethodInfo CovariancePopulationWithNullableDecimals => __covariancePopulationWithNullableDecimals;
        public static MethodInfo CovariancePopulationWithNullableDoubles => __covariancePopulationWithNullableDoubles;
        public static MethodInfo CovariancePopulationWithNullableInt32s => __covariancePopulationWithNullableInt32s;
        public static MethodInfo CovariancePopulationWithNullableInt64s => __covariancePopulationWithNullableInt64s;
        public static MethodInfo CovariancePopulationWithNullableSingles => __covariancePopulationWithNullableSingles;
        public static MethodInfo CovariancePopulationWithSingles => __covariancePopulationWithSingles;
        public static MethodInfo CovarianceSampleWithDecimals => __covarianceSampleWithDecimals;
        public static MethodInfo CovarianceSampleWithDoubles => __covarianceSampleWithDoubles;
        public static MethodInfo CovarianceSampleWithInt32s => __covarianceSampleWithInt32s;
        public static MethodInfo CovarianceSampleWithInt64s => __covarianceSampleWithInt64s;
        public static MethodInfo CovarianceSampleWithNullableDecimals => __covarianceSampleWithNullableDecimals;
        public static MethodInfo CovarianceSampleWithNullableDoubles => __covarianceSampleWithNullableDoubles;
        public static MethodInfo CovarianceSampleWithNullableInt32s => __covarianceSampleWithNullableInt32s;
        public static MethodInfo CovarianceSampleWithNullableInt64s => __covarianceSampleWithNullableInt64s;
        public static MethodInfo CovarianceSampleWithNullableSingles => __covarianceSampleWithNullableSingles;
        public static MethodInfo CovarianceSampleWithSingles => __covarianceSampleWithSingles;
        public static MethodInfo DenseRank => __denseRank;
        public static MethodInfo DerivativeWithDecimal => __derivativeWithDecimal;
        public static MethodInfo DerivativeWithDecimalAndUnit => __derivativeWithDecimalAndUnit;
        public static MethodInfo DerivativeWithDouble => __derivativeWithDouble;
        public static MethodInfo DerivativeWithDoubleAndUnit => __derivativeWithDoubleAndUnit;
        public static MethodInfo DerivativeWithInt32 => __derivativeWithInt32;
        public static MethodInfo DerivativeWithInt32AndUnit => __derivativeWithInt32AndUnit;
        public static MethodInfo DerivativeWithInt64 => __derivativeWithInt64;
        public static MethodInfo DerivativeWithInt64AndUnit => __derivativeWithInt64AndUnit;
        public static MethodInfo DerivativeWithSingle => __derivativeWithSingle;
        public static MethodInfo DerivativeWithSingleAndUnit => __derivativeWithSingleAndUnit;
        public static MethodInfo DocumentNumber => __documentNumber;
        public static MethodInfo ExponentialMovingAverageWithDecimal => __exponentialMovingAverageWithDecimal;
        public static MethodInfo ExponentialMovingAverageWithDouble => __exponentialMovingAverageWithDouble;
        public static MethodInfo ExponentialMovingAverageWithInt32 => __exponentialMovingAverageWithInt32;
        public static MethodInfo ExponentialMovingAverageWithInt64 => __exponentialMovingAverageWithInt64;
        public static MethodInfo ExponentialMovingAverageWithSingle => __exponentialMovingAverageWithSingle;
        public static MethodInfo First => __first;
        public static MethodInfo IntegralWithDecimal => __integralWithDecimal;
        public static MethodInfo IntegralWithDecimalAndUnit => __integralWithDecimalAndUnit;
        public static MethodInfo IntegralWithDouble => __integralWithDouble;
        public static MethodInfo IntegralWithDoubleAndUnit => __integralWithDoubleAndUnit;
        public static MethodInfo IntegralWithInt32 => __integralWithInt32;
        public static MethodInfo IntegralWithInt32AndUnit => __integralWithInt32AndUnit;
        public static MethodInfo IntegralWithInt64 => __integralWithInt64;
        public static MethodInfo IntegralWithInt64AndUnit => __integralWithInt64AndUnit;
        public static MethodInfo IntegralWithSingle => __integralWithSingle;
        public static MethodInfo IntegralWithSingleAndUnit => __integralWithSingleAndUnit;
        public static MethodInfo Last => __last;
        public static MethodInfo Max => __max;
        public static MethodInfo Min => __min;
        public static MethodInfo Push => __push;
        public static MethodInfo Rank => __rank;
        public static MethodInfo Shift => __shift;
        public static MethodInfo ShiftWithDefaultValue => __shiftWithDefaultValue;
        public static MethodInfo StandardDeviationPopulationWithDecimal => __standardDeviationPopulationWithDecimal;
        public static MethodInfo StandardDeviationPopulationWithDouble => __standardDeviationPopulationWithDouble;
        public static MethodInfo StandardDeviationPopulationWithInt32 => __standardDeviationPopulationWithInt32;
        public static MethodInfo StandardDeviationPopulationWithInt64 => __standardDeviationPopulationWithInt64;
        public static MethodInfo StandardDeviationPopulationWithNullableDecimal => __standardDeviationPopulationWithNullableDecimal;
        public static MethodInfo StandardDeviationPopulationWithNullableDouble => __standardDeviationPopulationWithNullableDouble;
        public static MethodInfo StandardDeviationPopulationWithNullableInt32 => __standardDeviationPopulationWithNullableInt32;
        public static MethodInfo StandardDeviationPopulationWithNullableInt64 => __standardDeviationPopulationWithNullableInt64;
        public static MethodInfo StandardDeviationPopulationWithNullableSingle => __standardDeviationPopulationWithNullableSingle;
        public static MethodInfo StandardDeviationPopulationWithSingle => __standardDeviationPopulationWithSingle;
        public static MethodInfo StandardDeviationSampleWithDecimal => __standardDeviationSampleWithDecimal;
        public static MethodInfo StandardDeviationSampleWithDouble => __standardDeviationSampleWithDouble;
        public static MethodInfo StandardDeviationSampleWithInt32 => __standardDeviationSampleWithInt32;
        public static MethodInfo StandardDeviationSampleWithInt64 => __standardDeviationSampleWithInt64;
        public static MethodInfo StandardDeviationSampleWithNullableDecimal => __standardDeviationSampleWithNullableDecimal;
        public static MethodInfo StandardDeviationSampleWithNullableDouble => __standardDeviationSampleWithNullableDouble;
        public static MethodInfo StandardDeviationSampleWithNullableInt32 => __standardDeviationSampleWithNullableInt32;
        public static MethodInfo StandardDeviationSampleWithNullableInt64 => __standardDeviationSampleWithNullableInt64;
        public static MethodInfo StandardDeviationSampleWithNullableSingle => __standardDeviationSampleWithNullableSingle;
        public static MethodInfo StandardDeviationSampleWithSingle => __standardDeviationSampleWithSingle;
        public static MethodInfo SumWithDecimal => __sumWithDecimal;
        public static MethodInfo SumWithDouble => __sumWithDouble;
        public static MethodInfo SumWithInt32 => __sumWithInt32;
        public static MethodInfo SumWithInt64 => __sumWithInt64;
        public static MethodInfo SumWithNullableDecimal => __sumWithNullableDecimal;
        public static MethodInfo SumWithNullableDouble => __sumWithNullableDouble;
        public static MethodInfo SumWithNullableInt32 => __sumWithNullableInt32;
        public static MethodInfo SumWithNullableInt64 => __sumWithNullableInt64;
        public static MethodInfo SumWithNullableSingle => __sumWithNullableSingle;
        public static MethodInfo SumWithSingle => __sumWithSingle;
    }
}
