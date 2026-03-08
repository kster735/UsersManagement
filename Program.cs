using UsersManagement.Models;
using UsersManagement.Services;
using UsersManagement.Utils;

using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromFile("NLog.config").GetCurrentClassLogger();

try
{

    logger.Info("Starting application");

    var builder = WebApplication.CreateBuilder(args);

    // Clear default logging providers
    builder.Logging.ClearProviders();
    // Configure NLog
    builder.Host.UseNLog();

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    builder.Services.AddSingleton<IUserRepository, UserInMemoryRepository>();


    var app = builder.Build();


    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();


    app.UseMiddleware<AuthMiddleware>();

    app.MapGet("/", () => "User API is running");


    // CREATE user
    app.MapPost("/auth/register", IResult (User user, IUserRepository userInMemoryRepository) =>
    {
        var errors = user.Validate();
        if (errors.Count > 0)
            return TypedResults.BadRequest(errors);
        try
        {
            var created = userInMemoryRepository.Create(user);
            return TypedResults.Created($"/users/{created.Id}", created);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    });

    app.MapPost("/auth/login", IResult (User user, IUserRepository userInMemoryRepository) =>
    {
        var existingUser = userInMemoryRepository.GetAll().FirstOrDefault(u => u.Email == user.Email && u.Password == user.Password);
        if (existingUser == null)
            return TypedResults.Unauthorized();

        // Generate a simple token (for demonstration purposes only)
        existingUser.Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        existingUser.ExpiresAt = DateTime.UtcNow.AddHours(1);
        return TypedResults.Ok(existingUser.WithoutPassword());
    });


    app.MapPost("/auth/logout", IResult (HttpContext context, IUserRepository userInMemoryRepository) =>
    {
        if (context.Items.TryGetValue("User", out var userObj) && userObj is User user)
        {
            user.Token = null;
            user.ExpiresAt = null;
            return TypedResults.Ok("Logged out successfully.");
        }
        return TypedResults.Unauthorized();
    });


    app.MapGet("/auth/me", IResult (HttpContext context) =>
    {
        if (context.Items.TryGetValue("User", out var userObj) && userObj is User user)
        {
            return TypedResults.Ok(user.WithoutPassword());
        }
        return TypedResults.Unauthorized();
    });


    // GET all users
    app.MapGet("/users", (IUserRepository userInMemoryRepository) =>
    {
        return TypedResults.Ok(userInMemoryRepository.GetAll().Select(u => u.WithoutTokensOrPassword()));
    });

    // GET user by id
    app.MapGet("/users/{id:Guid}", IResult (Guid id, IUserRepository userInMemoryRepository) =>
    {
        var user = userInMemoryRepository.GetById(id);
        return user is not null ? TypedResults.Ok(user.WithoutTokensOrPassword()) : TypedResults.NotFound();
    });



    // UPDATE user
    app.MapPut("/users/{id:Guid}", IResult (
            Guid id,
            User updated,
            HttpContext context,
            IUserRepository userInMemoryRepository) =>
    {
        if (context.Items["User"] is not User currentUser || currentUser.Id != id)
        {
            return TypedResults.Unauthorized();
        }
        var errors = updated.Validate();
        if (errors.Count > 0)
            return TypedResults.BadRequest(errors);
        var ok = userInMemoryRepository.Update(id, updated);
        return ok ? TypedResults.NoContent() : TypedResults.NotFound();
    });

    // DELETE user
    app.MapDelete("/users/{id:Guid}", IResult (
        Guid id,
        HttpContext context,
        IUserRepository userInMemoryRepository) =>
    {
        if (context.Items["User"] is not User currentUser || currentUser.Id != id)
        {
            return TypedResults.Unauthorized();
        }
        var ok = userInMemoryRepository.Delete(id);
        return ok ? TypedResults.NoContent() : TypedResults.NotFound();
    });

    app.Run();

}
catch (Exception ex)
{
    logger.Error(ex, "Application stopped due to exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}



