using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core
{
    public class PriceList
    {
        public PriceList()
        {

        }
        public PriceList(int mcPricePerHour, int carPricePerHour, int busPricePerHour, int bicyclePricePerHour)
        {
            MCVehiclePrice = mcPricePerHour;
            CarVehiclePrice = carPricePerHour;
            BusVehiclePrice = busPricePerHour;
            BicycleVehiclePrice = bicyclePricePerHour;
        }
        public int MCVehiclePrice { get; set; }
        public int CarVehiclePrice { get; set; }
        public int BusVehiclePrice { get; set; }
        public int BicycleVehiclePrice { get; set; }
    }
}
