using PragueParking.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core
{
    public class Vehicle : IParkable
    {
        // Constructor
        public Vehicle(string regNumber)
        {
            RegNumber = regNumber;
            ArrivalTime = DateTime.Now;
        }
        // Properties
        public string RegNumber { get; init; }
        public int VehicleSize { get; set; }
        public DateTime ArrivalTime { get; init; }
        public DateTime DepartureTime { get; set; }
        public decimal PricePerHour { get; set; }

        // Mehtods
        public int HoursToCharge()
        {
            TimeSpan parkedTime = DepartureTime - ArrivalTime;
            double chargedHours = parkedTime.TotalHours - (10.0 / 60.0);     // first 10 mins free, so subract 10 mins as a fraction of an hour

            // If no fee to be charged, return 0
            if (chargedHours <= 0)
            {
                return 0;
            }

            // Math.Ceiling rounds up to next whole number --> same as hours started
            return (int)Math.Ceiling(chargedHours);
        }

        public decimal ParkingFee()
        {
            DepartureTime = DateTime.Now;
            decimal parkingFee = HoursToCharge() * PricePerHour;
            return parkingFee;
        }

        public virtual string PrintParkingReceipt()     // TODO: overload method in each subclass?
        {
            decimal parkingFee = ParkingFee();
            return string.Format($"Arrival Time: {ArrivalTime:dd/MM/yyyy HH:mm}\nDeparture Time: {DepartureTime:dd/MM/yyyy HH:mm}\n" +
                $"Price per Hour: {PricePerHour} CZK\nParking fee: {parkingFee} CZK\n");
        }
        public override string ToString()       // TODO: override method in each subclass? Or remove from Vehicle class?
        {
            return string.Format($"\n{RegNumber}\nArrival Time: {ArrivalTime:dd/MM/yyyy HH:mm}\nPrice per Hour: {PricePerHour} CZK");
        }
    }
}
