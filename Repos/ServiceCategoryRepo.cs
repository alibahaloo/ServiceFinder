using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Utility;

namespace ServiceFinder.Repos
{
    public class ServiceCategoryRepo
    {
        private readonly ApplicationDbContext _context;

        public ServiceCategoryRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task DeleteEntityAsync(int? id)
        {
            var serviceCategory = await _context.Categories.FindAsync(id);

            if (serviceCategory != null)
            {
                _context.Categories.Remove(serviceCategory);
                await _context.SaveChangesAsync();
            }
        }

        public bool ValidateForSavingNavItem(int? id = null)
        {
            if (id == null)
            {
                return _context.Categories.AsNoTracking().Where(c => c.IsNavItem).Count() < 5;
            }
            else
            {
                var navItems = _context.Categories.AsNoTracking()
                    .Where(c => c.IsNavItem)
                    .OrderBy(c => c.Id) // Order by ID to ensure consistent results
                    .Take(5) // Take the first 5 categories with IsNavItem set to true
                    .ToList();

                return navItems.Count < 5 || navItems.Any(c => c.Id == id);
            }
        }


        public async Task<Dictionary<int, string>> GetNavItems()
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsNavItem)
                .Take(5)
                .ToDictionaryAsync(c => c.Id, c => c.Name);
        }


        public IList<Category> GetAll()
        {
            return _context.Categories.AsNoTracking().ToList(); ;
        }

        public async Task<PagedResult<Category>> GetAllAsync(int curPage)
        {
            return await _context.Categories.AsNoTracking()
                .OrderBy(s => s.Id)
                .Include(s => s.Industry)
                .GetPaged(curPage);
        }

        public async Task<Category?> GetEntityAsync(int? id)
        {
            return await _context.Categories
                .AsNoTracking()
                .Include(s => s.Industry)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task UpdateEntityAsync(Category serviceCategory)
        {
            try
            {
                _context.Categories.Update(serviceCategory);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task CreateEntityAsync(Category serviceCategory)
        {
            if (_context.Categories == null || serviceCategory == null) { return; }
            _context.Categories.Add(serviceCategory);
            await _context.SaveChangesAsync();
        }
    }
}
