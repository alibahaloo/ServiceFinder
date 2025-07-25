using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using System.Globalization;
using Vereyon.Web;
using static Amazon.S3.Util.S3EventNotification;

namespace ServiceFinder.Pages.MyServices
{
    [Authorize(Policy = "IsVerifiedPolicy")]
    public class UnavailableDateModel : BasePageModel
    {
        private readonly UnavailableDateRepo _unavailableDateRepo;
        private readonly ServiceRepo _serviceRepo;
        private readonly ApplicationDbContext _context;
        public UnavailableDateModel(UnavailableDateRepo unavailableDateRepo, IFlashMessage flashMessage, UserManager<Data.ApplicationUser> userManager, ServiceRepo serviceRepo, ApplicationDbContext context)
            : base(userManager, flashMessage)
        {
            _unavailableDateRepo = unavailableDateRepo;
            _serviceRepo = serviceRepo;
            _context = context;
        }
        public int CurPage { get; set; }
        public int PageCount { get; set; }
        [BindProperty]
        public Data.InputModels.UnavailableDate Input { get; set; } = default!;
        [BindProperty]
        public Data.Service Service { get; set; } = default!;
        public IList<Data.UnavailableDate> UnavailableDates { get; set; } = default!;
        public List<DateOnly> CalendarDates { get; set; } = default!;
        [BindProperty]
        public string SelectedMonth { get; set; } = default!;
        [BindProperty]
        public int SelectedYear { get; set; } = default!;
        private enum AdjacentMode
        {
            Next, Previous
        }
        private static (string Month, int Year) GetAdjacentMonths(string currentMonth, int currentYear, AdjacentMode mode)
        {
            if (!DateTime.TryParseExact(currentMonth, "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime currentDate))
            {
                throw new ArgumentException("Invalid month name provided.");
            }
            DateTime current = new(currentYear, currentDate.Month, 1);
            var result = mode switch
            {
                AdjacentMode.Next => current.AddMonths(1),
                AdjacentMode.Previous => current.AddMonths(-1),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Invalid mode provided."),
            };
            return (result.ToString("MMMM"), result.Year);
        }
        public IActionResult OnPostNextMonth(int serviceId, string CurrentMonth, int CurrentYear)
        {
            //Pass the current selected month, find the next, navigate to it
            var (Month, Year) = GetAdjacentMonths(CurrentMonth, CurrentYear, AdjacentMode.Next);
            return RedirectToPage(new { serviceId = Input.ServiceId, SelectedMonth = Month, SelectedYear = Year });
        }
        public IActionResult OnPostPrevMonth(int serviceId, string CurrentMonth, int CurrentYear)
        {
            //Pass the current selected month, find the previous, navigate to it
            var (Month, Year) = GetAdjacentMonths(CurrentMonth, CurrentYear, AdjacentMode.Next);
            return RedirectToPage(new { serviceId = Input.ServiceId, SelectedMonth = Month, SelectedYear = Year });
        }
        public async Task<IActionResult> OnPostDeleteDateAsync(int serviceId, DateOnly date)
        {
            if (serviceId == 0)
            {
                return NotFound();
            }

            //Check if the input date is greater than today
            if (Input.Date < DateOnly.FromDateTime(DateTime.Now))
            {
                _flashMessage.Danger("Choose a date that is either today or in the future.");
                return RedirectToPage(new { ServiceId = Input.ServiceId });
            }

            var unAvailableDate = await _context.UnavailableDates.Where(ud => ud.ServiceId == serviceId && ud.Date == date).FirstOrDefaultAsync();

            if (unAvailableDate == null) { return NotFound(); }

            _context.UnavailableDates.Remove(unAvailableDate);
            await _context.SaveChangesAsync();
            _flashMessage.Confirmation("Date Removed");
            return RedirectToPage(new { ServiceId = unAvailableDate.ServiceId, SelectedMonth = Input.Date.ToString("MMMM"), SelectedYear = Input.Date.Year });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //Check if the input date is greater than today
            if (Input.Date < DateOnly.FromDateTime(DateTime.Now))
            {
                _flashMessage.Danger("Choose a date that is either today or in the future.");
                return RedirectToPage(new { ServiceId = Input.ServiceId });
            }

            if (await _unavailableDateRepo.CheckDateExistsAsync(Input.Date, Input.ServiceId))
            {
                _flashMessage.Danger("Date already exists. Please select a different date.");
                return RedirectToPage(new { ServiceId = Input.ServiceId });
            }

            var newDate = new Data.UnavailableDate
            {
                Date = Input.Date,
                ServiceId = Input.ServiceId,
            };
            await _unavailableDateRepo.CreateEntityAsync(newDate);
            _flashMessage.Confirmation("Date Added");
            return RedirectToPage(new { ServiceId = Input.ServiceId, SelectedMonth = Input.Date.ToString("MMMM"), SelectedYear = Input.Date.Year });
        }

        public async Task<IActionResult> OnGetAsync(int serviceId, string SelectedMonth, int SelectedYear, int curPage = 1)
        {
            if (serviceId == 0)
            {
                return NotFound();
            }

            var service = await _serviceRepo.GetEntityAsync(serviceId);
            if (service == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || !await _serviceRepo.IsUserOwnerAsync(user.Id, service.Id)) { return Unauthorized(); }

            if (SelectedMonth != null)
            {
                this.SelectedMonth = SelectedMonth;
            }
            else
            {
                this.SelectedMonth = DateTime.Now.ToString("MMMM");
            }

            if (SelectedYear != 0)
            {
                this.SelectedYear = SelectedYear;
            }
            else
            {
                this.SelectedYear = DateTime.Now.Year;
            }

            Service = service;

            //Get all for this service
            var result = await _unavailableDateRepo.GetAllByServiceAsync(curPage, service.Id, this.SelectedMonth, this.SelectedYear);
            UnavailableDates = result.Data;

            CurPage = curPage;
            PageCount = result.PageCount;

            var calendarDates = await _unavailableDateRepo.GetAllUnavailableDatesAsync(service.Id, this.SelectedMonth, this.SelectedYear);
            CalendarDates = calendarDates.ToList();


            return Page();
        }
    }
}
