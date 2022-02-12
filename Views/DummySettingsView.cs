using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace Manlaan.Mounts.Views
{
    class DummySettingsView : View
    {
        protected override void Build(Container buildPanel) {
            Label settingOrderLeft_Label = new Label()
            {
                Size = buildPanel.ContentRegion.Size,
                StrokeText = true,
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Italic),
                Parent = buildPanel,
                Text = "Settings have moved to the mounts tab!",
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }
    }
}
