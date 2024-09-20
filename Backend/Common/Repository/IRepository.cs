using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Common.Repository;

public interface IRepository<T>
{
    Task AddAsync(T item);
    Task SetAsync(IEnumerable<T> items);
    Task UpdateAsync(T item);
    Task AddRangeAsync(IEnumerable<T> items);
    Task<T?> FindAsync(object key);
    Task<IEnumerable<T>> GetAllAsync();
    Task<long> GetCountAsync();
    Task DeleteAsync(object key);
    Task Clear();
}