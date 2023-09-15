using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Manlaan.Mounts.Controls
{
    public class DrawManualIcons : Container
    {

        private static readonly Logger Logger = Logger.GetLogger<DrawManualIcons>();
        private readonly Helper _helper;
        private readonly TextureCache _textureCache;
        private bool _dragging;
        private Point _dragStart = Point.Zero;

        public DrawManualIcons(Helper helper, TextureCache textureCache)
        {
            _helper = helper;
            _textureCache = textureCache;
            Parent = GameService.Graphics.SpriteScreen;
            Location = Module._settingLoc.Value;
            Size = new Point(Module._settingImgWidth.Value * Module._things.Count, Module._settingImgWidth.Value * Module._things.Count);


            int curX = 0;
            int curY = 0;

            foreach (var thing in Module._availableOrderedThings)
            {
                Texture2D img = _textureCache.GetMountImgFile(thing);
                Image _btnMount = new Image
                {
                    Parent = this,
                    Texture = img,
                    Size = new Point(Module._settingImgWidth.Value, Module._settingImgWidth.Value),
                    Location = new Point(curX, curY),
                    Opacity = Module._settingOpacity.Value,
                    BasicTooltipText = thing.DisplayName
                };
                _btnMount.LeftMouseButtonPressed += async delegate { await thing.DoAction(); };

                if (Module._settingOrientation.Value.Equals("Horizontal"))
                    curX += Module._settingImgWidth.Value;
                else
                    curY += Module._settingImgWidth.Value;

            }

            if (Module._settingOrientation.Value.Equals("Horizontal"))
            {
                Size = new Point(Module._settingImgWidth.Value * Module._availableOrderedThings.Count, Module._settingImgWidth.Value);
            }
            else
            {
                Size = new Point(Module._settingImgWidth.Value, Module._settingImgWidth.Value * Module._availableOrderedThings.Count);
            }

            if (Module._settingDrag.Value)
            {
                Panel dragBox = new Panel()
                {
                    Parent = this,
                    Location = new Point(0, 0),
                    Size = new Point(Module._settingImgWidth.Value / 2, Module._settingImgWidth.Value / 2),
                    BackgroundColor = Color.White,
                    ZIndex = 10,
                };
                dragBox.LeftMouseButtonPressed += delegate {
                    _dragging = true;
                    _dragStart = Input.Mouse.Position;
                };
                dragBox.LeftMouseButtonReleased += delegate {
                    _dragging = false;
                    Module._settingLoc.Value = Location;
                };
            }
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse;
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
