using Blish_HUD;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Input;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Manlaan.Mounts
{
    public class Helper
    {
        private readonly ContentsManager contentsManager;

        private static SemaphoreSlim keybindSemaphore = new SemaphoreSlim(1, 1);

        private readonly Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();
        private static readonly Logger Logger = Logger.GetLogger<Helper>();

        public Helper(ContentsManager contentsManager)
        {
            this.contentsManager = contentsManager;
        }

        Gw2Sharp.Models.MapType[] warclawOnlyMaps = {
                Gw2Sharp.Models.MapType.RedBorderlands,
                Gw2Sharp.Models.MapType.RedHome,
                Gw2Sharp.Models.MapType.BlueBorderlands,
                Gw2Sharp.Models.MapType.BlueHome,
                Gw2Sharp.Models.MapType.GreenBorderlands,
                Gw2Sharp.Models.MapType.GreenHome,
                Gw2Sharp.Models.MapType.EternalBattlegrounds,
                Gw2Sharp.Models.MapType.Center,
            };

        public Texture2D GetImgFile(string filename)
        {
            string textureName = filename;

            switch (Module._settingDisplay.Value)
            {
                default:
                case "Solid":
                    textureName += ".png";
                    break;
                case "Transparent":
                    textureName += "-trans.png";
                    break;
                case "SolidText":
                    textureName += "-text.png";
                    break;
            }

            if (!_textureCache.ContainsKey(textureName)) {
                _textureCache[textureName] = contentsManager.GetTexture(textureName);
            }

            return _textureCache[textureName];
        }

        private bool IsPlayerInWvWMap()
        {
            return Array.Exists(warclawOnlyMaps, mapType => mapType == GameService.Gw2Mumble.CurrentMap.Type);
        }

        private bool IsPlayerUnderOrCloseToWater()
        {
            return GameService.Gw2Mumble.PlayerCharacter.Position.Z <= 0;
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

        public bool IsKeybindBeingTriggered() {
            return keybindSemaphore.CurrentCount != 1;
        }

        public async Task TriggerKeybind(SettingEntry<KeyBinding> keybindingSetting)
        {
            try
            {
                await keybindSemaphore.WaitAsync();
                Logger.Debug("TriggerKeybind entered");
                if (keybindingSetting.Value.ModifierKeys != ModifierKeys.None)
                {
                    Logger.Debug($"TriggerKeybind press modifiers {keybindingSetting.Value.ModifierKeys}");
                    if (keybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Alt))
                        Blish_HUD.Controls.Intern.Keyboard.Press(VirtualKeyShort.MENU, true);
                    if (keybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Ctrl))
                        Blish_HUD.Controls.Intern.Keyboard.Press(VirtualKeyShort.CONTROL, true);
                    if (keybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Shift))
                        Blish_HUD.Controls.Intern.Keyboard.Press(VirtualKeyShort.SHIFT, true);
                }
                Logger.Debug($"TriggerKeybind press PrimaryKey {keybindingSetting.Value.PrimaryKey}");
                Blish_HUD.Controls.Intern.Keyboard.Press(ToVirtualKey(keybindingSetting.Value.PrimaryKey), true);
                await Task.Delay(100);
                Logger.Debug($"TriggerKeybind release PrimaryKey {keybindingSetting.Value.PrimaryKey}");
                Blish_HUD.Controls.Intern.Keyboard.Release(ToVirtualKey(keybindingSetting.Value.PrimaryKey), true);
                if (keybindingSetting.Value.ModifierKeys != ModifierKeys.None)
                {
                    Logger.Debug($"TriggerKeybind release modifiers {keybindingSetting.Value.ModifierKeys}");
                    if (keybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Shift))
                        Blish_HUD.Controls.Intern.Keyboard.Release(VirtualKeyShort.SHIFT, true);
                    if (keybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Ctrl))
                        Blish_HUD.Controls.Intern.Keyboard.Release(VirtualKeyShort.CONTROL, true);
                    if (keybindingSetting.Value.ModifierKeys.HasFlag(ModifierKeys.Alt))
                        Blish_HUD.Controls.Intern.Keyboard.Release(VirtualKeyShort.MENU, true);
                }
            }
            finally
            {
                keybindSemaphore.Release();
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
