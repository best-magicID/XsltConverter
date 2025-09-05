namespace XsltConverter.Models
{
    /// <summary>
    /// Класс Работник для импорта из XML
    /// </summary>
    public class Employee : BaseEmployee
    {
        /// <summary>
        /// Сумма работника 
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Месяц 
        /// </summary>
        public Month Month { get; set; } 


        public Employee(string newName, string newSurName, double newAmount, Month newMonth) 
            : base(newName, newSurName)
        {
            Amount = newAmount;
            Month = newMonth;
        }
    }
}
