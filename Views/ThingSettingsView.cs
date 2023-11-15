using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Mounts;
using Manlaan.Mounts.Things;
using System.Reflection;
using Blish_HUD;

namespace Manlaan.Mounts.Views
{
    public class ThingSettingsView : Container
    {
        private int orderWidth = 80;

        private Panel panel;

        protected ThingsSettings CurrentThingSettings;

        public ThingSettingsView(ThingsSettings currentThingSettings)
        {
            CurrentThingSettings = currentThingSettings;
            Width = 600;
            Height = 600;
            panel = new Panel
            {
                CanScroll = false,
                Width = 600,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = this
            };
            BuildThingSettingsPanel();
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle drawBounds, Rectangle scissor)
        {
            base.Draw(spriteBatch, drawBounds, scissor);
        }


        protected void BuildThingSettingsPanel()
        {
            panel.ClearChildren();
           
            int curY = 0;
            var thingsNotYetInSettings = Module._things.Where(t => t.IsAvailable).Where(t => !CurrentThingSettings.Things.Any(tt => tt.Equals(t))).ToList();
            if (thingsNotYetInSettings.Any())
            {
                Dropdown addThing_Select = new Dropdown()
                {
                    Location = new Point(0, 0),
                    Width = orderWidth,
                    Parent = panel,
                    BasicTooltipText = "Only things that have a keybind in the General Settings tab will show up here."
                };
                thingsNotYetInSettings.ForEach(t => addThing_Select.Items.Add(t.DisplayName));
                addThing_Select.SelectedItem = thingsNotYetInSettings.FirstOrDefault()?.DisplayName;
                var addThing_Button = new StandardButton
                {
                    Parent = panel,
                    Location = new Point(addThing_Select.Right, addThing_Select.Top),
                    Text = Strings.Add
                };
                addThing_Button.Click += (args, sender) => {
                    CurrentThingSettings.AddThing(Module._things.Single(t => t.DisplayName == addThing_Select.SelectedItem));
                    BuildThingSettingsPanel();
                };
                curY = addThing_Select.Bottom;
            }

            if (!CurrentThingSettings.Things.Any())
            {
                Label thingInSettings_Label = new Label()
                {
                    Location = new Point(0, curY),
                    AutoSizeWidth = true,
                    AutoSizeHeight = false,
                    Parent = panel,
                    TextColor = Color.Red,
                    Font = GameService.Content.DefaultFont18,
                    Text = "You need to configure something or this context is pointless.",
                };
            }

            foreach (var thingItemAndIndex in CurrentThingSettings.Things.Select((value, i) => new { i, value }))
            {
                var thing = thingItemAndIndex.value;
                int index = thingItemAndIndex.i;
                var isAvailable = thing.IsAvailable;

                var curX = index%2 == 0 ? 0 : 300;
                curY += index % 2 == 0 ? 30 : 0;
                Label thingInSettings_Label = new Label()
                {
                    Location = new Point(curX, curY),
                    AutoSizeWidth = true,
                    AutoSizeHeight = false,
                    Parent = panel,
                    TextColor = isAvailable ? Color.White : Color.Red,
                    BasicTooltipText = isAvailable ? null : "No keybind is set in the General Settings tab",
                    Text = $"{index+1}. {thing.Name}",
                };
                var deleteThing_Button = new StandardButton
                {
                    Parent = panel,
                    Location = new Point(thingInSettings_Label.Right, thingInSettings_Label.Top),
                    Text = Strings.Delete
                };
                deleteThing_Button.Click += (args, sender) =>
                {
                    CurrentThingSettings.RemoveThing(thing);
                    BuildThingSettingsPanel();
                };
            }
        }
    }
}
