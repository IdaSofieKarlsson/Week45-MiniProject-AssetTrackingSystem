using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Week45_MiniProject_AssetTrackingSystem.Models;
using Week45_MiniProject_AssetTrackingSystem.Services;

namespace Week45_MiniProject_AssetTrackingSystem.UI
{
    // ------------------------------------------------------------------
    // ConsoleMenu - everything the user sees and types
    // ------------------------------------------------------------------
    // This class handles the menu, the input and the printing.
    // It does NOT store any assets itself - it asks AssetTracker.
    //
    // Splitting the program like this is called "separation of concerns":
    //   Models    -> what an asset IS            (Asset, Computer, ...)
    //   Services  -> what we DO with assets      (AssetTracker, CurrencyConverter)
    //   UI        -> how the user TALKS to us    (ConsoleMenu)
    // ------------------------------------------------------------------
    public class ConsoleMenu
    {
        private readonly AssetTracker tracker;

        // The menu gets the tracker through its constructor, instead of
        // creating one itself. Program.cs decides which tracker to use.
        public ConsoleMenu(AssetTracker tracker)
        {
            this.tracker = tracker;
        }

        // ==============================================================
        // Run - the main menu loop
        // ==============================================================
        // Shows the menu again and again until the user chooses Exit.
        public void Run()
        {
            bool running = true;

            while (running)
            {
                Console.WriteLine();
                Console.WriteLine("=== TRERIKSRÖSETS IT-LÖSNINGAR - ASSET TRACKING ===");
                Console.WriteLine("1. Show all assets");
                Console.WriteLine("2. Add asset");
                Console.WriteLine("3. Remove asset");
                Console.WriteLine("4. Show exchange rates");
                Console.WriteLine("0. Exit");

                int choice = ReadIntInRange("Choose an option: ", 0, 4);
                Console.WriteLine();

                // A switch statement runs the case that matches "choice"
                switch (choice)
                {
                    case 1:
                        PrintAssetList();
                        break;
                    case 2:
                        AddAsset();
                        break;
                    case 3:
                        RemoveAsset();
                        break;
                    case 4:
                        PrintRateInfo();
                        break;
                    case 0:
                        running = false; // The while loop stops after this round
                        Console.WriteLine("Goodbye!");
                        break;
                }
            }
        }

        // ==============================================================
        // Menu option 1: Show all assets
        // ==============================================================
        private void PrintAssetList()
        {
            List<Asset> assets = tracker.GetSortedAssets();
            DateTime today = DateTime.Today;

            if (assets.Count == 0)
            {
                Console.WriteLine("There are no assets yet.");
                return;
            }

            // Negative numbers in {0,-5} mean "left-align in a column 5 characters wide".
            // Positive numbers ({7,11}) mean "right-align" - good for prices.
            const string rowFormat = "{0,-5}{1,-13}{2,-10}{3,-10}{4,-15}{5,-12}{6,-8}{7,11}{8,16}  {9}";
            string line = new string('-', 110);

            Console.WriteLine("ASSET LIST");
            Console.WriteLine(line);
            Console.WriteLine(rowFormat, "ID", "Office", "Type", "Brand", "Model", "Purchased", "Age", "Price EUR", "Local Price", "Status");
            Console.WriteLine(line);

            foreach (Asset asset in assets)
            {
                LifecycleStatus status = asset.GetStatus(today);

                // Set the text colour for this row based on the status
                Console.ForegroundColor = GetColor(status);

                // POLYMORPHISM in action: "asset" is declared as Asset, but
                // asset.AssetType runs the Computer, MobilePhone or Tablet
                // version depending on what the object really is.
                Console.WriteLine(
                    rowFormat,
                    asset.Id,
                    asset.Office,
                    asset.AssetType,
                    asset.Brand,
                    asset.Model,
                    asset.PurchaseDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    asset.GetAgeText(today),
                    asset.PriceEur.ToString("0.00", CultureInfo.InvariantCulture),
                    $"{asset.LocalPrice.ToString("0.00", CultureInfo.InvariantCulture)} {asset.LocalCurrency}",
                    GetStatusText(status));

                // Always reset, otherwise the colour "leaks" into the next row
                Console.ResetColor();
            }

            Console.WriteLine(line);
            Console.WriteLine($"Total: {assets.Count} assets");
        }

        // ==============================================================
        // Menu option 2: Add asset
        // ==============================================================
        private void AddAsset()
        {
            Console.WriteLine("ADD ASSET");
            Console.WriteLine("1. Computer");
            Console.WriteLine("2. Phone");
            Console.WriteLine("3. Tablet");
            int typeChoice = ReadIntInRange("Asset type: ", 1, 3);

            string brand = ReadRequiredText("Brand: ");
            string model = ReadRequiredText("Model: ");
            DateTime purchaseDate = ReadPurchaseDate("Purchase date (yyyy-MM-dd): ");
            decimal priceEur = ReadPositiveDecimal("Price in EUR: ");
            Office office = ReadOffice();

            // Create the right kind of object. The variable is of type Asset,
            // but the object is a Computer, MobilePhone or Tablet.
            // A switch expression returns one value based on typeChoice.
            Asset newAsset = typeChoice switch
            {
                1 => new Computer(brand, model, purchaseDate, priceEur, office),
                2 => new MobilePhone(brand, model, purchaseDate, priceEur, office),
                _ => new Tablet(brand, model, purchaseDate, priceEur, office)
            };

            tracker.AddAsset(newAsset);

            // Uses the overridden ToString() from Asset
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Added: {newAsset}");
            Console.ResetColor();
        }

