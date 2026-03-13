using UsersManagement.Models;
using UsersManagement.Services;
using UsersManagement.Utils;

using NLog;
using NLog.Web;
using Scalar.AspNetCore;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Http.HttpResults;
using UsersManagement.DTOs;
using UsersManagement.Requests;

var logger = LogManager.Setup().LoadConfigurationFromFile("NLog.config").GetCurrentClassLogger();

try
{

    logger.Info("Starting application");

    var builder = WebApplication.CreateBuilder(args);

    // Clear default logging providers
    builder.Logging.ClearProviders();
    // Configure NLog
    builder.Host.UseNLog();

    // builder.Services.AddEndpointsApiExplorer();
    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

    builder.Services.AddOpenApi();


    builder.Services.AddSingleton<IUserRepository, UserInMemoryRepository>();


    var app = builder.Build();


    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseHttpsRedirection();

    app.UseMiddleware<AuthMiddleware>();

    app.MapGet("/", Ok<ResponseMessage> () => TypedResults.Ok(new ResponseMessage("User API is running")));

    // CREATE user
    app.MapPost(
        "/auth/register",
        Results<Created<UserNoPasswordDTO>,
        BadRequest<List<string>>,
        BadRequest<ResponseMessage>> (SignupRequest user, IUserRepository userInMemoryRepository) =>
    {
        var errors = user.Validate();
        if (errors.Count > 0)
            return TypedResults.BadRequest(errors);
        try
        {
            User created = userInMemoryRepository.Create(new User
            {
                Email = user.Email,
                Password = user.Password
            });
            return TypedResults.Created<UserNoPasswordDTO>($"/user/{created.Id}", created.WithoutPassword());
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(new ResponseMessage(ex.Message));
        }
    });

    app.MapPost(
        "/auth/login",
        Results<Ok<UserNoPasswordDTO>, UnauthorizedHttpResult, JsonHttpResult<ResponseMessage>> (SignInRequest user, IUserRepository userInMemoryRepository) =>
    {
        var existingUser = userInMemoryRepository.GetAll().FirstOrDefault(
                u => u.Email == user.Email
                && HashingPasswords.VerifyPasswordWithSalt(user.Password!, u.Password!, u.Salt!)
            );
        if (existingUser == null)
            return TypedResults.Json(new ResponseMessage("Invalid email or password"), statusCode: 401);

        // Generate a simple token (for demonstration purposes only)
        existingUser.Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        existingUser.ExpiresAt = DateTime.UtcNow.AddHours(1);
        return TypedResults.Ok(existingUser.WithoutPassword());
    });



    app.MapPost(
        "/auth/logout",
        Results<Ok<ResponseMessage>, UnauthorizedHttpResult, JsonHttpResult<ResponseMessage>> (HttpContext context, IUserRepository userInMemoryRepository) =>
    {
        if (context.Items.TryGetValue("User", out var userObj) && userObj is User user)
        {
            user.Token = null;
            user.ExpiresAt = null;
            return TypedResults.Ok(new ResponseMessage("Logged out successfully."));
        }
        return TypedResults.Json(new ResponseMessage("Unauthorized"), statusCode: 401);
    });


    app.MapGet("/auth/me", Results<Ok<UserNoPasswordDTO>, UnauthorizedHttpResult, JsonHttpResult<ResponseMessage>> (HttpContext context) =>
    {
        if (context.Items.TryGetValue("User", out var userObj) && userObj is User user)
        {
            return TypedResults.Ok(user.WithoutPassword());
        }
        return TypedResults.Json(new ResponseMessage("You are not authenticated."), statusCode: 401);
    });


    // GET all users
    app.MapGet("/users", Results<Ok<IEnumerable<UserNoPasswordNoTokenDTO>>, BadRequest<ResponseMessage>> (IUserRepository userInMemoryRepository) =>
    {
        return TypedResults.Ok(userInMemoryRepository.GetAll().Select(u => u.WithoutTokensOrPassword()));
    });

    // GET user by id
    app.MapGet("/users/{id:Guid}", Results<Ok<UserNoPasswordNoTokenDTO>, NotFound<ResponseMessage>> (Guid id, IUserRepository userInMemoryRepository) =>
    {
        var user = userInMemoryRepository.GetById(id);
        return user is not null ? TypedResults.Ok(user.WithoutTokensOrPassword()) : TypedResults.NotFound(new ResponseMessage("User not found."));
    });



    // UPDATE user
    app.MapPut("/users/{id:Guid}", Results<NoContent, BadRequest<List<string>>, NotFound, UnauthorizedHttpResult> (
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
    app.MapDelete("/users/{id:Guid}", Results<UnauthorizedHttpResult, NoContent, NotFound> (
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


readonly record struct ResponseMessage(string Message);