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
using System.Collections.Generic;
using System.Threading;
using Etherna.MongoDB.Bson.Serialization;

namespace Etherna.MongoDB.Fork.Tests.Support
{
    /// <summary>
    /// An <see cref="IBsonSerializerRegistry"/> that behaves exactly like the static registry
    /// except for a handful of types, for which it returns a caller-supplied serializer.
    /// </summary>
    /// <remarks>
    /// Delegating to the static registry (instead of building a standalone
    /// <see cref="BsonSerializerRegistry"/>, which starts with no serialization providers) keeps
    /// the contextual registry usable for arbitrary types while still being observably different
    /// from the static one. That difference is what every "which registry did this code path
    /// consult?" assertion in this suite is built on.
    /// </remarks>
    public sealed class ContextualSerializerRegistry : IBsonSerializerRegistry
    {
        private readonly Dictionary<Type, IBsonSerializer> _overrides = new Dictionary<Type, IBsonSerializer>();
        private readonly IBsonSerializerRegistry _staticRegistry =
            BsonSerializer.GetSerializerRegistry(forceStaticSerializerRegistry: true);

        public ContextualSerializerRegistry Override<T>(IBsonSerializer<T> serializer)
        {
            _overrides[typeof(T)] = serializer;
            return this;
        }

        public IBsonSerializer GetSerializer(Type type) =>
            _overrides.TryGetValue(type, out var serializer) ? serializer : _staticRegistry.GetSerializer(type);

        public IBsonSerializer<T> GetSerializer<T>() => (IBsonSerializer<T>)GetSerializer(typeof(T));
    }

    /// <summary>
    /// The <see cref="ISerializationContextAccessor"/> used by the whole suite. It is installed on
    /// <see cref="BsonSerializer"/> once, permanently, and resolves to whatever registry the
    /// current <see cref="ContextualRegistryScope"/> published — or to null (i.e. "fall back to the
    /// static registry") when no scope is active.
    /// </summary>
    /// <remarks>
    /// <see cref="BsonSerializer.SetSerializationContextAccessor"/> mutates process-global state and
    /// there is no way to scope it per test, so the accessor is installed once and the actual
    /// scoping happens in the <see cref="AsyncLocal{T}"/> below.
    /// </remarks>
    public sealed class AsyncLocalSerializationContextAccessor : ISerializationContextAccessor
    {
        internal static readonly AsyncLocal<IBsonSerializerRegistry> CurrentRegistry =
            new AsyncLocal<IBsonSerializerRegistry>();

        private static int _installed;
        private static int _invocationCount;

        /// <summary>Number of times the driver has asked the accessor for a registry.</summary>
        public static int InvocationCount => Volatile.Read(ref _invocationCount);

        public static void ResetInvocationCount() => Volatile.Write(ref _invocationCount, 0);

        public static void EnsureInstalled()
        {
            if (Interlocked.Exchange(ref _installed, 1) == 0)
                BsonSerializer.SetSerializationContextAccessor(new AsyncLocalSerializationContextAccessor());
        }

        public IBsonSerializerRegistry TryGetCurrentBsonSerializerRegistry()
        {
            Interlocked.Increment(ref _invocationCount);
            return CurrentRegistry.Value;
        }
    }

    /// <summary>
    /// Publishes a contextual registry for the duration of the scope, the way a MongODM-style
    /// DbContext would.
    /// </summary>
    public sealed class ContextualRegistryScope : IDisposable
    {
        private readonly IBsonSerializerRegistry _previous;

        private ContextualRegistryScope(IBsonSerializerRegistry registry)
        {
            _previous = AsyncLocalSerializationContextAccessor.CurrentRegistry.Value;
            AsyncLocalSerializationContextAccessor.CurrentRegistry.Value = registry;
        }

        public static ContextualRegistryScope Enter(IBsonSerializerRegistry registry)
        {
            AsyncLocalSerializationContextAccessor.EnsureInstalled();
            return new ContextualRegistryScope(registry);
        }

        public void Dispose() => AsyncLocalSerializationContextAccessor.CurrentRegistry.Value = _previous;
    }
}
