using ServicePlayground.Common.Model;

namespace ServicePlayground.Common;

public class MongoCollectionChange<TCollection> where TCollection : MongoItem
{
    public string Id { get; set; }
    public OperationType OperationType { get; set; }
    public TCollection? ChangedItem { get; set; }
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