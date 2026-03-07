using UsersManagement.Models;

namespace UsersManagement.Services;

interface IUserRepository
{
    IEnumerable<User> GetAll();
    User? GetById(Guid id);
    User Create(User user);
    bool Update(Guid id, User updated);
    bool Delete(Guid id);
}
