﻿using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Blish_HUD.Graphics.UI;
using System.Linq;
using Blish_HUD;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using Mounts;

namespace Manlaan.Mounts.Views
{
    class SettingsView : View
    {
        private const string NoValueSelected = "Please select a value";

        private Texture2D anetTexture { get; }

        private Panel ManualPanel { get; set; }

        public SettingsView(TextureCache textureCache)
        {
            anetTexture = textureCache.GetImgFile(TextureCache.AnetIconTextureName);
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
            int labelWidth                = 150;
            int labelWidth2               = 250;
            int orderWidth                = 80;
            int bindingWidth              = 170;
            int mountsAndRadialInputWidth = 125;

            Label labelExplanation = new Label()
            {
                Location = new Point(10, 10),
                Width = 800,
                AutoSizeHeight = true,
                WrapText = true,
                Parent = buildPanel,
                TextColor = Color.Red,
                Font = GameService.Content.DefaultFont18,
                Text = "For this module to work you need to fill in your in-game keybindings in the settings below.\nNo keybind means the mount is DISABLED.".Replace(" ", "  "),
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

            Panel mountsPanel = CreateDefaultPanel(buildPanel, new Point(panelPadding, labelExplanation.Bottom + panelPadding), 600);
            BuildMountsPanel(mountsPanel, labelWidth, bindingWidth, orderWidth);

            Panel defaultMountPanel = CreateDefaultPanel(buildPanel, new Point(mountsPanel.Right + 20, labelExplanation.Bottom + panelPadding));
            BuildDefaultMountPanel(defaultMountPanel, labelWidth2, mountsAndRadialInputWidth);

            Panel radialPanel = CreateDefaultPanel(buildPanel, new Point(mountsPanel.Right + 20, 500));
            BuildRadialPanel(radialPanel, labelWidth2, mountsAndRadialInputWidth);
        }



        private void BuildMountsPanel(Panel mountsPanel, int labelWidth, int bindingWidth, int orderWidth)
        {
            var anetImage = new Image
            {
                Parent = mountsPanel,
                Size = new Point(16, 16),
                Location = new Point(5, 2),
                Texture = anetTexture,
            };
            Label keybindWarning_Label = new Label()
            {
                Location = new Point(anetImage.Right + 3, anetImage.Bottom - 16),
                Width = 300,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = mountsPanel,
                Text = "must match in-game key binding",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            Label settingBinding_Label = new Label()
            {
                Location = new Point(labelWidth + 5, keybindWarning_Label.Bottom + 6),
                Width = bindingWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = mountsPanel,
                Text = "In-game key binding",
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            var settingBinding_Image = new Image
            {
                Parent = mountsPanel,
                Size = new Point(16, 16),
                Location = new Point(settingBinding_Label.Right - 20, settingBinding_Label.Bottom - 16),
                Texture = anetTexture,
            };

            Label settingMountImageFile_Label = new Label()
            {
                Location = new Point(settingBinding_Image.Right + 5, settingBinding_Label.Top),
                Width = bindingWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = mountsPanel,
                Text = "Image",
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            int curY = settingBinding_Label.Bottom;

            foreach (var thing in Module._things)
            {
                Label settingMount_Label = new Label()
                {
                    Location = new Point(0, curY + 6),
                    Width = labelWidth,
                    AutoSizeHeight = false,
                    WrapText = false,
                    Parent = mountsPanel,
                    Text = $"{thing.DisplayName}: ",
                };
                KeybindingAssigner settingMount_Keybind = new KeybindingAssigner(thing.KeybindingSetting.Value)
                {
                    NameWidth = 0,
                    Size = new Point(bindingWidth, 20),
                    Parent = mountsPanel,
                    Location = new Point(settingMount_Label.Right + 5, settingMount_Label.Top - 1),
                };
                settingMount_Keybind.BindingChanged += delegate {
                    thing.KeybindingSetting.Value = settingMount_Keybind.KeyBinding;
                };

                Dropdown settingMountImageFile_Select = new Dropdown()
                {
                    Location = new Point(settingMount_Keybind.Right + 5, settingMount_Label.Top - 4),
                    Width = 200,
                    Parent = mountsPanel,
                };
                settingMountImageFile_Select.Items.Add(NoValueSelected);
                Module._thingImageFiles
                    .Where(mIF => mIF.Name.Contains(thing.ImageFileName)).OrderByDescending(mIF => mIF.Name).ToList()
                    .ForEach(mIF => settingMountImageFile_Select.Items.Add(mIF.Name));
                settingMountImageFile_Select.SelectedItem = thing.ImageFileNameSetting.Value == "" ? NoValueSelected : thing.ImageFileNameSetting.Value;
                settingMountImageFile_Select.ValueChanged += delegate {
                    if (settingMountImageFile_Select.SelectedItem.Equals(NoValueSelected))
                        thing.ImageFileNameSetting.Value = "";
                    else
                        thing.ImageFileNameSetting.Value = settingMountImageFile_Select.SelectedItem;
                };

                curY = settingMount_Label.Bottom;
            }
        }

        private void BuildDefaultMountPanel(Panel defaultMountPanel, int labelWidth2, int mountsAndRadialInputWidth)
        {
            Label settingDefaultSettingsMount_Label = new Label()
            {
                Location = new Point(0, 0),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Default mount settings: "
            };                   
            Label settingDefaultMountKeybind_Label = new Label()
            {
                Location = new Point(0, settingDefaultSettingsMount_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Key binding: ",
            };
            KeybindingAssigner settingDefaultMount_Keybind = new KeybindingAssigner(Module._settingDefaultMountBinding.Value)
            {
                NameWidth = 0,
                Size = new Point(mountsAndRadialInputWidth, 20),
                Parent = defaultMountPanel,
                Location = new Point(settingDefaultMountKeybind_Label.Right + 4, settingDefaultMountKeybind_Label.Top - 1),
            };
            Label settingDefaultMountBehaviour_Label = new Label()
            {
                Location = new Point(0, settingDefaultMountKeybind_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Keybind behaviour: ",
            };
            Dropdown settingDefaultMountBehaviour_Select = new Dropdown()
            {
                Location = new Point(settingDefaultMountBehaviour_Label.Right + 5, settingDefaultMountBehaviour_Label.Top - 4),
                Width = settingDefaultMount_Keybind.Width,
                Parent = defaultMountPanel,
            };
            settingDefaultMountBehaviour_Select.Items.Add("Disabled");
            List<string> mountBehaviours = Module._mountBehaviour.ToList();
            foreach (string i in mountBehaviours)
            {
                settingDefaultMountBehaviour_Select.Items.Add(i.ToString());
            }
            settingDefaultMountBehaviour_Select.SelectedItem = mountBehaviours.Any(m => m == Module._settingDefaultMountBehaviour.Value) ? Module._settingDefaultMountBehaviour.Value : "Disabled";
            settingDefaultMountBehaviour_Select.ValueChanged += delegate {
                Module._settingDefaultMountBehaviour.Value = settingDefaultMountBehaviour_Select.SelectedItem;
            };

            Label settingDisplayModuleOnLoadingScreen_Label = new Label()
            {
                Location = new Point(0, settingDefaultMountBehaviour_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Display module on loading screen:"
            };
            Checkbox settingDisplayModuleOnLoadingScreen_Checkbox = new Checkbox()
            {
                Size = new Point(labelWidth2, 20),
                Parent = defaultMountPanel,
                Checked = Module._settingDisplayModuleOnLoadingScreen.Value,
                Location = new Point(settingDisplayModuleOnLoadingScreen_Label.Right + 5, settingDisplayModuleOnLoadingScreen_Label.Top - 1),
            };
            settingDisplayModuleOnLoadingScreen_Checkbox.CheckedChanged += delegate {
                Module._settingDisplayModuleOnLoadingScreen.Value = settingDisplayModuleOnLoadingScreen_Checkbox.Checked;
            };

            Label settingMountAutomaticallyAfterLoadingScreen_Label = new Label()
            {
                Location = new Point(0, settingDisplayModuleOnLoadingScreen_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Mount automatically after loading screen:"
            };
            Checkbox settingMountAutomaticallyAfterLoadingScreen_Checkbox = new Checkbox()
            {
                Size = new Point(labelWidth2, 20),
                Parent = defaultMountPanel,
                Checked = Module._settingMountAutomaticallyAfterLoadingScreen.Value,
                Location = new Point(settingMountAutomaticallyAfterLoadingScreen_Label.Right + 5, settingMountAutomaticallyAfterLoadingScreen_Label.Top - 1),
            };
            settingMountAutomaticallyAfterLoadingScreen_Checkbox.CheckedChanged += delegate {
                Module._settingMountAutomaticallyAfterLoadingScreen.Value = settingMountAutomaticallyAfterLoadingScreen_Checkbox.Checked;
            };


            Label settingEnableMountQueueing_Label = new Label()
            {
                Location = new Point(0, settingMountAutomaticallyAfterLoadingScreen_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Enable out of combat queueing:"
            };
            Checkbox settingEnableMountQueueing_Checkbox = new Checkbox()
            {
                Size = new Point(labelWidth2, 20),
                Parent = defaultMountPanel,
                Checked = Module._settingEnableMountQueueing.Value,
                Location = new Point(settingEnableMountQueueing_Label.Right + 5, settingEnableMountQueueing_Label.Top - 1),
            };
            settingEnableMountQueueing_Checkbox.CheckedChanged += delegate {
                Module._settingEnableMountQueueing.Value = settingEnableMountQueueing_Checkbox.Checked;
            };


            Label settingDisplayMountQueueing_Label = new Label()
            {
                Location = new Point(0, settingEnableMountQueueing_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Display out of combat queueing:"
            };
            Checkbox settingDisplayMountQueueing_Checkbox = new Checkbox()
            {
                Size = new Point(labelWidth2, 20),
                Parent = defaultMountPanel,
                Checked = Module._settingDisplayMountQueueing.Value,
                Location = new Point(settingDisplayMountQueueing_Label.Right + 5, settingDisplayMountQueueing_Label.Top - 1),
            };
            settingDisplayMountQueueing_Checkbox.CheckedChanged += delegate {
                Module._settingDisplayMountQueueing.Value = settingDisplayMountQueueing_Checkbox.Checked;
            };

            Label dragMountQueueing_Label = new Label()
            {
                Location = new Point(0, settingDisplayMountQueueing_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Drag out of combat queueing: "
            };
            Checkbox dragMountQueueing_Checkbox = new Checkbox()
            {
                Size = new Point(20, 20),
                Parent = defaultMountPanel,
                Checked = Module._settingDragMountQueueing.Value,
                Location = new Point(dragMountQueueing_Label.Right + 5, dragMountQueueing_Label.Top - 1),
            };
            dragMountQueueing_Checkbox.CheckedChanged += delegate {
                Module._settingDragMountQueueing.Value = dragMountQueueing_Checkbox.Checked;
            };



            Label combatLaunchMasteryUnlocked_Label = new Label()
            {
                Location = new Point(0, dragMountQueueing_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Combat Launch mastery unlocked: ",
                BasicTooltipText = "EoD and SotO masteries are not detectable in the API yet, see documentation for more info."
            };
            Checkbox combatLaunchMasteryUnlocked_Checkbox = new Checkbox()
            {
                Size = new Point(20, 20),
                Parent = defaultMountPanel,
                Checked = Module._settingCombatLaunchMasteryUnlocked.Value,
                Location = new Point(combatLaunchMasteryUnlocked_Label.Right + 5, combatLaunchMasteryUnlocked_Label.Top - 1),
            };
            combatLaunchMasteryUnlocked_Checkbox.CheckedChanged += delegate {
                Module._settingCombatLaunchMasteryUnlocked.Value = combatLaunchMasteryUnlocked_Checkbox.Checked;
            };
        }

        private void BuildRadialPanel(Container radialPanel, int labelWidth, int mountsAndRadialInputWidth)
        {
            Label settingMountRadialSettingsMount_Label = new Label()
            {
                Location = new Point(0,0),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = radialPanel,
                Text = "Radial settings: ",
            };
            Label settingMountRadialSpawnAtMouse_Label = new Label()
            {
                Location = new Point(0, settingMountRadialSettingsMount_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = radialPanel,
                Text = "Spawn at mouse: ",
            };
            Checkbox settingMountRadialSpawnAtMouse_Checkbox = new Checkbox()
            {
                Size = new Point(labelWidth, 20),
                Parent = radialPanel,
                Checked = Module._settingMountRadialSpawnAtMouse.Value,
                Location = new Point(settingMountRadialSpawnAtMouse_Label.Right + 5, settingMountRadialSpawnAtMouse_Label.Top - 1),
            };
            settingMountRadialSpawnAtMouse_Checkbox.CheckedChanged += delegate {
                Module._settingMountRadialSpawnAtMouse.Value = settingMountRadialSpawnAtMouse_Checkbox.Checked;
            };
            Label settingMountRadialRadiusModifier_Label = new Label()
            {
                Location = new Point(0, settingMountRadialSpawnAtMouse_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = radialPanel,
                Text = "Radius: ",
            };
            TrackBar settingMountRadialRadiusModifier_Slider = new TrackBar()
            {
                Location = new Point(settingMountRadialRadiusModifier_Label.Right + 5, settingMountRadialRadiusModifier_Label.Top),
                Width = mountsAndRadialInputWidth,
                MaxValue = 100,
                MinValue = 20,
                Value = Module._settingMountRadialRadiusModifier.Value * 100,
                Parent = radialPanel,
            };
            settingMountRadialRadiusModifier_Slider.ValueChanged += delegate { Module._settingMountRadialRadiusModifier.Value = settingMountRadialRadiusModifier_Slider.Value / 100; };
            Label settingMountRadialStartAngle_Label = new Label()
            {
                Location = new Point(0, settingMountRadialRadiusModifier_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = radialPanel,
                Text = "Start angle: ",
            };
            TrackBar settingMountRadialStartAngle_Slider = new TrackBar()
            {
                Location = new Point(settingMountRadialStartAngle_Label.Right + 5, settingMountRadialStartAngle_Label.Top),
                Width = mountsAndRadialInputWidth,
                MaxValue = 360,
                MinValue = 0,
                Value = Module._settingMountRadialStartAngle.Value * 360,
                Parent = radialPanel,
            };
            settingMountRadialStartAngle_Slider.ValueChanged += delegate { Module._settingMountRadialStartAngle.Value = settingMountRadialStartAngle_Slider.Value / 360; };
            Label settingMountRadialIconSizeModifier_Label = new Label()
            {
                Location = new Point(0, settingMountRadialStartAngle_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = radialPanel,
                Text = "Icon size: ",
            };
            TrackBar settingMountRadialIconSizeModifier_Slider = new TrackBar()
            {
                Location = new Point(settingMountRadialIconSizeModifier_Label.Right + 5, settingMountRadialIconSizeModifier_Label.Top),
                Width = mountsAndRadialInputWidth,
                MaxValue = 100,
                MinValue = 5,
                Value = Module._settingMountRadialIconSizeModifier.Value * 100,
                Parent = radialPanel,
            };
            settingMountRadialIconSizeModifier_Slider.ValueChanged += delegate { Module._settingMountRadialIconSizeModifier.Value = settingMountRadialIconSizeModifier_Slider.Value / 100; };
            Label settingMountRadialIconOpacity_Label = new Label()
            {
                Location = new Point(0, settingMountRadialIconSizeModifier_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = radialPanel,
                Text = "Icon opacity: ",
            };
            TrackBar settingMountRadialIconOpacity_Slider = new TrackBar()
            {
                Location = new Point(settingMountRadialIconOpacity_Label.Right + 5, settingMountRadialIconOpacity_Label.Top),
                Width = mountsAndRadialInputWidth,
                MaxValue = 100,
                MinValue = 5,
                Value = Module._settingMountRadialIconOpacity.Value * 100,
                Parent = radialPanel,
            };
            settingMountRadialIconOpacity_Slider.ValueChanged += delegate { Module._settingMountRadialIconOpacity.Value = settingMountRadialIconOpacity_Slider.Value / 100; };

            Label settingMountRadialToggleActionCameraKeyBinding_Label = new Label()
            {
                Location = new Point(0, settingMountRadialIconOpacity_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = radialPanel,
                Text = "In-game action camera key binding: ",
            };
            new Image
            {
                Parent = radialPanel,
                Size = new Point(16, 16),
                Location = new Point(settingMountRadialToggleActionCameraKeyBinding_Label.Right - 32, settingMountRadialToggleActionCameraKeyBinding_Label.Bottom - 16),
                Texture = anetTexture,
            };
            KeybindingAssigner settingMountRadialToggleActionCameraKeyBinding_Keybind = new KeybindingAssigner(Module._settingMountRadialToggleActionCameraKeyBinding.Value)
            {
                NameWidth = 0,
                Size = new Point(mountsAndRadialInputWidth, 20),
                Parent = radialPanel,
                Location = new Point(settingMountRadialToggleActionCameraKeyBinding_Label.Right + 4, settingMountRadialToggleActionCameraKeyBinding_Label.Top - 1),
            };
        }
    }
}
