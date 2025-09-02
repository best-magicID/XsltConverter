using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using XsltConverter.Models;

namespace XsltConverter.Windows
{
    /// <summary>
    /// Логика взаимодействия
    /// </summary>
    public partial class WindowAddItemInFile : Window, INotifyPropertyChanged
    {
        #region ПОЛЯ И СВОЙСТВА

        /// <summary>
        /// Флаг, означающий, добавить ли item в файл
        /// </summary>
        public bool IsAdd { get; set; } = false;

        /// <summary>
        /// Список месяцев
        /// </summary>
        public ObservableCollection<Month> ListMonths { get; set; } = [];

        /// <summary>
        /// Новый item
        /// </summary>
        public Employee NewEmployee 
        {
            get => _NewEmployee;
            set
            {
                _NewEmployee = value;
                OnPropertyChanged();
            }
        }
        private Employee _NewEmployee = new Employee(string.Empty, string.Empty, 0, Month.january);

        /// <summary>
        /// Выбранный месяц
        /// </summary>
        public Month SelectedMonth { get; set; } = Month.january;

        #endregion

        #region КОНСТРУКТОР

        public WindowAddItemInFile(List<Month> newListMonths)
        {
            InitializeComponent();

            foreach (Month month in newListMonths) 
            {
                ListMonths.Add(month);
            }

            DataContext = this;
        }

        #endregion

        #region ОБНОВЛЕНИЕ UI

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region МЕТОДЫ

        /// <summary>
        /// Добавление нового item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (NewEmployee.Name != string.Empty && NewEmployee.SurName != string.Empty)
            {
                IsAdd = true;
                Close();
                return;
            }

            MessageBox.Show("Введите имя и фамилию для добавления", 
                            "Внимание", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Warning);
        }

        #endregion

    }
}
