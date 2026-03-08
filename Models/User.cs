using System.ComponentModel.DataAnnotations;

namespace UsersManagement.Models;

class User
{
    public Guid Id { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    [MinLength(6)]
    public string? Password { get; set; }

    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
}