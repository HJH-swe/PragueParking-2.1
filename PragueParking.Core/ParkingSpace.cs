using PragueParking.Core.Interfaces;
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
        // total size of parking spaces shouldn't be changed - so private readonly
        // TODO: Now loaded from config file, so remove?
        private readonly int totalSize = 4;

        // Properties - change setters for encapsulation?
        public int TotalSize { get; set; }
        public int SpaceNumber { get; set; }
        public int AvailableSize { get; set; }
        public List<Vehicle> ParkedVehicles { get; set; }       // Needed to change this back to <Vehicle> so json can serialize/deserialize properly

        // A special constructor for JSON - found the idea online - ONLY CONSTRUCTOR?
        [JsonConstructor]
        public ParkingSpace(int totalSize, int spaceNumber, List<Vehicle> parkedVehicles)
        {
            TotalSize = totalSize;
            SpaceNumber = spaceNumber;
            AvailableSize = totalSize;
            ParkedVehicles = parkedVehicles;
        }


        // Methods
        public bool IsEnoughSpace(IParkable vehicle)
        {
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
                AvailableSize -= vehicle.VehicleSize;
                return true;
            }
        }
        public ParkingSpace RemoveVehicle(Vehicle vehicle)
        {
            ParkedVehicles.Remove(vehicle);
            AvailableSize += vehicle.VehicleSize;
            return this;
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
        // TODO: Remove method? Check how many references
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
                    vehicles += vehicle.RegNumber + "  ";
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
