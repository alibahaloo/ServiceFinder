using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using ServiceFinder.Data;
using System.Net;
using System.Net.Mail;

namespace ServiceFinder.Utility
{
    public class EmailService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<EmailService> _logger;

        private readonly string? smtpServer = default!;
        private readonly string? smtpUsername = default!;
        private readonly string? smtpPassword = default!;
        private readonly string fromEmail = default!;
        private readonly string supportEmail = default!;
        private readonly int smtpPort = 587;
        private readonly bool enableSsl = true;

        private readonly string baseURL = "somedomain.com";
        private readonly string baseAppName = "ServiceFinder";

        public EmailService(IWebHostEnvironment webHostEnvironment, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<EmailService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;

            smtpServer = _configuration["EmailSettings:SmtpServer"] ?? throw new NullReferenceException("EmailSettings:SmtpServer");
            smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? throw new NullReferenceException("EmailSettings:SmtpUsername");
            smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? throw new NullReferenceException("EmailSettings:SmtpPassword");
            fromEmail = _configuration["EmailSettings:NoReplyEmail"] ?? throw new NullReferenceException("EmailSettings:NoReplyEmail");
            supportEmail = _configuration["EmailSettings:SupportEmail"] ?? throw new NullReferenceException("EmailSettings:SupportEmail");
            smtpPort = Convert.ToInt32(_configuration["EmailSettings:SmtpPort"] ?? "587");

            _logger = logger;
        }

        private string GetLink(string url)
        {
            if (_httpContextAccessor == null || _httpContextAccessor.HttpContext == null)
            {
                throw new NullReferenceException(nameof(_httpContextAccessor));
            }

            // Get the base URL
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";

            return baseUrl + url;
        }

        public (bool, string) SendAccountAccessRequest(string toEmailAddress)
        {
            try
            {
                using SmtpClient client = new SmtpClient(smtpServer);
                client.Port = smtpPort;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = enableSsl;

                using MailMessage message = new MailMessage();
                message.From = new MailAddress(fromEmail); // Must be a verified email
                message.To.Add(new MailAddress(toEmailAddress));
                message.Subject = "Service Finder - Access Request Submitted";

                // Read the HTML content from an HTML file
                string htmlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates/AccountAccessRequest.html");
                string body = System.IO.File.ReadAllText(htmlFilePath);

                // Set the HTML content as the email body
                message.IsBodyHtml = true;
                message.Body = body.Replace("{{BaseURL}}", baseURL).Replace("{{BaseAppName}}", baseAppName).Replace("{{fromEmail}}", fromEmail); ;

                client.Send(message);

                _logger.LogInformation($"SendAccountAccessRequest: Email sent successfully to {toEmailAddress}");
                return new(true, $"Email sent successfully to {toEmailAddress}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendAccountAccessRequest: Email sending failed: {ex.Message}");
                return new(false, $"Email sending failed: {ex.Message}");
            }
        }

        public (bool, string) SendAccountVerified(string toEmailAddress)
        {
            try
            {
                using SmtpClient client = new SmtpClient(smtpServer);
                client.Port = smtpPort;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = enableSsl;

                using MailMessage message = new MailMessage();
                message.From = new MailAddress(fromEmail); // Must be a verified email
                message.To.Add(new MailAddress(toEmailAddress));
                message.Subject = "Service Finder - Account Verified";

                // Read the HTML content from an HTML file
                string htmlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates/AccountVerified.html");
                string body = System.IO.File.ReadAllText(htmlFilePath);

                // Set the HTML content as the email body
                message.IsBodyHtml = true;
                message.Body = body.Replace("{{BaseURL}}", baseURL).Replace("{{BaseAppName}}", baseAppName).Replace("{{fromEmail}}", fromEmail); ;

                client.Send(message);

                _logger.LogInformation($"SendAccountVerified: Email sent successfully to {toEmailAddress}");
                return new(true, $"Email sent successfully to {toEmailAddress}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendPasswordUpdate: Email sending failed: {ex.Message}");
                return new(false, $"Email sending failed: {ex.Message}");
            }
        }

