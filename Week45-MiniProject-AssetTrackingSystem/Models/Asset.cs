using System;
using System.Collections.Generic;
using System.Text;

namespace Week45_MiniProject_AssetTrackingSystem.Models
{
    // Base class for every company asset.
    // Computer and MobilePhone inherit from this class, which lets us
    // store both kinds of objects in the same List<Asset>.
    public class Asset
    {
        // Properties shared by all assets
        public string AssetType { get; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public DateTime PurchaseDate { get; set; }

        // Price in the company's base currency (EUR)
        public decimal PriceEur { get; set; }

        // Price in the office's local currency.
        // Hardcoded for now - currency conversion via API comes in Level 3.
        public decimal LocalPrice { get; set; }
        public string LocalCurrency { get; set; }

        public string Office { get; set; }

        // Constructor: runs when a new Asset object is created with "new".
        // "protected" means only subclasses (Computer, MobilePhone) can call it.
        protected Asset(
            string assetType,
            string brand,
            string model,
            DateTime purchaseDate,
            decimal priceEur,
            decimal localPrice,
            string localCurrency,
            string office)
        {
            AssetType = assetType;
            Brand = brand;
            Model = model;
            PurchaseDate = purchaseDate;
            PriceEur = priceEur;
            LocalPrice = localPrice;
            LocalCurrency = localCurrency;
            Office = office;
        }
    }
}
