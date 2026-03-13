namespace UsersManagement.DTOs;

public record UserNoPasswordNoTokenDTO(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Email
);