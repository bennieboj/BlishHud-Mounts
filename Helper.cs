using Blish_HUD;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Manlaan.Mounts
{
    public class Helper
    {
        private static readonly Logger Logger = Logger.GetLogger<Helper>();
        public Mount MountOnHide = null;
        public string CharacterNameOnHide;

        Gw2Sharp.Models.MapType[] warclawOnlyMaps = {
                Gw2Sharp.Models.MapType.RedBorderlands,
                Gw2Sharp.Models.MapType.BlueBorderlands,
                Gw2Sharp.Models.MapType.GreenBorderlands,
                Gw2Sharp.Models.MapType.EternalBattlegrounds,
                Gw2Sharp.Models.MapType.Center,
                Gw2Sharp.Models.MapType.WvwLounge
            };

        private bool IsPlayerInWvWMap()
        {
            return Array.Exists(warclawOnlyMaps, mapType => mapType == GameService.Gw2Mumble.CurrentMap.Type);
        }
        private bool IsPlayerGlidingOrFalling()
        {
            return Module.IsPlayerGlidingOrFalling;
        }

        private bool IsPlayerUnderOrCloseToWater()
        {
            return GameService.Gw2Mumble.PlayerCharacter.Position.Z <= 0;
        }

        private Mount GetFlyingMount()
        {
            return Module._mounts.SingleOrDefault(m => m.IsFlyingMount && m.Name == Module._settingDefaultFlyingMountChoice.Value);
        }

        private static Mount GetWaterMount()
        {
            return Module._mounts.SingleOrDefault(m => m.IsWaterMount && m.Name == Module._settingDefaultWaterMountChoice.Value);
        }

        internal Mount GetInstantMount()
        {
            if (IsPlayerInWvWMap())
            {
                return Module._mounts.Single(m => m.IsWvWMount);
            }

            if (IsPlayerGlidingOrFalling())
            {
                return GetFlyingMount();
            }

            if (IsPlayerUnderOrCloseToWater())
            {
                return GetWaterMount();
            }

            return null;
        }

        internal Mount GetCenterMount()
        {
            if (Module._settingMountRadialCenterMountBehavior.Value == "Default")
                return GetDefaultMount();
            if (Module._settingMountRadialCenterMountBehavior.Value == "LastUsed")
                return GetLastUsedMount();
             return null;
        }

        internal Mount GetDefaultMount()
        {
            return Module._mounts.SingleOrDefault(m => m.Name == Module._settingDefaultMountChoice.Value);
        }
        internal Mount GetLastUsedMount()
        {
            return Module._mounts.Where(m => m.LastUsedTimestamp != null).OrderByDescending(m => m.LastUsedTimestamp).FirstOrDefault();
        }

        public async Task TriggerKeybind(SettingEntry<KeyBinding> keybindingSetting)
        {
            Logger.Debug("TriggerKeybind entered");
            if (keybindingSetting.Value.ModifierKeys != ModifierKeys.None)
            {
                Logger.Debug($"TriggerKeybind press modifiers {keybindingSetting.Value.ModifierKeys}");
                if (keybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Alt))
                    Blish_HUD.Controls.Intern.Keyboard.Press(VirtualKeyShort.MENU, false);
                if (keybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Ctrl))
                    Blish_HUD.Controls.Intern.Keyboard.Press(VirtualKeyShort.CONTROL, false);
                if (keybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Shift))
                    Blish_HUD.Controls.Intern.Keyboard.Press(VirtualKeyShort.SHIFT, false);
            }
            Logger.Debug($"TriggerKeybind press PrimaryKey {keybindingSetting.Value.PrimaryKey}");
            Blish_HUD.Controls.Intern.Keyboard.Press(ToVirtualKey(keybindingSetting.Value.PrimaryKey), false);
            await Task.Delay(50);
            Logger.Debug($"TriggerKeybind release PrimaryKey {keybindingSetting.Value.PrimaryKey}");
            Blish_HUD.Controls.Intern.Keyboard.Release(ToVirtualKey(keybindingSetting.Value.PrimaryKey), false);
            if (keybindingSetting.Value.ModifierKeys != ModifierKeys.None)
            {
                Logger.Debug($"TriggerKeybind release modifiers {keybindingSetting.Value.ModifierKeys}");
                if (keybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Shift))
                    Blish_HUD.Controls.Intern.Keyboard.Release(VirtualKeyShort.SHIFT, false);
                if (keybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Ctrl))
                    Blish_HUD.Controls.Intern.Keyboard.Release(VirtualKeyShort.CONTROL, false);
                if (keybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Alt))
                    Blish_HUD.Controls.Intern.Keyboard.Release(VirtualKeyShort.MENU, false);
            }
        }


        private VirtualKeyShort ToVirtualKey(Keys key)
        {
            try
            {
                return (VirtualKeyShort)key;
            }
            catch
            {
                return new VirtualKeyShort();
            }
        }
    }
}