        public (bool, string) SendPasswordUpdate(string toEmailAddress)
        {
            try
            {
                using SmtpClient client = new SmtpClient(smtpServer);
                client.Port = smtpPort;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = enableSsl;

                using MailMessage message = new MailMessage();
                message.From = new MailAddress(fromEmail); // Must be a verified email
                message.To.Add(new MailAddress(toEmailAddress));
                message.Subject = "Service Finder - Password Updated";

                // Read the HTML content from an HTML file
                string htmlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates/AccountPasswordUpdated.html");
                string body = System.IO.File.ReadAllText(htmlFilePath);

                // Set the HTML content as the email body
                message.IsBodyHtml = true;
                message.Body = body.Replace("{{BaseURL}}", baseURL).Replace("{{BaseAppName}}", baseAppName).Replace("{{fromEmail}}", fromEmail); ;

                client.Send(message);

                _logger.LogInformation($"SendPasswordUpdate: Email sent successfully to {toEmailAddress}");
                return new(true, $"Email sent successfully to {toEmailAddress}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendPasswordUpdate: Email sending failed: {ex.Message}");
                return new(false, $"Email sending failed: {ex.Message}");
            }
        }

        public (bool, string) SendPasswordReset(string passwordResetLink, string toEmailAddress)
        {
            try
            {
                using SmtpClient client = new SmtpClient(smtpServer);
                client.Port = smtpPort;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = enableSsl;

                using MailMessage message = new MailMessage();
                message.From = new MailAddress(fromEmail); // Must be a verified email
                message.To.Add(new MailAddress(toEmailAddress));
                message.Subject = "Service Finder - Password Reset";

                // Read the HTML content from an HTML file
                string htmlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates/AccountResetPassword.html");
                string htmlContent = System.IO.File.ReadAllText(htmlFilePath);

                //Replace the  link placeholder
                string body = htmlContent.Replace("{{PasswordResetLink}}", passwordResetLink);

                // Set the HTML content as the email body
                message.IsBodyHtml = true;
                message.Body = body.Replace("{{BaseURL}}", baseURL).Replace("{{BaseAppName}}", baseAppName).Replace("{{fromEmail}}", fromEmail); ;

                client.Send(message);

                _logger.LogInformation($"SendPasswordReset: Email sent successfully to {toEmailAddress}");
                return new(true, $"Email sent successfully to {toEmailAddress}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendPasswordReset: Email sending failed: {ex.Message}");
                return new(false, $"Email sending failed: {ex.Message}");
            }
        }

        public (bool, string) SendBookingConfirmation(string toEmailAddress, string serviceName, string serviceProviderName, string bookingLink)
        {
            try
            {
                using SmtpClient client = new SmtpClient(smtpServer);
                client.Port = smtpPort;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = enableSsl;

                using MailMessage message = new MailMessage();
                message.From = new MailAddress(fromEmail); // Must be a verified email
                message.To.Add(new MailAddress(toEmailAddress));
                message.Subject = "Service Finder - Booking Confirmation";

                // Read the HTML content from an HTML file
                string htmlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates/BookingConfirmation.html");
                string htmlContent = System.IO.File.ReadAllText(htmlFilePath);

                //Replace the  link placeholder
                string body = htmlContent.Replace("{{BookingLink}}", GetLink(bookingLink)).Replace("{{ServiceName}}", serviceName).Replace("{{ServiceProviderName}}", serviceProviderName);

                // Set the HTML content as the email body
                message.IsBodyHtml = true;
                message.Body = body.Replace("{{BaseURL}}", baseURL).Replace("{{BaseAppName}}", baseAppName).Replace("{{fromEmail}}", fromEmail); ;

                client.Send(message);

                _logger.LogInformation($"SendBookingConfirmation: Email sent successfully to {toEmailAddress}");
                return new(true, $"Email sent successfully to {toEmailAddress}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendBookingConfirmation: Email sending failed: {ex.Message}");
                return new(false, $"Email sending failed: {ex.Message}");
            }
        }

