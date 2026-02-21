using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Mounts.Controls
{
    internal class ReorderableListItem : Panel
    {
        private readonly Panel _dragHandle;

        private bool _mouseDown;
        private Point _mouseDownPos;
        private int _dragStartTop;
        private const int DragThreshold = 3;

        public bool IsDragging { get; private set; }
        public int DragY { get; private set; }

        public readonly Panel ContentPanel;

        public ReorderableListItem(IReorderableListItemContent content)
        {
            this.Height = 32;

            // Drag handle
            _dragHandle = new Panel
            {
                Parent = this,
                Width = 24,
                Height = 24,
                Left = 4,
                Top = (this.Height - 24) / 2,
                BackgroundColor = Color.Red,
                BasicTooltipText = "Click and drag to reorder"
            };

            // Content panel
            ContentPanel = new Panel
            {
                Parent = this,
                Left = _dragHandle.Right + 4,
                Top = 0,
                Width = this.Width - _dragHandle.Width - 8,
                Height = this.Height,
                BackgroundColor = Color.Transparent
            };

            // Place content inside the content panel
            var ctrl = content.CreateContent();
            ctrl.Parent = ContentPanel;

            // Mouse events
            _dragHandle.LeftMouseButtonPressed += (_, _) =>
            {
                _mouseDown = true;
                _mouseDownPos = Input.Mouse.Position;
                _dragStartTop = this.Top;
            };

            _dragHandle.LeftMouseButtonReleased += (_, _) =>
            {
                _mouseDown = false;
                if (IsDragging)
                    StopDragging();
            };
        }

        protected override CaptureType CapturesInput() => CaptureType.Mouse;

        public void StopDragging()
        {
            IsDragging = false;
            this.ZIndex = 0;
            _dragHandle.BackgroundColor = Color.Red;
        }

        public void UpdateDragLogic()
        {
            // Resize content panel to match
            ContentPanel.Left = _dragHandle.Right + 4;
            ContentPanel.Width = this.Width - _dragHandle.Width - 8;
            ContentPanel.Height = this.Height;

            // Hover colors
            if (IsDragging)
                _dragHandle.BackgroundColor = Color.Orange;
            else if (_dragHandle.MouseOver)
                _dragHandle.BackgroundColor = Color.Green;
            else
                _dragHandle.BackgroundColor = Color.Red;

            // Start dragging if mouse moves enough
            if (_mouseDown && !IsDragging)
            {
                var delta = Input.Mouse.Position - _mouseDownPos;
                if (Math.Abs(delta.Y) >= DragThreshold)
                {
                    IsDragging = true;
                    ZIndex = 10000; // float above
                }
            }

            // Update dragged position
            if (IsDragging)
            {
                DragY = _dragStartTop + (Input.Mouse.Position.Y - _mouseDownPos.Y);

                // IMPORTANT: Do not override Top repeatedly — only update when changed
                if (Top != DragY)
                    Top = DragY;
            }
        }

    }
}
