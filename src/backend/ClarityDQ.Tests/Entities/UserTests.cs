using ClarityDQ.Core.Entities;
using FluentAssertions;

namespace ClarityDQ.Tests.Entities;

public class UserTests
{
    [Fact]
    public void User_CanBeCreated()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraIdObjectId = "oid-123",
            Email = "test@example.com",
            DisplayName = "Test User",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        user.Should().NotBeNull();
        user.EntraIdObjectId.Should().Be("oid-123");
    }

    [Fact]
    public void User_AllRolesAreValid()
    {
        var viewer = new User { Role = UserRole.Viewer };
        var contributor = new User { Role = UserRole.Contributor };
        var admin = new User { Role = UserRole.Admin };

        viewer.Role.Should().Be(UserRole.Viewer);
        contributor.Role.Should().Be(UserRole.Contributor);
        admin.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void User_LastLoginCanBeNull()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraIdObjectId = "oid",
            Email = "test@example.com",
            DisplayName = "Test",
            Role = UserRole.Viewer,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = null
        };

        user.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public void User_LastLoginCanBeSet()
    {
        var loginTime = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraIdObjectId = "oid",
            Email = "test@example.com",
            DisplayName = "Test",
            Role = UserRole.Viewer,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = loginTime
        };

        user.LastLoginAt.Should().Be(loginTime);
    }

    [Fact]
    public void UserRole_EnumValuesAreCorrect()
    {
        ((int)UserRole.Viewer).Should().Be(0);
        ((int)UserRole.Contributor).Should().Be(1);
        ((int)UserRole.Admin).Should().Be(2);
    }
}
