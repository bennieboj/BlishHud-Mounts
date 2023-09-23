using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Blish_HUD;
using System.Diagnostics;
using System.Linq;
using Blish_HUD.Graphics.UI;
using Mounts;
using Blish_HUD.Settings.UI.Views;
using Mounts.Settings;
using System;

namespace Manlaan.Mounts.Views
{
    class IconThingSettingsView : View
    {
        private int labelWidth = 170;

        private Panel IconSettingsListPanel;
        private Panel IconSettingsDetailPanel;

        private IconThingSettings currentIconSettings;
        
        public IconThingSettingsView()
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

            IconSettingsListPanel = CreateDefaultPanel(buildPanel, new Point(panelPadding, labelExplanation.Bottom + panelPadding), 600);
            CreateRadialSettingsListPanel();

            currentIconSettings = Module.IconThingSettings.First();
            IconSettingsDetailPanel = CreateDefaultPanel(buildPanel, new Point(10, 300));
            BuildRadialSettingsDetailPanel();
        }

        private void CreateRadialSettingsListPanel()
        {
            Label nameHeader_Label = new Label()
            {
                Location = new Point(0, 10),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = IconSettingsListPanel,
                Text = "Name",
                HorizontalAlignment = HorizontalAlignment.Center,
            };


            int curY = nameHeader_Label.Bottom + 6;

            foreach (var radialSettings in Module.IconThingSettings)
            {
                Label name_Label = new Label()
                {
                    Location = new Point(0, curY + 6),
                    Width = labelWidth,
                    AutoSizeHeight = false,
                    WrapText = false,
                    Parent = IconSettingsListPanel,
                    Text = $"{radialSettings.Name}: ",
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
                var editRadialSettingsButton = new StandardButton
                {
                    Parent = IconSettingsListPanel,
                    Location = new Point(name_Label.Right, name_Label.Top),
                    Text = Strings.Edit
                };
                editRadialSettingsButton.Click += (args, sender) => {
                    SetCurrentSettings(radialSettings);
                    BuildRadialSettingsDetailPanel();
                };

                curY = name_Label.Bottom;
            }            
        }

        private void SetCurrentSettings(IconThingSettings settings)
        {
            currentIconSettings = settings;
        }

        private void BuildRadialSettingsDetailPanel()
        {
            IconSettingsDetailPanel.ClearChildren();
           
            Label radialSettingstName_Label = new Label()
            {
                Location = new Point(0, 0),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = IconSettingsDetailPanel,
                Text = $"{currentIconSettings.Name}"
            };

            Label radialSettingsIsEnabled_Label = new Label()
            {
                Location = new Point(0, radialSettingstName_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = IconSettingsDetailPanel,
                Text = "Enabled"
            };
            Checkbox radialSettingsIsEnabled_Checkbox = new Checkbox()
            {
                Size = new Point(20, 20),
                Parent = IconSettingsDetailPanel,
                Checked = currentIconSettings.IsEnabled.Value,
                Location = new Point(radialSettingsIsEnabled_Label.Right + 5, radialSettingsIsEnabled_Label.Top - 1),
            };
            radialSettingsIsEnabled_Checkbox.CheckedChanged += delegate {
                currentIconSettings.IsEnabled.Value = radialSettingsIsEnabled_Checkbox.Checked;
            };

            Label settingManualOrientation_Label = new Label()
            {
                Location = new Point(0, radialSettingsIsEnabled_Label.Bottom + 6),
                Width = 75,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = IconSettingsDetailPanel,
                Text = "Orientation: ",
            };
            Dropdown settingManualOrientation_Select = new Dropdown()
            {
                Location = new Point(settingManualOrientation_Label.Right + 5, settingManualOrientation_Label.Top - 4),
                Width = 100,
                Parent = IconSettingsDetailPanel,
            };
            foreach (IconOrientation s in Enum.GetValues(typeof(IconOrientation)))
            {
                settingManualOrientation_Select.Items.Add(s.ToString());
            }
            settingManualOrientation_Select.SelectedItem = currentIconSettings.Orientation.Value.ToString();
            settingManualOrientation_Select.ValueChanged += delegate {
                currentIconSettings.Orientation.Value = (IconOrientation) Enum.Parse(typeof(IconOrientation), settingManualOrientation_Select.SelectedItem);
            };

            Label settingManualWidth_Label = new Label()
            {
                Location = new Point(0, settingManualOrientation_Label.Bottom + 6),
                Width = 75,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = IconSettingsDetailPanel,
                Text = "Icon Width: ",
            };
            TrackBar settingImgWidth_Slider = new TrackBar()
            {
                Location = new Point(settingManualWidth_Label.Right + 5, settingManualWidth_Label.Top),
                Width = 220,
                MaxValue = 200,
                MinValue = 0,
                Value = currentIconSettings.Size.Value,
                Parent = IconSettingsDetailPanel,
            };
            settingImgWidth_Slider.ValueChanged += delegate { currentIconSettings.Size.Value = (int)settingImgWidth_Slider.Value; };

            Label settingManualOpacity_Label = new Label()
            {
                Location = new Point(0, settingManualWidth_Label.Bottom + 6),
                Width = 75,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = IconSettingsDetailPanel,
                Text = "Opacity: ",
            };
            TrackBar settingOpacity_Slider = new TrackBar()
            {
                Location = new Point(settingManualOpacity_Label.Right + 5, settingManualOpacity_Label.Top),
                Width = 220,
                MaxValue = 100,
                MinValue = 0,
                Value = currentIconSettings.Opacity.Value * 100,
                Parent = IconSettingsDetailPanel,
            };
            settingOpacity_Slider.ValueChanged += delegate { currentIconSettings.Opacity.Value = settingOpacity_Slider.Value / 100; };

            Label radialSettingsIsDraggingEnabled_Label = new Label()
            {
                Location = new Point(0, settingManualOpacity_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = IconSettingsDetailPanel,
                Text = "Drag: "
            };
            Checkbox radialSettingsIsDraggingEnabled_Checkbox = new Checkbox()
            {
                Size = new Point(20, 20),
                Parent = IconSettingsDetailPanel,
                Checked = currentIconSettings.IsDraggingEnabled.Value,
                Location = new Point(radialSettingsIsDraggingEnabled_Label.Right + 5, radialSettingsIsDraggingEnabled_Label.Top - 1),
            };
            radialSettingsIsDraggingEnabled_Checkbox.CheckedChanged += delegate {
                currentIconSettings.IsDraggingEnabled.Value = radialSettingsIsDraggingEnabled_Checkbox.Checked;
            };

            ThingSettingsView thingSettingsView = new ThingSettingsView(currentIconSettings)
            {
                Location = new Point(0, radialSettingsIsDraggingEnabled_Label.Bottom),
                Parent = IconSettingsDetailPanel,
                Width = 500,
                Height = 500
            };
        }
    }
}
