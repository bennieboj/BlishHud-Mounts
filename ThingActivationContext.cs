using Blish_HUD.Settings;
using Manlaan.Mounts.Things;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Manlaan.Mounts
{
    public class ThingActivationContext
    {
        public readonly string Name;
        public readonly int Order;
        public readonly Func<bool> IsApplicable;

        public SettingEntry<bool> IsEnabledSetting;
        public SettingEntry<bool> ApplyInstantlyIfSingleSetting;
        private SettingEntry<IList<Type>> ThingsSetting;

        public ThingActivationContext(SettingCollection settingCollection, string name, int order, Func<bool> isApplicable, bool defaultIsEnabled, bool defaultApplyInstantlyIfSingle, IList<Thing> defaultThings)
        {
            Name = name;
            Order = order;
            IsApplicable = isApplicable;

            IsEnabledSetting = settingCollection.DefineSetting($"ThingActivationContext{name}IsEnabled", defaultIsEnabled);
            ApplyInstantlyIfSingleSetting = settingCollection.DefineSetting($"ThingActivationContext{name}ApplyInstantlyIfSingle", defaultApplyInstantlyIfSingle);
            ThingsSetting = settingCollection.DefineSetting($"ThingActivationContext{name}Things", (IList<Type>) defaultThings.Select(t => t.GetType()).ToList());

        }

        public void SetThings(IEnumerable<Thing> defaultThings)
        {
            ThingsSetting.Value = defaultThings.Select(t => t.GetType()).ToList();
        }

        public IList<Thing> Things => Module._things.Where(t => ThingsSetting.Value.Any(typeOfThingInContext => typeOfThingInContext == t.GetType())).ToList();

        public void AddThing(Thing thingToAdd)
        {
            ThingsSetting.Value.Add(thingToAdd.GetType());
        }

        public void RemoveThing(Thing thingToAdd)
        {
            ThingsSetting.Value.Remove(thingToAdd.GetType());
        }
    }
}
