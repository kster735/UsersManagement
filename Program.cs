using UsersManagement.Models;
using UsersManagement.Services;

var builder = WebApplication.CreateBuilder(args);

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


app.MapGet("/", () => "User API is running");

// GET all users
app.MapGet("/users", (IUserRepository userInMemoryRepository) =>
{
    return TypedResults.Ok(userInMemoryRepository.GetAll());
});

// GET user by id
app.MapGet("/users/{id:Guid}", IResult (Guid id, IUserRepository userInMemoryRepository) =>
{
    var user = userInMemoryRepository.GetById(id);
    return user is not null ? TypedResults.Ok(user) : TypedResults.NotFound();
});

// CREATE user
app.MapPost("/users", (User user, IUserRepository userInMemoryRepository) =>
{
    var created = userInMemoryRepository.Create(user);
    return TypedResults.Created($"/users/{created.Id}", created);
});

// UPDATE user
app.MapPut("/users/{id:Guid}", IResult (Guid id, User updated, IUserRepository userInMemoryRepository) =>
{
    var ok = userInMemoryRepository.Update(id, updated);
    return ok ? TypedResults.NoContent() : TypedResults.NotFound();
});

// DELETE user
app.MapDelete("/users/{id:Guid}", IResult (Guid id, IUserRepository userInMemoryRepository) =>
{
    var ok = userInMemoryRepository.Delete(id);
    return ok ? TypedResults.NoContent() : TypedResults.NotFound();
});

app.Run();
