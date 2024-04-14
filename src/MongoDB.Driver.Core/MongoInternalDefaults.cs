/* Copyright 2021-present MongoDB Inc.
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

namespace Etherna.MongoDB.Driver
{
    internal static class MongoInternalDefaults
    {
        public static class Logging
        {
            public const int MaxDocumentSize = 1000;
        }

        public static class ConnectionPool
        {
            public const int MaxConnecting = 2;
        }

        public static class MongoClientSettings
        {
            public const string SrvServiceName = "mongodb";
        }
    }
}
