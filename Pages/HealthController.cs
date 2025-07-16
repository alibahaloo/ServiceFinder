using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

namespace ServiceFinder.Pages
{
    public class HealthData
    {
        public string Status { get; set; } = default!;
        public string Message { get; set; } = default!;
    }

    [Route("health")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;
        public HealthData HealthData { get; set; } = new HealthData();
        public HealthController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        [HttpGet]
        public async Task<IActionResult> CheckHealthAsync()
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();

            switch (healthReport.Status)
            {
                case HealthStatus.Unhealthy:
                    HealthData.Status = "Healthy";
                    HealthData.Message = "Service is unhealthy";
                    break;
                case HealthStatus.Degraded:
                    HealthData.Status = "Degraded";
                    HealthData.Message = "Service is degraded";
                    break;
                case HealthStatus.Healthy:
                    HealthData.Status = "Healthy";
                    HealthData.Message = "Service is up and running";
                    break;
                default:
                    break;
            }

            // Serialize the data to JSON
            var json = JsonConvert.SerializeObject(HealthData);

            // Return the JSON response with a 200 status code
            return Content(json, "application/json");
        }
    }
}
