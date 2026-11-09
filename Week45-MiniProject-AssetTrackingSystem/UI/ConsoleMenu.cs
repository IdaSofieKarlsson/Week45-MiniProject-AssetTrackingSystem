using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Week45_MiniProject_AssetTrackingSystem.Models;
using Week45_MiniProject_AssetTrackingSystem.Services;

namespace Week45_MiniProject_AssetTrackingSystem.UI
{
    // ConsoleMenu - everything the user sees and types
    // This class handles the menu, the input and the printing. It does NOT store any assets itself - it asks AssetTracker.
    // Splitting the program like this is called "separation of concerns":
    //   Models    -> what an asset IS            (Asset, Computer, ...)
    //   Services  -> what we DO with assets      (AssetTracker, CurrencyConverter)
    //   UI        -> how the user TALKS to us    (ConsoleMenu)
    public class ConsoleMenu
    {
        private readonly AssetTracker tracker;

        // The menu gets the tracker through its constructor, instead of creating one itself. Program.cs decides which tracker to use.
        public ConsoleMenu(AssetTracker tracker)
        {
            this.tracker = tracker;
        }

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

        // Menu option 1: Show all assets
        private void PrintAssetList()
        {
            List<Asset> assets = tracker.GetSortedAssets();
            DateTime today = DateTime.Today;

            if (assets.Count == 0)
            {
                Console.WriteLine("There are no assets yet.");
                return;
            }

            // Dynamic column widths
            //   1. Turn every asset into a row of text cells
            //   2. Measure the longest text in each column (header included)
            //   3. Pad every cell to that width
            // The table now adapts to whatever data is in the list.
            string[] headers = { "ID", "Office", "Type", "Brand", "Model", "Purchased", "Age", "Price EUR", "Local Price", "Status" };

            // Columns that should be right-aligned
            // 7 = Price EUR, 8 = Local Price (counting from 0)
            int[] rightAlignedColumns = { 7, 8 };

            // Step 1: build the text for every row.
            // "asset" is declared as Asset, but
            List<string[]> rows = new List<string[]>();

            foreach (Asset asset in assets)
            {
                rows.Add(new string[]
                {
                    asset.Id.ToString(),
                    asset.Office.ToString(),
                    asset.AssetType,
                    asset.Brand,
                    asset.Model,
                    asset.PurchaseDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    asset.GetAgeText(today),
                    asset.PriceEur.ToString("0.00", CultureInfo.InvariantCulture),
                    $"{asset.LocalPrice.ToString("0.00", CultureInfo.InvariantCulture)} {asset.LocalCurrency}",
                    GetStatusText(asset.GetStatus(today))
                });
            }

            // Step 2: find the widest text in each column.
            // Start with the header's length, then grow if any row is wider.
            int[] widths = new int[headers.Length];

            for (int col = 0; col < headers.Length; col++)
            {
                widths[col] = headers[col].Length;

                foreach (string[] row in rows)
                {
                    widths[col] = Math.Max(widths[col], row[col].Length);
                }
            }

            // Step 3: a local function that pads one row of cells to the column widths and joins them with two spaces between columns.
            // PadRight(n) = left-aligned, PadLeft(n) = right-aligned.
            string FormatRow(string[] cells)
            {
                List<string> parts = new List<string>();

                for (int col = 0; col < cells.Length; col++)
                {
                    parts.Add(rightAlignedColumns.Contains(col)
                        ? cells[col].PadLeft(widths[col])
                        : cells[col].PadRight(widths[col]));
                }

                // TrimEnd removes trailing spaces after the last column
                return string.Join("  ", parts).TrimEnd();
            }

            // The separator line is as wide as the table: all column widths + 2 spaces between each pair of columns
            string line = new string('-', widths.Sum() + 2 * (headers.Length - 1));

            Console.WriteLine("ASSET LIST");
            Console.WriteLine(line);
            Console.WriteLine(FormatRow(headers));
            Console.WriteLine(line);

            for (int i = 0; i < assets.Count; i++)
            {
                // Set the text colour for this row based on the status
                Console.ForegroundColor = GetColor(assets[i].GetStatus(today));

                Console.WriteLine(FormatRow(rows[i]));

                // Always reset, otherwise the colour "leaks" into the next row
                Console.ResetColor();
            }

            Console.WriteLine(line);
            Console.WriteLine($"Total: {assets.Count} assets");
        }

        // Menu option 2: Add asset
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

            // Create the right kind of object. The variable is of type Asset, but the object is a Computer, MobilePhone or Tablet.
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

        // Menu option 3: Remove asset
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

        // Menu option 4: Exchange rate info
        // "public" so Program.cs can also show it once at startup
        public void PrintRateInfo()
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

            string sek = CurrencyConverter.GetRate(Currency.SEK).ToString("0.0000", CultureInfo.InvariantCulture);
            string nok = CurrencyConverter.GetRate(Currency.NOK).ToString("0.0000", CultureInfo.InvariantCulture);
            Console.WriteLine($"1 EUR = {sek} SEK | 1 EUR = {nok} NOK");
        }

        // Input helpers
        // Each helper keeps asking until the input is valid
        // "TryParse" methods return true/false instead of crashing on bad input, and put the parsed value in the "out" variable.
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

                // TryParseExact only accepts EXACTLY the given format, so "2024-3-5" or "05/03/2024" are rejected.
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

        // Lets the user pick an office from a numbered list. The list is built from the Office enum,
        // so a new office added to the enum shows up here automatically.
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