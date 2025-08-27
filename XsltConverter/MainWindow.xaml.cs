using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using XsltConverter.Classes;

namespace XsltConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region ПОЛЯ И СВОЙСТВА

        public ObservableCollection<Employee> ListEmployees { get; set; } = [];

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
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(nameFile);

            XmlElement? RootObject = xmlDocument.DocumentElement;

            if (RootObject != null)
            {
                foreach (XmlElement node in RootObject)
                {
                    XmlNode? name = node.Attributes.GetNamedItem(nameof(name));
                    XmlNode? surname = node.Attributes.GetNamedItem(nameof(surname));
                    XmlNode? amount = node.Attributes.GetNamedItem(nameof(amount));
                    XmlNode? mount = node.Attributes.GetNamedItem(nameof(mount));


                    Employee employee = new Employee(newName: name == null ? string.Empty : name.Value,
                                                     newSurName: string.Empty,
                                                     newAmount: 0,
                                                     newMount: string.Empty);


                    ListEmployees.Add(employee);
                }
            }
        }

        #endregion
    }
}