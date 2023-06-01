using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading.Tasks;

namespace Manlaan.Mounts.Things.Mounts
{
    public abstract class Mount : IThing
    {
        private readonly Helper _helper;
        private static readonly Logger Logger = Logger.GetLogger<Mount>();

        public Mount(SettingCollection settingCollection, Helper helper,
            string name, string displayName, string imageFileName,
            bool isUnderwaterMount, bool isFlyingMount, bool isWvWMount, int defaultOrderSetting)
        {
            _helper = helper;
            Name = name;
            DisplayName = displayName;
            ImageFileName = imageFileName;
            IsWaterMount = isUnderwaterMount;
            IsFlyingMount = isFlyingMount;
            IsWvWMount = isWvWMount;
            OrderSetting = settingCollection.DefineSetting($"Mount{name}Order2", defaultOrderSetting, () => $"{displayName} Order", () => "");
            KeybindingSetting = settingCollection.DefineSetting($"Mount{name}Binding", new KeyBinding(Keys.None), () => $"{displayName} Binding", () => "");
            ImageFileNameSetting = settingCollection.DefineSetting($"Mount{name}ImageFileName", "", () => $"{displayName} Image File Name", () => "");
        }

        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public string ImageFileName { get; private set; }

        protected abstract MountType MountType { get; }
        public DateTime? QueuedTimestamp { get; internal set; }
        public DateTime? LastUsedTimestamp { get; internal set; }
        public bool IsWaterMount { get; private set; }
        public bool IsFlyingMount { get; private set; }
        public bool IsWvWMount { get; private set; }


        public SettingEntry<int> OrderSetting { get; private set; }
        public SettingEntry<KeyBinding> KeybindingSetting { get; private set; }
        public SettingEntry<string> ImageFileNameSetting { get; private set; }
        public CornerIcon CornerIcon { get; private set; }
        public bool IsAvailable => OrderSetting.Value != 0 && IsKeybindSet;
        public bool IsKeybindSet => KeybindingSetting.Value.ModifierKeys != ModifierKeys.None || KeybindingSetting.Value.PrimaryKey != Keys.None;

        public async Task DoUnmountAction()
        {
            await _helper.TriggerKeybind(KeybindingSetting);
        }

        public async Task DoMountAction()
        {
            if (GameService.Gw2Mumble.PlayerCharacter.IsInCombat)
            {
                QueuedTimestamp = DateTime.UtcNow;
                return;
            }

            if (!Module.IsMountSwitchable())
            {
                _helper.StoreMountForLaterUse(this, GameService.Gw2Mumble.PlayerCharacter.Name);
                Logger.Debug($"DoMountAction StoreMountForLaterUse: {DisplayName}");
                return;
            }

            if (GameService.Gw2Mumble.PlayerCharacter.CurrentMount == MountType.None)
            {
                LastUsedTimestamp = DateTime.UtcNow;
            }

            await _helper.TriggerKeybind(KeybindingSetting);
        }

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
            CornerIcon.Click += async delegate { await DoMountAction(); };
        }

        public void DisposeCornerIcon()
        {
            CornerIcon?.Dispose();
        }

        public bool IsInUse() {
            return GameService.Gw2Mumble.PlayerCharacter.CurrentMount == MountType;
        }
    }
}
