using System.ComponentModel.DataAnnotations;

namespace UsersManagement.Models;

public class User
{
    public Guid Id { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    [MinLength(6)]
    public string? Password { get; set; } // this is the hashed password. In a real application, consider using a more secure approach like ASP.NET Identity or similar libraries.

    public byte[]? Salt { get; set; } // For demonstration purposes, we store the salt with the user. In a real application, consider a more secure approach like storing in the Db.

    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
}