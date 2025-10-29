using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core.Interfaces
{
    public interface IParkable
    {
        // methods
        int HoursToCharge();
        decimal ParkingFee();
        string PrintParkingReceipt();
        string ToString();      // remove from here?
        // TODO: More methods here?
    }
}
