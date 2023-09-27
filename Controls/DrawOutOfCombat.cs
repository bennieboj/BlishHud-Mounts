using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Mounts.Settings;
using System;
using System.Linq;

namespace Manlaan.Mounts.Controls
{
    public class DrawOutOfCombat : Container
    {
        private bool _dragging;
        private Point _dragStart = Point.Zero;
        private LoadingSpinner _spinner;

        public DrawOutOfCombat()
        {
            Draw();
        }

        public void HideSpinner()
        {
            if(_spinner!= null)
                _spinner.Visible = false;
        }

        public void ShowSpinner()
        {
            if (_spinner != null)
                _spinner.Visible = true;
        }

        private void Draw()
        {

            if (!Module._settingDisplayMountQueueing.Value && !Module._settingDragMountQueueing.Value)
            {
                return;
            }

            Parent = GameService.Graphics.SpriteScreen;
            Location = Module._settingDisplayMountQueueingLocation.Value;
            Width = 100;
            Height = 100;
            _spinner = new LoadingSpinner
            {
                Parent = this,
                Visible = Module._settingDragMountQueueing.Value
            };

            if (Module._settingDragMountQueueing.Value)
            {
                Panel dragBox = new Panel()
                {
                    Parent = this,
                    Location = new Point(0, 0),
                    Size = new Point(_spinner.Width / 2, _spinner.Width / 2),
                    BackgroundColor = Color.White,
                    ZIndex = 10,
                };
                dragBox.LeftMouseButtonPressed += delegate
                {
                    _dragging = true;
                    _dragStart = Input.Mouse.Position;
                };
                dragBox.LeftMouseButtonReleased += delegate
                {
                    _dragging = false;
                    Module._settingDisplayMountQueueingLocation.Value = Location;
                };
            }
        }

        private void IconSettingsUpdated(object sender, SettingsUpdatedEvent e)
        {
            Draw();
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
