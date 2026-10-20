using LiveChatWidgetAPI.Services;

namespace LiveChatWidgetAPI.Middleware
{
    public class TokenAuthMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly string[] AllowedOrigins =
        {
            "http://localhost:5173",

 "http://localhost:5175",

            "https://purple-wave-0ccea6f03.5.azurestaticapps.net",
            "https://appfe-obotp1-dev-live-chat-widget-mw-aadfdnf9baaxc4ag.swedencentral-01.azurewebsites.net"
        };

        public TokenAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            AzureAdTokenService tokenService,
            ILogger<TokenAuthMiddleware> logger)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant();

            if (IsAnonymousPath(path))
            {
                await _next(context);
                return;
            }

            // -----------------------------
            // Origin validation
            // -----------------------------
            var origin = context.Request.Headers.Origin.ToString();

            // Only validate when an Origin header is present (browser requests)
            if (!string.IsNullOrEmpty(origin) &&
                !AllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Origin is not allowed."
                });
                return;
            }

           

            // Get a service token for downstream API calls. Calling the service
            // directly avoids an unnecessary HTTP call back into /api/Auth/token.
            try
            {
                var tokenResponse = await tokenService.GetAccessTokenAsync();
                var accessToken = tokenResponse?.AccessToken;

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    throw new InvalidOperationException(
                        "The authentication service returned an empty access token.");
                }

                context.Items["AccessToken"] = accessToken;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to obtain a token for {RequestPath}.", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status502BadGateway;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Unable to authenticate the downstream API request."
                });
                return;
            }

            await _next(context);
        }

        private static bool IsAnonymousPath(string? path)
        {
            if (string.IsNullOrEmpty(path) || path == "/")
            {
                return true;
            }

            return path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
                || path.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase);
        }
    }

    public interface ICurrentTokenAccessor
    {
        string? AccessToken { get; }
    }

    public class CurrentTokenAccessor : ICurrentTokenAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentTokenAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? AccessToken =>
            _httpContextAccessor.HttpContext?.Items["AccessToken"] as string;
    }
}