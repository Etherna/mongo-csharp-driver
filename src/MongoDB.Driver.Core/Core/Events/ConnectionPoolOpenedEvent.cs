/* Copyright 2013-present MongoDB Inc.
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
using Etherna.MongoDB.Driver.Core.Clusters;
using Etherna.MongoDB.Driver.Core.Configuration;
using Etherna.MongoDB.Driver.Core.Servers;

namespace Etherna.MongoDB.Driver.Core.Events
{
    /// <summary>
    /// Occurs after the pool is opened.
    /// </summary>
    public struct ConnectionPoolOpenedEvent
    {
        private readonly ConnectionPoolSettings _connectionPoolSettings;
        private readonly ServerId _serverId;
        private readonly DateTime _timestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionPoolOpenedEvent"/> struct.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="connectionPoolSettings">The connection pool settings.</param>
        public ConnectionPoolOpenedEvent(ServerId serverId, ConnectionPoolSettings connectionPoolSettings)
        {
            _serverId = serverId;
            _connectionPoolSettings = connectionPoolSettings;
            _timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the cluster identifier.
        /// </summary>
        public ClusterId ClusterId
        {
            get { return _serverId.ClusterId; }
        }

        /// <summary>
        /// Gets the connection pool settings.
        /// </summary>
        public ConnectionPoolSettings ConnectionPoolSettings
        {
            get { return _connectionPoolSettings; }
        }

        /// <summary>
        /// Gets the server identifier.
        /// </summary>
        public ServerId ServerId
        {
            get { return _serverId; }
        }

        /// <summary>
        /// Gets the timestamp.
        /// </summary>
        public DateTime Timestamp
        {
            get { return _timestamp; }
        }
    }
}
