using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Mounts.Settings;
using System.Linq;
using Manlaan.Mounts.Things;

namespace Manlaan.Mounts.Controls
{
    public class InfoPanel : Container
    {
        private readonly TextureCache _textureCache;
        private readonly Helper _helper;
        private bool _dragging;
        private Point _dragStart = Point.Zero;
        private Image infoPanelInfo;
        private Image rangedThing;
        private Image rangedIndicator;
        private Image outOfCombatQueuingThing;
        private Image outOfCombatQueueingIndicator;
        private Image laterActivationThing;
        private Image laterActivationIndicator;

        public InfoPanel(TextureCache textureCache, Helper helper)
        {
            _textureCache = textureCache;
            _helper = helper;
            _helper.RangedThingUpdated += RangedThingUpdated;
            _helper.StoredThingForLaterUpdated += StoredThingForLaterUpdated;
            foreach (var t in Module._things)
            {
                t.QueuedTimestampUpdated += QueuedTimestampUpdated;
            }

            Draw();
        }

        private void QueuedTimestampUpdated(object sender, ValueChangedEventArgs e)
        {
            if (outOfCombatQueuingThing != null && outOfCombatQueueingIndicator != null && Module._settingDisplayMountQueueing.Value)
            {
                var thing = _helper.GetQueuedThing();
                if (thing != null)
                {
                    SetThingOnImage(thing, outOfCombatQueuingThing);
                    outOfCombatQueuingThing.ZIndex = 2;
                    outOfCombatQueueingIndicator.Visible = true;
                    outOfCombatQueueingIndicator.ZIndex = 3;
                }
                else
                {
                    outOfCombatQueuingThing.Visible = false;
                    outOfCombatQueueingIndicator.Visible = false;

                }
            }
        }

        private void SetThingOnImage(Thing thing, Image image)
        {
            var img = _textureCache.GetThingImgFile(thing);
            image.Texture = img;
            image.Visible = true;
        }

        private void RangedThingUpdated(object sender, ValueChangedEventArgs<Thing> e)
        {
            var currentRangedThing = e.NewValue;
            if (currentRangedThing != null && Module._settingDisplayGroundTargetingAction.Value)
            {
                SetThingOnImage(currentRangedThing, rangedThing);
                rangedThing.ZIndex = 2;
                rangedIndicator.Visible = true;
                rangedIndicator.ZIndex = 3;
            }
            else
            {
                rangedThing.Visible = false;
                rangedIndicator.Visible = false;
            }
        }

        private void StoredThingForLaterUpdated(object sender, ValueChangedEventArgs<System.Collections.Generic.Dictionary<string, Thing>> e)
        {
            if (laterActivationThing != null && laterActivationIndicator != null && Module._settingDisplayLaterActivation.Value)
            {                
                e.NewValue.TryGetValue(GameService.Gw2Mumble.PlayerCharacter.Name, out Thing thing);
                if (thing != null)
                {
                    SetThingOnImage(thing, laterActivationThing);
                    laterActivationThing.ZIndex = 2;
                    laterActivationIndicator.Visible = true;
                    laterActivationIndicator.ZIndex = 3;
                }
                else
                {
                    laterActivationThing.Visible = false;
                    laterActivationIndicator.Visible = false;

                }
            }
        }


        public void Update()
        {
        }

        private void Draw()
        {
            Parent = GameService.Graphics.SpriteScreen;
            Location = Module._settingInfoPanelLocation.Value;
            Width = 64;
            Height = 64;

            Texture2D img = _textureCache.GetImgFile(TextureCache.ModuleLogoTextureName);
            infoPanelInfo = new Image
            {
                Parent = this,
                Texture = img,
                Size = new Point(Width, Height),
                Location = new Point(0, 0),
                Visible = Module._settingDragInfoPanel.Value,
                ZIndex = 1,
                BasicTooltipText = "Mounts & More info panel\nDisplays out of combat queueing, ground target action and tap action.\nSee settings and documentation for more info."
            };

            outOfCombatQueuingThing = new Image
            {
                Parent = this,
                Visible = false,
                Size = new Point(Width, Height),
                Location = new Point(0, 0),
                BasicTooltipText = "Action will be performed when out of combat"
            };

            Texture2D imgSword = _textureCache.GetImgFile(TextureCache.InCombatTextureName);
            outOfCombatQueueingIndicator = new Image
            {
                Texture = imgSword,
                Parent = this,
                Visible = false,
                Size = new Point(Width, Height),
                Location = new Point(0, 0),
            };

            rangedThing = new Image
            {
                Parent = this,
                Visible = false,
                Size = new Point(Width, Height),
                Location = new Point(0, 0),
                BasicTooltipText = "Ranged action, left click to use action"
            };

            Texture2D imgRanged = _textureCache.GetImgFile(TextureCache.RangeIndicatorTextureName);
            rangedIndicator = new Image
            {
                Texture = imgRanged,
                Parent = this,
                Visible = false,
                Size = new Point(Width / 3, Height / 3),
                Location = new Point(Width / 2 - Width / 3 / 2, Height - Height / 3)
            };

            laterActivationThing = new Image
            {
                Parent = this,
                Visible = false,
                Size = new Point(Width, Height),
                Location = new Point(0, 0),
                BasicTooltipText = "Action will be performed later"
            };

            Texture2D imgLater = _textureCache.GetImgFile(TextureCache.LaterActivationTextureName);
            laterActivationIndicator = new Image
            {
                Texture = imgLater,
                Parent = this,
                Visible = false,
                Size = new Point(Width / 3, Height / 3),
                Location = new Point(Width / 2 - Width / 3 / 2, Height - Height / 3)
            };


            if (Module._settingDragInfoPanel.Value)
            {
                Panel dragBox = new Panel()
                {
                    Parent = this,
                    Location = new Point(0, 0),
                    Size = new Point(25, 25),
                    BackgroundColor = Color.White,
                    BasicTooltipText = "Drag info panel",
                    ZIndex = 100,
                };
                dragBox.LeftMouseButtonPressed += delegate
                {
                    _dragging = true;
                    _dragStart = Input.Mouse.Position;
                };
                dragBox.LeftMouseButtonReleased += delegate
                {
                    _dragging = false;
                    Module._settingInfoPanelLocation.Value = Location;
                };
            }
        }

        protected override CaptureType CapturesInput()
        {
            return Module._settingDragInfoPanel.Value ? CaptureType.Mouse : CaptureType.None;
        }


        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_dragging)
            {
                var nOffset = Input.Mouse.Position - _dragStart;
                Location += nOffset;
                _dragStart = Input.Mouse.Position;
            }


            base.PaintBeforeChildren(spriteBatch, bounds);
        }
    }
}
