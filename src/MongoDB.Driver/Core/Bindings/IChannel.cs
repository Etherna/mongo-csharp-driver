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
using System.Collections.Generic;
using System.Threading.Tasks;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.IO;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Driver.Core.Connections;
using Etherna.MongoDB.Driver.Core.WireProtocol;
using Etherna.MongoDB.Driver.Core.WireProtocol.Messages;
using Etherna.MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace Etherna.MongoDB.Driver.Core.Bindings
{
    internal interface IChannel : IDisposable
    {
        IConnectionHandle Connection { get; }
        ConnectionDescription ConnectionDescription { get; }

        TResult Command<TResult>(
            OperationContext operationContext,
            ICoreSession session,
            ReadPreference readPreference,
            DatabaseNamespace databaseNamespace,
            BsonDocument command,
            IEnumerable<BatchableCommandMessageSection> commandPayloads,
            IElementNameValidator commandValidator,
            BsonDocument additionalOptions,
            Action<IMessageEncoderPostProcessor> postWriteAction,
            CommandResponseHandling responseHandling,
            IBsonSerializer<TResult> resultSerializer,
            MessageEncoderSettings messageEncoderSettings);

        Task<TResult> CommandAsync<TResult>(
            OperationContext operationContext,
            ICoreSession session,
            ReadPreference readPreference,
            DatabaseNamespace databaseNamespace,
            BsonDocument command,
            IEnumerable<BatchableCommandMessageSection> commandPayloads,
            IElementNameValidator commandValidator,
            BsonDocument additionalOptions,
            Action<IMessageEncoderPostProcessor> postWriteAction,
            CommandResponseHandling responseHandling,
            IBsonSerializer<TResult> resultSerializer,
            MessageEncoderSettings messageEncoderSettings);
    }

    internal interface IChannelHandle : IChannel
    {
        IChannelHandle Fork();
    }
}
