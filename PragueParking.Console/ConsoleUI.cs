using System;
using System.Collections.Generic;
using Spectre.Console;
using PragueParking.Core;
using PragueParking.Data;
using System.Collections.Specialized;
using PragueParking.Core.VehicleTypes;
using PragueParking.Core.Interfaces;

namespace PragueParking.Console
{
    public class ConsoleUI
    {

        public static void MainMenu(out bool breaker)
        {
            var (garage, priceList) = Initialize();

            FileManager fileManager = new FileManager();
            breaker = true;
            WritePanel("MAIN MENU", "#ff00ff", "#5fffd7");

            List<string> menuOptions = new List<string>
            {
                "[springgreen1]Park Vehicle[/]\n",
                "[springgreen1]Check Out Vehicle[/]\n",
                "[springgreen1]Search for Vehicle[/]\n",
                "[springgreen1]Move Vehicle[/]\n",
                "[springgreen1]Parking Overview[/]\n",
                "[springgreen1]Reload Price List[/]\n",
                "[springgreen1]Close Prague Parking[/]\n"
            };

            string menuSelect = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("[#ff00ff]\n\nSelect in the menu using the arrow keys\n[/]")
                .AddChoices(menuOptions)
                );

            string cleanSelect = Markup.Remove(menuSelect).Trim();
            AnsiConsole.Clear();
            switch (cleanSelect)
            {
                case "Park Vehicle":
                    {
                        WritePanel("PARK VEHICLE", "#ff00ff", "#5fffd7");
                        IParkable? vehicleToPark = SelectVehicleType(garage.MCVehicleSize, garage.CarVehicleSize, garage.BusVehicleSize, garage.BicycleVehicleSize,
                                                                        garage.AllowedVehicles, priceList);
                        if (vehicleToPark == null)
                        {
                            break;
                        }
                        else
                        {
                            bool parked = garage.ParkVehicle(vehicleToPark, out int parkedSpace);
                            if (parked)
                            {
                                AnsiConsole.Write(new Markup("\n[#5fffd7]Vehicle successfully parked.[/]\n\n"));
                                AnsiConsole.Write(new Markup(garage.GetParkingSpace(parkedSpace).ToString(), Color.Magenta1));

                                fileManager.SaveParkingData(garage.GetAllSpaces(), "../../../parkingdata.json");
                            }
                            else
                            {
                                AnsiConsole.Write(new Markup("\n[aquamarine1]No free space available.\nWelcome back another time.[/]\n\n"));
                            }
                        }
                        break;
                    }
                case "Check Out Vehicle":
                    {
                        WritePanel("CHECK OUT VEHICLE", "#ff00ff", "#5fffd7");
                        string regNumber = CollectRegNumber();
                        if (string.IsNullOrEmpty(regNumber))    // User selected Main Menu
                        {
                            break;
                        }
                        IParkingSpace space = garage.FindVehicleSpace(regNumber);
                        if (space == null)
                        {
                            AnsiConsole.Write(new Markup("\n\nError! Vehicle not found.", Color.Aquamarine1));
                            break;
                        }
                        IParkable vehicle = space.FindVehicleInSpace(regNumber);
                        if (vehicle == null)
                        {
                            AnsiConsole.Write(new Markup("\n\nError! Vehicle not found.", Color.Aquamarine1));
                            break;
                        }
                        // Need to cast to Vehicle, as I had to update methods in ParkingSpace
                        Vehicle concreteVehicle = (Vehicle)vehicle;
                        space.RemoveVehicle(concreteVehicle);
                        AnsiConsole.Write(new Markup("\nVehicle successfully checked out.\n", Color.Aquamarine1));
                        AnsiConsole.Write(new Markup(vehicle.PrintParkingReceipt(), Color.Aquamarine1));        //TODO: Change to print in panel

                        //Update parkingdata file here - no earlier in case check out fails
                        fileManager.SaveParkingData(garage.GetAllSpaces(), "../../../parkingdata.json");
                        break;
                    }
                case "Search for Vehicle":
                    {
                        WritePanel("SEARCH FOR VEHICLE", "#ff00ff", "#5fffd7");
                        string regNumber = CollectRegNumber();
                        if (string.IsNullOrEmpty(regNumber))    // User selected Main Menu
                        {
                            break;
                        }
                        IParkingSpace space = garage.FindVehicleSpace(regNumber);
                        if (space == null)
                        {
                            AnsiConsole.Write(new Markup("\n\nError! Vehicle not found.", Color.Aquamarine1));
                            break;
                        }
                        IParkable vehicle = space.FindVehicleInSpace(regNumber);
                        if (vehicle == null)
                        {
                            AnsiConsole.Write(new Markup("\n\nError! Vehicle not found.", Color.Aquamarine1));
                            break;
                        }

                        AnsiConsole.Write(new Markup(vehicle.ToString(), Color.Aquamarine1));
                        break;
                    }
                case "Move Vehicle":
                    {
                        WritePanel("MOVE VEHICLE", "#ff00ff", "#5fffd7");
                        // Find vehicle by regnumber
                        string regNumber = CollectRegNumber();
                        if (string.IsNullOrEmpty(regNumber))    // User selected Main Menu
                        {
                            break;
                        }
                        // Save space number vehicle is in - space.SpaceNumber
                        IParkingSpace space = garage.FindVehicleSpace(regNumber);
                        if (space == null)
                        {

                            AnsiConsole.Write(new Markup("\n\nError! Vehicle not found.", Color.Aquamarine1));
                            break;

                        }
                        // "Get" vehicle from space
                        IParkable vehicle = space.FindVehicleInSpace(regNumber);
                        if (vehicle == null)
                        {
                            AnsiConsole.Write(new Markup("\n\nError! Vehicle not found.", Color.Aquamarine1));
                            break;
                        }
                        // Try to park in new space - need space number
                        string spaceNumberString = AnsiConsole.Prompt(new TextPrompt<string>("[#ff00ff]\nEnter parking space to move vehicle to:[/]")
                            .AllowEmpty()
                            .Validate(input =>
                            {
                                if (string.IsNullOrWhiteSpace(input))
                                {
                                    return ValidationResult.Error($"[springgreen1]\n\nError! Please enter a number from 1 to {garage.GarageSize}.[/]");
                                }

                                if (Convert.ToInt32(input) < 1 || Convert.ToInt32(input) > garage.GarageSize)
                                { return ValidationResult.Error($"[springgreen1]\n\nError! Parking spaces are numbered from 1 to {garage.GarageSize}.[/]"); }

                                return ValidationResult.Success();     // default case
                            })
                            );
                        int spaceNumber = int.Parse(spaceNumberString);
                        IParkingSpace spaceMoveTo = garage.GetParkingSpace(spaceNumber - 1);     // minus 1 for correct index
                        // Try to add the vehicle from above to new parking space
                        Vehicle concreteVehicle = (Vehicle)vehicle;
                        bool isParked = spaceMoveTo.AddVehicle(concreteVehicle);
                        if (isParked)
                        {
                            AnsiConsole.Write(new Markup($"\n\n[aquamarine1]Vehicle successfully moved to space: {spaceMoveTo.SpaceNumber}.[/]\n\n"));
                            AnsiConsole.Write(new Markup(spaceMoveTo.ToString(), Color.Aquamarine1));

                            // remove vehicle from original spot
                            space.RemoveVehicle(concreteVehicle);
                            fileManager.SaveParkingData(garage.GetAllSpaces(), "../../../parkingdata.json");
                        }
                        else
                        {
                            AnsiConsole.Write(new Markup($"\n[aquamarine1]Unable to move vehicle to space: {spaceMoveTo.SpaceNumber}.[/]\n\n"));
                            // Load data from last save - cancels out incomplete move
                            fileManager.LoadParkingData("../../../parkingdata.json");
                        }
                        break;
                    }
                case "Parking Overview":
                    {
                        garage.VisualParkingGarage();
                        List<string> updateOptions = new List<string>
                        {
                            "[#ff00ff]YES[/]",
                            "[#ff00ff]NO[/]\n\n",
                        };
                        string updateSelect = AnsiConsole.Prompt(new SelectionPrompt<string>()
                             .Title("[#ff00ff]\n\nDo you want to print a detailed list of all parking spaces?[/]")
                             .AddChoices(updateOptions)
                        );
                        string cleanUpdateSelect = Markup.Remove(updateSelect).Trim();
                        if (cleanUpdateSelect == "YES")
                        {
                            // Print long list of parked vehicles - using ParkingGarage override ToString
                            AnsiConsole.Write(new Markup(garage.ToString(), Color.Aquamarine1));
                        }
                        break;
                    }
                case "Reload Price List":
                    {
                        WritePanel("RELOAD PRICE LIST", "#ff00ff", "#5fffd7");
                        List<string> updateOptions = new List<string>
                        {
                            "[#ff00ff]YES[/]",
                            "[#ff00ff]NO[/]\n\n",
                            "[#ff00ff]Exit to Main Menu[/]"
                        };
                        string updateSelect = AnsiConsole.Prompt(new SelectionPrompt<string>()
                             .Title("[#ff00ff]\n\nDo you want to update prices of parked vehicles?[/]")
                             .AddChoices(updateOptions)
                        );
                        string cleanUpdateSelect = Markup.Remove(updateSelect).Trim();
                        if (cleanUpdateSelect == "YES")
                        {
                            // Update pricelist here, in case user selects "Exit to Main Menu"
                            priceList = LoadPriceList(fileManager);
                            garage.UpdateAllVehiclePrices(priceList);
                            AnsiConsole.Write(new Markup($"\n\nPrices for all parked vehicles have been updated.", Color.Aquamarine1));
                            // Re-save parked vehicles with new prices
                            fileManager.SaveParkingData(garage.GetAllSpaces(), "../../../parkingdata.json");
                        }
                        else if (cleanUpdateSelect == "NO")
                        {
                            // Update pricelist here, in case user selects "Exit to Main Menu"
                            priceList = LoadPriceList(fileManager);
                            AnsiConsole.Write(new Markup($"\n\nNew prices will apply to new parked vehicles.", Color.Aquamarine1));
                        }
                        // No "else" needed. If user selects "Exit to Main Menu" --> break and return to Main Menu
                        break;
                    }
                case "Close Prague Parking":
                    {
                        WritePanel("SAVE DATA AND CLOSE", "#ff00ff", "#5fffd7");
                        // Breaker is the on/off switch for the menu loop --> breaker = false will stop do-while loop in Program.cs
                        breaker = false;
                        // Save data one last time
                        fileManager.SaveParkingData(garage.GetAllSpaces(), "../../../parkingdata.json");
                        AnsiConsole.Write(new Markup($"\n\nParked vehicles saved to file.\n\nGood bye, and [#5fffd7 slowblink]drive safe![/]", Color.Magenta1));
                        break;
                    }
                default:
                    {
                        WritePanel("Unexpected error!\nReturning to Main Menu", "#ff0000", "#800000");
                        break;
                    }
            }
            AnsiConsole.Console.Input.ReadKey(false);
            AnsiConsole.Clear();
        }



