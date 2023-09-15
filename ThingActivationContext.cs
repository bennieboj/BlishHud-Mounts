using System;
using System.Collections.Generic;

namespace Manlaan.Mounts
{
    public class ThingActivationContext
    {
        public readonly string Name;
        public readonly int Order;
        public readonly Func<bool> IsApplicable;
        public readonly List<Type> ThingTypes;
        public readonly bool ApplyInstantlyIfSingle;

        public ThingActivationContext(string name, int order, Func<bool> isApplicable, bool applyInstantlyIfSingle, List<Type> thingTypes)
        {
            Name = name;
            Order = order;
            IsApplicable = isApplicable;
            ApplyInstantlyIfSingle = applyInstantlyIfSingle;
            ThingTypes = thingTypes;
        }
    }
}
