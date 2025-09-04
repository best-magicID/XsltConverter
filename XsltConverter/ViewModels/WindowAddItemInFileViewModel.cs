using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using XsltConverter.Models;

namespace XsltConverter.ViewModels
{
    public class WindowAddItemInFileViewModel : INotifyPropertyChanged
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


        public RaiseCommand AddItemCommand { get; set; }

        public event Action? RequestClose;

        #endregion

        #region КОНСТРУКТОР

        public WindowAddItemInFileViewModel(List<Month> newListMonths)
        {
            newListMonths.ForEach(x => ListMonths.Add(x));

            AddItemCommand = new RaiseCommand(AddItemCommand_Execute);
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

        private void AddItemCommand_Execute(object parameter)
        {
            AddItem();
        }

        /// <summary>
        /// Добавление нового item
        /// </summary>
        private void AddItem()
        {
            if (NewEmployee.Name != string.Empty && NewEmployee.SurName != string.Empty)
            {
                IsAdd = true;
                OnClose();
                return;
            }

            MessageBox.Show("Введите имя и фамилию для добавления",
                            "Внимание",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
        }

        /// <summary>
        /// Закрытие окна
        /// </summary>
        public void OnClose()
        {
            RequestClose?.Invoke();
        }

        #endregion
    }
}
