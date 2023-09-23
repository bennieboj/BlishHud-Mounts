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

        protected ThingsSettings(SettingCollection settingCollection, IEnumerable<Thing> things, string thingSettingsName)
        {
            ThingsSetting = settingCollection.DefineSetting(thingSettingsName, (IList<Type>)things.Select(t => t.GetType()).ToList());
        }

        public void SetThings(IEnumerable<Thing> things)
        {
            ThingsSetting.Value = things.Select(t => t.GetType()).ToList();
        }

        public ICollection<Thing> Things => ThingsSetting.Value.Select(typeOfThingInSettings => Module._things.Single(t => typeOfThingInSettings == t.GetType())).ToList();

        public void AddThing(Thing thingToAdd)
        {
            //update by assignment to trigger SettingChanged, not by modification of the value itself (would not trigger SettingChanged)
            ThingsSetting.Value = ThingsSetting.Value.Append(thingToAdd.GetType()).ToList();
        }

        public void RemoveThing(Thing thingToRemove)
        {
            //update by assignment to trigger SettingChanged, not by modification of the value itself (would not trigger SettingChanged)
            ThingsSetting.Value = ThingsSetting.Value.Where(t => t != thingToRemove.GetType()).ToList();
        }
    }
}