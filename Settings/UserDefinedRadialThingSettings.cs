using Blish_HUD.Input;
using Blish_HUD.Settings;
using Manlaan.Mounts;
using Manlaan.Mounts.Things;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mounts.Settings
{
    internal class UserDefinedRadialThingSettings : RadialThingSettings
    {
        public int Id { get; }
        public Func<KeybindTriggerType, Task> _callback { get; }
        public override string Name { get => NameSetting.Value; }

        public SettingEntry<string> NameSetting;
        public SettingEntry<KeyBinding> Keybind;

        public UserDefinedRadialThingSettings(SettingCollection settingCollection, int id, Func<KeybindTriggerType, Task> callback)
            : base(settingCollection, $"RadialThingSettings{id}", true, new List<Thing>())
        {
            Id = id;
            _callback = callback;
            NameSetting = settingCollection.DefineSetting($"RadialThingSettings{Id}Name", "");
            Keybind = settingCollection.DefineSetting($"RadialThingSettings{Id}Keybind", new KeyBinding(Keys.None));
            Keybind.Value.Enabled = true;
            Keybind.Value.Activated += async delegate { await _callback.Invoke(KeybindTriggerType.UserDefined); };
            Keybind.Value.BlockSequenceFromGw2 = Module._settingBlockSequenceFromGw2.Value;
        }

        public override void DeleteFromSettings(SettingCollection settingCollection)
        {
            settingCollection.UndefineSetting($"RadialThingSettings{Id}Name");
            settingCollection.UndefineSetting($"RadialThingSettings{Id}Keybind");
            Keybind.Value.Enabled = false;
            base.DeleteFromSettings(settingCollection);
        }

        public override SettingEntry<KeyBinding> GetKeybind()
        {
            return Keybind;
        }

        public override bool GetIsApplicable()
        {
            return true;
        }
    }
}
