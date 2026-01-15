namespace ClarityDQ.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public string EntraIdObjectId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public enum UserRole
{
    Viewer,
    Contributor,
    Admin
}
