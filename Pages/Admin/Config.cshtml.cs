namespace ServiceFinder.Pages.Admin
{
    public class ConfigModel : AdminPageModel
    {
        public string Env { get; set; } = string.Empty;
        public void OnGet()
        {
            Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "NOT SET";
        }
    }
}
