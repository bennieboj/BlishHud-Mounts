using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Blish_HUD;
using System.Diagnostics;
using System.Linq;
using Blish_HUD.Graphics.UI;
using Mounts;
using System;

namespace Manlaan.Mounts.Views
{
    class RadialThingSettingsView : View
    {
        private int labelWidth = 170;
        private int bindingWidth = 170;

        private Panel RadialSettingsListPanel;
        private Panel RadialSettingsDetailPanel;

        private RadialThingSettings currentRadialSettings;
        
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
                Text = "When enabled, these radial settings dictate which actions are taken into account in which conditions.\nFor more info, see the documentation.".Replace(" ", "  "),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var documentationButton = new StandardButton
            {
                Parent = buildPanel,
                Location = new Point(labelExplanation.Right, labelExplanation.Top),
                Text = Strings.Documentation_Button_Label
            };
            documentationButton.Click += (args, sender) => {
                Process.Start("https://github.com/bennieboj/BlishHud-Mounts/#settings");
            };

            var panelPadding = 20;

            RadialSettingsListPanel = CreateDefaultPanel(buildPanel, new Point(panelPadding, labelExplanation.Bottom + panelPadding), 600);
            BuildRadialSettingsListPanel();

            currentRadialSettings = Module.OrderedRadialSettings().First();
            RadialSettingsDetailPanel = CreateDefaultPanel(buildPanel, new Point(10, 300));
            BuildRadialSettingsDetailPanel();
        }

        private void BuildRadialSettingsListPanel()
        {
            Label nameHeader_Label = new Label()
            {
                Location = new Point(0, 10),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsListPanel,
                Text = "Name",
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            Label orderHeader_label = new Label()
            {
                Location = new Point(nameHeader_Label.Right + 5, nameHeader_Label.Top),
                Width = bindingWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsListPanel,
                Text = "Evaluation Order",
                HorizontalAlignment = HorizontalAlignment.Center,
            };


            int curY = nameHeader_Label.Bottom + 6;

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
                Checked = currentRadialSettings.IsEnabled.Value,
                Location = new Point(radialSettingsIsEnabled_Label.Right + 5, radialSettingsIsEnabled_Label.Top - 1),
            };
            radialSettingsIsEnabled_Checkbox.CheckedChanged += delegate {
                currentRadialSettings.IsEnabled.Value = radialSettingsIsEnabled_Checkbox.Checked;
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
                Checked = currentRadialSettings.ApplyInstantlyIfSingle.Value,
                Location = new Point(radialSettingsApplyInstantlyIfSingle_Label.Right + 5, radialSettingsApplyInstantlyIfSingle_Label.Top - 1),
            };
            radialSettingsApplyInstantlyIfSingle_Checkbox.CheckedChanged += delegate {
                currentRadialSettings.ApplyInstantlyIfSingle.Value = radialSettingsApplyInstantlyIfSingle_Checkbox.Checked;
            };
            currentRadialSettings.ApplyInstantlyIfSingle.SettingChanged += delegate
            {
                BuildRadialSettingsDetailPanel();
            };

            Label settingDefaultThing_Label = new Label()
            {
                Location = new Point(0, radialSettingsApplyInstantlyIfSingle_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsDetailPanel,
                Text = "Default: ",
            };
            Dropdown settingDefaultThing_Select = new Dropdown()
            {
                Location = new Point(settingDefaultThing_Label.Right + 5, settingDefaultThing_Label.Top - 4),
                Width = labelWidth,
                Parent = RadialSettingsDetailPanel,
            };
            settingDefaultThing_Select.Items.Add("Disabled");
            var thingNames = currentRadialSettings.Things.Select(m => m.Name);
            foreach (string i in thingNames)
            {
                settingDefaultThing_Select.Items.Add(i.ToString());
            }
            settingDefaultThing_Select.SelectedItem = thingNames.Any(m => m == currentRadialSettings.DefaultThingChoice.Value) ? currentRadialSettings.DefaultThingChoice.Value : "Disabled";
            settingDefaultThing_Select.ValueChanged += delegate {
                currentRadialSettings.DefaultThingChoice.Value = settingDefaultThing_Select.SelectedItem;
            };


            Label settingRadialCenterMountBehavior_Label = new Label()
            {
                Location = new Point(0, settingDefaultThing_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsDetailPanel,
                Text = "Center: ",
            };
            Dropdown settingRadialCenterMountBehavior_Select = new Dropdown()
            {
                Location = new Point(settingRadialCenterMountBehavior_Label.Right + 5, settingRadialCenterMountBehavior_Label.Top - 4),
                Width = labelWidth,
                Parent = RadialSettingsDetailPanel,
            };
            foreach (CenterBehavior i in Enum.GetValues(typeof(CenterBehavior)))
            {
                settingRadialCenterMountBehavior_Select.Items.Add(i.ToString());
            }
            settingRadialCenterMountBehavior_Select.SelectedItem = currentRadialSettings.CenterThingBehavior.Value.ToString();
            settingRadialCenterMountBehavior_Select.ValueChanged += delegate {
                currentRadialSettings.CenterThingBehavior.Value = (CenterBehavior) Enum.Parse(typeof(CenterBehavior), settingRadialCenterMountBehavior_Select.SelectedItem);
            };
            Label settingRadialRemoveCenterMount_Label = new Label()
            {
                Location = new Point(0, settingRadialCenterMountBehavior_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsDetailPanel,
                Text = "Remove center from radial: ",
            };
            Checkbox settingRadialRemoveCenterMount_Checkbox = new Checkbox()
            {
                Size = new Point(labelWidth, 20),
                Parent = RadialSettingsDetailPanel,
                Checked = currentRadialSettings.RemoveCenterMount.Value,
                Location = new Point(settingRadialRemoveCenterMount_Label.Right + 5, settingRadialRemoveCenterMount_Label.Top - 1),
            };
            settingRadialRemoveCenterMount_Checkbox.CheckedChanged += delegate {
                currentRadialSettings.RemoveCenterMount.Value = settingRadialRemoveCenterMount_Checkbox.Checked;
            };

            ThingSettingsView thingSettingsView = new ThingSettingsView(currentRadialSettings)
            {
                Location = new Point(0, settingRadialRemoveCenterMount_Label.Bottom),
                Parent = RadialSettingsDetailPanel,
                Width = 500,
                Height = 500
            };
        }
    }
}
