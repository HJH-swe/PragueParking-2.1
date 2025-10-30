using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Data
{
    public class GarageConfiguration
    {
        public GarageConfiguration(int garageSize, List<string> allowedVehicles,
            int mcVehicleSize, int carVehicleSize, int busVehicleSize, int bicycleVehicleSize, int parkingSpaceSize)
        {
            GarageSize = garageSize;
            AllowedVehicles = allowedVehicles;
            MCVehicleSize = mcVehicleSize;
            CarVehicleSize = carVehicleSize;
            BusVehicleSize = busVehicleSize;
            BicycleVehicleSize = bicycleVehicleSize;
            ParkingSpaceSize = parkingSpaceSize;
        }
        public int GarageSize { get; }                  // Props don't need setters --> shouldn't change after configuration
        public List<string> AllowedVehicles { get; }
        public int MCVehicleSize { get; }
        public int CarVehicleSize { get; }
        public int BusVehicleSize { get; }
        public int BicycleVehicleSize { get; }
        public int ParkingSpaceSize { get; }

    }
}
