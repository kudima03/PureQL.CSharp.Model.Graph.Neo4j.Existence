using Neo4j.Driver;
using PureQL.CSharp.Model.Samples.Queries.Joins;

namespace PureQL.CSharp.Model.Graph.Neo4j.Existence.Tests;

[Collection(Neo4jServer.Name)]
public sealed record QueryExistenceTests : IAsyncLifetime
{
    private static readonly Guid Id = new Guid("0f8e6a4c-2b1d-4e3f-9a5b-7c6d8e9f0a1b");

    private readonly Neo4jFixture _fixture;

    public QueryExistenceTests(Neo4jFixture fixture)
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
        Assert.False(await ValueAsync(Id));
    }

    [Fact]
    public async Task IsTrueForStoredQuery()
    {
        await SeedAsync();
        Assert.True(await ValueAsync(Id));
    }

    [Fact]
    public async Task IsFalseForAnotherId()
    {
        await SeedAsync();
        Assert.False(await ValueAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task IsTrueWithSchemaCreated()
    {
        await _fixture.CreateSchemaAsync();
        await SeedAsync();
        Assert.True(await ValueAsync(Id));
    }

    [Fact]
    public async Task IsTrueInsideTransaction()
    {
        await SeedAsync();
        await using IAsyncSession session = _fixture.Session();
        Assert.True(
            await session.ExecuteReadAsync(transaction =>
                new QueryExistence(transaction, Id).ValueAsync()
            )
        );
    }

    [Fact]
    public async Task FollowsInsertionOverStore()
    {
        QueryExistenceOf existence = new QueryExistenceOf(_fixture.Store, Id);
        Assert.False(await existence.ValueAsync());
        await SeedAsync();
        Assert.True(await existence.ValueAsync());
        Assert.False(
            await new QueryExistenceOf(_fixture.Store, Guid.NewGuid()).ValueAsync()
        );
    }

    [Fact]
    public async Task ThrowsWhenCancelled()
    {
        await using IAsyncSession session = _fixture.Session();
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new QueryExistence(session, Id).ValueAsync(new CancellationToken(true))
        );
    }

    [Fact]
    public async Task ThrowsWhenCancelledOverStore()
    {
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new QueryExistenceOf(_fixture.Store, Id).ValueAsync(
                new CancellationToken(true)
            )
        );
    }

    private async Task SeedAsync()
    {
        await _fixture.InsertAsync(QueryGraphs.Of(new InnerJoinQuery().Value, Id));
    }

    private async Task<bool> ValueAsync(Guid id)
    {
        await using IAsyncSession session = _fixture.Session();
        return await new QueryExistence(session, id).ValueAsync();
    }
}
