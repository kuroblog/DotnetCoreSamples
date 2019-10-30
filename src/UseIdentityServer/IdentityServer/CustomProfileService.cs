using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Test;
using Microsoft.Extensions.Logging;

public class CustomProfileService : IProfileService
{
    private readonly ILogger logger;
    private readonly TestUserStore userStore;

    public CustomProfileService(ILogger<TestUserProfileService> logger, TestUserStore userStore)
    {
        this.logger = logger;
        this.userStore = userStore;
    }

    public Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        context.LogProfileRequest(logger);

        if (context.RequestedClaimTypes.Any())
        {
            var user = userStore.FindBySubjectId(context.Subject.GetSubjectId());
            if (user != null)
            {
                context.AddRequestedClaims(user.Claims);
            }
        }

        context.LogIssuedClaims(logger);

        return Task.CompletedTask;
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        logger.LogDebug("IsActive called from: {caller}", context.Caller);

        var user = userStore.FindBySubjectId(context.Subject.GetSubjectId());
        context.IsActive = user?.IsActive == true;

        return Task.CompletedTask;
    }
}
