using Blish_HUD.Settings;
using System.Collections.Generic;
using System;
using Manlaan.Mounts.Things;
using System.Linq;

namespace Manlaan.Mounts
{
    public abstract class ThingsSettings
    {
        protected SettingEntry<IList<Type>> ThingsSetting;

        public void SetThings(IEnumerable<Thing> defaultThings)
        {
            ThingsSetting.Value = defaultThings.Select(t => t.GetType()).ToList();
        }

        public IEnumerable<Thing> Things => ThingsSetting.Value.Select(typeOfThingInContext => Module._things.Single(t => typeOfThingInContext == t.GetType()));

        public void AddThing(Thing thingToAdd)
        {
            ThingsSetting.Value = ThingsSetting.Value.Append(thingToAdd.GetType()).ToList();
        }

        public void RemoveThing(Thing thingToAdd)
        {
            ThingsSetting.Value.Remove(thingToAdd.GetType());
        }
    }
}