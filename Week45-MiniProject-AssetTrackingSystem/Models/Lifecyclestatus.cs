using System;
using System.Collections.Generic;
using System.Text;


namespace Week45_MiniProject_AssetTrackingSystem.Models
// An enum is a fixed set of named values.
// Using it instead of plain strings ("RED", "YELLOW") means the compiler
// catches typos, and we can only ever have one of these three values.
{
    public enum LifecycleStatus
    {
        Ok,      // More than 6 months left
        Yellow,  // Less than 6 months left
        Red      // Less than 3 months left (or already past end of life)
    }
}
