// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Amazon.S3;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServiceFinder.Data;
using System.ComponentModel.DataAnnotations;

namespace ServiceFinder.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ServiceFinder.Data.ApplicationDbContext _context;
        private readonly IAmazonS3 _s3Client;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IAmazonS3 s3Client)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _s3Client = s3Client;
        }

        public string ProfilePictureURL { get; set; } = default!;

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        /// 
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "Full Name")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            public string FullName { get; set; }

            [Required]
            [Display(Name = "Address Line 1")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }

            [Required]
            [Display(Name = "Postal Code")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.PostalCode)]
            public string PostalCode { get; set; }

            [Required]
            [Display(Name = "City")]
            public int CityId { get; set; }

            public ServiceProviderType ServiceProviderType { get; set; } = default!;
        }

        public string UserType { get; set; } = string.Empty;

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FullName = user.FullName,
                AddressLine1 = user.AddressLine1,
                AddressLine2 = user.AddressLine2,
                PostalCode = user.PostalCode,
                CityId = user.CityId,
                ServiceProviderType = user.ServiceProviderType,
            };
        }
        public async Task<IActionResult> OnPostDeleteImageAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var userProfile = await _context.ApplicationUsers.FirstOrDefaultAsync(i => i.Id == user.Id);
            if (userProfile != null)
            {
                userProfile.ProfileURL = null;
                _context.ApplicationUsers.Update(userProfile);
                await _context.SaveChangesAsync();
            }
            //TODO delete from S3

            StatusMessage = "Profile picture removed successfully.";
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostUploadImageAsync(IFormFile image)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (image == null || image.Length == 0)
            {
                StatusMessage = "Image file cannot be empty!";
                return RedirectToPage();
            }

            try
            {
                // Generate a unique key for the file in the "service-logos" folder
                var key = $"profile-pictures/{Guid.NewGuid()}_{image.FileName}";

                // Upload the file to S3
                using (var stream = image.OpenReadStream())
                {
                    var request = new Amazon.S3.Transfer.TransferUtilityUploadRequest
                    {
                        InputStream = stream,
                        Key = key,
                        BucketName = "findservice",
                        CannedACL = S3CannedACL.PublicRead // Set appropriate ACL for your use case
                    };

                    var fileTransferUtility = new Amazon.S3.Transfer.TransferUtility(_s3Client);
                    await fileTransferUtility.UploadAsync(request);
                }

                // Construct the public URL for the uploaded file
                var fileUrl = $"https://findservice.s3.amazonaws.com/{key}";
                var userProfile = await _context.ApplicationUsers.FirstOrDefaultAsync(i => i.Id == user.Id);
                if (userProfile != null)
                {
                    userProfile.ProfileURL = fileUrl;
                    _context.ApplicationUsers.Update(userProfile);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }

            StatusMessage = "Profile picture uploaded successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);

            UserType = user.UserType;
            ProfilePictureURL = user.ProfileURL;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userProfile = await _context.ApplicationUsers.FirstOrDefaultAsync(i => i.Id == user.Id);
            if (userProfile != null)
            {
                userProfile.FullName = Input.FullName;
                userProfile.AddressLine1 = Input.AddressLine1;
                userProfile.AddressLine2 = Input.AddressLine2;
                userProfile.PhoneNumber = Input.PhoneNumber;
                userProfile.PostalCode = Input.PostalCode;
                userProfile.CityId = Input.CityId;
                userProfile.ServiceProviderType = Input.ServiceProviderType;
                _context.ApplicationUsers.Update(userProfile);
                await _context.SaveChangesAsync();
            }


            /*
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }
            */

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
