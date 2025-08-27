using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml;
using XsltConverter.Classes;

namespace XsltConverter
{
    public enum Month
    {
        january = 1,
        february = 2,
        march = 3,
        april = 4,
        may = 5,
        june = 6,
        july = 7,
        august = 8,
        september = 9,
        october = 10,
        november = 11,
        december = 12,
        unknown = 13,
    }

    /// <summary>
    /// Логика для главного окна
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

        public List<Month> ListMonths { get; set; } = new List<Month>();

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

            ListMonths = Enum.GetValues(typeof(Month)).Cast<Month>().ToList();

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
            try
            {
                SelectFile();
                ReadXmlFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка:" + ex.ToString(), 
                                "Внимание",
                                MessageBoxButton.OK);
            }
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
        public void ReadXmlFile()
        {
            ListAllEmployees.Clear();

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(PathAndNameFile!);

            XmlElement? RootObject = xmlDocument.DocumentElement;

            if (RootObject != null)
            {
                foreach (XmlElement node in RootObject)
                {
                    var temp = Enum.TryParse(node.Name.ToLower(), out Month outMount0) ? outMount0 : Month.unknown;

                    if (ListMonths.Contains(temp))
                    {
                        foreach (XmlNode childNode in node.ChildNodes)
                        {
                            AddInListEmployees(xmlNode: childNode);
                        }
                    }

                    AddInListEmployees(xmlElement: node);
                }
            }
        }

        /// <summary>
        /// Добавление сотрудников в список
        /// </summary>
        /// <param name="xmlElement"></param>
        /// <param name="xmlNode"></param>
        public void AddInListEmployees(XmlElement? xmlElement = null, XmlNode? xmlNode = null)
        {
            var node = xmlElement ?? xmlNode;
            if (node == null || node?.Attributes?.Count < 1)
            {
                return;
            }

            XmlNode? name = node.Attributes.GetNamedItem(nameof(name));
            string nameString = name?.Value ?? string.Empty;

            XmlNode? surname = node.Attributes.GetNamedItem(nameof(surname));
            string surnameString = surname?.Value ?? string.Empty;

            XmlNode? amount = node.Attributes.GetNamedItem(nameof(amount));
            var tempAmount = amount?.Value?.ToString().Replace('.', ',');
            var amountDouble = double.TryParse(tempAmount, out double outAmount) ? outAmount : 0;

            XmlNode? mount = node.Attributes.GetNamedItem(nameof(mount));
            string mountString = mount?.Value ?? string.Empty;
            Month mountEnum = Enum.TryParse(mountString.ToLower(), out Month outMount) ? outMount : Month.unknown;

            Employee employee = new Employee(newName: nameString,
                                             newSurName: surnameString,
                                             newAmount: amountDouble,
                                             newMount: mountEnum);

            ListAllEmployees.Add(employee);
        }

        /// <summary>
        /// Выполнить команду "открыть файл"
        /// </summary>
        /// <param name="parameter"></param>
        private void OpenFileCommand_Execute(object parameter)
        {
            try
            {
                ReadXmlFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка:" + ex.ToString(),
                                "Внимание",
                                MessageBoxButton.OK);
            }
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