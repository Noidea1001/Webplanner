using Microsoft.AspNetCore.Identity;

namespace WebPlanner.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
