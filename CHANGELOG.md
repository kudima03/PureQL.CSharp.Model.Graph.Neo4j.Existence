# Changelog

All notable changes to `PureQL.CSharp.Model.Graph.Neo4j.Existence` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
