using ClarityDQ.Core.Entities;
using FluentAssertions;

namespace ClarityDQ.Tests.Entities;

public class DataProfileTests
{
    [Fact]
    public void DataProfile_CanBeCreated()
    {
        var profile = new DataProfile
        {
            Id = Guid.NewGuid(),
            WorkspaceId = "ws-1",
            DatasetName = "dataset-1",
            TableName = "table-1",
            ProfiledAt = DateTime.UtcNow,
            RowCount = 1000,
            ColumnCount = 5,
            SizeInBytes = 50000,
            ProfileData = "{}",
            Status = ProfileStatus.Completed
        };

        profile.Should().NotBeNull();
        profile.WorkspaceId.Should().Be("ws-1");
        profile.Status.Should().Be(ProfileStatus.Completed);
    }

    [Fact]
    public void DataProfile_AllStatusesAreValid()
    {
        var pending = new DataProfile { Status = ProfileStatus.Pending };
        var inProgress = new DataProfile { Status = ProfileStatus.InProgress };
        var completed = new DataProfile { Status = ProfileStatus.Completed };
        var failed = new DataProfile { Status = ProfileStatus.Failed };

        pending.Status.Should().Be(ProfileStatus.Pending);
        inProgress.Status.Should().Be(ProfileStatus.InProgress);
        completed.Status.Should().Be(ProfileStatus.Completed);
        failed.Status.Should().Be(ProfileStatus.Failed);
    }

    [Fact]
    public void DataProfile_ErrorMessageCanBeNull()
    {
        var profile = new DataProfile
        {
            Id = Guid.NewGuid(),
            WorkspaceId = "ws-1",
            DatasetName = "ds-1",
            TableName = "t-1",
            ProfiledAt = DateTime.UtcNow,
            Status = ProfileStatus.Completed,
            ErrorMessage = null
        };

        profile.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void DataProfile_ErrorMessageCanBeSet()
    {
        var profile = new DataProfile
        {
            Id = Guid.NewGuid(),
            WorkspaceId = "ws-1",
            DatasetName = "ds-1",
            TableName = "t-1",
            ProfiledAt = DateTime.UtcNow,
            Status = ProfileStatus.Failed,
            ErrorMessage = "Connection timeout"
        };

        profile.ErrorMessage.Should().Be("Connection timeout");
    }

    [Fact]
    public void DataProfile_CanHaveLargeRowCount()
    {
        var profile = new DataProfile
        {
            Id = Guid.NewGuid(),
            WorkspaceId = "ws-1",
            DatasetName = "ds-1",
            TableName = "t-1",
            ProfiledAt = DateTime.UtcNow,
            RowCount = 1_000_000_000,
            Status = ProfileStatus.Completed
        };

        profile.RowCount.Should().Be(1_000_000_000);
    }

    [Fact]
    public void ProfileStatus_EnumValuesAreCorrect()
    {
        ((int)ProfileStatus.Pending).Should().Be(0);
        ((int)ProfileStatus.InProgress).Should().Be(1);
        ((int)ProfileStatus.Completed).Should().Be(2);
        ((int)ProfileStatus.Failed).Should().Be(3);
    }
}
