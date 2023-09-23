﻿using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Mounts.Settings;
using System.Collections.Generic;
using Manlaan.Mounts.Things;
using System;

namespace Manlaan.Mounts.Controls
{
    public class DrawIcons : Container, IDisposable
    {

        private bool _disposed = false;
        private static readonly Logger Logger = Logger.GetLogger<DrawIcons>();
        private readonly IconThingSettings _iconThingSettings;
        private readonly Helper _helper;
        private readonly TextureCache _textureCache;
        private bool _dragging;
        private Point _dragStart = Point.Zero;

        public DrawIcons(IconThingSettings iconThingSettings, Helper helper, TextureCache textureCache)
        {
            _iconThingSettings = iconThingSettings;
            _helper = helper;
            _textureCache = textureCache;


            _iconThingSettings.IconSettingsUpdated += IconSettingsUpdated;
            Parent = GameService.Graphics.SpriteScreen;
            Location = _iconThingSettings.Location.Value;

            Draw();
        }

        private void Draw()
        {
            ClearChildren();

            DrawManualIcons();
            DrawCornerIcons();
        }

        private void DrawCornerIcons()
        {
            if (!_iconThingSettings.IsDefault)
            {
                return;
            }

            //dispose all!
            foreach (var thing in Module._things)
            {
                thing.DisposeCornerIcon();
            }
            foreach (var thing in _iconThingSettings.Things)
            {
                thing.CreateCornerIcon(_textureCache.GetMountImgFile(thing));
            }
        }

        private void DrawManualIcons()
        {
            int curX = 0;
            int curY = 0;
            var things = _iconThingSettings.Things;
            foreach (var thing in things)
            {
                Texture2D img = _textureCache.GetMountImgFile(thing);
                Image _btnMount = new Image
                {
                    Parent = this,
                    Texture = img,
                    Size = new Point(_iconThingSettings.Size.Value, _iconThingSettings.Size.Value),
                    Location = new Point(curX, curY),
                    Opacity = _iconThingSettings.Opacity.Value,
                    BasicTooltipText = thing.DisplayName
                };
                _btnMount.LeftMouseButtonPressed += async delegate { await thing.DoAction(); };

                if (_iconThingSettings.Orientation.Value == IconOrientation.Horizontal)
                    curX += _iconThingSettings.Size.Value;
                else
                    curY += _iconThingSettings.Size.Value;

            }

            if (_iconThingSettings.Orientation.Value == IconOrientation.Horizontal)
            {
                Size = new Point(_iconThingSettings.Size.Value * things.Count, _iconThingSettings.Size.Value);
            }
            else
            {
                Size = new Point(_iconThingSettings.Size.Value, _iconThingSettings.Size.Value * things.Count);
            }

            if (_iconThingSettings.IsDraggingEnabled.Value)
            {
                Panel dragBox = new Panel()
                {
                    Parent = this,
                    Location = new Point(0, 0),
                    Size = new Point(_iconThingSettings.Size.Value / 2, _iconThingSettings.Size.Value / 2),
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
                    _iconThingSettings.Location.Value = Location;
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

        protected override void OnHidden(EventArgs e)
        {

            foreach (var mount in _iconThingSettings.Things)
            {
                mount.CornerIcon?.Hide();
            }
            base.OnHidden(e);
        }

        protected override void OnShown(EventArgs e)
        {

            foreach (var mount in _iconThingSettings.Things)
            {
                mount.CornerIcon?.Show();
            }
            base.OnHidden(e);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // dispose managed state (managed objects).
                foreach (var thing in Module._things)
                {
                    thing.DisposeCornerIcon();
                }

            }

            // free unmanaged resources (unmanaged objects) and override a finalizer below.
            // set large fields to null.

            _disposed = true;
        }

    }
}