using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Manlaan.Mounts.Controls
{
    internal class DebugControl : Control
    {
        public string[] Content { get; set; }

        public DebugControl()
        {
            Visible = true;
            Content = new string[] { };
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.None;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) 
        {
            int i = 0;
            foreach (var item in Content)
            {
                DrawDbg(spriteBatch, i, item);
                i += 30;
            }
        }

        private void DrawDbg(SpriteBatch spriteBatch, int position, string s)
        {
            spriteBatch.DrawStringOnCtrl(this, s, GameService.Content.DefaultFont32, new Rectangle(new Point(0, position), new Point(400, 400)), Color.Red);

        }
    }
}