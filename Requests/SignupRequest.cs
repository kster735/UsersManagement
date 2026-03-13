namespace UsersManagement.Requests;

using System.ComponentModel.DataAnnotations;

public class SignupRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [MinLength(6)]
    public required string Password { get; init; }
}
