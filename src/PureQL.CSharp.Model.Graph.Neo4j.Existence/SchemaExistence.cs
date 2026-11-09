using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Existence;

/// <summary>
/// Tells whether the schema of the PureQL query graphs exists: the constraint and the
/// indexes named in <see cref="SchemaNames"/>.
/// </summary>
/// <param name="Runner">
/// The session or transaction the statements run on, owned by the caller.
/// </param>
/// <remarks>
/// Runs <c>SHOW CONSTRAINTS</c> and <c>SHOW INDEXES</c> and matches on their
/// <c>name</c> column only: the kind and the properties of what is found are not
/// checked. An equivalent index under another name does not count, although it makes
/// Creation's <c>CREATE INDEX IF NOT EXISTS</c> a no-op. Every call reads the database
/// again.
/// </remarks>
public sealed record SchemaExistence(IAsyncQueryRunner Runner)
{
    private const string Name = "name";

    private const string Constraints = $"SHOW CONSTRAINTS YIELD {Name} RETURN {Name}";

    private const string Indexes = $"SHOW INDEXES YIELD {Name} RETURN {Name}";

    /// <summary>
    /// Reads whether the constraint <see cref="SchemaNames.QueryId"/> and the indexes
    /// <see cref="SchemaNames.NodeQueryId"/>, <see cref="SchemaNames.NodeFieldSource"/>
    /// and <see cref="SchemaNames.NodeParam"/> all exist.
    /// </summary>
    /// <param name="ct">A token to cancel reading the results.</param>
    /// <returns>
    /// <see langword="true"/> when all four exist, otherwise <see langword="false"/>.
    /// </returns>
    public async Task<bool> ValueAsync(CancellationToken ct = default)
    {
        IReadOnlySet<string> constraints = await NamesAsync(Constraints, ct);
        if (!constraints.Contains(SchemaNames.QueryId))
        {
            return false;
        }

        IReadOnlySet<string> indexes = await NamesAsync(Indexes, ct);
        return SchemaNames.Indexes.All(indexes.Contains);
    }

    private async Task<IReadOnlySet<string>> NamesAsync(
        string statement,
        CancellationToken ct
    )
    {
        ct.ThrowIfCancellationRequested();
        IResultCursor cursor = await Runner.RunAsync(statement);
        List<string> names = await cursor.ToListAsync(
            record => (string)record[Name],
            cancellationToken: ct
        );
        return new HashSet<string>(names, StringComparer.Ordinal);
    }
}
