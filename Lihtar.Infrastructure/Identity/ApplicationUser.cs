using Lihtar.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Lihtar.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = default!;
    public UserRole Role { get; set; } = UserRole.Client;

    public bool IsBlocked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
