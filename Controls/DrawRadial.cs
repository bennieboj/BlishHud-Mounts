using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Manlaan.Mounts.Controls
{
    internal class RadialMount
    {
        public double AngleBegin { get; set; }
        public double AngleEnd { get; set; }
        public Mount Mount { get; set; }
        public Texture Texture { get; set; }
        public int ImageX { get; set; }
        public int ImageY { get; set; }
        public bool Selected { get; set; }
        public bool Default { get; internal set; }
    }

    internal class DrawRadial : Container
    {
        private readonly Helper _helper;
        private List<RadialMount> RadialMounts = new List<RadialMount>();
        private RadialMount SelectedMount => RadialMounts.SingleOrDefault(m => m.Selected);

        int radius = 0;
        int mountIconSize = 0;
        int _maxRadialSize = 0;

        private Point SpawnPoint = default;

        public DrawRadial(Helper helper)
        {
            Visible = false;
            Padding = Thickness.Zero;
            _helper = helper;
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            RadialMounts.Clear();
            var mounts = Module._availableOrderedMounts;
            var defaultMount = _helper.GetDefaultMount();
            if (defaultMount != null)
            {
                mounts.Remove(defaultMount);
                var texture = _helper.GetImgFile(defaultMount.ImageFileName);
                int loc = _maxRadialSize / 2 - radius;
                RadialMounts.Add(new RadialMount { Texture = texture, Mount = defaultMount, ImageX = loc, ImageY = loc, Default = true });
            }
            double currentAngle = 0;
            var partAngleStep = Math.PI * 2 / mounts.Count();
            foreach (var mount in mounts)
            {
                var angleMid = currentAngle + partAngleStep / 2;
                var angleEnd = currentAngle + partAngleStep;
                var texture = _helper.GetImgFile(mount.ImageFileName);

                int x = (int)Math.Round(radius + radius * Math.Cos(angleMid));
                int y = (int)Math.Round(radius + radius * Math.Sin(angleMid));
                RadialMounts.Add(new RadialMount
                {
                    Texture = texture,
                    Mount = mount,
                    ImageX = x,
                    ImageY = y,
                    AngleBegin = currentAngle,
                    AngleEnd = angleEnd
                });
                currentAngle = angleEnd;
            }


            var mousePos = Input.Mouse.Position;
            var diff = mousePos - SpawnPoint;
            var angle = Math.Atan2(diff.Y, diff.X);
            if (angle < 0)
            {
                angle += Math.PI * 2;
            }

            var length = new Vector2(diff.Y, diff.X).Length();
            
            Children.Clear();
            foreach (var radialMount in RadialMounts)
            {
                Image _btnMount = new Image
                {
                    Parent = this,
                    Texture = (Blish_HUD.Content.AsyncTexture2D)radialMount.Texture,
                    Size = new Point(mountIconSize, mountIconSize),
                    Location = new Point(radialMount.ImageX, radialMount.ImageY),
                    BasicTooltipText = radialMount.Mount.DisplayName
                };

                if(length < radius / 2)
                {
                    radialMount.Selected = radialMount.Default;
                }
                else
                {
                    radialMount.Selected = radialMount.AngleBegin <= angle && radialMount.AngleEnd > angle;
                }

                _btnMount.Opacity = radialMount.Selected ? 1f : 0.5f;
                AddChild(_btnMount);
            }
        }

        public void TriggerSelectedMount()
        {
            SelectedMount?.Mount.DoHotKey();
        }

        internal void Start()
        {
            if (!Visible)
            {
                _maxRadialSize = Math.Min(GameService.Graphics.SpriteScreen.Width, GameService.Graphics.SpriteScreen.Height);
                radius = _maxRadialSize / 4;
                mountIconSize = _maxRadialSize / 6;
                Size = new Point(_maxRadialSize, _maxRadialSize);

                if (Module._settingMountRadialSpawnAtMouse.Value)
                {
                    SpawnPoint = Input.Mouse.Position;
                }
                else
                {
                    Mouse.SetPosition(GameService.Graphics.WindowWidth / 2, GameService.Graphics.WindowHeight / 2);
                    SpawnPoint = new Point(GameService.Graphics.SpriteScreen.Width / 2, GameService.Graphics.SpriteScreen.Height / 2);
                }

                Location = new Point(SpawnPoint.X - radius - mountIconSize / 2, SpawnPoint.Y - radius - mountIconSize / 2);
            }
            Visible = true;
        }

        internal void Stop()
        {
            TriggerSelectedMount();
            Visible = false;
        }
    }
}