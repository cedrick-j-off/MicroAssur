using MicroAssur.Services;

namespace MicroAssur;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

#if ANDROID
        var apiBaseAddress = "http://10.0.2.2:5080/";
#else
        var apiBaseAddress = "http://localhost:5080/";
#endif
        builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(apiBaseAddress) });
        builder.Services.AddSingleton<PaymentApiClient>();

        return builder.Build();
    }
}
