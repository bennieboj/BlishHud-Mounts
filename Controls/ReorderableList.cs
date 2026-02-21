using Blish_HUD;
using Blish_HUD.Controls;
using Manlaan.Mounts;
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
        private readonly List<ReorderableListItemWithContent<TContent>> _items = new();
        private int _hoverInsertIndex = -1;
        private ReorderableListItemWithContent<TContent> _draggingToCommit;
        private int _commitIndex;
        private List<(int Top, int Height)> _itemPositionsSnapshot = null;
        private bool _mouseDown;

        public ReorderableList()
        {
            LeftMouseButtonPressed += (_, _) => _mouseDown = true;
            LeftMouseButtonReleased += (_, _) => _mouseDown = false;
        }

        public void AddItem(TContent contentInstance)
        {
            var item = new ReorderableListItemWithContent<TContent>(contentInstance);
            item.Parent = this;
            _items.Add(item);
            LayoutItems();
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            //var dragging = _items.FirstOrDefault(i => i.IsDragging);
            //Module._debug.Add("someting is dragging", () => (dragging != null).ToString());


            //foreach (var it in _items)
            //    it.UpdateDragLogic();

            //UpdateInsertIndex(dragging);
            //LayoutItemsDuringDrag(dragging);
            DrawInsertRuler(spriteBatch, bounds);

            //// Commit drag on mouse release
            //if (dragging != null && Input.Mouse.State.LeftButton == ButtonState.Released)
            //{
            //    CommitReorder(dragging, _hoverInsertIndex);
            //    dragging.StopDragging();
            //}

            //CommitPendingDrag();

            base.PaintAfterChildren(spriteBatch, bounds);
        }

        private void LayoutItemsDuringDrag(ReorderableListItem dragging)
        {
            int y = 0;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item == dragging) continue;

                if (i == _hoverInsertIndex)
                    y += dragging.Height;

                item.Top = y;
                item.Left = 0;
                item.Width = this.Width;

                y += item.Height;
            }

            if (_hoverInsertIndex >= _items.Count)
                y += dragging?.Height ?? 0;

            if (dragging != null)
                dragging.Top = dragging.DragY;
        }


        private void UpdateInsertIndex(ReorderableListItem dragging)
        {
            if (dragging == null || _itemPositionsSnapshot == null)
            {
                _hoverInsertIndex = -1;
                return;
            }

            int centerY = dragging.DragY + dragging.Height / 2;
            int insertIndex = _items.Count;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item == dragging) continue;

                var (top, height) = _itemPositionsSnapshot[i];
                int middle = top + height / 2;
                if (centerY < middle)
                {
                    insertIndex = i;
                    break;
                }
            }

            _hoverInsertIndex = insertIndex;
            Module._debug.Add("_hoverInsertIndex", () => _hoverInsertIndex.ToString());
        }


        private void CommitPendingDrag()
        {
            if (_draggingToCommit == null) return;

            CommitReorder(_draggingToCommit, _commitIndex);
            _draggingToCommit = null;
            _commitIndex = -1;
        }

        private void CommitReorder(ReorderableListItemWithContent<TContent> dragging, int newIndex)
        {
            _items.Remove(dragging);
            newIndex = Math.Max(0, Math.Min(newIndex, _items.Count));
            _items.Insert(newIndex, dragging);
            LayoutItems();
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
            int y = 0;
            var dragging = _items.FirstOrDefault(i => i.IsDragging);

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item == dragging) continue;

                if (i == _hoverInsertIndex && dragging != null)
                    y += dragging.Height;

                item.Top = y;
                item.Left = 0;
                item.Width = Width;

                y += item.Height;
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            var dragging = _items.FirstOrDefault(i => i.IsDragging);
            Module._debug.Add("someting is dragging", () => (dragging != null).ToString());
            Module._debug.Add("left mouse state xna", () => Mouse.GetState().LeftButton.ToString());
            Module._debug.Add("left mouse state", () => Input.Mouse.State.LeftButton.ToString());
            Module._debug.Add("left mouse _mouseDown", () => _mouseDown.ToString());

            foreach (var it in _items)
                it.UpdateDragLogic();

            // Capture snapshot when drag starts
            if (dragging != null && _itemPositionsSnapshot == null)
            {
                _itemPositionsSnapshot = _items
                    .Select(i => (i.Top, i.Height))
                    .ToList();
                _draggingToCommit = dragging;
            }

            // Commit on mouse release, but not inside Paint!
            //if (dragging != null && !_mouseDown)
            //{
            //    CommitReorder(dragging, _hoverInsertIndex);
            //    dragging.StopDragging();
            //    _itemPositionsSnapshot = null;
            //}

            //CommitPendingDrag();

            if (_itemPositionsSnapshot != null && (dragging == null || !dragging.IsDragging))
            {
                //var committedItem = _items.FirstOrDefault(i => !i.IsDragging && _itemPositionsSnapshot != null);

                //if (committedItem != null)
                //{
                    
                //}
                CommitReorder(_draggingToCommit, _hoverInsertIndex);

                _itemPositionsSnapshot = null;
                _hoverInsertIndex = -1;
            }
            else {
                UpdateInsertIndex(dragging);
                LayoutItemsDuringDrag(dragging);
            }

            Module._debug.Add("_itemPositionsSnapshot top", () =>
            {
                if (_itemPositionsSnapshot == null)
                    return "";
                else
                    return string.Join(",", _itemPositionsSnapshot.Select(i => i.Top));
            });
            Module._debug.Add("items top", () => string.Join(",", _items.Select(i => i.Top)));
            Module._debug.Add("items ZIndex", () => string.Join(",", _items.Select(i => i.ZIndex)));
            Module._debug.Add("items MouseOver", () => string.Join(",", _items.Select(i => i.MouseOver)));
            Module._debug.Add("items IsDragging", () => string.Join(",", _items.Select(i => i.IsDragging)));
        }
    }
}
