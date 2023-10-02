using Blish_HUD.Settings;
using System.Collections.Generic;
using Manlaan.Mounts.Things;
using System.Linq;

namespace Manlaan.Mounts
{
    public abstract class ThingsSettings
    {
        protected SettingEntry<IList<string>> ThingsSetting;
        private string ThingSettingsName;

        protected ThingsSettings(SettingCollection settingCollection, IEnumerable<Thing> things, string thingSettingsName)
        {
            ThingSettingsName = thingSettingsName;
            if(things == null){
                things = new List<Thing>();
            }

            ThingsSetting = settingCollection.DefineSetting(ThingSettingsName, (IList<string>)things.Select(t => t.GetType().FullName).ToList());
        }

        public void SetThings(IEnumerable<Thing> things)
        {
            ThingsSetting.Value = things.Select(t => t.GetType().FullName).ToList();
        }

        public ICollection<Thing> Things => ThingsSetting.Value.Select(typeOfThingInSettings => Module._things.Single(t => typeOfThingInSettings == t.GetType().FullName)).ToList();

        public ICollection<Thing> AvailableThings => Things.Where(t => t.IsAvailable).ToList();

        public void AddThing(Thing thingToAdd)
        {
            //update by assignment to trigger SettingChanged, not by modification of the value itself (would not trigger SettingChanged)
            ThingsSetting.Value = ThingsSetting.Value.Append(thingToAdd.GetType().FullName).ToList();
        }

        public void RemoveThing(Thing thingToRemove)
        {
            //update by assignment to trigger SettingChanged, not by modification of the value itself (would not trigger SettingChanged)
            ThingsSetting.Value = ThingsSetting.Value.Where(t => t != thingToRemove.GetType().FullName).ToList();
        }
        public virtual void DeleteFromSettings(SettingCollection settingCollection)
        {
            settingCollection.UndefineSetting(ThingSettingsName);
        }
    }
}