using Week45_MiniProject_AssetTrackingSystem.Models;
using System.Collections;
using System.Globalization;
using Week45_MiniProject_AssetTrackingSystem.Services;
using Week45_MiniProject_AssetTrackingSystem.UI;

// Treriksrösets IT-lösningar - Asset Tracking System
// Level 4: Abstract base class, polymorphism, asset IDs, age and a menu

// Program.cs START the program:
//   1. Fetch exchange rates
//   2. Create the tracker and fill it with starting data
//   3. Hand over to the menu
// All the real work happens in the other classes.

// 1. Fetch today's exchange rates ONCE, before anything else happens
Console.WriteLine("Fetching exchange rates...");
await CurrencyConverter.LoadRatesAsync();

// 2. Create the tracker and add some starting assets.
AssetTracker tracker = new AssetTracker();

tracker.AddAsset(new Computer("HP", "Laptop 15s", new DateTime(2023, 3, 15), 1500m, Office.Helsingborg));
tracker.AddAsset(new Computer("Lenovo", "ThinkPad X1", new DateTime(2023, 10, 1), 1300m, Office.Oslo));
tracker.AddAsset(new Computer("Asus", "ZenBook", new DateTime(2025, 6, 2), 1100m, Office.Mariehamn));
tracker.AddAsset(new Computer("Dell", "Latitude 7440", new DateTime(2024, 4, 10), 1250m, Office.Mariehamn));
tracker.AddAsset(new MobilePhone("Samsung", "Galaxy S23", new DateTime(2025, 1, 10), 850m, Office.Mariehamn));
tracker.AddAsset(new MobilePhone("Samsung", "Galaxy A14", new DateTime(2023, 6, 7), 950m, Office.Helsingborg));
tracker.AddAsset(new MobilePhone("Nokia", "XR20", new DateTime(2024, 1, 20), 450m, Office.Oslo));
tracker.AddAsset(new MobilePhone("Google", "Pixel 8", new DateTime(2025, 3, 18), 700m, Office.Oslo));
tracker.AddAsset(new Tablet("Samsung", "Galaxy Tab S12 Ultra", new DateTime(2026, 11, 5), 1590m, Office.Helsingborg));
tracker.AddAsset(new Tablet("Samsung", "Galaxy Tab S9", new DateTime(2025, 9, 1), 900m, Office.Oslo));

// 3. Create the menu, show where the rates came from, and start the menu loop. Run() keeps going until the user chooses 0 (Exit).
ConsoleMenu menu = new ConsoleMenu(tracker);
menu.PrintRateInfo();
menu.Run();
