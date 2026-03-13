namespace UsersManagement.Requests;

using System.ComponentModel.DataAnnotations;

public class SignInRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}
