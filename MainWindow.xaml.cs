using System.Windows;
using SpectrumAnalyzer.ViewModels;

namespace SpectrumAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Dispose ViewModel when window closes
            if (DataContext is IDisposable disposableViewModel)
            {
                disposableViewModel.Dispose();
            }
            
            base.OnClosed(e);
        }
    }
}