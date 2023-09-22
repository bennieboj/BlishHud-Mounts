using System;

namespace Mounts.Settings
{
    public class ThingsUpdatedEventArgs : EventArgs
    {
        public int NewCount { get; set; }
    }
}
