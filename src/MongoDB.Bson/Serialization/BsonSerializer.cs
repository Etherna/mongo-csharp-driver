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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

// don't add using statement for MongoDB.Bson.Serialization.Serializers to minimize dependencies on DefaultSerializer
using Etherna.MongoDB.Bson.IO;
using Etherna.MongoDB.Bson.Serialization.Attributes;
using Etherna.MongoDB.Bson.Serialization.Conventions;
using Etherna.MongoDB.Bson.Serialization.IdGenerators;

namespace Etherna.MongoDB.Bson.Serialization
{
    /// <summary>
    /// A static class that represents the BSON serialization functionality.
    /// </summary>
    public static class BsonSerializer
    {
        // private static fields
        private static ReaderWriterLockSlim __configLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private static Dictionary<Type, IIdGenerator> __idGenerators = new Dictionary<Type, IIdGenerator>();
        private static Dictionary<Type, IDiscriminatorConvention> __discriminatorConventions = new Dictionary<Type, IDiscriminatorConvention>();
        private static Dictionary<BsonValue, HashSet<Type>> __discriminators = new Dictionary<BsonValue, HashSet<Type>>();
        private static HashSet<Type> __discriminatedTypes = new HashSet<Type>();
        private static ISerializationContextAccessor __serializationContextAccessor;
        private static BsonSerializerRegistry __serializerRegistry;
        private static TypeMappingSerializationProvider __typeMappingSerializationProvider;
        // ConcurrentDictionary<Type, object> is being used as a concurrent set of Type. The values will always be null.
        private static ConcurrentDictionary<Type, object> __typesWithRegisteredKnownTypes = new ConcurrentDictionary<Type, object>();

        private static bool __useNullIdChecker = false;
        private static bool __useZeroIdChecker = false;

        // static constructor
        static BsonSerializer()
        {
            CreateStaticSerializerRegistry();
            RegisterIdGenerators();
        }

        // public static properties
        /// <summary>
        /// Gets or sets whether to use the NullIdChecker on reference Id types that don't have an IdGenerator registered.
        /// </summary>
        public static bool UseNullIdChecker
        {
            get { return __useNullIdChecker; }
            set { __useNullIdChecker = value; }
        }

        /// <summary>
        /// Gets or sets whether to use the ZeroIdChecker on value Id types that don't have an IdGenerator registered.
        /// </summary>
        public static bool UseZeroIdChecker
        {
            get { return __useZeroIdChecker; }
            set { __useZeroIdChecker = value; }
        }

        // internal static properties
        internal static ReaderWriterLockSlim ConfigLock
        {
            get { return __configLock; }
        }

        // public static methods
        /// <summary>
        /// Deserializes an object from a BsonDocument.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="document">The BsonDocument.</param>
        /// <param name="configurator">The configurator.</param>
        /// <returns>A deserialized value.</returns>
        public static TNominalType Deserialize<TNominalType>(BsonDocument document, Action<BsonDeserializationContext.Builder> configurator = null)
        {
            using (var bsonReader = new BsonDocumentReader(document))
            {
                return Deserialize<TNominalType>(bsonReader, configurator);
            }
        }

        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="configurator">The configurator.</param>
        /// <returns>A deserialized value.</returns>
        public static TNominalType Deserialize<TNominalType>(IBsonReader bsonReader, Action<BsonDeserializationContext.Builder> configurator = null)
        {
            var serializer = LookupSerializer<TNominalType>();
            var context = BsonDeserializationContext.CreateRoot(bsonReader, configurator);
            return serializer.Deserialize(context);
        }

        /// <summary>
        /// Deserializes an object from a BSON byte array.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="bytes">The BSON byte array.</param>
        /// <param name="configurator">The configurator.</param>
        /// <returns>A deserialized value.</returns>
        public static TNominalType Deserialize<TNominalType>(byte[] bytes, Action<BsonDeserializationContext.Builder> configurator = null)
        {
            using (var buffer = new ByteArrayBuffer(bytes, isReadOnly: true))
            using (var stream = new ByteBufferStream(buffer))
            {
                return Deserialize<TNominalType>(stream, configurator);
            }
        }

