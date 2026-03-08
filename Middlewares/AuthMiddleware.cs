using UsersManagement.Services;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly HashSet<string> PublicPaths = new()
    {
        "/",
        "/auth/register",
        "/auth/login"
    };

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository repo)
    {
        var path = context.Request.Path.Value?.ToLower();

        // Allow public routes
        if (path != null && PublicPaths.Contains(path))
        {
            await _next(context);
            return;
        }

        // Extract token
        string? token = null;

        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var header = authHeader.ToString();
            if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = header.Substring("Bearer ".Length);
        }

        if (token is null && context.Request.Headers.TryGetValue("X-Api-Key", out var apiKey))
        {
            token = apiKey.ToString();
        }

        if (token is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing authentication token.");
            return;
        }

        // Validate token
        var user = repo.GetAll().FirstOrDefault(u => u.Token == token);

        if (user is null || user.ExpiresAt < DateTime.UtcNow)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid or expired token.");
            return;
        }

        // Attach user to context
        context.Items["User"] = user;

        await _next(context);
    }
}
