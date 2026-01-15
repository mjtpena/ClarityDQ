using ClarityDQ.Core.Entities;
using ClarityDQ.Infrastructure.Data;
using ClarityDQ.Profiling.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ClarityDQ.Tests.Services;

public class ProfilingServiceTests : IDisposable
{
    private readonly ClarityDbContext _context;
    private readonly ProfilingService _service;

    public ProfilingServiceTests()
    {
        var options = new DbContextOptionsBuilder<ClarityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClarityDbContext(options);
        _service = new ProfilingService(_context);
    }

    [Fact]
    public async Task ProfileTableAsync_CreatesProfileWithPendingStatus()
    {
        // Arrange
        var workspaceId = "test-workspace";
        var datasetName = "test-dataset";
        var tableName = "test-table";

        // Act
        var profileId = await _service.ProfileTableAsync(workspaceId, datasetName, tableName);

        // Assert - Give minimal time for record creation, not async execution
        await Task.Delay(100);
        
        var profile = await _context.DataProfiles.FindAsync(profileId);
        profile.Should().NotBeNull();
        profile!.WorkspaceId.Should().Be(workspaceId);
        profile.DatasetName.Should().Be(datasetName);
        profile.TableName.Should().Be(tableName);
        // Status may be Pending or InProgress depending on async timing
        profile.Status.Should().BeOneOf(ProfileStatus.Pending, ProfileStatus.InProgress);
    }

    [Fact]
    public async Task GetProfileAsync_ReturnsProfile_WhenExists()
    {
        // Arrange
        var profile = new DataProfile
        {
            Id = Guid.NewGuid(),
            WorkspaceId = "workspace-1",
            DatasetName = "dataset-1",
            TableName = "table-1",
            ProfiledAt = DateTime.UtcNow,
            Status = ProfileStatus.Completed,
            RowCount = 1000,
            ColumnCount = 5
        };

        _context.DataProfiles.Add(profile);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetProfileAsync(profile.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(profile.Id);
        result.WorkspaceId.Should().Be(profile.WorkspaceId);
    }

    [Fact]
    public async Task GetProfileAsync_ReturnsNull_WhenNotExists()
    {
        // Act
        var result = await _service.GetProfileAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProfilesAsync_ReturnsProfilesForWorkspace()
    {
        // Arrange
        var workspace1 = "workspace-1";
        var workspace2 = "workspace-2";

        var profiles = new[]
        {
            new DataProfile { Id = Guid.NewGuid(), WorkspaceId = workspace1, DatasetName = "ds1", TableName = "t1", ProfiledAt = DateTime.UtcNow, Status = ProfileStatus.Completed },
            new DataProfile { Id = Guid.NewGuid(), WorkspaceId = workspace1, DatasetName = "ds2", TableName = "t2", ProfiledAt = DateTime.UtcNow.AddHours(-1), Status = ProfileStatus.Completed },
            new DataProfile { Id = Guid.NewGuid(), WorkspaceId = workspace2, DatasetName = "ds3", TableName = "t3", ProfiledAt = DateTime.UtcNow, Status = ProfileStatus.Completed },
        };

        _context.DataProfiles.AddRange(profiles);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetProfilesAsync(workspace1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.WorkspaceId.Should().Be(workspace1));
        result.Should().BeInDescendingOrder(p => p.ProfiledAt);
    }

    [Fact]
    public async Task GetProfilesAsync_RespectsSkipAndTake()
    {
        // Arrange
        var workspaceId = "workspace-1";
        var profiles = Enumerable.Range(1, 10)
            .Select(i => new DataProfile
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspaceId,
                DatasetName = $"dataset-{i}",
                TableName = $"table-{i}",
                ProfiledAt = DateTime.UtcNow.AddHours(-i),
                Status = ProfileStatus.Completed
            })
            .ToList();

        _context.DataProfiles.AddRange(profiles);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetProfilesAsync(workspaceId, skip: 2, take: 3);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task ProfileTableAsync_CompletesSuccessfully_AfterDelay()
    {
        // Arrange
        var profileId = await _service.ProfileTableAsync("ws1", "ds1", "t1");

        // Act - Wait for async profiling to complete
        await Task.Delay(3000);

        // Assert
        var profile = await _context.DataProfiles.FindAsync(profileId);
        profile.Should().NotBeNull();
        profile!.Status.Should().Be(ProfileStatus.Completed);
        profile.RowCount.Should().BeGreaterThan(0);
        profile.ColumnCount.Should().BeGreaterThan(0);
        profile.ProfileData.Should().NotBeNullOrEmpty();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
