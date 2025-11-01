using PragueParking.Core;
using PragueParking.Core.Interfaces;
using PragueParking.Core.VehicleTypes;
using PragueParking.Data;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace PragueParking.Console
{
    public class ConsoleUI
    {

        public static void MainMenu(ParkingGarage garage, PriceList priceList, out bool breaker)
        {
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
                "[springgreen1]Configure Parking Garage or Price List[/]\n",
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
                                AnsiConsole.Write(new Markup(garage.GetParkingSpace(parkedSpace).PrintParkingSpace(parkedSpace), Color.Magenta1));

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

                        if (vehicle.VehicleSize == garage.BusVehicleSize)  // check if vehicle is bus
                        {
                            garage.RemoveBus(vehicle, space);
                        }
                        else
                        {
                            Vehicle concreteVehicle = (Vehicle)vehicle;
                            space.RemoveVehicle(concreteVehicle);
                        }
                        AnsiConsole.Write(new Markup("\nVehicle successfully checked out.\n\n", Color.Aquamarine1));
                        WritePanel(vehicle.PrintParkingReceipt(), "#5fffd7", "#ff00ff");
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

                        //AnsiConsole.Write(new Markup(vehicle.ToString(), Color.Aquamarine1));
                        AnsiConsole.Write(new Markup(space.PrintParkingSpace(space.SpaceNumber), Color.Aquamarine1));

                        break;
                    }
                case "Move Vehicle":
                    {
                        WritePanel("MOVE VEHICLE", "#ff00ff", "#5fffd7");
                        // Find vehicle by regnumber
                        string regNumber = CollectRegNumber();
                        int spaceNumber;                        // Will use spaceNumber later
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
                        if (vehicle.VehicleSize == 16)
                        {
                            // Buses can only fit in space 1-48, which means user must choose space 1-45 (first space bus will be parked on)
                            string spaceNumberString = AnsiConsole.Prompt(new TextPrompt<string>("[#ff00ff]\nEnter parking space to move vehicle to (1-45):[/]")
                                .AllowEmpty()
                                .Validate(input =>
                                {
                                    if (string.IsNullOrWhiteSpace(input))
                                    {
                                        return ValidationResult.Error($"[springgreen1]\n\nError! Please enter a number from 1 to 45.[/]");
                                    }

                                    if (Convert.ToInt32(input) < 1 || Convert.ToInt32(input) > 45)
                                    {
                                        return ValidationResult.Error($"[springgreen1]\n\nError! A bus will not fit in space {input} to {input + 3}\n.[/]");
                                    }
                                    return ValidationResult.Success();     // default case
                                })
                                );
                            spaceNumber = int.Parse(spaceNumberString);
                        }
                        else
                        {
                            //garage.MoveVehicle(vehicle);
                            // Ask user for space number to move vehicle to
                            string spaceNumberString = AnsiConsole.Prompt(new TextPrompt<string>("[#ff00ff]\nEnter parking space to move vehicle to:[/]")
                                .AllowEmpty()
                                .Validate(input =>
                                {
                                    if (string.IsNullOrWhiteSpace(input))
                                    {
                                        return ValidationResult.Error($"[springgreen1]\n\nError! Please enter a number from 1 to {garage.GarageSize}.[/]");
                                    }

                                    if (Convert.ToInt32(input) < 1 || Convert.ToInt32(input) > garage.GarageSize)
                                    {
                                        return ValidationResult.Error($"[springgreen1]\n\nError! Parking spaces are numbered from 1 to {garage.GarageSize}.[/]");
                                    }
                                    return ValidationResult.Success();     // default case
                                })
                                );
                            spaceNumber = int.Parse(spaceNumberString);
                        }
                        IParkingSpace spaceMoveTo = garage.GetParkingSpace(spaceNumber - 1);     // minus 1 for correct index
                        // Try to add the vehicle from above to new parking space
                        // Had to separate buses and other vehicles here
                        bool isParked;
                        if (vehicle.VehicleSize == garage.BusVehicleSize)
                        {
                            isParked = garage.MoveBus(vehicle, spaceNumber);
                            if (isParked)
                            {
                                AnsiConsole.Write(new Markup($"\n\n[aquamarine1]Vehicle successfully moved to spaces: {spaceNumber} - {spaceNumber + 3}.[/]\n\n"));
                                AnsiConsole.Write(new Markup(spaceMoveTo.PrintParkingSpace(spaceMoveTo.SpaceNumber), Color.Magenta1));

                                // remove vehicle from original spot and save data
                                garage.RemoveBus(vehicle, space);
                                fileManager.SaveParkingData(garage.GetAllSpaces(), "../../../parkingdata.json");
                            }
                            else
                            {
                                AnsiConsole.Write(new Markup($"\n[aquamarine1]Unable to move vehicle to space: {spaceMoveTo.SpaceNumber}.[/]\n\n"));
                                // Load data from last save - cancels out incomplete move
                                fileManager.LoadParkingData("../../../parkingdata.json");
                            }

                        }
                        else
                        {
                            Vehicle concreteVehicle = (Vehicle)vehicle;
                            isParked = spaceMoveTo.AddVehicle(concreteVehicle);
                            if (isParked)
                            {
                                AnsiConsole.Write(new Markup($"\n\n[aquamarine1]Vehicle successfully moved to space: {spaceMoveTo.SpaceNumber}.[/]\n\n"));
                                AnsiConsole.Write(new Markup(spaceMoveTo.PrintParkingSpace(spaceMoveTo.SpaceNumber), Color.Magenta1));

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
                case "Configure Parking Garage or Price List":
                    {
                        WritePanel("CONFIGURE GARAGE OR PRICE LIST", "#ff00ff", "#5fffd7");
                        List<string> configureOptions = new List<string>
                        {
                            "[#ff00ff]PARKING GARAGE[/]",
                            "[#ff00ff]PRICE LIST[/]\n\n",
                            "[#ff00ff]Exit to Main Menu[/]"
                        };
                        string updateSelect = AnsiConsole.Prompt(new SelectionPrompt<string>()
                             .Title("[#ff00ff]\n\nSelect to configure the parking garage or price list:\n[/]")
                             .AddChoices(configureOptions)
                        );
                        string cleanUpdateSelect = Markup.Remove(updateSelect).Trim();
                        if (cleanUpdateSelect == "PARKING GARAGE")
                        {
                            garage = ReConfigureParkingGarage(garage);
                            if (garage == null)
                            {
                                AnsiConsole.Write(new Markup("\n\nParking garage was not reconfigured.", Color.Aquamarine1));
                            }
                            else
                            {
                                AnsiConsole.Write(new Markup("\n\nParking garage has been reconfigured.", Color.Aquamarine1));
                                Thread.Sleep(2000);
                                AnsiConsole.Clear();
                                MainMenu(garage, priceList, out breaker);
                            }
                        }
                        else if (cleanUpdateSelect == "PRICE LIST")
                        {
                            priceList = UpdatePriceList(fileManager, priceList);
                            ApplyNewPrices(garage, priceList);
                            MainMenu(garage, priceList, out breaker);
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
            if (garage.GarageSize < parkingData.Count)
            {
                garage.RemoveRangeOfSpaces(garage.GarageSize, parkingData.Count);
            }
            return garage;

        }
        public static ParkingGarage? ReConfigureParkingGarage(ParkingGarage garage)
        {
            FileManager fileManager = new FileManager();
            List<IParkingSpace> parkingData = fileManager.LoadParkingData("../../../parkingdata.json");
            GarageConfiguration gc = fileManager.ConfigureParkingGarage("../../../configuration.json");
            List<string> configOptions = new List<string>
                        {
                            "[#ff00ff]INCREASE[/]",
                            "[#ff00ff]DECREASE[/]\n\n",
                        };
            string updateSelect = AnsiConsole.Prompt(new SelectionPrompt<string>()
                 .Title($"[#ff00ff]\n\nDo you want to increase or decrease the number of parking spaces?\n(Current parking garage size: {garage.GarageSize})[/]")
                 .AddChoices(configOptions)
            );
            string cleanUpdateSelect = Markup.Remove(updateSelect).Trim();
            if (cleanUpdateSelect == "INCREASE")
            {
                string sizeString = AnsiConsole.Prompt(new TextPrompt<string>("[#ff00ff]\nEnter new size of parking garage:[/]"));
                int newSize = int.Parse(sizeString);
                gc.GarageSize = newSize;
                ParkingGarage updatedGarage = new ParkingGarage(parkingData, gc.GarageSize, gc.AllowedVehicles,
                    gc.MCVehicleSize, gc.CarVehicleSize, gc.BusVehicleSize, gc.BicycleVehicleSize, gc.ParkingSpaceSize);
                // Save new configuration to config file
                string saveResult = fileManager.SaveConfigurationData(gc);
                // Print if save was successful
                AnsiConsole.Write(new Markup($"\n\n{saveResult}", Color.Aquamarine1));
                // Update parkingdata.json file
                fileManager.SaveParkingData(updatedGarage.GetAllSpaces(), "../../../parkingdata.json");
                return updatedGarage;
            }
            else if (cleanUpdateSelect == "DECREASE")
            {
                string sizeString = AnsiConsole.Prompt(new TextPrompt<string>("[#ff00ff]\nEnter new size of parking garage:[/]"));
                int newSize = int.Parse(sizeString);
                // Check if there are vehicles parked in spaces to delete
                bool parkedVehicles = garage.CheckForParkedVehicles(newSize - 1);
                if (parkedVehicles)
                {
                    var howToProceed = AnsiConsole.Prompt(new SelectionPrompt<string>()
                              .Title("[#ff00ff]\n\nWarning! There are vehicles parked in parking spaces to delete.\n" +
                              "Do you want to delete parking spaces anyway and lose customer data?\n\n[/]?")
                              .AddChoices(new[] { "[#ff00ff]YES[/]",
                                                  "[#ff00ff]NO[/]" }));
                    string cleanSelect = Markup.Remove(howToProceed).Trim();
                    if (cleanSelect == "YES")
                    {
                        garage.RemoveRangeOfSpaces(newSize - 1, garage.GarageSize - 1);
                        gc.GarageSize = newSize;
                        garage.GarageSize = newSize;
                        // Save new configuration to config file
                        string saveResult = fileManager.SaveConfigurationData(gc);
                        // Print if save was successful
                        AnsiConsole.Write(new Markup($"\n\n{saveResult}", Color.Aquamarine1));
                        // Update parkingdata.json file
                        fileManager.SaveParkingData(garage.GetAllSpaces(), "../../../parkingdata.json");
                        return garage;
                    }
                    else
                    {
                        return null;
                    }
                }
                // If no parked vehicles in spaces to remove
                else
                {
                    garage.RemoveRangeOfSpaces(newSize - 1, garage.GarageSize - 1);
                    gc.GarageSize = newSize;
                    garage.GarageSize = newSize;
                    // Save new configuration to config file
                    string saveResult = fileManager.SaveConfigurationData(gc);
                    // Print if save was successful
                    AnsiConsole.Write(new Markup($"\n\n{saveResult}", Color.Aquamarine1));
                    return garage;
                }
            }
            return null;  // Shouldn't end up here
        }

        // Method to load Price List
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

        public static PriceList ReloadPriceList(FileManager fileManager, string filePath)
        {
            PriceListConfiguration priceConfig = fileManager.ConfigurePriceList(filePath);
            if (priceConfig == null)
            {
                WritePanel("ERROR! Could not reload price list!", "#ff0000", "#800000");
                return null;
            }

            PriceList priceList = new PriceList(priceConfig.PriceList.MCVehiclePrice, priceConfig.PriceList.CarVehiclePrice,
                priceConfig.PriceList.BusVehiclePrice, priceConfig.PriceList.BicycleVehiclePrice);

            return priceList;
        }
        public static PriceList? UpdatePriceList(FileManager fileManager, PriceList priceList)
        {
            // Get new prices from user input
            string carPrice = AnsiConsole.Prompt(new TextPrompt<string>("[#ff00ff]\nEnter new price for cars:[/]"));
            string mcPrice = AnsiConsole.Prompt(new TextPrompt<string>("[#ff00ff]\nEnter new price for motorbikes:[/]"));
            string busPrice = AnsiConsole.Prompt(new TextPrompt<string>("[#ff00ff]\nEnter new price for buses:[/]"));
            string bicyclePrice = AnsiConsole.Prompt(new TextPrompt<string>("[#ff00ff]\nEnter new price for bicycles:[/]"));

            string updatedPriceList = "# Price list - prices for different vehicle types\r\n" +
                "# To change the price: Change the number after the equal sign\r\n" +
                "# To reload the prices in the application: Choose menu option \"Configure Parking Garage or Price List\"\r\n" +
                $"# Price for cars (CZK)\r\nCAR.price={carPrice}\r\n" +
                $"# Price for motorbikes (CZK)\r\nMC.price={mcPrice}\r\n" +
                $"# Price for buses (CZK)\r\nBUS.price={busPrice}\r\n" +
                $"# Prices for bicycles (CZK)\r\nBICYCLE.price={bicyclePrice}";

            string filePath = "../../../pricelist.txt";
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine(updatedPriceList);
            }

            return priceList = ReloadPriceList(fileManager, filePath);
        }
        public static void ApplyNewPrices(ParkingGarage garage, PriceList priceList)
        {
            List<string> updateOptions = new List<string>
                        {
                            "[#ff00ff]YES[/]",
                            "[#ff00ff]NO[/]\n\n"
                        };
            string updateSelect = AnsiConsole.Prompt(new SelectionPrompt<string>()
                 .Title("[#ff00ff]\n\nDo you want to update the prices of parked vehicles?[/]")
                 .AddChoices(updateOptions)
            );
            string cleanUpdateSelect = Markup.Remove(updateSelect).Trim();
            if (cleanUpdateSelect == "YES")
            {
                garage.UpdateAllVehiclePrices(priceList);
                AnsiConsole.Write(new Markup($"\n\nPrices for all parked vehicles have been updated.", Color.Aquamarine1));
                // Re-save parked vehicles with new prices
                FileManager fileManager = new FileManager();
                fileManager.SaveParkingData(garage.GetAllSpaces(), "../../../parkingdata.json");
            }
            else if (cleanUpdateSelect == "NO")
            {
                AnsiConsole.Write(new Markup($"\n\nNew prices will apply to new parked vehicles.", Color.Aquamarine1));
            }
            Thread.Sleep(2000);
            AnsiConsole.Clear();
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