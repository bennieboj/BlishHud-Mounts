using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Manlaan.Mounts
{
    internal class Helper
    {
        private readonly ContentsManager contentsManager;

        private readonly Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();

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

        internal Mount GetDefaultMount()
        {
            if (IsPlayerInWvWMap())
            {
                return Module._mounts.Single(m => m.IsWvWMount);
            }

            if (IsPlayerUnderOrCloseToWater())
            {
                return GetWaterMount();
            }

            return Module._mounts.SingleOrDefault(m => m.Name == Module._settingDefaultMountChoice.Value);
        }

        internal Mount GetLastUsedMount()
        {
            if (IsPlayerUnderOrCloseToWater())
            {
                return GetWaterMount();
            }

            return Module._mounts.Where(m => m.LastUsedTimestamp != null).OrderByDescending(m => m.LastUsedTimestamp).FirstOrDefault();
        }
    }
}
