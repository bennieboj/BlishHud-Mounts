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
using Mounts.Settings;
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
        private DateTime lastTimeJumped = DateTime.MinValue;        

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

        public bool IsPlayerInCombat()
        {
            return GameService.Gw2Mumble.PlayerCharacter.IsInCombat;
        }

        public bool IsPlayerMounted()
        {
            return GameService.Gw2Mumble.PlayerCharacter.CurrentMount != Gw2Sharp.Models.MountType.None;
        }

        public bool IsPlayerGlidingOrFalling()
        {
            return _isPlayerGlidingOrFalling;
        }

        public void UpdateLastJumped()
        {
            if (!IsPlayerMounted())
            {
                lastTimeJumped = DateTime.UtcNow;
            }
        }
        private bool DidPlayerJumpRecently()
        {
            return DateTime.UtcNow.Subtract(lastTimeJumped).TotalMilliseconds < 5000;
        }

        public void UpdatePlayerGlidingOrFalling(GameTime gameTime)
        {
            var currentZPosition = GameService.Gw2Mumble.PlayerCharacter.Position.Z;
            var currentUpdateSeconds = gameTime.TotalGameTime.TotalSeconds;
            var secondsDiff = currentUpdateSeconds - _lastUpdateSeconds;
            var zPositionDiff = currentZPosition - _lastZPosition;

            bool shouldUpdate = false;
            if (false)
            {
                shouldUpdate = OldStuff(zPositionDiff, secondsDiff);
            }
            else
            {
                shouldUpdate = NewStuff(zPositionDiff, secondsDiff);
            }

            if (shouldUpdate)
            {
                _lastZPosition = currentZPosition;
                _lastUpdateSeconds = currentUpdateSeconds;
            }
        }

        private bool NewStuff(float zPositionDiff, double secondsDiff)
        {
            var velocity = zPositionDiff / secondsDiff;

            if (secondsDiff < 0.1f)
            {
                return false;
            }

            //Module._debug.Add("velocity", () => $"{velocity.ToString("N2")}");
            Module._debug.Add("DidPlayerJumpRecently", () => $"{DidPlayerJumpRecently()}.");

            if (velocity > 10 || velocity < -10)
                _isPlayerGlidingOrFalling = true;
            else if (DidPlayerJumpRecently() && velocity < -2)
                _isPlayerGlidingOrFalling = true;
            else
                _isPlayerGlidingOrFalling = false;

            return true;
        }

        private bool OldStuff(float zPositionDiff, double secondsDiff)
        {
            if (zPositionDiff < -0.0001 && secondsDiff != 0)
            {
                var velocity = zPositionDiff / secondsDiff;
                _isPlayerGlidingOrFalling = velocity < -2.5;
            }
            else
            {
                _isPlayerGlidingOrFalling = false;
            }
            return true;
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

        internal Thing GetQueuedThing()
        {
            return Module._things.Where(m => m.QueuedTimestamp != null).OrderByDescending(m => m.QueuedTimestamp).FirstOrDefault();
        }

        internal void StoreThingForLaterActivation(Thing thing, string characterName, string reason)
        {
            Logger.Debug($"{nameof(StoreThingForLaterActivation)}: {thing.Name} for character: {characterName} with reason: {reason}");
            StoredThingForLater[characterName] = thing;
        }

        internal bool IsSomethingStoredForLaterActivation(string characterName)
        {
            var result = StoredThingForLater.ContainsKey(characterName);
            Logger.Debug($"{nameof(IsSomethingStoredForLaterActivation)} for character {characterName} : {result}");
            return result;
        }

        internal void ClearSomethingStoredForLaterActivation(string characterName)
        {
            Logger.Debug($"{nameof(ClearSomethingStoredForLaterActivation)} for character: {characterName}");
            StoredThingForLater.Remove(characterName);
        }

        internal async Task DoThingActionForLaterActivation(string characterName)
        {
            var thing = StoredThingForLater[characterName];
            Logger.Debug($"{nameof(ClearSomethingStoredForLaterActivation)} {thing?.Name} for character: {characterName}");
            await thing?.DoAction(false);
            ClearSomethingStoredForLaterActivation(characterName);
        }

        internal ContextualRadialThingSettings GetApplicableContextualRadialThingSettings() => Module.ContextualRadialSettings.OrderBy(c => c.Order).FirstOrDefault(c => c.IsEnabled.Value && c.IsApplicable());

        internal RadialThingSettings GetTriggeredRadialSettings()
        {
            if (!Module._settingDefaultMountBinding.IsNull && Module._settingDefaultMountBinding.Value.IsTriggering)
            {
                return GetApplicableContextualRadialThingSettings();
            }

            var userdefinedList = Module.UserDefinedRadialSettings.Where(s => !s.Keybind.IsNull && s.Keybind.Value.IsTriggering);
            if (userdefinedList.Count() == 1) {
                return userdefinedList.Single();
            }

            return null;
        }
    }
}
