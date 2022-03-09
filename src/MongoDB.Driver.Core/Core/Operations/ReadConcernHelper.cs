﻿/* Copyright 2017-present MongoDB Inc.
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

using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Driver.Core.Bindings;
using Etherna.MongoDB.Driver.Core.Connections;
using Etherna.MongoDB.Driver.Core.Misc;

namespace Etherna.MongoDB.Driver.Core.Operations
{
    internal static class ReadConcernHelper
    {
        public static BsonDocument GetReadConcernForCommand(ICoreSession session, ConnectionDescription connectionDescription, ReadConcern readConcern)
        {
            return session.IsInTransaction ? null : ToBsonDocument(session, connectionDescription, readConcern);
        }

        public static BsonDocument GetReadConcernForFirstCommandInTransaction(ICoreSession session, ConnectionDescription connectionDescription)
        {
            var readConcern = session.CurrentTransaction.TransactionOptions.ReadConcern;
            return ToBsonDocument(session, connectionDescription, readConcern);
        }

        public static BsonDocument GetReadConcernForSnapshotSesssion(ICoreSession session, ConnectionDescription connectionDescription)
        {
            if (AreSessionsSupported(connectionDescription) && session.IsSnapshot)
            {
                Feature.SnapshotReads.ThrowIfNotSupported(connectionDescription.MaxWireVersion);

                var readConcernDocument = ReadConcern.Snapshot.ToBsonDocument();
                if (session.SnapshotTime != null)
                {
                    readConcernDocument.Add("atClusterTime", session.SnapshotTime);
                }

                return readConcernDocument;
            }

            return null;
        }

        // private static methods
        private static BsonDocument ToBsonDocument(ICoreSession session, ConnectionDescription connectionDescription, ReadConcern readConcern)
        {
            // causal consistency
            var shouldSendAfterClusterTime = AreSessionsSupported(connectionDescription) && session.IsCausallyConsistent && session.OperationTime != null;
            var shouldSendReadConcern = !readConcern.IsServerDefault || shouldSendAfterClusterTime;

            if (shouldSendReadConcern)
            {
                var readConcernDocument = readConcern.ToBsonDocument();
                if (shouldSendAfterClusterTime)
                {
                    readConcernDocument.Add("afterClusterTime", session.OperationTime);
                }
                return readConcernDocument;
            }

            return null;
        }

        private static bool AreSessionsSupported(ConnectionDescription connectionDescription) =>
            connectionDescription?.HelloResult.LogicalSessionTimeout != null || connectionDescription?.ServiceId != null;
    }
}
