using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XsltConverter.Classes
{
    public class Employee 
    {
        public string Name { get; set; }
        public string SurName { get; set; }
        public double Amount { get; set; }
        public Month Mount { get; set; } 

        public Employee(string newName, string newSurName, double newAmount, Month newMount) 
        {
            Name = newName;
            SurName = newSurName;
            Amount = newAmount;
            Mount = newMount;
        }
    }
}
