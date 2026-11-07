using System;
using System.Collections.Generic;
using System.Text;
using Week45_MiniProject_AssetTrackingSystem.Services;

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

        // Purchase price in the company's base currency (EUR)
        public decimal PriceEur { get; set; }

        // Which office the asset belongs to
        public Office Office { get; set; }

        // Calculated properties: nothing is stored, the values are worked out
        // every time they are read. If the asset moves to another office,
        // currency and local price update automatically.
        public Currency LocalCurrency => CurrencyConverter.GetCurrencyForOffice(Office);
        public decimal LocalPrice => CurrencyConverter.ConvertFromEur(PriceEur, LocalCurrency);

        // Business rule: all company assets have a lifespan of 3 years
        public const int LifespanYears = 3;

        // Calculated property: purchase date + 3 years
        public DateTime EndOfLifeDate => PurchaseDate.AddYears(LifespanYears);

        // Works out the asset's status compared to a given date (usually today).
        // Taking "today" as a parameter makes the method easy to test later.
        public LifecycleStatus GetStatus(DateTime today)
        {
            // The date from which the asset counts as RED / YELLOW
            DateTime redFrom = EndOfLifeDate.AddMonths(-3);
            DateTime yellowFrom = EndOfLifeDate.AddMonths(-6);

            // Check RED first: an asset with less than 3 months left
            // also has less than 6 months left, so the order matters.
            if (today >= redFrom)
            {
                return LifecycleStatus.Red;
            }

            if (today >= yellowFrom)
            {
                return LifecycleStatus.Yellow;
            }

            return LifecycleStatus.Ok;
        }

        // Constructor: runs when a new Asset object is created with "new".
        // "protected" means only subclasses (Computer, MobilePhone) can call it.
        // Local price and currency are no longer passed in - they are calculated.
        protected Asset(
            string assetType,
            string brand,
            string model,
            DateTime purchaseDate,
            decimal priceEur,
            Office office)
        {
            AssetType = assetType;
            Brand = brand;
            Model = model;
            PurchaseDate = purchaseDate;
            PriceEur = priceEur;
            Office = office;
        }
    }
}


