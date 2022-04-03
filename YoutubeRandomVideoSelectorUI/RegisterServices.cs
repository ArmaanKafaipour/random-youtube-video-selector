using YoutubeRandomVideoLibrary;

namespace YoutubeRandomVideoSelectorUI;

public static class RegisterServices
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();

        builder.Services.AddScoped<Helper>();
    }

}
