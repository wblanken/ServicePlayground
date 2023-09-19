namespace ServicePlayground.Common;

[AttributeUsage(AttributeTargets.Class)]
public class MongoItemAttribute : Attribute
{
    public string CollectionName { get; }

    public MongoItemAttribute(string collectionName)
    {
        CollectionName = collectionName;
    }
}