using ClarityDQ.Core.Entities;

namespace ClarityDQ.Core.Interfaces;

public interface ILineageService
{
    Task<Guid> CreateNodeAsync(LineageNode node, CancellationToken cancellationToken = default);
    Task<Guid> CreateEdgeAsync(LineageEdge edge, CancellationToken cancellationToken = default);
    Task<LineageGraph> GetLineageGraphAsync(string workspaceId, string? datasetName = null, CancellationToken cancellationToken = default);
    Task<LineageGraph> GetUpstreamLineageAsync(Guid nodeId, int depth = 10, CancellationToken cancellationToken = default);
    Task<LineageGraph> GetDownstreamLineageAsync(Guid nodeId, int depth = 10, CancellationToken cancellationToken = default);
    Task<LineageNode?> GetNodeAsync(Guid nodeId, CancellationToken cancellationToken = default);
    Task<List<LineageNode>> GetNodesAsync(string workspaceId, LineageNodeType? nodeType = null, CancellationToken cancellationToken = default);
    Task DeleteNodeAsync(Guid nodeId, CancellationToken cancellationToken = default);
}
