using System;
using System.IO;
using PragueParking.Core;
using PragueParking.Data;
using Spectre.Console;

namespace PragueParking.Console
{
    public class Program
    {
        private static void Main(string[] args)
        {


            bool breaker = true;
            do
            {
                var (garage, priceList) = ConsoleUI.Initialize();
                ConsoleUI.MainMenu(garage, priceList, out breaker);
            }
            while (breaker);
        }
    }
}
