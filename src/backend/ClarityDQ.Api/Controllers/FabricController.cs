using ClarityDQ.FabricClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClarityDQ.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FabricController : ControllerBase
{
    private readonly IFabricClient _fabricClient;
    private readonly ILogger<FabricController> _logger;

    public FabricController(IFabricClient fabricClient, ILogger<FabricController> logger)
    {
        _fabricClient = fabricClient;
        _logger = logger;
    }

    [HttpGet("workspaces")]
    [ProducesResponseType(typeof(FabricWorkspace[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkspaces(CancellationToken cancellationToken)
    {
        try
        {
            var workspaces = await _fabricClient.GetWorkspacesAsync(cancellationToken);
            return Ok(workspaces);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workspaces");
            return StatusCode(500, "Error retrieving workspaces");
        }
    }

    [HttpGet("workspaces/{workspaceId}/items")]
    [ProducesResponseType(typeof(FabricItem[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkspaceItems(string workspaceId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workspaceId))
        {
            return BadRequest("Workspace ID is required");
        }

        try
        {
            var items = await _fabricClient.GetWorkspaceItemsAsync(workspaceId, cancellationToken);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving items for workspace {WorkspaceId}", workspaceId);
            return StatusCode(500, "Error retrieving workspace items");
        }
    }

    [HttpGet("workspaces/{workspaceId}/lakehouses/{lakehouseId}/tables/{tableName}/schema")]
    [ProducesResponseType(typeof(FabricTableSchema), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTableSchema(
        string workspaceId,
        string lakehouseId,
        string tableName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workspaceId))
        {
            return BadRequest("Workspace ID is required");
        }

        if (string.IsNullOrWhiteSpace(lakehouseId))
        {
            return BadRequest("Lakehouse ID is required");
        }

        if (string.IsNullOrWhiteSpace(tableName))
        {
            return BadRequest("Table name is required");
        }

        try
        {
            var schema = await _fabricClient.GetTableSchemaAsync(workspaceId, lakehouseId, tableName, cancellationToken);
            return Ok(schema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schema for table {TableName} in lakehouse {LakehouseId}", tableName, lakehouseId);
            return StatusCode(500, "Error retrieving table schema");
        }
    }

    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", service = "fabric-integration" });
    }
}
