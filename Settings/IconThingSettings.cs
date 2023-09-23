using Blish_HUD;
using Blish_HUD.Settings;
using Manlaan.Mounts.Things;
using Microsoft.Xna.Framework;
using Mounts.Settings;
using System;
using System.Collections.Generic;

namespace Manlaan.Mounts
{
    public class IconThingSettings : ThingsSettings
    {
        public readonly string Name;
        public SettingEntry<bool> IsEnabled;
        public SettingEntry<bool> DisplayCornerIcons;

        public SettingEntry<IconOrientation> Orientation;
        public SettingEntry<Point> Location;
        public SettingEntry<bool> IsDraggingEnabled;
        public SettingEntry<int> Size;
        public SettingEntry<float> Opacity;

        public bool IsDefault => Name == "Default";

        public bool ShouldDisplayCornerIcons => IsDefault && DisplayCornerIcons.Value;

        public event EventHandler<SettingsUpdatedEvent> IconSettingsUpdated;

        public IconThingSettings(SettingCollection settingCollection, string name, bool defaultIsEnabled, 
            bool defaultDisplayCornerIcons, IconOrientation defaultOrientation, Point defaultLocation,
            bool defaultIsDraggingEnabled, int defaultSize, float defaultOpacity, IList<Thing> defaultThings)
            : base(settingCollection, defaultThings, $"IconThingSettings{name}Things")
        {
            Name = name;

            IsEnabled = settingCollection.DefineSetting($"IconThingSettings{name}IsEnabled", defaultIsEnabled);
            DisplayCornerIcons = settingCollection.DefineSetting($"IconThingSettings{name}DisplayCornerIcons", defaultDisplayCornerIcons);
            Orientation = settingCollection.DefineSetting($"IconThingSettings{name}Orientation", defaultOrientation);
            Location = settingCollection.DefineSetting($"IconThingSettings{name}Location", defaultLocation);
            IsDraggingEnabled = settingCollection.DefineSetting($"IconThingSettings{name}IsDragging", defaultIsDraggingEnabled);
            Size = settingCollection.DefineSetting($"IconThingSettings{name}Size", defaultSize);
            Size.SetRange(0, 200);
            Opacity = settingCollection.DefineSetting($"IconThingSettings{name}Opacity", defaultOpacity);
            Opacity.SetRange(0f, 1f);


            IsEnabled.SettingChanged += SettingChanged;
            DisplayCornerIcons.SettingChanged += SettingChanged;
            Orientation.SettingChanged += SettingChanged;
            Location.SettingChanged += SettingChanged;
            IsDraggingEnabled.SettingChanged += SettingChanged;
            Size.SettingChanged += SettingChanged;
            Opacity.SettingChanged += SettingChanged;
            ThingsSetting.SettingChanged += ThingsSetting_SettingChanged;
        }
        private void UpdateIconThingSettingsInternal()
        {
            var myevent = new SettingsUpdatedEvent();

            if (IconSettingsUpdated != null)
            {
                IconSettingsUpdated(this, myevent);
            }
        }

        private void ThingsSetting_SettingChanged(object sender, ValueChangedEventArgs<IList<Type>> e)
        {
            UpdateIconThingSettingsInternal();
        }

        private void SettingChanged(object sender, ValueChangedEventArgs<IconOrientation> e)
        {
            UpdateIconThingSettingsInternal();
        }

        private void SettingChanged(object sender, ValueChangedEventArgs<Point> e)
        {
            UpdateIconThingSettingsInternal();
        }

        private void SettingChanged(object sender, ValueChangedEventArgs<int> e)
        {
            UpdateIconThingSettingsInternal();
        }

        private void SettingChanged(object sender, ValueChangedEventArgs<float> e)
        {
            UpdateIconThingSettingsInternal();
        }

        private void SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            UpdateIconThingSettingsInternal();
        }
    }
}
