using BlogAPI.Interface;

namespace BlogAPI.Middleware;

public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITokenBlacklist _tokenBlacklist;

    public TokenBlacklistMiddleware(RequestDelegate next, ITokenBlacklist tokenBlacklist)
    {
        _next = next;
        _tokenBlacklist = tokenBlacklist;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var token))
        {
            token = token.ToString().Replace("Bearer", "");
            if (_tokenBlacklist.isTokenBlacklisted(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }

        await _next(context);
    }
}