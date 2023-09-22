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

        public event EventHandler<ThingsUpdatedEventArgs> OnThingsUpdatedEventHandler;

        public void SetThings(IEnumerable<Thing> defaultThings)
        {
            ThingsSetting.Value = defaultThings.Select(t => t.GetType()).ToList();
        }

        private void HandleThingsUpdatedInternal()
        {
            var myevent = new ThingsUpdatedEventArgs();
            myevent.NewCount = Things.Count();

            if (OnThingsUpdatedEventHandler != null)
            {
                OnThingsUpdatedEventHandler(this, myevent);
            }
        }

        public IEnumerable<Thing> Things => ThingsSetting.Value.Select(typeOfThingInSettings => Module._things.Single(t => typeOfThingInSettings == t.GetType()));

        public void AddThing(Thing thingToAdd)
        {
            ThingsSetting.Value = ThingsSetting.Value.Append(thingToAdd.GetType()).ToList();
            HandleThingsUpdatedInternal();
        }

        public void RemoveThing(Thing thingToRemove)
        {
            ThingsSetting.Value.Remove(thingToRemove.GetType());
            HandleThingsUpdatedInternal();
        }
    }
}