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

using System.Net;
using Etherna.MongoDB.Driver.Core.Configuration;
using Etherna.MongoDB.Driver.Core.Connections;
using Etherna.MongoDB.Driver.Core.Events;
using Etherna.MongoDB.Driver.Core.Misc;
using Etherna.MongoDB.Driver.Core.Servers;

namespace Etherna.MongoDB.Driver.Core.ConnectionPools
{
    internal sealed class ExclusiveConnectionPoolFactory : IConnectionPoolFactory
    {
        // fields
        private readonly IConnectionFactory _connectionFactory;
        private readonly IEventSubscriber _eventSubscriber;
        private readonly ConnectionPoolSettings _settings;

        public ExclusiveConnectionPoolFactory(ConnectionPoolSettings settings, IConnectionFactory connectionFactory, IEventSubscriber eventSubscriber)
        {
            _settings = Ensure.IsNotNull(settings, nameof(settings));
            _connectionFactory = Ensure.IsNotNull(connectionFactory, nameof(connectionFactory));
            _eventSubscriber = Ensure.IsNotNull(eventSubscriber, nameof(eventSubscriber));
        }

        public IConnectionPool CreateConnectionPool(ServerId serverId, EndPoint endPoint, IConnectionExceptionHandler connectionExceptionHandler)
        {
            Ensure.IsNotNull(serverId, nameof(serverId));
            Ensure.IsNotNull(endPoint, nameof(endPoint));

            return new ExclusiveConnectionPool(serverId, endPoint, _settings, _connectionFactory, _eventSubscriber, connectionExceptionHandler);
        }
    }
}
