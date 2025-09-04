using System.Windows;
using XsltConverter.ViewModels;
using XsltConverter.Views;

namespace XsltConverter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindowView mainWindowView = new MainWindowView()
            {
                DataContext = new MainViewModel()
            };

            mainWindowView.Show();
        }
    }

}
