using System;
using System.Collections.Generic;
using System.Text;


namespace Week45_MiniProject_AssetTrackingSystem.Models

{
    public enum LifecycleStatus
    {
        Ok,      // More than 6 months left
        Yellow,  // Less than 6 months left
        Red      // Less than 3 months left (or already past end of life)
    }
}
