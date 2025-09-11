/* Copyright 2010-present MongoDB Inc.
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

using Etherna.MongoDB.Driver.Core.Misc;

namespace Etherna.MongoDB.Driver.Core.WireProtocol.Messages.Encoders.JsonEncoders
{
    internal sealed class CommandRequestMessageJsonEncoder : IMessageEncoder
    {
        // private fields
        private readonly CommandMessageJsonEncoder _wrappedEncoder;

        // constructors
        public CommandRequestMessageJsonEncoder(CommandMessageJsonEncoder wrappedEncoder)
        {
            _wrappedEncoder = Ensure.IsNotNull(wrappedEncoder, nameof(wrappedEncoder));
        }

        // public methods
        public CommandRequestMessage ReadMessage()
        {
            var wrappedMessage = (CommandMessage)_wrappedEncoder.ReadMessage();
            return new CommandRequestMessage(wrappedMessage);
        }

        public void WriteMessage(
            CommandRequestMessage message,
            bool forceStaticSerializerRegistry)
        {
            var wrappedMessage = message.WrappedMessage;
            _wrappedEncoder.WriteMessage(wrappedMessage, forceStaticSerializerRegistry);
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
            WriteMessage((CommandRequestMessage)message, forceStaticSerializerRegistry);
        }
    }
}
