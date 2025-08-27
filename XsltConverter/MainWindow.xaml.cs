using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml;
using XsltConverter.Classes;

namespace XsltConverter
{
    public enum Month
    {
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12,
        Unknown = 13,
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region ПОЛЯ И СВОЙСТВА

        /// <summary>
        /// Лист сотрудников
        /// </summary>
        public ObservableCollection<Employee> ListAllEmployees { get; set; } = [];

        /// <summary>
        /// Словарь месяцев
        /// </summary>
        public Dictionary<Month, ObservableCollection<Employee>> DictionaryMonths = [];

        /// <summary>
        /// Выбранный файл (путь, название, формат)
        /// </summary>
        public string? PathAndNameFile { get; set; }

        /// <summary>
        /// Имя и расширение файла
        /// </summary>
        public string? NameFile
        {
            get => _NameFile;
            set
            {
                _NameFile = value;
                OnPropertyChanged();
            }
        }
        private string? _NameFile = string.Empty;

        #region КОМАНДЫ

        public RaiseCommand? SelectFileCommand { get; set; }
        public RaiseCommand? OpenFileCommand { get; set; }

        #endregion

        #endregion

        #region КОНСТРУКТОР

        public MainWindow()
        {
            InitializeComponent();

            LoadCommands();

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
        /// Загрузка команд
        /// </summary>
        public void LoadCommands()
        {
            SelectFileCommand = new RaiseCommand(SelectFileCommand_Execute);
            OpenFileCommand = new RaiseCommand(OpenFileCommand_Execute, OpenFileCommand_CanExecute);
        }

        /// <summary>
        /// Выполнить команду "Выбрать файл"
        /// </summary>
        /// <param name="parameter"></param>
        private void SelectFileCommand_Execute(object parameter)
        {
            SelectFile();
            OpenXmlFile();
        }

        /// <summary>
        /// Выбор файла
        /// </summary>
        /// <returns>Путь с названием файла и его расширением</returns>
        public void SelectFile()
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.ShowDialog();

            NameFile = openFileDialog.SafeFileName;

            PathAndNameFile = openFileDialog.FileName;
        }

        /// <summary>
        /// Открыть и прочитать XML файл
        /// </summary>
        public void OpenXmlFile()
        {
            ListAllEmployees.Clear();

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(PathAndNameFile!);

            XmlElement? RootObject = xmlDocument.DocumentElement;

            if (RootObject != null)
            {
                foreach (XmlElement node in RootObject)
                {
                    XmlNode? name = node.Attributes.GetNamedItem(nameof(name));
                    string nameString = name?.Value ?? string.Empty;

                    XmlNode? surname = node.Attributes.GetNamedItem(nameof(surname));
                    string surnameString = surname?.Value ?? string.Empty;

                    XmlNode? amount = node.Attributes.GetNamedItem(nameof(amount));
                    var amountDouble = double.TryParse(amount?.Value, out double outAmount) ? outAmount : 0;

                    XmlNode? mount = node.Attributes.GetNamedItem(nameof(mount));
                    string mountString = mount?.Value ?? string.Empty;
                    Month mountEnum = Enum.TryParse(mountString, out Month outMount) ? outMount : Month.Unknown;

                    Employee employee = new Employee(newName: nameString,
                                                     newSurName: surnameString,
                                                     newAmount: amountDouble,
                                                     newMount: mountEnum);

                    ListAllEmployees.Add(employee);
                }
            }
        }

        /// <summary>
        /// Выполнить команду "открыть файл"
        /// </summary>
        /// <param name="parameter"></param>
        private void OpenFileCommand_Execute(object parameter)
        {
            OpenXmlFile();
        }

        /// <summary>
        /// Проверка команды "Открыть файл"
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private bool OpenFileCommand_CanExecute(object parameter)
        {
            return !string.IsNullOrEmpty(PathAndNameFile);
        }

        #endregion
    }
}