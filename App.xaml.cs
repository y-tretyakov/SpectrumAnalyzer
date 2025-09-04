using System.Windows;

namespace SpectrumAnalyzer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Initialize main window
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}