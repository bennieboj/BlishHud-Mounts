using Blish_HUD.Controls;

namespace Mounts.Controls
{
    internal class ReorderableListItemWithContent<TContent> : ReorderableListItem
        where TContent : IReorderableListItemContent
    {
        public ReorderableListItemWithContent(TContent content)
        {
            var ctrl = content.CreateContent();
            ctrl.Parent = ContentPanel;
        }
    }
}
