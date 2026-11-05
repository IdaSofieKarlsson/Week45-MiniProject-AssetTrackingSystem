using System;
using System.Collections.Generic;
using System.Text;

namespace Week45_MiniProject_AssetTrackingSystem.Models
{
    // A Computer IS an Asset (inheritance), so it gets all Asset properties.
    public class Computer : Asset
    {
        public Computer(
        string brand,
        string model,
        DateTime purchaseDate,
        decimal priceEur,
        decimal localPrice,
        string localCurrency,
        string office)
        // ": base(...)" passes the values on to the Asset constructor,
        // with the asset type fixed to "Computer".
        : base("Computer", brand, model, purchaseDate, priceEur, localPrice, localCurrency, office)
        {
        }
    }
}
