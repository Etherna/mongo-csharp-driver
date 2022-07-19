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

namespace Etherna.MongoDB.Driver.Encryption
{
    /// <summary>
    /// Rewrap many data keys result.
    /// </summary>
    public sealed class RewrapManyDataKeyResult
    {
        /// <summary>
        /// Create RewrapManyDataKeyResult.
        /// </summary>
        public RewrapManyDataKeyResult() { }

        /// <summary>
        /// Create RewrapManyDataKeyResult.
        /// </summary>
        /// <param name="bulkWriteResult">The bulkWriteResult.</param>
        public RewrapManyDataKeyResult(BulkWriteResult bulkWriteResult) => BulkWriteResult = bulkWriteResult;

        /// <summary>
        /// Bulk write result.
        /// </summary>
        public BulkWriteResult BulkWriteResult { get; }
    }
}
