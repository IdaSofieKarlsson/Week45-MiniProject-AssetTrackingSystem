using Week45_MiniProject_AssetTrackingSystem.Models;
using System.Collections;
using System.Globalization;
using Week45_MiniProject_AssetTrackingSystem.Services;

// Treriksrösets IT-lösningar - Asset Tracking System

// Fetch today's exchange rates ONCE, before anything else happens.
// "await" waits for the API call to finish before the program continues,
// so every price below is converted with the newest rates.
// (With top-level statements in Program.cs we can use "await" directly -
//  C# makes the hidden Main method async for us.)
Console.WriteLine("Fetching exchange rates...");
await CurrencyConverter.LoadRatesAsync();
PrintRateInfo();
Console.WriteLine();

// Prices are entered in EUR. Local price and currency are calculated from the office
List<Asset> assets = new List<Asset>
{
    new Computer("Apple", "MacBook Pro", new DateTime(2024, 3, 15), 1500m, Office.Helsingborg),
    new Computer("Lenovo", "ThinkPad X1", new DateTime(2023, 10, 1), 1300m, Office.Oslo),
    new Computer("Asus", "ZenBook", new DateTime(2025, 6, 2), 1100m, Office.Mariehamn),
    new Computer("Dell", "Latitude 7440", new DateTime(2024, 4, 10), 1250m, Office.Mariehamn),
    new MobilePhone("Samsung", "Galaxy S23", new DateTime(2025, 1, 10), 850m, Office.Mariehamn),
    new MobilePhone("Apple", "iPhone 15", new DateTime(2024, 12, 5), 950m, Office.Helsingborg),
    new MobilePhone("Nokia", "XR20", new DateTime(2024, 1, 20), 450m, Office.Oslo),
    new MobilePhone("Google", "Pixel 8", new DateTime(2025, 3, 18), 700m, Office.Oslo)
};

// LINQ sorting: first by office, then by purchase date within each office.
// Office is an enum, so it sorts in the order the offices are declared in Office.cs (Helsingborg, Oslo, Mariehamn) - not alphabetically.
List<Asset> sortedAssets = assets
    .OrderBy(asset => asset.Office)
    .ThenBy(asset => asset.PurchaseDate)
    .ToList();

PrintAssetList(sortedAssets, DateTime.Today);

// Prints all assets as a table, coloured by end-of-life status
static void PrintAssetList(List<Asset> assets, DateTime today)
{
    // Negative numbers in {0,-12} mean "left-align in a column 12 characters wide".
    // Positive numbers ({5,12}) mean "right-align" - good for prices.
    const string rowFormat = "{0,-13}{1,-10}{2,-10}{3,-15}{4,-15}{5,12}{6,16}  {7}";
    string line = new string('-', 101);

    Console.WriteLine("ASSET LIST - Treriksrösets IT-lösningar");
    Console.WriteLine(line);
    Console.WriteLine(rowFormat, "Office", "Type", "Brand", "Model", "Purchase Date", "Price (EUR)", "Local Price", "Status");
    Console.WriteLine(line);

    foreach (Asset asset in assets)
    {
        LifecycleStatus status = asset.GetStatus(today);

        // Set the text colour for this row based on the status
        Console.ForegroundColor = GetColor(status);

        Console.WriteLine(
            rowFormat,
            asset.Office,
            asset.AssetType,
            asset.Brand,
            asset.Model,
            // InvariantCulture keeps the date and number format the same on every computer
            asset.PurchaseDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            asset.PriceEur.ToString("0.00", CultureInfo.InvariantCulture),
            $"{asset.LocalPrice.ToString("0.00", CultureInfo.InvariantCulture)} {asset.LocalCurrency}",
            GetStatusText(status));

        // Always reset, otherwise the colour "leaks" into the next row
        Console.ResetColor();
    }

    Console.WriteLine(line);
}

// Shows where the exchange rates came from, and the rates themselves
static void PrintRateInfo()
{
    if (CurrencyConverter.IsUsingLiveRates)
    {
        // "!" after RatesDate tells the compiler "I know this is not null here", because IsUsingLiveRates is only true when a date was set.
        string date = CurrencyConverter.RatesDate!.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        Console.WriteLine($"Exchange rates from the European Central Bank, {date}");
    }
    else
    {
        // Yellow warning: the program still works, but with approximate rates
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"WARNING: {CurrencyConverter.ErrorMessage}");
        Console.WriteLine("Using approximate backup exchange rates instead.");
        Console.ResetColor();
    }

    // Prints e.g. "1 EUR = 11.0123 SEK | 1 EUR = 11.7456 NOK"
    string sek = CurrencyConverter.GetRate(Currency.SEK).ToString("0.0000", CultureInfo.InvariantCulture);
    string nok = CurrencyConverter.GetRate(Currency.NOK).ToString("0.0000", CultureInfo.InvariantCulture);
    Console.WriteLine($"1 EUR = {sek} SEK | 1 EUR = {nok} NOK");
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