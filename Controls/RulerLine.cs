using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mounts.Controls
{
    internal class RulerLine : Control
    {

        public RulerLine()
        {
            BackgroundColor = Color.White * 0.8f;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.Draw(
                ContentService.Textures.Pixel,
                bounds,
                this.BackgroundColor
            );
        }
    }
}
