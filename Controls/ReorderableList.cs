using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mounts.Controls
{
    internal class ReorderableList<TContent> : Panel
        where TContent : IReorderableListItemContent
    {
        private readonly List<ReorderableListItem> _items = new();
        private int _hoverInsertIndex = -1;
        private ReorderableListItem _draggingToCommit = null;
        private int _commitIndex = -1;

        public void AddItem(TContent contentInstance)
        {
            var item = new ReorderableListItemWithContent<TContent>(contentInstance);
            item.Parent = this;
            _items.Add(item);
            LayoutItems();
        }

        public void AddItem(Func<TContent> factory)
        {
            AddItem(factory());
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var dragging = _items.FirstOrDefault(i => i.IsDragging);

            foreach (var it in _items)
                it.UpdateDragLogic();

            UpdateInsertIndex(dragging);
            LayoutItemsDuringDrag(dragging);
            DrawInsertRuler(spriteBatch, bounds);

            // Commit drag only once on release
            if (dragging != null && Input.Mouse.State.LeftButton == ButtonState.Released)
            {
                CommitReorder(dragging, _hoverInsertIndex);
            }

            base.PaintAfterChildren(spriteBatch, bounds);
        }

        private void UpdateInsertIndex(ReorderableListItem dragging)
        {
            if (dragging == null)
            {
                _hoverInsertIndex = -1;
                return;
            }

            int center = dragging.DragY + dragging.Height / 2;
            int insertIndex = _items.Count;

            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] == dragging)
                    continue;

                if (center < _items[i].Bottom)
                {
                    insertIndex = i;
                    break;
                }
            }

            _hoverInsertIndex = insertIndex;
        }

        private void LayoutItemsDuringDrag(ReorderableListItem dragging)
        {
            int y = 0;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];

                if (item == dragging)
                    continue; // dragged item floats separately

                // Insert gap if the dragged item would go here
                if (_hoverInsertIndex == i)
                {
                    y += dragging.Height; // leave space
                }

                item.Top = y;
                item.Left = 0;
                item.Width = this.Width;

                y += item.Height;
            }

            // Handle case when dragging below all items
            if (_hoverInsertIndex >= _items.Count && dragging != null)
            {
                // gap is already after all items
            }

            // dragged item floats at DragY
            if (dragging != null)
                dragging.Top = dragging.DragY;
        }


        private void CommitPendingDrag()
        {
            if (_draggingToCommit == null) return;

            CommitReorder(_draggingToCommit, _commitIndex);
            _draggingToCommit = null;
            _commitIndex = -1;
        }

        private void CommitReorder(ReorderableListItem dragging, int newIndex)
        {
            _items.Remove(dragging);
            newIndex = Math.Max(0, Math.Min(newIndex, _items.Count));
            _items.Insert(newIndex, dragging);
            LayoutItems();
            _hoverInsertIndex = -1;
        }

        private void DrawInsertRuler(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_hoverInsertIndex < 0) return;

            int y = 0;
            for (int i = 0; i < _hoverInsertIndex; i++)
            {
                if (_items[i].IsDragging) continue;
                y += _items[i].Height;
            }

            spriteBatch.DrawOnCtrl(
                this,
                ContentService.Textures.Pixel,
                new Rectangle(0, y - 2, Width, 4),
                Color.Yellow * 0.9f
            );
        }

        private void LayoutItems()
        {
            var dragging = _items.FirstOrDefault(i => i.IsDragging);
            int y = 0;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item == dragging) continue;

                if (_hoverInsertIndex == i && dragging != null)
                    y += dragging.Height;

                item.Top = y;
                item.Left = 0;
                item.Width = Width;

                y += item.Height;
            }
        }
    }
}
