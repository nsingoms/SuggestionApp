namespace SuggestionAppUI
{
    public static class RegisterService
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {   // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddMemoryCache();
        }
    }
}
