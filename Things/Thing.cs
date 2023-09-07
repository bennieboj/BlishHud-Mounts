using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading.Tasks;

namespace Manlaan.Mounts.Things
{
    public abstract class Thing
    {
        public Thing(SettingCollection settingCollection, Helper helper, string name, string displayName, string imageFileName, int defaultOrderSetting)
        {
            _helper = helper;
            Name = name;
            DisplayName = displayName;
            ImageFileName = imageFileName;
            OrderSetting = settingCollection.DefineSetting($"Mount{name}Order2", defaultOrderSetting, () => $"{displayName} Order", () => "");
            KeybindingSetting = settingCollection.DefineSetting($"Mount{name}Binding", new KeyBinding(Keys.None), () => $"{displayName} Binding", () => "");
            ImageFileNameSetting = settingCollection.DefineSetting($"Mount{name}ImageFileName", $"{imageFileName}.png", () => $"{displayName} Image File Name", () => "");
        }

        protected static readonly Logger Logger = Logger.GetLogger<Thing>();
        protected readonly Helper _helper;

        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public string ImageFileName { get; private set; }
        public SettingEntry<int> OrderSetting { get; private set; }
        public SettingEntry<KeyBinding> KeybindingSetting { get; private set; }
        public SettingEntry<string> ImageFileNameSetting { get; private set; }
        public CornerIcon CornerIcon { get; private set; }
        public DateTime? QueuedTimestamp { get; internal set; }
        public DateTime? LastUsedTimestamp { get; internal set; }
        public bool IsKeybindSet => KeybindingSetting.Value.ModifierKeys != ModifierKeys.None || KeybindingSetting.Value.PrimaryKey != Keys.None;
        public bool IsAvailable => OrderSetting.Value != 0 && IsKeybindSet;

        public void CreateCornerIcon(Texture2D img)
        {
            CornerIcon?.Dispose();
            CornerIcon = new CornerIcon()
            {
                IconName = DisplayName,
                Icon = img,
                HoverIcon = img,
                Priority = 10
            };
            CornerIcon.Click += async delegate { await DoAction(); };
        }

        public void DisposeCornerIcon()
        {
            CornerIcon?.Dispose();
        }

        public async Task DoAction()
        {
            if (GameService.Gw2Mumble.PlayerCharacter.IsInCombat)
            {
                QueuedTimestamp = DateTime.UtcNow;
                return;
            }

            if (!Module.IsMountSwitchable())
            {
                _helper.StoreThingForLaterUse(this, GameService.Gw2Mumble.PlayerCharacter.Name);
                Logger.Debug($"DoAction StoreMountForLaterUse: {DisplayName}");
                return;
            }

            LastUsedTimestamp = DateTime.UtcNow;

            await Helper.TriggerKeybind(KeybindingSetting);
        }

        public async Task DoReverseAction()
        {
            await Helper.TriggerKeybind(KeybindingSetting);
        }

        public abstract bool IsInUse();

        public abstract bool IsUsableOnMount();
        public abstract bool IsInstactActionApplicable();
        public abstract int IsInstactActionApplicableOrder();
    }
}
