﻿using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Blish_HUD.Graphics.UI;
using System.Linq;
using Blish_HUD;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using Mounts;
using System;
using Blish_HUD.Input;

namespace Manlaan.Mounts.Views
{
    class SettingsView : View
    {
        private const string NoValueSelected = "Please select a value";

        private Texture2D anetTexture { get; }

        private List<KeybindingAssigner> KeybindingAssigners = new List<KeybindingAssigner>();
        private KeybindingAssigner settingDefaultMount_Keybind;
        Label labelExplanation;

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
            int mountsAndRadialInputWidth = 170;

            labelExplanation = new Label()
            {
                Location = new Point(10, 10),
                Width = 800,
                AutoSizeHeight = true,
                WrapText = true,
                Parent = buildPanel,
                TextColor = Color.Red,
                Font = GameService.Content.DefaultFont18,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            UpdateLabelText("");

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

            Panel thingsPanel = CreateDefaultPanel(buildPanel, new Point(panelPadding, labelExplanation.Bottom + panelPadding), 600);
            BuildThingsPanel(thingsPanel, labelWidth, bindingWidth, orderWidth);

            Panel generalSettingsPanel = CreateDefaultPanel(buildPanel, new Point(thingsPanel.Right + 20, labelExplanation.Bottom + panelPadding), 600);
            BuildGeneralSettingsPanel(generalSettingsPanel, labelWidth2, mountsAndRadialInputWidth);

            Panel radialPanel = CreateDefaultPanel(buildPanel, new Point(thingsPanel.Right + 20, 500), 600);
            BuildRadialSettingsPanel(radialPanel, labelWidth2, mountsAndRadialInputWidth);

            ValidateKeybindOverlaps();
        }

        private void UpdateLabelText(string addendum)
        {
            var mystring = "For this module to work you need to fill in your in-game keybindings in the settings below.\nNo keybind means the action is DISABLED. For more info, see the documentation.";
            if (!string.IsNullOrWhiteSpace(addendum))
                mystring = string.Concat(mystring, $"\n{addendum}");
            labelExplanation.Text = mystring.Replace(" ", "  ");
        }

