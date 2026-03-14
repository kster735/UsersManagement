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

    app.MapGet("/", Ok<ResponseMessage> () => TypedResults.Ok(new ResponseMessage("User API is running")))
        .Produces<ResponseMessage>(statusCode: 200)
        .WithName("Root")
        .WithSummary("Root endpoint")
        .WithDescription("Returns a simple message indicating that the User API is running.")
        .WithTags("General")
        .AllowAnonymous();

    // CREATE user
    app.MapPost(
        "/auth/register",
        Results<Created<UserNoPasswordDTO>,
        BadRequest<List<string>>,
        InternalServerError<ResponseMessage>> (SignupRequest user, IUserRepository userInMemoryRepository) =>
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
            return TypedResults.InternalServerError(new ResponseMessage(ex.Message));
        }
    })
    .Produces<UserNoPasswordDTO>(statusCode: 201)
    .Produces<List<string>>(statusCode: 400)
    .Produces<ResponseMessage>(statusCode: 500)
    .WithName("Register")
    .WithSummary("Register a new user")
    .WithDescription("Creates a new user account with the provided email and password. Returns the created user without the password.")
    .WithTags("Authentication")
    .AllowAnonymous();

    app.MapPost(
        "/auth/login",
        Results<Ok<UserNoPasswordDTO>, UnauthorizedHttpResult, JsonHttpResult<ResponseMessage>> (SignInRequest user, HttpContext context, IUserRepository userInMemoryRepository) =>
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


        // Add an http-only cookie for SPA clients (optional, can be used instead of Authorization header)
        // NOTE: If you use cookies, make sure to configure CORS and CSRF protections appropriately.
        context.Response.Cookies.Append("auth_token", existingUser.Token!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // required for HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = existingUser.ExpiresAt
        });

        return TypedResults.Ok(existingUser.WithoutPassword());
    })
    .Produces<UserNoPasswordDTO>(statusCode: 200)
    .Produces<ResponseMessage>(statusCode: 401)
    .WithName("Login")
    .WithSummary("Authenticate a user")
    .WithDescription("Authenticates a user with the provided email and password. Returns the authenticated user without the password if successful, or an error message if authentication fails.")
    .WithTags("Authentication")
    .AllowAnonymous();



    app.MapPost(
        "/auth/logout",
        Results<Ok<ResponseMessage>, UnauthorizedHttpResult, JsonHttpResult<ResponseMessage>> (HttpContext context, IUserRepository userInMemoryRepository) =>
    {
        if (context.Items.TryGetValue("User", out var userObj) && userObj is User user)
        {
            user.Token = null;
            user.ExpiresAt = null;

            // Remove cookie
            context.Response.Cookies.Append(
                "auth_token",
                "",
                new CookieOptions
                {
                    Expires = DateTime.UnixEpoch,
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                }
            );

            return TypedResults.Ok(new ResponseMessage("Logged out successfully."));
        }
        return TypedResults.Json(new ResponseMessage("Unauthorized"), statusCode: 401);
    })
    .Produces<ResponseMessage>(statusCode: 200)
    .Produces<ResponseMessage>(statusCode: 401)
    .WithName("Logout")
    .WithSummary("Logout a user")
    .WithDescription("Logs out the currently authenticated user by invalidating their token. Returns a success message if the user was logged out, or an error message if the user is not authenticated.")
    .WithTags("Authentication");



    app.MapGet("/auth/me", Results<Ok<UserNoPasswordDTO>, UnauthorizedHttpResult, JsonHttpResult<ResponseMessage>> (HttpContext context) =>
    {
        if (context.Items.TryGetValue("User", out var userObj) && userObj is User user)
        {
            return TypedResults.Ok(user.WithoutPassword());
        }
        return TypedResults.Json(new ResponseMessage("You are not authenticated."), statusCode: 401);
    })
    .Produces<UserNoPasswordDTO>(statusCode: 200)
    .Produces<ResponseMessage>(statusCode: 401)
    .WithName("GetCurrentUser")
    .WithSummary("Get current authenticated user")
    .WithDescription("Retrieves the currently authenticated user without the password.")
    .WithTags("User Management");

    // GET all users
    app.MapGet("/users", Results<Ok<IEnumerable<UserNoPasswordNoTokenDTO>>, BadRequest<ResponseMessage>> (IUserRepository userInMemoryRepository) =>
    {
        return TypedResults.Ok(userInMemoryRepository.GetAll().Select(u => u.WithoutTokensOrPassword()));
    })
    .Produces<IEnumerable<UserNoPasswordNoTokenDTO>>(statusCode: 200)
    .Produces<ResponseMessage>(statusCode: 400)
    .WithName("GetAllUsers")
    .WithSummary("Get all users")
    .WithDescription("Retrieves a list of all users without the password or token.")
    .WithTags("User Management");


    // GET user by id
    app.MapGet("/users/{id:Guid}", Results<Ok<UserNoPasswordNoTokenDTO>, NotFound<ResponseMessage>> (Guid id, IUserRepository userInMemoryRepository) =>
    {
        var user = userInMemoryRepository.GetById(id);
        return user is not null ? TypedResults.Ok(user.WithoutTokensOrPassword()) : TypedResults.NotFound(new ResponseMessage("User not found."));
    })
    .Produces<UserNoPasswordNoTokenDTO>(statusCode: 200)
    .Produces<ResponseMessage>(statusCode: 404)
    .WithName("GetUserById")
    .WithSummary("Get user by ID")
    .WithDescription("Retrieves a user by their unique identifier without the password or token.")
    .WithTags("User Management");


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
    })
    .Produces(statusCode: 204)
    .Produces<List<string>>(statusCode: 400)
    .WithName("UpdateUser")
    .WithSummary("Update a user")
    .WithDescription("Updates the details of an existing user.")
    .WithTags("User Management");


    // DELETE user
    app.MapDelete("/users/{id:Guid}", Results<UnauthorizedHttpResult, JsonHttpResult<ResponseMessage>, NoContent, NotFound> (
        Guid id,
        HttpContext context,
        IUserRepository userInMemoryRepository) =>
    {
        if (context.Items["User"] is not User currentUser || currentUser.Id != id)
        {
            return TypedResults.Json(new ResponseMessage("Unauthorized"), statusCode: 401);
        }
        var ok = userInMemoryRepository.Delete(id);
        return ok ? TypedResults.NoContent() : TypedResults.NotFound();
    })
    .Produces(statusCode: 204)
    .Produces<ResponseMessage>(statusCode: 401)
    .Produces<ResponseMessage>(statusCode: 404)
    .WithName("DeleteUser")
    .WithSummary("Delete a user")
    .WithDescription("Deletes an existing user.")
    .WithTags("User Management");


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
