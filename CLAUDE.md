# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

All `dotnet` commands must be run from the `./src` directory. The projects need the .NET 11 SDK (for the model's C# 15 unions); where the system SDK is older, use `~/.dotnet/dotnet` or put it first on `PATH` with `DOTNET_ROOT=$HOME/.dotnet`.

```bash
dotnet restore
dotnet build --no-restore -warnaserror
dotnet format --verify-no-changes          # check code style (CI enforces this)
dotnet format                              # auto-fix code style
dotnet tool restore && dotnet csharpier check .   # check formatting (CI enforces this)
dotnet test --no-build --verbosity normal  # run tests
dotnet pack --configuration Release -p:Version=<version> --output .
```

CI additionally passes `-p:AssemblyVersion` (pinned to the major) and `-p:FileVersion`; see `.github/workflows/publish-nuget.yml`.

## Architecture

This is the **Neo4j existence package** of the `PureQL.CSharp.Model.Graph` family. It depends on `PureQL.CSharp.Model.Graph` `0.1.0-preview.1.0.0`, `PureQL.CSharp.Model.Graph.Neo4j.Abstractions` `0.1.0-preview.1.0.0` and `Neo4j.Driver` `6.5.1`. Namespace `PureQL.CSharp.Model.Graph.Neo4j.Existence`, one public type per file:

- `SchemaNames` — the names of the uniqueness constraint (`QueryId`, `pureql_query_id`) and the three indexes (`NodeQueryId`, `NodeFieldSource`, `NodeParam`: `pureql_node_query_id`, `pureql_node_field_source`, `pureql_node_param`), plus the internal list `Indexes`. `PureQL.CSharp.Model.Graph.Neo4j.Creation` creates them under these names; this package must not reference Creation (no operation package may be referenced), so they are duplicated and must stay equal to Creation's.
- `SchemaExistence(IAsyncQueryRunner Runner)` — `ValueAsync` runs `SHOW CONSTRAINTS YIELD name RETURN name` and, if the constraint is listed, `SHOW INDEXES YIELD name RETURN name`; `true` when all four names are present. Names only: kind and properties are not checked.
- `QueryExistence(IAsyncQueryRunner Runner, Guid Id)` — `ValueAsync` runs `MATCH (q:Query {id: $id}) RETURN count(q) > 0 AS found` (built from `GraphLabels.Query` and `GraphProperties.Id`), `$id` the "D" string.
- `SchemaExistenceOf(INeo4jStoredQueries Store)` and `QueryExistenceOf(INeo4jStoredQueries Store, Guid Id)` — open an `IAsyncSession` on `Store.Driver` against `Store.Database` and run the records above on it (auto-commit).

The public API is the shared contract of the `PureQL.CSharp.Model.Graph` family: do not rename or reshape it. Parameters go to the driver as `Dictionary<string, object>` (no anonymous objects, which the driver reads by reflection), and results are read with casts, so the package stays AOT-compatible. Every public member has XML documentation (`GenerateDocumentationFile`; a missing comment fails the build under `-warnaserror`).

**Test project:** `PureQL.CSharp.Model.Graph.Neo4j.Existence.Tests` targets `net11.0` and uses xunit with coverlet, `PureQL.CSharp.Model.Samples` and `Testcontainers.Neo4j` (image `neo4j:5-community`, random host ports; Docker is required). `Neo4jFixture` starts one server for the `Neo4jServer` collection and holds the statements the tests prepare the database with, duplicated from the sibling packages since they cannot be referenced: the schema as Creation creates it (`CREATE CONSTRAINT pureql_query_id IF NOT EXISTS FOR (q:Query) REQUIRE q.id IS UNIQUE`, `CREATE INDEX <name> IF NOT EXISTS FOR (n:Node) ON (...)`) and a query graph as Insertion writes it (one `UNWIND $nodes ... CREATE (n:$(node.labels)) ... UNWIND $relationships ... CREATE (parent)-[r:$(relationship.type)]->(child)` statement over `QueryGraphs.Of`). Before each test it deletes every node, constraint and index but the lookup indexes, since an equivalent index under another name would turn `CREATE INDEX IF NOT EXISTS` into a no-op. `SchemaExistenceTests` and `QueryExistenceTests` cover an empty database, the created schema with each piece dropped, a stored sample and another id, transactions, the `...Of` records and cancellation; `SchemaNamesTests` checks the names.

**Target framework:** net11.0 only, as the model. `IsAotCompatible` is `true`.

**Package validation:** enabled against `PackageValidationBaselineVersion` `0.1.0-preview.0.1.0`, the first release; the `nuget-publish` skill moves it after each release, and intentional breaks go to `CompatibilitySuppressions.xml` next to the csproj.

**Mutation testing:** the Stryker step of CI is commented out.

**Publishing:** triggered by pushing a semver tag (pattern `*.*.*`). The tag becomes the `PackageVersion`. The workflow publishes to both GitHub Packages and NuGet.org.

## Code Style

Enforced via `.editorconfig`, `dotnet format --verify-no-changes` and `dotnet csharpier check .` in CI:

- No `var` — always use explicit types
- No expression-bodied methods or constructors — use block bodies
- Expression-bodied properties and accessors are required
- File-scoped namespace declarations (`namespace Foo.Bar;`)
- Private fields prefixed with `_` in camelCase (e.g., `_options`)
- Braces always required, even for single-statement blocks
- Max line length: 90 characters
- Allman brace style — opening brace on its own line for all blocks
- `System.*` using directives sorted first, no blank line between using groups
- Parentheses required in arithmetic and relational binary operators for clarity

## Commit Messages

Do not mention Claude or AI assistance in commit messages.
