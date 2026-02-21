namespace Mounts.Controls
{
    internal class ReorderableListItemWithContent<TContent> : ReorderableListItem
        where TContent : IReorderableListItemContent
    {
        public ReorderableListItemWithContent(TContent content)
            : base(content) // pass content to base constructor
        {
            // Nothing else needed here, base already sets up ContentPanel
        }
    }
}
