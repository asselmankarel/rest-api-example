using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Movies.Api.Auth;

public class AdminAuthRequirement : IAuthorizationRequirement, IAuthorizationHandler
{
    private readonly string _apiKey;
    private readonly IConfiguration _configuration;

    public AdminAuthRequirement(string apiKey, IConfiguration configuration)
    {
        _apiKey = apiKey;
        _configuration = configuration;
    }

    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.User.HasClaim(AuthConstants.AdminUserPolicyName, "true"))
        {
            context.Succeed(this);
            return Task.CompletedTask;
        }
        var httpContext = context.Resource as HttpContext;
        if(httpContext is null)
        {
            return Task.CompletedTask;
        }
        
        if (!httpContext.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Fail();
            return Task.CompletedTask;
        }
        
        var apiKey = _configuration[AuthConstants.ApiKeyConfigSection]!;
        if (apiKey != extractedApiKey)
        {
            context.Fail();
            return Task.CompletedTask;
        }
        
        var identity = (ClaimsIdentity)httpContext.User.Identity!;
        identity.AddClaim(new Claim("userid", "b8671179-6a29-41b2-8fff-cf21e818c876"));
        
        context.Succeed(this);
        return Task.CompletedTask;  
    }
}