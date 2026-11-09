using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Existence;

/// <summary>
/// Tells whether the schema of the PureQL query graphs exists in a store, in a session
/// of its own.
/// </summary>
/// <param name="Store">The database holding the query graphs.</param>
/// <remarks>
/// Opens a session on <see cref="INeo4jStoredQueries.Driver"/> against
/// <see cref="INeo4jStoredQueries.Database"/> and runs
/// <see cref="SchemaExistence"/> in auto-commit transactions. Every call reads the
/// database again.
/// </remarks>
public sealed record SchemaExistenceOf(INeo4jStoredQueries Store)
{
    /// <summary>
    /// Reads whether the constraint and the indexes named in <see cref="SchemaNames"/>
    /// all exist.
    /// </summary>
    /// <param name="ct">A token to cancel reading the results.</param>
    /// <returns>
    /// <see langword="true"/> when all four exist, otherwise <see langword="false"/>.
    /// </returns>
    public async Task<bool> ValueAsync(CancellationToken ct = default)
    {
        await using IAsyncSession session = Store.Driver.AsyncSession(builder =>
            builder.WithDatabase(Store.Database)
        );
        return await new SchemaExistence(session).ValueAsync(ct);
    }
}
