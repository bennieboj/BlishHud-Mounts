using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;

namespace Manlaan.Mounts.Controls
{
    public class DebugControl : Control
    {
        private ConcurrentDictionary<string, Func<string>> StringsToDisplay { get; set; }

        public DebugControl()
        {
            Visible = true;
            StringsToDisplay = new ConcurrentDictionary<string, Func<string>>();
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.None;
        }

        public void Add(string key, Func<string> value)
        {
            StringsToDisplay[key] = value;
        }

        public bool Remove(string key)
        {
            return StringsToDisplay.TryRemove(key, out _);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if(!DebugHelper.IsDebugEnabled()) return;

            int i = 0;
            foreach (var item in StringsToDisplay)
            {
                DrawDbg(spriteBatch, i, $"{item.Key}: {item.Value.Invoke()}");
                i += 30;
            }
        }

        private void DrawDbg(SpriteBatch spriteBatch, int position, string s)
        {
            spriteBatch.DrawStringOnCtrl(this, s, GameService.Content.DefaultFont32, new Rectangle(new Point(0, position), new Point(400, 400)), Color.Red);

        }
    }
}