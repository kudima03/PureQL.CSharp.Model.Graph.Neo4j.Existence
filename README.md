# PureQL.CSharp.Model.Graph.Neo4j.Existence

Tells whether the Neo4j schema of the **PureQL** query graphs and a stored query exist.

[![.NET build & test](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Existence/actions/workflows/build-and-test.yml/badge.svg?branch=main)](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Existence/actions/workflows/build-and-test.yml)
[![Build and Deploy](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Existence/actions/workflows/publish-nuget.yml/badge.svg?branch=main)](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Existence/actions/workflows/publish-nuget.yml)
[![NuGet](https://img.shields.io/nuget/v/PureQL.CSharp.Model.Graph.Neo4j.Existence)](https://www.nuget.org/packages/PureQL.CSharp.Model.Graph.Neo4j.Existence)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Overview

`PureQL.CSharp.Model.Graph.Neo4j.Existence` is the existence package of the `PureQL.CSharp.Model.Graph` family: it answers whether the constraint and indexes of the query graphs exist in a Neo4j database, and whether a query with a given id is stored there.

## Public API

Namespace `PureQL.CSharp.Model.Graph.Neo4j.Existence`:

| Name | Description |
|------|-------------|
| `SchemaExistence` | `sealed record SchemaExistence(IAsyncQueryRunner Runner)`; `Task<bool> ValueAsync(CancellationToken ct = default)` is `true` when the constraint and the three indexes of `SchemaNames` all exist |
| `QueryExistence` | `sealed record QueryExistence(IAsyncQueryRunner Runner, Guid Id)`; `Task<bool> ValueAsync(CancellationToken ct = default)` is `true` when a query with `Id` is stored |
| `SchemaExistenceOf` | `sealed record SchemaExistenceOf(INeo4jStoredQueries Store)`: `SchemaExistence` in a session of its own |
| `QueryExistenceOf` | `sealed record QueryExistenceOf(INeo4jStoredQueries Store, Guid Id)`: `QueryExistence` in a session of its own |
| `SchemaNames` | The names of the constraint and indexes `PureQL.CSharp.Model.Graph.Neo4j.Creation` creates: `QueryId` (`pureql_query_id`, `Query.id` unique), `NodeQueryId` (`pureql_node_query_id`, `Node(queryId)`), `NodeFieldSource` (`pureql_node_field_source`, `Node(field, source)`) and `NodeParam` (`pureql_node_param`, `Node(param)`) |

The records run on an `IAsyncQueryRunner`, so they work on an `IAsyncSession` or inside an `IAsyncTransaction` next to the operations of the other packages; the `...Of` records open a session on `Store.Driver` against `Store.Database` and run auto-commit queries. Every call of `ValueAsync` reads the database again.

### Statements

- Schema: `SHOW CONSTRAINTS YIELD name RETURN name` and `SHOW INDEXES YIELD name RETURN name`, matched on the `name` column only (an equivalent index under another name does not count).
- Query: `MATCH (q:Query {id: $id}) RETURN count(q) > 0 AS found`, with `$id` the id in "D" format.

This package does not depend on Creation: `SchemaNames` duplicates its names, and the two must stay equal.

## Usage

```csharp
using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;
using PureQL.CSharp.Model.Graph.Neo4j.Existence;

await using IDriver driver = GraphDatabase.Driver(
    "neo4j://localhost:7687",
    AuthTokens.Basic("neo4j", "password")
);
INeo4jStoredQueries store = new Neo4jStoredQueries(driver);

if (!await new SchemaExistenceOf(store).ValueAsync())
{
    // create it with PureQL.CSharp.Model.Graph.Neo4j.Creation
}

Guid id = /* the id a visualization holds */;
bool stored = await new QueryExistenceOf(store, id).ValueAsync();

// Or inside a transaction shared with other operations:
await using IAsyncSession session = driver.AsyncSession(
    builder => builder.WithDatabase(store.Database)
);
bool storedInTransaction = await session.ExecuteReadAsync(
    transaction => new QueryExistence(transaction, id).ValueAsync()
);
```

## Design Principles

- **Sealed records** — the dependencies in the constructor, one question per record
- **Driver-level** — plain `Neo4j.Driver` statements, no reflection; values are plain CLR types
- **AOT-compatible** — `IsAotCompatible=true`

## Dependencies

- [`PureQL.CSharp.Model.Graph`](https://github.com/kudima03/PureQL.CSharp.Model.Graph) `0.1.0-preview.1.1.0`
- [`PureQL.CSharp.Model.Graph.Neo4j.Abstractions`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Abstractions) `0.1.0-preview.1.1.0`
- [`Neo4j.Driver`](https://www.nuget.org/packages/Neo4j.Driver) `6.5.1`

## Target Frameworks

- .NET 11

## Installation

```
dotnet add package PureQL.CSharp.Model.Graph.Neo4j.Existence
```

## Related packages

- [`PureQL.CSharp.Model.Graph`](https://github.com/kudima03/PureQL.CSharp.Model.Graph) — maps a `PureQLQuery` to a property graph and back, without a driver
- [`PureQL.CSharp.Model.Graph.Neo4j.Abstractions`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Abstractions) — the stored-queries contract
- [`PureQL.CSharp.Model.Graph.Neo4j.Creation`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Creation) — constraints and indexes
- [`PureQL.CSharp.Model.Graph.Neo4j.Insertion`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Insertion) — writes a query
- [`PureQL.CSharp.Model.Graph.Neo4j.Querying`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Querying) — reads queries back
- [`PureQL.CSharp.Model.Graph.Neo4j.Deletion`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Deletion) — deletes queries
- [`PureQL.CSharp.Model.Graph.Neo4j.Updating`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Updating) — replaces a query in one transaction
- [`PureQL.CSharp.Model.Graph.Neo4j.Destruction`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Destruction) — drops the schema and every stored query
- [`PureQL.CSharp.Model.Graph.Neo4j.Migration`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Migration) — migrates stored graphs between model versions
