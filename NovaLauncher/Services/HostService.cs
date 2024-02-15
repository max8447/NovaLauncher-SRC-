using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Mvvm.Contracts;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;
using Wpf.Ui.Mvvm.Services;
using NovaLauncher.Models;

namespace NovaLauncher.Services
{
    public class HostService
    {
        public static readonly IHost _host = Host
          .CreateDefaultBuilder()
          .ConfigureAppConfiguration(c => {
              c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location));
          })
          .ConfigureServices((context, services) => {
              services.AddHostedService<ApplicationHostService>();
              services.AddSingleton<IPageService, PageService>();
              services.AddSingleton<IThemeService, ThemeService>();
              services.AddSingleton<ITaskBarService, TaskBarService>();
              services.AddSingleton<INavigationService, NavigationService>();
              services.AddScoped<INavigationWindow, Views.Windows.ContentWindow>();
              services.AddScoped<Views.ViewModels.ContentWindowModel>();
              services.AddScoped<Views.Pages.ContentPages.HomePage>();
              services.AddScoped<Views.Pages.ContentPages.LibraryPage>();
              services.AddScoped<Views.Pages.ContentPages.DownloadPage>();
              services.AddScoped<Views.Pages.ContentPages.ServerPage>();
              services.AddScoped<Views.Pages.ContentPages.StorePage>();
              services.AddScoped<Views.Pages.ContentPages.LeaderboardPage>();
              services.AddScoped<Views.Pages.ContentPages.NewsPage>();
              services.AddScoped<Views.Pages.ContentPages.SettingsPage>();

              services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
          }).Build();
    }
}
