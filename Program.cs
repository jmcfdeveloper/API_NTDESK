using PuppeteerSharp;
using System.Diagnostics;

namespace PdfGeneratorApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await InitializePuppeteerAsync();

            // Abre el navegador con la URL deseada
            OpenBrowser("http://localhost:5000/swagger/index.html");

            await host.RunAsync();
        }

        private static async Task InitializePuppeteerAsync()
        {
            var launchOptions = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe" // Ruta al ejecutable de Chrome
            };

            await Puppeteer.LaunchAsync(launchOptions);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://localhost:5000", "https://localhost:7204");
                    webBuilder.UseStartup<Startup>();
                });

        private static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al abrir el navegador: {ex.Message}");
            }
        }
    }
}
