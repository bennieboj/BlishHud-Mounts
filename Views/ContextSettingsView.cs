using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Blish_HUD;
using System.Diagnostics;
using System.Linq;
using Blish_HUD.Graphics.UI;

namespace Manlaan.Mounts.Views
{
    class ContextSettingsView : View
    {
        private int labelWidth = 150;
        private int bindingWidth = 170;

        private Panel contextListPanel;
        Panel contextDetailPanel;

        private ThingActivationContext currentContext;
        ThingSettingsView thingSettingsView;

        public ContextSettingsView()
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

            thingSettingsView = new ThingSettingsView(currentContext)
            {
                Location = new Point(0, contextApplyInstantlyIfSingle_Label.Bottom),
                Parent = contextDetailPanel,
                Width = 500,
                Height = 500
            };
            thingSettingsView.OnThingsUpdated += OnThingsUpdated;
        }

        void OnThingsUpdated(object sender, ThingsUpdatedEventArgs e)
        {
            currentContext.ApplyInstantlyIfSingleSetting.Value = e.NewCount == 1;
            BuildContextDetailPanel();
        }

}
}