        /// <summary>
        /// Deserializes an object from a BSON Stream.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="stream">The BSON Stream.</param>
        /// <param name="configurator">The configurator.</param>
        /// <returns>A deserialized value.</returns>
        public static TNominalType Deserialize<TNominalType>(Stream stream, Action<BsonDeserializationContext.Builder> configurator = null)
        {
            using (var bsonReader = new BsonBinaryReader(stream))
            {
                return Deserialize<TNominalType>(bsonReader, configurator);
            }
        }

        /// <summary>
        /// Deserializes an object from a JSON string.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="json">The JSON string.</param>
        /// <param name="configurator">The configurator.</param>
        /// <returns>A deserialized value.</returns>
        public static TNominalType Deserialize<TNominalType>(string json, Action<BsonDeserializationContext.Builder> configurator = null)
        {
            using (var bsonReader = new JsonReader(json))
            {
                return Deserialize<TNominalType>(bsonReader, configurator);
            }
        }

        /// <summary>
        /// Deserializes an object from a JSON TextReader.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="textReader">The JSON TextReader.</param>
        /// <param name="configurator">The configurator.</param>
        /// <returns>A deserialized value.</returns>
        public static TNominalType Deserialize<TNominalType>(TextReader textReader, Action<BsonDeserializationContext.Builder> configurator = null)
        {
            using (var bsonReader = new JsonReader(textReader))
            {
                return Deserialize<TNominalType>(bsonReader, configurator);
            }
        }

        /// <summary>
        /// Deserializes an object from a BsonDocument.
        /// </summary>
        /// <param name="document">The BsonDocument.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="configurator">The configurator.</param>
        /// <returns>A deserialized value.</returns>
        public static object Deserialize(BsonDocument document, Type nominalType, Action<BsonDeserializationContext.Builder> configurator = null)
        {
            using (var bsonReader = new BsonDocumentReader(document))
            {
                return Deserialize(bsonReader, nominalType, configurator);
            }
        }

        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="configurator">The configurator.</param>
        /// <returns>A deserialized value.</returns>
        public static object Deserialize(IBsonReader bsonReader, Type nominalType, Action<BsonDeserializationContext.Builder> configurator = null)
        {
            var serializer = LookupSerializer(nominalType);
            var context = BsonDeserializationContext.CreateRoot(bsonReader, configurator);
            return serializer.Deserialize(context);
        }

        /// <summary>
        /// Deserializes an object from a BSON byte array.
        /// </summary>
        /// <param name="bytes">The BSON byte array.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="configurator">The configurator.</param>
        /// <returns>A deserialized value.</returns>
        public static object Deserialize(byte[] bytes, Type nominalType, Action<BsonDeserializationContext.Builder> configurator = null)
        {
            using (var buffer = new ByteArrayBuffer(bytes, isReadOnly: true))
            using (var stream = new ByteBufferStream(buffer))
            {
                return Deserialize(stream, nominalType, configurator);
            }
        }

        /// <summary>
        /// Deserializes an object from a BSON Stream.
        /// </summary>
        /// <param name="stream">The BSON Stream.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="configurator">The configurator.</param>
        /// <returns>A deserialized value.</returns>
        public static object Deserialize(Stream stream, Type nominalType, Action<BsonDeserializationContext.Builder> configurator = null)
        {
            using (var bsonReader = new BsonBinaryReader(stream))
            {
                return Deserialize(bsonReader, nominalType, configurator);
            }
        }

        /// <summary>
        /// Deserializes an object from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="configurator">The configurator.</param>
        /// <returns>A deserialized value.</returns>
        public static object Deserialize(string json, Type nominalType, Action<BsonDeserializationContext.Builder> configurator = null)
        {
            using (var bsonReader = new JsonReader(json))
            {
                return Deserialize(bsonReader, nominalType, configurator);
            }
        }

