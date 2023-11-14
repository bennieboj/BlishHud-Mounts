using Blish_HUD;
using Blish_HUD.Settings;
using Manlaan.Mounts.Things;
using Mounts.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Manlaan.Mounts
{
    public abstract class RadialThingSettings : ThingsSettings
    {
        public SettingEntry<bool> IsEnabled;
        public SettingEntry<CenterBehavior> CenterThingBehavior;
        public SettingEntry<bool> RemoveCenterThing;
        public SettingEntry<string> DefaultThingChoice;

        public event EventHandler<SettingsUpdatedEvent> RadialSettingsUpdated;

        public abstract string Name { get; }

        public RadialThingSettings(SettingCollection settingCollection, string settingsPrefix, bool defaultIsEnabled, IList<Thing> defaultThings)
            : base(settingCollection, defaultThings, $"{settingsPrefix}Things")
        {
            IsEnabled = settingCollection.DefineSetting($"{settingsPrefix}IsEnabled", defaultIsEnabled);

            CenterThingBehavior = settingCollection.DefineSetting($"{settingsPrefix}CenterThingBehavior", CenterBehavior.None);
            RemoveCenterThing = settingCollection.DefineSetting($"{settingsPrefix}RemoveCenterThingFromRadial", true);
            DefaultThingChoice = settingCollection.DefineSetting($"{settingsPrefix}DefaultMountChoice", "Disabled");

            ThingsSetting.SettingChanged += ThingsSetting_SettingChanged;
        }

        private void ThingsSetting_SettingChanged(object sender, ValueChangedEventArgs<IList<string>> e)
        {
            if (GetDefaultThing() == null)
            {
                DefaultThingChoice.Value = "Disabled";
            }

            var myevent = new SettingsUpdatedEvent();

            if (RadialSettingsUpdated != null)
            {
                RadialSettingsUpdated(this, myevent);
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
