using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core.VehicleTypes
{
    public class ShoppingTrolley : Vehicle
    {
        // Shopping trolley is not an allowed vehicle,
        // so this class can basically be empty.
        public ShoppingTrolley(string regNumber) : base(regNumber)
        {

        }
    }
}
