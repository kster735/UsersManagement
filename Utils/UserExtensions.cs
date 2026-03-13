using UsersManagement.DTOs;
using UsersManagement.Models;

namespace UsersManagement.Utils;

public static class UserExtensions
{
    public static UserNoPasswordDTO WithoutPassword(this User user)
    {
        return new
        UserNoPasswordDTO(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Token,
            user.ExpiresAt
        );
    }

    public static UserNoPasswordNoTokenDTO WithoutTokensOrPassword(this User user)
    {
        return new UserNoPasswordNoTokenDTO(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email
        );
    }
}