using Amazon.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ServiceFinder.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServiceLogoController : Controller
    {
        private readonly IAmazonS3 _s3Client;
        public ServiceLogoController(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        [HttpPost("uploadLogo")]
        public async Task<IActionResult> UploadLogo(IFormFile businessLogo)
        {
            /*
             * This uploads the given image onto S3
             * replace the variables with proper values.
             */

            string folder = "service-logos";
            string bucketName = "findservice";
            string s3BaseURL = "https://findservice.s3.amazonaws.com/";

            if (businessLogo == null || businessLogo.Length == 0)
            {
                return BadRequest("File not selected or empty.");
            }

            try
            {
                // Generate a unique key for the file in the "service-logos" folder
                var key = $"{folder}/{Guid.NewGuid()}_{businessLogo.FileName}";

                // Upload the file to S3
                using (var stream = businessLogo.OpenReadStream())
                {
                    var request = new Amazon.S3.Transfer.TransferUtilityUploadRequest
                    {
                        InputStream = stream,
                        Key = key,
                        BucketName = bucketName,
                        CannedACL = S3CannedACL.PublicRead // Set appropriate ACL for your use case
                    };

                    var fileTransferUtility = new Amazon.S3.Transfer.TransferUtility(_s3Client);
                    await fileTransferUtility.UploadAsync(request);
                }

                // Construct the public URL for the uploaded file
                var fileUrl = $"{s3BaseURL}/{key}";

                return Ok(new { Success = true, Message = "File successfully uploaded.", FileUrl = fileUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = "An error occurred: " + ex.Message });
            }
        }
    }
}
