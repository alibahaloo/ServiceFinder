using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;

namespace ServiceFinder.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class NotificationsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public Dictionary<NotificationFunction, bool> NotificationValues { get; set; } = new();
        [TempData]
        public string StatusMessage { get; set; } = default!;
        public NotificationsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return Unauthorized(); }

            var userNotifications = await _context.Notifications.Where(n => n.ApplicationUserId == user.Id).ToListAsync();

            foreach (var item in userNotifications)
            {
                if (Enum.IsDefined(typeof(NotificationFunction), item.Function))
                {
                    // item.Function is a valid value in the NotificationFunction enum
                    NotificationValues.Add(item.Function, item.Status);
                }
                else
                {
                    // item.Function is not a valid value in the NotificationFunction enum
                    // So by default we say it's on
                    NotificationValues.Add(item.Function, true);
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(List<NotificationFunction> selectedNotifications)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return Unauthorized(); }

            //Deleting all configs
            var userNotifications = await _context.Notifications.Where(n => n.ApplicationUserId == user.Id).ToListAsync();

            _context.Notifications.RemoveRange(userNotifications);
            await _context.SaveChangesAsync();

            //Then saving new ones
            foreach (NotificationFunction function in Enum.GetValues(typeof(NotificationFunction)))
            {
                var newNotificationConfig = new Notification { ApplicationUserId = user.Id, Function = function };
                if (selectedNotifications.Contains(function))
                {
                    // The function is in the selectedNotifications list
                    // set is as true
                    newNotificationConfig.Status = true;
                }
                else
                {
                    // The function is not in the selectedNotifications list
                    // set it as false
                    newNotificationConfig.Status = false;
                }
                _context.Notifications.Add(newNotificationConfig);
                await _context.SaveChangesAsync();
            }

            StatusMessage = "Your notifications have been updated";
            return RedirectToPage();
        }
    }
}
