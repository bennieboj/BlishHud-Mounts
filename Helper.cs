using Blish_HUD;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Manlaan.Mounts.Things;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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
        public bool IsPlayerGlidingOrFalling = false;

        public Helper()
        {
            Module._debug.Add("IsPlayerUnderWater", () => $"{IsPlayerUnderWater()}");
            Module._debug.Add("IsPlayerOnWaterSurface", () => $"{IsPlayerOnWaterSurface()}");
            Module._debug.Add("IsPlayerGlidingOrFalling", () => $"{IsPlayerGlidingOrFalling}");
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

        public void UpdatePlayerGlidingOrFalling(GameTime gameTime)
        {
            var currentZPosition = GameService.Gw2Mumble.PlayerCharacter.Position.Z;
            var currentUpdateSeconds = gameTime.TotalGameTime.TotalSeconds;
            var secondsDiff = currentUpdateSeconds - _lastUpdateSeconds;
            var zPositionDiff = currentZPosition - _lastZPosition;

            if (zPositionDiff < -0.0001 && secondsDiff != 0)
            {
                var velocity = zPositionDiff / secondsDiff;
                IsPlayerGlidingOrFalling = velocity < -2.5;
            }
            else
            {
                IsPlayerGlidingOrFalling = false;
            }

            _lastZPosition = currentZPosition;
            _lastUpdateSeconds = currentUpdateSeconds;
        }

        internal Thing GetCenterThing()
        {
            if (Module._settingMountRadialCenterMountBehavior.Value == "Default")
                return GetDefaultThing();
            if (Module._settingMountRadialCenterMountBehavior.Value == "LastUsed")
                return GetLastUsedThing();
             return null;
        }

        internal Thing GetDefaultThing()
        {
            return Module._things.SingleOrDefault(m => m.Name == Module._settingDefaultMountChoice.Value);
        }
        internal Thing GetLastUsedThing()
        {
            return Module._things.Where(m => m.LastUsedTimestamp != null).OrderByDescending(m => m.LastUsedTimestamp).FirstOrDefault();
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

        internal void StoreThingForLaterUse(Thing mount, string characterName)
        {
            ThingOnHide = mount;
            CharacterNameOnHide = characterName;
        }

        internal bool IsCharacterTheSameAfterMapLoad(string characterName)
        {
            return CharacterNameOnHide == characterName;
        }

        internal Task DoThingActionForLaterUse()
        {
            return ThingOnHide?.DoAction();
        }

        internal void ClearThingForLaterUse()
        {
            ThingOnHide = null;
            CharacterNameOnHide = null;
        }
    }
}
