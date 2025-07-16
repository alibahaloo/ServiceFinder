using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Utility;

namespace ServiceFinder.Repos
{
    public class ProvinceRepo
    {
        private readonly ApplicationDbContext _context;

        public ProvinceRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task DeleteEntityAsync(int? id)
        {
            var province = await _context.Provinces.FindAsync(id);

            if (province != null)
            {
                _context.Provinces.Remove(province);
                await _context.SaveChangesAsync();
            }
        }

        public IList<Province> GetAll()
        {
            return _context.Provinces
                .OrderBy(p => p.Id)
                .ToList();
        }

        public async Task<PagedResult<Province>> GetAllAsync(int curPage)
        {
            return await _context.Provinces
                .OrderBy(p => p.Id)
                .GetPaged(curPage);
        }

        public async Task<Province?> GetEntityAsync(int? id)
        {
            return await _context.Provinces.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task UpdateEntityAsync(Province province)
        {
            try
            {
                _context.Provinces.Update(province);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task CreateEntityAsync(Province province)
        {
            if (_context.Provinces == null || province == null) { return; }
            _context.Provinces.Add(province);
            await _context.SaveChangesAsync();
        }
    }
}
