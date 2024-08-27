﻿/* Copyright 2018-present MongoDB Inc.
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
using System.IO;
using Etherna.MongoDB.Driver.Core.Misc;

namespace Etherna.MongoDB.Driver.Core.WireProtocol.Messages.Encoders.BinaryEncoders
{
    /// <summary>
    /// Represents a binary encoder for a CommandRequestMessage.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.WireProtocol.Messages.Encoders.IMessageEncoder" />
    public class CommandRequestMessageBinaryEncoder : IMessageEncoder
    {
        // private fields
        private readonly CommandMessageBinaryEncoder _wrappedEncoder;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRequestMessageBinaryEncoder" /> class.
        /// </summary>
        /// <param name="wrappedEncoder">The wrapped encoder.</param>
        public CommandRequestMessageBinaryEncoder(CommandMessageBinaryEncoder wrappedEncoder)
        {
            _wrappedEncoder = Ensure.IsNotNull(wrappedEncoder, nameof(wrappedEncoder));
        }

        // public methods
        /// <summary>
        /// Reads the message.
        /// </summary>
        /// <returns>A message.</returns>
        public CommandRequestMessage ReadMessage()
        {
            var wrappedMessage = (CommandMessage)_wrappedEncoder.ReadMessage();
            return new CommandRequestMessage(wrappedMessage, null);
        }

        /// <summary>
        /// Writes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteMessage(CommandRequestMessage message)
        {
            var wrappedMessage = message.WrappedMessage;
            _wrappedEncoder.WriteMessage(wrappedMessage);
        }

        // explicit interface implementations
        MongoDBMessage IMessageEncoder.ReadMessage(
            bool forceStaticSerializerRegistry)
        {
            return ReadMessage();
        }

        void IMessageEncoder.WriteMessage(
            MongoDBMessage message,
            bool forceStaticSerializerRegistry)
        {
            WriteMessage((CommandRequestMessage)message);
        }
    }
}
