using System;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Test;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;

public class CustomResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
{
    private readonly TestUserStore userStore;
    private readonly ISystemClock clock;

    public CustomResourceOwnerPasswordValidator(TestUserStore userStore, ISystemClock clock)
    {
        this.userStore = userStore;
        this.clock = clock;
    }

    public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        if (userStore.ValidateCredentials(context.UserName, context.Password))
        {
            var user = userStore.FindByUsername(context.UserName);
            context.Result = new GrantValidationResult(
                user.SubjectId ?? throw new ArgumentException("Subject ID not set", nameof(user.SubjectId)),
                OidcConstants.AuthenticationMethods.Password, clock.UtcNow.UtcDateTime,
                user.Claims);
        }
        else
        {
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid custom credential");
        }

        return Task.CompletedTask;
    }
}
