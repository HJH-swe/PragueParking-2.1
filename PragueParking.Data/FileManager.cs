using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PragueParking.Core;
using PragueParking.Core.Interfaces;

namespace PragueParking.Data
{
    public class FileManager        // TODO: update methods for bus and bicycle
    {
        public FileManager()
        {
        }

        public string SaveParkingData<T>(T parkingSpaces, string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(parkingSpaces, options);
                File.WriteAllText(filePath, jsonString);
                return $"Parking data saved to {filePath}";
            }
            catch (Exception e)
            {
                return $"Error saving data to {filePath}: {e.Message}";
            }
        }

        public List<IParkingSpace> LoadParkingData(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new List<IParkingSpace>();
            }

            string jsonString = File.ReadAllText(filePath);
            List<ParkingSpace>? savedData = JsonSerializer.Deserialize<List<ParkingSpace>>(jsonString);       // Error here - could not deserialize interface type.
            //return savedData ?? new List<ParkingSpace>();       
            return savedData?.Cast<IParkingSpace>().ToList() ?? new List<IParkingSpace>();      // Copilot helped me with this line, following above error ^
                                                                                                // Use ParkingSpace for json, and then cast to IParkingSpace
                                                                                                // if savedData is null, return a new list of IParkingSpace
        }

        public GarageConfiguration? ConfigureParkingGarage(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                return null;
            }

            string jsonString = File.ReadAllText(configFilePath);
            var configuration = JsonSerializer.Deserialize<GarageConfiguration>(jsonString);

            return configuration;
        }

        public PriceListConfiguration? ConfigurePriceList(string priceListFilePath)
        {
            if (!File.Exists(priceListFilePath))
            {
                return null;
            }

            // Use Streamreader to read pricelist file
            using (StreamReader sr = new StreamReader(priceListFilePath))
            {
                PriceListConfiguration priceConfiguration = new PriceListConfiguration();

                priceConfiguration.PriceList = new PriceList();

                // Go through pricelist.txt line by line - find what's relevant
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    // if the line is a comment, starts with #, skip
                    if (line.StartsWith("#"))
                    {
                        continue;
                    }
                    // Set PriceList properties directly
                    if (line.StartsWith("CAR"))
                    {
                        priceConfiguration.PriceList.CarVehiclePrice = Convert.ToInt32(line.Substring(line.IndexOf("=") + 1)); // CAR.price[=]20
                    }
                    else if (line.StartsWith("MC"))
                    {
                        priceConfiguration.PriceList.MCVehiclePrice = Convert.ToInt32(line.Substring(line.IndexOf("=") + 1));
                    }
                    else if (line.StartsWith("BUS"))
                    {
                        priceConfiguration.PriceList.BusVehiclePrice = Convert.ToInt32(line.Substring(line.IndexOf("=") + 1));
                    }
                    else if (line.StartsWith("BICYCLE"))
                    {
                        priceConfiguration.PriceList.BicycleVehiclePrice = Convert.ToInt32(line.Substring(line.IndexOf("=") + 1));
                    }
                }
                return priceConfiguration;
            }
        }
    }
}
