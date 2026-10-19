# AGENTS.md - Etherna.MongoDB

## Overview
This repository is the **Etherna fork** of the official [mongo-csharp-driver](https://github.com/mongodb/mongo-csharp-driver), maintained by Etherna SA and published on NuGet as the `Etherna.MongoDB.*` packages. All namespaces are renamed from `MongoDB.*` to `Etherna.MongoDB.*`, so the fork installs side-by-side with the official driver without conflicts.

The fork exists primarily to support **scoped, per-context BSON serialization**: the official driver resolves serializers through global static registries, which prevents hosting multiple isolated serialization contexts (e.g. MongODM-style DbContexts, one per tenant/domain) in the same process. See "Differences vs the official driver" below.

## Fork model
- Branch `etherna` is the fork's main line. Remote `origin` is `Etherna/mongo-csharp-driver`; remote `mongodb` is the official repository.
- Official release tags are merged into `etherna` periodically (3.2.1 → 3.3.0 → 3.4.0 → 3.5.0 → 3.6.0 → 3.8.1 → 3.10.0 → 3.11.0 → **3.12.0**, the current upstream baseline).
- Fork releases are tagged `eth-v<major>.<minor>.<patch>`. Versions are computed by GitVersion (`GitVersion.yml` + `GitVersion.MsBuild`); pushing an `eth-v*` tag triggers `.github/workflows/nuget-stable-deploy.yml`, which builds, packs, and pushes to NuGet. The push carries no stored key: it authenticates with the trusted publishing of nuget.org, where the job asks GitHub for an OIDC token (`permissions: id-token: write`) and `NuGet/login` exchanges it, right before the push, for an API key that lives one hour. nuget.org issues it by matching the token against a policy of the `etherna` organization owning the packages, registered on the repository owner, the repository and the workflow **file name** — renaming `nuget-stable-deploy.yml`, or moving the push into another workflow, stops the publish until the policy is updated on nuget.org — with the `NUGET_USER` organization secret naming the account that created it. GitHub runs the workflow file as it is at the tagged commit, so a workflow change ships only with a tag that includes it.

## Tech Stack
- .NET library projects producing NuGet packages
- Multi-targeted `netstandard2.1;net472;net6.0`
- C# LangVersion 12

## Project Structure
Only `src/` and `test/` are tracked. The upstream test suites, spec corpora, and CI are **not part of the fork** (see below).

| Directory | Project file | NuGet package |
|---|---|---|
| `src/MongoDB.Bson/` | `Etherna.MongoDB.Bson.csproj` | `Etherna.MongoDB.Bson` |
| `src/MongoDB.Driver/` | `Etherna.MongoDB.Driver.csproj` | `Etherna.MongoDB.Driver` |
| `src/MongoDB.Driver.Encryption/` | `Etherna.MongoDB.Driver.Encryption.csproj` | `Etherna.MongoDB.Driver.Encryption` |
| `src/MongoDB.Driver.Authentication.AWS/` | `Etherna.MongoDB.Driver.Authentication.AWS.csproj` | `Etherna.MongoDB.Driver.Authentication.AWS` |
| `test/Etherna.MongoDB.Fork.Tests/` | `Etherna.MongoDB.Fork.Tests.csproj` | *(not packable)* |

Note the singular `test/` — the plural `tests/`, along with `benchmarks/` and `specifications/`, may exist on disk as ignored leftovers of upstream merges: they are untracked, not referenced by the solution, and reference the old upstream project names. Do not edit or build them.

## Differences vs the official driver
Baseline for this section: upstream tag `v3.12.0`. Beyond the mechanical namespace rename (~1900 files), the semantic delta is small and deliberate.

### Packaging & naming
- Namespaces and project/package IDs renamed `MongoDB.*` → `Etherna.MongoDB.*`; `Authors` is `Etherna SA` and package descriptions state "Fork by Etherna of...".
- **Strong-name signing removed** (`SignAssembly` and the `.snk` dropped from `src/Directory.Build.props`): fork assemblies are not strong-named, unlike the official ones.
- Package icon removed; versioning switched from build-injected `$(Version)` to GitVersion.

### Contextual serializer registry (the core fork feature)
- New public interface `Etherna.MongoDB.Bson.Serialization.ISerializationContextAccessor` (`src/MongoDB.Bson/Serialization/ISerializationContextAccessor.cs`), installed via `BsonSerializer.SetSerializationContextAccessor(...)`. It lets the application resolve a *per-context* `IBsonSerializerRegistry` (e.g. from an async-local scope) instead of the process-wide static one.
- The static property `BsonSerializer.SerializerRegistry` is **replaced** by `BsonSerializer.GetSerializerRegistry(bool forceStaticSerializerRegistry = false)`: it asks the accessor first and falls back to the static registry. `BsonSerializer.LookupSerializer(...)` gains the same optional `forceStaticSerializerRegistry` parameter.
- A `ForceStaticSerializerRegistry` flag is threaded through the serialization pipeline so that specific code paths can bypass the contextual registry: `BsonSerializationArgs`/`BsonDeserializationArgs`, `BsonDeserializationContext.CreateRoot`/`.With`, `BsonSerializer.Deserialize` (every overload), `IBsonSerializerExtensions.Serialize`/`Deserialize`, `IBsonWriter.WriteEndDocument`, `BsonDocument.GetValue`/`TryGetValue`/`ToString`, `RawBsonDocument.Materialize`, and the (internal) wire-protocol message encoders (`IMessageEncoder.ReadMessage`/`WriteMessage` and the section formatters).
- **The wire layer always forces the static registry**: `BinaryConnection` reads and writes protocol messages with `forceStaticSerializerRegistry: true`, and `CommandUsingQueryMessageWireProtocol` materializes `$clusterTime`/`operationTime`/error documents the same way. Protocol-level BSON must never be affected by an application's contextual registry. SDAM logging does the same (`ClusterDescription`/`ServerDescription`/`TopologyVersion` gain a `ToString(bool forceStaticSerializerRegistry)` overload, used by `StructuredLogTemplateProviders`). Infrastructure BSON built before the wire layer forces the static registry too: the handshake client document (`ClientDocumentHelper`), the SASL auth payloads and server responses (`OidcSaslStep`, `AWSFirstSaslStep`, `AWSLastSaslStep`), and the hedge rendering of `ReadPreference`/`ReadPreferenceHedge.ToString()` — these can run on driver-owned threads (SDAM monitors, connection opening) that inherit the application's execution context, where a contextual lookup would race with the registry initialization. For the same reason the `BsonValue` family propagates `ForceStaticSerializerRegistry` through every recursive step: the `BsonValueSerializer` type dispatcher (document, array and JavaScript-with-scope cases), the `BsonDocumentSerializer` id-first path, and the `BsonJavaScriptWithScopeSerializer` scope.
- `MongoDatabaseSettings.SerializerRegistry` and `MongoCollectionSettings.SerializerRegistry` become **settable** (upstream they are read-only aliases of the static registry); collection settings inherit the database's registry via `ApplyDefaultValues`.
- `BsonDefaults.DynamicArraySerializer`/`DynamicDocumentSerializer` properties are replaced by `GetDynamicArraySerializer(bool)`/`GetDynamicDocumentSerializer(bool)` + `SetDynamicArraySerializer`/`SetDynamicDocumentSerializer` methods, so dynamic serializer lookup is registry-aware too.

### Discriminator convention default (behavioral)
`BsonSerializer.LookupDiscriminatorConvention` is rewritten: for a class whose base is `object` with no convention registered for `object`, the fork defaults to `StandardDiscriminatorConvention.Hierarchical` where upstream defaults to `Scalar`. Documents containing discriminators (`_t`) can therefore serialize **differently from the official driver** (hierarchy array vs. single name). Conventions are also registered per-type on lookup rather than walking/registering the whole ancestor chain upstream-style.

### Serialize-as-nominal-type honored by class maps (behavioral)
`BsonClassMapSerializer<TClass>.Serialize` honors `BsonSerializationArgs.SerializeAsNominalType`: when the flag is set, the value serializes through the class map of `TClass` even when its runtime type differs, instead of delegating to the actual type's serializer. This matches the behavior `ClassSerializerBase` and `EnumerableSerializerBase` already have upstream (where the flag is otherwise ignored by class maps, making `SerializeAsNominalTypeSerializer` unusable with class-mapped types), and lets a caller serialize a runtime-generated subclass — e.g. a MongODM proxy model — through its base class map. Upstream support is requested by CSHARP-3153. MongODM relies on this behavior for proxy model serialization since MODM-189.

### Secondary API changes
- `FieldValueSerializerHelper` is `public` (internal upstream).
- Otherwise cosmetic whitespace/EOL differences only. (The MONGODB-AWS error messages had been mangled to "Etherna.MongoDB-AWS" by the blanket namespace rename — a mechanism name that does not exist — and were restored to match upstream.)

### Threading the flag: known non-forwarding sites
The flag is only forced on paths that actually reach a serializer. These deliberately ignore it, and an audit that flags "declared but unused" should not treat them as defects:
- `WriteEndDocument(bool)` on `BsonWriter`/`BsonBinaryWriter`/`BsonDocumentWriter`/`JsonWriter` — nothing is serialized at end-of-document; the parameter exists only so `ElementAppendingBsonWriter`, which does append elements there, can honor it.
- `CompressedMessageBinaryEncoder.WriteMessage` — the original message arrives already encoded, so compression never consults a registry (the *read* side does, and forwards).
- The JSON encoders (`QueryMessageJsonEncoder`, `ReplyMessageJsonEncoder`, `CompressedMessageJsonEncoder`) drop the flag. Nothing in the fork constructs a `JsonMessageEncoderFactory` — they are off every wire path — so they were left alone rather than widening the delta.

To re-run the audit after an upstream merge, compare the count of `bool forceStaticSerializerRegistry` declarations against total occurrences per file; a file where they are equal never uses the parameter it declares.

### Removed from the repository
Upstream `tests/`, `specifications/` (spec-test corpora), `benchmarks/`, `evergreen/` (upstream CI), `Release/` tooling, and `apidocs/` are deleted. CI is a single GitHub Actions pipeline (`.github/workflows/nuget-stable-deploy.yml`).

Correctness of the **merged upstream surface** relies on upstream's own testing of the merged release tag, so keep fork-specific changes minimal and reviewable. The fork's own suite (`test/Etherna.MongoDB.Fork.Tests`) covers **only the fork delta documented above** — it deliberately does not re-test upstream behavior. See "Fork test suite" below.

## Compatibility notes
- The fork is a **source-compatible sibling**, not a drop-in binary replacement: namespaces differ, assemblies are not strong-named, and some upstream public signatures changed (e.g. `BsonSerializer.SerializerRegistry`, `IBsonWriter.WriteEndDocument`, `BsonDefaults` accessors).
- Upstream's semantic versioning applies to the merged upstream surface; fork-specific API listed above evolves with `eth-v*` releases.

## Editing
- Be careful to preserve file BOMs.
- Keep the fork delta minimal: prefer optional parameters with upstream-compatible defaults (as done throughout) so future upstream merges stay tractable.

## Async conventions (inherited from upstream, still binding)
- **Paired sync/async public surface.** Public I/O methods come in pairs (`Foo` / `FooAsync` with a `CancellationToken`); changes must land on both sides. Operations under `src/MongoDB.Driver/Core/Operations/` duplicate the pairing internally (`Execute`/`ExecuteAsync`) — never collapse them with sync-over-async shortcuts.
- **`ConfigureAwait(false)`** on all library `await`s.
- **Cancellation / CSOT propagation.** Pass `CancellationToken` and `OperationContext` through unchanged; substituting a fresh context mid-stack drops the caller's deadline.

## Fork test suite
`test/Etherna.MongoDB.Fork.Tests` (xunit, `net8.0`, `IsPackable=false`) exists to pin **the fork delta and nothing else**. Every test must correspond to an entry under "Differences vs the official driver"; if a behavior is identical in upstream v3.12.0, it does not belong here. Two behaviors that look fork-specific but are not, and were deliberately left out: the `_t` array emitted inside a `[BsonDiscriminator(RootClass = true)]` hierarchy (upstream's `BsonClassMap` registers the hierarchical convention itself), and the scalar `_t` of an ordinary base/derived pair.

Constraints that shape the suite:
- **Public API only.** The `InternalsVisibleTo` entries in `src/*/Properties/AssemblyInfo.cs` still name upstream assemblies *with MongoDB's public key*, and fork assemblies are unsigned, so no fork test project can satisfy them. The wire-protocol encoders, `BinaryConnection` and `StructuredLogTemplateProviders` are therefore unreachable; their force-static rule is covered one level down through the public `ToString(bool)` overloads on `ClusterDescription`/`ServerDescription`/`TopologyVersion`, through `ReadPreference`/`ReadPreferenceHedge`, and through `RawBsonDocument`'s public `GetValue`/`TryGetValue`/`Materialize` overloads (the calls `CommandUsingQueryMessageWireProtocol` makes to materialize `$clusterTime`/`operationTime`/error documents).
- **Serial execution.** `BsonSerializer`'s registries, the serialization-context accessor, the discriminator conventions and the class map registry are all process-global, so `AssemblyInfo.cs` disables test parallelization.
- **Irreversible global state.** Class maps, discriminator conventions and serializer registrations can never be unregistered — every test uses its own throwaway types. `BsonDefaults.SetDynamicArraySerializer`/`SetDynamicDocumentSerializer` latch a flag no API can clear, so the suite exercises only the getters.
- **Order-sensitive observations** are captured in a `[ModuleInitializer]` (`Support/PristineDiscriminatorState.cs`) because the fork's hierarchical discriminator default only holds while `typeof(object)` has no registered convention — and constructing an `ObjectSerializer` anywhere registers one permanently.

## Upstream merge checklist
When merging a new official release tag into `etherna`:
1. Merge the upstream tag (e.g. `git merge mongodb/vX.Y.Z` or the `vX.Y.Z` tag from the `mongodb` remote).
2. Delete re-added upstream infrastructure (`tests/`, `specifications/`, `benchmarks/`, `evergreen/`, `Release/`, `apidocs/`, upstream workflows) and keep the solution limited to the four `src/` projects plus `test/Etherna.MongoDB.Fork.Tests`.
3. Re-apply the namespace rename `MongoDB.*` → `Etherna.MongoDB.*` on new/changed files, and keep the `Etherna.*.csproj` file names.
4. Re-thread `forceStaticSerializerRegistry` through any new serialization entry point; new wire-protocol read/write paths and log formatters must force the static registry.
5. Build all target frameworks: `dotnet build CSharpDriver.sln -c Release`.
6. Run the fork suite: `dotnet test CSharpDriver.sln -c Release`. It is the regression net for the delta in step 4.

## Commands
- Build: `dotnet build CSharpDriver.sln`
- Test: `dotnet test CSharpDriver.sln` (fork-delta tests only — see "Fork test suite")
- Pack (what CI publishes): `dotnet pack --configuration Release -o nupkg`

## Commit and PR Conventions
- Fork work uses plain descriptive commit messages (e.g. `fix static serializer registry with binary connection`, `merge 3.8.1`); reference an Etherna Jira key (e.g. `MODM-xxx`) when one exists.
- Upstream-merged commits keep their original `CSHARP-xxxx:` prefixes.
- PRs target the `etherna` branch.
