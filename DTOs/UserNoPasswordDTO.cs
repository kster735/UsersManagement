namespace UsersManagement.DTOs;

public record UserNoPasswordDTO(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Token,
    DateTime? ExpiresAt
);