        public (bool, string) SendBookingStatusUpdated(string toEmailAddress, string bookingLink, string bookingStatus)
        {
            try
            {
                using SmtpClient client = new SmtpClient(smtpServer);
                client.Port = smtpPort;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = enableSsl;

                using MailMessage message = new MailMessage();
                message.From = new MailAddress(fromEmail); // Must be a verified email
                message.To.Add(new MailAddress(toEmailAddress));
                message.Subject = "Service Finder - Booking Received";

                // Read the HTML content from an HTML file
                string htmlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates/BookingStatusUpdated.html");
                string htmlContent = System.IO.File.ReadAllText(htmlFilePath);

                //Replace the  link placeholder
                string body = htmlContent.Replace("{{BookingLink}}", GetLink(bookingLink)).Replace("{{Status}}", bookingStatus);

                // Set the HTML content as the email body
                message.IsBodyHtml = true;
                message.Body = body.Replace("{{BaseURL}}", baseURL).Replace("{{BaseAppName}}", baseAppName).Replace("{{fromEmail}}", fromEmail); ;

                client.Send(message);

                _logger.LogInformation($"SendBookingStatusUpdated: Email sent successfully to {toEmailAddress}");
                return new(true, $"Email sent successfully to {toEmailAddress}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendBookingStatusUpdated: Email sending failed: {ex.Message}");
                return new(false, $"Email sending failed: {ex.Message}");
            }
        }

        public (bool, string) SendBookingReceived(string userName, string toEmailAddress, string serviceName, string bookingLink)
        {
            try
            {
                using SmtpClient client = new SmtpClient(smtpServer);
                client.Port = smtpPort;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = enableSsl;

                using MailMessage message = new MailMessage();
                message.From = new MailAddress(fromEmail); // Must be a verified email
                message.To.Add(new MailAddress(toEmailAddress));
                message.Subject = "Service Finder - Booking Received";

                // Read the HTML content from an HTML file
                string htmlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates/BookingReceived.html");
                string htmlContent = System.IO.File.ReadAllText(htmlFilePath);

                //Replace the  link placeholder
                string body = htmlContent.Replace("{{BookingLink}}", GetLink(bookingLink)).Replace("{{UserName}}", userName).Replace("{{ServiceName}}", serviceName);

                // Set the HTML content as the email body
                message.IsBodyHtml = true;
                message.Body = body.Replace("{{BaseURL}}", baseURL).Replace("{{BaseAppName}}", baseAppName).Replace("{{fromEmail}}", fromEmail); ;

                client.Send(message);

                _logger.LogInformation($"SendBookingReceived: Email sent successfully to {toEmailAddress}");
                return new(true, $"Email sent successfully to {toEmailAddress}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendBookingReceived: Email sending failed: {ex.Message}");
                return new(false, $"Email sending failed: {ex.Message}");
            }
        }

        public (bool, string) SendReviewCreated(string toEmailAddress, string reviewLink)
        {
            try
            {
                using SmtpClient client = new SmtpClient(smtpServer);
                client.Port = smtpPort;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = enableSsl;

                using MailMessage message = new MailMessage();
                message.From = new MailAddress(fromEmail); // Must be a verified email
                message.To.Add(new MailAddress(toEmailAddress));
                message.Subject = "Service Finder - New Review";

                // Read the HTML content from an HTML file
                string htmlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates/ReviewCreated.html");
                string htmlContent = System.IO.File.ReadAllText(htmlFilePath);

                //Replace the  link placeholder
                string body = htmlContent.Replace("{{ReviewLink}}", GetLink(reviewLink));

                // Set the HTML content as the email body
                message.IsBodyHtml = true;
                message.Body = body.Replace("{{BaseURL}}", baseURL).Replace("{{BaseAppName}}", baseAppName).Replace("{{fromEmail}}", fromEmail); ;

                client.Send(message);

                _logger.LogInformation($"SendReviewCreated: Email sent successfully to {toEmailAddress}");
                return new(true, $"Email sent successfully to {toEmailAddress}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendReviewCreated: Email sending failed: {ex.Message}");
                return new(false, $"Email sending failed: {ex.Message}");
            }
        }

