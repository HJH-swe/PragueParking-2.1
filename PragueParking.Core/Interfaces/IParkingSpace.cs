using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core.Interfaces
{
    public interface IParkingSpace
    {
        // Properties that are used in methods throughout code
        int TotalSize { get; set; }         // Setters because they can be changed from configuration or in methods
        int SpaceNumber { get; set; }
        int AvailableSize { get; set; }
        List<Vehicle> ParkedVehicles { get; set; }

        bool IsEnoughSpace(IParkable vehicle);
        bool AddVehicle(Vehicle vehicle);
        ParkingSpace RemoveVehicle(Vehicle vehicle);
        IParkable FindVehicleInSpace(string regNumber);
        string PrintParkingSpace(int spaceNumber);

    }
}