        public static IParkable? SelectVehicleType(int mcVehicleSize, int carVehicleSize, int busVehicleSize, int bicycleVehicleSize,
                                                        List<string> allowedVehicles, PriceList pricelist)
        {
            string regNumber = CollectRegNumber();
            if (string.IsNullOrEmpty(regNumber))    // User selected Main Menu
            {
                return null;
            }
            List<string> vehicleOptions = new List<string>      // TODO: Update vehicle options, add bus, bicycle and shopping trolley
                                                                // Change to alphabetical order??
            {
                "[#ff00ff]BANANA BOAT[/]",
                "[#ff00ff]BICYCLE[/]",
                "[#ff00ff]BUS[/]",
                "[#ff00ff]CAR[/]",
                "[#ff00ff]MC[/]",
                "[#ff00ff]SHOPPING TROLLEY[/]\n\n",
                "[#ff00ff]Exit to Main Menu[/]"
            };
            string vehicleSelect = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("[#ff00ff]\n\nSelect vehicle type using the arrow keys[/]")
                .AddChoices(vehicleOptions)
            );

            string cleanSelect = Markup.Remove(vehicleSelect).Trim();
            switch (cleanSelect)
            {
                case "BANANA BOAT":
                    {
                        if (allowedVehicles.Contains(cleanSelect))
                        {
                            return new BananaBoat(regNumber);
                        }
                        AnsiConsole.Write(new Markup($"\n\n{cleanSelect} is not an allowed vehicle.", Color.Aquamarine1));
                        return null;
                    }
                case "BICYCLE":
                    {
                        if (allowedVehicles.Contains(cleanSelect))
                        {
                            return new Bicycle(regNumber, bicycleVehicleSize, pricelist.BicycleVehiclePrice);
                        }
                        AnsiConsole.Write(new Markup($"\n\n{cleanSelect} is not an allowed vehicle.", Color.Aquamarine1));
                        return null;
                    }
                case "BUS":
                    {
                        if (allowedVehicles.Contains(cleanSelect))
                        {
                            return new Bus(regNumber, busVehicleSize, pricelist.BusVehiclePrice);
                        }
                        AnsiConsole.Write(new Markup($"\n\n{cleanSelect} is not an allowed vehicle.", Color.Aquamarine1));
                        return null;
                    }
                case "CAR":
                    {
                        if (allowedVehicles.Contains(cleanSelect))
                        {
                            return new Car(regNumber, carVehicleSize, pricelist.CarVehiclePrice);
                        }
                        AnsiConsole.Write(new Markup($"\n\n{cleanSelect} is not an allowed vehicle.", Color.Aquamarine1));
                        return null;
                    }
                case "MC":
                    {
                        if (allowedVehicles.Contains(cleanSelect))
                        {
                            return new MC(regNumber, mcVehicleSize, pricelist.MCVehiclePrice);
                        }
                        AnsiConsole.Write(new Markup($"\n\n{cleanSelect} is not an allowed vehicle.", Color.Aquamarine1));
                        return null;
                    }
                case "SHOPPING TROLLEY":
                    {
                        if (allowedVehicles.Contains(cleanSelect))
                        {
                            return new ShoppingTrolley(regNumber);
                        }
                        AnsiConsole.Write(new Markup($"\n\n{cleanSelect} is not an allowed vehicle.", Color.Aquamarine1));
                        return null;
                    }

                case "Exit to Main Menu":
                    return null;
                default:
                    return null;

            }

        }
        // Method: user input regnumber
        private static string CollectRegNumber()
        {
            string regNumber = AnsiConsole.Prompt(
                new TextPrompt<string>("[#ff00ff]\n\nEnter registration number (hit Enter for Main Menu): [/]")
                .AllowEmpty()
                .Validate(input =>                                          // Validation borrowed from Tim Corey
                {
                    input = input.Trim();
                    if (!string.IsNullOrEmpty(input) && input.Length > 10)
                    {
                        return ValidationResult.Error("[aquamarine1]\n\nError! Invalid registration number.[/]");
                    }
                    return ValidationResult.Success();     // default case
                })
                );
            // If user only hits Enter --> Main Menu
            if (string.IsNullOrEmpty(regNumber))
            {
                return null;
            }
            else
            {
                return regNumber.ToUpper();     // always want reg number in capital letters
            }
        }

        //Method to load saved cars, configuration, and initialize all parking spaces
        public static (ParkingGarage garage, PriceList priceList) Initialize()
        {
            // both configuration methods need a FileManager
            var fileManager = new FileManager();

            ParkingGarage garage = InitializeGarage(fileManager);
            PriceList priceList = LoadPriceList(fileManager);

            return (garage, priceList);
        }
        // Method to initialize Parking Garage
        public static ParkingGarage InitializeGarage(FileManager fileManager)
        {
            string parkingPath = "../../../parkingdata.json";
            string configPath = "../../../configuration.json";

            List<IParkingSpace> parkingData = fileManager.LoadParkingData(parkingPath);
            GarageConfiguration config = fileManager.ConfigureParkingGarage(configPath);
            if (config == null)
            {
                WritePanel("ERROR! Could not initialize application!", "#ff0000", "#800000");
                throw new Exception("Could not find configuration file.");
            }
            ParkingGarage garage = new ParkingGarage(parkingData, config.GarageSize, config.AllowedVehicles,
                config.MCVehicleSize, config.CarVehicleSize, config.BusVehicleSize, config.BicycleVehicleSize, config.ParkingSpaceSize);

            // Add check to see if Garage Size is smaller than amount of parking spaces in loaded data
            // TODO: This doesn't catch if there are vehicles parked in spaces removed
            if (garage.GarageSize < parkingData.Count)
            {
                garage.RemoveRangeOfSpaces(garage.GarageSize, parkingData.Count);
            }
            return garage;

        }
        // Moethod to load Price List
        public static PriceList LoadPriceList(FileManager fileManager)
        {
            string priceListPath = "../../../pricelist.txt";
            PriceListConfiguration priceConfig = fileManager.ConfigurePriceList(priceListPath);
            if (priceConfig == null)
            {
                WritePanel("ERROR! Could not load price list!", "#ff0000", "#800000");
                throw new Exception("Could not find price list file.");
            }

            PriceList priceList = new PriceList(priceConfig.PriceList.MCVehiclePrice, priceConfig.PriceList.CarVehiclePrice,
                priceConfig.PriceList.BusVehiclePrice, priceConfig.PriceList.BicycleVehiclePrice);

            return priceList;
        }
        private static void WritePanel(string panelText, string textColor, string borderColor)
        {
            Panel menuPanel = new Panel(new Markup($"[{textColor} bold]{panelText}[/]").Centered());
            menuPanel.Border = BoxBorder.Heavy;
            menuPanel.BorderColor(Color.FromHex(borderColor));
            menuPanel.Padding = new(2, 1);
            AnsiConsole.Write(menuPanel);
        }
    }
}