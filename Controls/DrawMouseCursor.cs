using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Manlaan.Mounts.Controls
{
    internal class DrawMouseCursor : Control
    {
        private Texture2D _mouseTexture;

        public DrawMouseCursor(TextureCache textureCache)
        {
            this.Visible = false;
            this.Padding = Thickness.Zero;
            _mouseTexture = textureCache.GetImgFile(TextureCache.MouseTextureName);
            Size = new Point(_mouseTexture.Width, _mouseTexture.Height);
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Filter;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            ZIndex = int.MaxValue;
            var rect = new Rectangle(0, 0, Size.X, Size.Y);
            spriteBatch.DrawOnCtrl(this, _mouseTexture, rect);
        }
    }
}
