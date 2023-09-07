using System;
using System.Collections.Generic;

namespace Manlaan.Mounts
{
    public class ThingActivationContext
    {
        public string Name;
        private readonly int Order;
        private Func<bool> IsApplicable;
        private List<Type> ThingTypes;

        public ThingActivationContext(string name, int order, Func<bool> isApplicable, List<Type> thingTypes)
        {
            Name = name;
            Order = order;
            IsApplicable = isApplicable;
            ThingTypes = thingTypes;
        }
    }
}
