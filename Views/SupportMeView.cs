using Blish_HUD.Controls;
using Blish_HUD;
using Blish_HUD.Graphics.UI;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Manlaan.Mounts.Views
{
    class SupportMeView : View
    {
        private readonly TextureCache textureCache;

        public SupportMeView(TextureCache textureCache)
        {
            this.textureCache = textureCache;
        }

        protected override void Build(Container buildPanel) {

            Label l = new Label
            {
                Text = "I don't expect anything in return, but if you want you can:\n- send some gold/items ingame: Bennieboj.2607\n- donate via Ko-fi:",
                Location = new Point(300, 300),
                Width = 800,
                AutoSizeHeight = true,
                WrapText = true,
                Font = GameService.Content.DefaultFont18,
                HorizontalAlignment = HorizontalAlignment.Left,
                Parent = buildPanel
            };

            StandardButton kofiSupport = new StandardButton
            {
                Left = 370,
                Top = 400,
                Icon = textureCache.GetImgFile(TextureCache.KofiTextureName),
                Height = 60,
                Width = 130,
                Parent = buildPanel,
                Text = "Ko-fi"
        };
            kofiSupport.Click += delegate
            {
                Process.Start("https://ko-fi.com/bennieboj");
            };


            
        }
    }
}
