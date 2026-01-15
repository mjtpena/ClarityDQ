using ClarityDQ.Api.Controllers;
using ClarityDQ.Core.Entities;
using ClarityDQ.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClarityDQ.Tests.Controllers;

public class ProfilingControllerTests
{
    private readonly Mock<IProfilingService> _mockService;
    private readonly Mock<ILogger<ProfilingController>> _mockLogger;
    private readonly ProfilingController _controller;

    public ProfilingControllerTests()
    {
        _mockService = new Mock<IProfilingService>();
        _mockLogger = new Mock<ILogger<ProfilingController>>();
        _controller = new ProfilingController(_mockService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ProfileTable_ReturnsCreatedResult_WithProfileId()
    {
        // Arrange
        var request = new ProfileRequest("workspace-1", "dataset-1", "table-1");
        var expectedProfileId = Guid.NewGuid();
        _mockService.Setup(s => s.ProfileTableAsync(request.WorkspaceId, request.DatasetName, request.TableName, default))
            .ReturnsAsync(expectedProfileId);

        // Act
        var result = await _controller.ProfileTable(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().Be(expectedProfileId);
        createdResult.ActionName.Should().Be(nameof(ProfilingController.GetProfile));
        createdResult.RouteValues!["id"].Should().Be(expectedProfileId);
    }

    [Fact]
    public async Task GetProfile_ReturnsOk_WhenProfileExists()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var profile = new DataProfile
        {
            Id = profileId,
            WorkspaceId = "workspace-1",
            DatasetName = "dataset-1",
            TableName = "table-1",
            ProfiledAt = DateTime.UtcNow,
            Status = ProfileStatus.Completed
        };

        _mockService.Setup(s => s.GetProfileAsync(profileId, default))
            .ReturnsAsync(profile);

        // Act
        var result = await _controller.GetProfile(profileId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(profile);
    }

    [Fact]
    public async Task GetProfile_ReturnsNotFound_WhenProfileDoesNotExist()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        _mockService.Setup(s => s.GetProfileAsync(profileId, default))
            .ReturnsAsync((DataProfile?)null);

        // Act
        var result = await _controller.GetProfile(profileId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetProfiles_ReturnsOk_WithProfiles()
    {
        // Arrange
        var workspaceId = "workspace-1";
        var profiles = new List<DataProfile>
        {
            new() { Id = Guid.NewGuid(), WorkspaceId = workspaceId, DatasetName = "ds1", TableName = "t1", ProfiledAt = DateTime.UtcNow, Status = ProfileStatus.Completed },
            new() { Id = Guid.NewGuid(), WorkspaceId = workspaceId, DatasetName = "ds2", TableName = "t2", ProfiledAt = DateTime.UtcNow, Status = ProfileStatus.Completed }
        };

        _mockService.Setup(s => s.GetProfilesAsync(workspaceId, 0, 50, default))
            .ReturnsAsync(profiles);

        // Act
        var result = await _controller.GetProfiles(workspaceId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProfiles = okResult.Value.Should().BeAssignableTo<List<DataProfile>>().Subject;
        returnedProfiles.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetProfiles_UsesCustomSkipAndTake()
    {
        // Arrange
        var workspaceId = "workspace-1";
        var skip = 10;
        var take = 20;

        _mockService.Setup(s => s.GetProfilesAsync(workspaceId, skip, take, default))
            .ReturnsAsync(new List<DataProfile>());

        // Act
        await _controller.GetProfiles(workspaceId, skip, take);

        // Assert
        _mockService.Verify(s => s.GetProfilesAsync(workspaceId, skip, take, default), Times.Once);
    }

    [Fact]
    public async Task ProfileTable_LogsInformation()
    {
        // Arrange
        var request = new ProfileRequest("ws1", "ds1", "t1");
        _mockService.Setup(s => s.ProfileTableAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        await _controller.ProfileTable(request);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Profiling request")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
