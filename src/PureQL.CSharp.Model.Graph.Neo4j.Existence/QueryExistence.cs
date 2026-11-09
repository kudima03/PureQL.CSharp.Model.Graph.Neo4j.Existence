using Neo4j.Driver;

namespace PureQL.CSharp.Model.Graph.Neo4j.Existence;

/// <summary>Tells whether a query with the given id is stored.</summary>
/// <param name="Runner">
/// The session or transaction the statement runs on, owned by the caller.
/// </param>
/// <param name="Id">The id of the stored query.</param>
/// <remarks>
/// Looks for the root node of the query: a <see cref="GraphLabels.Query"/> node whose
/// <see cref="GraphProperties.Id"/> is <paramref name="Id"/> in "D" format. Every call
/// reads the database again.
/// </remarks>
public sealed record QueryExistence(IAsyncQueryRunner Runner, Guid Id)
{
    private const string IdParameter = "id";

    private const string Found = "found";

    private const string Statement =
        $"MATCH (q:{GraphLabels.Query} {{{GraphProperties.Id}: ${IdParameter}}}) "
        + $"RETURN count(q) > 0 AS {Found}";

    /// <summary>Reads whether the query is stored.</summary>
    /// <param name="ct">A token to cancel reading the result.</param>
    /// <returns>
    /// <see langword="true"/> when a query with <see cref="Id"/> is stored, otherwise
    /// <see langword="false"/>.
    /// </returns>
    public async Task<bool> ValueAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        IResultCursor cursor = await Runner.RunAsync(
            Statement,
            new Dictionary<string, object> { [IdParameter] = Id.ToString("D") }
        );
        return await cursor.SingleAsync(record => (bool)record[Found], ct);
    }
}
