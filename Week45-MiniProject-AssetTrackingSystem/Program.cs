//using AssetTracking.Models;
using Week45_MiniProject_AssetTrackingSystem.Models;
using System.Collections;
using System.Globalization;

// Treriksrösets IT-lösningar - Asset Tracking System
// Level 1: Basic classes & objects stored in a List<Asset>

// A List<Asset> can hold both Computer and MobilePhone objects,
// because both classes inherit from Asset.
List<Asset> assets = new List<Asset>
{
    new Computer("Apple", "MacBook Pro", new DateTime(2024, 3, 15), 1500m, 17250m, "SEK", "Helsingborg"),
    new Computer("Lenovo", "ThinkPad X1", new DateTime(2023, 10, 1), 1300m, 15340m, "NOK", "Oslo"),
    new MobilePhone("Samsung", "Galaxy S23", new DateTime(2025, 1, 10), 850m, 850m, "EUR", "Mariehamn"),
    new MobilePhone("Apple", "iPhone 15", new DateTime(2024, 12, 5), 950m, 10925m, "SEK", "Helsingborg")
};

PrintAssetList(assets);

// Prints all assets as a table in the console
static void PrintAssetList(List<Asset> assets)
{
    // Negative numbers in {0,-12} mean "left-align in a column 12 characters wide"
    const string rowFormat = "{0,-12}{1,-12}{2,-15}{3,-15}{4,-12}{5,14}";
    string line = new string('-', 80);

    Console.WriteLine("ASSET LIST");
    Console.WriteLine(line);
    Console.WriteLine(rowFormat, "Type", "Brand", "Model", "Purchase Date", "Office", "Local Price");
    Console.WriteLine(line);

    foreach (Asset asset in assets)
    {
        Console.WriteLine(
            rowFormat,
            asset.AssetType,
            asset.Brand,
            asset.Model,
            // InvariantCulture keeps the date format the same on every computer
            asset.PurchaseDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            asset.Office,
            $"{asset.LocalPrice.ToString("0.00", CultureInfo.InvariantCulture)} {asset.LocalCurrency}");
    }

    Console.WriteLine(line);
}