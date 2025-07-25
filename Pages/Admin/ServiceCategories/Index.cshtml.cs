using ServiceFinder.Data;
using ServiceFinder.Repos;

namespace ServiceFinder.Pages.Admin.ServiceCategories
{
    public class IndexModel : AdminPageModel
    {
        private readonly ServiceCategoryRepo _repo;
        public int CurPage { get; set; }
        public int PageCount { get; set; }
        public IndexModel(ServiceCategoryRepo repo)
        {
            _repo = repo;
        }

        public IList<Category> ServiceCategories { get; set; } = default!;

        public async Task OnGetAsync(int curPage = 1)
        {
            CurPage = curPage;

            var result = await _repo.GetAllAsync(CurPage);
            ServiceCategories = result.Data;

            PageCount = result.PageCount;
        }
    }
}
