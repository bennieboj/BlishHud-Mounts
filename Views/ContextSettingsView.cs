using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Blish_HUD.Graphics.UI;
using Blish_HUD;
using System.Diagnostics;
using System.Linq;
using Manlaan.Mounts.Things;
using Blish_HUD.Common.UI.Views;

namespace Manlaan.Mounts.Views
{
    class ContextSettingsView : View
    {
        private const string NoValueSelected = "Please select a value";

        private int labelWidth = 150;
        private int orderWidth = 80;
        private int bindingWidth = 170;

        private Panel contextListPanel;
        Panel contextDetailPanel;
        private ThingActivationContext currentContext;

        public ContextSettingsView(TextureCache textureCache)
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

            contextListPanel = CreateDefaultPanel(buildPanel, new Point(panelPadding, labelExplanation.Bottom + panelPadding), 600);
            CreateContextListPanel();

            currentContext = Module.OrderedContexts().First();
            contextDetailPanel = CreateDefaultPanel(buildPanel, new Point(10, 300));
            BuildContextDetailPanel();


        }

        private void CreateContextListPanel()
        {
            Label orderHeading_Label = new Label()
            {
                Location = new Point(0, 10),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = contextListPanel,
                Text = "Context name",
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            Label nameHeader_label = new Label()
            {
                Location = new Point(orderHeading_Label.Right + 5, orderHeading_Label.Top),
                Width = bindingWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = contextListPanel,
                Text = "Evaluation Order",
                HorizontalAlignment = HorizontalAlignment.Center,
            };


            int curY = orderHeading_Label.Bottom + 6;

            foreach (var context in Module.OrderedContexts())
            {
                Label name_Label = new Label()
                {
                    Location = new Point(0, curY + 6),
                    Width = labelWidth,
                    AutoSizeHeight = false,
                    WrapText = false,
                    Parent = contextListPanel,
                    Text = $"{context.Name}: ",
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
                Label order_Label = new Label()
                {
                    Location = new Point(name_Label.Right + 5, name_Label.Top - 4),
                    Width = labelWidth,
                    AutoSizeHeight = false,
                    WrapText = false,
                    Parent = contextListPanel,
                    Text = $"{context.Order}",
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                var editContextButton = new StandardButton
                {
                    Parent = contextListPanel,
                    Location = new Point(order_Label.Right, order_Label.Top),
                    Text = Strings.Edit
                };
                editContextButton.Click += (args, sender) => {
                    currentContext = context;
                    BuildContextDetailPanel();
                };

                curY = name_Label.Bottom;
            }            
        }

        private void BuildContextDetailPanel()
        {
            contextDetailPanel.ClearChildren();
           
            Label contextName_Label = new Label()
            {
                Location = new Point(0, 0),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = contextDetailPanel,
                Text = $"{currentContext.Name}"
            };

            Label contextIsEnabled_Label = new Label()
            {
                Location = new Point(0, contextName_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = contextDetailPanel,
                Text = "Enabled"
            };
            Checkbox contextIsEnabled_Checkbox = new Checkbox()
            {
                Size = new Point(20, 20),
                Parent = contextDetailPanel,
                Checked = currentContext.IsEnabledSetting.Value,
                Location = new Point(contextIsEnabled_Label.Right + 5, contextIsEnabled_Label.Top - 1),
            };
            contextIsEnabled_Checkbox.CheckedChanged += delegate {
                currentContext.IsEnabledSetting.Value = contextIsEnabled_Checkbox.Checked;
            };

            Label contextApplyInstantlyIfSingle_Label = new Label()
            {
                Location = new Point(0, contextIsEnabled_Label.Bottom + 6),
                Width = labelWidth,
                AutoSizeHeight = false,
                WrapText = false,
                Parent = contextDetailPanel,
                Text = "ApplyInstantlyIfSingle"
            };
            Checkbox contextApplyInstantlyIfSingle_Checkbox = new Checkbox()
            {
                Size = new Point(20, 20),
                Parent = contextDetailPanel,
                Checked = currentContext.ApplyInstantlyIfSingleSetting.Value,
                Location = new Point(contextApplyInstantlyIfSingle_Label.Right + 5, contextApplyInstantlyIfSingle_Label.Top - 1),
            };
            contextApplyInstantlyIfSingle_Checkbox.CheckedChanged += delegate {
                currentContext.ApplyInstantlyIfSingleSetting.Value = contextApplyInstantlyIfSingle_Checkbox.Checked;
            };

            int curY = contextApplyInstantlyIfSingle_Label.Bottom;
            var thingsNotYetInContext = Module._things.Where(t => !currentContext.Things.Any(tt => tt.Equals(t))).ToList();
            if (thingsNotYetInContext.Any())
            {
                Dropdown addThing_Select = new Dropdown()
                {
                    Location = new Point(contextApplyInstantlyIfSingle_Label.Left, contextApplyInstantlyIfSingle_Label.Bottom + 6),
                    Width = orderWidth,
                    Parent = contextDetailPanel,
                };
                thingsNotYetInContext.ForEach(t => addThing_Select.Items.Add(t.DisplayName));
                addThing_Select.SelectedItem = thingsNotYetInContext.FirstOrDefault()?.DisplayName;
                var addThing_Button = new StandardButton
                {
                    Parent = contextDetailPanel,
                    Location = new Point(addThing_Select.Right, addThing_Select.Top),
                    Text = Strings.Add
                };
                addThing_Button.Click += (args, sender) => {
                    currentContext.AddThing(Module._things.Single(t => t.DisplayName == addThing_Select.SelectedItem));
                    BuildContextDetailPanel();
                };
                curY = addThing_Select.Bottom;
            }

            int curX = 0;
            foreach (var thing in currentContext.Things)
            {
                Label thingInContext_Label = new Label()
                {
                    Location = new Point(curX, curY),
                    AutoSizeWidth = true,
                    AutoSizeHeight = false,
                    Parent = contextDetailPanel,
                    TextColor = thing.IsAvailable ? Color.Black : Color.Red,
                    Tooltip = thing.IsAvailable ? null : new Tooltip(new BasicTooltipView("NO KEYBIND SET")),
                    Text = $"{thing.Name}",
                };
                var deleteThing_Button = new StandardButton
                {
                    Parent = contextDetailPanel,
                    Location = new Point(thingInContext_Label.Right, thingInContext_Label.Top),
                    Text = Strings.Delete
                };
                deleteThing_Button.Click += (args, sender) =>
                {
                    currentContext.RemoveThing(thing);
                    BuildContextDetailPanel();
                };

                curX = deleteThing_Button.Right + 6;
                if (curX > 300)
                {
                    curY = deleteThing_Button.Bottom + 6;
                    curX = 0;
                }
            }
        }

    }
}
