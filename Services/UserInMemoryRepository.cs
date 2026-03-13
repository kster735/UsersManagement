using NLog.LayoutRenderers;
using UsersManagement.Models;
using UsersManagement.Utils;

namespace UsersManagement.Services;

class UserInMemoryRepository : IUserRepository
{
    private readonly ILogger<UserInMemoryRepository> _logger;
    private static readonly byte[] _salt1 = HashingPasswords.GenerateSalt();
    private static readonly byte[] _salt2 = HashingPasswords.GenerateSalt();
    public UserInMemoryRepository(ILogger<UserInMemoryRepository> logger)
    {
        _logger = logger;
    }

    private readonly List<User> _users = new()
    {
        new User{
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Salt = _salt1,
            Password = HashingPasswords.HashPasswordWithSalt("password123", _salt1)
        },
        new User {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Salt = _salt2,
            Password = HashingPasswords.HashPasswordWithSalt("password456", _salt2)
        }
    };


    public User Create(User user)
    {

        var existingUser = _users.FirstOrDefault(u => u.Email == user.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Attempt to create user with existing email: {Email}", user.Email);
            throw new InvalidOperationException("Email already exists.");
        }

        user.Salt = HashingPasswords.GenerateSalt();
        user.Password = HashingPasswords.HashPasswordWithSalt(user.Password!, user.Salt!);

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Password = user.Password,
            Salt = user.Salt
        };

        _users.Add(newUser);
        _logger.LogInformation("User created successfully: {UserId}", newUser.Id);
        return newUser;
    }

    public bool Delete(Guid id)
    {
        _logger.LogInformation("Attempting to delete user with ID: {UserId}", id);
        return _users.RemoveAll(u => u.Id == id) > 0;
    }

    public IEnumerable<User> GetAll() => _users;

    public User? GetById(Guid id) => _users.FirstOrDefault(u => u.Id == id);

    public bool Update(Guid id, User updated)
    {
        _logger.LogInformation("Attempting to update user with ID: {UserId}", id);
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null) return false;

        user.FirstName = updated.FirstName;
        user.LastName = updated.LastName;
        user.Email = updated.Email;

        var hashedPassword = HashingPasswords.HashPasswordWithSalt(updated.Password!, user.Salt!);

        user.Password = hashedPassword;
        _logger.LogInformation("User updated successfully: {UserId}", user.Id);
        return true;
    }
}