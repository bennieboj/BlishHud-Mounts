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
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;

namespace Manlaan.Mounts
{
    public class RangedThingUpdatedEvent : EventArgs
    {
        public Thing NewThing { get; set; }
        public RangedThingUpdatedEvent(Thing newThing)
        {
            NewThing = newThing;
        }
    }

    public class Helper
    {
        private static readonly Logger Logger = Logger.GetLogger<Helper>();

        private Dictionary<string, Thing> StoredThingForLater = new Dictionary<string, Thing>();

        private Thing StoredRangedThing = null;

        private readonly Dictionary<(Keys, ModifierKeys), SemaphoreSlim> _semaphores;

        private float _lastZPosition = 0.0f;
        private double _lastUpdateSeconds = 0.0f;
        private bool _isPlayerGlidingOrFalling = false;
        private Gw2ApiManager Gw2ApiManager;
        private bool _isCombatLaunchUnlocked;
        private DateTime lastTimeJumped = DateTime.MinValue;

        public event EventHandler<RangedThingUpdatedEvent> RangedThingUpdated;

        public Helper(Gw2ApiManager gw2ApiManager)
        {
            _semaphores = new Dictionary<(Keys, ModifierKeys), SemaphoreSlim>();
            Gw2ApiManager = gw2ApiManager;

            Module._debug.Add("StoreThingForLaterActivation", () => $"{string.Join(", ", StoredThingForLater.Select(x => x.Key + "=" + x.Value.Name).ToArray())}");
        }

        public bool IsCombatLaunchUnlocked()
        {
            return _isCombatLaunchUnlocked && Module._settingCombatLaunchMasteryUnlocked.Value;
        }

        public async Task IsCombatLaunchUnlockedAsync()
        {
            if (!Gw2ApiManager.HasPermissions(new List<TokenPermission> { TokenPermission.Progression }))
            {
                _isCombatLaunchUnlocked = false;
            }

            var masteries = await Gw2ApiManager.Gw2ApiClient.V2.Masteries.AllAsync();
            _isCombatLaunchUnlocked = masteries.Any(m => m.Levels.Any(ml => ml.Name == "Combat Launch"));
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

            if (NewStuff(zPositionDiff, secondsDiff))
            {
                _lastZPosition = currentZPosition;
                _lastUpdateSeconds = currentUpdateSeconds;
            }
        }

        private bool NewStuff(float zPositionDiff, double secondsDiff)
        {
            var velocity = zPositionDiff / secondsDiff;

            if (secondsDiff < Module._settingFallingOrGlidingUpdateFrequency.Value)
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

        private SemaphoreSlim GetOrCreateSemaphore(KeyBinding keyBinding)
        {
            lock (_semaphores)
            {
                if (!_semaphores.TryGetValue((keyBinding.PrimaryKey, keyBinding.ModifierKeys), out var semaphore))
                {
                    semaphore = new SemaphoreSlim(1, 1);
                    _semaphores[(keyBinding.PrimaryKey, keyBinding.ModifierKeys)] = semaphore;
                }

                return semaphore;
            }
        }

        public async Task TriggerKeybind(SettingEntry<KeyBinding> keybindingSetting)
        {
            var semaphore = GetOrCreateSemaphore(keybindingSetting.Value);

            await semaphore.WaitAsync();

            try
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
            finally
            {
                semaphore.Release();
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

        internal void StoreThingForLaterActivation(Thing thing, string reason)
        {
            var characterName = GameService.Gw2Mumble.PlayerCharacter.Name;
            Logger.Debug($"{nameof(StoreThingForLaterActivation)}: {thing.Name} for character: {characterName} with reason: {reason}");
            StoredThingForLater[characterName] = thing;
        }

        internal void StoreRangedThing(Thing thing)
        {
            Logger.Debug($"{nameof(StoreRangedThing)}: {thing?.Name}");
            StoredRangedThing = thing;
            if (RangedThingUpdated != null)
            {
                RangedThingUpdated(this, new RangedThingUpdatedEvent(StoredRangedThing));
            }
        }

        internal async Task DoRangedThing()
        {
            if(StoredRangedThing != null)
            {
                var thing = StoredRangedThing;
                Logger.Debug($"{nameof(DoRangedThing)} {thing?.Name}");
                await thing?.DoAction(false, false);
                StoredRangedThing = null;
                if (RangedThingUpdated != null)
                {
                    RangedThingUpdated(this, new RangedThingUpdatedEvent(StoredRangedThing));
                }
            }
        }

        internal Thing IsSomethingStoredForLaterActivation()
        {
            var characterName = GameService.Gw2Mumble.PlayerCharacter.Name;
            StoredThingForLater.TryGetValue(characterName, out Thing result);
            Logger.Debug($"{nameof(IsSomethingStoredForLaterActivation)} for character {characterName} : {result?.Name}");
            return result;
        }

        internal void ClearSomethingStoredForLaterActivation()
        {
            var characterName = GameService.Gw2Mumble.PlayerCharacter.Name;
            Logger.Debug($"{nameof(ClearSomethingStoredForLaterActivation)} for character: {characterName}");
            StoredThingForLater.Remove(characterName);
        }

        internal async Task DoThingActionForLaterActivation()
        {
            var characterName = GameService.Gw2Mumble.PlayerCharacter.Name;
            var thing = StoredThingForLater[characterName];
            Logger.Debug($"{nameof(DoThingActionForLaterActivation)} {thing?.Name} for character: {characterName}");
            await thing?.DoAction(false, false);
            ClearSomethingStoredForLaterActivation();
        }

        internal ContextualRadialThingSettings GetApplicableContextualRadialThingSettings() => Module.ContextualRadialSettings.OrderBy(c => c.Order).FirstOrDefault(c => c.IsEnabled.Value && c.IsApplicable());
        internal ContextualRadialThingSettings GetApplicableTriggeringContextualRadialThingSettings() => Module.ContextualRadialSettings.OrderBy(c => c.Order).FirstOrDefault(c => c.IsEnabled.Value && c.IsApplicable() && c.GetKeybind().Value.IsTriggering);

        internal IEnumerable<RadialThingSettings> GetAllGenericRadialThingSettings()
        {
            var contextualRadialSettingsCasted = Module.ContextualRadialSettings.ConvertAll(x => (RadialThingSettings)x);
            var userDefinedRadialSettingsCasted = Module.UserDefinedRadialSettings.ConvertAll(x => (RadialThingSettings)x);
            return contextualRadialSettingsCasted.Concat(userDefinedRadialSettingsCasted);
        }

        internal RadialThingSettings GetTriggeredRadialSettings()
        {
            var contextual = GetApplicableTriggeringContextualRadialThingSettings();
            if(contextual != null)
            {
                return contextual;
            }

            var userdefinedList = Module.UserDefinedRadialSettings.Where(s => s.GetKeybind().Value.IsTriggering);
            if (userdefinedList.Count() == 1)
            {
                return userdefinedList.Single();
            }

            return null;
        }
    }
}
