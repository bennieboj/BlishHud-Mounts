using Blish_HUD.Controls;

namespace Mounts.Controls
{
    public interface IReorderableListItemContent
    {
        /// <summary>
        /// Returns the Control that will be displayed inside a ReorderableListItem.
        /// The `parent` is the ReorderableListItem's content panel.
        /// </summary>
        Control CreateContent();
    }
}