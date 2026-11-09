using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Existence.Tests;

[Collection(Neo4jServer.Name)]
public sealed record SchemaExistenceTests : IAsyncLifetime
{
    private readonly Neo4jFixture _fixture;

    public SchemaExistenceTests(Neo4jFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task IsFalseOnEmptyDatabase()
    {
        Assert.False(await ValueAsync());
    }

    [Fact]
    public async Task IsTrueAfterSchemaIsCreated()
    {
        await _fixture.CreateSchemaAsync();
        Assert.True(await ValueAsync());
    }

    [Theory]
    [InlineData("pureql_node_query_id")]
    [InlineData("pureql_node_field_source")]
    [InlineData("pureql_node_param")]
    public async Task IsFalseWhenOneIndexIsDropped(string index)
    {
        await _fixture.CreateSchemaAsync();
        await _fixture.RunAsync($"DROP INDEX {index}");
        Assert.False(await ValueAsync());
    }

    [Fact]
    public async Task IsFalseWhenConstraintIsDropped()
    {
        await _fixture.CreateSchemaAsync();
        await _fixture.RunAsync("DROP CONSTRAINT pureql_query_id");
        Assert.False(await ValueAsync());
    }

    [Fact]
    public async Task IsFalseWithIndexesUnderOtherNames()
    {
        await _fixture.RunAsync(
            "CREATE CONSTRAINT pureql_query_id FOR (q:Query) REQUIRE q.id IS UNIQUE",
            "CREATE INDEX other_query_id FOR (n:Node) ON (n.queryId)",
            "CREATE INDEX other_field_source FOR (n:Node) ON (n.field, n.source)",
            "CREATE INDEX other_param FOR (n:Node) ON (n.param)"
        );
        Assert.False(await ValueAsync());
    }

    [Fact]
    public async Task IsTrueInsideTransaction()
    {
        await _fixture.CreateSchemaAsync();
        await using IAsyncSession session = _fixture.Session();
        Assert.True(
            await session.ExecuteReadAsync(transaction =>
                new SchemaExistence(transaction).ValueAsync()
            )
        );
    }

    [Fact]
    public async Task FollowsCreationOverStore()
    {
        SchemaExistenceOf existence = new SchemaExistenceOf(_fixture.Store);
        Assert.False(await existence.ValueAsync());
        await _fixture.CreateSchemaAsync();
        Assert.True(await existence.ValueAsync());
    }

    [Fact]
    public async Task ThrowsWhenCancelled()
    {
        await using IAsyncSession session = _fixture.Session();
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new SchemaExistence(session).ValueAsync(new CancellationToken(true))
        );
    }

    [Fact]
    public async Task ThrowsWhenCancelledOverStore()
    {
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new SchemaExistenceOf(_fixture.Store).ValueAsync(new CancellationToken(true))
        );
    }

    private async Task<bool> ValueAsync()
    {
        await using IAsyncSession session = _fixture.Session();
        return await new SchemaExistence(session).ValueAsync();
    }
}
