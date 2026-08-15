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

using System.Runtime.CompilerServices;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Bson.Serialization.Conventions;

namespace Etherna.MongoDB.Fork.Tests.Support
{
    /// <summary>
    /// Captures the fork's default discriminator conventions before any test can perturb them.
    /// </summary>
    /// <remarks>
    /// The fork's default is conditional: <c>LookupDiscriminatorConvention</c> only falls back to
    /// <see cref="StandardDiscriminatorConvention.Hierarchical"/> while <c>typeof(object)</c> has no
    /// registered convention, and that latch is one-way — constructing an <c>ObjectSerializer</c>,
    /// or looking up any interface, registers one permanently and there is no API to undo it.
    /// A lookup is itself a mutation (it registers the resolved convention for the type), so these
    /// observations cannot be repeated later either.
    ///
    /// Running the lookups from a module initializer makes the observation deterministic regardless
    /// of the order xUnit happens to run collections in, which matters because
    /// <see cref="DiscriminatorConventionDefaultTests.AnObjectConventionIsInherited_ByClassesDerivedFromObject"/>
    /// deliberately trips that latch.
    /// </remarks>
    public static class PristineDiscriminatorState
    {
        public class RootProbe { public int X { get; set; } }
        public class LeafProbe : RootProbe { public int Y { get; set; } }

        /// <summary>Convention resolved for a class whose base is <see cref="object"/>.</summary>
        public static IDiscriminatorConvention ForClassDerivedFromObject { get; private set; }

        /// <summary>Convention resolved for a class one level further down.</summary>
        public static IDiscriminatorConvention ForDerivedClass { get; private set; }

        [ModuleInitializer]
        internal static void Capture()
        {
            ForClassDerivedFromObject = BsonSerializer.LookupDiscriminatorConvention(typeof(RootProbe));
            ForDerivedClass = BsonSerializer.LookupDiscriminatorConvention(typeof(LeafProbe));
        }
    }
}
