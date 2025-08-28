using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XsltConverter.Classes
{
    public class EmployeeInfoForYear : INotifyPropertyChanged
    {
        public string Name { get; set; }

        public string SurName { get; set; }

        public double AmountForJanuary { get; set; } 
        public ObservableCollection<Employee> ListForJanuary { get; set; } = [];

        public double AmountForFebruary { get; set; }
        public ObservableCollection<Employee> ListForFebruary { get; set; } = [];

        public double AmountForMarch { get; set; }
        public ObservableCollection<Employee> ListForMarch { get; set; } = [];

        public double AmountForApril { get; set; }
        public ObservableCollection<Employee> ListForApril { get; set; } = [];

        public double AmountForMay { get; set; }
        public ObservableCollection<Employee> ListForMay { get; set; } = [];

        public double AmountForJune { get; set; }
        public ObservableCollection<Employee> ListForJune { get; set; } = [];

        public double AmountForJuly { get; set; }
        public ObservableCollection<Employee> ListForJuly { get; set; } = [];

        public double AmountForAugust { get; set; }
        public ObservableCollection<Employee> ListForAugust { get; set; } = [];

        public double AmountForSeptember { get; set; }
        public ObservableCollection<Employee> ListForSeptember { get; set; } = [];

        public double AmountForOctober { get; set; }
        public ObservableCollection<Employee> ListForOctober { get; set; } = [];

        public double AmountForNovember { get; set; }
        public ObservableCollection<Employee> ListForNovember { get; set; } = [];

        public double AmountForDecember { get; set; }
        public ObservableCollection<Employee> ListForDecember { get; set; } = [];

        public double AmountForUnknown { get; set; }
        public ObservableCollection<Employee> ListForUnknown { get; set; } = [];

        public double AmountForYear { get; set; }


        public EmployeeInfoForYear(string newName, string newSurName)
        {
            Name = newName;
            SurName = newSurName; 
        }


        #region ОБНОВЛЕНИЕ UI

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        /// <summary>
        /// Получение списка со всеми месяцами сотрудника
        /// </summary>
        /// <returns></returns>
        public List<ObservableCollection<Employee>> GetListCollections()
        {
            var list = new List<ObservableCollection<Employee>>();
            list.Add(ListForJanuary);
            list.Add(ListForFebruary);
            list.Add(ListForMarch);
            list.Add(ListForApril);
            list.Add(ListForMay);
            list.Add(ListForJune);
            list.Add(ListForJuly);
            list.Add(ListForAugust);
            list.Add(ListForSeptember);
            list.Add(ListForOctober);
            list.Add(ListForDecember);

            return list;
        }
        
        /// <summary>
        /// Получение всей суммы сотрудника
        /// </summary>
        /// <returns></returns>
        public double GetAllAmount()
        {
            double allAmount = AmountForJanuary +
                               AmountForFebruary +
                               AmountForMarch +
                               AmountForApril +
                               AmountForMay +
                               AmountForJune +
                               AmountForJuly +
                               AmountForAugust +
                               AmountForSeptember +
                               AmountForOctober +
                               AmountForDecember;

            return Math.Round(allAmount, 2);
        }

    }
}
