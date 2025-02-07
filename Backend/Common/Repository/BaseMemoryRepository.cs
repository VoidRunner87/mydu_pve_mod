using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Common.Repository;

public abstract class BaseMemoryRepository<TKey, T>(IServiceProvider provider) : IRepository<T> where T : IHasKey<TKey>
{
    private readonly ILogger<BaseMemoryRepository<TKey, T>> _logger = provider.CreateLogger<BaseMemoryRepository<TKey, T>>();
    private readonly Dictionary<TKey, T> _items = new();

    public Task AddAsync(T item)
    {
        _items.TryAdd(item.GetKey(), item);

        return Task.CompletedTask;
    }

    public Task SetAsync(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            AddAsync(item);
        }
        
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T item)
    {
        if (!_items.ContainsKey(item.GetKey()))
        {
            return AddAsync(item);
        }

        _items[item.GetKey()] = item;
        
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            if (!_items.TryAdd(item.GetKey(), item))
            {
                _logger.LogWarning(
                    "Failed to Add item {Key} to {Repo}. Item is already added. You might have duplicated key somewhere", 
                    item.GetKey(), 
                    GetType().Name
                );
            }
        }
        
        return Task.CompletedTask;
    }

    public Task<T?> FindAsync(object key)
    {
        if (_items.TryGetValue((TKey)key!, out var item))
        {
            return Task.FromResult(item);
        }

        return Task.FromResult(default(T?));
    }

    public Task<bool> FindAsync(object key, out T item)
    {
        return Task.FromResult(_items.TryGetValue((TKey)key!, out item));
    }

    public Task<IEnumerable<T>> GetAllAsync() => Task.FromResult<IEnumerable<T>>(_items.Values);
    public Task<long> GetCountAsync()
    {
        return Task.FromResult<long>(_items.Count);
    }

    public Task DeleteAsync(object key)
    {
        _items.Remove((TKey)key);

        return Task.CompletedTask;
    }

    public Task Clear()
    {
        _items.Clear();

        return Task.CompletedTask;
    }
}