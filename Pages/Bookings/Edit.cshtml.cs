using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.Bookings
{
    [Authorize]
    public class EditModel : BasePageModel
    {
        private readonly BookingRepo _bookingRepo;
        private readonly UnavailableDateRepo _unavailableDateRepo;
        private readonly ServiceRepo _serviceRepo;

        public EditModel(BookingRepo bookingRepo, IFlashMessage flashMessage, UserManager<Data.ApplicationUser> userManager, UnavailableDateRepo unavailableDateRepo, ServiceRepo serviceRepo)
            : base(userManager, flashMessage)
        {
            _bookingRepo = bookingRepo;
            _unavailableDateRepo = unavailableDateRepo;
            _serviceRepo = serviceRepo;
        }
        public DateOnly BookingDate { get; set; } = default!;
        [BindProperty]
        public string SelectedMonth { get; set; } = default!;
        [BindProperty]
        public int SelectedYear { get; set; } = default!;
        [BindProperty]
        public Data.InputModels.Booking Input { get; set; } = default!;
        public List<DateOnly> UnavailableDates { get; set; } = default!;
        public Data.Service Service { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id, string SelectedMonth, int SelectedYear, DateOnly BookingDate)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _bookingRepo.GetEntityAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            var service = await _serviceRepo.GetEntityAsync(booking.ServiceId);

            if (service == null)
            {
                return NotFound();
            }

            //Check ownership
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !await _bookingRepo.IsUserOwnerAsync(user.Id, booking.Id)) { return Unauthorized(); }

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

            if (BookingDate >= DateOnly.FromDateTime(DateTime.Today))
            {
                this.BookingDate = BookingDate;
            }
            else
            {
                this.BookingDate = DateOnly.FromDateTime(DateTime.Today);
            }

            //Get a list of unavailable dates for this service
            var unavailableDates = await _unavailableDateRepo.GetAllUnavailableDatesAsync(service.Id, this.SelectedMonth, this.SelectedYear);

            //get a list of other bookings from other customers
            var curBookedDates = await _bookingRepo.GetAllBookedDatesAsync(service.Id, this.SelectedMonth, this.SelectedYear);
            UnavailableDates = unavailableDates.Concat(curBookedDates).ToList();

            Service = service;

            Input = new Data.InputModels.Booking
            {
                Id = booking.Id,
                StartDate = booking.StartDate,
                ServiceId = booking.ServiceId,
                Description = booking.Description
            };

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //Check ownership
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !await _bookingRepo.IsUserOwnerAsync(user.Id, Input.Id)) { return Unauthorized(); }

            var booking = await _bookingRepo.GetEntityAsync(Input.Id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Description = Input.Description;
            booking.StartDate = Input.StartDate;
            booking.NumberOfHours = Input.NumberOfHours;

            await _bookingRepo.UpdateEntityAsync(booking);
            _flashMessage.Confirmation("Booking Updated Successfully!");

            return Redirect($"/Bookings/Details?Id={booking.Id}");
        }
    }
}
