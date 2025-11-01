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
                var (garage, priceList) = ConsoleUI.Initialize();

                bool breaker = true;
                do
                {
                    ConsoleUI.MainMenu(garage, priceList, out breaker);
                }
                while (breaker);
        }
    }
}
