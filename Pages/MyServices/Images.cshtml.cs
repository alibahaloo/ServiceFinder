using Amazon.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ServiceFinder.Data;
using ServiceFinder.Repos;
using Vereyon.Web;

namespace ServiceFinder.Pages.MyServices
{
    [Authorize]
    public class ImagesModel : PageModel
    {
        private readonly ServiceRepo _serviceRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAmazonS3 _s3Client;
        private readonly ApplicationDbContext _context;
        private readonly IFlashMessage _flashMessage;
        public ImagesModel(ServiceRepo serviceRepo, UserManager<ApplicationUser> userManager, IAmazonS3 s3Client, ApplicationDbContext context, IFlashMessage flashMessage)
        {
            _serviceRepo = serviceRepo;
            _userManager = userManager;
            _s3Client = s3Client;
            _context = context;
            _flashMessage = flashMessage;
        }

        public Service Service { get; set; } = default!;
        public ApplicationUser ApplicationUser { get; set; } = default!;
        [BindProperty]
        public ServiceImage ServiceImage { get; set; } = default!;

        public async Task<IActionResult> OnPostUploadImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                _flashMessage.Warning("Image file cannot be empty!");
                return RedirectToPage(new { serviceId = ServiceImage.ServiceId });
            }

            if (ServiceImage.Description == null)
            {
                _flashMessage.Warning("Image description cannot be empty!");
                return RedirectToPage(new { serviceId = ServiceImage.ServiceId });
            }

            //Check max allowed (8)
            var count = await _context.ServiceImages.Where(si => si.ServiceId == ServiceImage.ServiceId).CountAsync();
            if (count >= 8)
            {
                _flashMessage.Warning("Maximum number of images exceeded (8)");
                return RedirectToPage(new { serviceId = ServiceImage.ServiceId });
            }

            try
            {
                // Generate a unique key for the file in the "service-logos" folder
                var key = $"service-images/{Guid.NewGuid()}_{image.FileName}";

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

                //Find the Order
                int order = await _context.ServiceImages.Where(si => si.ServiceId == ServiceImage.ServiceId).CountAsync() + 1;

                //Now save into DB
                ServiceImage.ImageUrl = fileUrl;
                ServiceImage.Order = order;

                _context.ServiceImages.Add(ServiceImage);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }

            _flashMessage.Confirmation("Image saved successfully.");
            return RedirectToPage(new { serviceId = ServiceImage.ServiceId });
        }

        public async Task<IActionResult> OnPostDeleteImageAsync(int id)
        {
            if (id == 0) { return NotFound(); };

            var serviceImage = await _context.ServiceImages.FirstOrDefaultAsync(si => si.Id == id);
            if (serviceImage == null) { return NotFound(); }

            //TODO delete from S3

            _context.ServiceImages.Remove(serviceImage);
            await _context.SaveChangesAsync();
            _flashMessage.Confirmation("Image removed successfully.");
            return RedirectToPage(new { serviceId = serviceImage.ServiceId });
        }

        public async Task<IActionResult> OnPostOrderImagesAsync(string imagesOrder)
        {
            // Your logic to order ingredients based on the dictionary
            Dictionary<int, int>? imagesOrderDictionary = null;

            try
            {
                // Deserialize JSON string to Dictionary<int, int>
                imagesOrderDictionary = JsonConvert.DeserializeObject<Dictionary<int, int>>(imagesOrder);
            }
            catch (Exception)
            {

                throw;
            }

            if (imagesOrderDictionary != null)
            {
                //Do stuff here
                foreach (var item in imagesOrderDictionary)
                {
                    var productIngredient = await _context.ServiceImages.FirstOrDefaultAsync(m => m.Id == item.Key);

                    if (productIngredient != null)
                    {
                        productIngredient.Order = item.Value;
                    }
                }
            }

            await _context.SaveChangesAsync();
            string referer = Request.Headers.Referer.ToString();
            _flashMessage.Confirmation("Order saved successfully.");
            return Redirect(referer);
        }

        public async Task<IActionResult> OnGet(int serviceId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return Unauthorized(); }

            ApplicationUser = user;

            if (serviceId == 0) { return NotFound(); }
            var service = await _serviceRepo.GetEntityAsync(serviceId);
            if (service == null) { return NotFound(); };

            Service = service;

            return Page();
        }
    }
}
