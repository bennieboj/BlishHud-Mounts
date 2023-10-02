using Blish_HUD;
using Blish_HUD.Settings;
using Manlaan.Mounts.Things;
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

        public SettingEntry<bool> IsEnabled;
        public SettingEntry<bool> ApplyInstantlyIfSingle;
        public SettingEntry<CenterBehavior> CenterThingBehavior;
        public SettingEntry<bool> RemoveCenterMount;
        public SettingEntry<string> DefaultThingChoice;

        public bool IsDefault => Order == 99;

        public RadialThingSettings(SettingCollection settingCollection, string name, int order, Func<bool> isApplicable, bool defaultIsEnabled, bool defaultApplyInstantlyIfSingle, IList<Thing> defaultThings)
            : base(settingCollection, defaultThings, $"RadialThingSettings{name}Things")
        {
            Name = name;
            Order = order;
            IsApplicable = isApplicable;

            IsEnabled = settingCollection.DefineSetting($"RadialThingSettings{name}IsEnabled", defaultIsEnabled);
            ApplyInstantlyIfSingle = settingCollection.DefineSetting($"RadialThingSettings{name}ApplyInstantlyIfSingle", defaultApplyInstantlyIfSingle);

            CenterThingBehavior = settingCollection.DefineSetting($"RadialThingSettings{name}CenterThingBehavior", CenterBehavior.None);
            RemoveCenterMount = settingCollection.DefineSetting($"RadialThingSettings{name}RemoveCenterThingFromRadial", true);
            DefaultThingChoice = settingCollection.DefineSetting($"RadialThingSettings{name}DefaultMountChoice", "Disabled");

            ThingsSetting.SettingChanged += ThingsSetting_SettingChanged;
        }

        private void ThingsSetting_SettingChanged(object sender, ValueChangedEventArgs<IList<string>> e)
        {
            ApplyInstantlyIfSingle.Value = ThingsSetting.Value.Count == 1;
            if (GetDefaultThing() == null)
            {
                DefaultThingChoice.Value = "Disabled";
            }
        }

        internal Thing GetCenterThing()
        {
            if (CenterThingBehavior.Value == CenterBehavior.Default)
                return GetDefaultThing();
            if (CenterThingBehavior.Value == CenterBehavior.LastUsed)
                return GetLastUsedThing();
            return null;
        }

        internal Thing GetDefaultThing()
        {
            return Things.SingleOrDefault(m => m.Name == DefaultThingChoice.Value);
        }

        internal Thing GetLastUsedThing()
        {
            return Things.Where(m => m.LastUsedTimestamp != null).OrderByDescending(m => m.LastUsedTimestamp).FirstOrDefault();
        }
    }
}
