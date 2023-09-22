using Blish_HUD.Settings;
using Manlaan.Mounts.Things;
using Mounts.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Manlaan.Mounts
{
    public class RadialThingSettings : ThingsSettings
    {
        public readonly string Name;
        public readonly int Order;
        public readonly Func<bool> IsApplicable;

        public SettingEntry<bool> IsEnabledSetting;
        public SettingEntry<bool> ApplyInstantlyIfSingleSetting;
        

        public RadialThingSettings(SettingCollection settingCollection, string name, int order, Func<bool> isApplicable, bool defaultIsEnabled, bool defaultApplyInstantlyIfSingle, IList<Thing> defaultThings)
        {
            Name = name;
            Order = order;
            IsApplicable = isApplicable;

            IsEnabledSetting = settingCollection.DefineSetting($"RadialThingSettings{name}IsEnabled", defaultIsEnabled);
            ApplyInstantlyIfSingleSetting = settingCollection.DefineSetting($"RadialThingSettings{name}ApplyInstantlyIfSingle", defaultApplyInstantlyIfSingle);
            ThingsSetting = settingCollection.DefineSetting($"RadialThingSettings{name}Things", (IList<Type>) defaultThings.Select(t => t.GetType()).ToList());

            base.OnThingsUpdated += OnThingsUpdated;
        }

        private void OnThingsUpdated(object sender, ThingsUpdatedEventArgs e)
        {
            ApplyInstantlyIfSingleSetting.Value = e.NewCount == 1;
        }
    }
}
