using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Existence.Tests;

/// <summary>
/// Every sample query stored side by side, under the schema Creation creates: each one
/// is found by its own id, whatever the shape of its graph, and an id stored by none of
/// them is not.
/// </summary>
[Collection(Neo4jServer.Name)]
public sealed record EverySampleTests : IAsyncLifetime
{
    private readonly Neo4jFixture _fixture;

    public EverySampleTests(Neo4jFixture fixture)
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
    public void EverySampleIsLoaded()
    {
        Assert.Equal(386, QuerySamples.All.Count);
    }

    [Fact]
    public async Task FindsEverySampleAmongTheOthers()
    {
        await _fixture.CreateSchemaAsync();
        Dictionary<string, Guid> ids = QuerySamples.All.Keys.ToDictionary(
            name => name,
            _ => Guid.NewGuid()
        );
        await _fixture.InsertAsync(
            ids.Select(sample =>
                QueryGraphs.Of(QuerySamples.All[sample.Key], sample.Value)
            )
        );

        List<string> missing = [];
        await using IAsyncSession session = _fixture.Session();
        foreach ((string name, Guid id) in ids)
        {
            if (!await new QueryExistence(session, id).ValueAsync())
            {
                missing.Add(name);
            }
        }

        Assert.Empty(missing);
        Assert.False(await new QueryExistence(session, Guid.NewGuid()).ValueAsync());
        Assert.Equal(ids.Count, await QueryCountAsync(session));
    }

    private static async Task<long> QueryCountAsync(IAsyncSession session)
    {
        IResultCursor cursor = await session.RunAsync(
            $"MATCH (q:{GraphLabels.Query}) RETURN count(q) AS queries"
        );
        IRecord record = await cursor.SingleAsync();
        return (long)record["queries"];
    }
}
