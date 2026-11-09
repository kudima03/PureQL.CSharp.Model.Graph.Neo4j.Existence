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
| `SchemaNames` | The names of the constraint and indexes `PureQL.CSharp.Model.Graph.Neo4j.Creation` creates |

## Dependencies

- [`PureQL.CSharp.Model.Graph`](https://github.com/kudima03/PureQL.CSharp.Model.Graph) `0.1.0-preview.1.0.0`
- [`PureQL.CSharp.Model.Graph.Neo4j.Abstractions`](https://github.com/kudima03/PureQL.CSharp.Model.Graph.Neo4j.Abstractions) `0.1.0-preview.1.0.0`
- [`Neo4j.Driver`](https://www.nuget.org/packages/Neo4j.Driver) `6.5.1`

## Target Frameworks

- .NET 11

## Installation

```
dotnet add package PureQL.CSharp.Model.Graph.Neo4j.Existence
```
