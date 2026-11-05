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
        decimal localPrice,
        string localCurrency,
        string office)
        // The asset type is fixed to "Phone" for every MobilePhone.
        : base("Phone", brand, model, purchaseDate, priceEur, localPrice, localCurrency, office)
        {
        }
    }
}
