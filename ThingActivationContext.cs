using Manlaan.Mounts.Things;
using System;
using System.Collections.Generic;

namespace Manlaan.Mounts
{
    public class ThingActivationContext
    {
        public readonly string Name;
        public readonly int Order;
        public readonly Func<bool> IsApplicable;
        public IList<Thing> Things;
        public bool ApplyInstantlyIfSingle;

        public ThingActivationContext(string name, int order, Func<bool> isApplicable, bool applyInstantlyIfSingle, IList<Thing> things)
        {
            Name = name;
            Order = order;
            IsApplicable = isApplicable;
            ApplyInstantlyIfSingle = applyInstantlyIfSingle;
            Things = things;
        }
    }
}
