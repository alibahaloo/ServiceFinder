using ServiceFinder.Data;
using ServiceFinder.Repos;

namespace ServiceFinder.Pages.Admin.Provinces
{
    public class IndexModel : AdminPageModel
    {
        private readonly ProvinceRepo _provinceRepo;
        public int CurPage { get; set; }
        public int PageCount { get; set; }
        public IndexModel(ProvinceRepo provinceRepo)
        {
            _provinceRepo = provinceRepo;
        }

        public IList<Province> Provinces { get; set; } = default!;

        public async Task OnGetAsync(int curPage = 1)
        {

            CurPage = curPage;

            var result = await _provinceRepo.GetAllAsync(CurPage);
            Provinces = result.Data;

            PageCount = result.PageCount;
        }
    }
}
