namespace XsltConverter.Models
{
    /// <summary>
    /// Класс Работник
    /// </summary>
    public class Employee 
    {
        /// <summary>
        /// Имя работника
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Фамилия работника
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// Сумма работника
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Месяц 
        /// </summary>
        public Month Month { get; set; } 

        public Employee(string newName, string newSurName, double newAmount, Month newMount) 
        {
            Name = newName;
            SurName = newSurName;
            Amount = newAmount;
            Month = newMount;
        }
    }
}
