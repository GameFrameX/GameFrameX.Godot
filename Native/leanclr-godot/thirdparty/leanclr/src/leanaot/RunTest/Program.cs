using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            App.s_logger = Console.WriteLine;
            App.Main();
        }
    }
}
