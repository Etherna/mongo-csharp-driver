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
using Etherna.MongoDB.Driver.Core.Servers;

namespace Etherna.MongoDB.Driver.Core.Events
{
    /// <summary>
    /// Occurs after a server has been removed from the cluster.
    /// </summary>
    public struct ClusterRemovedServerEvent : IEvent
    {
        private readonly TimeSpan _duration;
        private readonly string _reason;
        private readonly ServerId _serverId;
        private readonly DateTime _timestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterRemovedServerEvent"/> struct.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="reason">The reason.</param>
        /// <param name="duration">The duration of time it took to remove the server.</param>
        public ClusterRemovedServerEvent(ServerId serverId, string reason, TimeSpan duration)
        {
            _serverId = serverId;
            _reason = reason;
            _duration = duration;
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
        /// Gets the duration of time it took to remove the server.
        /// </summary>
        public TimeSpan Duration
        {
            get { return _duration; }
        }

        /// <summary>
        /// Gets the reason the server was removed.
        /// </summary>
        public string Reason
        {
            get { return _reason; }
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

        // explicit interface implementations
        EventType IEvent.Type => EventType.ClusterRemovedServer;
    }
}
