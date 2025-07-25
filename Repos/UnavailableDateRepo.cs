using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Utility;
using System.Globalization;

namespace ServiceFinder.Repos
{
    public class UnavailableDateRepo
    {
        private readonly ApplicationDbContext _context;

        public UnavailableDateRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IList<DateOnly>> GetAllUnavailableDatesAsync(int serviceId, string targetMonth, int year)
        {
            // Convert the targetMonth to a month number
            int targetMonthNumber = DateTime.ParseExact(targetMonth, "MMMM", CultureInfo.InvariantCulture).Month;

            // Calculate the start and end dates for the target month and year
            DateOnly startDate = new DateOnly(year, targetMonthNumber, 1);
            DateOnly endDate = startDate.AddMonths(1).AddDays(-1); // End of the target month

            return await _context.UnavailableDates
                .Where(u => u.ServiceId == serviceId && u.Date >= startDate && u.Date <= endDate)
               .Include(u => u.Service)
               .OrderByDescending(b => b.Date)
               .Select(u => u.Date)
               .ToListAsync();
        }


        public async Task<PagedResult<UnavailableDate>> GetAllByServiceAsync(int curPage, int serviceId, string targetMonth, int year)
        {
            // Convert the targetMonth to a month number
            int targetMonthNumber = DateTime.ParseExact(targetMonth, "MMMM", CultureInfo.InvariantCulture).Month;

            // Calculate the start and end dates for the target month and year
            DateOnly startDate = new DateOnly(year, targetMonthNumber, 1);
            DateOnly endDate = startDate.AddMonths(1).AddDays(-1); // End of the target month

            return await _context.UnavailableDates
                .Where(u => u.ServiceId == serviceId && u.Date >= startDate && u.Date <= endDate)
                .Include(u => u.Service)
                .OrderBy(b => b.Date)
                .GetPaged(curPage, 8);
        }


        public async Task<UnavailableDate?> GetEntityAsync(int? id)
        {
            return await _context.UnavailableDates
                .Include(u => u.Service)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> CheckDateExistsAsync(DateOnly date, int serviceId)
        {
            return await _context.UnavailableDates
                .Where(u => u.ServiceId == serviceId && u.Date == date).AnyAsync();
        }

        public async Task<int> CreateEntityAsync(UnavailableDate unavailableDate)
        {
            _context.UnavailableDates.Add(unavailableDate);
            await _context.SaveChangesAsync();

            return unavailableDate.Id;
        }

        public async Task DeleteEntityAsync(int? id)
        {
            var entity = await _context.UnavailableDates.FindAsync(id);

            if (entity != null)
            {
                _context.UnavailableDates.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
