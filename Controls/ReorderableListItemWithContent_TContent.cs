namespace Mounts.Controls
{
    public class ReorderableListItemWithContent<TContent> : ReorderableListItem
        where TContent : IReorderableListItemContent
    {
        public TContent Content { get; }

        public ReorderableListItemWithContent(TContent content)
            : base(content)
        {
            Content = content;
        }
    }
}