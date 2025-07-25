using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;

namespace ServiceFinder.Utility
{
    public class AnonymizerService
    {
        private readonly ApplicationDbContext _context;

        public AnonymizerService(ApplicationDbContext context)
        {
            _context = context;
        }

        static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"; // Characters to choose from
            Random random = new Random();
            char[] randomChars = new char[length];

            for (int i = 0; i < length; i++)
            {
                randomChars[i] = chars[random.Next(chars.Length)]; // Choose a random character from the available set
            }

            return new string(randomChars);
        }

        public async Task AnonymizeUserData(string userId)
        {
            var user = await _context.Users.FindAsync(userId) ?? throw new InvalidDataException(userId);

            user.FullName = "Deleted User";
            user.Email = GenerateRandomString(15); //getting a random string, we cannot have duplicate usernames for accounts
            user.UserName = user.Email;
            user.NormalizedUserName = user.UserName.ToUpper();
            user.NormalizedEmail = user.Email.ToUpper();
            user.AddressLine1 = string.Empty;
            user.AddressLine2 = string.Empty;
            user.PhoneNumber = string.Empty;
            user.EmailConfirmed = false;
            user.IsVerified = false;
            user.PostalCode = string.Empty;
            user.PasswordHash = string.Empty;
            user.DeletedOn = DateTime.UtcNow;
            _context.ApplicationUsers.Update(user);

            //delete all comms
            var comms = await _context.MessageThreads.Where(c => c.AddedById == userId).ToListAsync();
            _context.MessageThreads.RemoveRange(comms);

            //delete all bookings
            var bookings = await _context.Bookings.Where(c => c.AddedById == userId).ToListAsync();
            _context.Bookings.RemoveRange(bookings);

            //delete all support tickets
            var supportTickets = await _context.SupportTickets.Where(c => c.UserId == userId).ToListAsync();
            _context.SupportTickets.RemoveRange(supportTickets);

            //Delete all services (if user was service provider)
            if (user.UserType == UserAccountRoles.ServiceProvider)
            {
                var services = await _context.Services.Where(c => c.AddedById == userId).ToListAsync();
                foreach (var service in services)
                {
                    //Remove all comms related to service
                    var serviceComms = await _context.MessageThreads.Where(c => c.ResourceId == service.Id && c.ResourceType == MessageResourceType.Service).ToListAsync();
                    _context.MessageThreads.RemoveRange(serviceComms);

                    //Remove all bookings related to service
                    var serviceBookings = await _context.Bookings.Where(b => b.ServiceId == service.Id).ToListAsync();
                    //Remove comms for each booking
                    foreach (var booking in serviceBookings)
                    {
                        var bookingComms = await _context.MessageThreads.Where(c => c.ResourceId == booking.Id && c.ResourceType == MessageResourceType.Booking).ToListAsync();
                        _context.MessageThreads.RemoveRange(bookingComms);
                    }

                    _context.Bookings.RemoveRange(serviceBookings);

                    //Remove all reviews related to service
                    var serviceReviews = await _context.Reviews.Where(r => r.ServiceId == service.Id).ToListAsync();
                    _context.Reviews.RemoveRange(serviceReviews);
                }

                //Remove all services
                _context.Services.RemoveRange(services);
            }

            await _context.SaveChangesAsync();
        }
    }
}
