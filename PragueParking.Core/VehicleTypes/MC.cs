using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core.VehicleTypes
{
    public class MC : Vehicle
    {
        public MC(string regNumber, int vehicleSize, int pricePerHour) : base(regNumber)
        {
            VehicleSize = vehicleSize;
            PricePerHour = pricePerHour;
        }
        // Override methods from Vehicle class
        public override string PrintParkingReceipt()
        {
            decimal parkingFee = ParkingFee();
            return string.Format($"\nMC {RegNumber}\n" + base.ToString());
        }
        public override string ToString()
        {
            return string.Format($"\nMC {RegNumber}\n" + base.ToString());
        }
    }
}


// NOTES FOR LATER
// If you want to reconfigure the garage and change the size
// so that you remove spaces that vehicles are parked on
// --> should say stop! Not allowed

// NOTE You should be able to change the configuration data while the programming is running
// - a menu option to reload the configfile -
// although perhaps you shouldn't be able to change certain things when you have cars parked
// So maybe add a warning "All your saved data will disappear now"
// and ask if you want to continue --> otherwise exit to main menu

// But if you want to add parking spaces - that's not a problem. Should be able to do that while the program is running