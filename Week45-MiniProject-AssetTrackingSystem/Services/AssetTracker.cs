using System;
using System.Collections.Generic;
using System.Text;
using Week45_MiniProject_AssetTrackingSystem.Models;

namespace Week45_MiniProject_AssetTrackingSystem.Services
{
    // ------------------------------------------------------------------
    // AssetTracker - owns the list of assets
    // ------------------------------------------------------------------
    // Before Level 4, the List<Asset> lived directly in Program.cs.
    // Now ONE class is responsible for the data: adding, removing,
    // finding and sorting assets. The menu (ConsoleMenu) only asks
    // AssetTracker for things - it never touches the list directly.
    //
    // This is called ENCAPSULATION: the list is private, and the only
    // way to change it is through the public methods below. In Level 5,
    // saving to / loading from a JSON file will also go in here.
    // ------------------------------------------------------------------
    public class AssetTracker
    {
        // The actual list. "private" = no code outside this class can reach it.
        // It is a List<Asset>, so it can hold Computers, MobilePhones and
        // Tablets at the same time (they are all Assets).
        private readonly List<Asset> assets = new List<Asset>();

        // How many assets there are (read-only from the outside)
        public int Count => assets.Count;

        public void AddAsset(Asset asset)
        {
            assets.Add(asset);
        }

        // Removes the asset with the given ID.
        // Returns true if it was found and removed, false if no such ID.
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

        // Finds one asset by ID, or returns null if there is none.
        // FirstOrDefault (LINQ) returns the first match, or null if no match.
        public Asset? FindById(int id)
        {
            return assets.FirstOrDefault(asset => asset.Id == id);
        }

        // Returns the assets sorted by office, then by purchase date.
        // Office is an enum, so it sorts in the order the offices are
        // declared in Office.cs (Helsingborg, Oslo, Mariehamn).
        //
        // Returns a NEW list - the private list itself is never handed out,
        // so nobody outside can add or remove items behind our back.
        public List<Asset> GetSortedAssets()
        {
            return assets
                .OrderBy(asset => asset.Office)
                .ThenBy(asset => asset.PurchaseDate)
                .ToList();
        }
    }
}
