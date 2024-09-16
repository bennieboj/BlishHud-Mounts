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

            Draw();
        }

        private void SetThingOnImage(Thing thing, Image image)
        {
            var img = _textureCache.GetThingImgFile(thing);
            image.Texture = img;
            image.Visible = true;
        }

        private void RangedThingUpdated(object sender, RangedThingUpdatedEvent e)
        {
            var currentRangedThing = e.NewThing;
            if (currentRangedThing != null && Module._settingDisplayTargettableAction.Value)
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

        public void Update()
        {
            if (infoPanelInfo != null)
            {
                infoPanelInfo.Visible = Module._settingDragInfoPanel.Value;
                infoPanelInfo.ZIndex = 1;
            }
            if (outOfCombatQueuingThing != null && outOfCombatQueueingIndicator != null && Module._settingDisplayMountQueueing.Value)
            {
                var thing = Module._things.FirstOrDefault(m => m.QueuedTimestamp != null);
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
            if (laterActivationThing != null && laterActivationIndicator != null && Module._settingDisplayLaterActivation.Value)
            {
                var thing = _helper.IsSomethingStoredForLaterActivation();
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
                Visible = false,
                Location = new Point(0, 0),
                BasicTooltipText = "Mounts & More info panel\nDisplays out of combat queueing, targetted action and tap action.\nSee settings and documentation for more info."
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
