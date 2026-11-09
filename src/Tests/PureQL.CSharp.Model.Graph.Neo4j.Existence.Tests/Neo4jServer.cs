namespace PureQL.CSharp.Model.Graph.Neo4j.Existence.Tests;

/// <summary>The tests that share one Neo4j server; they run one after another.</summary>
[CollectionDefinition(Name)]
public sealed class Neo4jServer : ICollectionFixture<Neo4jFixture>
{
    public const string Name = "Neo4j";
}
