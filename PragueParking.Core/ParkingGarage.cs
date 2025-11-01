using PragueParking.Core.Interfaces;
using PragueParking.Core.VehicleTypes;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Quic;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core
{
    public class ParkingGarage
    {
        private readonly List<IParkingSpace> parkingSpaces = new List<IParkingSpace>();

        public ParkingGarage(List<IParkingSpace> savedData, int garageSize, List<string> allowedVehicles,
            int mcVehicleSize, int carVehicleSize, int busVehicleSize, int bicycleVehicleSize, int parkingSpaceSize)
        {
            parkingSpaces = savedData;
            GarageSize = garageSize;
            AllowedVehicles = allowedVehicles;
            MCVehicleSize = mcVehicleSize;
            CarVehicleSize = carVehicleSize;
            BusVehicleSize = busVehicleSize;
            BicycleVehicleSize = bicycleVehicleSize;
            ParkingSpaceSize = parkingSpaceSize;

            for (int i = parkingSpaces.Count; i < GarageSize; i++)
            {
                parkingSpaces.Add(new ParkingSpace(parkingSpaceSize, i + 1, new List<Vehicle>()));
            }
        }

        public int GarageSize { get; set; }
        public List<string> AllowedVehicles { get; set; }
        public int MCVehicleSize { get; set; }
        public int CarVehicleSize { get; set; }
        public int BusVehicleSize { get; set; }
        public int BicycleVehicleSize { get; set; }
        public int ParkingSpaceSize { get; set; }

        // Methods
        public List<IParkingSpace> GetAllSpaces()
        {
            return parkingSpaces;
        }

        // FindFreeSpace method:
        // "Save" first 48 spaces for buses (as only first 50 fit buses, 48 --> 12 buses)
        // Buses: find a free space from 1 - 48.
        // Other vehicles: first find a free space from 49 - GarageSize.
        // If no free space, then look at space 1-48
        public int FindFreeSpace(IParkable vehicle)
        {
            if (vehicle is Bus)
            {
                for (int i = 0; i < 48; i++)
                {
                    // Several nested if-statements to find 4 spaces next to each other
                    if (parkingSpaces[i].AvailableSize == ParkingSpaceSize)
                    {
                        if (parkingSpaces[i + 1].AvailableSize == ParkingSpaceSize)
                        {
                            if (parkingSpaces[i + 2].AvailableSize == ParkingSpaceSize)
                            {
                                if (parkingSpaces[i + 3].AvailableSize == ParkingSpaceSize)
                                {
                                    return i;   // First of the 4 free spaces
                                }
                            }
                        }
                    }
                }
            }
            else if (vehicle is Car)
            {
                for (int i = 48; i < parkingSpaces.Count; i++)      // Start from 48 first (space 49) to "save" spaces for busses
                {
                    if (parkingSpaces[i].AvailableSize == vehicle.VehicleSize)
                    {
                        return i;
                    }
                }
                for (int i = 0; i < parkingSpaces.Count; i++)
                {
                    if (parkingSpaces[i].AvailableSize == vehicle.VehicleSize)
                    {
                        return i;
                    }
                }
            }
            else if (vehicle is MC)
            {
                // Start from 48 to "save" spaces for busses
                // Optimize parking - first check to fill 1 space with 2 MC
                for (int i = 48; i < parkingSpaces.Count; i++)
                {
                    if (parkingSpaces[i].AvailableSize == vehicle.VehicleSize)
                    {
                        return i;
                    }
                }
                // If no space next to MC, look for any space
                for (int i = 48; i < parkingSpaces.Count; i++)
                {
                    if (parkingSpaces[i].AvailableSize > vehicle.VehicleSize)
                    {
                        return i;
                    }
                }
                // Same - but from parking space 1
                for (int i = 0; i < parkingSpaces.Count; i++)
                {
                    if (parkingSpaces[i].AvailableSize == vehicle.VehicleSize)
                    {
                        return i;
                    }
                }
                for (int i = 0; i < parkingSpaces.Count; i++)
                {
                    if (parkingSpaces[i].AvailableSize > vehicle.VehicleSize)
                    {
                        return i;
                    }
                }
            }
            else if (vehicle is Bicycle)
            {
                // Optimize parking, look for spaces with 1 or 3 bicycles (if 2 bicycles a motorbike also would fit)
                for (int i = 48; i < parkingSpaces.Count; i++)
                {
                    if (parkingSpaces[i].AvailableSize == vehicle.VehicleSize)
                    {
                        return i;
                    }
                    if (parkingSpaces[i].AvailableSize == (ParkingSpaceSize - vehicle.VehicleSize))
                    {
                        return i;
                    }
                }
            }
            // If no space with other bicycles, look for any space
            for (int i = 48; i < parkingSpaces.Count; i++)
            {
                if (parkingSpaces[i].AvailableSize > vehicle.VehicleSize)
                {
                    return i;
                }
            }
            // Same, but from first parking space
            for (int i = 0; i < parkingSpaces.Count; i++)
            {
                if (parkingSpaces[i].AvailableSize == vehicle.VehicleSize)
                {
                    return i;
                }
                if (parkingSpaces[i].AvailableSize == (ParkingSpaceSize - vehicle.VehicleSize))
                {
                    return i;
                }
            }
            for (int i = 0; i < parkingSpaces.Count; i++)
            {
                if (parkingSpaces[i].AvailableSize > vehicle.VehicleSize)
                {
                    return i;
                }
            }
            // Return -1 if no free spaces, garage is full 
            return -1;
        }
        public bool ParkVehicle(IParkable vehicle, out int freeSpace)
        {
            freeSpace = FindFreeSpace(vehicle);
            if (freeSpace == -1)
            {
                return false;
            }
            else
            {
                if (vehicle is Bus bus)
                {
                    bus.SpacesUsing.Add(parkingSpaces[freeSpace]);
                    bus.SpacesUsing.Add(parkingSpaces[freeSpace + 1]);
                    bus.SpacesUsing.Add(parkingSpaces[freeSpace + 2]);
                    bus.SpacesUsing.Add(parkingSpaces[freeSpace + 3]);

                    // Buses are parked in 4 steps (for 4 spaces) --> need to know if all 4 steps were successfull
                    // Use bools - if AddVehicle fails at any point, the whole bus isn't parked 
                    Vehicle concreteBus = (Vehicle)vehicle;
                    bool allBusParked = true;
                    foreach (IParkingSpace space in bus.SpacesUsing)
                    {
                        bool success = space.AddVehicle(concreteBus);
                        if (!success)
                        {
                            allBusParked = false;
                            break;
                        }
                    }
                    return allBusParked;
                }
                else
                {
                    var spaceToUse = parkingSpaces[freeSpace];
                    Vehicle concreteVehicle = (Vehicle)vehicle;
                    return spaceToUse.AddVehicle(concreteVehicle);
                }
            }
        }
        public bool MoveBus(IParkable vehicle, int spaceNumber)
        {
            try
            {
                int spaceIndex = spaceNumber - 1;
                // Need to check if there's space to move bus to - check 4 spaces 
                for (int i = 0; i < 4; i++)
                {
                    if (parkingSpaces[spaceIndex + i].ParkedVehicles.Count > 0)
                    {
                        return false;
                    }
                }
                Vehicle bus = (Vehicle)vehicle;
                parkingSpaces[spaceIndex].ParkedVehicles.Add(bus);
                parkingSpaces[spaceIndex].AvailableSize -= (vehicle.VehicleSize / 4);
                parkingSpaces[spaceIndex + 1].ParkedVehicles.Add(bus);
                parkingSpaces[spaceIndex + 1].AvailableSize -= (vehicle.VehicleSize / 4);
                parkingSpaces[spaceIndex + 2].ParkedVehicles.Add(bus);
                parkingSpaces[spaceIndex + 2].AvailableSize -= (vehicle.VehicleSize / 4);
                parkingSpaces[spaceIndex + 3].ParkedVehicles.Add(bus);
                parkingSpaces[spaceIndex + 3].AvailableSize -= (vehicle.VehicleSize / 4);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public void RemoveBus(IParkable vehicle, IParkingSpace space)
        {
            int spaceIndex = space.SpaceNumber - 1;
            parkingSpaces[spaceIndex].ParkedVehicles = new List<Vehicle>();
            parkingSpaces[spaceIndex].AvailableSize += (vehicle.VehicleSize / 4);
            parkingSpaces[spaceIndex + 1].ParkedVehicles = new List<Vehicle>();
            parkingSpaces[spaceIndex + 1].AvailableSize += (vehicle.VehicleSize / 4);
            parkingSpaces[spaceIndex + 2].ParkedVehicles = new List<Vehicle>();
            parkingSpaces[spaceIndex + 2].AvailableSize += (vehicle.VehicleSize / 4);
            parkingSpaces[spaceIndex + 3].ParkedVehicles = new List<Vehicle>();
            parkingSpaces[spaceIndex + 3].AvailableSize += (vehicle.VehicleSize / 4);
        }
        public IParkingSpace FindVehicleSpace(string regNumber)
        {
            foreach (var parkingSpace in parkingSpaces)
            {
                foreach (var vehicle in parkingSpace.ParkedVehicles)
                {
                    if (vehicle.RegNumber == regNumber)
                    {
                        return parkingSpace;
                    }
                }
            }
            return null;
        }
        public IParkingSpace GetParkingSpace(int spaceNumber)
        {
            if (spaceNumber < 0 || spaceNumber > parkingSpaces.Count)
            {
                return null;
            }
            else
            {
                return parkingSpaces[spaceNumber];
            }
        }
        public bool CheckForParkedVehicles(int fromIndex)
        {
            bool vehicleFound = false;
            for (int i = fromIndex; i < parkingSpaces.Count; i++)
            {
                if (parkingSpaces[i].ParkedVehicles.Count > 0)      // If there's a vehicle in any space
                {
                    vehicleFound = true;
                }
            }
            return vehicleFound;
        }
        // Method to remove extra spaces if new configuration of Garage Size is smaller than before
        public void RemoveRangeOfSpaces(int fromIndex, int toIndex)
        {
            parkingSpaces.RemoveRange(fromIndex, (toIndex - fromIndex));        // toIndex - fromIndex = number of spaces to remove
            // Last space didn't get the right space number, so fixing here
            parkingSpaces[fromIndex].SpaceNumber = fromIndex + 1;
        }
        public void UpdateAllVehiclePrices(PriceList priceList)
        {
            foreach (var space in parkingSpaces)
            {
                foreach (var vehicle in space.ParkedVehicles)
                {
                    if (vehicle.VehicleSize == 4)
                    {
                        vehicle.PricePerHour = priceList.CarVehiclePrice;
                    }
                    else if (vehicle.VehicleSize == 2)
                    {
                        vehicle.PricePerHour = priceList.MCVehiclePrice;
                    }
                    else if (vehicle.VehicleSize == 16)
                    {
                        vehicle.PricePerHour = priceList.BusVehiclePrice;
                    }
                    else if (vehicle.VehicleSize == 1)
                    {
                        vehicle.PricePerHour = priceList.BicycleVehiclePrice;
                    }
                }
            }
        }
        public void VisualParkingGarage()
        {
            // Header to match other menu headings.
            // Used a table here to match the other tables in the parking overview
            Table header = new Table()
                .Centered()
                .Border(TableBorder.HeavyEdge)
                .BorderColor(Color.Aquamarine1)
                .Width(70);
            header.AddColumn(new TableColumn("[#ff00ff bold] PRAGUE PARKING OVERVIEW[/]").Centered());
            AnsiConsole.Write(header);

            // One table for subheading
            Table subHeader = new Table();
            subHeader.AddColumns("[#d7ffff]EMPTY SPACE:[/] [lime]GREEN[/]",
                "[#d7ffff]PARTIALLY OCCUPIED:[/] [yellow]YELLOW[/]",
                "[#d7ffff]FULL SPACE:[/] [red]RED[/]")
                .Centered()
                .Alignment(Justify.Center);
            subHeader.Border(TableBorder.HeavyEdge);
            subHeader.BorderColor(Color.Aquamarine1);
            subHeader.Width(70);
            AnsiConsole.Write(subHeader);

            // Another table for visual over parking garage
            Table allSpaces = new Table().Centered();
            var colorString = string.Empty;
            var printSpots = string.Empty;
            int spaceCounter = 1, emptyCounter = 0, halfCounter = 0, fullCounter = 0;       // Will use counters for bar chart
            foreach (var space in parkingSpaces)
            {
                if (space.AvailableSize == space.TotalSize)       // If available == total --> empty
                {
                    colorString = "lime";
                    emptyCounter++;
                }
                else if (space.AvailableSize > 0 && space.AvailableSize < space.TotalSize)  // greater than 0, less than total --> partially occupied
                {
                    colorString = "yellow";
                    halfCounter++;
                }
                else                                                                        // Only reasonable option left --> space occupied 
                {
                    colorString = "red";
                    fullCounter++;
                }
                // Format spaces so numbers align nicely
                if (spaceCounter < 10)
                {
                    printSpots += $"[{colorString}]{spaceCounter}    [/]";
                }
                else if(spaceCounter > 9 && spaceCounter < 100)
                {
                    printSpots += $"[{colorString}]{spaceCounter}   [/]";
                }
                else if(spaceCounter >= 100)
                {
                    printSpots += $"[{colorString}]{spaceCounter}  [/]";
                }
                spaceCounter++;
            }

            allSpaces.AddColumn(new TableColumn(printSpots).PadLeft(3)).Centered();
            allSpaces.Border(TableBorder.HeavyEdge);
            allSpaces.BorderColor(Color.Aquamarine1);
            allSpaces.Width(70);
            AnsiConsole.Write(allSpaces);

            // Quick bar chart to show free/partially occupied/full spaces

            var barChart = new BarChart()
                //.Width(65)
                .AddItem("[red]FULL SPACES[/]", fullCounter, Color.Red)
                .AddItem("[yellow]PARTIALLY OCCUPIED[/]", halfCounter, Color.Yellow)
                .AddItem("[lime]EMPTY SPACES[/]", emptyCounter, Color.Lime);

            // Put chart in a table for nice format
            Table chartTable = new Table()
                .Width(70)
                .Centered()
                .Border(TableBorder.HeavyEdge)
                .BorderColor(Color.Aquamarine1)
                .AddColumn(new TableColumn(barChart).Centered());

            AnsiConsole.Write(chartTable);

        }
        public override string ToString()
        {
            StringBuilder parkingGarage = new StringBuilder();
            parkingGarage.AppendLine("\nStatus of entire garage:\n");

            if (parkingSpaces.Count > 0)
            {
                //parkingGarage.Append(string.Join("\n", parkingSpaces));
                foreach (var parkingSpace in parkingSpaces)
                {
                    parkingGarage.Append(parkingSpace + "\n");
                }
            }
            else
            {
                parkingGarage.AppendLine("The garage is empty.");
            }
            return parkingGarage.ToString();
        }
    }
}