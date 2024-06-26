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
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Driver.Core.Misc;

namespace Etherna.MongoDB.Driver.Linq.Linq2Implementation
{
    internal sealed class MongoQueryableImpl<TInput, TOutput> : IOrderedMongoQueryable<TOutput>
    {
        private readonly IMongoQueryProviderInternal _queryProvider;
        private readonly Expression _expression;

        public MongoQueryableImpl(IMongoQueryProviderInternal queryProvider)
        {
            _queryProvider = Ensure.IsNotNull(queryProvider, nameof(queryProvider));
            _expression = Expression.Constant(this, typeof(IMongoQueryable<TOutput>));
        }

        public MongoQueryableImpl(IMongoQueryProviderInternal queryProvider, Expression expression)
        {
            _queryProvider = Ensure.IsNotNull(queryProvider, nameof(queryProvider));
            _expression = Ensure.IsNotNull(expression, nameof(expression));
        }

        public Type ElementType
        {
            get { return typeof(TOutput); }
        }

        public Expression Expression
        {
            get { return _expression; }
        }

        public BsonDocument[] LoggedStages => _queryProvider.LoggedStages;

        public IMongoQueryProvider Provider
        {
            get { return _queryProvider; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return _queryProvider; }
        }

        public IEnumerator<TOutput> GetEnumerator()
        {
            var results = (IEnumerable<TOutput>)_queryProvider.Execute(_expression);
            return results.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public QueryableExecutionModel GetExecutionModel()
        {
            return _queryProvider.GetExecutionModel(_expression);
        }

        public IAsyncCursor<TOutput> ToCursor(CancellationToken cancellationToken)
        {
            var model = _queryProvider.GetExecutionModel(_expression);
            var mongoQueryProvider = (MongoQueryProviderImpl<TInput>)_queryProvider;
            return (IAsyncCursor<TOutput>)mongoQueryProvider.ExecuteModel(model);
        }

        public Task<IAsyncCursor<TOutput>> ToCursorAsync(CancellationToken cancellationToken)
        {
            return _queryProvider.ExecuteAsync<IAsyncCursor<TOutput>>(_expression, cancellationToken);
        }

        public override string ToString()
        {
            var pipeline = GetExecutionModel();
            return pipeline.ToString();
        }
    }
}
