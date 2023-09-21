using System.Diagnostics.CodeAnalysis;
using ServicePlayground.Common;

namespace ServicePlayground.Service;

public interface IDbService<TCollection> 
    where TCollection : MongoItem
{
    public Task<List<TCollection>?> GetAllAsync();
    public Task<TCollection?> GetAsync(string id);
    public Task CreateAsync([NotNull] TCollection newEntry);
    public Task UpdateAsync(string id, [NotNull] TCollection updatedEntry);
    public Task RemoveAsync(string id);
}