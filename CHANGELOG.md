# Changelog

All notable changes to `PureQL.CSharp.Model.Graph.Neo4j.Existence` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed

- **Breaking:** depends on `PureQL.CSharp.Model.Graph` `0.1.0-preview.2.0.0`
  and `PureQL.CSharp.Model.Graph.Neo4j.Abstractions` `0.1.0-preview.2.0.0`
  (were `0.1.0-preview.1.1.0`), and through them on `PureQL.CSharp.Model`
  `0.1.0-preview.15.0.0`, the model of PureQL specification
  `0.1.0-preview.3.0.0`. The public API of this package is unchanged, but
  `INeo4jStoredQueries` comes from the new Abstractions release and the
  stored query graphs follow the new model.
- The schema and query checks are unchanged: a query is still found by its
  `Query` root node. The tests use `PureQL.CSharp.Model.Samples`
  `0.1.0-preview.4.0.0`.

## [0.1.0-preview.0.2.0] - 2026-11-11

### Changed

- Updated `PureQL.CSharp.Model.Graph` and
  `PureQL.CSharp.Model.Graph.Neo4j.Abstractions` to 0.1.0-preview.1.1.0, their
  .NET 11 releases.
- The build and publish workflows install the .NET 11 SDK by version; the
  `global.json` pin is removed.

## [0.1.0-preview.0.1.0] - 2026-11-09

First release: tells whether the schema of the PureQL query graphs and a stored
query exist in a Neo4j database.

### Added

- **`SchemaNames`** — the names of the uniqueness constraint (`QueryId`,
  `pureql_query_id`) and the three indexes (`NodeQueryId`, `NodeFieldSource`,
  `NodeParam`) that `PureQL.CSharp.Model.Graph.Neo4j.Creation` creates.
- **`SchemaExistence`** — `sealed record SchemaExistence(IAsyncQueryRunner Runner)`;
  `ValueAsync` is `true` when `SHOW CONSTRAINTS` and `SHOW INDEXES` list all four
  names of `SchemaNames`. It matches on names only, so an equivalent index under
  another name makes it `false`.
- **`QueryExistence`** — `sealed record QueryExistence(IAsyncQueryRunner Runner,
  Guid Id)`; `ValueAsync` is `true` when a query with that id is stored.
- **`SchemaExistenceOf`** and **`QueryExistenceOf`** — the same answers for an
  `INeo4jStoredQueries`, read in a session of their own on its driver and
  database.
- Depends on `PureQL.CSharp.Model.Graph` 0.1.0-preview.1.0.0,
  `PureQL.CSharp.Model.Graph.Neo4j.Abstractions` 0.1.0-preview.1.0.0 and
  `Neo4j.Driver` 6.5.1.
