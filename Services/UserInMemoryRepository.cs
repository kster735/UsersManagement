using UsersManagement.Models;

namespace UsersManagement.Services;

class UserInMemoryRepository : IUserRepository
{
    private readonly List<User> _users = new()
    {
        new User { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Password = "password123" },
        new User { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", Password = "password456" }
    };

    public User Create(User user)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Password = user.Password
        };
    }

    public bool Delete(Guid id)
    {
        return _users.RemoveAll(u => u.Id == id) > 0;
    }

    public IEnumerable<User> GetAll() => _users;

    public User? GetById(Guid id) => _users.FirstOrDefault(u => u.Id == id);

    public bool Update(Guid id, User updated)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null) return false;

        user.FirstName = updated.FirstName;
        user.LastName = updated.LastName;
        user.Email = updated.Email;
        user.Password = updated.Password;
        return true;
    }
}