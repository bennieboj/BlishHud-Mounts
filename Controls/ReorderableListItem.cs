using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using System;

namespace Mounts.Controls
{
    public class ReorderableListItem : Panel
    {
        private bool _mouseDown;
        private Point _mouseDownPos;
        private int _dragStartTop;
        private const int DragThreshold = 3;

        public bool IsDragging { get; private set; }
        public int DragY { get; private set; }

        public event Action<ReorderableListItem> DragStarted;
        public event Action<ReorderableListItem> DragEnded;

        public ReorderableListItem(IReorderableListItemContent content)
        {
            this.Height = 40;

            // Host content directly
            var ctrl = content.CreateContent();
            ctrl.Parent = this;
            ctrl.Left = 4;
            ctrl.Top = 4;
            ctrl.Width = this.Width - 8;
            ctrl.Height = this.Height - 8;

            this.LeftMouseButtonPressed += (_, _) =>
            {
                _mouseDown = true;
                _mouseDownPos = Input.Mouse.Position;
                _dragStartTop = this.Top;
            };

            this.LeftMouseButtonReleased += (_, _) =>
            {
                _mouseDown = false;
                if (IsDragging)
                {
                    StopDragging();
                    DragEnded?.Invoke(this);
                }
            };
        }

        protected override CaptureType CapturesInput() => CaptureType.Mouse;

        public void StopDragging()
        {
            IsDragging = false;
            ZIndex = 0;
        }

        public void UpdateDragLogic()
        {
            if (_mouseDown && !IsDragging)
            {
                var delta = Input.Mouse.Position - _mouseDownPos;
                if (Math.Abs(delta.Y) >= DragThreshold)
                {
                    IsDragging = true;
                    ZIndex = 10000;
                    DragStarted?.Invoke(this);
                }
            }

            if (IsDragging)
            {
                DragY = _dragStartTop + (Input.Mouse.Position.Y - _mouseDownPos.Y);
                Top = DragY;
            }
        }
    }
}