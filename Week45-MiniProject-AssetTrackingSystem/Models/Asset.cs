using System;
using System.Collections.Generic;
using System.Text;
using Week45_MiniProject_AssetTrackingSystem.Services;

namespace Week45_MiniProject_AssetTrackingSystem.Models
{
    // Asset - ABSTRACT base class for every company asset
    // Computer, MobilePhone and Tablet inherit from Asset. 
    public abstract class Asset
    {
        // Unique Asset ID
        // There is only ONE nextId shared by all assets, so it works as a counter: the first asset gets 1, the next gets 2, and so on.
        private static int nextId = 1;

        // Each object's own ID. Only a getter: it is set once in the constructor and can never be changed afterwards.
        public int Id { get; }

        // ABSTRACT property - every subclass must provide its own value
        public abstract string AssetType { get; }

        // Business rule: all assets last 3 years. No subclass overrides this 
        public virtual int LifespanYears => 3;

        // Properties shared by all assets
        public string Brand { get; set; }
        public string Model { get; set; }
        public DateTime PurchaseDate { get; set; }

        // Purchase price in the company's base currency (EUR)
        public decimal PriceEur { get; set; }

        // Which office the asset belongs to
        public Office Office { get; set; }

        // Calculated properties: nothing is stored, the values are worked out every time they are read.
        // If the asset moves to another office, currency and local price update automatically.
        public Currency LocalCurrency => CurrencyConverter.GetCurrencyForOffice(Office);
        public decimal LocalPrice => CurrencyConverter.ConvertFromEur(PriceEur, LocalCurrency);

        // Calculated property: purchase date + lifespan
        public DateTime EndOfLifeDate => PurchaseDate.AddYears(LifespanYears);

        // Asset age calculation. Returns how many WHOLE months old the asset is on a given date.
        public int GetAgeInMonths(DateTime today)
        {
            int months = (today.Year - PurchaseDate.Year) * 12 + (today.Month - PurchaseDate.Month);

            if (today.Day < PurchaseDate.Day)
            {
                months--;
            }

            // Math.Max protects against negative ages if a purchase date were ever set in the future
            return Math.Max(months, 0);
        }

        // Age as readable text, e.g. 31 months -> "2y 7m"
        public string GetAgeText(DateTime today)
        {
            int totalMonths = GetAgeInMonths(today);
            int years = totalMonths / 12;
            int months = totalMonths % 12;

            return $"{years}y {months}m";
        }

        // Works out the asset's status compared to a given date (usually today). Taking "today" as a parameter makes the method easy to test later.
        public LifecycleStatus GetStatus(DateTime today)
        {
            // The date from which the asset counts as RED / YELLOW
            DateTime redFrom = EndOfLifeDate.AddMonths(-3);
            DateTime yellowFrom = EndOfLifeDate.AddMonths(-6);

            // Check RED first: an asset with less than 3 months left also has less than 6 months left, so the order matters.
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

        public override string ToString() => $"#{Id} {AssetType} {Brand} {Model} ({Office})";

        // Constructor: runs when a new asset object is created with "new"."protected" means only subclasses (Computer, MobilePhone, Tablet) can call it 
        protected Asset(
            string brand,
            string model,
            DateTime purchaseDate,
            decimal priceEur,
            Office office)
        {
            // Give this asset the next free ID, then move the counter on. "nextId++" uses the current value FIRST, then adds 1.
            Id = nextId++;

            Brand = brand;
            Model = model;
            PurchaseDate = purchaseDate;
            PriceEur = priceEur;
            Office = office;
        }
    }
}


