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

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.IO;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Xunit;

namespace Etherna.MongoDB.Fork.Tests
{
    /// <summary>
    /// Covers the fork's behavioral change in <see cref="BsonClassMapSerializer{TClass}"/>:
    /// it honors <see cref="BsonSerializationArgs.SerializeAsNominalType"/>, so a value whose
    /// runtime type differs from <c>TClass</c> is still serialized through <c>TClass</c>'s class map.
    ///
    /// Upstream v3.10.0 ignores the flag in class maps and unconditionally re-dispatches to the
    /// runtime type's own serializer, which is what makes
    /// <see cref="SerializeAsNominalTypeSerializer{TActualType, TNominalType}"/> unusable with
    /// class-mapped types (CSHARP-3153). MongODM depends on the fork behavior to serialize its
    /// runtime-generated proxy models through the base class map (MODM-189).
    ///
    /// Each test uses its own throwaway types: class maps register into a process-global,
    /// append-only dictionary that offers no way to unregister or re-map a type.
    /// </summary>
    public class SerializeAsNominalTypeTests
    {
        public class HandBase { public int A { get; set; } }
        public class HandDerived : HandBase { public int B { get; set; } }

        public class ProxyBase { public int A { get; set; } }

        public class ControlBase { public int A { get; set; } }
        public class ControlDerived : ControlBase { public int B { get; set; } }

        private static BsonDocument SerializeThroughClassMapOf<TBase>(TBase value, bool serializeAsNominalType)
        {
            var classMap = BsonClassMap.LookupClassMap(typeof(TBase));
            var serializer = new BsonClassMapSerializer<TBase>(classMap);

            var document = new BsonDocument();
            using (var writer = new BsonDocumentWriter(document))
            {
                var context = BsonSerializationContext.CreateRoot(writer);
                var args = new BsonSerializationArgs
                {
                    NominalType = typeof(TBase),
                    SerializeAsNominalType = serializeAsNominalType
                };

                serializer.Serialize(context, args, value);
            }

            return document;
        }

        [Fact]
        public void SerializeAsNominalType_SerializesADerivedInstanceThroughTheBaseClassMap()
        {
            var document = SerializeThroughClassMapOf<HandBase>(new HandDerived { A = 1, B = 2 }, serializeAsNominalType: true);

            // Only the base class map's members are written, and no discriminator is emitted
            // because the nominal type is exactly the class map's type.
            Assert.Equal(new[] { "A" }, document.Names.ToArray());
            Assert.Equal(1, document["A"].AsInt32);
            Assert.False(document.Contains("_t"));
        }

        [Fact]
        public void WithoutTheFlag_TheDerivedInstanceIsStillDispatchedToItsOwnSerializer()
        {
            // The control case, which upstream and the fork agree on: without the flag the runtime
            // type wins, so the derived member and a discriminator both show up.
            var document = SerializeThroughClassMapOf<ControlBase>(new ControlDerived { A = 1, B = 2 }, serializeAsNominalType: false);

            Assert.True(document.Contains("_t"));
            Assert.True(document.Contains("B"));
            Assert.Equal(2, document["B"].AsInt32);
        }

        [Fact]
        public void SerializeAsNominalType_WorksForARuntimeGeneratedProxySubclass()
        {
            // The MongODM proxy-model shape: a subclass that does not exist at compile time and has
            // no class map of its own. Upstream would look up a serializer for the proxy type and
            // emit its (proxy) discriminator; the fork routes it through ProxyBase's class map.
            var proxyType = EmitProxySubclassOf(typeof(ProxyBase));
            var proxy = (ProxyBase)Activator.CreateInstance(proxyType);
            proxy.A = 42;

            var document = SerializeThroughClassMapOf<ProxyBase>(proxy, serializeAsNominalType: true);

            Assert.Equal(new[] { "A" }, document.Names.ToArray());
            Assert.Equal(42, document["A"].AsInt32);
            Assert.False(document.Contains("_t"));
        }

        private static Type EmitProxySubclassOf(Type baseType)
        {
            var assemblyName = new AssemblyName("Etherna.MongoDB.Fork.Tests.Proxies");
            var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var module = assembly.DefineDynamicModule(assemblyName.Name);

            return module
                .DefineType(baseType.Name + "Proxy", TypeAttributes.Public | TypeAttributes.Class, baseType)
                .CreateTypeInfo()
                .AsType();
        }
    }
}
