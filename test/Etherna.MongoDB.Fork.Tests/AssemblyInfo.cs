/* Copyright 2020-present Etherna SA
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Xunit;

// The whole fork feature set under test is built on process-global static state:
// BsonSerializer's static serializer registry, its serialization-context accessor,
// the discriminator convention registry, the class map registry and BsonDefaults'
// dynamic serializers. None of that can be isolated per test, so the assembly runs
// strictly serially.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
