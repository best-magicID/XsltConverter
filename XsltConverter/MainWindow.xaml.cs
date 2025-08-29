using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Xps.Packaging;
using System.Xml;
using System.Xml.Linq;
using XsltConverter.Classes;
using XsltConverter.Windows;

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
        public RaiseCommand? OpenReadChangeFileCommand { get; set; }
        public RaiseCommand? SaveFileCommand { get; set; }
        public RaiseCommand? AddItemInFileCommand { get; set; }

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
            OpenReadChangeFileCommand = new RaiseCommand(OpenReadChangeFileCommand_Execute, OpenReadChangeFileCommand_CanExecute);
            SaveFileCommand = new RaiseCommand(SaveFileCommand_Execute, OpenReadChangeFileCommand_CanExecute);
            AddItemInFileCommand = new RaiseCommand(AddItemInFileCommand_Execute, OpenReadChangeFileCommand_CanExecute);
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
        /// Прочитать XML файл
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

            XmlElement? rootObject = xmlDocument.DocumentElement;

            if (rootObject != null)
            {
                foreach (XmlElement node in rootObject)
                {
                    if(node.Name.ToLower() =="item")
                    {
                        AddInListEmployees(xmlElement: node);
                    }
                    else
                    {
                        var temp = Enum.TryParse(node.Name.ToLower(), out Month outMount) ? outMount : Month.unknown;

                        if (ListMonths.Contains(temp))
                        {
                            foreach (XmlNode childNode in node.ChildNodes)
                            {
                                AddInListEmployees(xmlNode: childNode);
                            }
                        }
                    }
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

            XmlNode? month = node.Attributes.GetNamedItem(nameof(month)) ?? node.Attributes.GetNamedItem("mount");
            string mountString = month?.Value ?? string.Empty;
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
            switch (employee.Month)
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
        /// Выполнить команду "Обработать файл"
        /// </summary>
        /// <param name="parameter"></param>
        private void OpenReadChangeFileCommand_Execute(object parameter)
        {
            try
            {
                ReadXmlFile();
                ConvertData();
                ChangeSourceFile();
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
        private bool OpenReadChangeFileCommand_CanExecute(object parameter)
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

                        XAttribute monthAttr = new XAttribute("month", item.Month);
                        salaryElement.Add(monthAttr);

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

        /// <summary>
        /// Изменить исходный XML файл. 
        /// Добавление общей суммы
        /// </summary>
        public void ChangeSourceFile()
        {
            if (!string.IsNullOrEmpty(PathAndNameFile))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(PathAndNameFile);

                XmlElement? rootObject = xmlDocument.DocumentElement;

                if (rootObject != null)
                {
                    if (rootObject.Name.ToLower() == "pay")
                    {
                        double allamount = ListAllEmployees.Sum(x => x.Amount);
                        XmlAttribute xmlAttribute = xmlDocument.CreateAttribute(nameof(allamount));
                        xmlAttribute.Value = allamount.ToString();
                        rootObject.Attributes.Append(xmlAttribute);

                        WriteTextInXml(xmlDocument);
                    }
                }
            }
        }

        /// <summary>
        /// Выполнить команду "Добавление нового item в файл"
        /// </summary>
        /// <param name="parameter"></param>
        private void AddItemInFileCommand_Execute(object parameter)
        {
            try
            {
                AddItemInFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка:" + ex.ToString(),
                                "Внимание",
                                MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Добавление нового item в файл
        /// </summary>
        public void AddItemInFile()
        {
            WindowAddItemInFile windowAddItemInFile = new WindowAddItemInFile(ListMonths);
            windowAddItemInFile.ShowDialog();

            if(!windowAddItemInFile.IsAdd)
            {
                return;
            }

            if (!string.IsNullOrEmpty(PathAndNameFile))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(PathAndNameFile);

                XmlElement? rootObject = xmlDocument.DocumentElement;

                if (rootObject != null)
                {
                    if (rootObject.Name.ToLower() == "pay")
                    {
                        if(rootObject.FirstChild?.Name?.ToLower() == "item")
                        {
                            XmlElement itemElem = xmlDocument.CreateElement("item");

                            XmlAttribute nameAttr = xmlDocument.CreateAttribute("name");
                            nameAttr.Value = windowAddItemInFile.NewEmployee.Name;
                            itemElem.Attributes.Append(nameAttr);

                            XmlAttribute surNmeAttr = xmlDocument.CreateAttribute("surname");
                            surNmeAttr.Value = windowAddItemInFile.NewEmployee.SurName;
                            itemElem.Attributes.Append(surNmeAttr);

                            XmlAttribute amountAttr = xmlDocument.CreateAttribute("amount");
                            amountAttr.Value = windowAddItemInFile.NewEmployee.Amount.ToString();
                            itemElem.Attributes.Append(amountAttr);

                            XmlAttribute monthAttr = xmlDocument.CreateAttribute("month");
                            monthAttr.Value = windowAddItemInFile.NewEmployee.Month.ToString();
                            itemElem.Attributes.Append(monthAttr);

                            rootObject.AppendChild(itemElem);

                            WriteTextInXml(xmlDocument);
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Запись изменений в файл с табуляцией
        /// </summary>
        /// <param name="xmlDocument"></param>
        public void WriteTextInXml(XmlDocument xmlDocument)
        {
            if (string.IsNullOrEmpty(PathAndNameFile))
            {
                MessageBox.Show("Не указан путь или файл", 
                                "Внимание", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
                return;
            }

            using XmlTextWriter xmlTextWriter = new XmlTextWriter(PathAndNameFile, Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlTextWriter.IndentChar = '\t';
            xmlTextWriter.Indentation = 1;

            xmlDocument.WriteTo(xmlTextWriter);
        }

        #endregion
    }
}