using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
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
        /// Список сотрудников с группировкой и объединением суммы по месяцам
        /// </summary>
        public ObservableCollection<EmployeeInfoForYear> ListEmployeesInfoForYear { get; set; } = [];

        /// <summary>
        /// Список месяцев
        /// </summary>
        public List<Month> ListMonths { get; set; } = [];

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
        public RaiseCommand? SaveFileCommand { get; set; }

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
            SaveFileCommand = new RaiseCommand(SaveFileCommand_Execute, OpenFileCommand_CanExecute);
        }

        /// <summary>
        /// Выполнить команду "Выбрать файл"
        /// </summary>
        /// <param name="parameter"></param>
        private void SelectFileCommand_Execute(object parameter)
        {
            try
            {
                if(SelectFile())
                {
                    ReadXmlFile();
                    ConvertData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка:" + ex.ToString(), 
                                "Внимание",
                                MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Выбор файла (Путь с названием файла и его расширением)
        /// </summary>
        /// <returns>true - если файл выбран</returns>
        public bool SelectFile()
        {
            OpenFileDialog openFileDialog = new();
            var isOpen = openFileDialog.ShowDialog() ?? false; 

            if(!isOpen)
            {
                return false;
            }

            NameFile = openFileDialog.SafeFileName;

            PathAndNameFile = openFileDialog.FileName;

            return true;
        }

        /// <summary>
        /// Открыть и прочитать XML файл
        /// </summary>
        public void ReadXmlFile()
        {
            if(string.IsNullOrEmpty(PathAndNameFile))
            {
                return;
            }    

            ListAllEmployees.Clear();

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(PathAndNameFile);

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
        /// Конвертирование данных
        /// </summary>
        public void ConvertData()
        {
            ListEmployeesInfoForYear.Clear();

            // Получение списка сотрудников без дубликатов
            foreach (var employee in ListAllEmployees)
            {
                if(!ListEmployeesInfoForYear.Any(x => x.Name == employee.Name && x.SurName == employee.SurName))
                {
                    ListEmployeesInfoForYear.Add(new EmployeeInfoForYear(newName: employee.Name,
                                                                         newSurName: employee.SurName));
                }
            }

            // Заполнение данных о сотруднике
            foreach (Employee employee in ListAllEmployees)
            {
                foreach (EmployeeInfoForYear employeeInfoForYear in ListEmployeesInfoForYear)
                {
                    if(employeeInfoForYear.Name == employee.Name && employeeInfoForYear.SurName == employee.SurName)
                    {
                        AddInListEmployeesInfoForYear(employee, employeeInfoForYear);
                    }
                }
            }

            // Подсчет годовой суммы
            foreach(var employeeInfoForYear in ListEmployeesInfoForYear)
            {
                employeeInfoForYear.AmountForYear = employeeInfoForYear.GetAllAmount();
            }
        }

        /// <summary>
        /// Распределение по месяцам
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="employeeInfoForYear"></param>
        public void AddInListEmployeesInfoForYear(Employee employee, EmployeeInfoForYear employeeInfoForYear)
        {
            switch (employee.Mount)
            {
                case Month.january:
                    employeeInfoForYear.ListForJanuary.Add(employee);
                    employeeInfoForYear.AmountForJanuary += employee.Amount;
                    break;

                case Month.february:
                    employeeInfoForYear.ListForFebruary.Add(employee);
                    employeeInfoForYear.AmountForFebruary += employee.Amount;
                    break;

                case Month.march:
                    employeeInfoForYear.ListForMarch.Add(employee);
                    employeeInfoForYear.AmountForMarch += employee.Amount;
                    break;

                case Month.april:
                    employeeInfoForYear.ListForApril.Add(employee);
                    employeeInfoForYear.AmountForApril += employee.Amount;
                    break;

                case Month.may:
                    employeeInfoForYear.ListForMay.Add(employee);
                    employeeInfoForYear.AmountForMay += employee.Amount;
                    break;

                case Month.june:
                    employeeInfoForYear.ListForJune.Add(employee);
                    employeeInfoForYear.AmountForJune += employee.Amount;
                    break;

                case Month.july:
                    employeeInfoForYear.ListForJuly.Add(employee);
                    employeeInfoForYear.AmountForJuly += employee.Amount;
                    break;

                case Month.august:
                    employeeInfoForYear.ListForAugust.Add(employee);
                    employeeInfoForYear.AmountForAugust += employee.Amount;
                    break;

                case Month.september:
                    employeeInfoForYear.ListForSeptember.Add(employee);
                    employeeInfoForYear.AmountForSeptember += employee.Amount;
                    break;

                case Month.october:
                    employeeInfoForYear.ListForOctober.Add(employee);
                    employeeInfoForYear.AmountForOctober += employee.Amount;
                    break;

                case Month.november:
                    employeeInfoForYear.ListForNovember.Add(employee);
                    employeeInfoForYear.AmountForNovember += employee.Amount;
                    break;

                case Month.december:
                    employeeInfoForYear.ListForDecember.Add(employee);
                    employeeInfoForYear.AmountForJanuary += employee.Amount;
                    break;

                default:
                    employeeInfoForYear.ListForUnknown.Add(employee);
                    employeeInfoForYear.AmountForUnknown += employee.Amount;
                    break;
            }
        }

        /// <summary>
        /// Выполнить команду "Открыть файл"
        /// </summary>
        /// <param name="parameter"></param>
        private void OpenFileCommand_Execute(object parameter)
        {
            try
            {
                ReadXmlFile();
                ConvertData();
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

        /// <summary>
        /// Выполнить команду "Сохранить файл"
        /// </summary>
        /// <param name="parameter"></param>
        private void SaveFileCommand_Execute(Object parameter)
        {
            try
            {
                SaveFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка:" + ex.ToString(),
                                "Внимание",
                                MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Сохранение Xml файла
        /// </summary>
        public void SaveFile()
        {
            SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = "Xml файлы|*.xml";
            saveFileDialog.FileName = "Employees " + DateTime.Now.ToString("dd.MM.yyyy hh-mm") + ".xml";
            var isSave = saveFileDialog.ShowDialog() ?? false;

            if(!isSave)
            {
                return;
            }

            XDocument xmlDocument = new XDocument();
            
            XElement rootEmployees = new XElement("Employees"); // Корневой элемент

            foreach (EmployeeInfoForYear employeeInfoForYear in ListEmployeesInfoForYear)
            {
                XElement employeeElement = new XElement(nameof(Employee)); // Сотрудник

                XAttribute nameAttr = new XAttribute("name", employeeInfoForYear.Name); // Атрибут сотрудника
                employeeElement.Add(nameAttr); 

                XAttribute surNameAttr = new XAttribute("surname", employeeInfoForYear.SurName);
                employeeElement.Add(surNameAttr);

                XAttribute allAmountAttr = new XAttribute("allamount", employeeInfoForYear.GetAllAmount());
                employeeElement.Add(allAmountAttr);

                foreach (var listForMonth in employeeInfoForYear.GetListCollections())
                {
                    foreach (Employee item in listForMonth)
                    {
                        XElement salaryElement = new XElement("salary");

                        XAttribute amountAttr = new XAttribute("amount", item.Amount);
                        salaryElement.Add(amountAttr);

                        XAttribute mountAttr = new XAttribute("mount", item.Mount);
                        salaryElement.Add(mountAttr);

                        employeeElement.Add(salaryElement);
                    }
                }
                rootEmployees.Add(employeeElement);
            }
            xmlDocument.Add(rootEmployees);

            using (XmlTextWriter xmlTextWriter = new XmlTextWriter(saveFileDialog.FileName + ".xml", Encoding.UTF8))
            {
                xmlTextWriter.Formatting = Formatting.Indented;
                xmlTextWriter.IndentChar = '\t';
                xmlTextWriter.Indentation = 1;
                xmlDocument.WriteTo(xmlTextWriter);
            }
        }

        #endregion
    }
}