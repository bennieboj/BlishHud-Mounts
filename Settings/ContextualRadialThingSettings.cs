using Blish_HUD;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Manlaan.Mounts;
using Manlaan.Mounts.Things;
using System;
using System.Collections.Generic;

namespace Mounts.Settings
{
    internal class ContextualRadialThingSettings : RadialThingSettings
    {
        private readonly string _name;
        public readonly int Order;
        public readonly Func<bool> IsApplicable;
        public SettingEntry<bool> ApplyInstantlyIfSingle;
        public SettingEntry<string> ApplyInstantlyOnTap;
        public SettingEntry<string> ApplyInstantlyOnHold;
        public SettingEntry<bool> UnconditionallyDoAction;

        public bool IsDefault => Order == 99;

        public override string Name { get => _name; }

        public ContextualRadialThingSettings(SettingCollection settingCollection, string name, int order, Func<bool> isApplicable, bool defaultIsEnabled, bool defaultApplyInstantlyIfSingle, bool defaultUnconditionallyDoAction, IList<Thing> defaultThings)
            : base(settingCollection, $"RadialThingSettings{name}", defaultIsEnabled, defaultThings)
        {
            _name = name;
            Order = order;
            IsApplicable = isApplicable;
            ApplyInstantlyIfSingle = settingCollection.DefineSetting($"RadialThingSettings{_name}ApplyInstantlyIfSingle", defaultApplyInstantlyIfSingle);
            ApplyInstantlyOnTap = settingCollection.DefineSetting($"RadialThingSettings{_name}ApplyInstantlyOnTap", "Disabled");
            ApplyInstantlyOnHold = settingCollection.DefineSetting($"RadialThingSettings{_name}ApplyInstantlyOnHold", "Disabled");
            UnconditionallyDoAction = settingCollection.DefineSetting($"RadialThingSettings{_name}UnconditionallyDoAction", defaultUnconditionallyDoAction);
        }

        public override SettingEntry<KeyBinding> GetKeybind()
        {
            return Module._settingDefaultMountBinding;
        }

        public override bool GetIsApplicable()
        {
            return IsApplicable();
        }

        public bool IsTapAndHoldBothApplicable()
        {
            return ThingsSetting.Value.Count == 2 && ApplyInstantlyOnHold.Value != "Disabled" && ApplyInstantlyOnTap.Value != "Disabled";
        }
    }
}
