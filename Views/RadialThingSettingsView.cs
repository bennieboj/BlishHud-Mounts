﻿using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Blish_HUD;
using System.Diagnostics;
using System.Linq;
using Blish_HUD.Graphics.UI;
using Mounts;
using System;
using Mounts.Settings;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Manlaan.Mounts.Views
{
    class RadialThingSettingsView : View
    {
        private int totalWidth = 2500;
        private int labelWidth = 170;
        private int bindingWidth = 170;

        private Panel RadialSettingsListPanel;
        private Panel RadialSettingsDetailPanel;

        private RadialThingSettings currentRadialSettings;
        private readonly Func<KeybindTriggerType, Task> _keybindCallback;
        private readonly Helper _helper;

        public RadialThingSettingsView(Func<KeybindTriggerType, Task> keybindCallback, Helper helper)
        {
            _keybindCallback = keybindCallback;
            _helper = helper;
        }
        private Panel CreateDefaultPanel(Container buildPanel, Point location, int width)
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
                Text = "These radial settings dictate which actions are being displayed in contextual and user-defined radials.\nFor more info, see the documentation.".Replace(" ", "  "),
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

            RadialSettingsListPanel = CreateDefaultPanel(buildPanel, new Point(panelPadding, labelExplanation.Bottom + panelPadding), totalWidth);
            BuildRadialSettingsListPanel();

            RadialSettingsDetailPanel = CreateDefaultPanel(buildPanel, new Point(10, 500), totalWidth);

            var settingsToDisplayOnOpen = (RadialThingSettings)Module.ContextualRadialSettings.Single(settings => settings.IsDefault);
            //used when to go to specific radial settings from the radial when no settings are configured!
            var triggeredRadialSettings = _helper.GetTriggeredRadialSettings();
            if (triggeredRadialSettings != null)
            {
                settingsToDisplayOnOpen = triggeredRadialSettings;
            }
            BuildRadialSettingsDetailPanel(settingsToDisplayOnOpen);
        }

        private void BuildRadialSettingsListPanel()
        {
            RadialSettingsListPanel.ClearChildren();

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
                BasicTooltipText = "Determines the order of evaluation of the contextual radials, 0 is checked first, then 1, etc.\nWhen an active contextual radial is found the evaluation stops."
            };
            Label enabledHeader_label = new Label()
            {
                Location = new Point(orderHeader_label.Right + 5, nameHeader_Label.Top),
                Width = bindingWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsListPanel,
                Text = "Enabled",
                BasicTooltipText = "Disabled radials are not taken into account for the evaluation and are thus not displayed.",
                HorizontalAlignment = HorizontalAlignment.Center,
            };


            int curY = nameHeader_Label.Bottom + 6;


            foreach (RadialThingSettings radialSettings in _helper.GetAllGenericRadialThingSettings())
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
                
                var orderText = "N/A";
                if (radialSettings is ContextualRadialThingSettings contextualRadialSettings)
                {
                    orderText = $"{contextualRadialSettings.Order}";
                }
                Label order_Label = new Label()
                {
                    Location = new Point(orderHeader_label.Left, name_Label.Top),
                    Width = labelWidth,
                    AutoSizeHeight = false,
                    WrapText = false,
                    Parent = RadialSettingsListPanel,
                    Text = orderText,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                Label enabled_Label = new Label()
                {
                    Location = new Point(enabledHeader_label.Left, name_Label.Top),
                    Width = labelWidth,
                    AutoSizeHeight = false,
                    WrapText = false,
                    Parent = RadialSettingsListPanel,
                    Text = radialSettings.IsEnabled.Value ? "Yes" : "No",
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                var editRadialSettingsButton = new StandardButton
                {
                    Parent = RadialSettingsListPanel,
                    Location = new Point(enabled_Label.Right, name_Label.Top),
                    Text = Strings.Edit
                };
                editRadialSettingsButton.Click += (args, sender) => {
                    BuildRadialSettingsDetailPanel(radialSettings);
                };

                if (radialSettings is UserDefinedRadialThingSettings userDefinedRadialSettingsDelete)
                {
                    var deleteRadialSettingsButton = new StandardButton
                    {
                        Parent = RadialSettingsListPanel,
                        Location = new Point(editRadialSettingsButton.Right, editRadialSettingsButton.Top),
                        Text = Strings.Delete
                    };
                    deleteRadialSettingsButton.Click += (args, sender) =>
                    {
                        int deleteIndex = Module.UserDefinedRadialSettings.IndexOf(userDefinedRadialSettingsDelete);
                        Module.UserDefinedRadialSettings = Module.UserDefinedRadialSettings.Where(udrs => udrs.Id != userDefinedRadialSettingsDelete.Id).ToList();
                        userDefinedRadialSettingsDelete.DeleteFromSettings(Module.settingscollection);
                        Module._settingUserDefinedRadialIds.Value = Module._settingUserDefinedRadialIds.Value.Where(id => id != userDefinedRadialSettingsDelete.Id).ToList();
                        BuildRadialSettingsListPanel();

                        var newindex = Math.Min(deleteIndex, Module.UserDefinedRadialSettings.Count - 1);
                        if (newindex >= 0)
                        {
                            currentRadialSettings = Module.UserDefinedRadialSettings.ElementAt(newindex);
                        }
                        else
                        {
                            currentRadialSettings = Module.ContextualRadialSettings.Last();
                        }
                        BuildRadialSettingsDetailPanel();
                    };
                }

                curY = name_Label.Bottom;
            }

            var addUserDefinedRadialSettings_Button = new StandardButton
            {
                Parent = RadialSettingsListPanel,
                Location = new Point(0, curY + 6),
                Text = Strings.Add_UserDefined_Radial,
                Width = labelWidth,
                Enabled = Module._settingUserDefinedRadialIds.Value.Count <= 5
            };
            addUserDefinedRadialSettings_Button.Click += (args, sender) => {
                int nextId = Module._settingUserDefinedRadialIds.Value.OrderByDescending(id => id).FirstOrDefault() + 1;
                Module.UserDefinedRadialSettings.Add(new UserDefinedRadialThingSettings(Module.settingscollection, nextId, _keybindCallback));
                Module._settingUserDefinedRadialIds.Value = Module._settingUserDefinedRadialIds.Value.Append(nextId).ToList();
                BuildRadialSettingsListPanel();
                currentRadialSettings = Module.UserDefinedRadialSettings.Last();
                BuildRadialSettingsDetailPanel();
            };
        }

        private void BuildRadialSettingsDetailPanel(RadialThingSettings newCurrentRadialSettings = null)
        {
            if(newCurrentRadialSettings != null)
            {
                if(currentRadialSettings != null)
                {
                    currentRadialSettings.RadialSettingsUpdated -= CurrentRadialSettings_RadialSettingsUpdated;
                }
                currentRadialSettings = newCurrentRadialSettings;
                currentRadialSettings.RadialSettingsUpdated += CurrentRadialSettings_RadialSettingsUpdated;
            }
            
            RadialSettingsDetailPanel.ClearChildren();

            Label radialSettingsName_Label = new Label()
            {
                Location = new Point(0, 0),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsDetailPanel,
                Text = $"{currentRadialSettings.Name}"
            };

            var curY = radialSettingsName_Label.Bottom;
            if (currentRadialSettings is UserDefinedRadialThingSettings userDefinedRadialSettingsName)
            {
                radialSettingsName_Label.Text = "Name: ";
                TextBox radialSettingstName_TextBox = new TextBox()
                {
                    Location = new Point(radialSettingsName_Label.Right + 5, 0),
                    Width = labelWidth,
                    Parent = RadialSettingsDetailPanel,
                    Text = $"{userDefinedRadialSettingsName.Name}"
                };
                radialSettingstName_TextBox.TextChanged += delegate {
                    userDefinedRadialSettingsName.NameSetting.Value = radialSettingstName_TextBox.Text;
                    BuildRadialSettingsListPanel();
                };

                Label settingDefaultMountKeybind_Label = new Label()
                {
                    Location = new Point(0, radialSettingsName_Label.Bottom + 6),
                    Width = labelWidth,
                    AutoSizeHeight = false,
                    WrapText = false,
                    Parent = RadialSettingsDetailPanel,
                    Text = "Key binding: ",
                    BasicTooltipText = "The keybind to display this user-defined radial."
                };
                KeybindingAssigner settingDefaultMount_Keybind = new KeybindingAssigner(userDefinedRadialSettingsName.Keybind.Value)
                {
                    NameWidth = 0,
                    Size = new Point(labelWidth, 20),
                    Parent = RadialSettingsDetailPanel,
                    Location = new Point(settingDefaultMountKeybind_Label.Right + 4, settingDefaultMountKeybind_Label.Top - 1),
                };
                curY = settingDefaultMountKeybind_Label.Bottom;
            }

            Label settingDefaultThing_Label = new Label()
            {
                Location = new Point(0, curY + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsDetailPanel,
                Text = "Default: ",
                BasicTooltipText = "The default action using in this radial, see documentation (\"Default action\") for more info."
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
                BasicTooltipText = "Which action is displayed in the middle of the radial."
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
                currentRadialSettings.CenterThingBehavior.Value = (CenterBehavior)Enum.Parse(typeof(CenterBehavior), settingRadialCenterMountBehavior_Select.SelectedItem);
            };
            Label settingRadialRemoveCenterMount_Label = new Label()
            {
                Location = new Point(0, settingRadialCenterMountBehavior_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsDetailPanel,
                Text = "Remove center from radial: ",
                BasicTooltipText = "Removes the center action from the radial ring when selected."
            };
            Checkbox settingRadialRemoveCenterMount_Checkbox = new Checkbox()
            {
                Size = new Point(labelWidth, 20),
                Parent = RadialSettingsDetailPanel,
                Checked = currentRadialSettings.RemoveCenterThing.Value,
                Location = new Point(settingRadialRemoveCenterMount_Label.Right + 5, settingRadialRemoveCenterMount_Label.Top - 1),
            };
            settingRadialRemoveCenterMount_Checkbox.CheckedChanged += delegate {
                currentRadialSettings.RemoveCenterThing.Value = settingRadialRemoveCenterMount_Checkbox.Checked;
            };

            Label radialSettingsIsEnabled_Label = new Label()
            {
                Location = new Point(0, settingRadialRemoveCenterMount_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = RadialSettingsDetailPanel,
                Text = "Enabled",
                BasicTooltipText = "Disabled radials are not taken into account for the evaluation and are thus not displayed."
            };

            var IsDefault = false;
            if (currentRadialSettings is ContextualRadialThingSettings contextualRadialSettings)
            {
                IsDefault = contextualRadialSettings.IsDefault;
            }

            Checkbox radialSettingsIsEnabled_Checkbox = new Checkbox()
            {
                Size = new Point(20, 20),
                Parent = RadialSettingsDetailPanel,
                Checked = currentRadialSettings.IsEnabled.Value,
                Location = new Point(radialSettingsIsEnabled_Label.Right + 5, radialSettingsIsEnabled_Label.Top - 1),
                Enabled = !IsDefault,
                BasicTooltipText = IsDefault ? "Cannot disable Default" : null
            };
            radialSettingsIsEnabled_Checkbox.CheckedChanged += delegate {
                currentRadialSettings.IsEnabled.Value = radialSettingsIsEnabled_Checkbox.Checked;
                BuildRadialSettingsListPanel();
            };

            if (currentRadialSettings is ContextualRadialThingSettings contextualRadialSettingsAtBottom)
            {
                Label radialSettingsApplyInstantlyIfSingle_Label = new Label()
                {
                    Location = new Point(0, radialSettingsIsEnabled_Label.Bottom + 6),
                    Width = labelWidth,
                    AutoSizeHeight = false,
                    WrapText = false,
                    Parent = RadialSettingsDetailPanel,
                    Text = "Apply instantly if single",
                    BasicTooltipText = "When there is only 1 action configured in a radial context and this option is checked we do not display the radial, but we perform the action immediately instead."
                };
                Checkbox radialSettingsApplyInstantlyIfSingle_Checkbox = new Checkbox()
                {
                    Size = new Point(20, 20),
                    Parent = RadialSettingsDetailPanel,
                    Checked = contextualRadialSettingsAtBottom.ApplyInstantlyIfSingle.Value,
                    Location = new Point(radialSettingsApplyInstantlyIfSingle_Label.Right + 5, radialSettingsApplyInstantlyIfSingle_Label.Top - 1),
                };
                radialSettingsApplyInstantlyIfSingle_Checkbox.CheckedChanged += delegate
                {
                    contextualRadialSettingsAtBottom.ApplyInstantlyIfSingle.Value = radialSettingsApplyInstantlyIfSingle_Checkbox.Checked;
                };
                contextualRadialSettingsAtBottom.ApplyInstantlyIfSingle.SettingChanged += delegate
                {
                    BuildRadialSettingsDetailPanel();
                };


                Label settingApplyInstantlyOnTap_Label = new Label()
                {
                    Location = new Point(0, radialSettingsApplyInstantlyIfSingle_Label.Bottom + 6),
                    Width = labelWidth,
                    AutoSizeHeight = false,
                    WrapText = false,
                    Parent = RadialSettingsDetailPanel,
                    Text = "Apply instantly on tap: ",
                    TextColor = Color.White,
                    BasicTooltipText = "The configured action will be hidden from the radial. When the module keybind is tapped we do not display the radial, but we perform this action immediately instead."
                };
                Dropdown settingApplyInstantlyOnTap_Select = new Dropdown()
                {
                    Location = new Point(settingApplyInstantlyOnTap_Label.Right + 5, settingApplyInstantlyOnTap_Label.Top - 4),
                    Width = labelWidth,
                    Parent = RadialSettingsDetailPanel,
                };
                if (Module._settingTapThresholdInMilliseconds.Value == 0)
                {
                    settingApplyInstantlyOnTap_Label.TextColor = Color.DarkGray;
                    settingApplyInstantlyOnTap_Label.BasicTooltipText = "Disabled since tap threshold is set to 0";
                    settingApplyInstantlyOnTap_Select.Enabled = false;
                }
                settingApplyInstantlyOnTap_Select.Items.Add("Disabled");
                var thingNamesApplyInstantlyOnTap = contextualRadialSettingsAtBottom.Things.Select(m => m.Name);
                foreach (string i in thingNamesApplyInstantlyOnTap)
                {
                    settingApplyInstantlyOnTap_Select.Items.Add(i.ToString());
                }
                settingApplyInstantlyOnTap_Select.SelectedItem = thingNames.Any(m => m == contextualRadialSettingsAtBottom.ApplyInstantlyOnTap.Value) ? contextualRadialSettingsAtBottom.ApplyInstantlyOnTap.Value : "Disabled";
                settingApplyInstantlyOnTap_Select.ValueChanged += delegate {
                    contextualRadialSettingsAtBottom.ApplyInstantlyOnTap.Value = settingApplyInstantlyOnTap_Select.SelectedItem;
                };






                Label radialSettingsUnconditionallyDoAction_Label = new Label()
                {
                    Location = new Point(0, settingApplyInstantlyOnTap_Label.Bottom + 6),
                    Width = labelWidth,
                    AutoSizeHeight = false,
                    WrapText = false,
                    Parent = RadialSettingsDetailPanel,
                    Text = "Unconditionally Do Action",
                    BasicTooltipText = "Used to disable \"out of combat queuing\", \"LastUsed\" and \"mount automatically\" functionality. Only useful when the user has configured a mount action (e.g.: Raptor) instead of the dismount action to dismount in the IsPlayerMounted contextual radial settings."
                };
                Checkbox radialSettingsUnconditionallyDoAction_Checkbox = new Checkbox()
                {
                    Size = new Point(20, 20),
                    Parent = RadialSettingsDetailPanel,
                    Checked = contextualRadialSettingsAtBottom.UnconditionallyDoAction.Value,
                    Location = new Point(radialSettingsUnconditionallyDoAction_Label.Right + 5, radialSettingsUnconditionallyDoAction_Label.Top - 1),
                };
                radialSettingsUnconditionallyDoAction_Checkbox.CheckedChanged += delegate {
                    contextualRadialSettingsAtBottom.UnconditionallyDoAction.Value = radialSettingsUnconditionallyDoAction_Checkbox.Checked;
                };
            }

            ThingSettingsView thingSettingsView = new ThingSettingsView(currentRadialSettings)
            {
                Location = new Point(500, 0),
                Parent = RadialSettingsDetailPanel
            };
        }

        private void CurrentRadialSettings_RadialSettingsUpdated(object sender, SettingsUpdatedEvent e)
        {
            BuildRadialSettingsDetailPanel();
        }
    }
}
