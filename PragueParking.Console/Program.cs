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
            //try   TODO: Ended up in catch, not sure why?
            //{
                var (garage, priceList) = ConsoleUI.Initialize();

                bool breaker = true;
                do
                {
                    ConsoleUI.MainMenu(garage, priceList, out breaker);
                }
                while (breaker);
            //}
            //catch (Exception ex)
            //{
            //    AnsiConsole.Write(new Markup($"[red]Error! {ex.Message}[/]"));
            //    AnsiConsole.Console.Input.ReadKey(false);
            //}
        }
    }
}
