using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using XsltConverter.Classes;

namespace XsltConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region ПОЛЯ И СВОЙСТВА

        public ObservableCollection<Employee> ListEmployees { get; set; } = [];
        public 

        public RaiseCommand? SelectFileCommand { get; set; }

        #endregion


        #region Конструктор

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
        }

        /// <summary>
        /// Выполнить команду "Выбрать файл"
        /// </summary>
        /// <param name="parameter"></param>
        private void SelectFileCommand_Execute(object parameter)
        {
            var nameFile = SelectFile();
            OpenXmlFile(nameFile);
        }

        /// <summary>
        /// Выбор файла
        /// </summary>
        /// <returns>Путь с названием файла и его расширением</returns>
        public string SelectFile()
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.ShowDialog();

            return openFileDialog.FileName;
        }

        public void OpenXmlFile(string nameFile)
        {
            ListEmployees.Clear();

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(nameFile);

            XmlElement? RootObject = xmlDocument.DocumentElement;

            if (RootObject != null)
            {
                foreach (XmlElement node in RootObject)
                {
                    XmlNode? name = node.Attributes.GetNamedItem(nameof(name));
                    string nameString = name != null ? name.Value : string.Empty;

                    XmlNode? surname = node.Attributes.GetNamedItem(nameof(surname));
                    string surnameString = surname != null ? surname.Value : string.Empty;

                    XmlNode? amount = node.Attributes.GetNamedItem(nameof(amount));
                    var amountDouble = double.TryParse(amount?.Value, out double result) ? result : 0;

                    XmlNode? mount = node.Attributes.GetNamedItem(nameof(mount));
                    string mountString = mount != null ? mount.Value : string.Empty;

                    Employee employee = new Employee(newName: nameString,
                                                     newSurName: surnameString,
                                                     newAmount: amountDouble,
                                                     newMount: mountString);

                    ListEmployees.Add(employee);
                }
            }
        }

        #endregion
    }
}