        private void BuildThingsPanel(Panel mountsPanel, int labelWidth, int bindingWidth, int orderWidth)
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
                Text = "must match in-game key settings",
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
                KeybindingAssigners.Add(settingMount_Keybind);
                settingMount_Keybind.BindingChanged += delegate {
                    thing.KeybindingSetting.Value = settingMount_Keybind.KeyBinding;
                    ValidateKeybindOverlaps();
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

        private void ValidateKeybindOverlaps()
        {
            var kbaIssues = KeybindingAssigners.Where(k => settingDefaultMount_Keybind.KeyBinding.EqualsKeyBinding(k.KeyBinding) && k.KeyBinding.PrimaryKey != Microsoft.Xna.Framework.Input.Keys.None);
            if (kbaIssues.Any() && settingDefaultMount_Keybind.KeyBinding.PrimaryKey != Microsoft.Xna.Framework.Input.Keys.None)
            {
                foreach (var kbaIssue in kbaIssues)
                {
                    kbaIssue.BackgroundColor = Color.Red;
                }
                settingDefaultMount_Keybind.BackgroundColor = Color.Red;
                UpdateLabelText("Validation failed: overlapping keybinds are not supported!");
            }
            else
            {
                foreach (var kba in KeybindingAssigners)
                {
                    kba.BackgroundColor = Color.Transparent;
                }
                settingDefaultMount_Keybind.BackgroundColor = Color.Transparent;
                UpdateLabelText("");
            }
        }

        private void BuildGeneralSettingsPanel(Panel defaultMountPanel, int labelWidth2, int optionWidth)
        {
            Label settingDefaultMountKeybind_Label = new Label()
            {
                Location = new Point(0, 0),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Module keybind: ",
                BasicTooltipText = "The module keybind is used to trigger the behaviour in the next setting."
            };
            settingDefaultMount_Keybind = new KeybindingAssigner(Module._settingDefaultMountBinding.Value)
            {
                NameWidth = 0,
                Width = optionWidth,
                Size = new Point(optionWidth, 20),
                Parent = defaultMountPanel,
                Location = new Point(settingDefaultMountKeybind_Label.Right + 4, settingDefaultMountKeybind_Label.Top - 1)
            };
            settingDefaultMount_Keybind.BindingChanged += delegate {
                ValidateKeybindOverlaps();
            };
            Label settingKeybindBehaviour_Label = new Label()
            {
                Location = new Point(0, settingDefaultMountKeybind_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Module keybind behaviour: ",
                BasicTooltipText = "Either display the radial or use the default action when the module keybind is held down.\nBoth are dependent on the context the player is in.\nDefault: Radial"
            };
            Dropdown settingKeybindBehaviour_Select = new Dropdown()
            {
                Location = new Point(settingKeybindBehaviour_Label.Right + 5, settingKeybindBehaviour_Label.Top - 4),
                Width = optionWidth,
                Parent = defaultMountPanel,
            };
            settingKeybindBehaviour_Select.Items.Add("Disabled");
            List<string> keybindBehaviours = Module._keybindBehaviours.ToList();
            foreach (string i in keybindBehaviours)
            {
                settingKeybindBehaviour_Select.Items.Add(i.ToString());
            }
            settingKeybindBehaviour_Select.SelectedItem = keybindBehaviours.Any(m => m == Module._settingKeybindBehaviour.Value) ? Module._settingKeybindBehaviour.Value : "Disabled";
            settingKeybindBehaviour_Select.ValueChanged += delegate {
                Module._settingKeybindBehaviour.Value = settingKeybindBehaviour_Select.SelectedItem;
            };

            Label settingTapThresholdInMilliseconds_Label = new Label()
            {
                Location = new Point(0, settingKeybindBehaviour_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Module Keybind Tap Threshold:",
                BasicTooltipText = "The threshold to determine whether a module keybind press is a \"tap\" (in milliseconds).\nOnly applicable for contextual radial settings.\nDefault: 500ms (0.5s)."
            };
            TrackBar settingTapThresholdInMilliseconds_Slider = new TrackBar()
            {
                Location = new Point(settingTapThresholdInMilliseconds_Label.Right + 5, settingTapThresholdInMilliseconds_Label.Top),
                Width = optionWidth,
                MaxValue = 5000,
                MinValue = 0,
                Value = Module._settingTapThresholdInMilliseconds.Value,
                Parent = defaultMountPanel,
                BasicTooltipText = $"{Module._settingTapThresholdInMilliseconds.Value}"
            };
            settingTapThresholdInMilliseconds_Slider.ValueChanged += delegate {
                Module._settingTapThresholdInMilliseconds.Value = (int)settingTapThresholdInMilliseconds_Slider.Value;
                settingTapThresholdInMilliseconds_Slider.BasicTooltipText = $"{Module._settingTapThresholdInMilliseconds.Value}";
            };

            Label settingJumpbinding_Label = new Label()
            {
                Location = new Point(0, settingTapThresholdInMilliseconds_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "In-game Jump key binding: ",
                BasicTooltipText = "Used to detect gliding better for the IsPlayerGlidingOrFalling contextual radial settings."
            };
            new Image
            {
                Parent = defaultMountPanel,
                Size = new Point(16, 16),
                Location = new Point(settingJumpbinding_Label.Right - 80, settingJumpbinding_Label.Bottom - 16),
                Texture = anetTexture,
            };
            var settingJump_Keybind = new KeybindingAssigner(Module._settingJumpBinding.Value)
            {
                NameWidth = 0,
                Size = new Point(optionWidth, 20),
                Parent = defaultMountPanel,
                Location = new Point(settingJumpbinding_Label.Right + 4, settingJumpbinding_Label.Top - 1),
            };

            Label settingFallingOrGlidingUpdateFrequency_Label = new Label()
            {
                Location = new Point(0, settingJumpbinding_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Falling or gliding update frequency:",
                BasicTooltipText = "Used to detect gliding and falling better for the IsPlayerGlidingOrFalling contextual radial settings.\nLower: faster reaction (might cause flickering when holding the module keybind).\nHigher: slightly slower reaction time, but more stable detection.\nDefault: 0.1."
            };
            TrackBar settingFallingOrGlidingUpdateFrequency_Slider = new TrackBar()
            {
                Location = new Point(settingFallingOrGlidingUpdateFrequency_Label.Right + 5, settingFallingOrGlidingUpdateFrequency_Label.Top),
                Width = optionWidth,
                MaxValue = 1.0f,
                SmallStep = true,
                MinValue = 0.0f,
                Value = Module._settingFallingOrGlidingUpdateFrequency.Value,
                Parent = defaultMountPanel,
                BasicTooltipText = $"{Module._settingFallingOrGlidingUpdateFrequency.Value}"
            };
            settingFallingOrGlidingUpdateFrequency_Slider.ValueChanged += delegate { 
                Module._settingFallingOrGlidingUpdateFrequency.Value = settingFallingOrGlidingUpdateFrequency_Slider.Value;
                settingFallingOrGlidingUpdateFrequency_Slider.BasicTooltipText = $"{Module._settingFallingOrGlidingUpdateFrequency.Value}";
            };

            Label settingBlockSequenceFromGw2_Label = new Label()
            {
                Location = new Point(0, settingFallingOrGlidingUpdateFrequency_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Block sequence from GW2:",
                BasicTooltipText = "When checked, the sequence is not sent to GW2 otherwise it is sent to GW2.\nDefault: enabled."
            };
            Checkbox settingBlockSequenceFromGw2_Checkbox = new Checkbox()
            {
                Size = new Point(labelWidth2, 20),
                Parent = defaultMountPanel,
                Checked = Module._settingBlockSequenceFromGw2.Value,
                Location = new Point(settingBlockSequenceFromGw2_Label.Right + 5, settingBlockSequenceFromGw2_Label.Top - 1),
            };
            settingBlockSequenceFromGw2_Checkbox.CheckedChanged += delegate {
                Module._settingBlockSequenceFromGw2.Value = settingBlockSequenceFromGw2_Checkbox.Checked;
                Module.UserDefinedRadialSettings.ForEach(u => u.Keybind.Value.BlockSequenceFromGw2 = settingBlockSequenceFromGw2_Checkbox.Checked);
                Module._settingDefaultMountBinding.Value.BlockSequenceFromGw2 = settingBlockSequenceFromGw2_Checkbox.Checked;
            };


            Label settingDisplayModuleOnLoadingScreen_Label = new Label()
            {
                Location = new Point(0, settingBlockSequenceFromGw2_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Display module on loading screen:",
                BasicTooltipText = "Allow the module to be displayed on the loading screen."
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
                Text = "Mount automatically after loading screen:",
                BasicTooltipText = "This option allows the an action to be activated after loading screen. Only applicable for mounts.\nDefault: disabled."
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

            Label combatLaunchMasteryUnlocked_Label = new Label()
            {
                Location = new Point(0, settingMountAutomaticallyAfterLoadingScreen_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Combat Launch mastery unlocked: ",
                BasicTooltipText = "Combat Launch mastery allows the user to mount on the Skyscale mount when in combat.\nThis is detected via the API.\nIf you don't want to use Combat Mastery you can disable this option and out of combat queuing will still happen.\nDefault: disabled."
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


            Label settingEnableMountQueueing_Label = new Label()
            {
                Location = new Point(0, combatLaunchMasteryUnlocked_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Enable out of combat queueing:",
                BasicTooltipText = "When using an action that cannot be done in combat we queue this action and perform in when the player is out of combat.\nOnly the last action in combat will be performed.\nDefault: disabled"
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

            Label dragInfoPanel_Label = new Label()
            {
                Location = new Point(0, settingEnableMountQueueing_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                BasicTooltipText = "The info panel displays out of combat queueing, \"mount automatically after loading screen\" and ground target action.\nSee settings and documentation for more info.",
                Text = "Drag info panel: "
            };
            Checkbox dragInfoPanel_Checkbox = new Checkbox()
            {
                Size = new Point(20, 20),
                Parent = defaultMountPanel,
                Checked = Module._settingDragInfoPanel.Value,
                Location = new Point(dragInfoPanel_Label.Right + 5, dragInfoPanel_Label.Top - 1),
            };
            dragInfoPanel_Checkbox.CheckedChanged += delegate {
                Module._settingDragInfoPanel.Value = dragInfoPanel_Checkbox.Checked;
            };


            Label settingDisplayMountQueueing_Label = new Label()
            {
                Location = new Point(0, dragInfoPanel_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Display out of combat queueing:",
                BasicTooltipText = "Displays \"out of combat queueing\" in the info panel."
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


            Label settingDisplayLaterActivation_Label = new Label()
            {
                Location = new Point(0, settingDisplayMountQueueing_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Display \"mount automatically after loading screen\":",
                BasicTooltipText = "Displays \"mount automatically after loading screen\" in the info panel."
            };
            Checkbox settingDisplayLaterActivation_Checkbox = new Checkbox()
            {
                Size = new Point(labelWidth2, 20),
                Parent = defaultMountPanel,
                Checked = Module._settingDisplayLaterActivation.Value,
                Location = new Point(settingDisplayLaterActivation_Label.Right + 5, settingDisplayLaterActivation_Label.Top - 1),
            };
            settingDisplayLaterActivation_Checkbox.CheckedChanged += delegate {
                Module._settingDisplayLaterActivation.Value = settingDisplayLaterActivation_Checkbox.Checked;
            };


            Label settingDisplayGroundTargetAction_Label = new Label()
            {
                Location = new Point(0, settingDisplayLaterActivation_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Display ground target action:",
                BasicTooltipText = "Displays the \"ground target action\" in the info panel."
            };
            Checkbox settingDisplayGroundTargetAction_Checkbox = new Checkbox()
            {
                Size = new Point(labelWidth2, 20),
                Parent = defaultMountPanel,
                Checked = Module._settingDisplayGroundTargetingAction.Value,
                Location = new Point(settingDisplayGroundTargetAction_Label.Right + 5, settingDisplayGroundTargetAction_Label.Top - 1),
            };
            settingDisplayGroundTargetAction_Checkbox.CheckedChanged += delegate {
                Module._settingDisplayGroundTargetingAction.Value = settingDisplayGroundTargetAction_Checkbox.Checked;
            };

            Label settingGroundTargetting_Label = new Label()
            {
                Location = new Point(0, settingDisplayGroundTargetAction_Label.Bottom + 6),
                Width = labelWidth2,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = defaultMountPanel,
                Text = "Ground targeting: ",
                BasicTooltipText = "Normal - Show range indicator on first press, cast on second.\nFast with range indicator - Show range indicator on keypress, cast on release.\nInstant - Instantly cast at your mouse cursor's location."
            };
            new Image
            {
                Parent = defaultMountPanel,
                Size = new Point(16, 16),
                Location = new Point(settingGroundTargetting_Label.Right - 140, settingGroundTargetting_Label.Bottom - 16),
                Texture = anetTexture,
            };
            Dropdown settingGroundTargetting_Select = new Dropdown()
            {
                Location = new Point(settingGroundTargetting_Label.Right + 5, settingGroundTargetting_Label.Top - 4),
                Width = optionWidth,
                Parent = defaultMountPanel,
            };
            foreach (var i in Enum.GetValues(typeof(GroundTargeting)))
            {
                settingGroundTargetting_Select.Items.Add(i.ToString());
            }
            settingGroundTargetting_Select.SelectedItem = Module._settingGroundTargeting.Value.ToString();
            settingGroundTargetting_Select.ValueChanged += delegate {
                Module._settingGroundTargeting.Value = (GroundTargeting)Enum.Parse(typeof(GroundTargeting), settingGroundTargetting_Select.SelectedItem);
            };






        }

        private void BuildRadialSettingsPanel(Container radialPanel, int labelWidth, int mountsAndRadialInputWidth)
        {
            Label settingMountRadialSettingsMount_Label = new Label()
            {
                Location = new Point(0,0),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = radialPanel,
                Text = "Radial settings: ",
                BasicTooltipText = "Settings applied to all radials, both contextual and user-defined."
            };
            Label settingMountRadialSpawnAtMouse_Label = new Label()
            {
                Location = new Point(0, settingMountRadialSettingsMount_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = radialPanel,
                Text = "Spawn at mouse: ",
                BasicTooltipText = "When enabled the radials will spawn at your current mouse position, otherwise they spawn at the middle of your screen and your mouse cursor will be moved towards that by the module.\nDefault: disabled."
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
                BasicTooltipText = "Configures the size of the radials."
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
            settingMountRadialRadiusModifier_Slider.ValueChanged += delegate {
                Module._settingMountRadialRadiusModifier.Value = settingMountRadialRadiusModifier_Slider.Value / 100;
                settingMountRadialRadiusModifier_Slider.BasicTooltipText = $"{Module._settingMountRadialRadiusModifier.Value}";
            };
            Label settingMountRadialStartAngle_Label = new Label()
            {
                Location = new Point(0, settingMountRadialRadiusModifier_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = radialPanel,
                Text = "Start angle: ",
                BasicTooltipText = "Configures the start point of the first action in the radials when displaying."
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
            settingMountRadialStartAngle_Slider.ValueChanged += delegate {
                Module._settingMountRadialStartAngle.Value = settingMountRadialStartAngle_Slider.Value / 360;
                settingMountRadialStartAngle_Slider.BasicTooltipText = $"{Module._settingMountRadialStartAngle.Value}";
            };
            Label settingMountRadialIconSizeModifier_Label = new Label()
            {
                Location = new Point(0, settingMountRadialStartAngle_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = radialPanel,
                Text = "Icon size: ",
                BasicTooltipText = "Configures the icon size when displaying icons in the radials."
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
            settingMountRadialIconSizeModifier_Slider.ValueChanged += delegate {
                Module._settingMountRadialIconSizeModifier.Value = settingMountRadialIconSizeModifier_Slider.Value / 100;
                settingMountRadialIconSizeModifier_Slider.BasicTooltipText = $"{Module._settingMountRadialIconSizeModifier.Value}";
            };
            Label settingMountRadialIconOpacity_Label = new Label()
            {
                Location = new Point(0, settingMountRadialIconSizeModifier_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = radialPanel,
                Text = "Icon opacity: ",
                BasicTooltipText = "Configures the icon opacity when displaying icons in the radials."
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
            settingMountRadialIconOpacity_Slider.ValueChanged += delegate {
                Module._settingMountRadialIconOpacity.Value = settingMountRadialIconOpacity_Slider.Value / 100;
                settingMountRadialIconOpacity_Slider.BasicTooltipText = $"{Module._settingMountRadialIconOpacity.Value}";
            };

            Label settingMountRadialToggleActionCameraKeyBinding_Label = new Label()
            {
                Location = new Point(0, settingMountRadialIconOpacity_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = radialPanel,
                Text = "In-game action camera key binding: ",
                BasicTooltipText = "Used to toggle the action camera in-game when displaying a radial to help with selecting an action."
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

    public static class KeyBindingExtensions
    {
        public static bool EqualsKeyBinding(this KeyBinding firstKeyBinding, KeyBinding secondKeyBinding)
        {
            if (firstKeyBinding == null || secondKeyBinding == null)
                return false;

            return firstKeyBinding.PrimaryKey == secondKeyBinding.PrimaryKey &&
                   firstKeyBinding.ModifierKeys == secondKeyBinding.ModifierKeys;
        }
    }
}
