using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Blish_HUD;
using System.Diagnostics;
using System.Linq;
using Blish_HUD.Graphics.UI;

namespace Manlaan.Mounts.Views
{
    class RadialThingSettingsView : View
    {
        private int labelWidth = 150;
        private int bindingWidth = 170;

        private Panel RadialSettingsListPanel;
        private Panel RadialSettingsDetailPanel;

        private RadialThingSettings currentRadialSettings;
        ThingSettingsView thingSettingsView;

        public RadialThingSettingsView()
        {
            
        }
        private Panel CreateDefaultPanel(Container buildPanel, Point location, int width = 420)
        {
            return new Panel {
                CanScroll = false,
                Parent = buildPanel,
                HeightSizingMode = SizingMode.AutoSize,
                Width = width,
                Location = location
            };
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

            var documentationButton = new StandardButton
            {
                Parent = buildPanel,
                Location = new Point(labelExplanation.Right, labelExplanation.Top),
                Text = Strings.Documentation_Button_Label
            };
            documentationButton.Click += (args, sender) => {
                Process.Start("https://github.com/manlaan/BlishHud-Mounts/#settings");
            };

            var panelPadding = 20;

            RadialSettingsListPanel = CreateDefaultPanel(buildPanel, new Point(panelPadding, labelExplanation.Bottom + panelPadding), 600);
            CreateRadialSettingsListPanel();

            currentRadialSettings = Module.OrderedRadialSettings().First();
            RadialSettingsDetailPanel = CreateDefaultPanel(buildPanel, new Point(10, 300));
            BuildRadialSettingsDetailPanel();
        }

        private void CreateRadialSettingsListPanel()
        {
            Label orderHeading_Label = new Label()
            {
                Location = new Point(0, 10),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsListPanel,
                Text = "Name",
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            Label nameHeader_label = new Label()
            {
                Location = new Point(orderHeading_Label.Right + 5, orderHeading_Label.Top),
                Width = bindingWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsListPanel,
                Text = "Evaluation Order",
                HorizontalAlignment = HorizontalAlignment.Center,
            };


            int curY = orderHeading_Label.Bottom + 6;

            foreach (var radialSettings in Module.OrderedRadialSettings())
            {
                Label name_Label = new Label()
                {
                    Location = new Point(0, curY + 6),
                    Width = labelWidth,
                    AutoSizeHeight = false,
                    WrapText = false,
                    Parent = RadialSettingsListPanel,
                    Text = $"{radialSettings.Name}: ",
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
                Label order_Label = new Label()
                {
                    Location = new Point(name_Label.Right + 5, name_Label.Top - 4),
                    Width = labelWidth,
                    AutoSizeHeight = false,
                    WrapText = false,
                    Parent = RadialSettingsListPanel,
                    Text = $"{radialSettings.Order}",
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                var editRadialSettingsButton = new StandardButton
                {
                    Parent = RadialSettingsListPanel,
                    Location = new Point(order_Label.Right, order_Label.Top),
                    Text = Strings.Edit
                };
                editRadialSettingsButton.Click += (args, sender) => {
                    currentRadialSettings = radialSettings;
                    BuildRadialSettingsDetailPanel();
                };

                curY = name_Label.Bottom;
            }            
        }

        private void BuildRadialSettingsDetailPanel()
        {
            RadialSettingsDetailPanel.ClearChildren();
           
            Label radialSettingstName_Label = new Label()
            {
                Location = new Point(0, 0),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsDetailPanel,
                Text = $"{currentRadialSettings.Name}"
            };

            Label radialSettingsIsEnabled_Label = new Label()
            {
                Location = new Point(0, radialSettingstName_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsDetailPanel,
                Text = "Enabled"
            };
            Checkbox radialSettingsIsEnabled_Checkbox = new Checkbox()
            {
                Size = new Point(20, 20),
                Parent = RadialSettingsDetailPanel,
                Checked = currentRadialSettings.IsEnabledSetting.Value,
                Location = new Point(radialSettingsIsEnabled_Label.Right + 5, radialSettingsIsEnabled_Label.Top - 1),
            };
            radialSettingsIsEnabled_Checkbox.CheckedChanged += delegate {
                currentRadialSettings.IsEnabledSetting.Value = radialSettingsIsEnabled_Checkbox.Checked;
            };

            Label radialSettingsApplyInstantlyIfSingle_Label = new Label()
            {
                Location = new Point(0, radialSettingsIsEnabled_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsDetailPanel,
                Text = "ApplyInstantlyIfSingle"
            };
            Checkbox radialSettingsApplyInstantlyIfSingle_Checkbox = new Checkbox()
            {
                Size = new Point(20, 20),
                Parent = RadialSettingsDetailPanel,
                Checked = currentRadialSettings.ApplyInstantlyIfSingleSetting.Value,
                Location = new Point(radialSettingsApplyInstantlyIfSingle_Label.Right + 5, radialSettingsApplyInstantlyIfSingle_Label.Top - 1),
            };
            radialSettingsApplyInstantlyIfSingle_Checkbox.CheckedChanged += delegate {
                currentRadialSettings.ApplyInstantlyIfSingleSetting.Value = radialSettingsApplyInstantlyIfSingle_Checkbox.Checked;
            };

            thingSettingsView = new ThingSettingsView(currentRadialSettings)
            {
                Location = new Point(0, radialSettingsApplyInstantlyIfSingle_Label.Bottom),
                Parent = RadialSettingsDetailPanel,
                Width = 500,
                Height = 500
            };
            thingSettingsView.OnThingsUpdated += OnThingsUpdated;
        }

        void OnThingsUpdated(object sender, ThingsUpdatedEventArgs e)
        {
            currentRadialSettings.ApplyInstantlyIfSingleSetting.Value = e.NewCount == 1;
            BuildRadialSettingsDetailPanel();
        }

}
}
