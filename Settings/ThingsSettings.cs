using Blish_HUD.Settings;
using System.Collections.Generic;
using System;
using Manlaan.Mounts.Things;
using System.Linq;
using Mounts.Settings;

namespace Manlaan.Mounts
{
    public abstract class ThingsSettings
    {
        protected SettingEntry<IList<Type>> ThingsSetting;

        public event EventHandler<ThingsUpdatedEventArgs> OnThingsUpdated;

        public void SetThings(IEnumerable<Thing> defaultThings)
        {
            ThingsSetting.Value = defaultThings.Select(t => t.GetType()).ToList();
        }

        private void HandleThingsUpdated()
        {
            var myevent = new ThingsUpdatedEventArgs();
            myevent.NewCount = Things.Count();

            if (OnThingsUpdated != null)
            {
                OnThingsUpdated(this, myevent);
            }
        }

        public IEnumerable<Thing> Things => ThingsSetting.Value.Select(typeOfThingInSettings => Module._things.Single(t => typeOfThingInSettings == t.GetType()));

        public void AddThing(Thing thingToAdd)
        {
            ThingsSetting.Value = ThingsSetting.Value.Append(thingToAdd.GetType()).ToList();
            HandleThingsUpdated();
        }

        public void RemoveThing(Thing thingToAdd)
        {
            ThingsSetting.Value.Remove(thingToAdd.GetType());
            HandleThingsUpdated();
        }
    }
}