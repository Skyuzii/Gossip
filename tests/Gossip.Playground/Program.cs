namespace Gossip.Playground;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(
                webHostBuilder =>
                {
                    webHostBuilder.UseStartup<Startup>();
                })
            .ConfigureLogging(static builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));
    }
}