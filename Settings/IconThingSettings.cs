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
        public int Id { get; }

        public SettingEntry<string> Name;
        public SettingEntry<bool> IsEnabled;
        public SettingEntry<bool> DisplayCornerIcons;

        public SettingEntry<IconOrientation> Orientation;
        public SettingEntry<Point> Location;
        public SettingEntry<bool> IsDraggingEnabled;
        public SettingEntry<int> Size;
        public SettingEntry<float> Opacity;

        public bool IsDefault => Id == 0;

        public bool ShouldDisplayCornerIcons => IsDefault && DisplayCornerIcons.Value;

        public event EventHandler<SettingsUpdatedEvent> IconSettingsUpdated;

        public IconThingSettings(SettingCollection settingCollection, int id, string defaultName = "", List<Thing> defaultThings = null)
            : base(settingCollection, defaultThings, $"IconThingSettings{id}Things")
        {
            Id = id;
            Name = settingCollection.DefineSetting($"IconThingSettings{id}name", defaultName);
            IsEnabled = settingCollection.DefineSetting($"IconThingSettings{id}IsEnabled", true);
            DisplayCornerIcons = settingCollection.DefineSetting($"IconThingSettings{id}DisplayCornerIcons", false);
            Orientation = settingCollection.DefineSetting($"IconThingSettings{id}Orientation", IconOrientation.Horizontal);
            Location = settingCollection.DefineSetting($"IconThingSettings{id}Location", new Point(100,100));
            IsDraggingEnabled = settingCollection.DefineSetting($"IconThingSettings{id}IsDragging", false);
            Size = settingCollection.DefineSetting($"IconThingSettings{id}Size", 50);
            Size.SetRange(0, 200);
            Opacity = settingCollection.DefineSetting($"IconThingSettings{id}Opacity", 1.0f);
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
