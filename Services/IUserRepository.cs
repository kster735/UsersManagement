using UsersManagement.Models;

namespace MinimalUsersApi.Services;

public interface IUserRepository
{
    IEnumerable<User> GetAll();
    User? GetById(int id);
    User Create(User user);
    bool Update(int id, User updated);
    bool Delete(int id);
}
