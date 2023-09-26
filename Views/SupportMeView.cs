using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Blish_HUD;
using Blish_HUD.Graphics.UI;

namespace Manlaan.Mounts.Views
{
    class SupportMeView : View
    {       
        public SupportMeView()
        {
            
        }

        protected override void Build(Container buildPanel) {

            Label labelExplanation = new Label()
            {
                Location = new Point(10, 10),
                Width = 800,
                AutoSizeHeight = true,
                WrapText = true,
                Parent = buildPanel,
                TextColor = Color.Red,
                Font = GameService.Content.DefaultFont18,
                Text = "When enabled, a context defines which actions are taken into account. Contexts are optional.\nTODOOOOOOO".Replace(" ", "  "),
                HorizontalAlignment = HorizontalAlignment.Left
            };            
        }
    }
}
