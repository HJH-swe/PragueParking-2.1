using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core.VehicleTypes
{
    public class Car : Vehicle
    {
        public Car(string regNumber, int vehicleSize, int pricePerHour) : base(regNumber)
        {
            VehicleSize = vehicleSize;
            PricePerHour = pricePerHour;
        }
        // Override methods from Vehicle class
        public override string PrintParkingReceipt()
        {
            decimal parkingFee = ParkingFee();
            return string.Format($"\nCAR {RegNumber}\nArrival Time: {ArrivalTime:dd/MM/yyyy HH:mm}\nDeparture Time: {DepartureTime:dd/MM/yyyy HH:mm}\n" +
                $"Price per Hour: {PricePerHour} CZK\nParking fee: {parkingFee} CZK\n");
        }
        public override string ToString()
        {
            return string.Format($"\nCAR {RegNumber}\nArrival Time: {ArrivalTime:dd/MM/yyyy HH:mm}\n" +
                $"Price per Hour: {PricePerHour} CZK");
        }
    }
}
