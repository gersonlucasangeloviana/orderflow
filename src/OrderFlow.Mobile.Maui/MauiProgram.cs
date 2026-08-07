using Microsoft.Extensions.Logging;
using OrderFlow.Mobile.Maui.Services;

namespace OrderFlow.Mobile.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri("https://api.example.com/") });
        builder.Services.AddSingleton<OrderFlowApiClient>();
        builder.Services.AddSingleton<MainPage>();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
