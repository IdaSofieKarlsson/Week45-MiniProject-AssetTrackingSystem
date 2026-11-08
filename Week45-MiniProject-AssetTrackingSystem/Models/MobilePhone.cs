using System;
using System.Collections.Generic;
using System.Text;

namespace Week45_MiniProject_AssetTrackingSystem.Models
{
    // A MobilePhone IS an Asset (inheritance), so it gets all Asset code.
    public class MobilePhone : Asset
    {
        // Fills in the abstract AssetType property from Asset
        public override string AssetType => "Phone";

        public MobilePhone(
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