        /// <summary>
        /// Deserializes an object from a JSON TextReader.
        /// </summary>
        /// <param name="textReader">The JSON TextReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="configurator">The configurator.</param>
        /// <returns>A deserialized value.</returns>
        public static object Deserialize(TextReader textReader, Type nominalType, Action<BsonDeserializationContext.Builder> configurator = null)
        {
            using (var bsonReader = new JsonReader(textReader))
            {
                return Deserialize(bsonReader, nominalType, configurator);
            }
        }

        internal static IDiscriminatorConvention GetOrRegisterDiscriminatorConvention(Type type, IDiscriminatorConvention discriminatorConvention)
        {
            __configLock.EnterReadLock();
            try
            {
                if (__discriminatorConventions.TryGetValue(type, out var registeredDiscriminatorConvention))
                {
                    return registeredDiscriminatorConvention;
                }
            }
            finally
            {
                __configLock.ExitReadLock();
            }

            __configLock.EnterWriteLock();
            try
            {
                if (__discriminatorConventions.TryGetValue(type, out var registeredDiscrimantorConvention))
                {
                    return registeredDiscrimantorConvention;
                }

                RegisterDiscriminatorConvention(type, discriminatorConvention);
                return discriminatorConvention;
            }
            finally
            {
                __configLock.ExitWriteLock();
            }
        }

