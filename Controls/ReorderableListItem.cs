using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Mounts.Controls
{
    internal class ReorderableListItem : Panel
    {
        private readonly Panel _dragHandle;
        private Point _mouseDownPos;
        private int _dragStartTop;
        private bool _mouseDown;
        private const int DragThreshold = 3;

        public bool IsDragging { get; private set; }
        public int DragY { get; private set; }
        public readonly Panel ContentPanel;

        public ReorderableListItem()
        {
            Height = 32;

            // Drag handle
            _dragHandle = new Panel()
            {
                Parent = this,
                Width = 24,
                Height = 24,
                Left = 4,
                Top = (Height - 24) / 2,
                BackgroundColor = Color.Red
            };

            // Content panel
            ContentPanel = new Panel()
            {
                Parent = this,
                Left = _dragHandle.Right + 4,
                Top = 0,
                Width = Width - _dragHandle.Width - 8,
                Height = Height
            };

            _dragHandle.LeftMouseButtonPressed += (_, _) =>
            {
                _mouseDown = true;
                _mouseDownPos = Input.Mouse.Position;
                _dragStartTop = Top;
            };

            _dragHandle.LeftMouseButtonReleased += (_, _) =>
            {
                _mouseDown = false;
                if (IsDragging)
                {
                    StopDragging();
                }
            };
        }

        protected override CaptureType CapturesInput() => CaptureType.Mouse;

        public void StopDragging()
        {
            IsDragging = false;
            ZIndex = 0;
            _dragHandle.BackgroundColor = Color.Red;
        }

        public void UpdateDragLogic()
        {
            // Update content size
            ContentPanel.Left = _dragHandle.Right + 4;
            ContentPanel.Width = Width - _dragHandle.Width - 8;
            ContentPanel.Height = Height;

            // Hover color
            if (IsDragging)
                _dragHandle.BackgroundColor = Color.Orange;
            else if (_dragHandle.MouseOver)
                _dragHandle.BackgroundColor = Color.Green;
            else
                _dragHandle.BackgroundColor = Color.Red;

            // Start drag if threshold reached
            if (_mouseDown && !IsDragging)
            {
                var delta = Input.Mouse.Position - _mouseDownPos;
                if (Math.Abs(delta.Y) >= DragThreshold)
                {
                    IsDragging = true;
                    ZIndex = 10000;
                }
            }

            // Update dragged position
            if (IsDragging)
            {
                DragY = _dragStartTop + (Input.Mouse.Position.Y - _mouseDownPos.Y);
                Top = DragY;
            }
        }
    }
}
