using PragueParking.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core.VehicleTypes
{
    public class Bicycle : Vehicle
    {
        public Bicycle(string regNumber, int vehicleSize, decimal pricePerHour) : base(regNumber)
        {
            VehicleSize = vehicleSize;
            PricePerHour = pricePerHour;
        }
        // Override methods from Vehicle class
        public override string PrintParkingReceipt()
        {
            decimal parkingFee = ParkingFee();
            return string.Format($"\nBICYCLYE {RegNumber}\nArrival Time: {ArrivalTime:dd/MM/yyyy HH:mm}\nDeparture Time: {DepartureTime:dd/MM/yyyy HH:mm}\n" +
                $"Price per Hour: {PricePerHour} CZK\nParking fee: {parkingFee} CZK\n");
        }
        public override string ToString()
        {
            return string.Format($"\nBICYCLE {RegNumber}\nArrival Time: {ArrivalTime:dd/MM/yyyy HH:mm}\n" +
                $"Price per Hour: {PricePerHour} CZK");
        }
    }
}
