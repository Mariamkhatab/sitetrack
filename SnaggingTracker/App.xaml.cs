using SnaggingTracker.Services;
using SnaggingTracker.ViewModels;
using SnaggingTracker.Views;
using System.Windows;

namespace SnaggingTracker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Manual DI composition root — no IoC container needed for this scale
            IDataService        dataService  = new JsonDataService();
            IExcelExportService excelService = new ExcelExportService();
            var viewModel = new MainViewModel(dataService, excelService);

            var mainWindow = new MainView { DataContext = viewModel };
            mainWindow.Show();
        }
    }
}
