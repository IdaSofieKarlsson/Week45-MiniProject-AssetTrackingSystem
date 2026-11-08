using System;
using System.Collections.Generic;
using System.Text;

namespace Week45_MiniProject_AssetTrackingSystem.Models
{
    // The offices of Treriksrösets IT-lösningar.
    // When sorting on an enum, C# uses the order the values are declared in (not alphabetical order), so the head office is listed first.
    public enum Office
    {
        Helsingborg, // Head office, Sweden
        Oslo,        // Norway
        Mariehamn    // Åland, Finland
    }
}
