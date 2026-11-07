using System;
using System.Collections.Generic;
using System.Text;

namespace Week45_MiniProject_AssetTrackingSystem.Models
{
    // A MobilePhone IS an Asset (inheritance), so it gets all Asset properties.
    public class MobilePhone : Asset
    {
        public MobilePhone(
            string brand,
            string model,
            DateTime purchaseDate,
            decimal priceEur,
            Office office)
            // The asset type is fixed to "Phone" for every MobilePhone.
            : base("Phone", brand, model, purchaseDate, priceEur, office)
        {
        }
    }
}
