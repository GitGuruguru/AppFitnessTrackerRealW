using Microsoft.Extensions.Logging;

namespace AppFitnessTrackerReal
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("MuseoModerno-ExtraBold.ttf", "MuseoModernoBold");
                    fonts.AddFont("MuseoModerno-Medium.ttf", "MuseoModernoMedium");
                    fonts.AddFont("Megrim-Regular.ttf", "Megrim");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
