using System.ComponentModel.DataAnnotations;

namespace UsersManagement.Models;

class User
{
    public Guid Id { get; set; }

    [Required]
    public string? FirstName { get; set; }
    [Required]
    public string? LastName { get; set; }
    [Required]
    public string? Email { get; set; }
    [Required]
    [MinLength(6)]
    public string? Password { get; set; }
}