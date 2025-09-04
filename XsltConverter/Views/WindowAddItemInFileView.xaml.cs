using System.Windows;
using XsltConverter.ViewModels;

namespace XsltConverter.Views
{
    /// <summary>
    /// Логика взаимодействия
    /// </summary>
    public partial class WindowAddItemInFileView : Window
    {
        #region КОНСТРУКТОР

        public WindowAddItemInFileView()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                if (DataContext is WindowAddItemInFileViewModel viewModel)
                {
                    viewModel.RequestClose += () => this.Close();
                }
            };
        }

        #endregion
    }
}
