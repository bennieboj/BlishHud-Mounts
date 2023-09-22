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
                Text = Strings.Add
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
            currentRadialSettings.ApplyInstantlyIfSingle.PropertyChanged += delegate
            {
                BuildRadialSettingsDetailPanel();
            };

            Label settingDefaultMount_Label = new Label()
            {
                Location = new Point(0, radialSettingsApplyInstantlyIfSingle_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsDetailPanel,
                Text = "Default mount: ",
            };
            Dropdown settingDefaultMount_Select = new Dropdown()
            {
                Location = new Point(settingDefaultMount_Label.Right + 5, settingDefaultMount_Label.Top - 4),
                Width = labelWidth,
                Parent = RadialSettingsDetailPanel,
            };
            settingDefaultMount_Select.Items.Add("Disabled");
            var mountNames = currentRadialSettings.Things.Select(m => m.Name);
            foreach (string i in mountNames)
            {
                settingDefaultMount_Select.Items.Add(i.ToString());
            }
            settingDefaultMount_Select.SelectedItem = mountNames.Any(m => m == currentRadialSettings.DefaultThingChoice.Value) ? currentRadialSettings.DefaultThingChoice.Value : "Disabled";
            settingDefaultMount_Select.ValueChanged += delegate {
                currentRadialSettings.DefaultThingChoice.Value = settingDefaultMount_Select.SelectedItem;
            };


            Label settingMountRadialCenterMountBehavior_Label = new Label()
            {
                Location = new Point(0, settingDefaultMount_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsDetailPanel,
                Text = "Center mount: ",
            };
            Dropdown settingMountRadialCenterMountBehavior_Select = new Dropdown()
            {
                Location = new Point(settingMountRadialCenterMountBehavior_Label.Right + 5, settingMountRadialCenterMountBehavior_Label.Top - 4),
                Width = labelWidth,
                Parent = RadialSettingsDetailPanel,
            };
            foreach (string i in RadialThingSettings._mountRadialCenterMountBehavior)
            {
                settingMountRadialCenterMountBehavior_Select.Items.Add(i.ToString());
            }
            settingMountRadialCenterMountBehavior_Select.SelectedItem = currentRadialSettings.CenterThingBehavior.Value.ToString();
            settingMountRadialCenterMountBehavior_Select.ValueChanged += delegate {
                currentRadialSettings.CenterThingBehavior.Value = (CenterBehavior) Enum.Parse(typeof(CenterBehavior), settingMountRadialCenterMountBehavior_Select.SelectedItem);
            };
            Label settingMountRadialRemoveCenterMount_Label = new Label()
            {
                Location = new Point(0, settingMountRadialCenterMountBehavior_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsDetailPanel,
                Text = "Remove center mount from radial: ",
            };
            Checkbox settingMountRadialRemoveCenterMount_Checkbox = new Checkbox()
            {
                Size = new Point(labelWidth, 20),
                Parent = RadialSettingsDetailPanel,
                Checked = currentRadialSettings.RemoveCenterMount.Value,
                Location = new Point(settingMountRadialRemoveCenterMount_Label.Right + 5, settingMountRadialRemoveCenterMount_Label.Top - 1),
            };
            settingMountRadialRemoveCenterMount_Checkbox.CheckedChanged += delegate {
                currentRadialSettings.RemoveCenterMount.Value = settingMountRadialRemoveCenterMount_Checkbox.Checked;
            };

            thingSettingsView = new ThingSettingsView(currentRadialSettings)
            {
                Location = new Point(0, settingMountRadialRemoveCenterMount_Label.Bottom),
                Parent = RadialSettingsDetailPanel,
                Width = 500,
                Height = 500
            };
        }
    }
}