        internal static bool IsDiscriminatorConventionRegisteredAtThisLevel(Type type)
        {
            __configLock.EnterReadLock();
            try
            {
                return __discriminatorConventions.ContainsKey(type);
            }
            finally
            {
                __configLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets the serializer registry.
        /// </summary>
        public static IBsonSerializerRegistry GetSerializerRegistry(
            bool forceStaticSerializerRegistry = false)
        {
            if (forceStaticSerializerRegistry)
                return __serializerRegistry;

            return __serializationContextAccessor?.TryGetCurrentBsonSerializerRegistry() ??
                __serializerRegistry;
        }

        /// <summary>
        /// Returns whether the given type has any discriminators registered for any of its subclasses.
        /// </summary>
        /// <param name="type">A Type.</param>
        /// <returns>True if the type is discriminated.</returns>
        public static bool IsTypeDiscriminated(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsInterface || __discriminatedTypes.Contains(type);
        }

        /// <summary>
        /// Looks up the actual type of an object to be deserialized.
        /// </summary>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="discriminator">The discriminator.</param>
        /// <returns>The actual type of the object.</returns>
        public static Type LookupActualType(Type nominalType, BsonValue discriminator)
        {
            if (discriminator == null)
            {
                return nominalType;
            }

            // note: EnsureKnownTypesAreRegistered handles its own locking so call from outside any lock
            EnsureKnownTypesAreRegistered(nominalType);

            __configLock.EnterReadLock();
            try
            {
                Type actualType = null;

                HashSet<Type> hashSet;
                var nominalTypeInfo = nominalType.GetTypeInfo();
                if (__discriminators.TryGetValue(discriminator, out hashSet))
                {
                    foreach (var type in hashSet)
                    {
                        if (nominalTypeInfo.IsAssignableFrom(type))
                        {
                            if (actualType == null)
                            {
                                actualType = type;
                            }
                            else
                            {
                                string message = string.Format("Ambiguous discriminator '{0}'.", discriminator);
                                throw new BsonSerializationException(message);
                            }
                        }
                    }

                    // no need for additional checks, we found the right type
                    if (actualType != null)
                    {
                        return actualType;
                    }
                }

                if (discriminator.IsString)
                {
                    actualType = TypeNameDiscriminator.GetActualType(discriminator.AsString); // see if it's a Type name
                }

                if (actualType == null)
                {
                    string message = string.Format("Unknown discriminator value '{0}'.", discriminator);
                    throw new BsonSerializationException(message);
                }

                if (!nominalTypeInfo.IsAssignableFrom(actualType))
                {
                    string message = string.Format(
                        "Actual type {0} is not assignable to expected type {1}.",
                        actualType.FullName, nominalType.FullName);
                    throw new BsonSerializationException(message);
                }

                return actualType;
            }
            finally
            {
                __configLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Looks up the discriminator convention for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A discriminator convention.</returns>
        public static IDiscriminatorConvention LookupDiscriminatorConvention(Type type)
        {
            __configLock.EnterReadLock();
            try
            {
                IDiscriminatorConvention convention;
                if (__discriminatorConventions.TryGetValue(type, out convention))
                {
                    return convention;
                }
            }
            finally
            {
                __configLock.ExitReadLock();
            }

            __configLock.EnterWriteLock();
            try
            {
                IDiscriminatorConvention convention;
                if (!__discriminatorConventions.TryGetValue(type, out convention))
                {
                    var typeInfo = type.GetTypeInfo();
                    if (type == typeof(object))
                    {
                        // if there is no convention registered for object register the default one
                        convention = new ObjectDiscriminatorConvention("_t");
                    }
                    else if (typeInfo.IsInterface)
                    {
                        // TODO: should convention for interfaces be inherited from parent interfaces?
                        convention = LookupDiscriminatorConvention(typeof(object));
                    }
                    else // type is not typeof(object), or interface
                    {
                        Type parentType = typeInfo.BaseType;

                        if (parentType == typeof(object) &&
                            !__discriminatorConventions.ContainsKey(typeof(object)))
                        {
                            // if parent type is object, and object isn't registered, default to standard hierarchical convention
                            convention = StandardDiscriminatorConvention.Hierarchical;
                        }
                        else
                        {
                            // inherit the discriminator convention from the closest parent that has one
                            convention = LookupDiscriminatorConvention(parentType);
                        }
                    }

                    // register the convention for current type
                    RegisterDiscriminatorConvention(type, convention);
                }

                return convention;
            }
            finally
            {
                __configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Looks up an IdGenerator.
        /// </summary>
        /// <param name="type">The Id type.</param>
        /// <returns>An IdGenerator for the Id type.</returns>
        public static IIdGenerator LookupIdGenerator(Type type)
        {
            __configLock.EnterReadLock();
            try
            {
                IIdGenerator idGenerator;
                if (__idGenerators.TryGetValue(type, out idGenerator))
                {
                    return idGenerator;
                }
            }
            finally
            {
                __configLock.ExitReadLock();
            }

            __configLock.EnterWriteLock();
            try
            {
                IIdGenerator idGenerator;
                if (!__idGenerators.TryGetValue(type, out idGenerator))
                {
                    var typeInfo = type.GetTypeInfo();
                    if (typeInfo.IsValueType && __useZeroIdChecker)
                    {
                        var iEquatableDefinition = typeof(IEquatable<>);
                        var iEquatableType = iEquatableDefinition.MakeGenericType(type);
                        if (iEquatableType.GetTypeInfo().IsAssignableFrom(type))
                        {
                            var zeroIdCheckerDefinition = typeof(ZeroIdChecker<>);
                            var zeroIdCheckerType = zeroIdCheckerDefinition.MakeGenericType(type);
                            idGenerator = (IIdGenerator)Activator.CreateInstance(zeroIdCheckerType);
                        }
                    }
                    else if (__useNullIdChecker)
                    {
                        idGenerator = NullIdChecker.Instance;
                    }
                    else
                    {
                        idGenerator = null;
                    }

                    __idGenerators[type] = idGenerator; // remember it even if it's null
                }

                return idGenerator;
            }
            finally
            {
                __configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Looks up a serializer for a Type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="forceStaticSerializerRegistry">Force to use static serializer registry</param>
        /// <returns>A serializer for type T.</returns>
        public static IBsonSerializer<T> LookupSerializer<T>(
            bool forceStaticSerializerRegistry = false)
        {
            return (IBsonSerializer<T>)LookupSerializer(typeof(T), forceStaticSerializerRegistry);
        }

        /// <summary>
        /// Looks up a serializer for a Type.
        /// </summary>
        /// <param name="type">The Type.</param>
        /// <param name="forceStaticSerializerRegistry">Force to use static serializer registry</param>
        /// <returns>A serializer for the Type.</returns>
        public static IBsonSerializer LookupSerializer(
            Type type,
            bool forceStaticSerializerRegistry = false)
        {
            return GetSerializerRegistry(forceStaticSerializerRegistry).GetSerializer(type);
        }

        /// <summary>
        /// Registers the discriminator for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="discriminator">The discriminator.</param>
        public static void RegisterDiscriminator(Type type, BsonValue discriminator)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsInterface)
            {
                var message = string.Format("Discriminators can only be registered for classes, not for interface {0}.", type.FullName);
                throw new BsonSerializationException(message);
            }

            __configLock.EnterWriteLock();
            try
            {
                HashSet<Type> hashSet;
                if (!__discriminators.TryGetValue(discriminator, out hashSet))
                {
                    hashSet = new HashSet<Type>();
                    __discriminators.Add(discriminator, hashSet);
                }

                if (!hashSet.Contains(type))
                {
                    hashSet.Add(type);

                    // mark all base types as discriminated (so we know that it's worth reading a discriminator)
                    for (var baseType = typeInfo.BaseType; baseType != null; baseType = baseType.GetTypeInfo().BaseType)
                    {
                        __discriminatedTypes.Add(baseType);
                    }
                }
            }
            finally
            {
                __configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Registers the discriminator convention for a type.
        /// </summary>
        /// <param name="type">Type type.</param>
        /// <param name="convention">The discriminator convention.</param>
        public static void RegisterDiscriminatorConvention(Type type, IDiscriminatorConvention convention)
        {
            __configLock.EnterWriteLock();
            try
            {
                if (!__discriminatorConventions.ContainsKey(type))
                {
                    __discriminatorConventions.Add(type, convention);
                }
                else
                {
                    var message = string.Format("There is already a discriminator convention registered for type {0}.", type.FullName);
                    throw new BsonSerializationException(message);
                }
            }
            finally
            {
                __configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Registers a generic serializer definition for a generic type.
        /// </summary>
        /// <param name="genericTypeDefinition">The generic type.</param>
        /// <param name="genericSerializerDefinition">The generic serializer definition.</param>
        public static void RegisterGenericSerializerDefinition(
            Type genericTypeDefinition,
            Type genericSerializerDefinition)
        {
            __typeMappingSerializationProvider.RegisterMapping(genericTypeDefinition, genericSerializerDefinition);
        }

        /// <summary>
        /// Registers an IdGenerator for an Id Type.
        /// </summary>
        /// <param name="type">The Id Type.</param>
        /// <param name="idGenerator">The IdGenerator for the Id Type.</param>
        public static void RegisterIdGenerator(Type type, IIdGenerator idGenerator)
        {
            __configLock.EnterWriteLock();
            try
            {
                __idGenerators[type] = idGenerator;
            }
            finally
            {
                __configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="value">The object.</param>
        /// <param name="configurator">The serialization context configurator.</param>
        /// <param name="args">The serialization args.</param>
        public static void Serialize<TNominalType>(
            IBsonWriter bsonWriter,
            TNominalType value,
            Action<BsonSerializationContext.Builder> configurator = null,
            BsonSerializationArgs args = default(BsonSerializationArgs))
        {
            args.SetOrValidateNominalType(typeof(TNominalType), "<TNominalType>");
            var serializer = LookupSerializer<TNominalType>(args.ForceStaticSerializerRegistry);
            var context = BsonSerializationContext.CreateRoot(bsonWriter, configurator);
            serializer.Serialize(context, args, value);
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="value">The object.</param>
        /// <param name="configurator">The serialization context configurator.</param>
        /// <param name="args">The serialization args.</param>
        public static void Serialize(
            IBsonWriter bsonWriter,
            Type nominalType,
            object value,
            Action<BsonSerializationContext.Builder> configurator = null,
            BsonSerializationArgs args = default(BsonSerializationArgs))
        {
            args.SetOrValidateNominalType(nominalType, "nominalType");
            var serializer = LookupSerializer(nominalType, args.ForceStaticSerializerRegistry);
            var context = BsonSerializationContext.CreateRoot(bsonWriter, configurator);
            serializer.Serialize(context, args, value);
        }

        /// <summary>
        /// Set a serialization context accessor
        /// </summary>
        /// <param name="serializationContextAccessor">The serialization context accessor</param>
        public static void SetSerializationContextAccessor(
            ISerializationContextAccessor serializationContextAccessor)
        {
            __configLock.EnterWriteLock();
            try
            {
                __serializationContextAccessor = serializationContextAccessor;
            }
            finally
            {
                __configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Tries to register a serializer for a type.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="type">The type.</param>
        /// <returns>True if the serializer was registered on this call, false if the same serializer was already registered on a previous call, throws an exception if a different serializer was already registered.</returns>
        public static bool TryRegisterSerializer(Type type, IBsonSerializer serializer)
        {
            return __serializerRegistry.TryRegisterSerializer(type, serializer);
        }

        /// <summary>
        /// Tries to register a serializer for a type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="serializer">The serializer.</param>
        /// <returns>True if the serializer was registered on this call, false if the same serializer was already registered on a previous call, throws an exception if a different serializer was already registered.</returns>
        public static bool TryRegisterSerializer<T>(IBsonSerializer<T> serializer)
        {
            return TryRegisterSerializer(typeof(T), serializer);
        }

        // internal static methods
        internal static void EnsureKnownTypesAreRegistered(Type nominalType)
        {
            if (__typesWithRegisteredKnownTypes.ContainsKey(nominalType))
            {
                return;
            }

            __configLock.EnterWriteLock();
            try
            {
                if (!__typesWithRegisteredKnownTypes.ContainsKey(nominalType))
                {
                    // only call LookupClassMap for classes with a BsonKnownTypesAttribute
                    var hasKnownTypesAttribute = nominalType.GetTypeInfo().GetCustomAttributes(typeof(BsonKnownTypesAttribute), inherit: false).Any();
                    if (hasKnownTypesAttribute)
                    {
                        // try and force a scan of the known types
                        LookupSerializer(nominalType);
                    }

                    // NOTE: The nominalType MUST be added to __typesWithRegisteredKnownTypes after all registration
                    //       work is done to ensure that other threads don't access a partially registered nominalType
                    //       when performing the initial check above outside the __config lock.
                    __typesWithRegisteredKnownTypes[nominalType] = null;
                }
            }
            finally
            {
                __configLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Try to register the discriminator convention for a type.
        /// </summary>
        /// <param name="type">Type type.</param>
        /// <param name="convention">The discriminator convention.</param>
        public static bool TryRegisterDiscriminatorConvention(Type type, IDiscriminatorConvention convention)
        {
            try
            {
                RegisterDiscriminatorConvention(type, convention);
                return true;
            }
            catch (BsonSerializationException)
            {
                return false;
            }
        }

        // private static methods
        private static void CreateStaticSerializerRegistry()
        {
            __serializerRegistry = new BsonSerializerRegistry();
            __typeMappingSerializationProvider = new TypeMappingSerializationProvider();

            // order matters. It's in reverse order of how they'll get consumed
            __serializerRegistry.RegisterSerializationProvider(new BsonClassMapSerializationProvider());
            __serializerRegistry.RegisterSerializationProvider(new DiscriminatedInterfaceSerializationProvider());
            __serializerRegistry.RegisterSerializationProvider(new CollectionsSerializationProvider());
            __serializerRegistry.RegisterSerializationProvider(new PrimitiveSerializationProvider());
            __serializerRegistry.RegisterSerializationProvider(new AttributedSerializationProvider());
            __serializerRegistry.RegisterSerializationProvider(__typeMappingSerializationProvider);
            __serializerRegistry.RegisterSerializationProvider(new BsonObjectModelSerializationProvider());
        }

        private static void RegisterIdGenerators()
        {
            RegisterIdGenerator(typeof(BsonObjectId), BsonObjectIdGenerator.Instance);
            RegisterIdGenerator(typeof(Guid), GuidGenerator.Instance);
            RegisterIdGenerator(typeof(ObjectId), ObjectIdGenerator.Instance);
        }
    }
}
