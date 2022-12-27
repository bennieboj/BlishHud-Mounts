using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Manlaan.Mounts.Controls
{
    internal class DebugControl : Control
    {
        public IEnumerable<string> StringsToDisplay { get; set; }

        public DebugControl()
        {
            Visible = true;
            StringsToDisplay = new List<string>();
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.None;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) 
        {
            int i = 0;
            foreach (var item in StringsToDisplay)
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