using ServicePlayground.Common.Model;

namespace ServicePlayground.Common;

public class ItemsChangedEventArgs : EventArgs
{
    public Item Item { get; set; }
    public OperationType OperationType { get; set; }
}

public enum OperationType
{
    Unkown,
    Insert,
    Update,
    Replace,
    Delete,
    Invalidate
}