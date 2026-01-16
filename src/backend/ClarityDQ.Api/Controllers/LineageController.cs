using ClarityDQ.Core.Entities;
using ClarityDQ.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClarityDQ.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LineageController : ControllerBase
{
    private readonly ILineageService _lineageService;
    private readonly ILogger<LineageController> _logger;

    public LineageController(ILineageService lineageService, ILogger<LineageController> logger)
    {
        _lineageService = lineageService;
        _logger = logger;
    }

    [HttpGet("workspace/{workspaceId}")]
    [ProducesResponseType(typeof(LineageGraph), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkspaceLineage(string workspaceId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workspaceId))
        {
            return BadRequest("Workspace ID is required");
        }

        try
        {
            var graph = await _lineageService.GetLineageGraphAsync(workspaceId, null, cancellationToken);
            return Ok(graph);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lineage for workspace {WorkspaceId}", workspaceId);
            return StatusCode(500, "Error retrieving workspace lineage");
        }
    }

    [HttpGet("table/{workspaceId}/{datasetName}/{tableName}")]
    [ProducesResponseType(typeof(LineageGraph), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTableLineage(
        string workspaceId,
        string datasetName,
        string tableName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workspaceId))
        {
            return BadRequest("Workspace ID is required");
        }

        if (string.IsNullOrWhiteSpace(datasetName))
        {
            return BadRequest("Dataset name is required");
        }

        if (string.IsNullOrWhiteSpace(tableName))
        {
            return BadRequest("Table name is required");
        }

        try
        {
            var graph = await _lineageService.GetLineageGraphAsync(workspaceId, datasetName, cancellationToken);
            return Ok(graph);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lineage for table {TableName}", tableName);
            return StatusCode(500, "Error retrieving table lineage");
        }
    }

    [HttpPost("node")]
    [ProducesResponseType(typeof(LineageNode), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddNode([FromBody] CreateLineageNodeRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.WorkspaceId))
        {
            return BadRequest("Workspace ID is required");
        }

        if (string.IsNullOrWhiteSpace(request.NodeName))
        {
            return BadRequest("Node name is required");
        }

        try
        {
            var node = new LineageNode
            {
                Id = Guid.NewGuid(),
                WorkspaceId = request.WorkspaceId,
                NodeName = request.NodeName,
                NodeType = request.NodeType,
                DatasetName = request.DatasetName,
                TableName = request.TableName,
                Description = request.Description,
                DiscoveredAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? "system"
            };

            var nodeId = await _lineageService.CreateNodeAsync(node, cancellationToken);
            node.Id = nodeId;

            return CreatedAtAction(nameof(GetWorkspaceLineage), new { workspaceId = request.WorkspaceId }, node);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding lineage node");
            return StatusCode(500, "Error adding lineage node");
        }
    }

    [HttpPost("edge")]
    [ProducesResponseType(typeof(LineageEdge), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddEdge([FromBody] CreateLineageEdgeRequest request, CancellationToken cancellationToken)
    {
        if (request.SourceNodeId == Guid.Empty)
        {
            return BadRequest("Source node ID is required");
        }

        if (request.TargetNodeId == Guid.Empty)
        {
            return BadRequest("Target node ID is required");
        }

        try
        {
            var edge = new LineageEdge
            {
                Id = Guid.NewGuid(),
                SourceNodeId = request.SourceNodeId,
                TargetNodeId = request.TargetNodeId,
                TransformationType = request.TransformationType,
                TransformationLogic = request.TransformationLogic,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? "system"
            };

            var edgeId = await _lineageService.CreateEdgeAsync(edge, cancellationToken);
            edge.Id = edgeId;

            return CreatedAtAction(nameof(GetWorkspaceLineage), new { workspaceId = "unknown" }, edge);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding lineage edge");
            return StatusCode(500, "Error adding lineage edge");
        }
    }
}

public record CreateLineageNodeRequest(
    string WorkspaceId,
    string NodeName,
    LineageNodeType NodeType,
    string? DatasetName,
    string? TableName,
    string? Description);

public record CreateLineageEdgeRequest(
    Guid SourceNodeId,
    Guid TargetNodeId,
    string TransformationType,
    string? TransformationLogic);
