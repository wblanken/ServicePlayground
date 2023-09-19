using System.Reflection;

namespace ServicePlayground.Common;

public abstract class MongoItem
{
    public string Id { get; set; }

    public static string GetCollectionName<TCollection>()
        where TCollection : MongoItem
    {
        if (typeof(TCollection).GetCustomAttribute(typeof(MongoItemAttribute)) is MongoItemAttribute mongoItemAttribute)
        {
            return mongoItemAttribute.CollectionName;
        }

        throw new Exception($"{typeof(MongoItem)} does not have MongoItemAttribute!");
    }
}