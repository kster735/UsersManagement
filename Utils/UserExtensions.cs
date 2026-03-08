using UsersManagement.Models;

namespace UsersManagement.Utils;

public static class UserExtensions
{
    public static object WithoutPassword(this User user)
    {
        return new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Token,
            user.ExpiresAt
        };
    }

    public static object WithoutTokensOrPassword(this User user)
    {
        return new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName
        };
    }
}