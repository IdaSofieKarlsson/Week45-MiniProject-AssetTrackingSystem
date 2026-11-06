using Week45_MiniProject_AssetTrackingSystem.Models;
using System.Collections;
using System.Globalization;

// Treriksrösets IT-lösningar - Asset Tracking System
// A List<Asset> can hold both Computer and MobilePhone objects,
// because both classes inherit from Asset.

List<Asset> assets = new List<Asset>
{
    new Computer("Apple", "MacBook Pro", new DateTime(2024, 3, 15), 1500m, 17250m, "SEK", "Helsingborg"),
    new Computer("Lenovo", "ThinkPad X1", new DateTime(2023, 10, 1), 1300m, 15340m, "NOK", "Oslo"),
    new Computer("Asus", "ZenBook", new DateTime(2025, 6, 2), 1100m, 1100m, "EUR", "Mariehamn"),
    new MobilePhone("Samsung", "Galaxy S23", new DateTime(2025, 1, 10), 850m, 850m, "EUR", "Mariehamn"),
    new MobilePhone("Apple", "iPhone 15", new DateTime(2024, 12, 5), 950m, 10925m, "SEK", "Helsingborg"),
    new MobilePhone("Nokia", "XR20", new DateTime(2024, 1, 20), 450m, 5310m, "NOK", "Oslo")
};

// LINQ sorting: first by asset type, then by purchase date within each type.
// OrderBy/ThenBy return a NEW sorted sequence - the original list is unchanged.
// ToList() turns that sequence back into a List<Asset>.
List<Asset> sortedAssets = assets
    .OrderBy(asset => asset.AssetType)
    .ThenBy(asset => asset.PurchaseDate)
    .ToList();

PrintAssetList(sortedAssets, DateTime.Today);

// Prints all assets as a table, coloured by end-of-life status
static void PrintAssetList(List<Asset> assets, DateTime today)
{
    // Negative numbers in {0,-12} mean "left-align in a column 12 characters wide"
    const string rowFormat = "{0,-10}{1,-10}{2,-14}{3,-15}{4,-13}{5,14}  {6}";
    string line = new string('-', 90);

    Console.WriteLine("ASSET LIST");
    Console.WriteLine(line);
    Console.WriteLine(rowFormat, "Type", "Brand", "Model", "Purchase Date", "Office", "Local Price", "Status");
    Console.WriteLine(line);

    foreach (Asset asset in assets)
    {
        LifecycleStatus status = asset.GetStatus(today);

        // Set the text colour for this row based on the status
        Console.ForegroundColor = GetColor(status);

        Console.WriteLine(
            rowFormat,
            asset.AssetType,
            asset.Brand,
            asset.Model,
            // InvariantCulture keeps the date format the same on every computer
            asset.PurchaseDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            asset.Office,
            $"{asset.LocalPrice.ToString("0.00", CultureInfo.InvariantCulture)} {asset.LocalCurrency}",
            GetStatusText(status));

        // Always reset, otherwise the colour "leaks" into the next row
        Console.ResetColor();
    }

    Console.WriteLine(line);
}

// Maps a status to a console colour.
// A switch expression picks one result based on the value.
static ConsoleColor GetColor(LifecycleStatus status) => status switch
{
    LifecycleStatus.Red => ConsoleColor.Red,
    LifecycleStatus.Yellow => ConsoleColor.Yellow,
    _ => ConsoleColor.Gray // "_" means "anything else"
};

// Text shown in the Status column (empty for assets that are fine)
static string GetStatusText(LifecycleStatus status) => status switch
{
    LifecycleStatus.Red => "RED",
    LifecycleStatus.Yellow => "YELLOW",
    _ => ""
};