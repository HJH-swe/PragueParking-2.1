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
        public BananaBoat(string regNumber) : base(regNumber)
        {
            
        }

        // No override methods: Banana boat is not an allowed vehicle,
        // so this class can be left empty - for noow.
    }
}
