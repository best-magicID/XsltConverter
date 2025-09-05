namespace XsltConverter.Models
{
    /// <summary>
    /// Базовый класс Работник
    /// </summary>
    public class BaseEmployee
    {
        /// <summary>
        /// Имя работника
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Фамилия работника
        /// </summary>
        public string SurName { get; set; }


        public BaseEmployee(string newName, string newSurName)
        {
            Name = newName;
            SurName = newSurName;
        }
    }
}
