using PragueParking.Core.Interfaces;
using PragueParking.Core.VehicleTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PragueParking.Core
{

    public class ParkingSpace : IParkingSpace
    {
        [JsonConstructor]
        public ParkingSpace(int totalSize, int spaceNumber, List<Vehicle> parkedVehicles)
        {
            TotalSize = totalSize;
            SpaceNumber = spaceNumber;
            AvailableSize = totalSize;
            ParkedVehicles = parkedVehicles;
        }

        // Properties 
        public int TotalSize { get; set; }
        public int SpaceNumber { get; set; }
        public int AvailableSize { get; set; }
        public List<Vehicle> ParkedVehicles { get; set; }       // Needed to change this back to <Vehicle> so json can serialize/deserialize properly

        // Methods
        public bool IsEnoughSpace(IParkable vehicle)
        {
            // A "quick fix" - not a sustainable solution.
            // The method checks if there's space for 1/4 of the bus in a parking space
            if (vehicle.VehicleSize == 16)
            {
                return (vehicle.VehicleSize / 4) <= AvailableSize;
            }
            return vehicle.VehicleSize <= AvailableSize;
        }
        public bool AddVehicle(Vehicle vehicle)
        {
            if (IsEnoughSpace(vehicle) == false)
            {
                return false;
            }
            else
            {
                ParkedVehicles.Add(vehicle);
                if (vehicle.VehicleSize == 16)
                {
                    AvailableSize -= (vehicle.VehicleSize / 4);
                }
                else
                {
                    AvailableSize -= vehicle.VehicleSize;
                }
                return true;
            }
        }
        public ParkingSpace RemoveVehicle(Vehicle vehicle)
        {
            if (vehicle.VehicleSize == 16)
            {
                ParkedVehicles.Remove(vehicle);
                AvailableSize += (vehicle.VehicleSize / 4);
                return this;
            }
            else
            {
                ParkedVehicles.Remove(vehicle);
                AvailableSize += vehicle.VehicleSize;
                return this;
            }
        }
        public IParkable FindVehicleInSpace(string regNumber)
        {
            foreach (var vehicle in ParkedVehicles)
            {
                if (vehicle.RegNumber == regNumber)
                {
                    return vehicle;
                }
            }
            return null;
        }
        public string PrintParkingSpace(int spaceNumber)
        {
            if (ParkedVehicles.Count == 0)
            {
                return $"Space {SpaceNumber}: (Empty)  -   Available space: {AvailableSize}";
            }
            else
            {
                string vehicles = "";
                foreach (IParkable vehicle in ParkedVehicles)
                {
                    // Write out all spaces buses occupy
                    if (vehicle.VehicleSize == 16)
                    {
                        return $"Spaces {SpaceNumber} - {SpaceNumber + 3}: {vehicle}\tAvailable space: {AvailableSize}";
                    }
                    else
                    {
                        vehicles += vehicle.RegNumber + "  ";
                    }
                }
                return $"Space {SpaceNumber}: {vehicles}\tAvailable space: {AvailableSize}";
            }
        }
        public override string ToString()
        {
            if (ParkedVehicles.Count == 0)
            {
                return $"Space {SpaceNumber}: (Empty)  -   Available space: {AvailableSize}\n";
            }
            else
            {
                string vehicles = "";
                foreach (IParkable vehicle in ParkedVehicles)
                {
                    vehicles += vehicle.RegNumber + "  ";
                }
                return $"Space {SpaceNumber}: {vehicles} -   Available space: {AvailableSize}\n";
            }
        }
    }
}
