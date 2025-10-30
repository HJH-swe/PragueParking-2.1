using PragueParking.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core.VehicleTypes
{
    public class BananaBoat : Vehicle
    {
        // Banana boat is not an allowed vehicle,
        // so this class can basically be empty.
        public BananaBoat(string regNumber) : base(regNumber)
        {
            
        }

    }
}
