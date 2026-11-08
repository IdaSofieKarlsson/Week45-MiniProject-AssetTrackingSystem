using System;
using System.Collections.Generic;
using System.Text;
using Week45_MiniProject_AssetTrackingSystem.Services;

namespace Week45_MiniProject_AssetTrackingSystem.Models
{
    // ------------------------------------------------------------------
    // Asset - ABSTRACT base class for every company asset
    // ------------------------------------------------------------------
    // "abstract" means:
    //   1. You can NEVER create a plain Asset with "new Asset(...)".
    //      That makes sense: there is no such thing as "just an asset" in
    //      the office - it is always a computer, a phone or a tablet.
    //   2. The class can contain ABSTRACT members: members that have NO
    //      code here, and that every subclass MUST write (override).
    //
    // Computer, MobilePhone and Tablet inherit from Asset. They get all the
    // shared code below for free (INHERITANCE), and each of them fills in
    // its own version of the abstract members (POLYMORPHISM).
    // ------------------------------------------------------------------
    public abstract class Asset
    {
        // --------------------------------------------------------------
        // Unique Asset ID
        // --------------------------------------------------------------
        // "static" = the field belongs to the CLASS, not to each object.
        // There is only ONE nextId shared by all assets, so it works as a
        // counter: the first asset gets 1, the next gets 2, and so on.
        // (In Level 5, when assets are loaded from a JSON file, we will
        // make sure the counter continues after the highest saved ID.)
        private static int nextId = 1;

        // Each object's own ID. Only a getter: it is set once in the
        // constructor and can never be changed afterwards.
        public int Id { get; }

        // --------------------------------------------------------------
        // ABSTRACT property - every subclass must provide its own value
        // --------------------------------------------------------------
        // There is no code here, only the "promise" that every asset has
        // an AssetType. Computer says "Computer", MobilePhone says "Phone".
        // When the program reads asset.AssetType on an item in a
        // List<Asset>, C# automatically runs the version belonging to the
        // REAL object type. That is polymorphism.
        public abstract string AssetType { get; }

        // --------------------------------------------------------------
        // VIRTUAL property - has a default, subclasses MAY change it
        // --------------------------------------------------------------
        // Difference between abstract and virtual:
        //   abstract = no default, subclass MUST override
        //   virtual  = has a default, subclass CAN override if it wants
        // Business rule: all assets last 3 years. No subclass overrides
        // this today, but the design makes it easy if the company ever
        // decides that e.g. phones should be replaced after 2 years.
        public virtual int LifespanYears => 3;

        // Properties shared by all assets
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

        // Calculated property: purchase date + lifespan
        public DateTime EndOfLifeDate => PurchaseDate.AddYears(LifespanYears);

        // --------------------------------------------------------------
        // Asset age calculation
        // --------------------------------------------------------------
        // Returns how many WHOLE months old the asset is on a given date.
        // Example: bought 2024-03-15, today 2026-11-07
        //   year difference  : 2026 - 2024 = 2  -> 2 * 12 = 24 months
        //   month difference : 11 - 3      = 8  -> 24 + 8 = 32 months
        //   day check        : 7 < 15, the 15th has not been reached yet
        //                      this month, so subtract 1 -> 31 months
        public int GetAgeInMonths(DateTime today)
        {
            int months = (today.Year - PurchaseDate.Year) * 12 + (today.Month - PurchaseDate.Month);

            if (today.Day < PurchaseDate.Day)
            {
                months--;
            }

            // Math.Max protects against negative ages if a purchase date
            // were ever set in the future
            return Math.Max(months, 0);
        }

        // Age as readable text, e.g. 31 months -> "2y 7m"
        public string GetAgeText(DateTime today)
        {
            int totalMonths = GetAgeInMonths(today);

            // Integer division (/) drops the decimals: 31 / 12 = 2
            // Modulo (%) gives the remainder:            31 % 12 = 7
            int years = totalMonths / 12;
            int months = totalMonths % 12;

            return $"{years}y {months}m";
        }

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

        // --------------------------------------------------------------
        // OVERRIDE of ToString()
        // --------------------------------------------------------------
        // Every class in C# inherits from "object", which has a virtual
        // ToString() method. By overriding it, Console.WriteLine(asset)
        // prints something useful instead of the class name.
        // It uses AssetType, so the text automatically differs between
        // Computer, MobilePhone and Tablet - polymorphism again.
        public override string ToString() => $"#{Id} {AssetType} {Brand} {Model} ({Office})";

        // Constructor: runs when a new asset object is created with "new".
        // "protected" means only subclasses (Computer, MobilePhone, Tablet)
        // can call it - another reminder that Asset itself is abstract.
        protected Asset(
            string brand,
            string model,
            DateTime purchaseDate,
            decimal priceEur,
            Office office)
        {
            // Give this asset the next free ID, then move the counter on.
            // "nextId++" uses the current value FIRST, then adds 1.
            Id = nextId++;

            Brand = brand;
            Model = model;
            PurchaseDate = purchaseDate;
            PriceEur = priceEur;
            Office = office;
        }
    }
}


