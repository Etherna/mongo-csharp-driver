# AGENTS.md - Etherna.MongoDB

## Overview
This repository is the **Etherna fork** of the official [mongo-csharp-driver](https://github.com/mongodb/mongo-csharp-driver), maintained by Etherna SA and published on NuGet as the `Etherna.MongoDB.*` packages. All namespaces are renamed from `MongoDB.*` to `Etherna.MongoDB.*`, so the fork installs side-by-side with the official driver without conflicts.

The fork exists primarily to support **scoped, per-context BSON serialization**: the official driver resolves serializers through global static registries, which prevents hosting multiple isolated serialization contexts (e.g. MongODM-style DbContexts, one per tenant/domain) in the same process. See "Differences vs the official driver" below.

## Fork model
- Branch `etherna` is the fork's main line. Remote `origin` is `Etherna/mongo-csharp-driver`; remote `mongodb` is the official repository.
- Official release tags are merged into `etherna` periodically (3.2.1 → 3.3.0 → 3.4.0 → 3.5.0 → 3.6.0 → 3.8.1 → **3.10.0**, the current upstream baseline).
- Fork releases are tagged `eth-v<major>.<minor>.<patch>`. Versions are computed by GitVersion (`GitVersion.yml` + `GitVersion.MsBuild`); pushing an `eth-v*` tag triggers `.github/workflows/nuget-stable-deploy.yml`, which builds, packs, and pushes to NuGet.

## Tech Stack
- .NET library projects producing NuGet packages
- Multi-targeted `netstandard2.1;net472;net6.0`
- C# LangVersion 12

## Project Structure
Only `src/` is tracked. The upstream test suites, spec corpora, and CI are **not part of the fork** (see below); the solution contains only the four library projects.

| Directory | Project file | NuGet package |
|---|---|---|
| `src/MongoDB.Bson/` | `Etherna.MongoDB.Bson.csproj` | `Etherna.MongoDB.Bson` |
| `src/MongoDB.Driver/` | `Etherna.MongoDB.Driver.csproj` | `Etherna.MongoDB.Driver` |
| `src/MongoDB.Driver.Encryption/` | `Etherna.MongoDB.Driver.Encryption.csproj` | `Etherna.MongoDB.Driver.Encryption` |
| `src/MongoDB.Driver.Authentication.AWS/` | `Etherna.MongoDB.Driver.Authentication.AWS.csproj` | `Etherna.MongoDB.Driver.Authentication.AWS` |

Directories such as `tests/`, `benchmarks/`, or `specifications/` may exist on disk as ignored leftovers of upstream merges: they are untracked, not referenced by the solution, and reference the old upstream project names. Do not edit or build them.

## Differences vs the official driver
Baseline for this section: upstream tag `v3.10.0`. Beyond the mechanical namespace rename (~1900 files), the semantic delta is small and deliberate.

### Packaging & naming
- Namespaces and project/package IDs renamed `MongoDB.*` → `Etherna.MongoDB.*`; `Authors` is `Etherna SA` and package descriptions state "Fork by Etherna of...".
- **Strong-name signing removed** (`SignAssembly` and the `.snk` dropped from `src/Directory.Build.props`): fork assemblies are not strong-named, unlike the official ones.
- Package icon removed; versioning switched from build-injected `$(Version)` to GitVersion.

### Contextual serializer registry (the core fork feature)
- New public interface `Etherna.MongoDB.Bson.Serialization.ISerializationContextAccessor` (`src/MongoDB.Bson/Serialization/ISerializationContextAccessor.cs`), installed via `BsonSerializer.SetSerializationContextAccessor(...)`. It lets the application resolve a *per-context* `IBsonSerializerRegistry` (e.g. from an async-local scope) instead of the process-wide static one.
- The static property `BsonSerializer.SerializerRegistry` is **replaced** by `BsonSerializer.GetSerializerRegistry(bool forceStaticSerializerRegistry = false)`: it asks the accessor first and falls back to the static registry. `BsonSerializer.LookupSerializer(...)` gains the same optional `forceStaticSerializerRegistry` parameter.
- A `ForceStaticSerializerRegistry` flag is threaded through the serialization pipeline so that specific code paths can bypass the contextual registry: `BsonSerializationArgs`/`BsonDeserializationArgs`, `BsonDeserializationContext.CreateRoot`/`.With`, `IBsonSerializerExtensions.Serialize`/`Deserialize`, `IBsonWriter.WriteEndDocument`, `BsonDocument.GetValue`/`TryGetValue`/`ToString`, `RawBsonDocument.Materialize`, and the (internal) wire-protocol message encoders (`IMessageEncoder.ReadMessage`/`WriteMessage` and the section formatters).
- **The wire layer always forces the static registry**: `BinaryConnection` reads and writes protocol messages with `forceStaticSerializerRegistry: true`, and `CommandUsingQueryMessageWireProtocol` materializes `$clusterTime`/`operationTime`/error documents the same way. Protocol-level BSON must never be affected by an application's contextual registry. SDAM logging does the same (`ClusterDescription`/`ServerDescription`/`TopologyVersion` gain a `ToString(bool forceStaticSerializerRegistry)` overload, used by `StructuredLogTemplateProviders`).
- `MongoDatabaseSettings.SerializerRegistry` and `MongoCollectionSettings.SerializerRegistry` become **settable** (upstream they are read-only aliases of the static registry); collection settings inherit the database's registry via `ApplyDefaultValues`.
- `BsonDefaults.DynamicArraySerializer`/`DynamicDocumentSerializer` properties are replaced by `GetDynamicArraySerializer(bool)`/`GetDynamicDocumentSerializer(bool)` + `SetDynamicArraySerializer`/`SetDynamicDocumentSerializer` methods, so dynamic serializer lookup is registry-aware too.

### Discriminator convention default (behavioral)
`BsonSerializer.LookupDiscriminatorConvention` is rewritten: for a class whose base is `object` with no convention registered for `object`, the fork defaults to `StandardDiscriminatorConvention.Hierarchical` where upstream defaults to `Scalar`. Documents containing discriminators (`_t`) can therefore serialize **differently from the official driver** (hierarchy array vs. single name). Conventions are also registered per-type on lookup rather than walking/registering the whole ancestor chain upstream-style.

### Secondary API changes
- `FieldValueSerializerHelper` is `public` (internal upstream).
- MONGODB-AWS error messages reworded ("MongoDB-AWS ..."); otherwise cosmetic whitespace/EOL differences only.

### Removed from the repository
Upstream `tests/`, `specifications/` (spec-test corpora), `benchmarks/`, `evergreen/` (upstream CI), `Release/` tooling, and `apidocs/` are deleted. CI is a single GitHub Actions pipeline (`.github/workflows/nuget-stable-deploy.yml`); the fork carries **no test suite of its own** — correctness of merged code relies on upstream's testing of the merged release tag, so keep fork-specific changes minimal and reviewable.

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

## Upstream merge checklist
When merging a new official release tag into `etherna`:
1. Merge the upstream tag (e.g. `git merge mongodb/vX.Y.Z` or the `vX.Y.Z` tag from the `mongodb` remote).
2. Delete re-added upstream infrastructure (`tests/`, `specifications/`, `benchmarks/`, `evergreen/`, `Release/`, `apidocs/`, upstream workflows) and keep the solution limited to the four `src/` projects.
3. Re-apply the namespace rename `MongoDB.*` → `Etherna.MongoDB.*` on new/changed files, and keep the `Etherna.*.csproj` file names.
4. Re-thread `forceStaticSerializerRegistry` through any new serialization entry point; new wire-protocol read/write paths and log formatters must force the static registry.
5. Build all target frameworks: `dotnet build CSharpDriver.sln -c Release`.

## Commands
- Build: `dotnet build CSharpDriver.sln`
- Pack (what CI publishes): `dotnet pack --configuration Release -o nupkg`
- There is no runnable test suite in this fork (see "Removed from the repository").

## Commit and PR Conventions
- Fork work uses plain descriptive commit messages (e.g. `fix static serializer registry with binary connection`, `merge 3.8.1`); reference an Etherna Jira key (e.g. `MODM-xxx`) when one exists.
- Upstream-merged commits keep their original `CSHARP-xxxx:` prefixes.
- PRs target the `etherna` branch.
