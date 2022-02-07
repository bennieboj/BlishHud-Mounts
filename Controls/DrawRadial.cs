using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Manlaan.Mounts.Controls
{
    internal class RadialMount
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Texture Texture { get; set; }
        public Mount Mount { get; set; }
    }

    internal class DrawRadial : Container
    {
        List<RadialMount> RadialMounts;
        private readonly Helper _helper;

        int radius = 0;
        int mountIconSize = 0;
        int _maxRadialSize = 0;
        internal RadialMount SelectedMount;

        public Point MiddleOfScreen { get; private set; }

        public DrawRadial(Helper helper)
        {
            Visible = false;
            Padding = Thickness.Zero;
            _helper = helper;
            RadialMounts = new List<RadialMount>();
            MiddleOfScreen = new Point(GameService.Graphics.WindowWidth / 2, GameService.Graphics.WindowHeight / 2);
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse;
        }

        internal void Update()
        {
            RadialMounts.Clear();

            var mounts = Module._availableOrderedMounts;
            var defaultMount = _helper.GetDefaultMount();
            if(defaultMount != null)
            {
                mounts.Remove(defaultMount);
                var texture = _helper.GetImgFile(defaultMount.ImageFileName);
                int loc = _maxRadialSize / 2 - radius;
                RadialMounts.Add(new RadialMount { Texture = texture, Mount = defaultMount, X = loc, Y = loc });
            }
            double currentAngle = 0;
            var partAngleStep = Math.PI * 2 / mounts.Count();
            foreach (var mount in mounts)
            {
                var midAngle = currentAngle + partAngleStep / 2;
                var texture = _helper.GetImgFile(mount.ImageFileName);

                int x = (int)Math.Round(radius + radius * Math.Cos(midAngle));
                int y = (int)Math.Round(radius + radius * Math.Sin(midAngle));
                RadialMounts.Add(new RadialMount { Texture = texture, Mount = mount, X = x, Y = y });
                currentAngle += partAngleStep;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if(_maxRadialSize == 0)
            {
                _maxRadialSize = Math.Min(GameService.Graphics.SpriteScreen.Width, GameService.Graphics.SpriteScreen.Height);
                radius = _maxRadialSize / 4;
                mountIconSize = _maxRadialSize / 6;
                Update();
                Location = new Point(GameService.Graphics.SpriteScreen.Width / 2 - radius - mountIconSize / 2, GameService.Graphics.SpriteScreen.Height / 2 - radius - mountIconSize / 2);
                Size = new Point(_maxRadialSize, _maxRadialSize);
            }

            foreach (var radialMount in RadialMounts)
            {
                Image _btnMount = new Image
                {
                    Parent = this,
                    Texture = (Blish_HUD.Content.AsyncTexture2D)radialMount.Texture,
                    Size = new Point(mountIconSize, mountIconSize),
                    Location = new Point(radialMount.X, radialMount.Y),
                    BasicTooltipText = radialMount.Mount.DisplayName
                };
                _btnMount.MouseEntered += delegate { _btnMount.BackgroundColor = Color.Red; SelectedMount = radialMount; SelectedMount = radialMount; };
                _btnMount.MouseLeft += delegate { _btnMount.BackgroundColor = Color.Transparent; };
                AddChild(_btnMount);

                //spriteBatch.DrawOnCtrl(this,
                //(Texture2D) radialMount.Texture,
                //new Rectangle(radialMount.X, radialMount.Y, mountIconSize, mountIconSize),
                //null,
                //Color.White
                //);
            }

        }

        public void TriggerSelectedMount()
        {
            SelectedMount?.Mount.DoHotKey();
            SelectedMount = null;
        }
    }
}