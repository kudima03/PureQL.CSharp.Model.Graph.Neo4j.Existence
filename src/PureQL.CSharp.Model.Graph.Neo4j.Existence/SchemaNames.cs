namespace PureQL.CSharp.Model.Graph.Neo4j.Existence;

/// <summary>
/// The names of the constraint and the indexes that make up the schema of the PureQL
/// query graphs.
/// </summary>
/// <remarks>
/// <c>PureQL.CSharp.Model.Graph.Neo4j.Creation</c> creates them under these names.
/// This package does not depend on it, so the names are duplicated here and must match
/// the ones Creation uses.
/// </remarks>
public static class SchemaNames
{
    /// <summary>
    /// The uniqueness constraint on the <c>id</c> property of <c>Query</c> nodes.
    /// </summary>
    public const string QueryId = "pureql_query_id";

    /// <summary>The index on the <c>queryId</c> property of <c>Node</c> nodes.</summary>
    public const string NodeQueryId = "pureql_node_query_id";

    /// <summary>
    /// The index on the <c>field</c> and <c>source</c> properties of <c>Node</c> nodes.
    /// </summary>
    public const string NodeFieldSource = "pureql_node_field_source";

    /// <summary>The index on the <c>param</c> property of <c>Node</c> nodes.</summary>
    public const string NodeParam = "pureql_node_param";
}
