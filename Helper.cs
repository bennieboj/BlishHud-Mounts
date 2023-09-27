using Blish_HUD;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Input;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
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

        private Dictionary<string, Thing> StoredThingForLater = new Dictionary<string, Thing>();

        private float _lastZPosition = 0.0f;
        private double _lastUpdateSeconds = 0.0f;
        private bool _isPlayerGlidingOrFalling = false;
        private Gw2ApiManager Gw2ApiManager;
        private bool _isCombatLaunchUnlocked;

        public Helper(Gw2ApiManager gw2ApiManager)
        {
            Gw2ApiManager = gw2ApiManager;

            Module._debug.Add("StoreThingForLaterActivation", () => $"{string.Join(", ", StoredThingForLater.Select(x => x.Key + "=" + x.Value.Name).ToArray())}");
        }

        public bool IsCombatLaunchUnlocked()
        {
            return _isCombatLaunchUnlocked || Module._settingCombatLaunchMasteryUnlocked.Value;
        }

        public async Task IsCombatLaunchUnlockedAsync()
        {
            if (!Gw2ApiManager.HasPermissions(new List<TokenPermission> { TokenPermission.Progression }))
            {
                _isCombatLaunchUnlocked = false;
            }

            var masteries = await Gw2ApiManager.Gw2ApiClient.V2.Masteries.AllAsync();
            _isCombatLaunchUnlocked = masteries.Any(m => m.Name == "Combat Launch");
        }

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
            return GameService.Gw2Mumble.PlayerCharacter.CurrentMount != Gw2Sharp.Models.MountType.None;
        }

        public void UpdatePlayerGlidingOrFalling(GameTime gameTime)
        {
            var currentZPosition = GameService.Gw2Mumble.PlayerCharacter.Position.Z;
            var currentUpdateSeconds = gameTime.TotalGameTime.TotalSeconds;
            var secondsDiff = currentUpdateSeconds - _lastUpdateSeconds;
            var zPositionDiff = currentZPosition - _lastZPosition;
            var velocity = zPositionDiff / secondsDiff;

            if (secondsDiff < 0.015)
            {
                return;
            }

            //Module._debug.Add("velocity", () => $"{velocity}");

            switch (velocity)
            {
                case double v1 when v1 > 5:
                case double v2 when v2 < -4:
                    _isPlayerGlidingOrFalling = true;
                    break;
                case double v3 when v3 >= 0 && v3 < 1:
                _isPlayerGlidingOrFalling = false;
                    break;
            };

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
            StoredThingForLater[characterName] = mount;
        }

        internal bool IsSomethingStored(string characterName)
        {
            return StoredThingForLater.ContainsKey(characterName);
        }

        internal void ClearSomethingStored(string characterName)
        {
            StoredThingForLater.Remove(characterName);
        }

        internal async Task DoThingActionForLaterActivation(string characterName)
        {
            await StoredThingForLater[characterName]?.DoAction();
            ClearSomethingStored(characterName);
        }

    }
}