        public (bool, string) SendSupportTicketUpdated(string toEmailAddress, string supportTicketLink)
        {
            try
            {
                using SmtpClient client = new SmtpClient(smtpServer);
                client.Port = smtpPort;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = enableSsl;

                using MailMessage message = new MailMessage();
                message.From = new MailAddress(fromEmail); // Must be a verified email
                message.To.Add(new MailAddress(toEmailAddress));
                message.Subject = "Service Finder - Support Ticket Updated";

                // Read the HTML content from an HTML file
                string htmlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates/SupportTicketUpdated.html");
                string htmlContent = System.IO.File.ReadAllText(htmlFilePath);

                //Replace the  link placeholder
                string body = htmlContent.Replace("{{SupportTicketLink}}", GetLink(supportTicketLink));

                // Set the HTML content as the email body
                message.IsBodyHtml = true;
                message.Body = body.Replace("{{BaseURL}}", baseURL).Replace("{{BaseAppName}}", baseAppName).Replace("{{fromEmail}}", fromEmail); ;

                client.Send(message);

                _logger.LogInformation($"SendSupportTicketCreated: Email sent successfully to {toEmailAddress}");
                return new(true, $"Email sent successfully to {toEmailAddress}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendSupportTicketCreated: Email sending failed: {ex.Message}");
                return new(false, $"Email sending failed: {ex.Message}");
            }
        }

        public (bool, string) SendSupportTicketCreated(string supportTicketLink, string? toEmailAddress = "")
        {
            try
            {
                string recipientEmail;
                if (string.IsNullOrEmpty(toEmailAddress)) {
                    recipientEmail = supportEmail;
                } else
                {
                    recipientEmail = toEmailAddress;
                }

                    using SmtpClient client = new SmtpClient(smtpServer);
                client.Port = smtpPort;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = enableSsl;

                using MailMessage message = new MailMessage();
                message.From = new MailAddress(fromEmail); // Must be a verified email
                message.To.Add(new MailAddress(recipientEmail));
                message.Subject = "Service Finder - Support Ticket Created";

                // Read the HTML content from an HTML file
                string htmlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates/SupportTicketCreated.html");
                string htmlContent = System.IO.File.ReadAllText(htmlFilePath);

                //Replace the  link placeholder
                string body = htmlContent.Replace("{{SupportTicketLink}}", GetLink(supportTicketLink));

                // Set the HTML content as the email body
                message.IsBodyHtml = true;
                message.Body = body.Replace("{{BaseURL}}", baseURL).Replace("{{BaseAppName}}", baseAppName).Replace("{{fromEmail}}", fromEmail); ;

                client.Send(message);

                _logger.LogInformation($"SendSupportTicketCreated: Email sent successfully to {toEmailAddress}");
                return new(true, $"Email sent successfully to {toEmailAddress}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendSupportTicketCreated: Email sending failed: {ex.Message}");
                return new(false, $"Email sending failed: {ex.Message}");
            }
        }

