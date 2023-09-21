using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System.Linq;
using Blish_HUD.Common.UI.Views;
using Microsoft.Xna.Framework.Graphics;
using System;
using Mounts;

namespace Manlaan.Mounts.Views
{
    public class ThingSettingsView : Container
    {
        private int orderWidth = 80;

        private Panel panel;
        public event EventHandler<ThingsUpdatedEventArgs> OnThingsUpdated;

        protected ThingsSettings CurrentThingSettings;

        public ThingSettingsView(ThingsSettings currentThingSettings)
        {
            CurrentThingSettings = currentThingSettings;
            panel = new Panel
            {
                CanScroll = false,
                Width = 420,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = this
            };
            BuildThingSettingsPanel();
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle drawBounds, Rectangle scissor)
        {
            base.Draw(spriteBatch, drawBounds, scissor);
        }

        private void HandleThingsUpdated() {
            var myevent = new ThingsUpdatedEventArgs();
            myevent.NewCount = CurrentThingSettings.Things.Count();

            if (OnThingsUpdated != null)
            {
                OnThingsUpdated(this, myevent);
            }

            BuildThingSettingsPanel();
        }


        protected void BuildThingSettingsPanel()
        {
            panel.ClearChildren();
           
            int curY = 0;
            var thingsNotYetInSettings = Module._things.Where(t => !CurrentThingSettings.Things.Any(tt => tt.Equals(t))).ToList();
            if (thingsNotYetInSettings.Any())
            {
                Dropdown addThing_Select = new Dropdown()
                {
                    Location = new Point(0, 6),
                    Width = orderWidth,
                    Parent = panel,
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
                    HandleThingsUpdated();
                };
                curY = addThing_Select.Bottom;
            }

            int curX = 0;
            foreach (var thing in CurrentThingSettings.Things)
            {
                Label thingInSettings_Label = new Label()
                {
                    Location = new Point(curX, curY),
                    AutoSizeWidth = true,
                    AutoSizeHeight = false,
                    Parent = panel,
                    TextColor = thing.IsAvailable ? Color.White : Color.Red,
                    BasicTooltipText = thing.IsAvailable ? null : "NO KEYBIND SET",
                    Text = $"{thing.Name}",
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
                    HandleThingsUpdated();
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