        // ==============================================================
        // Menu option 3: Remove asset
        // ==============================================================
        private void RemoveAsset()
        {
            int id = ReadIntInRange("ID of the asset to remove: ", 1, int.MaxValue);
            Asset? asset = tracker.FindById(id);

            if (asset is null)
            {
                Console.WriteLine($"No asset with ID {id}.");
                return;
            }

            // Ask before deleting - removing cannot be undone
            Console.Write($"Remove {asset}? (y/n): ");
            string answer = (Console.ReadLine() ?? "").Trim().ToLower();

            if (answer == "y")
            {
                tracker.RemoveAsset(id);
                Console.WriteLine("Asset removed.");
            }
            else
            {
                Console.WriteLine("Nothing was removed.");
            }
        }

        // ==============================================================
        // Menu option 4: Exchange rate info
        // ==============================================================
        // "public" so Program.cs can also show it once at startup
        public void PrintRateInfo()
        {
            if (CurrencyConverter.IsUsingLiveRates)
            {
                // "!" after RatesDate tells the compiler "I know this is not null here",
                // because IsUsingLiveRates is only true when a date was set.
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

            string sek = CurrencyConverter.GetRate(Currency.SEK).ToString("0.0000", CultureInfo.InvariantCulture);
            string nok = CurrencyConverter.GetRate(Currency.NOK).ToString("0.0000", CultureInfo.InvariantCulture);
            Console.WriteLine($"1 EUR = {sek} SEK | 1 EUR = {nok} NOK");
        }

        // ==============================================================
        // Input helpers
        // ==============================================================
        // Users make typos. Each helper keeps asking until the input is
        // valid, so the rest of the code can trust the values it gets.
        // "TryParse" methods return true/false instead of crashing on bad
        // input, and put the parsed value in the "out" variable.

        // Reads a whole number between min and max (inclusive)
        private static int ReadIntInRange(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();

                if (int.TryParse(input, out int value) && value >= min && value <= max)
                {
                    return value;
                }

                PrintError("Please enter a valid number.");
            }
        }

        // Reads text that must not be empty
        private static string ReadRequiredText(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                // "?? """ = if ReadLine returns null, use an empty string instead
                string input = (Console.ReadLine() ?? "").Trim();

                // IsNullOrWhiteSpace: true for "", "   " and null
                if (!string.IsNullOrWhiteSpace(input))
                {
                    return input;
                }

                PrintError("This field cannot be empty.");
            }
        }

        // Reads a date in the format yyyy-MM-dd that is not in the future
        private static DateTime ReadPurchaseDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();

                // TryParseExact only accepts EXACTLY the given format,
                // so "2024-3-5" or "05/03/2024" are rejected.
                bool isValid = DateTime.TryParseExact(
                    input,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime date);

                if (isValid && date <= DateTime.Today)
                {
                    return date;
                }

                PrintError("Please enter a date like 2025-03-15 (not in the future).");
            }
        }

        // Reads a price above 0. Accepts both "1299.50" and "1299,50"
        private static decimal ReadPositiveDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                // Replace a Swedish decimal comma with a dot, so both work
                string input = (Console.ReadLine() ?? "").Trim().Replace(',', '.');

                if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value) && value > 0)
                {
                    return value;
                }

                PrintError("Please enter a price above 0, e.g. 1299.50");
            }
        }

        // Lets the user pick an office from a numbered list.
        // The list is built from the Office enum, so a new office added
        // to the enum shows up here automatically.
        private static Office ReadOffice()
        {
            Office[] offices = Enum.GetValues<Office>();

            for (int i = 0; i < offices.Length; i++)
            {
                // Show 1-based numbers to the user (arrays start at 0)
                Console.WriteLine($"{i + 1}. {offices[i]}");
            }

            int choice = ReadIntInRange("Office: ", 1, offices.Length);
            return offices[choice - 1];
        }

        // Prints an error message in red
        private static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        // ==============================================================
        // Colour helpers
        // ==============================================================
        // Maps a status to a console colour
        private static ConsoleColor GetColor(LifecycleStatus status) => status switch
        {
            LifecycleStatus.Red => ConsoleColor.Red,
            LifecycleStatus.Yellow => ConsoleColor.Yellow,
            _ => ConsoleColor.Gray // "_" means "anything else"
        };

        // Text shown in the Status column (empty for assets that are fine)
        private static string GetStatusText(LifecycleStatus status) => status switch
        {
            LifecycleStatus.Red => "RED",
            LifecycleStatus.Yellow => "YELLOW",
            _ => ""
        };
    }
}
