using Neo4j.Driver;
using PureQL.CSharp.Model.Graph.Neo4j.Abstractions;

namespace PureQL.CSharp.Model.Graph.Neo4j.Existence;

/// <summary>
/// Tells whether a query with the given id is stored in a store, in a session of its
/// own.
/// </summary>
/// <param name="Store">The database holding the query graphs.</param>
/// <param name="Id">The id of the stored query.</param>
/// <remarks>
/// Opens a session on <see cref="INeo4jStoredQueries.Driver"/> against
/// <see cref="INeo4jStoredQueries.Database"/> and runs <see cref="QueryExistence"/>
/// in an auto-commit transaction. Every call reads the database again.
/// </remarks>
public sealed record QueryExistenceOf(INeo4jStoredQueries Store, Guid Id)
{
    /// <summary>Reads whether the query is stored.</summary>
    /// <param name="ct">A token to cancel reading the result.</param>
    /// <returns>
    /// <see langword="true"/> when a query with <see cref="Id"/> is stored, otherwise
    /// <see langword="false"/>.
    /// </returns>
    public async Task<bool> ValueAsync(CancellationToken ct = default)
    {
        await using IAsyncSession session = Store.Driver.AsyncSession(builder =>
            builder.WithDatabase(Store.Database)
        );
        return await new QueryExistence(session, Id).ValueAsync(ct);
    }
}
