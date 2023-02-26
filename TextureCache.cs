using System.Collections.Generic;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework.Graphics;

namespace Manlaan.Mounts
{
    public class TextureCache
    {
        private readonly ContentsManager contentsManager;
        private readonly Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();
        public readonly string MouseTexture = "255329.png";

        public TextureCache(ContentsManager contentsManager)
        {
            this.contentsManager = contentsManager;
            PreCacheTextures();
        }

        private void PreCacheTextures()
        {
            foreach (var mount in Module._mounts)
            {
                foreach (var mountDisplay in Module._mountDisplay)
                {
                    var textureName = GetTextureName(mount.ImageFileName, mountDisplay);
                    if (!_textureCache.ContainsKey(textureName))
                    {
                        PreCacheTexture(textureName);
                    }
                }
            }
            PreCacheTexture(MouseTexture);
        }

        private void PreCacheTexture(string textureName)
        {
            _textureCache[textureName] = contentsManager.GetTexture(textureName);
        }

        public Texture2D GetImgFile(string filename)
        {
            return _textureCache[filename];
        }

        public Texture2D GetMountImgFile(string filename)
        {
            string textureName = GetTextureName(filename, Module._settingDisplay.Value);

            return _textureCache[textureName];
        }

        private static string GetTextureName(string filename, string displaySetting)
        {
            string textureName = filename;

            switch (displaySetting)
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

            return textureName;
        }
    }
}
