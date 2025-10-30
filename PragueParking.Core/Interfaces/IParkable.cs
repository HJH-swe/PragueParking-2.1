using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core.Interfaces
{
    public interface IParkable
    {
        // Added a few of the essential properties from Vehicle to IParkable.
        // Got several errors in the code when trying to use IParkable in methods, instead of Vehicle (as in version 2.0)
        string RegNumber { get; }           // no setter, as that is set when Vehicle (car, bus etc.) objects are created
        int VehicleSize { get; set; }       // setters here so they can be set by the configuration file
        decimal PricePerHour { get; set; }

        // methods
        int HoursToCharge();
        decimal ParkingFee();
        string PrintParkingReceipt();
    }
}
