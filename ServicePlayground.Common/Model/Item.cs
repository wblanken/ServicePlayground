namespace ServicePlayground.Common.Model;

[MongoItem("items")]
public class Item : MongoItem
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}