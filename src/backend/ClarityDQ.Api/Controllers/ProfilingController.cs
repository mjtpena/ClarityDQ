using ClarityDQ.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClarityDQ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfilingController : ControllerBase
{
    private readonly IProfilingService _profilingService;
    private readonly ILogger<ProfilingController> _logger;

    public ProfilingController(IProfilingService profilingService, ILogger<ProfilingController> logger)
    {
        _profilingService = profilingService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> ProfileTable([FromBody] ProfileRequest request)
    {
        _logger.LogInformation("Profiling request for {Workspace}/{Dataset}/{Table}", 
            request.WorkspaceId, request.DatasetName, request.TableName);

        var profileId = await _profilingService.ProfileTableAsync(
            request.WorkspaceId, 
            request.DatasetName, 
            request.TableName);

        return CreatedAtAction(nameof(GetProfile), new { id = profileId }, profileId);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Core.Entities.DataProfile>> GetProfile(Guid id)
    {
        var profile = await _profilingService.GetProfileAsync(id);
        if (profile == null)
            return NotFound();

        return Ok(profile);
    }

    [HttpGet("workspace/{workspaceId}")]
    public async Task<ActionResult<List<Core.Entities.DataProfile>>> GetProfiles(
        string workspaceId, 
        [FromQuery] int skip = 0, 
        [FromQuery] int take = 50)
    {
        var profiles = await _profilingService.GetProfilesAsync(workspaceId, skip, take);
        return Ok(profiles);
    }
}

public record ProfileRequest(string WorkspaceId, string DatasetName, string TableName);
