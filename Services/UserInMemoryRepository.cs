using UsersManagement.Models;

namespace UsersManagement.Services;

class UserInMemoryRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public User Create(User user)
    {
        throw new NotImplementedException();
    }

    public bool Delete(Guid id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<User> GetAll() => _users;

    public User? GetById(Guid id) => _users.FirstOrDefault(u => u.Id == id);

    public bool Update(Guid id, User updated)
    {
        throw new NotImplementedException();
    }
}