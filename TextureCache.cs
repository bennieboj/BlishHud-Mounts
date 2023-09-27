using System;
using System.Collections.Generic;
using System.IO;
using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Manlaan.Mounts.Things;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Manlaan.Mounts
{
    public class TextureCache
    {
        private readonly ContentsManager contentsManager;
        private readonly Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();


        public static readonly string MouseTextureName = "255329.png";
        public static readonly string MountLogoTextureName = "514394-grey-plus-plus100.png";
        public static readonly string TabBackgroundTextureName = "156006-big.png";
        public static readonly string SettingsTextureName = "155052.png";
        public static readonly string RadialSettingsTextureName = "1130623-32.png";
        public static readonly string IconSettingsTextureName = "2208345.png";
        public static readonly string SupportMeTabTextureName = "156127-32-grey.png";
        public static readonly string KofiTextureName = "kofi-small.png";
        public static readonly string AnetIconTextureName = "1441452.png";

        public TextureCache(ContentsManager contentsManager)
        {
            this.contentsManager = contentsManager;
            PreCacheTextures();
        }

        private void PreCacheTextures()
        {
            Func<string, Texture2D> getTextureFromRef = (textureName) => contentsManager.GetTexture(textureName);

            foreach (var mountImageFile in Module._thingImageFiles)
            {
                PreCacheTexture(mountImageFile.Name, PremultiplyTexture);
            }
            PreCacheTexture(MouseTextureName, getTextureFromRef);
            PreCacheTexture(MountLogoTextureName, getTextureFromRef);
            PreCacheTexture(TabBackgroundTextureName, getTextureFromRef);
            PreCacheTexture(SettingsTextureName, getTextureFromRef);
            PreCacheTexture(RadialSettingsTextureName, getTextureFromRef);
            PreCacheTexture(IconSettingsTextureName, getTextureFromRef);
            PreCacheTexture(SupportMeTabTextureName, getTextureFromRef);
            PreCacheTexture(KofiTextureName, getTextureFromRef);
            PreCacheTexture(AnetIconTextureName, getTextureFromRef);
        }

        private Texture2D PremultiplyTexture(string textureName)
        {
            Texture2D texture;

            try
            {
                var filePath = Path.Combine(Module.mountsDirectory, textureName);
                using (FileStream titleStream = File.OpenRead(filePath))
                using (var gdc = GameService.Graphics.LendGraphicsDeviceContext())
                {
                    texture = Texture2D.FromStream(gdc.GraphicsDevice, titleStream);
                    titleStream.Close();

                    Color[] buffer = new Color[texture.Width * texture.Height];
                    texture.GetData(buffer);
                    for (int i = 0; i < buffer.Length; i++)
                        buffer[i] = Color.FromNonPremultiplied(buffer[i].R, buffer[i].G, buffer[i].B, buffer[i].A);
                    texture.SetData(buffer);
                }
            }
            catch
            {
                texture = ContentService.Textures.Error;
            }
            return texture;
        }

        private void PreCacheTexture(string textureName, Func<string, Texture2D> getTextureAction)
        {
            if (!_textureCache.ContainsKey(textureName))
            { 
                _textureCache[textureName] = getTextureAction(textureName);
            }
        }

        public Texture2D GetImgFile(string filename)
        {
            return GetTexture(filename);
        }

        public Texture2D GetMountImgFile(Thing mount)
        {
            return GetTexture(mount.ImageFileNameSetting.Value);
        }

        private Texture2D GetTexture (string filename)
        {
            if (_textureCache.ContainsKey(filename))
            {
                return _textureCache[filename];
            }
            return ContentService.Textures.Error;
        }
    }
}
