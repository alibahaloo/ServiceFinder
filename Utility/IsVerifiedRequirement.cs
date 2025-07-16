using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServiceFinder.Data;

namespace ServiceFinder.Utility
{
    public class IsVerifiedRequirement : IAuthorizationRequirement
    {
    }

    public class IsVerifiedRequirementHandler : AuthorizationHandler<IsVerifiedRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IsVerifiedRequirementHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task<Task> HandleRequirementAsync(
            AuthorizationHandlerContext context,
            IsVerifiedRequirement requirement)
        {
            var user = context.User;

            var curUser = await _userManager.GetUserAsync(user);

            if (curUser != null && curUser.UserType == UserAccountRoles.Admin)
            {
                context.Succeed(requirement); // Admins always get a bypass
            }

            if (curUser != null && curUser.IsVerified && curUser.UserType == UserAccountRoles.ServiceProvider)
            {
                context.Succeed(requirement); // User is verified
            }

            return Task.CompletedTask;
        }
    }

}
