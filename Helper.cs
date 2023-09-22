using Blish_HUD;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Gw2Sharp.Models;
using Manlaan.Mounts.Things;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Manlaan.Mounts
{
    public class Helper
    {
        private static readonly Logger Logger = Logger.GetLogger<Helper>();

        private Thing ThingOnHide = null;
        private string CharacterNameOnHide;

        private float _lastZPosition = 0.0f;
        private double _lastUpdateSeconds = 0.0f;
        private bool _isPlayerGlidingOrFalling = false;


        public bool IsPlayerGlidingOrFalling()
        {
            return _isPlayerGlidingOrFalling;
        }

        public bool IsPlayerInWvwMap()
        {
            MapType[] warclawOnlyMaps = {
                MapType.RedBorderlands,
                MapType.BlueBorderlands,
                MapType.GreenBorderlands,
                MapType.EternalBattlegrounds,
                MapType.Center,
                MapType.WvwLounge
            };
            return Array.Exists(warclawOnlyMaps, mapType => mapType == GameService.Gw2Mumble.CurrentMap.Type);
        }

        public bool IsPlayerUnderWater()
        {
            return GameService.Gw2Mumble.PlayerCharacter.Position.Z <= -1.2;
        }

        public bool IsPlayerOnWaterSurface()
        {
            var zpos = GameService.Gw2Mumble.PlayerCharacter.Position.Z;
            return zpos > -1.2 && zpos < 0;
        }

        public bool IsPlayerMounted()
        {
            return GameService.Gw2Mumble.PlayerCharacter.CurrentMount != MountType.None;
        }

        public void UpdatePlayerGlidingOrFalling(GameTime gameTime)
        {
            var currentZPosition = GameService.Gw2Mumble.PlayerCharacter.Position.Z;
            var currentUpdateSeconds = gameTime.TotalGameTime.TotalSeconds;
            var secondsDiff = currentUpdateSeconds - _lastUpdateSeconds;
            var zPositionDiff = currentZPosition - _lastZPosition;

            if (zPositionDiff < -0.0001 && secondsDiff != 0)
            {
                var velocity = zPositionDiff / secondsDiff;
                _isPlayerGlidingOrFalling = velocity < -2.5;
            }
            else
            {
                _isPlayerGlidingOrFalling = false;
            }

            _lastZPosition = currentZPosition;
            _lastUpdateSeconds = currentUpdateSeconds;
        }

        public static async Task TriggerKeybind(SettingEntry<KeyBinding> keybindingSetting)
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


        private static VirtualKeyShort ToVirtualKey(Keys key)
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

        internal void StoreThingForLaterActivation(Thing mount, string characterName)
        {
            ThingOnHide = mount;
            CharacterNameOnHide = characterName;
        }

        internal bool IsCharacterTheSameAfterMapLoad(string characterName)
        {
            return CharacterNameOnHide == characterName;
        }

        internal async Task DoThingActionForLaterActivation()
        {
            await ThingOnHide?.DoAction();
            ThingOnHide = null;
            CharacterNameOnHide = null;            
        }
    }
}