        public (bool, string) SendMessageNotification(string messageThread, string? toEmailAddress = "")
        {
            try
            {
                string recipientEmail;
                if (string.IsNullOrEmpty(toEmailAddress))
                {
                    recipientEmail = supportEmail;
                }
                else
                {
                    recipientEmail = toEmailAddress;
                }

                using SmtpClient client = new SmtpClient(smtpServer);
                client.Port = smtpPort;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = enableSsl;

                using MailMessage message = new MailMessage();
                message.From = new MailAddress(fromEmail); // Must be a verified email
                message.To.Add(new MailAddress(recipientEmail));
                message.Subject = "Service Finder - New Message";

                // Read the HTML content from an HTML file
                string htmlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates/MessageReceived.html");
                string htmlContent = System.IO.File.ReadAllText(htmlFilePath);

                //Replace the  link placeholder
                string body = htmlContent.Replace("{{MessageThread}}", GetLink(messageThread));

                // Set the HTML content as the email body
                message.IsBodyHtml = true;
                message.Body = body.Replace("{{BaseURL}}", baseURL).Replace("{{BaseAppName}}", baseAppName).Replace("{{fromEmail}}", fromEmail); ;

                client.Send(message);

                _logger.LogInformation($"SendMessageNotification: Email sent successfully to {toEmailAddress}");
                return new(true, $"Email sent successfully to {toEmailAddress}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendMessageNotification: Email sending failed: {ex.Message}");
                return new(false, $"Email sending failed: {ex.Message}");
            }
        }

        public (bool, string) SendConfirmation(string toEmailAddress, string confirmationLink, string type)
        {
            try
            {
                using SmtpClient client = new SmtpClient(smtpServer);
                client.Port = smtpPort;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = enableSsl;

                using MailMessage message = new MailMessage();
                message.From = new MailAddress(fromEmail); // Must be a verified email
                message.To.Add(new MailAddress(toEmailAddress));
                message.Subject = "Service Finder - Confirmation Email";

                // Read the HTML content from an HTML file
                string htmlFilePath;
                string htmlContent;
                string body;
                switch (type)
                {
                    case UserAccountRoles.ServiceProvider:
                        htmlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates/AccountConfirmationProvider.html");
                        htmlContent = System.IO.File.ReadAllText(htmlFilePath);
                        body = htmlContent.Replace("{{ConfirmationLink}}", confirmationLink).Replace("{{VerificationLink}}", GetLink($"/Identity/Account/RegisterConfirmation?email={toEmailAddress}"));
                        break;
                    default:
                        htmlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates/AccountConfirmation.html");
                        htmlContent = System.IO.File.ReadAllText(htmlFilePath);
                        body = htmlContent.Replace("{{ConfirmationLink}}", confirmationLink);
                        break;
                }

                // Set the HTML content as the email body
                message.IsBodyHtml = true;
                message.Body = body.Replace("{{BaseURL}}", baseURL).Replace("{{BaseAppName}}", baseAppName).Replace("{{fromEmail}}", fromEmail); ;

                client.Send(message);

                _logger.LogInformation($"SendConfirmation: Email sent successfully to {toEmailAddress}");
                return new(true, $"Email sent successfully to {toEmailAddress}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendConfirmation: Email sending failed: {ex.Message}");
                return new(false, $"Email sending failed: {ex.Message}");
            }
        }

        public (bool, string) SendInvitation(string toEmailAddress)
        {
            try
            {
                using SmtpClient client = new SmtpClient(smtpServer);
                client.Port = smtpPort;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = enableSsl;

                using MailMessage message = new MailMessage();
                message.From = new MailAddress(fromEmail); // Must be a verified email
                message.To.Add(new MailAddress(toEmailAddress));
                message.Subject = "Invitation to Service Finder";

                // Read the HTML content from an HTML file
                string htmlFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates/AccountInvitation.html");
                string htmlContent = System.IO.File.ReadAllText(htmlFilePath);

                //Replace the registration link placeholder
                string body = htmlContent.Replace("{{RegistrationLink}}", GetLink("/Identity/Account/Register"));

                // Set the HTML content as the email body
                message.IsBodyHtml = true;
                message.Body = body.Replace("{{BaseURL}}", baseURL).Replace("{{BaseAppName}}", baseAppName).Replace("{{fromEmail}}", fromEmail); ;

                client.Send(message);

                _logger.LogInformation($"SendInvitation: Email sent successfully to {toEmailAddress}");
                return new(true, $"Invitation sent successfully to {toEmailAddress}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendInvitation: Email sending failed: {ex.Message}");
                return new(false, $"Email sending failed: {ex.Message}");
            }
        }
    }
}
