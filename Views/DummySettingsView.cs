using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework;

namespace Manlaan.Mounts.Views
{
    class DummySettingsView : View
    {
        private ContentsManager contentsManager;

        public DummySettingsView(ContentsManager contentsManager)
        {
            this.contentsManager = contentsManager;
        }

        protected override void Build(Container buildPanel) {
            Label text1_label = new Label()
            {
                Parent = buildPanel,
                Location = new Point(200, 140),
                AutoSizeWidth = true,
                StrokeText = true,
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Italic),
                Text = "Settings have moved to the ",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Image _btnTab = new Image
            {
                Parent = buildPanel,
                Size = new Point(32, 32),
                Location = new Point(text1_label.Right + 3, text1_label.Bottom - 32),
                Texture = contentsManager.GetTexture("514394-grey.png"),
            };
            Label text2_label = new Label()
            {
                Parent = buildPanel,
                Location = new Point(_btnTab.Right + 3, text1_label.Top),
                AutoSizeWidth = true,
                StrokeText = true,
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Italic),
                Text = "tab!"
            };
        }
    }
}
