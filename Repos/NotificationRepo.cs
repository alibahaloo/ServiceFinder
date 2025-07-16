using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;

namespace ServiceFinder.Repos
{
    public class NotificationRepo
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> GetUserNotificationStatusByFunction(NotificationFunction function, string userId)
        {
            var notificationConfig = await _context.Notifications
                .FirstOrDefaultAsync(n => n.ApplicationUserId == userId && n.Function == function);
            if (notificationConfig != null)
            {
                return notificationConfig.Status;
            }
            else
            {
                return true; //by default, all notifications are on .. unless the user has defined as off
            }
        }
    }
}
