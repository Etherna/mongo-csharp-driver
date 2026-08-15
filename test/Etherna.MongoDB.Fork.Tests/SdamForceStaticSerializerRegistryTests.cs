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

using System.Net;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Core.Clusters;
using Etherna.MongoDB.Driver.Core.Servers;
using Etherna.MongoDB.Fork.Tests.Support;
using Xunit;

namespace Etherna.MongoDB.Fork.Tests
{
    /// <summary>
    /// Covers the fork's rule that driver-owned, infrastructure-level BSON never goes through an
    /// application's contextual registry. These descriptions are rendered by SDAM logging on driver
    /// threads that inherit the application's execution context, so a contextual lookup there would
    /// race with the registry's own initialization.
    ///
    /// The <c>ToString(bool)</c> overloads are fork additions; upstream has only <c>ToString()</c>.
    ///
    /// The wire-protocol encoders enforce the same rule, but they are <c>internal</c> and the fork's
    /// <c>InternalsVisibleTo</c> entries name upstream assemblies with MongoDB's public key, which
    /// unsigned fork assemblies cannot satisfy — so they are out of reach from here.
    /// </summary>
    public class SdamForceStaticSerializerRegistryTests
    {
        private static readonly EndPoint EndPoint = new DnsEndPoint("localhost", 27017);

        private static TopologyVersion CreateTopologyVersion() => new TopologyVersion(ObjectId.Empty, 5);

        private static ServerDescription CreateServerDescription() =>
            new ServerDescription(
                new ServerId(new ClusterId(1), EndPoint),
                EndPoint,
                topologyVersion: CreateTopologyVersion(),
                state: ServerState.Connected,
                type: ServerType.Standalone);

        private static ContextualSerializerRegistry RegistryThatOffsetsInt64() =>
            new ContextualSerializerRegistry().Override(new OffsetBsonInt64Serializer());

        [Fact]
        public void TopologyVersionToString_RendersThroughTheStaticRegistry_WhenForced()
        {
            var topologyVersion = CreateTopologyVersion();

            using (ContextualRegistryScope.Enter(RegistryThatOffsetsInt64()))
            {
                Assert.Contains("\"counter\" : 5", topologyVersion.ToString(forceStaticSerializerRegistry: true));

                // Control: the contextual registry really is installed and really would have changed
                // the output, so the assertion above is not vacuous.
                Assert.Contains("\"counter\" : 1005", topologyVersion.ToString());
            }
        }

        [Fact]
        public void ServerDescriptionToString_RendersItsTopologyVersion_ThroughTheStaticRegistry()
        {
            var serverDescription = CreateServerDescription();

            using (ContextualRegistryScope.Enter(RegistryThatOffsetsInt64()))
            {
                Assert.Contains("\"counter\" : 5", serverDescription.ToString(forceStaticSerializerRegistry: true));
                Assert.Contains("\"counter\" : 1005", serverDescription.ToString());
            }
        }

        [Fact]
        public void ClusterDescriptionToString_RendersItsServers_ThroughTheStaticRegistry()
        {
            var clusterDescription = new ClusterDescription(
                new ClusterId(1),
                directConnection: true,
                dnsMonitorException: null,
                ClusterType.Standalone,
                new[] { CreateServerDescription() });

            using (ContextualRegistryScope.Enter(RegistryThatOffsetsInt64()))
            {
                Assert.Contains("\"counter\" : 5", clusterDescription.ToString(forceStaticSerializerRegistry: true));
                Assert.Contains("\"counter\" : 1005", clusterDescription.ToString());
            }
        }

#pragma warning disable CS0618 // ReadPreferenceHedge is obsolete upstream; the fork's forcing still applies.
        [Fact]
        public void ReadPreferenceHedgeToString_AlwaysRendersThroughTheStaticRegistry()
        {
            var hedge = new ReadPreferenceHedge(isEnabled: true);
            var contextual = new ContextualSerializerRegistry().Override(new NegatingBsonBooleanSerializer());

            using (ContextualRegistryScope.Enter(contextual))
            {
                // ToString forces the static registry unconditionally — there is no opt-out.
                Assert.Contains("\"enabled\" : true", hedge.ToString());

                // Control: ToBsonDocument() is not forced, so the contextual registry does apply there.
                Assert.Contains("\"enabled\" : false", hedge.ToBsonDocument().ToJson());
            }
        }

        [Fact]
        public void ReadPreferenceToString_RendersItsHedge_ThroughTheStaticRegistry()
        {
            var readPreference = new ReadPreference(ReadPreferenceMode.Secondary, hedge: new ReadPreferenceHedge(isEnabled: true));
            var contextual = new ContextualSerializerRegistry().Override(new NegatingBsonBooleanSerializer());

            using (ContextualRegistryScope.Enter(contextual))
            {
                Assert.Contains("\"enabled\" : true", readPreference.ToString());
            }
        }
#pragma warning restore CS0618
    }
}
