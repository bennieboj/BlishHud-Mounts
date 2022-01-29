using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Blish_HUD.Settings.UI.Views;
using Blish_HUD.Graphics.UI;
using System;

namespace Manlaan.Mounts.Views
{
    class SettingsView : View
    {
        protected override void Build(Container buildPanel) {
            int labelWidth = 100;
            int orderWidth = 80;
            int bindingWidth = 150;

            Panel mountsLeftPanel = new Panel() {
                CanScroll = false,
                Parent = buildPanel,
                HeightSizingMode = SizingMode.AutoSize,
                Width = 330,
                Location = new Point(10, 10),
            };
            Panel otherPanel = new Panel() {
                CanScroll = false,
                Parent = buildPanel,
                HeightSizingMode = SizingMode.AutoSize,
                Width = 330,
                Location = new Point(mountsLeftPanel.Right + 20, 10),
            };
            Panel manualPanel = new Panel() {
                CanScroll = false,
                Parent = buildPanel,
                HeightSizingMode = SizingMode.AutoSize,
                Width = 330,
                Location = new Point(mountsLeftPanel.Right + 20, 63),
            };
            if (Module._settingDisplay.Value.Equals("Transparent Manual") || Module._settingDisplay.Value.Equals("Solid Manual") || Module._settingDisplay.Value.Equals("Solid Manual Text")) 
                manualPanel.Show();
            else 
                manualPanel.Hide();

            #region Mounts Panel
            Label settingOrderLeft_Label = new Label() {
                Location = new Point(labelWidth + 5, 2),
                Width = orderWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = mountsLeftPanel,
                Text = "Order",
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            Label settingBindingLeft_Label = new Label() {
                Location = new Point(settingOrderLeft_Label.Right + 5, settingOrderLeft_Label.Top),
                Width = bindingWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = mountsLeftPanel,
                Text = "Key Binding",
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            Label settingRaptor_Label = new Label() {
                Location = new Point(0, settingOrderLeft_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = mountsLeftPanel,
                Text = "Raptor: ",
            };
            Dropdown settingRaptor_Select = new Dropdown() {
                Location = new Point(settingRaptor_Label.Right + 5, settingRaptor_Label.Top - 4),
                Width = orderWidth,
                Parent = mountsLeftPanel,
            };
            foreach (int i in Module._mountOrder) {
                if (i == 0)
                    settingRaptor_Select.Items.Add("Disabled");
                else
                    settingRaptor_Select.Items.Add(i.ToString());
            }
            settingRaptor_Select.SelectedItem = Module._settingRaptorOrder.Value == 0 ? "Disabled" : Module._settingRaptorOrder.Value.ToString();
            settingRaptor_Select.ValueChanged += delegate {
                if (settingRaptor_Select.SelectedItem.Equals("Disabled"))
                    Module._settingRaptorOrder.Value = 0;
                else
                    Module._settingRaptorOrder.Value = int.Parse(settingRaptor_Select.SelectedItem);
            };
            KeybindingAssigner settingRaptor_Keybind = new KeybindingAssigner() {
                NameWidth = 0,
                Size = new Point(bindingWidth, 20),
                Parent = mountsLeftPanel,
                KeyBinding = Module._settingRaptorBinding.Value,
                Location = new Point(settingRaptor_Select.Right + 5, settingRaptor_Label.Top - 1),
            };

            Label settingSpringer_Label = new Label() {
                Location = new Point(0, settingRaptor_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = mountsLeftPanel,
                Text = "Springer: ",
            };
            Dropdown settingSpringer_Select = new Dropdown() {
                Location = new Point(settingSpringer_Label.Right + 5, settingSpringer_Label.Top - 4),
                Width = orderWidth,
                Parent = mountsLeftPanel,
            };
            foreach (int i in Module._mountOrder) {
                if (i == 0)
                    settingSpringer_Select.Items.Add("Disabled");
                else
                    settingSpringer_Select.Items.Add(i.ToString());
            }
            settingSpringer_Select.SelectedItem = Module._settingSpringerOrder.Value == 0 ? "Disabled" : Module._settingSpringerOrder.Value.ToString();
            settingSpringer_Select.ValueChanged += delegate {
                if (settingSpringer_Select.SelectedItem.Equals("Disabled"))
                    Module._settingSpringerOrder.Value = 0;
                else
                    Module._settingSpringerOrder.Value = int.Parse(settingSpringer_Select.SelectedItem);
            };
            KeybindingAssigner settingSpringer_Keybind = new KeybindingAssigner() {
                NameWidth = 0,
                Size = new Point(bindingWidth, 20),
                Parent = mountsLeftPanel,
                KeyBinding = Module._settingSpringerBinding.Value,
                Location = new Point(settingSpringer_Select.Right + 5, settingSpringer_Label.Top - 1),
            };

            Label settingSkimmer_Label = new Label() {
                Location = new Point(0, settingSpringer_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = mountsLeftPanel,
                Text = "Skimmer: ",
            };
            Dropdown settingSkimmer_Select = new Dropdown() {
                Location = new Point(settingSkimmer_Label.Right + 5, settingSkimmer_Label.Top - 4),
                Width = orderWidth,
                Parent = mountsLeftPanel,
            };
            foreach (int i in Module._mountOrder) {
                if (i == 0)
                    settingSkimmer_Select.Items.Add("Disabled");
                else
                    settingSkimmer_Select.Items.Add(i.ToString());
            }
            settingSkimmer_Select.SelectedItem = Module._settingSkimmerOrder.Value == 0 ? "Disabled" : Module._settingSkimmerOrder.Value.ToString();
            settingSkimmer_Select.ValueChanged += delegate {
                if (settingSkimmer_Select.SelectedItem.Equals("Disabled"))
                    Module._settingSkimmerOrder.Value = 0;
                else
                    Module._settingSkimmerOrder.Value = int.Parse(settingSkimmer_Select.SelectedItem);
            };
            KeybindingAssigner settingSkimmer_Keybind = new KeybindingAssigner() {
                NameWidth = 0,
                Size = new Point(bindingWidth, 20),
                Parent = mountsLeftPanel,
                KeyBinding = Module._settingSkimmerBinding.Value,
                Location = new Point(settingSkimmer_Select.Right + 5, settingSkimmer_Label.Top - 1),
            };

            Label settingJackal_Label = new Label() {
                Location = new Point(0, settingSkimmer_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = mountsLeftPanel,
                Text = "Jackal: ",
            };
            Dropdown settingJackal_Select = new Dropdown() {
                Location = new Point(settingJackal_Label.Right + 5, settingJackal_Label.Top - 4),
                Width = orderWidth,
                Parent = mountsLeftPanel,
            };
            foreach (int i in Module._mountOrder) {
                if (i == 0)
                    settingJackal_Select.Items.Add("Disabled");
                else
                    settingJackal_Select.Items.Add(i.ToString());
            }
            settingJackal_Select.SelectedItem = Module._settingJackalOrder.Value == 0 ? "Disabled" : Module._settingJackalOrder.Value.ToString();
            settingJackal_Select.ValueChanged += delegate {
                if (settingJackal_Select.SelectedItem.Equals("Disabled"))
                    Module._settingJackalOrder.Value = 0;
                else
                    Module._settingJackalOrder.Value = int.Parse(settingJackal_Select.SelectedItem);
            };
            KeybindingAssigner settingJackal_Keybind = new KeybindingAssigner() {
                NameWidth = 0,
                Size = new Point(bindingWidth, 20),
                Parent = mountsLeftPanel,
                KeyBinding = Module._settingJackalBinding.Value,
                Location = new Point(settingJackal_Select.Right + 5, settingJackal_Label.Top - 1),
            };

            Label settingGriffon_Label = new Label() {
                Location = new Point(0, settingJackal_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = mountsLeftPanel,
                Text = "Griffon: ",
            };
            Dropdown settingGriffon_Select = new Dropdown() {
                Location = new Point(settingGriffon_Label.Right + 5, settingGriffon_Label.Top - 4),
                Width = orderWidth,
                Parent = mountsLeftPanel,
            };
            foreach (int i in Module._mountOrder) {
                if (i == 0)
                    settingGriffon_Select.Items.Add("Disabled");
                else
                    settingGriffon_Select.Items.Add(i.ToString());
            }
            settingGriffon_Select.SelectedItem = Module._settingGriffonOrder.Value == 0 ? "Disabled" : Module._settingGriffonOrder.Value.ToString();
            settingGriffon_Select.ValueChanged += delegate {
                if (settingGriffon_Select.SelectedItem.Equals("Disabled"))
                    Module._settingGriffonOrder.Value = 0;
                else
                    Module._settingGriffonOrder.Value = int.Parse(settingGriffon_Select.SelectedItem);
            };
            KeybindingAssigner settingGriffon_Keybind = new KeybindingAssigner() {
                NameWidth = 0,
                Size = new Point(bindingWidth, 20),
                Parent = mountsLeftPanel,
                KeyBinding = Module._settingGriffonBinding.Value,
                Location = new Point(settingGriffon_Select.Right + 5, settingGriffon_Label.Top - 1),
            };

            Label settingRoller_Label = new Label() {
                Location = new Point(0, settingGriffon_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = mountsLeftPanel,
                Text = "Roller: ",
            };
            Dropdown settingRoller_Select = new Dropdown() {
                Location = new Point(settingRoller_Label.Right + 5, settingRoller_Label.Top - 4),
                Width = orderWidth,
                Parent = mountsLeftPanel,
            };
            foreach (int i in Module._mountOrder) {
                if (i == 0)
                    settingRoller_Select.Items.Add("Disabled");
                else
                    settingRoller_Select.Items.Add(i.ToString());
            }
            settingRoller_Select.SelectedItem = Module._settingRollerOrder.Value == 0 ? "Disabled" : Module._settingRollerOrder.Value.ToString();
            settingRoller_Select.ValueChanged += delegate {
                if (settingRoller_Select.SelectedItem.Equals("Disabled"))
                    Module._settingRollerOrder.Value = 0;
                else
                    Module._settingRollerOrder.Value = int.Parse(settingRoller_Select.SelectedItem);
            };
            KeybindingAssigner settingRoller_Keybind = new KeybindingAssigner() {
                NameWidth = 0,
                Size = new Point(bindingWidth, 20),
                Parent = mountsLeftPanel,
                KeyBinding = Module._settingRollerBinding.Value,
                Location = new Point(settingRoller_Select.Right + 5, settingRoller_Label.Top - 1),
            };

            Label settingWarclaw_Label = new Label() {
                Location = new Point(0, settingRoller_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = mountsLeftPanel,
                Text = "Warclaw: ",
            };
            Dropdown settingWarclaw_Select = new Dropdown() {
                Location = new Point(settingWarclaw_Label.Right + 5, settingWarclaw_Label.Top - 4),
                Width = orderWidth,
                Parent = mountsLeftPanel,
            };
            foreach (int i in Module._mountOrder) {
                if (i == 0)
                    settingWarclaw_Select.Items.Add("Disabled");
                else
                    settingWarclaw_Select.Items.Add(i.ToString());
            }
            settingWarclaw_Select.SelectedItem = Module._settingWarclawOrder.Value == 0 ? "Disabled" : Module._settingWarclawOrder.Value.ToString();
            settingWarclaw_Select.ValueChanged += delegate {
                if (settingWarclaw_Select.SelectedItem.Equals("Disabled"))
                    Module._settingWarclawOrder.Value = 0;
                else
                    Module._settingWarclawOrder.Value = int.Parse(settingWarclaw_Select.SelectedItem);
            };
            KeybindingAssigner settingWarclaw_Keybind = new KeybindingAssigner() {
                NameWidth = 0,
                Size = new Point(bindingWidth, 20),
                Parent = mountsLeftPanel,
                KeyBinding = Module._settingWarclawBinding.Value,
                Location = new Point(settingWarclaw_Select.Right + 5, settingWarclaw_Label.Top - 1),
            };

            Label settingSkyscale_Label = new Label()
            {
                Location = new Point(0, settingWarclaw_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = mountsLeftPanel,
                Text = "Skyscale: ",
            };
            Dropdown settingSkyscale_Select = new Dropdown()
            {
                Location = new Point(settingSkyscale_Label.Right + 5, settingSkyscale_Label.Top - 4),
                Width = orderWidth,
                Parent = mountsLeftPanel,
            };
            foreach (int i in Module._mountOrder)
            {
                if (i == 0)
                    settingSkyscale_Select.Items.Add("Disabled");
                else
                    settingSkyscale_Select.Items.Add(i.ToString());
            }
            settingSkyscale_Select.SelectedItem = Module._settingSkyscaleOrder.Value == 0 ? "Disabled" : Module._settingSkyscaleOrder.Value.ToString();
            settingSkyscale_Select.ValueChanged += delegate {
                if (settingSkyscale_Select.SelectedItem.Equals("Disabled"))
                    Module._settingSkyscaleOrder.Value = 0;
                else
                    Module._settingSkyscaleOrder.Value = int.Parse(settingSkyscale_Select.SelectedItem);
            };
            KeybindingAssigner settingSkyscale_Keybind = new KeybindingAssigner()
            {
                NameWidth = 0,
                Size = new Point(bindingWidth, 20),
                Parent = mountsLeftPanel,
                KeyBinding = Module._settingSkyscaleBinding.Value,
                Location = new Point(settingSkyscale_Select.Right + 5, settingSkyscale_Label.Top - 1),
            };

            Label settingDefaultMount_Label = new Label()
            {
                Location = new Point(0, settingSkyscale_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = mountsLeftPanel,
                Text = "Default mount: ",
            };
            Dropdown settingDefaultMount_Select = new Dropdown()
            {
                Location = new Point(settingDefaultMount_Label.Right + 5, settingDefaultMount_Label.Top - 4),
                Width = orderWidth,
                Parent = mountsLeftPanel,
            };
            foreach (string i in Module._defaultMountChoices)
            {
                settingDefaultMount_Select.Items.Add(i.ToString());
            }
            settingDefaultMount_Select.SelectedItem = Array.Exists(Module._defaultMountChoices, e => e == Module._settingDefaultMountChoice.Value) ? Module._settingDefaultMountChoice.Value : "Disabled";
            settingDefaultMount_Select.ValueChanged += delegate {
                Module._settingDefaultMountChoice.Value = settingDefaultMount_Select.SelectedItem;
            };
            KeybindingAssigner settingDefaultMount_Keybind = new KeybindingAssigner()
            {
                NameWidth = 0,
                Size = new Point(bindingWidth, 20),
                Parent = mountsLeftPanel,
                KeyBinding = Module._settingDefaultMountBinding.Value,
                Location = new Point(settingDefaultMount_Select.Right + 5, settingDefaultMount_Label.Top - 1),
            };




            #endregion

            #region OtherPanel
            Label settingDisplay_Label = new Label() {
                Location = new Point(0, 4),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = otherPanel,
                Text = "Display: ",
            };
            Dropdown settingDisplay_Select = new Dropdown() {
                Location = new Point(settingDisplay_Label.Right + 5, settingDisplay_Label.Top - 4),
                Width = 160,
                Parent = otherPanel,
            };
            foreach (string s in Module._mountDisplay) {
                settingDisplay_Select.Items.Add(s);
            }
            settingDisplay_Select.SelectedItem = Module._settingDisplay.Value;
            settingDisplay_Select.ValueChanged += delegate {
                Module._settingDisplay.Value = settingDisplay_Select.SelectedItem;
                if (settingDisplay_Select.SelectedItem.Equals("Transparent Manual") || settingDisplay_Select.SelectedItem.Equals("Solid Manual") || settingDisplay_Select.SelectedItem.Equals("Solid Manual Text"))
                    manualPanel.Show();
                else
                    manualPanel.Hide();
            };
            #endregion

            #region manual Panel
            Label settingManual_Label = new Label() {
                Location = new Point(0, 2),
                Width = manualPanel.Width,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = manualPanel,
                Text = "Manual Settings",
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Label settingManualOrientation_Label = new Label() {
                Location = new Point(0, settingManual_Label.Bottom + 6),
                Width = 75,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = manualPanel,
                Text = "Orientation: ",
            };
            Dropdown settingManualOrientation_Select = new Dropdown() {
                Location = new Point(settingManualOrientation_Label.Right + 5, settingManualOrientation_Label.Top - 4),
                Width = 100,
                Parent = manualPanel,
            };
            foreach (string s in Module._mountOrientation) {
                settingManualOrientation_Select.Items.Add(s);
            }
            settingManualOrientation_Select.SelectedItem = Module._settingOrientation.Value;
            settingManualOrientation_Select.ValueChanged += delegate {
                Module._settingOrientation.Value = settingManualOrientation_Select.SelectedItem;
            };

            Label settingManualWidth_Label = new Label() {
                Location = new Point(0, settingManualOrientation_Label.Bottom + 6),
                Width = 75,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = manualPanel,
                Text = "Icon Width: ",
            };
            TrackBar settingImgWidth_Slider = new TrackBar() {
                Location = new Point(settingManualWidth_Label.Right + 5, settingManualWidth_Label.Top),
                Width = 220,
                MaxValue = 200,
                MinValue = 0,
                Value = Module._settingImgWidth.Value,
                Parent = manualPanel,
            };
            settingImgWidth_Slider.ValueChanged += delegate { Module._settingImgWidth.Value = (int)settingImgWidth_Slider.Value; };

            Label settingManualOpacity_Label = new Label() {
                Location = new Point(0, settingManualWidth_Label.Bottom + 6),
                Width = 75,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = manualPanel,
                Text = "Opacity: ",
            };
            TrackBar settingOpacity_Slider = new TrackBar() {
                Location = new Point(settingManualOpacity_Label.Right + 5, settingManualOpacity_Label.Top),
                Width = 220,
                MaxValue = 100,
                MinValue = 0,
                Value = Module._settingOpacity.Value * 100,
                Parent = manualPanel,
            };
            settingOpacity_Slider.ValueChanged += delegate { Module._settingOpacity.Value = settingOpacity_Slider.Value / 100; };

            IView settingClockDrag_View = SettingView.FromType(Module._settingDrag, buildPanel.Width);
            ViewContainer settingClockDrag_Container = new ViewContainer() {
                WidthSizingMode = SizingMode.Fill,
                Location = new Point(0, settingManualOpacity_Label.Bottom + 3),
                Parent = manualPanel
            };
            settingClockDrag_Container.Show(settingClockDrag_View);
            #endregion
        }
    }
}
