using System;
using System.Collections.Generic;
using System.Text;

namespace Week45_MiniProject_AssetTrackingSystem.Models
{
    // A Computer IS an Asset (inheritance), so it gets all Asset code.
    public class Computer : Asset
    {
        // "override" fills in the abstract AssetType property from Asset.
        // If we forgot this line, the code would not compile - the compiler
        // forces every subclass to keep the "promise" made by the abstract class.
        public override string AssetType => "Computer";

        public Computer(
            string brand,
            string model,
            DateTime purchaseDate,
            decimal priceEur,
            Office office)
            // ": base(...)" passes the values on to the Asset constructor
            : base(brand, model, purchaseDate, priceEur, office)
        {
        }
    }
}
