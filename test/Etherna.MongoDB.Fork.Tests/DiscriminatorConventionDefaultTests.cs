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

using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Bson.Serialization.Conventions;
using Etherna.MongoDB.Fork.Tests.Support;
using Xunit;

namespace Etherna.MongoDB.Fork.Tests
{
    /// <summary>
    /// Covers the fork's rewritten <see cref="BsonSerializer.LookupDiscriminatorConvention"/>.
    ///
    /// Upstream v3.10.0 decides the convention for a class whose base is <see cref="object"/> by
    /// testing <c>parentType == typeof(object)</c> <em>before</em> consulting the registry, and so
    /// always yields <see cref="StandardDiscriminatorConvention.Scalar"/>. The fork checks the
    /// registry first: with nothing registered for <see cref="object"/> it yields
    /// <see cref="StandardDiscriminatorConvention.Hierarchical"/>, and once something *is*
    /// registered for <see cref="object"/> it inherits that instead of ignoring it.
    ///
    /// Note that a plain base/derived pair emits the same scalar <c>_t</c> under either convention —
    /// <see cref="HierarchicalDiscriminatorConvention"/> only expands to an array inside a declared
    /// root-class hierarchy, and there upstream's class map already registers the hierarchical
    /// convention itself (BsonClassMap auto-registration), so that output is *not* a fork
    /// difference. What the fork actually changes is which convention a class resolves to.
    /// </summary>
    public class DiscriminatorConventionDefaultTests
    {
        public class InheritedRoot { public int X { get; set; } }
        public class InheritedLeaf : InheritedRoot { public int Y { get; set; } }

        [Fact]
        public void AClassDerivedFromObject_DefaultsToTheHierarchicalConvention()
        {
            // Observed at module load; see PristineDiscriminatorState for why it cannot be
            // re-observed here. Upstream would have resolved StandardDiscriminatorConvention.Scalar.
            Assert.Same(
                StandardDiscriminatorConvention.Hierarchical,
                PristineDiscriminatorState.ForClassDerivedFromObject);
        }

        [Fact]
        public void ADerivedClass_InheritsTheHierarchicalDefaultFromItsBase()
        {
            Assert.Same(
                StandardDiscriminatorConvention.Hierarchical,
                PristineDiscriminatorState.ForDerivedClass);
        }

        [Fact]
        public void AnObjectConventionIsInherited_ByClassesDerivedFromObject()
        {
            // Idempotent in both drivers: object resolves to ObjectDiscriminatorConvention. Doing it
            // explicitly makes this test independent of whatever already ran in the process — but it
            // also permanently latches object's convention, which is why the pristine-state
            // observations above are captured at module load rather than here.
            var objectConvention = BsonSerializer.LookupDiscriminatorConvention(typeof(object));
            Assert.IsType<ObjectDiscriminatorConvention>(objectConvention);

            // The fork inherits it. Upstream ignores object's registration for classes and would
            // still return StandardDiscriminatorConvention.Scalar here.
            var convention = BsonSerializer.LookupDiscriminatorConvention(typeof(InheritedRoot));

            Assert.Same(objectConvention, convention);
            Assert.IsNotType<ScalarDiscriminatorConvention>(convention);
        }

        [Fact]
        public void TheInheritedObjectConvention_EmitsAnAssemblyQualifiedDiscriminator()
        {
            // Consequence on the wire of the previous test: ObjectDiscriminatorConvention discriminates
            // by assembly-qualified type name, where upstream's scalar convention emits the bare
            // class name. Documents written by the fork are therefore not interchangeable.
            BsonSerializer.LookupDiscriminatorConvention(typeof(object));

            var document = new InheritedLeaf { X = 1, Y = 2 }.ToBsonDocument<InheritedRoot>();

            Assert.Equal(BsonType.String, document["_t"].BsonType);
            Assert.Contains(typeof(InheritedLeaf).FullName, document["_t"].AsString);
            Assert.Contains(", ", document["_t"].AsString); // assembly-qualified, not the bare name
        }
    }
}
