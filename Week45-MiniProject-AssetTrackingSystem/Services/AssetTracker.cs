using System;
using System.Collections.Generic;
using System.Text;
using Week45_MiniProject_AssetTrackingSystem.Models;

namespace Week45_MiniProject_AssetTrackingSystem.Services
{
    // AssetTracker - owns the list of assets
    // ONE class is responsible for the data: adding, removing, finding and sorting assets.
    // The menu (ConsoleMenu) only asks AssetTracker for things - it never touches the list directly.
    public class AssetTracker
    {
        private readonly List<Asset> assets = new List<Asset>();

        // How many assets there are (read-only from the outside)
        public int Count => assets.Count;

        public void AddAsset(Asset asset)
        {
            assets.Add(asset);
        }

        // Removes the asset with the given ID. Returns true if it was found and removed, false if no such ID.
        public bool RemoveAsset(int id)
        {
            Asset? asset = FindById(id);

            if (asset is null)
            {
                return false;
            }

            assets.Remove(asset);
            return true;
        }

        // Finds one asset by ID, or returns null if there is none. FirstOrDefault (LINQ) returns the first match, or null if no match.
        public Asset? FindById(int id)
        {
            return assets.FirstOrDefault(asset => asset.Id == id);
        }

        // Returns the assets sorted by office, then by purchase date.
        // Office is an enum, so it sorts in the order the offices are declared in Office.cs (Helsingborg, Oslo, Mariehamn).
        public List<Asset> GetSortedAssets()
        {
            return assets
                .OrderBy(asset => asset.Office)
                .ThenBy(asset => asset.PurchaseDate)
                .ToList();
        }
    }
}
