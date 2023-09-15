using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System;

namespace Manlaan.Mounts.Views
{
    class DummySettingsView : View
    {
        public EventHandler OnSettingsButtonClicked { get; internal set; }

        protected override void Build(Container buildPanel)
        {
            var _settingsButton = new StandardButton
            {
                Parent = buildPanel,
                Location = new Point(100, 100),
                Text = Strings.Settings_Button_Label
            };

            _settingsButton.Click += (args, sender) => {
                OnSettingsButtonClicked(args, sender);
            };
        }
    }
}
