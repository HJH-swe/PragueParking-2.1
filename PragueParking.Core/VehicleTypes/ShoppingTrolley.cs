using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core.VehicleTypes
{
    public class ShoppingTrolley : Vehicle
    {
        public ShoppingTrolley(string regNumber) : base(regNumber)
        {

        }

        // No override methods: Shopping trolley is not an allowed vehicle,
        // so this class can be left empty - for now.
    }
}
