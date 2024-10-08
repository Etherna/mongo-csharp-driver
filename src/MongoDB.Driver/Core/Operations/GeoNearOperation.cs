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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    internal sealed class GeoNearOperation<TResult> : IReadOperation<TResult>
    {
        private Collation _collation;
        private readonly CollectionNamespace _collectionNamespace;
        private double? _distanceMultiplier;
        private BsonDocument _filter;
        private bool? _includeLocs;
        private int? _limit;
        private double? _maxDistance;
        private TimeSpan? _maxTime;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly BsonValue _near;
        private ReadConcern _readConcern = ReadConcern.Default;
        private readonly IBsonSerializer<TResult> _resultSerializer;
        private bool? _spherical;
        private bool? _uniqueDocs;

        public GeoNearOperation(CollectionNamespace collectionNamespace, BsonValue near, IBsonSerializer<TResult> resultSerializer, MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _near = Ensure.IsNotNull(near, nameof(near));
            _resultSerializer = Ensure.IsNotNull(resultSerializer, nameof(resultSerializer));
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
        }

        public Collation Collation
        {
            get { return _collation; }
            set { _collation = value; }
        }

        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        public double? DistanceMultiplier
        {
            get { return _distanceMultiplier; }
            set { _distanceMultiplier = value; }
        }

        public BsonDocument Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        public bool? IncludeLocs
        {
            get { return _includeLocs; }
            set { _includeLocs = value; }
        }

        public int? Limit
        {
            get { return _limit; }
            set { _limit = value; }
        }

        public double? MaxDistance
        {
            get { return _maxDistance; }
            set { _maxDistance = value; }
        }

        public TimeSpan? MaxTime
        {
            get { return _maxTime; }
            set { _maxTime = Ensure.IsNullOrInfiniteOrGreaterThanOrEqualToZero(value, nameof(value)); }
        }

        public MessageEncoderSettings MessageEncoderSettings
        {
            get { return _messageEncoderSettings; }
        }

        public BsonValue Near
        {
            get { return _near; }
        }

        public ReadConcern ReadConcern
        {
            get { return _readConcern; }
            set { _readConcern = Ensure.IsNotNull(value, nameof(value)); }
        }

        public IBsonSerializer<TResult> ResultSerializer
        {
            get { return _resultSerializer; }
        }

        public bool? Spherical
        {
            get { return _spherical; }
            set { _spherical = value; }
        }

        public bool? UniqueDocs
        {
            get { return _uniqueDocs; }
            set { _uniqueDocs = value; }
        }

        public BsonDocument CreateCommand(ConnectionDescription connectionDescription, ICoreSession session)
        {
            var readConcern = ReadConcernHelper.GetReadConcernForCommand(session, connectionDescription, _readConcern);
            return new BsonDocument
            {
                { "geoNear", _collectionNamespace.CollectionName },
                { "near", _near },
                { "limit", () => _limit.Value, _limit.HasValue },
                { "maxDistance", () => _maxDistance.Value, _maxDistance.HasValue },
                { "query", _filter, _filter != null },
                { "spherical", () => _spherical.Value, _spherical.HasValue },
                { "distanceMultiplier", () => _distanceMultiplier.Value, _distanceMultiplier.HasValue },
                { "includeLocs", () => _includeLocs.Value, _includeLocs.HasValue },
                { "uniqueDocs", () => _uniqueDocs.Value, _uniqueDocs.HasValue },
                { "maxTimeMS", () => MaxTimeHelper.ToMaxTimeMS(_maxTime.Value), _maxTime.HasValue },
                { "collation", () => _collation.ToBsonDocument(), _collation != null },
                { "readConcern", readConcern, readConcern != null }
            };
        }

        public TResult Execute(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));
            using (var channelSource = binding.GetReadChannelSource(cancellationToken))
            using (var channel = channelSource.GetChannel(cancellationToken))
            using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel, binding.ReadPreference, binding.Session.Fork()))
            {
                var operation = CreateOperation(channel, channelBinding);
                return operation.Execute(channelBinding, cancellationToken);
            }
        }

        public async Task<TResult> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));
            using (var channelSource = await binding.GetReadChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel, binding.ReadPreference, binding.Session.Fork()))
            {
                var operation = CreateOperation(channel, channelBinding);
                return await operation.ExecuteAsync(channelBinding, cancellationToken).ConfigureAwait(false);
            }
        }

        private ReadCommandOperation<TResult> CreateOperation(IChannel channel, IBinding binding)
        {
            var command = CreateCommand(channel.ConnectionDescription, binding.Session);
            return new ReadCommandOperation<TResult>(
                _collectionNamespace.DatabaseNamespace,
                command,
                _resultSerializer,
                _messageEncoderSettings)
            {
                RetryRequested = false
            };
        }
    }
}
