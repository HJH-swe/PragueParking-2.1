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
        public PriceList(decimal mcPricePerHour, decimal carPricePerHour, decimal busPricePerHour, decimal bicyclePricePerHour)
        {
            MCVehiclePrice = mcPricePerHour;
            CarVehiclePrice = carPricePerHour;
            BusVehiclePrice = busPricePerHour;
            BicycleVehiclePrice = bicyclePricePerHour;
        }
        public decimal MCVehiclePrice { get; set; }
        public decimal CarVehiclePrice { get; set; }
        public decimal BusVehiclePrice { get; set; }
        public decimal BicycleVehiclePrice { get; set; }
    }
}
