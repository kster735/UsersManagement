using UsersManagement.Models;

namespace UsersManagement.Services;

class UserInMemoryRepository : IUserRepository
{
    private readonly ILogger<UserInMemoryRepository> _logger;

    public UserInMemoryRepository(ILogger<UserInMemoryRepository> logger)
    {
        _logger = logger;
    }

    private readonly List<User> _users = new()
    {
        new User { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Password = "password123" },
        new User { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", Password = "password456" }
    };

    public User Create(User user)
    {
        var existingUser = _users.FirstOrDefault(u => u.Email == user.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Attempt to create user with existing email: {Email}", user.Email);
            throw new InvalidOperationException("Email already exists.");
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Password = user.Password
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
        user.Password = updated.Password;
        _logger.LogInformation("User updated successfully: {UserId}", user.Id);
        return true;
    }
}