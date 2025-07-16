using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Utility;

namespace ServiceFinder.Repos
{
    public class UserRepo
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Utility.AnonymizerService _anonymizerService;
        public UserRepo(ApplicationDbContext context, UserManager<ApplicationUser> userManager, AnonymizerService anonymizerService)
        {
            _context = context;
            _userManager = userManager;
            _anonymizerService = anonymizerService;
        }

        public Task<bool> HasNewMessages(string? id)
        {
            return _context.NewMessageThreadAlerts.AnyAsync(a => a.ApplicationUserId == id);
        }

        public async Task DeleteEntityAsync(string id)
        {
            var applicationUser = await _context.ApplicationUsers.FindAsync(id);

            if (applicationUser != null)
            {
                //Anonymize user's data
                await _anonymizerService.AnonymizeUserData(applicationUser.Id);
            }
        }

        public IQueryable<ApplicationUser> GetSearchQuery()
        {
            return _context.ApplicationUsers
                .Include(x => x.Subscription)
                .Include(x => x.City)
                .Where(x => x.DeletedOn == null);
        }

        public async Task<PagedResult<ApplicationUser>> SearchQuery(int curPage, IQueryable<ApplicationUser> query)
        {
            return await query
                .OrderBy(s => s.Id)
                .GetPaged(curPage);
        }

        public async Task<ApplicationUser?> GetEntityAsync(string id)
        {
            return await _context.ApplicationUsers.Include(x => x.Subscription)
            .Include(x => x.City).FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task UpdateEntityAsync(ApplicationUser applicationUser)
        {
            try
            {
                var entity = await _context.ApplicationUsers.FirstOrDefaultAsync(i => i.Id == applicationUser.Id);

                if (entity != null)
                {
                    //Update basic info
                    entity.FullName = applicationUser.FullName;
                    entity.AddressLine1 = applicationUser.AddressLine1;
                    entity.AddressLine2 = applicationUser.AddressLine2;
                    entity.PhoneNumber = applicationUser.PhoneNumber;
                    entity.PostalCode = applicationUser.PostalCode.ToUpper();
                    entity.CityId = applicationUser.CityId;

                    //Update Subscription
                    entity.SubscriptionId = applicationUser.SubscriptionId;

                    //Update email and login username
                    if (applicationUser.Email != null)
                    {
                        entity.NormalizedEmail = applicationUser.Email?.ToUpper();
                        entity.Email = applicationUser.Email;
                        entity.UserName = applicationUser.Email;
                        entity.NormalizedUserName = applicationUser.Email?.ToUpper();
                    }

                    //For providers
                    entity.IsVerified = applicationUser.IsVerified;
                    entity.ServiceProviderType = applicationUser.ServiceProviderType;

                    //update type
                    entity.UserType = applicationUser.UserType;

                    _context.ApplicationUsers.Update(entity);
                    await _context.SaveChangesAsync();


                    //Update role
                    var userCurrentRoles = await GetUserRolesAsync(entity);
                    if (userCurrentRoles != null)
                    {
                        //Remove all the old ones
                        await _userManager.RemoveFromRolesAsync(entity, userCurrentRoles);
                    }
                    //Add the new one
                    await _userManager.AddToRoleAsync(entity, applicationUser.UserType);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        private async Task<IList<string>> GetUserRolesAsync(ApplicationUser entity)
        {
            return await _userManager.GetRolesAsync(entity);
        }
    }
}
