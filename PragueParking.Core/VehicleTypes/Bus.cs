using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core.VehicleTypes
{
    public class Bus : Vehicle
    {
        // Constructor
        public Bus(string regNumber, int vehicleSize, int pricePerHour) : base(regNumber)
        {
            VehicleSize = vehicleSize;
            PricePerHour = pricePerHour;
        }

        // Override methods from Vehicle class
        public override string PrintParkingReceipt()
        {
            decimal parkingFee = ParkingFee();
            return string.Format($"\nBUS {RegNumber}\n" + base.ToString());
        }
        public override string ToString()
        {
            return string.Format($"\nBUS {RegNumber}\n" + base.ToString());
        }
    }
}
