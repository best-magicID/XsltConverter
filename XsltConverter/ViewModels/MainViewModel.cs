using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using XsltConverter.Models;
using XsltConverter.Windows;

namespace XsltConverter.ViewModels
{
    public class MainViewModel: INotifyPropertyChanged
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

        public MainViewModel()
        {
            LoadCommands();

            ListMonths = Enum.GetValues(typeof(Month)).Cast<Month>().ToList();
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
                if (SelectFile())
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

            if (!isOpen)
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
            if (string.IsNullOrEmpty(PathAndNameFile))
            {
                return;
            }

            XDocument xmlDocument = XDocument.Load(PathAndNameFile);

            var list = xmlDocument.Element("Pay")?
                                  .Descendants()
                                  .Where(node => node.Name == "item")
                                  .Select(node => new Employee
                                  (
                                      newName: node.Attribute("name")?.Value ?? string.Empty,
                                      newSurName: node.Attribute("surname")?.Value ?? string.Empty,
                                      newAmount: double.TryParse(node.Attribute("amount")?.Value.Replace(".", ","), out double outAmount) ? outAmount : 0,
                                      newMonth: Enum.TryParse(node.Attribute("mount")?.Value ?? node.Attribute("month")?.Value, out Month outMonth) ? outMonth : Month.unknown
                                  ))
                                  .ToList();

            if (list != null)
            {
                ListAllEmployees.Clear();

                list.ForEach(x => ListAllEmployees.Add(x));
            }
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
                if (!ListEmployeesInfoForYear.Any(x => x.Name == employee.Name && x.SurName == employee.SurName))
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
                    if (employeeInfoForYear.Name == employee.Name && employeeInfoForYear.SurName == employee.SurName)
                    {
                        AddInListEmployeesInfoForYear(employee, employeeInfoForYear);
                    }
                }
            }

            // Подсчет годовой суммы
            foreach (var employeeInfoForYear in ListEmployeesInfoForYear)
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
        private void SaveFileCommand_Execute(object parameter)
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
            saveFileDialog.FileName = "Employees " + DateTime.Now.ToString("dd.MM.yyyy HH-mm") + ".xml";

            if (!saveFileDialog.ShowDialog() ?? false)
            {
                return;
            }

            XDocument xDocument = new();

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
            xDocument.Add(rootEmployees);

            WriteToXml(xDocument, saveFileDialog.FileName);
        }

        /// <summary>
        /// Запись в XML с табуляцией
        /// </summary>
        /// <param name="xDocument"></param>
        /// <param name="text"></param>
        public void WriteToXml(XDocument xDocument, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Не указан путь или файл",
                                "Внимание",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            using XmlTextWriter xmlTextWriter = new XmlTextWriter(text, Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlTextWriter.IndentChar = '\t';
            xmlTextWriter.Indentation = 1;

            xDocument.WriteTo(xmlTextWriter);

            MessageBox.Show($"XML файл сохранен: \r\n{text}",
                            "Внимание",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        /// <summary>
        /// Изменение исходного XML файла. 
        /// Добавление общей суммы.
        /// </summary>
        public void ChangeSourceFile()
        {
            if (!string.IsNullOrEmpty(PathAndNameFile))
            {
                XDocument xDocument = XDocument.Load(PathAndNameFile);

                var rootNode = xDocument.Element("Pay");
                if (rootNode != null)
                {
                    var allAmount = ListAllEmployees.Sum(x => x.Amount).ToString();

                    var attr = rootNode.Attribute("allamount");
                    if (attr != null)
                    {
                        attr.Value = allAmount;
                    }
                    else
                    {
                        XAttribute allAmountAttr = new XAttribute("allamount", allAmount);
                        rootNode.Add(allAmountAttr);
                    }
                    xDocument.Save(PathAndNameFile);
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

            if (!windowAddItemInFile.IsAdd)
            {
                return;
            }

            if (!string.IsNullOrEmpty(PathAndNameFile))
            {
                XDocument xDocument = XDocument.Load(PathAndNameFile);
                var rootNode = xDocument.Element("Pay");

                if (rootNode != null)
                {
                    rootNode.Add(new XElement("item",
                                              new XAttribute("name", windowAddItemInFile.NewEmployee.Name),
                                              new XAttribute("surname", windowAddItemInFile.NewEmployee.SurName),
                                              new XAttribute("amount", windowAddItemInFile.NewEmployee.Amount.ToString()),
                                              new XAttribute("month", windowAddItemInFile.NewEmployee.Month.ToString())));

                    xDocument.Save(PathAndNameFile);
                }
            }
        }

        #endregion
    }
}
