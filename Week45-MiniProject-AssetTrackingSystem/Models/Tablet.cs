using System;
using System.Collections.Generic;
using System.Text;

namespace Week45_MiniProject_AssetTrackingSystem.Models
{
    // A Tablet IS an Asset (inheritance), so it gets all Asset code.
    // Adding a whole new asset type only takes this small class -
    // that is one of the big benefits of the abstract base class.
    public class Tablet : Asset
    {
        // Fills in the abstract AssetType property from Asset
        public override string AssetType => "Tablet";

        public Tablet(
            string brand,
            string model,
            DateTime purchaseDate,
            decimal priceEur,
            Office office)
            : base(brand, model, purchaseDate, priceEur, office)
        {
        }
    }
}
