using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using ServiceFinder.Utility;
using Vereyon.Web;

namespace ServiceFinder.Pages.Reviews
{
    [Authorize]
    public class CreateModel : BasePageModel
    {
        private readonly ServiceRepo _serviceRepo;
        private readonly ReviewRepo _reviewRepo;
        private readonly EmailService _emailService;
        private readonly NotificationRepo _notificationRepo;

        public CreateModel(ServiceRepo serviceRepo, IFlashMessage flashMessage, UserManager<Data.ApplicationUser> userManager, ReviewRepo reviewRepo, EmailService emailService, NotificationRepo notificationRepo)
        : base(userManager, flashMessage)
        {
            _serviceRepo = serviceRepo;
            _reviewRepo = reviewRepo;
            _emailService = emailService;
            _notificationRepo = notificationRepo;
        }

        [BindProperty]
        public Data.InputModels.Review Input { get; set; } = default!;
        public Data.Service Service { get; set; } = default!;

        public async Task<IActionResult> OnGet(int serviceId)
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

            if (user == null)
            {
                return Unauthorized();
            }

            //Check if the user is a service provider (an an owner)
            if (user.UserType == UserAccountRoles.ServiceProvider)
            {
                _flashMessage.Danger("You cannot using this functionality when logged in as a 'Service Provider'. Log in as a 'Customer' to be able to do so.");
                return Redirect("/Services");
            }

            //Check if user has already a review for this service
            var existingReview = await _reviewRepo.CheckExistingReviewByUser(user.Id, service.Id);
            if (existingReview != null)
            {
                //Redirect to the existing review
                _flashMessage.Warning("You already have written a review for this service. You cannot post anymore, but you can edit the existing one!");
                return Redirect($"/Reviews/Details?Id={existingReview.Id}");
            }

            Service = service;

            Input = new()
            {
                ServiceId = serviceId
            };

            return Page();
        }


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (Input.ServiceId == 0)
            {
                return NotFound();
            }

            var service = await _serviceRepo.GetEntityAsync(Input.ServiceId);
            if (service == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            //Check if user has already a review for this service
            var existingReview = await _reviewRepo.CheckExistingReviewByUser(user.Id, service.Id);
            if (existingReview != null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }
            Service = service;
            Data.Review review = new()
            {
                ServiceId = Input.ServiceId,
                Title = Input.Title,
                Body = Input.Body,
                Stars = Input.Stars
            };

            var newId = await _reviewRepo.CreateEntityAsync(review);

            //Send email to service provider
            if (service.ServiceProvider.Email != null && await _notificationRepo.GetUserNotificationStatusByFunction(NotificationFunction.ReviewsReceived, service.ServiceProvider.Id))
                _emailService.SendReviewCreated(service.ServiceProvider.Email, $"/Reviews/Details?Id={newId}");

            _flashMessage.Confirmation("Review Saved Successfully!");
            return Redirect($"/Reviews/Details?Id={newId}");
        }
    }
}
