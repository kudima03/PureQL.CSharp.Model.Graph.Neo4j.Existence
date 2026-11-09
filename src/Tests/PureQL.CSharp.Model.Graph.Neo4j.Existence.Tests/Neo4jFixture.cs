using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;
using Testcontainers.Neo4j;

namespace PureQL.CSharp.Model.Graph.Neo4j.Existence.Tests;

/// <summary>
/// A Neo4j server in a container, shared by the tests of <see cref="Neo4jServer"/>,
/// with the statements the tests prepare the database with.
/// </summary>
public sealed class Neo4jFixture : IAsyncLifetime
{
    /// <summary>The schema as Creation creates it.</summary>
    public static readonly IReadOnlyList<string> SchemaCreation =
    [
        "CREATE CONSTRAINT pureql_query_id IF NOT EXISTS "
            + "FOR (q:Query) REQUIRE q.id IS UNIQUE",
        "CREATE INDEX pureql_node_query_id IF NOT EXISTS "
            + "FOR (n:Node) ON (n.queryId)",
        "CREATE INDEX pureql_node_field_source IF NOT EXISTS "
            + "FOR (n:Node) ON (n.field, n.source)",
        "CREATE INDEX pureql_node_param IF NOT EXISTS FOR (n:Node) ON (n.param)",
    ];

    /// <summary>A query graph as Insertion writes it, in one statement.</summary>
    public const string Insertion = """
        UNWIND $nodes AS node
        CREATE (n:$(node.labels))
        SET n = node.properties
        WITH collect(n) AS created
        UNWIND $relationships AS relationship
        WITH created[relationship.from] AS parent,
             created[relationship.to] AS child,
             relationship
        CREATE (parent)-[r:$(relationship.type)]->(child)
        SET r.position = relationship.position
        """;

    private const string Constraints = "SHOW CONSTRAINTS YIELD name RETURN name";

    private const string Indexes =
        "SHOW INDEXES YIELD name, type, owningConstraint "
        + "WHERE type <> 'LOOKUP' AND owningConstraint IS NULL RETURN name";

    private readonly Neo4jContainer _container = new Neo4jBuilder(
        "neo4j:5-community"
    ).Build();

    private IDriver? _driver;

    public IDriver Driver =>
        _driver ?? throw new InvalidOperationException("The server is not started.");

    public INeo4jStoredQueries Store => new Neo4jStoredQueries(Driver);

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _driver = GraphDatabase.Driver(_container.GetConnectionString(), AuthTokens.None);
    }

    public async Task DisposeAsync()
    {
        if (_driver is not null)
        {
            await _driver.DisposeAsync();
        }

        await _container.DisposeAsync();
    }

    /// <summary>
    /// Deletes every node, constraint and index but the built-in lookup indexes, so
    /// that an equivalent index under another name cannot turn the creation of the
    /// schema into a no-op.
    /// </summary>
    public async Task ResetAsync()
    {
        await RunAsync("MATCH (n) DETACH DELETE n");
        foreach (string name in await NamesAsync(Constraints))
        {
            await RunAsync($"DROP CONSTRAINT `{name}`");
        }

        foreach (string name in await NamesAsync(Indexes))
        {
            await RunAsync($"DROP INDEX `{name}`");
        }
    }

    public async Task CreateSchemaAsync()
    {
        await RunAsync(SchemaCreation);
    }

    public async Task RunAsync(params IEnumerable<string> statements)
    {
        await using IAsyncSession session = Session();
        foreach (string statement in statements)
        {
            IResultCursor cursor = await session.RunAsync(statement);
            _ = await cursor.ConsumeAsync();
        }
    }

    public async Task InsertAsync(QueryGraph graph)
    {
        await using IAsyncSession session = Session();
        IResultCursor cursor = await session.RunAsync(Insertion, Parameters(graph));
        _ = await cursor.ConsumeAsync();
    }

    private async Task<List<string>> NamesAsync(string statement)
    {
        await using IAsyncSession session = Session();
        IResultCursor cursor = await session.RunAsync(statement);
        return await cursor.ToListAsync(record => (string)record["name"]);
    }

    public IAsyncSession Session()
    {
        return Driver.AsyncSession(builder => builder.WithDatabase(Store.Database));
    }

    private static Dictionary<string, object> Parameters(QueryGraph graph)
    {
        List<object> nodes = [];
        foreach (WrittenNode node in graph.Nodes.OrderBy(node => node.Index))
        {
            nodes.Add(
                new Dictionary<string, object>
                {
                    ["labels"] = node.Labels.ToList(),
                    ["properties"] = new Dictionary<string, object>(node.Properties),
                }
            );
        }

        List<object> relationships = [];
        foreach (WrittenRelationship relationship in graph.Relationships)
        {
            relationships.Add(
                new Dictionary<string, object?>
                {
                    ["from"] = (long)relationship.From,
                    ["to"] = (long)relationship.To,
                    ["type"] = relationship.Type,
                    ["position"] = relationship.Position,
                }
            );
        }

        return new Dictionary<string, object>
        {
            ["nodes"] = nodes,
            ["relationships"] = relationships,
        };
    }
}
