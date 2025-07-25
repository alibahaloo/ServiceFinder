using ServiceFinder.Data;
using ServiceFinder.Repos;

namespace ServiceFinder.Pages.Admin.Cities
{
    public class IndexModel : AdminPageModel
    {
        private readonly CityRepo _cityService;
        public int CurPage { get; set; }
        public int PageCount { get; set; }

        public IndexModel(CityRepo cityService)
        {
            this._cityService = cityService;
        }

        public IList<City> Cities { get; set; } = default!;

        public async Task OnGetAsync(int curPage = 1)
        {
            CurPage = curPage;

            var result = await _cityService.GetAllAsync(CurPage);
            Cities = result.Data;

            PageCount = result.PageCount;
        }
    }
}
