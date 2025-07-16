using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Utility;

namespace ServiceFinder.Repos
{
    public class CityRepo
    {
        private readonly ApplicationDbContext _context;
        public CityRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task DeleteEntityAsync(int? id)
        {
            var city = await _context.Cities.FindAsync(id);

            if (city != null)
            {
                _context.Cities.Remove(city);
                await _context.SaveChangesAsync();
            }
        }
        public bool ValidateForSavingNavItem(int? id = null)
        {
            if (id == null)
            {
                return _context.Cities.AsNoTracking().Where(c => c.IsNavItem).Count() < 5;
            }
            else
            {
                var navItems = _context.Cities.AsNoTracking()
                    .Where(c => c.IsNavItem)
                    .OrderBy(c => c.Id) // Order by ID to ensure consistent results
                    .Take(5) // Take the first 5 categories with IsNavItem set to true
                    .ToList();

                return navItems.Count < 5 || navItems.Any(c => c.Id == id);
            }
        }


        public async Task<Dictionary<int, string>> GetNavItems()
        {
            return await _context.Cities
                .AsNoTracking()
                .Where(c => c.IsNavItem)
                .Take(5)
                .ToDictionaryAsync(c => c.Id, c => c.Name);
        }
        public async Task<PagedResult<City>> GetAllAsync(int curPage)
        {
            return await _context.Cities.AsNoTracking()
                .Include(c => c.Province)
                .OrderBy(c => c.Id)
                .GetPaged(curPage);
        }

        public IList<City> GetAll()
        {
            return _context.Cities.AsNoTracking().ToList();
        }

        public async Task<City?> GetEntityAsync(int? id)
        {
            return await _context.Cities.AsNoTracking().Include(c => c.Province).FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task UpdateEntityAsync(City city)
        {
            try
            {
                _context.Cities.Update(city);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task CreateEntityAsync(City city)
        {
            if (_context.Cities == null || city == null) { return; }
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();
        }
    }
}
