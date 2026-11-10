using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Mounts.Controls
{
    public class ReorderableList<TContent> : Panel where TContent : IReorderableListItemContent
    {
        private readonly FlowPanel _flow;
        private readonly List<ReorderableListItemWithContent<TContent>> _items = new();

        private ReorderableListItem _activeDrag;
        private Panel _placeholder;
        private int _targetIndex;

        public ReorderableList()
        {
            _flow = new FlowPanel
            {
                Parent = this,
                Location = new Point(0, 0),
                Size = this.Size,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                ControlPadding = new Microsoft.Xna.Framework.Vector2(0, 2)
            };

            this.Resized += (_, _) =>
            {
                _flow.Size = this.Size;
            };
        }

        public void AddItem(TContent content)
        {
            var item = new ReorderableListItemWithContent<TContent>(content);
            _items.Add(item);
            item.Parent = _flow;

            item.DragStarted += OnDragStarted;
            item.DragEnded += OnDragEnded;
        }

        private void OnDragStarted(ReorderableListItem item)
        {
            _activeDrag = item;

            _placeholder = new Panel
            {
                Width = item.Width,
                Height = item.Height
            };

            _targetIndex = _flow.Children.IndexOf(item);

            _flow.RemoveChild(item);
            _targetIndex = MathExtensions.Clamp(_targetIndex, 0, _flow.Children.Count);
            _flow.Children.Insert(_targetIndex, _placeholder);

            item.Parent = this;
            item.ZIndex = 9999;
        }

        private void OnDragEnded(ReorderableListItem item)
        {
            _flow.RemoveChild(_placeholder);

            _targetIndex = MathExtensions.Clamp(_targetIndex, 0, _flow.Children.Count);

            _flow.Children.Insert(_targetIndex, item);
            item.Parent = _flow;
            item.ZIndex = 0;

            _activeDrag = null;
            _placeholder = null;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (_activeDrag != null)
            {
                var mouseY = Input.Mouse.Position.Y - _flow.AbsoluteBounds.Y;

                int index = 0;
                foreach (var ctrl in _flow.Children.Where(c => c != _activeDrag && c != _placeholder))
                {
                    if (mouseY < ctrl.Top + ctrl.Height / 2)
                        break;

                    index++;
                }

                _targetIndex = index;

                _flow.Children.Remove(_placeholder);
                _flow.Children.Insert(_targetIndex, _placeholder);
            }

            foreach (var item in _items)
                item.UpdateDragLogic();
        }
    }
}