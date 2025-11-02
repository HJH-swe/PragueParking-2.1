using PragueParking.Core;
using PragueParking.Core.Interfaces;
using PragueParking.Core.VehicleTypes;
namespace PragueParking.Tests
{
    [TestClass]
    public sealed class PragueParkingTests
    {
        [TestMethod]
        public void AddVehicle_AddMotorbikeToEmptySpace_AvailableSpaceResultTwo()
        {
            // Arrange
            // Create mc and parking space
            MC myMc = new MC("XYZ789", 2, 10);
            ParkingSpace myParkingSpace = new ParkingSpace(4, 20, new List<Vehicle>());

            // Act
            // Add mc to space and check available size
            myParkingSpace.AddVehicle(myMc);
            int result = myParkingSpace.AvailableSize;
            int expected = 2; 

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void FindVehicleInSpace_FindCarInSpaceWithMotorbike_ResultNull()
        {
            // Arrange
            // Create var, mc and parking space
            Car myCar = new Car("ABC123", 4, 20);
            MC myMc = new MC("XYZ789", 2, 10);
            ParkingSpace myParkingSpace = new ParkingSpace(4, 20, new List<Vehicle>());

            // Act
            // Add mc to space, and search for car in space
            myParkingSpace.AddVehicle(myMc);
            var result = myParkingSpace.FindVehicleInSpace("ABC123");
            IParkable expected = null;

            // Assert
            Assert.AreEqual(expected, result);
        }

    }
}
